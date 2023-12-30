using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Entities
{
    [Table("PromotionMapping")]
    public class PromotionMapping
    {
        [Key]
        public int? MappingID { get; set; }
        public int? PromotionID { get; set; }
        public int? OrderID { get; set; }
        public double? DiscountAmount { get; set; }
    }
}
