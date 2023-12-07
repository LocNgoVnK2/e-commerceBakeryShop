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
    public interface IStoreRepository : IRepository<Store>
    {
    }

    public class StoreRepository : Repository<Store>, IStoreRepository
    {
        public StoreRepository(EXDbContext context) : base(context)
        {
        }
    }

}
