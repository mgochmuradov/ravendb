using System.Threading;
using System.Threading.Tasks;

using Raven.Abstractions.Database.Smuggler.Database;

namespace Raven.Smuggler.Database
{
    internal abstract class SmugglerBase
    {
        protected readonly DatabaseSmugglerOptions Options;

        protected readonly DatabaseSmugglerNotifications Notifications;

        protected readonly IDatabaseSmugglerSource Source;

        protected readonly IDatabaseSmugglerDestination Destination;

        protected SmugglerBase(DatabaseSmugglerOptions options, DatabaseSmugglerNotifications notifications, IDatabaseSmugglerSource source, IDatabaseSmugglerDestination destination)
        {
            Options = options;
            Notifications = notifications;
            Source = source;
            Destination = destination;
        }

        public abstract Task SmuggleAsync(DatabaseSmugglerOperationState state, CancellationToken cancellationToken);
    }
}
