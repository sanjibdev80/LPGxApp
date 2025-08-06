using LPGxWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserMappingController : ControllerBase
    {
        private readonly AppDbContext _context; // Replace with your actual DbContext class name
        List<UserMapping> mappings = new List<UserMapping>();

        public UserMappingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserMapping
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserMappingDto>>> GetUserMapping()
        {
            var query = from um in _context.USERMAPPING
                        join login in _context.LOGIN on um.USERCODE equals login.USERCODE into loginGroup
                        from lg in loginGroup.DefaultIfEmpty()
                        join bldInfo in _context.BRANCHINFO on um.BRANCHCODE equals bldInfo.BRANCHCODE into bldGroup
                        from bld in bldGroup.DefaultIfEmpty()
                        join projInfo in _context.PROJECTINFO on bld.PROJECTCODE equals projInfo.PROJECTCODE into projGroup
                        from proj in projGroup.DefaultIfEmpty()
                        select new UserMappingDto
                        {
                            MAPPINGID = um.MAPPINGID,  // Correct: keep MAPPINGID as int if it's an integer in the DB
                            USERCODE = um.USERCODE,    // Correct: keep USERCODE as string if it's a string in the DB
                            USERNAME = lg.USERNAME ?? string.Empty,  // Nullable string handled
                            USERLEVEL = lg.USERLEVEL,  // Nullable int handled
                            SIGNONID = lg.SIGNONID ?? string.Empty,  // Nullable string handled
                            EMAILID = lg.EMAILID ?? string.Empty,  // Nullable string handled
                            BRANCHCODE = um.BRANCHCODE,  // Correct: keep branchcode as string if it's a string in the DB
                            BRANCHNAME = bld.BLDNAME ?? string.Empty,  // Nullable string handled
                            PROJECTCODE = um.PROJECTCODE,  // Correct: keep PROJECTCODE as string if it's a string in the DB
                            PROJECTNAME = proj.PROJECTNAME ?? string.Empty,  // Nullable string handled
                            ENTRYUSER = um.ENTRYUSER,  // Correct: keep ENTRYUSER as string
                            CREATEDATE = um.CREATEDATE ?? DateTime.MinValue,  // Nullable DateTime handled
                            ENABLEYN = um.ENABLEYN  // Correct: keep ENABLEYN as string
                        };

            return await query.ToListAsync();
        }

        // DTO class to hold the result data (structure based on the SQL output)
        public class UserMappingDto
        {
            public int MAPPINGID { get; set; }  // Correct: MAPPINGID is an integer
            public int USERCODE { get; set; }  // Correct: USERCODE is a string
            public string? USERNAME { get; set; }  // Nullable string (for USERNAME from LOGIN)
            public int? USERLEVEL { get; set; }  // Nullable string (for USERNAME from LOGIN)
            public string? SIGNONID { get; set; }  // Nullable string (for SIGNONID from LOGIN)
            public string? EMAILID { get; set; }  // Nullable string (for EMAILID from LOGIN)
            public string BRANCHCODE { get; set; }  // Correct: BRANCHCODE is a string
            public string? BRANCHNAME { get; set; }  // Nullable string (for BLDNAME from BLDINFO)
            public string PROJECTCODE { get; set; }  // Correct: PROJECTCODE is a string
            public string? PROJECTNAME { get; set; }  // Nullable string (for PROJECTNAME from PROJECTINFO)
            public int? ENTRYUSER { get; set; }  // Correct: ENTRYUSER is a string
            public DateTime? CREATEDATE { get; set; }  // Correct: DateTime, but nullable DateTime handled in LINQ
            public string ENABLEYN { get; set; }  // Correct: ENABLEYN is a string
        }

        // GET: api/UserMapping/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserMapping>> GeUserMapping(int id)
        {
            var userMapping = await _context.USERMAPPING.FindAsync(id);

            if (userMapping == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid User Mapping"
                });
            }

            return userMapping;
        }

        // GET: api/UserMapping/branch/{branchcode}
        [HttpGet("branch/{branchcode}")]
        public async Task<ActionResult<IEnumerable<UserMappingDto>>> GetUserMappingBybranchcode(string branchcode)
        {
            var query = from um in _context.USERMAPPING
                        join login in _context.LOGIN on um.USERCODE equals login.USERCODE into loginGroup
                        from lg in loginGroup.DefaultIfEmpty()
                        join bldInfo in _context.BRANCHINFO on um.BRANCHCODE equals bldInfo.BRANCHCODE into bldGroup
                        from bld in bldGroup.DefaultIfEmpty()
                        join projInfo in _context.PROJECTINFO on bld.PROJECTCODE equals projInfo.PROJECTCODE into projGroup
                        from proj in projGroup.DefaultIfEmpty()
                        select new UserMappingDto
                        {
                            MAPPINGID = um.MAPPINGID,  // Correct: keep MAPPINGID as int if it's an integer in the DB
                            USERCODE = um.USERCODE,    // Correct: keep USERCODE as string if it's a string in the DB
                            USERNAME = lg.USERNAME ?? string.Empty,  // Nullable string handled
                            USERLEVEL = lg.USERLEVEL,  // Nullable int handled
                            SIGNONID = lg.SIGNONID ?? string.Empty,  // Nullable string handled
                            EMAILID = lg.EMAILID ?? string.Empty,  // Nullable string handled
                            BRANCHCODE = um.BRANCHCODE,  // Correct: keep branchcode as string if it's a string in the DB
                            BRANCHNAME = bld.BLDNAME ?? string.Empty,  // Nullable string handled
                            PROJECTCODE = um.PROJECTCODE,  // Correct: keep PROJECTCODE as string if it's a string in the DB
                            PROJECTNAME = proj.PROJECTNAME ?? string.Empty,  // Nullable string handled
                            ENTRYUSER = um.ENTRYUSER,  // Correct: keep ENTRYUSER as string
                            CREATEDATE = um.CREATEDATE ?? DateTime.MinValue,  // Nullable DateTime handled
                            ENABLEYN = um.ENABLEYN  // Correct: keep ENABLEYN as string
                        };

            return await query.Where(x => x.BRANCHCODE == branchcode).ToListAsync();
        }

        // POST: api/UserMapping/registration
        [HttpPost("registration")]
        public async Task<ActionResult<UserMapping>> PostUserMapping([FromBody] UserMappingRegister request)
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

            // Validate required fields
            if (request.USERCODE <= 0 || request.branchcode.Length == 0 || request.ENTRYUSER <= 0)
            {
                return BadRequest(new { Message = "Required information not found" });
            }

            // Check if the user exists and retrieve their details
            var loginCheck = await _context.LOGIN
                .Where(t => t.USERCODE == request.ENTRYUSER)
                .Select(t => new { t.USERCODE, t.SIGNONID, t.USERLEVEL })
                .FirstOrDefaultAsync();

            if (loginCheck == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Retrieve the branch information based on multiple branchcodes
            var branchInfos = await _context.BRANCHINFO
                .Where(branch => request.branchcode.Contains(branch.BRANCHCODE))
                .ToListAsync();

            // Check if any branch code is not found
            var notFoundCodes = request.branchcode.Except(branchInfos.Select(branch => branch.BRANCHCODE)).ToArray();
            if (notFoundCodes.Any())
            {
                return NotFound(new { Message = $"branch information not found for the following branchcode(s): {string.Join(", ", notFoundCodes)}" });
            }


            // Retrieve existing mappings in one batch
            var existingMappings = await _context.USERMAPPING
                .Where(m => m.USERCODE == request.USERCODE && request.branchcode.Contains(m.BRANCHCODE))
                .Select(m => m.BRANCHCODE)
                .ToListAsync();

            // In-memory filtering: find which branchcodes are already registered and which are new
            var newMappings = new List<UserMapping>();
            var existingbranchcodes = new List<string>();

            foreach (var bldItem in branchInfos)
            {
                if (existingMappings.Contains(bldItem.BRANCHCODE))
                {
                    // If the branchcode already exists, add it to the existing list
                    existingbranchcodes.Add(bldItem.BRANCHCODE);
                }
                else
                {
                    // Otherwise, create a new mapping to be inserted
                    newMappings.Add(new UserMapping
                    {
                        MAPPINGID = 0,  // Assuming MAPPINGID is auto-generated
                        USERCODE = request.USERCODE,
                        BRANCHCODE = bldItem.BRANCHCODE,
                        PROJECTCODE = bldItem.PROJECTCODE,
                        ENTRYUSER = request.ENTRYUSER,
                        CREATEDATE = DateTime.Now,
                        ENABLEYN = "Y"
                    });
                }
            }

            // Insert new mappings in a batch
            if (newMappings.Any())
            {
                try
                {
                    await _context.USERMAPPING.AddRangeAsync(newMappings);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while saving the mapping information. Please try again." });
                }
            }

            // Return the result: which branchcodes were inserted and which were already registered
            return Ok(new
            {
                Message = "User mapping operation completed.",
                Data = new
                {
                    InsertedbranchcodeS = newMappings.Select(mapping => mapping.BRANCHCODE),
                    ExistingbranchcodeS = existingbranchcodes
                }
            });
        }

        public class UserMappingRegister
        {
            public int USERCODE { get; set; }
            public string[] branchcode { get; set; }
            public int? ENTRYUSER { get; set; }

        }

        // PUT: api/UserMapping/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserMapping(int id, UserMapping userMapping)
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

            if (id != userMapping.MAPPINGID)
            {
                return BadRequest(new
                {
                    Message = $"User Mapping not Mathched"
                });
            }

            _context.Entry(userMapping).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserMappingExists(id))
                {
                    return NotFound(new
                    {
                        Message = $"Invalid User Mapping"
                    });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/UserMapping/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserMapping(int id)
        {
            var userMapping = await _context.USERMAPPING.FindAsync(id);
            if (userMapping == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid User Mapping"
                });
            }

            _context.USERMAPPING.Remove(userMapping);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserMappingExists(int id)
        {
            return _context.USERMAPPING.Any(e => e.MAPPINGID == id);
        }
    }
}
