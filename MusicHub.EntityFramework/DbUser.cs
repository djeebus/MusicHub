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

        [Required]
        public string Username { get; set; }

        [Required]
        public string DisplayName { get; set; }

        public virtual ICollection<DbConnection> Connections { get; set; }

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
