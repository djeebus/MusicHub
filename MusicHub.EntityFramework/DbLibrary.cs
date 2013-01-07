using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    [Table("Libraries")]
    public class DbLibrary
    {
        [Column("LibraryId")]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual DbUser User { get; set; }

        [Column("TypeId")]
        public LibraryType Type { get; set; }

        [StringLength(1024)]
        public string Path { get; set; }
        [StringLength(1024)]
        public string Username { get; set; }
        [StringLength(1024)]
        public string Password { get; set; }

        public virtual ICollection<DbSong> Songs { get; set; }

        public DateTime? LastSync { get; set; }

        private string GetName(DbLibrary library)
        {
            switch (library.Type)
            {
                case LibraryType.SharedFolder:
                    return library.Path;

                case LibraryType.GoogleMusic:
                    return library.Username;

                default:
                    throw new NotImplementedException();
            }
        }


        internal LibraryInfo ToModel()
        {
            return new LibraryInfo
            {
                Id = this.Id.ToString(),
                Name = GetName(this),
                Username = this.Username,
                Password = this.Password,
                Location = this.Path,
                Type = this.Type,
                TotalSongs = this.Songs == null ? 0 : this.Songs.Count(),
            };
        }
    }
}
