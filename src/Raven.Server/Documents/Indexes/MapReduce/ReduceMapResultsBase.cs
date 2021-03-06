﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Raven.Client.Documents.Indexes;
using Raven.Server.Documents.Indexes.Persistence.Lucene;
using Raven.Server.Documents.Indexes.Workers;
using Raven.Server.ServerWide.Context;
using Raven.Server.Utils;
using Sparrow;
using Sparrow.Binary;
using Sparrow.Json;
using Sparrow.Logging;
using Voron;
using Voron.Data.BTrees;
using Voron.Data.Tables;
using Voron.Impl;
using Voron.Data.Compression;
using Voron.Global;

namespace Raven.Server.Documents.Indexes.MapReduce
{
    public abstract unsafe class ReduceMapResultsBase<T> : IIndexingWork where T : IndexDefinitionBase
    {
        internal static readonly Slice PageNumberSlice;
        internal static readonly string PageNumberToReduceResultTableName = "PageNumberToReduceResult";
        private readonly Logger _logger;
        private readonly AggegationBatch _aggregationBatch = new AggegationBatch();
        private readonly Index _index;
        protected readonly T _indexDefinition;
        private readonly IndexStorage _indexStorage;
        private readonly MetricCounters _metrics;
        private readonly MapReduceIndexingContext _mapReduceContext;

        internal static readonly TableSchema ReduceResultsSchema;

        private IndexingStatsScope _treeReductionStatsInstance;
        private IndexingStatsScope _nestedValuesReductionStatsInstance;
        private readonly TreeReductionStats _treeReductionStats = new TreeReductionStats();
        private readonly NestedValuesReductionStats _nestedValuesReductionStats = new NestedValuesReductionStats();

        protected ReduceMapResultsBase(Index index, T indexDefinition, IndexStorage indexStorage, MetricCounters metrics, MapReduceIndexingContext mapReduceContext)
        {
            _index = index;
            _indexDefinition = indexDefinition;
            _indexStorage = indexStorage;
            _metrics = metrics;
            _mapReduceContext = mapReduceContext;
            _logger = LoggingSource.Instance.GetLogger<ReduceMapResultsBase<T>>(indexStorage.DocumentDatabase.Name);
        }

        static ReduceMapResultsBase()
        {
            Slice.From(StorageEnvironment.LabelsContext, "PageNumber", ByteStringType.Immutable, out PageNumberSlice);

            ReduceResultsSchema = new TableSchema()
                .DefineKey(new TableSchema.SchemaIndexDef
                {
                    StartIndex = 0,
                    Count = 1,
                    Name = PageNumberSlice
                });
        }

        public string Name { get; } = "Reduce";

        public bool Execute(DocumentsOperationContext databaseContext, TransactionOperationContext indexContext, Lazy<IndexWriteOperation> writeOperation,
                            IndexingStatsScope stats, CancellationToken token)
        {
            if (_mapReduceContext.StoreByReduceKeyHash.Count == 0)
            {
                WriteLastEtags(indexContext); // we need to write etags here, because if we filtered everything during map then we will loose last indexed etag information and this will cause an endless indexing loop
                return false;
            }

            ReduceResultsSchema.Create(indexContext.Transaction.InnerTransaction, PageNumberToReduceResultTableName, 32);
            var table = indexContext.Transaction.InnerTransaction.OpenTable(ReduceResultsSchema, PageNumberToReduceResultTableName);

            var lowLevelTransaction = indexContext.Transaction.InnerTransaction.LowLevelTransaction;

            var writer = writeOperation.Value;

            var treeScopeStats = stats.For(IndexingOperation.Reduce.TreeScope, start: false);
            var nestedValuesScopeStats = stats.For(IndexingOperation.Reduce.NestedValuesScope, start: false);

            foreach (var store in _mapReduceContext.StoreByReduceKeyHash)
            {
                using (var reduceKeyHash = indexContext.GetLazyString(store.Key.ToString(CultureInfo.InvariantCulture)))
                using (store.Value)
                using (_aggregationBatch)
                {
                    var modifiedStore = store.Value;

                    switch (modifiedStore.Type)
                    {
                        case MapResultsStorageType.Tree:
                            using (treeScopeStats.Start())
                            {
                                HandleTreeReduction(indexContext, treeScopeStats, modifiedStore, lowLevelTransaction, writer, reduceKeyHash, table, token);
                            }
                            break;
                        case MapResultsStorageType.Nested:
                            using (nestedValuesScopeStats.Start())
                            {
                                HandleNestedValuesReduction(indexContext, nestedValuesScopeStats, modifiedStore, writer, reduceKeyHash, token);
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(modifiedStore.Type.ToString());
                    }
                }
            }

            WriteLastEtags(indexContext);

            return false;
        }

        public bool CanContinueBatch(DocumentsOperationContext documentsContext, TransactionOperationContext indexingContext, IndexingStatsScope stats, long currentEtag, long maxEtag, int count)
        {
            throw new NotSupportedException();
        }

        private void WriteLastEtags(TransactionOperationContext indexContext)
        {
            foreach (var lastEtag in _mapReduceContext.ProcessedDocEtags)
            {
                _indexStorage.WriteLastIndexedEtag(indexContext.Transaction, lastEtag.Key, lastEtag.Value);
            }

            foreach (var lastEtag in _mapReduceContext.ProcessedTombstoneEtags)
            {
                _indexStorage.WriteLastTombstoneEtag(indexContext.Transaction, lastEtag.Key, lastEtag.Value);
            }
        }

        private void HandleNestedValuesReduction(TransactionOperationContext indexContext, IndexingStatsScope stats,
                    MapReduceResultsStore modifiedStore,
                    IndexWriteOperation writer, LazyStringValue reduceKeyHash, CancellationToken token)
        {
            EnsureValidNestedValuesReductionStats(stats);

            var numberOfEntriesToReduce = 0;

            try
            {
                var section = modifiedStore.GetNestedResultsSection();

                if (section.IsModified == false)
                    return;

                using (_nestedValuesReductionStats.NestedValuesRead.Start())
                {
                    numberOfEntriesToReduce += section.GetResults(indexContext, _aggregationBatch.Items);
                }

                stats.RecordReduceAttempts(numberOfEntriesToReduce);

                AggregationResult result;
                using (_nestedValuesReductionStats.NestedValuesAggregation.Start())
                {
                    result = AggregateOn(_aggregationBatch.Items, indexContext, token);
                }

                if (section.IsNew == false)
                    writer.DeleteReduceResult(reduceKeyHash, stats);

                foreach (var output in result.GetOutputs())
                {
                    writer.IndexDocument(reduceKeyHash, output, stats, indexContext);
                }

                _index.ReducesPerSec.Mark(numberOfEntriesToReduce);
                _metrics.MapReduceIndexes.ReducedPerSec.Mark(numberOfEntriesToReduce);

                stats.RecordReduceSuccesses(numberOfEntriesToReduce);
            }
            catch (Exception e)
            {
                _index.ThrowIfCorruptionException(e);

                LogReductionError(e, reduceKeyHash, stats, updateStats: true, page: null, numberOfNestedValues: numberOfEntriesToReduce);
            }
        }

        private void HandleTreeReduction(TransactionOperationContext indexContext, IndexingStatsScope stats,
             MapReduceResultsStore modifiedStore, LowLevelTransaction lowLevelTransaction,
            IndexWriteOperation writer, LazyStringValue reduceKeyHash, Table table, CancellationToken token)
        {
            EnsureValidTreeReductionStats(stats);

            var tree = modifiedStore.Tree;

            var branchesToAggregate = new HashSet<long>();

            var parentPagesToAggregate = new HashSet<long>();

            var page = new TreePage(null, Constants.Storage.PageSize);

            HashSet<long> compressedEmptyPages = null;

            foreach (var modifiedPage in modifiedStore.ModifiedPages)
            {
                token.ThrowIfCancellationRequested();

                page.Base = lowLevelTransaction.GetPage(modifiedPage).Pointer;

                stats.RecordReduceTreePageModified(page.IsLeaf);

                if (page.IsLeaf == false)
                {
                    Debug.Assert(page.IsBranch);
                    branchesToAggregate.Add(modifiedPage);

                    continue;
                }

                var leafPage = page;

                var compressed = leafPage.IsCompressed;

                if (compressed)
                    stats.RecordCompressedLeafPage();

                using (compressed ? (DecompressedLeafPage)(leafPage = tree.DecompressPage(leafPage, skipCache: true)) : null)
                {
                    if (leafPage.NumberOfEntries == 0)
                    {
                        if (leafPage.PageNumber == tree.State.RootPageNumber)
                        {
                            writer.DeleteReduceResult(reduceKeyHash, stats);

                            var emptyPageNumber = Bits.SwapBytes(leafPage.PageNumber);
                            using (Slice.External(indexContext.Allocator, (byte*)&emptyPageNumber, sizeof(long), out Slice pageNumSlice))
                                table.DeleteByKey(pageNumSlice);

                            continue;
                        }

                        if (compressed)
                        {
                            // it doesn't have any entries after decompression because 
                            // each compressed entry has the delete tombstone

                            if (compressedEmptyPages == null)
                                compressedEmptyPages = new HashSet<long>();

                            compressedEmptyPages.Add(leafPage.PageNumber);
                            continue;
                        }

                        throw new UnexpectedReduceTreePageException(
                            $"Encountered empty page which isn't a root. Page {leafPage} in '{tree.Name}' tree.");
                    }

                    var parentPage = tree.GetParentPageOf(leafPage);

                    stats.RecordReduceAttempts(leafPage.NumberOfEntries);

                    try
                    {
                        using (var result = AggregateLeafPage(leafPage, lowLevelTransaction, indexContext, token))
                        {
                            if (parentPage == -1)
                            {
                                writer.DeleteReduceResult(reduceKeyHash, stats);

                                foreach (var output in result.GetOutputs())
                                {
                                    writer.IndexDocument(reduceKeyHash, output, stats, indexContext);
                                }
                            }
                            else
                            {
                                StoreAggregationResult(leafPage.PageNumber, leafPage.NumberOfEntries, table, result);
                                parentPagesToAggregate.Add(parentPage);
                            }

                            _metrics.MapReduceIndexes.ReducedPerSec.Mark(leafPage.NumberOfEntries);

                            stats.RecordReduceSuccesses(leafPage.NumberOfEntries);
                        }
                    }
                    catch (Exception e)
                    {
                        _index.ThrowIfCorruptionException(e);

                        LogReductionError(e, reduceKeyHash, stats, updateStats: parentPage == -1, page: leafPage);
                    }
                }
            }

            long tmp = 0;
            using (Slice.External(indexContext.Allocator, (byte*)&tmp, sizeof(long), out Slice pageNumberSlice))
            {
                foreach (var freedPage in modifiedStore.FreedPages)
                {
                    tmp = Bits.SwapBytes(freedPage);
                    table.DeleteByKey(pageNumberSlice);
                }
            }

            while (parentPagesToAggregate.Count > 0 || branchesToAggregate.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                var branchPages = parentPagesToAggregate;
                parentPagesToAggregate = new HashSet<long>();

                foreach (var pageNumber in branchPages)
                {
                    page.Base = lowLevelTransaction.GetPage(pageNumber).Pointer;

                    try
                    {
                        if (page.IsBranch == false)
                        {
                            throw new UnexpectedReduceTreePageException("Parent page was found that wasn't a branch, error at " + page);
                        }

                        stats.RecordReduceAttempts(page.NumberOfEntries);

                        var parentPage = tree.GetParentPageOf(page);

                        using (var result = AggregateBranchPage(page, table, indexContext, branchesToAggregate, token))
                        {
                            if (parentPage == -1)
                            {
                                writer.DeleteReduceResult(reduceKeyHash, stats);

                                foreach (var output in result.GetOutputs())
                                {
                                    writer.IndexDocument(reduceKeyHash, output, stats, indexContext);
                                }
                            }
                            else
                            {
                                parentPagesToAggregate.Add(parentPage);

                                StoreAggregationResult(page.PageNumber, page.NumberOfEntries, table, result);
                            }

                            _metrics.MapReduceIndexes.ReducedPerSec.Mark(page.NumberOfEntries);

                            stats.RecordReduceSuccesses(page.NumberOfEntries);
                        }
                    }
                    catch (Exception e)
                    {
                        _index.ThrowIfCorruptionException(e);

                        LogReductionError(e, reduceKeyHash, stats, updateStats: true, page: page);
                    }
                    finally
                    {
                        branchesToAggregate.Remove(pageNumber);
                    }
                }

                if (parentPagesToAggregate.Count == 0 && branchesToAggregate.Count > 0)
                {
                    // we still have unaggregated branches which were modified but their children were not modified (branch page splitting) so we missed them
                    parentPagesToAggregate.Add(branchesToAggregate.First());
                }
            }

            if (compressedEmptyPages != null && compressedEmptyPages.Count > 0)
            {
                // we had some compressed pages that are empty after decompression
                // let's remove them and reduce the tree once again

                modifiedStore.ModifiedPages.Clear();
                modifiedStore.FreedPages.Clear();

                foreach (var pageNumber in compressedEmptyPages)
                {
                    page.Base = lowLevelTransaction.GetPage(pageNumber).Pointer;

                    using (var emptyPage = tree.DecompressPage(page, skipCache: true))
                    {
                        if (emptyPage.NumberOfEntries > 0) // could be changed meanwhile
                            continue;

                        modifiedStore.Tree.RemoveEmptyDecompressedPage(emptyPage);
                    }
                }

                HandleTreeReduction(indexContext, stats, modifiedStore, lowLevelTransaction, writer, reduceKeyHash, table, token);
            }
        }

        private AggregationResult AggregateLeafPage(TreePage page, LowLevelTransaction lowLevelTransaction, TransactionOperationContext indexContext, CancellationToken token)
        {
            using (_treeReductionStats.LeafAggregation.Start())
            {
                for (int i = 0; i < page.NumberOfEntries; i++)
                {
                    var valueReader = TreeNodeHeader.Reader(lowLevelTransaction, page.GetNode(i));
                    var reduceEntry = new BlittableJsonReaderObject(valueReader.Base, valueReader.Length, indexContext);

                    _aggregationBatch.Items.Add(reduceEntry);
                }

                return AggregateBatchResults(_aggregationBatch.Items, indexContext, token);
            }
        }

        private AggregationResult AggregateBranchPage(TreePage page, Table table, TransactionOperationContext indexContext, HashSet<long> remainingBranchesToAggregate, CancellationToken token)
        {
            using (_treeReductionStats.BranchAggregation.Start())
            {
                for (int i = 0; i < page.NumberOfEntries; i++)
                {
                    var pageNumber = page.GetNode(i)->PageNumber;
                    var childPageNumber = Bits.SwapBytes(pageNumber);
                    using (Slice.External(indexContext.Allocator, (byte*)&childPageNumber, sizeof(long), out Slice childPageNumberSlice))
                    {
                        if (table.ReadByKey(childPageNumberSlice, out TableValueReader tvr) == false)
                        {
                            if (remainingBranchesToAggregate.Contains(pageNumber))
                            {
                                // we have a modified branch page but its children were not modified (branch page splitting) so we didn't aggregated it yet, let's do it now
                                try
                                {
                                    page.Base = indexContext.Transaction.InnerTransaction.LowLevelTransaction.GetPage(pageNumber).Pointer;

                                    using (var result = AggregateBranchPage(page, table, indexContext, remainingBranchesToAggregate, token))
                                    {
                                        StoreAggregationResult(page.PageNumber, page.NumberOfEntries, table, result);
                                    }
                                }
                                finally
                                {
                                    remainingBranchesToAggregate.Remove(pageNumber);
                                }

                                table.ReadByKey(childPageNumberSlice, out tvr);
                            }
                            else
                            {
                                throw new InvalidOperationException("Couldn't find pre-computed results for existing page " + pageNumber);
                            }
                        }

                        var numberOfResults = *(int*)tvr.Read(2, out int size);

                        for (int j = 0; j < numberOfResults; j++)
                        {
                            _aggregationBatch.Items.Add(new BlittableJsonReaderObject(tvr.Read(3 + j, out size), size, indexContext));
                        }
                    }
                }

                return AggregateBatchResults(_aggregationBatch.Items, indexContext, token);
            }
        }

        private AggregationResult AggregateBatchResults(List<BlittableJsonReaderObject> aggregationBatch, TransactionOperationContext indexContext, CancellationToken token)
        {
            AggregationResult result;

            try
            {
                result = AggregateOn(aggregationBatch, indexContext, token);
            }
            finally
            {
                aggregationBatch.Clear();
            }

            return result;
        }

        private void StoreAggregationResult(long modifiedPage, int aggregatedEntries, Table table, AggregationResult result)
        {
            using (_treeReductionStats.StoringReduceResult.Start())
            {
                var pageNumber = Bits.SwapBytes(modifiedPage);
                var numberOfOutputs = result.Count;

                using (table.Allocate(out TableValueBuilder tvb))
                {
                    tvb.Add(pageNumber);
                    tvb.Add(aggregatedEntries);
                    tvb.Add(numberOfOutputs);

                    foreach (var output in result.GetOutputsToStore())
                    {
                        tvb.Add(output.BasePointer, output.Size);
                    }

                    table.Set(tvb);
                }
            }
        }

        protected abstract AggregationResult AggregateOn(List<BlittableJsonReaderObject> aggregationBatch, TransactionOperationContext indexContext, CancellationToken token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureValidTreeReductionStats(IndexingStatsScope stats)
        {
            if (_treeReductionStatsInstance == stats)
                return;

            _treeReductionStatsInstance = stats;

            _treeReductionStats.LeafAggregation = stats.For(IndexingOperation.Reduce.LeafAggregation, start: false);
            _treeReductionStats.BranchAggregation = stats.For(IndexingOperation.Reduce.BranchAggregation, start: false);
            _treeReductionStats.StoringReduceResult = stats.For(IndexingOperation.Reduce.StoringReduceResult, start: false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureValidNestedValuesReductionStats(IndexingStatsScope stats)
        {
            if (_nestedValuesReductionStatsInstance == stats)
                return;

            _nestedValuesReductionStatsInstance = stats;

            _nestedValuesReductionStats.NestedValuesRead = stats.For(IndexingOperation.Reduce.NestedValuesRead, start: false);
            _nestedValuesReductionStats.NestedValuesAggregation = stats.For(IndexingOperation.Reduce.NestedValuesAggregation, start: false);
        }

        private void LogReductionError(Exception error, LazyStringValue reduceKeyHash, IndexingStatsScope stats, bool updateStats, TreePage page,
            int numberOfNestedValues = -1)
        {
            var builder = new StringBuilder("Failed to execute reduce function on ");

            if (page != null)
                builder.Append($"page {page} ");
            else
                builder.Append("nested values ");

            builder.Append($"of '{_indexDefinition.Name}' index (reduce key hash: {reduceKeyHash}");

            var sampleItem = _aggregationBatch?.Items?.FirstOrDefault();

            if (sampleItem != null)
                builder.Append($", sample item to reduce: {sampleItem}");

            builder.Append(")");

            var message = builder.ToString();

            if (_logger.IsInfoEnabled)
                _logger.Info(message, error);

            if (updateStats)
            {
                var errorCount = page?.NumberOfEntries ?? numberOfNestedValues;

                Debug.Assert(errorCount != -1);

                stats.RecordReduceErrors(errorCount);
                stats.AddReduceError(message + $" Exception: {error}");
            }
        }

        private class AggegationBatch : IDisposable
        {
            public readonly List<BlittableJsonReaderObject> Items = new List<BlittableJsonReaderObject>();

            public void Dispose()
            {
                foreach (var item in Items)
                {
                    item.Dispose();
                }

                Items.Clear();
            }
        }

        private class TreeReductionStats
        {
            public IndexingStatsScope LeafAggregation;
            public IndexingStatsScope BranchAggregation;
            public IndexingStatsScope StoringReduceResult;
        }

        private class NestedValuesReductionStats
        {
            public IndexingStatsScope NestedValuesRead;
            public IndexingStatsScope NestedValuesAggregation;
        }
    }
}
