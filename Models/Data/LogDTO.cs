using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Shop.Models.Data
{
    [Table("tblLog")]
    public class LogDTO
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Event { get; set; }
        public DateTime EventTime { get; set; }
    }
}