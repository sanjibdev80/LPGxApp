using LPGxWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LPGxWebApp.Request;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductInfosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductInfosController(AppDbContext context)
        {
            _context = context;
        }


        // GET: api/ProductInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductInfos>>> GetProductInfos()
        {
            return await _context.PRODUCTSTAB.ToListAsync();
        }

        // GET: api/ProductInfos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductInfos>> GetProductInfos(string id)
        {
            var ProductInfos = await _context.PRODUCTSTAB.FindAsync(id);
            if (ProductInfos == null)
            {
                return NotFound();
            }

            return ProductInfos;
        }

        // PUT: api/ProductInfos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductInfos(long id, ProductInfos ProductInfos)
        {
            if (id != ProductInfos.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(ProductInfos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductInfosExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProductInfos
        [HttpPost]
        public async Task<ActionResult<ProductInfos>> PostProductInfos(ProductInfos ProductInfos)
        {
            // If you need to manually control the ProductId (e.g., ensure it starts from 1), 
            // you can add that logic here similar to previous examples (but it's not recommended).
            if (ProductInfos.ProductId == 0) // Check if not set manually
            {
                var maxId = _context.PRODUCTSTAB.Any() ? _context.PRODUCTSTAB.Max(x => x.ProductId) : 0;
                ProductInfos.ProductId = maxId + 1;
            }

            // Add the ProductInfos object to the PRODUCTSTAB table
            _context.PRODUCTSTAB.Add(ProductInfos);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Product information registered successfully",
                Data = new
                {
                    ProductId = ProductInfos.ProductId,
                    ProductName = ProductInfos.ProductName
                }
            });

        }

        // DELETE: api/ProductInfos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductInfos(long id)
        {
            var ProductInfos = await _context.PRODUCTSTAB.FindAsync(id);
            if (ProductInfos == null)
            {
                return NotFound();
            }

            _context.PRODUCTSTAB.Remove(ProductInfos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/ProductInfos/Branches/{branchcode}
        [HttpGet("Branches/{branchcode}")]
        public async Task<ActionResult<object>> GetBranchesData(string branchcode)
        {
            var ProductInfos = await _context.PRODUCTSTAB.Where(x => x.BranchCode == branchcode && x.ActiveYN == "Y").ToListAsync();
            if (ProductInfos == null)
            {
                return NotFound(new
                {
                    Message = $"No records found !",
                    Data = ""
                });
            }

            return Ok(new
            {
                Message = $"Product Information retrieved successfully",
                Data = ProductInfos
            });
        }

        // PUT: api/ProductInfos/Distributor/Mark
        [HttpPut("Distributor/Mark")]
        public async Task<IActionResult> MarkDistributor([FromBody] DistributorModel request)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Invalid data. Please check the provided information.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            // Find the product by ProductId
            var productInfo = await _context.PRODUCTSTAB.FindAsync(request.ProductId);
            if (productInfo == null)
            {
                return NotFound(new { Message = "Product information not found. Please verify the provided ProductId" });
            }

            // Update the owner information fields
            productInfo.DistributorYN = request.DistributorYN;

            // Mark only the updated fields as modified, without calling UpdateRange()
            _context.PRODUCTSTAB.Update(productInfo);

            // Save changes and handle any database update errors
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while updating the owner information. Please try again." });
            }

            // Return successful response
            return Ok(new
            {
                Message = $"{productInfo.ProductName} marked distributor successfully.",
                Data = new
                {
                    ProductId = productInfo.ProductId,
                    ProductName = productInfo.ProductName
                }
            });
        }

        // GET: api/ProductInfos/Distributor/{branchcode}
        [HttpGet("Distributor/{branchcode}")]
        public async Task<ActionResult<object>> GetDistributorsData(string branchcode)
        {
            var ProductInfos = await _context.PRODUCTSTAB.Where(x => x.BranchCode == branchcode && x.DistributorYN == "Y" && x.ActiveYN == "Y").ToListAsync();
            if (ProductInfos == null)
            {
                return NotFound(new
                {
                    Message = $"No records found !",
                    Data = ""
                });
            }

            return Ok(new
            {
                Message = $"Distributor Information retrieved successfully",
                Data = ProductInfos
            });
        }


        private bool ProductInfosExists(long id)
        {
            return _context.PRODUCTSTAB.Any(e => e.ProductId == id);
        }

    }
}
