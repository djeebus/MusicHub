using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    [Table("SongAffinities")]
    public class DbSongAffinity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual DbUser User { get; set; }

        public Guid SongId { get; set; }
        public virtual DbSong Song { get; set; }

        public bool IsLove { get; set; }
    }
}
