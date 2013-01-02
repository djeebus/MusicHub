using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    [Table("Songs")]
    public class DbSong
    {
        public Guid Id { get; set; }
        [Required, StringLength(1024)]
        public string ExternalId { get; set; }

        public Guid LibraryId { get; set; }
        public virtual DbLibrary Library { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }

        public string Album { get; set; }
        public uint? Year { get; set; }
        public uint? Track { get; set; }

        public DateTime LastSeen { get; set; }

        internal Song ToModel()
        {
            return new Song
            {
                Album = this.Album,
                Artist = this.Artist,
                ExternalId = this.ExternalId,
                Id = this.Id.ToString(),
                LibraryId = this.LibraryId.ToString(),
                Title = this.Title,
                Track = this.Track,
                Year = this.Year,
            };
        }
    }
}
