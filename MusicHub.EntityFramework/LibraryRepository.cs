using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    public class LibraryRepository : ILibraryRepository
    {
        private readonly DbContext _db;

        public LibraryRepository(DbContext db)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            this._db = db;
        }

        public Library[] GetLibrariesForUser(string userId)
        {
            Guid guid = Guid.Parse(userId);

            var dbLibraries = from l in this._db.Libraries
                              where l.UserId == guid
                              select l;

            return (from l in dbLibraries.ToArray()
                    select new Library
                    {
                        Id = l.Id.ToString(),
                        Path = l.Path,
                        UserId = l.UserId.ToString(),
                    }).ToArray();
        }

        public void Create(string userId, string path)
        {
            var guid = Guid.Parse(userId);

            var dbLibrary = new DbLibrary
            {
                Id = Guid.NewGuid(),
                Path = path,
                UserId = guid,
            };

            this._db.Libraries.Add(dbLibrary);
            this._db.SaveChanges();
        }
    }
}
