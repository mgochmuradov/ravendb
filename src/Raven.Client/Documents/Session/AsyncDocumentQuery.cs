using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session.Operations;
using Raven.Client.Documents.Session.Operations.Lazy;
using Raven.Client.Documents.Session.Tokens;
using Raven.Client.Extensions;
using Raven.Client.Util;
using Sparrow.Json;

namespace Raven.Client.Documents.Session
{
    /// <summary>
    /// A query against a Raven index
    /// </summary>
    public partial class AsyncDocumentQuery<T> : AbstractDocumentQuery<T, AsyncDocumentQuery<T>>, IAsyncDocumentQuery<T>,
        IAsyncRawDocumentQuery<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDocumentQuery{T}"/> class.
        /// </summary>
        public AsyncDocumentQuery(InMemoryDocumentSessionOperations session, string indexName, string collectionName, bool isGroupBy, DeclareToken declareToken = null, List<LoadToken> loadTokens = null, string fromAlias = null)
            : base(session, indexName, collectionName, isGroupBy, declareToken, loadTokens, fromAlias)
        {
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Include(string path)
        {
            Include(path);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Include(Expression<Func<T, object>> path)
        {
            Include(path);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Not
        {
            get
            {
                NegateNext();
                return this;
            }
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.Take(int count)
        {
            Take(count);
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.Take(int count)
        {
            Take(count);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.Skip(int count)
        {
            Skip(count);
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.Skip(int count)
        {
            Skip(count);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereLucene(string fieldName, string whereClause)
        {
            WhereLucene(fieldName, whereClause);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereEquals(string fieldName, object value, bool exact)
        {
            WhereEquals(fieldName, value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereEquals(string fieldName, MethodCall value, bool exact)
        {
            WhereEquals(fieldName, value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereEquals<TValue>(Expression<Func<T, TValue>> propertySelector, TValue value, bool exact)
        {
            WhereEquals(GetMemberQueryPath(propertySelector.Body), value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereEquals<TValue>(Expression<Func<T, TValue>> propertySelector, MethodCall value, bool exact)
        {
            WhereEquals(GetMemberQueryPath(propertySelector.Body), value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereEquals(WhereParams whereParams)
        {
            WhereEquals(whereParams);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereNotEquals(string fieldName, object value, bool exact)
        {
            WhereNotEquals(fieldName, value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereNotEquals(string fieldName, MethodCall value, bool exact)
        {
            WhereNotEquals(fieldName, value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereNotEquals<TValue>(Expression<Func<T, TValue>> propertySelector, TValue value, bool exact)
        {
            WhereNotEquals(GetMemberQueryPath(propertySelector.Body), value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereNotEquals<TValue>(Expression<Func<T, TValue>> propertySelector, MethodCall value, bool exact)
        {
            WhereNotEquals(GetMemberQueryPath(propertySelector.Body), value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereNotEquals(WhereParams whereParams)
        {
            WhereNotEquals(whereParams);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereIn(string fieldName, IEnumerable<object> values, bool exact)
        {
            WhereIn(fieldName, values, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereIn<TValue>(Expression<Func<T, TValue>> propertySelector, IEnumerable<TValue> values, bool exact)
        {
            WhereIn(GetMemberQueryPath(propertySelector.Body), values.Cast<object>(), exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereStartsWith(string fieldName, object value)
        {
            WhereStartsWith(fieldName, value);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereStartsWith<TValue>(Expression<Func<T, TValue>> propertySelector, TValue value)
        {
            WhereStartsWith(GetMemberQueryPath(propertySelector.Body), value);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereEndsWith(string fieldName, object value)
        {
            WhereEndsWith(fieldName, value);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereEndsWith<TValue>(Expression<Func<T, TValue>> propertySelector, TValue value)
        {
            WhereEndsWith(GetMemberQueryPath(propertySelector.Body), value);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereBetween(string fieldName, object start, object end, bool exact)
        {
            WhereBetween(fieldName, start, end, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereBetween<TValue>(Expression<Func<T, TValue>> propertySelector, TValue start, TValue end, bool exact)
        {
            WhereBetween(GetMemberQueryPath(propertySelector.Body), start, end, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereGreaterThan(string fieldName, object value, bool exact)
        {
            WhereGreaterThan(fieldName, value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereGreaterThan<TValue>(Expression<Func<T, TValue>> propertySelector, TValue value, bool exact)
        {
            WhereGreaterThan(GetMemberQueryPath(propertySelector.Body), value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereGreaterThanOrEqual(string fieldName, object value, bool exact)
        {
            WhereGreaterThanOrEqual(fieldName, value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereGreaterThanOrEqual<TValue>(Expression<Func<T, TValue>> propertySelector, TValue value, bool exact)
        {
            WhereGreaterThanOrEqual(GetMemberQueryPath(propertySelector.Body), value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereLessThan(string fieldName, object value, bool exact)
        {
            WhereLessThan(fieldName, value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereLessThan<TValue>(Expression<Func<T, TValue>> propertySelector, TValue value, bool exact)
        {
            WhereLessThan(GetMemberQueryPath(propertySelector.Body), value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereLessThanOrEqual(string fieldName, object value, bool exact)
        {
            WhereLessThanOrEqual(fieldName, value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereLessThanOrEqual<TValue>(Expression<Func<T, TValue>> propertySelector, TValue value, bool exact)
        {
            WhereLessThanOrEqual(GetMemberQueryPath(propertySelector.Body), value, exact);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereExists<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            WhereExists(GetMemberQueryPath(propertySelector.Body));
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereExists(string fieldName)
        {
            WhereExists(fieldName);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereRegex<TValue>(Expression<Func<T, TValue>> propertySelector, string pattern)
        {
            WhereRegex(GetMemberQueryPath(propertySelector.Body), pattern);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.WhereRegex(string fieldName, string pattern)
        {
            WhereRegex(fieldName, pattern);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.AndAlso()
        {
            AndAlso();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrElse()
        {
            OrElse();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Boost(decimal boost)
        {
            Boost(boost);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Fuzzy(decimal fuzzy)
        {
            Fuzzy(fuzzy);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Proximity(int proximity)
        {
            Proximity(proximity);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.RandomOrdering()
        {
            RandomOrdering();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.RandomOrdering(string seed)
        {
            RandomOrdering(seed);
            return this;
        }

#if FEATURE_CUSTOM_SORTING
        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.CustomSortUsing(string typeName, bool descending)
        {
            CustomSortUsing(typeName, descending);
            return this;
        }
#endif

        /// <inheritdoc />
        IAsyncDocumentQuery<TResult> IAsyncDocumentQuery<T>.OfType<TResult>()
        {
            return CreateDocumentQueryInternal<TResult>();
        }

        /// <inheritdoc />
        IAsyncGroupByDocumentQuery<T> IAsyncDocumentQuery<T>.GroupBy(string fieldName, params string[] fieldNames)
        {
            GroupBy(fieldName, fieldNames);
            return new AsyncGroupByDocumentQuery<T>(this);
        }

        /// <inheritdoc />
        IAsyncGroupByDocumentQuery<T> IAsyncDocumentQuery<T>.GroupBy((string Name, GroupByMethod Method) field, params (string Name, GroupByMethod Method)[] fields)
        {
            GroupBy(field, fields);
            return new AsyncGroupByDocumentQuery<T>(this);
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderBy(string field, OrderingType ordering)
        {
            OrderBy(field, ordering);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderBy<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            OrderBy(GetMemberQueryPathForOrderBy(propertySelector), OrderingUtil.GetOrderingOfType(propertySelector.ReturnType));
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderBy<TValue>(Expression<Func<T, TValue>> propertySelector, OrderingType ordering)
        {
            OrderBy(GetMemberQueryPathForOrderBy(propertySelector), ordering);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderBy<TValue>(params Expression<Func<T, TValue>>[] propertySelectors)
        {
            foreach (var item in propertySelectors)
            {
                OrderBy(GetMemberQueryPathForOrderBy(item), OrderingUtil.GetOrderingOfType(item.ReturnType));
            }

            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderByDescending(string field, OrderingType ordering)
        {
            OrderByDescending(field, ordering);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderByDescending<TValue>(Expression<Func<T, TValue>> propertySelector)
        {
            OrderByDescending(GetMemberQueryPathForOrderBy(propertySelector), OrderingUtil.GetOrderingOfType(propertySelector.ReturnType));
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderByDescending<TValue>(Expression<Func<T, TValue>> propertySelector, OrderingType ordering)
        {
            OrderByDescending(GetMemberQueryPathForOrderBy(propertySelector), ordering);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderByDescending<TValue>(params Expression<Func<T, TValue>>[] propertySelectors)
        {
            foreach (var item in propertySelectors)
            {
                OrderByDescending(GetMemberQueryPathForOrderBy(item), OrderingUtil.GetOrderingOfType(item.ReturnType));
            }

            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.BeforeQueryExecuted(Action<IndexQuery> beforeQueryExecuted)
        {
            BeforeQueryExecuted(beforeQueryExecuted);
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.BeforeQueryExecuted(Action<IndexQuery> beforeQueryExecuted)
        {
            BeforeQueryExecuted(beforeQueryExecuted);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.WaitForNonStaleResults(TimeSpan? waitTimeout)
        {
            WaitForNonStaleResults(waitTimeout);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.AddParameter(string name, object value)
        {
            AddParameter(name, value);
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.AddParameter(string name, object value)
        {
            AddParameter(name, value);
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.WaitForNonStaleResults(TimeSpan? waitTimeout)
        {
            WaitForNonStaleResults(waitTimeout);
            return this;
        }

        /// <inheritdoc />
        public IAsyncDocumentQuery<TProjection> SelectFields<TProjection>()
        {
            var propertyInfos = ReflectionUtil.GetPropertiesAndFieldsFor<TProjection>(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            var projections = propertyInfos.Select(x => x.Name).ToArray();
            var identityProperty = Conventions.GetIdentityProperty(typeof(TProjection));
            var fields = propertyInfos.Select(p => p == identityProperty ? Constants.Documents.Indexing.Fields.DocumentIdFieldName : p.Name).ToArray();
            return SelectFields<TProjection>(new QueryData(fields, projections));
        }

        /// <inheritdoc />
        public IAsyncDocumentQuery<TProjection> SelectFields<TProjection>(params string[] fields)
        {
            return SelectFields<TProjection>(new QueryData(fields, fields));
        }

        /// <inheritdoc />
        public IAsyncDocumentQuery<TProjection> SelectFields<TProjection>(QueryData queryData)
        {
            return CreateDocumentQueryInternal<TProjection>(queryData);
        }

        /// <inheritdoc />
        Lazy<Task<int>> IAsyncDocumentQueryBase<T>.CountLazilyAsync(CancellationToken token)
        {
            if (QueryOperation == null)
            {
                Take(0);
                QueryOperation = InitializeQueryOperation();
            }

            var lazyQueryOperation = new LazyQueryOperation<T>(TheSession.Conventions, QueryOperation, AfterQueryExecutedCallback);

            return ((AsyncDocumentSession)TheSession).AddLazyCountOperation(lazyQueryOperation, token);
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.AddOrder(string fieldName, bool descending, OrderingType ordering)
        {
            if (descending)
                OrderByDescending(fieldName, ordering);
            else
                OrderBy(fieldName, ordering);

            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderByScore()
        {
            OrderByScore();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OrderByScoreDescending()
        {
            OrderByScoreDescending();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.AddOrder<TValue>(Expression<Func<T, TValue>> propertySelector, bool descending, OrderingType ordering)
        {
            var fieldName = GetMemberQueryPath(propertySelector.Body);
            if (descending)
                OrderByDescending(fieldName, ordering);
            else
                OrderBy(fieldName, ordering);

            return this;
        }

        void IQueryBase<T, IAsyncDocumentQuery<T>>.AfterQueryExecuted(Action<QueryResult> action)
        {
            AfterQueryExecuted(action);
        }


        void IQueryBase<T, IAsyncRawDocumentQuery<T>>.AfterQueryExecuted(Action<QueryResult> action)
        {
            AfterQueryExecuted(action);
        }

        void IQueryBase<T, IAsyncDocumentQuery<T>>.AfterStreamExecuted(Action<BlittableJsonReaderObject> action)
        {
            AfterStreamExecuted(action);
        }

        void IQueryBase<T, IAsyncRawDocumentQuery<T>>.AfterStreamExecuted(Action<BlittableJsonReaderObject> action)
        {
            AfterStreamExecuted(action);
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.OpenSubclause()
        {
            OpenSubclause();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Search(string fieldName, string searchTerms, SearchOperator @operator)
        {
            Search(fieldName, searchTerms, @operator);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Search<TValue>(Expression<Func<T, TValue>> propertySelector, string searchTerms, SearchOperator @operator)
        {
            Search(GetMemberQueryPath(propertySelector.Body), searchTerms, @operator);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.CloseSubclause()
        {
            CloseSubclause();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Intersect()
        {
            Intersect();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.ContainsAny(string fieldName, IEnumerable<object> values)
        {
            ContainsAny(fieldName, values);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.ContainsAny<TValue>(Expression<Func<T, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            ContainsAny(GetMemberQueryPath(propertySelector.Body), values.Cast<object>());
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.ContainsAll(string fieldName, IEnumerable<object> values)
        {
            ContainsAll(fieldName, values);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IFilterDocumentQueryBase<T, IAsyncDocumentQuery<T>>.ContainsAll<TValue>(Expression<Func<T, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            ContainsAll(GetMemberQueryPath(propertySelector.Body), values.Cast<object>());
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.Statistics(out QueryStatistics stats)
        {
            Statistics(out stats);
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.Statistics(out QueryStatistics stats)
        {
            Statistics(out stats);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.UsingDefaultOperator(QueryOperator queryOperator)
        {
            UsingDefaultOperator(queryOperator);
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.UsingDefaultOperator(QueryOperator queryOperator)
        {
            UsingDefaultOperator(queryOperator);
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.NoTracking()
        {
            NoTracking();
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.NoTracking()
        {
            NoTracking();
            return this;
        }

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.NoCaching()
        {
            NoCaching();
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.NoCaching()
        {
            NoCaching();
            return this;
        }

#if FEATURE_SHOW_TIMINGS
        /// <inheritdoc />
        IAsyncDocumentQuery<T> IQueryBase<T, IAsyncDocumentQuery<T>>.ShowTimings()
        {
            ShowTimings();
            return this;
        }

        /// <inheritdoc />
        IAsyncRawDocumentQuery<T> IQueryBase<T, IAsyncRawDocumentQuery<T>>.ShowTimings()
        {
            ShowTimings();
            return this;
        }
#endif

        /// <inheritdoc />
        IAsyncDocumentQuery<T> IDocumentQueryBase<T, IAsyncDocumentQuery<T>>.Distinct()
        {
            Distinct();
            return this;
        }

#if FEATURE_EXPLAIN_SCORES
        /// <inheritdoc />
        public IAsyncDocumentQuery<T> ExplainScores()
        {
            ShouldExplainScores = true;
            return this;
        }
#endif

        /// <inheritdoc />
        async Task<List<T>> IAsyncDocumentQueryBase<T>.ToListAsync(CancellationToken token)
        {
            await InitAsync(token).ConfigureAwait(false);
            var tuple = await ProcessEnumerator(QueryOperation).WithCancellation(token).ConfigureAwait(false);
            return tuple.Item2;
        }

        /// <inheritdoc />
        async Task<T> IAsyncDocumentQueryBase<T>.FirstAsync(CancellationToken token)
        {
            var operation = await ExecuteQueryOperation(1, token).ConfigureAwait(false);
            return operation.First();
        }

        /// <inheritdoc />
        async Task<T> IAsyncDocumentQueryBase<T>.FirstOrDefaultAsync(CancellationToken token)
        {
            var operation = await ExecuteQueryOperation(1, token).ConfigureAwait(false);
            return operation.FirstOrDefault();
        }

        /// <inheritdoc />
        async Task<T> IAsyncDocumentQueryBase<T>.SingleAsync(CancellationToken token)
        {
            var operation = await ExecuteQueryOperation(2, token).ConfigureAwait(false);
            return operation.Single();
        }

        /// <inheritdoc />
        async Task<T> IAsyncDocumentQueryBase<T>.SingleOrDefaultAsync(CancellationToken token)
        {
            var operation = await ExecuteQueryOperation(2, token).ConfigureAwait(false);
            return operation.SingleOrDefault();
        }

        private async Task<IEnumerable<T>> ExecuteQueryOperation(int take, CancellationToken token)
        {
            if (PageSize.HasValue == false || PageSize > take)
                Take(take);

            await InitAsync(token).ConfigureAwait(false);

            return QueryOperation.Complete<T>();
        }

        /// <inheritdoc />
        Lazy<Task<IEnumerable<T>>> IAsyncDocumentQueryBase<T>.LazilyAsync(Action<IEnumerable<T>> onEval)
        {
            if (QueryOperation == null)
            {
                QueryOperation = InitializeQueryOperation();
            }

            var lazyQueryOperation = new LazyQueryOperation<T>(TheSession.Conventions, QueryOperation, AfterQueryExecutedCallback);
            return ((AsyncDocumentSession)TheSession).AddLazyOperation(lazyQueryOperation, onEval);
        }

        /// <inheritdoc />
        async Task<int> IAsyncDocumentQueryBase<T>.CountAsync(CancellationToken token)
        {
            Take(0);
            var result = await GetQueryResultAsync(token).ConfigureAwait(false);
            return result.TotalResults;
        }

        private static Task<Tuple<QueryResult, List<T>>> ProcessEnumerator(QueryOperation currentQueryOperation)
        {
            var list = currentQueryOperation.Complete<T>();
            return Task.FromResult(Tuple.Create(currentQueryOperation.CurrentQueryResults, list));
        }

        /// <inheritdoc />
        public async Task<QueryResult> GetQueryResultAsync(CancellationToken token = default(CancellationToken))
        {
            await InitAsync(token).ConfigureAwait(false);

            return QueryOperation.CurrentQueryResults.CreateSnapshot();
        }

        protected async Task InitAsync(CancellationToken token)
        {
            if (QueryOperation != null)
                return;

            var beforeQueryExecutedEventArgs = new BeforeQueryEventArgs(TheSession, this);
            TheSession.OnBeforeQueryInvoke(beforeQueryExecutedEventArgs);

            QueryOperation = InitializeQueryOperation();
            await ExecuteActualQueryAsync(token).ConfigureAwait(false);
        }

        private async Task ExecuteActualQueryAsync(CancellationToken token)
        {
            using (QueryOperation.EnterQueryContext())
            {
                var command = QueryOperation.CreateRequest();
                await TheSession.RequestExecutor.ExecuteAsync(command, TheSession.Context, TheSession.SessionInfo, token).ConfigureAwait(false);
                QueryOperation.SetResult(command.Result);
            }

            InvokeAfterQueryExecuted(QueryOperation.CurrentQueryResults);
        }

        private AsyncDocumentQuery<TResult> CreateDocumentQueryInternal<TResult>(QueryData queryData = null)
        {
            var newFieldsToFetch = queryData != null && queryData.Fields.Length > 0
                ? FieldsToFetchToken.Create(queryData.Fields, queryData.Projections.ToArray(), queryData.IsCustomFunction)
                : null;

            if (newFieldsToFetch != null)
                UpdateFieldsToFetchToken(newFieldsToFetch);

            var query = new AsyncDocumentQuery<TResult>(
                TheSession,
                IndexName,
                CollectionName,
                IsGroupBy,
                queryData?.DeclareToken,
                queryData?.LoadTokens,
                queryData?.FromAlias)
            {
                PageSize = PageSize,
                SelectTokens = SelectTokens,
                FieldsToFetchToken = FieldsToFetchToken,
                WhereTokens = WhereTokens,
                OrderByTokens = OrderByTokens,
                GroupByTokens = GroupByTokens,
                QueryParameters = QueryParameters,
                Start = Start,
                Timeout = Timeout,
                QueryStats = QueryStats,
                TheWaitForNonStaleResults = TheWaitForNonStaleResults,
                Negate = Negate,
                Includes = new HashSet<string>(Includes),
                RootTypes = { typeof(T) },
                BeforeQueryExecutedCallback = BeforeQueryExecutedCallback,
                AfterQueryExecutedCallback = AfterQueryExecutedCallback,
                AfterStreamExecutedCallback = AfterStreamExecutedCallback,
#if FEATURE_HIGHLIGHTING
                HighlightedFields = new List<HighlightedField>(HighlightedFields),
                HighlighterPreTags = HighlighterPreTags,
                HighlighterPostTags = HighlighterPostTags,
#endif
                DisableEntitiesTracking = DisableEntitiesTracking,
                DisableCaching = DisableCaching,
#if FEATURE_SHOW_TIMINGS
                ShowQueryTimings = ShowQueryTimings,
#endif
#if FEATURE_EXPLAIN_SCORES
                ShouldExplainScores = ShouldExplainScores,
#endif
                IsIntersect = IsIntersect,
                DefaultOperator = DefaultOperator
            };

            return query;
        }
    }
}
