using System.Collections.Generic;
using System.Threading.Tasks;


namespace Zad5
{
    public interface IDbService
    {
        public Task<int> AddWarehouse(Warehouse warehouse);
    }
}
