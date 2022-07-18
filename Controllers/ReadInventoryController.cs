using InventoryAPI.DatabaseObjects;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ReadInventoryController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult GetInstrumentTypes()
        {
            List<InstrumentType> instrumentTypes = DbHelper.Retrieve<InstrumentType>("SELECT * FROM InstrumentType", null, false);
            return Ok(instrumentTypes);
        }

        [HttpGet("[action]")]
        public IActionResult GetProducts()
        {
            List<Product> products = DbHelper.Retrieve<Product>("SELECT * FROM Product", null, false);
            return Ok(products);
        }
    }
}
