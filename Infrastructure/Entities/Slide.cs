using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Entities
{
    [Table("Slides")]
    public class Slide
    {
            [Key]
            public int SlideID { get; set; }
            public byte[]? Image { get; set; }
            public int Precedence { get; set; }

        
    }
}
