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
    public interface IPromotionMappingRepository : IRepository<PromotionMapping>
    {
    }

    public class PromotionMappingRepository : Repository<PromotionMapping>, IPromotionMappingRepository
    {
        public PromotionMappingRepository(EXDbContext context) : base(context)
        {
        }
    }
    
}
