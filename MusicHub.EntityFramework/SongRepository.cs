using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
DECLARE @previousUserId UNIQUEIDENTIFIER

SELECT @previousUserId = l.UserId
FROM Songs s
    JOIN Libraries l on s.LibraryId = l.LibraryId
WHERE s.Id = @previousSongId

SET @previousUserId = ISNULL(@previousUserId, NEWID())

SELECT TOP 1 *
FROM Songs s
    JOIN Libraries l ON s.LibraryId = l.LibraryId
WHERE l.UserId != @previousUserId
ORDER BY NEWID() DESC";

        public Song GetRandomSong(Song previousSong)
        {
            var result = GetNextSongModel(previousSong);

            if (result != null)
                return result.ToModel();

            result = GetNextSongModel(null);
            if (result != null)
                return result.ToModel();

            return null;
        }

        private DbSong GetNextSongModel(Song previousSong)
        {
            var param = new SqlParameter("@previousSongId", previousSong == null ? Guid.Empty.ToString() : previousSong.Id);

            var result = this._db.Database
                .SqlQuery<DbSong>(RandomSongQuery, param)
                .FirstOrDefault();
            return result;
        }
    }
}
