using System.Threading;
using System.Threading.Tasks;

using Raven.Abstractions.Util;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;
using Raven.Smuggler.Common;

namespace Raven.Smuggler.Database.Streams
{
    public class DatabaseSmugglerStreamDocumentActions : SmugglerStreamActionsBase, IDatabaseSmugglerDocumentActions
    {
        public DatabaseSmugglerStreamDocumentActions(JsonTextWriter writer)
            : base(writer, "Docs")
        {
        }

        public Task WriteDocumentAsync(RavenJObject document, CancellationToken cancellationToken)
        {
            document.WriteTo(Writer);
            return new CompletedTask();
        }
    }
}
