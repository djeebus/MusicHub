using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
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

        const string pruneSongsQuery = @"
DELETE Songs
FROM Songs s
    JOIN Library l ON s.LibraryId = l.LibraryId
WHERE s.LibraryId = @libraryId 
  AND s.LastSeen < l.LastSync";
        public void PruneSongs(string libraryId)
        {
            var result = this._db.Database.ExecuteSqlCommand(
                pruneSongsQuery,
                new SqlParameter("@libraryId", libraryId));

            Trace.WriteLine(string.Format("Pruned {0} songs", result), "SongRepository");
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

        internal class SongStub
        {
            public Guid Id { get; set; }

            public override string ToString()
            {
                return this.Id.ToString();
            }
        }

        public Song GetRandomSong(Song previousSong)
        {
            var result = GetNextSongModel();

            if (result == null)
                return null;

            var song = this._db.Songs.First(s => s.Id == result.Id);

            return song.ToModel();
        }

        const string RandomSongQuery = @"
SELECT TOP 1 Id
FROM Songs s
WHERE s.LibraryId = @oldestLibraryId
ORDER BY NEWID() DESC";

        private SongStub GetNextSongModel()
        {
            var lastLibrary = (from l in this._db.Libraries
                                 where l.Songs.Count() > 0 && l.User.IsAvailable
                                 orderby l.LastPlayed ascending
                                 select l).FirstOrDefault();
            if (lastLibrary == null)
                return null;

            Trace.WriteLine(string.Format("Oldest library: {0}", lastLibrary.Id), "SongRepository");

            var result = this._db.Database
                .SqlQuery<SongStub>(RandomSongQuery, new SqlParameter("@oldestLibraryId", lastLibrary.Id))
                .FirstOrDefault();

            Trace.WriteLine(string.Format("Random song selected: {0}", result), "SongRepository");
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
