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
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("userId");

            var guid = Guid.Parse(userId);

            return this._db.Users.FirstOrDefault(u => u.Id == guid).ToModel();
        }

        public User[] GetOnlineUsers()
        {
            var onlineUsers = from u in this._db.Users
                              where u.Connections.Count() > 0
                              select u;

            return (from u in onlineUsers.ToArray()
                    select u.ToModel()).ToArray();
        }

        public User EnsureUser(string username, string displayName)
        {
            var user = this.GetByName(username);
            if (user != null)
                return user;

            return Create(username, displayName);
        }

        public User Create(string username, string displayName)
        {
            var dbUser = new DbUser
            {
                Id = Guid.NewGuid(),
                DisplayName = displayName,
                Username = username,
            };

            this._db.Users.Add(dbUser);
            this._db.SaveChanges();

            return dbUser.ToModel();
        }
    }
}
