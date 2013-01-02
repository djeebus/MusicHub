using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    public class SongRepository : ISongRepository
    {
        private readonly DbContext _db;

        public SongRepository(DbContext db)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            this._db = db;
        }

        public void PruneSongs(string libraryId)
        {
            var guid = Guid.Parse(libraryId);

            var dbSongs = from s in _db.Songs
                          where s.Library.LastSync.HasValue &&
                                s.LastSeen < s.Library.LastSync
                          select s;

            foreach (var song in dbSongs)
                this._db.Songs.Remove(song);

            this._db.SaveChanges();
        }

        public Song UpsertSong(string libraryId, string externalId, string artist, string title, string album, uint? track, uint? year)
        {
            var guid = Guid.Parse(libraryId);

            var dbSong = (from s in _db.Songs
                          where s.LibraryId == guid && s.ExternalId == externalId
                          select s).FirstOrDefault();
            if (dbSong == null)
            {
                dbSong = new DbSong
                {
                    Id = Guid.NewGuid(),
                    LibraryId = guid,
                    ExternalId = externalId,
                };

                this._db.Songs.Add(dbSong);
            }

            dbSong.Album = album;
            dbSong.Artist = artist;
            dbSong.Title = title;
            dbSong.Track = track;
            dbSong.Year = year;

            dbSong.LastSeen = DateTime.Now;

            this._db.SaveChanges();

            return dbSong.ToModel();
        }

        const string RandomSongQuery = @"
SELECT TOP 1 *
FROM Songs
ORDER BY NEWID() DESC";

        public Song GetRandomSong()
        {
            var result = this._db.Database.SqlQuery<DbSong>(RandomSongQuery).FirstOrDefault(); ;
            if (result == null)
                return null;

            return result.ToModel();
        }
    }
}
