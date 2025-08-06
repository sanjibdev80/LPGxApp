using LPGxWebApi.Model;
using LPGxWebApi.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IVACInfosController : ControllerBase
    {
        private readonly AppDbContext _context; // Replace with your actual DbContext class name

        public IVACInfosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/IVACInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IVACInfos>>> GetIVACInfos()
        {
            return await _context.IVACINFO.ToListAsync();
        }

        // GET: api/IVACInfos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IVACInfos>> GetIVACInfos(int id)
        {
            var fileInfo = await _context.IVACINFO.FindAsync(id);

            if (fileInfo == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid User file"
                });
            }

            return fileInfo;
        }

		// GET: api/IVACInfos/webno/{WEBNO}
		[HttpGet("webno/{WEBNO}")]
		public async Task<ActionResult<IVACInfos>> GetIVACInfosByWebNo(string WEBNO)
		{
			var fileInfo = await _context.IVACINFO
				.FirstOrDefaultAsync(x => x.WEBNO == WEBNO);

			if (fileInfo == null)
			{
                return Ok(new
                {
                    Message = "Web File Number not found",
                    Data = "",
                });
			}
            else
            {
				return Ok(new
				{
					Message = "Web File Number matched",
					Data = fileInfo,
				});
			}
		}

		// POST: api/IVACInfos
		[HttpPost]
        public async Task<ActionResult<IVACInfos>> PostIVACInfo([FromBody] IVACInfos fileInfo)
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

            fileInfo.REMARKS = "File Initiated";

			// Encrypt PIN if provided
			var plainPass = "";
			if (!string.IsNullOrEmpty(fileInfo.PIN))
			{
				plainPass = fileInfo.PIN;
				var crypto = new Crypto(EncryptionSettings.Key);
				fileInfo.PIN = crypto.Encrypt(fileInfo.PIN);
			}

            // Encrypt EmailPIN if provided
            var plainEPass = "";
            if (!string.IsNullOrEmpty(fileInfo.EMAILPIN))
            {
                plainEPass = fileInfo.EMAILPIN;
                var crypto = new Crypto(EncryptionSettings.Key);
                fileInfo.EMAILPIN = crypto.Encrypt(fileInfo.EMAILPIN);
            }

            // Check if the fileInfo already exists
            if (_context.IVACINFO.Any(x => x.ID == fileInfo.ID))
			{
				return Conflict(new
				{
					Message = $"fileInfo with ID {fileInfo.ID} already exists."
				});
			}

			try
            {
                // Add the fileInfo to the database
                _context.IVACINFO.Add(fileInfo);
                await _context.SaveChangesAsync();

                // Return 200
				return StatusCode(200, new { Message = "Registration complete", Details = fileInfo.ID });
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

        // POST: api/IVACInfos/ChangeRemarks
        [HttpPost("ChangeRemarks")]
        public async Task<IActionResult> ChangeRemarks([FromBody] IVACInfos input)
        {
            if (string.IsNullOrEmpty(input.WEBNO) || string.IsNullOrEmpty(input.REMARKS))
            {
                return BadRequest(new { Message = "WEBFILENO and REMARKS are required." });
            }

            try
            {
                var record = await _context.IVACINFO.FirstOrDefaultAsync(x => x.WEBNO == input.WEBNO);
                if (record == null)
                {
                    return NotFound(new { Message = "Record not found for given WEBFILENO." });
                }

                record.REMARKS = input.REMARKS;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Remarks updated successfully", Details = input.WEBNO });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }


        // PUT: api/IVACInfos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutfileInfo(int id, IVACInfos fileInfo)
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

            if (id != fileInfo.ID)
            {
                return BadRequest(new
                {
                    Message = $"Id not Mathched"
                });
            }

            _context.Entry(fileInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!fileInfoExists(id))
                {
                    return NotFound(new
                    {
                        Message = $"Invalid Id"
                    });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/IVACInfos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletefileInfo(int id)
        {
            var fileInfo = await _context.IVACINFO.FindAsync(id);
            if (fileInfo == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid User file"
                });
            }

            _context.IVACINFO.Remove(fileInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool fileInfoExists(int id)
        {
            return _context.IVACINFO.Any(e => e.ID == id);
        }
    }
}
