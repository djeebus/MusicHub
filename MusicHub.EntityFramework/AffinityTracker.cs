using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    public class AffinityTracker : IAffinityTracker
    {
        private readonly DbContext _context;

        public AffinityTracker(DbContext context)
        {
            this._context = context;
        }

        public void Record(string userId, string songId, Affinity affinity)
        {
            Guid userGuid = Guid.Parse(userId);
            Guid songGuid = Guid.Parse(songId);

            var dbAffinity = (from sa in this._context.SongAffinities
                              where sa.UserId == userGuid &&
                                    sa.SongId == songGuid
                              select sa).FirstOrDefault();

            if (dbAffinity == null)
            {
                dbAffinity = new DbSongAffinity
                {
                    Id = Guid.NewGuid(),
                    UserId = userGuid,
                    SongId = songGuid,
                };

                this._context.SongAffinities.Add(dbAffinity);
            }

            dbAffinity.IsLove = affinity == Affinity.Love ? true : false;

            this._context.SaveChanges();
        }
    }
}
