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

        public User ClientConnected(string userId, string connectionId)
        {
            throw new NotImplementedException();
        }

        public User ClientDisconnected(string connectionId)
        {
            throw new NotImplementedException();
        }
    }
}
