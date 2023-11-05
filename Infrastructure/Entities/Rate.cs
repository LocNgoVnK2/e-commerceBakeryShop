using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Entities
{
    [Table("Rates")]
    public class Rate
    {
        [Key]
        public int? RatingId { get; set; }
        public int Star { get; set; }
        public int ProductId { get; set; }
        public bool? StatusRate { get; set; }
    }
}
