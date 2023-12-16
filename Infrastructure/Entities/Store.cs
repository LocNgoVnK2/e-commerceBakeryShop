using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Entities
{
    [Table("Stores")]
    public class Store
    {
        [Key]
        public int IdStore { get; set; }
        public string Address {  get; set; }
        public string StoreName { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public bool IsUsed { get; set; }
    }
}
