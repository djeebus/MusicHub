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
                        Name = GetName(l),
                    }).ToArray();
        }

        private string GetName(DbLibrary l)
        {
            switch (l.Type)
            {
                case LibraryType.SharedFolder:
                    return l.Path;

                case LibraryType.GoogleMusic:
                    return l.Username;

                default:
                    throw new NotImplementedException();
            }
        }

        public void Create(string userId, LibraryType type, string path, string username, string password)
        {
            var guid = Guid.Parse(userId);

            var dbLibrary = new DbLibrary
            {
                Id = Guid.NewGuid(),
                Path = path,
                Username = username,
                Password = password,
                Type = type,
                UserId = guid,
            };

            this._db.Libraries.Add(dbLibrary);
            this._db.SaveChanges();
        }

        public void Delete(string libraryId)
        {
            var guid = Guid.Parse(libraryId);

            var dbLibrary = this._db.Libraries.FirstOrDefault(l => l.Id == guid);
            if (dbLibrary == null)
                throw new ArgumentOutOfRangeException("libraryId", libraryId, "Unknown library");

            this._db.Libraries.Remove(dbLibrary);
            this._db.SaveChanges();
        }
    }
}
