using InventoryAPI.DatabaseObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ModifyInventoryController : ControllerBase
    {
        [HttpPost("[action]")]
        public IActionResult AdjustQuantity(int product_Id, int quantityToAdd)
        {
            List<Product> products = DbHelper.Retrieve<Product>("SELECT * FROM Product", null, false);
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"Id", product_Id}, {"Quantity", quantityToAdd}
            };

            DbHelper.ExecuteNonQuery("dbo.spProduct_UpdateQuantity", parameters);

            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult AdjustPrice(int product_Id, float newPrice)
        {

            List<Product> products = DbHelper.Retrieve<Product>("SELECT * FROM Product", null, false);
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"Id", product_Id}, {"Price", newPrice}
            };

            DbHelper.ExecuteNonQuery("dbo.spProduct_UpdatePrice", parameters);

            return Ok();
        }
        [HttpPost("[action]")]
        public IActionResult AddNewInstrumentType([FromBody]InstrumentType instrumentType)
        {
            List<InstrumentType> instrumentTypes = DbHelper.Retrieve<InstrumentType>("SELECT * FROM InstrumentType", null, false);
            
            if (!instrumentTypes.Any(instruments => instruments.Type.Equals(instrumentType.Type, StringComparison.OrdinalIgnoreCase)))
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"Type", instrumentType.Type}, {"Description", instrumentType.Description}
            };
                DbHelper.ExecuteNonQuery("dbo.spInstrumentType_Insert", parameters);
            }
            return Ok();


        }

        [HttpPost("[action]")]
        public IActionResult AddNewProduct([FromBody]Product product)
        {
            List<Product> products = DbHelper.Retrieve<Product>("SELECT * FROM Product", null, false);
            if(!products.Any(products => products.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    {"Name", product.Name}, {"Description", product.Description}, {"ImageLocation", product.ImageLocation}, 
                    {"Price", product.Price}, {"Quantity", product.Quantity}, {"InstrumentType_Id", product.InstrumentType_Id}
                };
                DbHelper.ExecuteNonQuery("dbo.spProduct_Insert", parameters);
                return Ok();
            }

            return BadRequest();
            

        }
    }
}