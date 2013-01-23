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

        const int CountForMostQueries = 3;

        IQueryable<string> GetOrderedArtistsQuery()
        {
            return from s in this._context.Songs
                   group s by s.Artist into artist
                   orderby artist.Sum(s => s.Feelings.Sum(f => f.IsLove ? 1 : -1))
                   select artist.Key;
        }

        public string[] GetMostLovedArtists()
        {
            return GetOrderedArtistsQuery()
                .Take(CountForMostQueries)
                .ToArray();
        }

        public string[] GetMostHatedArtists()
        {
            return GetOrderedArtistsQuery()
                .Reverse()
                .Take(CountForMostQueries)
                .ToArray();
        }

        private IQueryable<DbUser> GetOrderedxxxedQuery()
        {
            return from u in _context.Users
                   orderby u.Libraries.Sum(l => l.Songs.Sum(s => s.Feelings.Sum(f => f.IsLove ? 1 : -1)))
                   select u;
        }

        public User[] GetMostLovedUsers()
        {
            return GetOrderedxxxedQuery()
                .Take(CountForMostQueries)
                .AsEnumerable()
                .Select(u => u.ToModel())
                .ToArray();
        }

        public User[] GetMostHatedUsers()
        {
            return GetOrderedxxxedQuery()
                .Reverse()
                .Take(CountForMostQueries)
                .AsEnumerable()
                .Select(u => u.ToModel())
                .ToArray();
        }

        private IOrderedQueryable<DbUser> GetOrderedxxxingQuery()
        {
            return from u in _context.Users
                   orderby u.Feelings.Sum(f => f.IsLove ? 1 : -1)
                   select u;
        }

        public User[] GetMostLovingUsers()
        {
            return GetOrderedxxxingQuery()
                .Take(CountForMostQueries)
                .AsEnumerable()
                .Select(u => u.ToModel())
                .ToArray();
        }

        public User[] GetMostHatingUsers()
        {
            return GetOrderedxxxingQuery()
                .Reverse()
                .Take(CountForMostQueries)
                .AsEnumerable()
                .Select(u => u.ToModel())
                .ToArray();
        }
    }
}
