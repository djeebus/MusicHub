using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    [Table("Users")]
    public class DbUser
    {
        [Column("UserId")]
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        public string DisplayName { get; set; }

        public bool IsAvailable { get; set; }

        public virtual ICollection<DbConnection> Connections { get; set; }
        public virtual ICollection<DbLibrary> Libraries { get; set; }
        public virtual ICollection<DbSongAffinity> Feelings { get; set; }

        internal User ToModel()
        {
            return new User
            {
                Id = this.Id.ToString(),
                DisplayName = this.DisplayName,
                Username = this.Username,
            };
        }        
    }
}
