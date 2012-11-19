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

        [Required]
        public string Path { get; set; }
    }
}
