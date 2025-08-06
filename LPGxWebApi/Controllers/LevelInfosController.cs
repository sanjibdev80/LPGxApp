using LPGxWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LevelInfosController : ControllerBase
    {
        private readonly AppDbContext _context; // Replace with your actual DbContext class name

        public LevelInfosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LevelInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LevelInfos>>> GetLevelInfos()
        {
            return await _context.LEVELINFO.ToListAsync();
        }

        // GET: api/LevelInfos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LevelInfos>> GetLevelInfo(int id)
        {
            var levelInfo = await _context.LEVELINFO.FindAsync(id);

            if (levelInfo == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid User Level"
                });
            }

            return levelInfo;
        }

        // POST: api/LevelInfos
        [HttpPost]
        public async Task<ActionResult<LevelInfos>> PostLevelInfo(LevelInfos levelInfo)
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

            // Implement the max+1 logic for USERLEVEL (next available level)
            if (levelInfo.USERLEVEL == 0) // If the level is not set manually
            {
                var maxLevel = _context.LEVELINFO.Any() ? _context.LEVELINFO.Max(x => x.USERLEVEL) : 0;
                levelInfo.USERLEVEL = maxLevel + 1;  // Set USERLEVEL to the next available value
            }

            try
            {
                // Add the levelInfo to the database
                _context.LEVELINFO.Add(levelInfo);
                await _context.SaveChangesAsync();

                // Return 201 Created with the URI of the newly created LevelInfo
                return CreatedAtAction(nameof(GetLevelInfo), new { id = levelInfo.USERLEVEL }, levelInfo);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database-specific errors (e.g., constraint violations)
                return StatusCode(500, new { Message = "An error occurred while saving data.", Details = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Handle other general exceptions
                return StatusCode(500, new { Message = "An unexpected error occurred.", Details = ex.Message });
            }
        }


        // PUT: api/LevelInfos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLevelInfo(int id, LevelInfos levelInfo)
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

            if (id != levelInfo.USERLEVEL)
            {
                return BadRequest(new
                {
                    Message = $"User Level not Mathched"
                });
            }

            _context.Entry(levelInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LevelInfoExists(id))
                {
                    return NotFound(new
                    {
                        Message = $"Invalid User Level"
                    });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/LevelInfos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLevelInfo(int id)
        {
            var levelInfo = await _context.LEVELINFO.FindAsync(id);
            if (levelInfo == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid User Level"
                });
            }

            _context.LEVELINFO.Remove(levelInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LevelInfoExists(int id)
        {
            return _context.LEVELINFO.Any(e => e.USERLEVEL == id);
        }
    }
}
