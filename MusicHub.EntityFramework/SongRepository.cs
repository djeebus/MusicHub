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
DECLARE @oldestLibraryId UNIQUEIDENTIFIER

SELECT TOP 1 @oldestLibraryId = l.LibraryId
FROM Libraries l
ORDER BY l.LastPlayed asc

SELECT TOP 1 Id, l.LibraryId
FROM Songs s
    JOIN Libraries l ON s.LibraryId = l.LibraryId
WHERE l.LibraryId = @oldestLibraryId
ORDER BY NEWID() DESC";

        internal class SongStub
        {
            public Guid Id { get; set; }
        }

        public Song GetRandomSong(Song previousSong)
        {
            var result = GetNextSongModel();

            if (result == null)
                return null;

            var song = this._db.Songs.First(s => s.Id == result.Id);

            return song.ToModel();
        }

        private SongStub GetNextSongModel()
        {
            var result = this._db.Database
                .SqlQuery<SongStub>(RandomSongQuery)
                .FirstOrDefault();
            return result;
        }

        public void MarkAsPlayed(string songId)
        {
            var guid = Guid.Parse(songId);

            var song = this._db.Songs.FirstOrDefault(s => s.Id == guid);
            if (song == null)
                throw new ArgumentOutOfRangeException("songId", songId, "Unknown song id");

            song.LastPlayed = DateTime.Now;
            song.PlayCount++;

            this._db.SaveChanges();
        }

        public void MarkUsersAsAway()
        {
            foreach (var user in this._db.Users)
            {
                user.IsAvailable = false;
            }

            this._db.SaveChanges();
        }
    }
}
