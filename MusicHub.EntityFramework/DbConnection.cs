﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace MusicHub.EntityFramework
{
    [Table("Connections")]
    public class DbConnection
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual DbUser User { get; set; }

        public string SignalRConnectionId { get; set; }
    }
}
