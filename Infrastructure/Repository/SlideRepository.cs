using Infrastructure.EF;
using Infrastructure.Entities;
using Infrastructure.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public interface ISlideRepository : IRepository<Slide>
    {
    }

    public class SlideRepository : Repository<Slide>, ISlideRepository
    {
        public SlideRepository(EXDbContext context) : base(context)
        {
        }
    }

}
