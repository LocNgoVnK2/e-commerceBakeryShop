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
    public interface IRateRepository : IRepository<Rate>
    {
    }

    public class RateRepository : Repository<Rate>, IRateRepository
    {
        public RateRepository(EXDbContext context) : base(context)
        {
        }
    }
}
