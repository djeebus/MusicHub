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

        public void UpsertSong(Song song)
        {
            throw new NotImplementedException();
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
