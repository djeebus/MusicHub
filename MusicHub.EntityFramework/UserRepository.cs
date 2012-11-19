using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    public class UserRepository : IUserRepository
    {
        private readonly DbContext _db;

        public UserRepository(DbContext db)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            this._db = db;
        }

        public User GetByName(string username)
        {
            var dbUser = this._db.Users.FirstOrDefault(u => u.Username == username);
            if (dbUser == null)
                return null;

            return dbUser.ToModel();
        }

        public User GetById(string userId)
        {
            var guid = Guid.Parse(userId);

            return this._db.Users.FirstOrDefault(u => u.Id == guid).ToModel();
        }

        public User ClientConnected(string userId, string connectionId)
        {
            throw new NotImplementedException();
        }

        public User ClientDisconnected(string connectionId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetOnlineUsers()
        {
            throw new NotImplementedException();
        }
    }
}
