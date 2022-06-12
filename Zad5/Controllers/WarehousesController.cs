using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Zad5.Controllers
{
    [Route("/api/warehouses")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IDbService dbService;
        public WarehousesController(IDbService dbService)
        {
            this.dbService = dbService;
        }

        [HttpPost]
        public async Task<IActionResult>  AddWarehouseAsync(Warehouse warehouse)
        {
            try
            {
                await dbService.AddWarehouse(warehouse);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }
    }
}
