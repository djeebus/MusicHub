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

        public LibraryInfo[] GetLibraries()
        {
            return (from l in _db.Libraries.AsEnumerable()
                    select l.ToModel()).ToArray();
        }

        public LibraryInfo[] GetLibrariesForUser(string userId)
        {
            Guid guid = Guid.Parse(userId);

            var dbLibraries = from l in this._db.Libraries.AsNoTracking()
                              where l.UserId == guid
                              select l;

            return (from l in dbLibraries.ToArray()
                    select l.ToModel()).ToArray();
        }

        public LibraryInfo Create(string userId, LibraryType type, string path, string username, string password)
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

            return dbLibrary.ToModel();
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

        public void UpdateLastSyncDate(string libraryId)
        {
            var guid = Guid.Parse(libraryId);

            var dbLibrary = this._db.Libraries.FirstOrDefault(l => l.Id == guid);
            if (dbLibrary == null)
                throw new ArgumentOutOfRangeException("libraryId", libraryId, "Library ID does not exist");

            dbLibrary.LastSync = DateTime.Now;

            this._db.SaveChanges();
        }

        public LibraryInfo GetLibrary(string libraryId)
        {
            var guid = Guid.Parse(libraryId);

            var dbLibrary = this._db.Libraries.AsNoTracking().FirstOrDefault(l => l.Id == guid);
            if (dbLibrary == null)
                return null;

            return dbLibrary.ToModel();
        }

        public void MarkAsPlayed(string libraryId)
        {
            var guid = Guid.Parse(libraryId);

            var library = _db.Libraries.FirstOrDefault(l => l.Id == guid);
            if (libraryId == null)
                throw new ArgumentOutOfRangeException("libraryId", libraryId, "Unknown library id");

            library.LastPlayed = DateTime.Now;
            library.PlayCount++;
            this._db.SaveChanges();
        }
    }
}
