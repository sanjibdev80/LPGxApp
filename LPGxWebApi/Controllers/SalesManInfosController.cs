using LPGxWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesManInfosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesManInfosController(AppDbContext context)
        {
            _context = context;
        }


        // GET: api/SalesManInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesManInfos>>> GetSalesManInfos()
        {
            return await _context.SALESMANTAB.ToListAsync();
        }

        // GET: api/SalesManInfos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesManInfos>> GetSalesManInfos(string id)
        {
            var SalesManInfos = await _context.SALESMANTAB.FindAsync(id);
            if (SalesManInfos == null)
            {
                return NotFound();
            }

            return SalesManInfos;
        }

        // PUT: api/SalesManInfos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSalesManInfos(long id, SalesManInfos SalesManInfos)
        {
            if (id != SalesManInfos.PersonId)
            {
                return BadRequest();
            }

            _context.Entry(SalesManInfos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalesManInfosExists(id))
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

        // POST: api/SalesManInfos
        [HttpPost]
        public async Task<ActionResult<SalesManInfos>> PostSalesManInfos(SalesManInfos SalesManInfos)
        {
            // If you need to manually control the PersonId (e.g., ensure it starts from 1), 
            // you can add that logic here similar to previous examples (but it's not recommended).
            if (SalesManInfos.PersonId == 0) // Check if not set manually
            {
                var maxId = _context.SALESMANTAB.Any() ? _context.SALESMANTAB.Max(x => x.PersonId) : 0;
                SalesManInfos.PersonId = maxId + 1;
            }

            _context.SALESMANTAB.Add(SalesManInfos);  // Add the SalesManInfos object to the SALESMANTAB table
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Salesman information registered successfully",
                Data = new
                {
                    PersonId = SalesManInfos.PersonId,
                    PersonName = SalesManInfos.PersonName
                }
            });

        }

        // DELETE: api/SalesManInfos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalesManInfos(long id)
        {
            var SalesManInfos = await _context.SALESMANTAB.FindAsync(id);
            if (SalesManInfos == null)
            {
                return NotFound();
            }

            _context.SALESMANTAB.Remove(SalesManInfos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/SalesManInfos/Branches/{branchcode}
        [HttpGet("Branches/{branchcode}")]
        public async Task<ActionResult<object>> GetBranchesData(string branchcode)
        {
            var SalesManInfos = await _context.SALESMANTAB.Where(x => x.BranchCode == branchcode && x.ActiveYN == "Y").ToListAsync();
            if (SalesManInfos == null)
            {
                return NotFound(new
                {
                    Message = $"No records found !",
                    Data = ""
                });
            }

            return Ok(new
            {
                Message = $"Salesmans Information retrieved successfully",
                Data = SalesManInfos
            });
        }


        private bool SalesManInfosExists(long id)
        {
            return _context.SALESMANTAB.Any(e => e.PersonId == id);
        }

    }
}
