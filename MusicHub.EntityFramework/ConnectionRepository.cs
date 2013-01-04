    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly DbContext _db;

        public ConnectionRepository(DbContext db)
        {
            this._db = db;
        }

        public void ClientConnected(string userId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("userId");

            Guid guid = Guid.Parse(userId);

            var dbConnection = new DbConnection
            {
                Id = Guid.NewGuid(),
                SignalRConnectionId = connectionId,
                UserId = guid,
            };

            this._db.Connections.Add(dbConnection);
            this._db.SaveChanges();
        }

        public void ClientDisconnected(string connectionId)
        {
            var matched = this._db.Connections.Where(c => c.SignalRConnectionId == connectionId);

            foreach (var item in matched)
                this._db.Connections.Remove(item);

            this._db.SaveChanges();
        }
    }
}
