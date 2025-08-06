using LPGxWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchInfosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BranchInfosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BranchInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BranchInfos>>> GetBRANCHINFO()
        {
            return await _context.BRANCHINFO.ToListAsync();
        }

        // GET: api/BranchInfos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BranchInfos>> GetBranchInfos(string id)
        {
            var BranchInfos = await _context.BRANCHINFO.FindAsync(id);
            if (BranchInfos == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid Branch Information"
                });
            }

            return BranchInfos;
        }

        // PUT: api/BranchInfos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBranchInfos(string id, BranchInfos BranchInfos)
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

            if (id != BranchInfos.BRANCHCODE)
            {
                return BadRequest(new
                {
                    Message = $"Branch Information not Mathched"
                });
            }

            _context.Entry(BranchInfos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BranchInfosExists(id))
                {
                    return NotFound(new
                    {
                        Message = $"Invalid Branch Information"
                    });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BranchInfos
        [HttpPost]
        public async Task<ActionResult<BranchInfos>> PostBranchInfos(BranchInfos BranchInfos)
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

            // Check user validation (SignonID must match after trimming and converting to lower case)
            var loginInfo = await _context.LOGIN
                .Where(t => t.USERCODE == BranchInfos.ENTRYUSER)
                .Select(t => new { t.USERCODE, t.SIGNONID, t.USERLEVEL })
                .FirstOrDefaultAsync();

            if (loginInfo == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Find the maximum branchcode for the given PROJECTCODE and increment
            var maxbranchcode = _context.BRANCHINFO
                .Select(a => a.BRANCHCODE)
                .Max();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(maxbranchcode) && int.TryParse(maxbranchcode, out int maxNumber))
            {
                nextNumber = maxNumber + 1;
            }

            // Format the new branchcode with leading zeros (e.g., 00001)
            var NEWBRANCHCODE = nextNumber.ToString("D5"); // Adjust "D5" for the desired format

            // Check if the generated BLDINFO already exists
            if (_context.BRANCHINFO.Any(a => a.BRANCHCODE == NEWBRANCHCODE))
            {
                return BadRequest(new { Message = "An building with the generated branchcode already exists" });
            }

            BranchInfos.BRANCHCODE = NEWBRANCHCODE;
            BranchInfos.ENTRYUSER = loginInfo.USERCODE;
            BranchInfos.CREATEDATE = System.DateTime.Now;
            BranchInfos.ENABLEYN = "Y";
            _context.BRANCHINFO.Add(BranchInfos);
            await _context.SaveChangesAsync();

            // Return a custom response with a message
            return Ok(new
            {
                Message = "Branch information registered successfully",
                Data = new
                {
                    branchcode = NEWBRANCHCODE,
                    BLDNAME = BranchInfos.BLDNAME,
                    COUNTRYCODE = BranchInfos.COUNTRYCODE
                }
            });
        }

        // DELETE: api/BranchInfos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranchInfos(string id)
        {
            var BranchInfos = await _context.BRANCHINFO.FindAsync(id);
            if (BranchInfos == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid Branch Information"
                });
            }

            _context.BRANCHINFO.Remove(BranchInfos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BranchInfosExists(string id)
        {
            return _context.BRANCHINFO.Any(e => e.BRANCHCODE == id);
        }

        // GET: api/BranchInfos/Branches/{signonname}
        [HttpGet("Branches/{signonname}")]
        public async Task<ActionResult<object>> GetBranchesData(string signonname)
        {
            var trimmedSignOnName = signonname.ToLower().Trim();

            // Fetch the login information for the user
            var loginInfo = await _context.LOGIN
                .Where(t => t.SIGNONID.ToLower().Trim() == trimmedSignOnName)
                .Select(t => new
                {
                    t.USERCODE,
                    t.USERNAME,
                    t.SIGNONID,
                    t.USERLEVEL,
                    t.EMAILID
                })
                .ToListAsync();

            if (!loginInfo.Any())
            {
                return NotFound(new { Message = "User login information not found" });
            }

            var _userLevel = loginInfo.First().USERLEVEL;

            if (_userLevel == 1)
            {
                // Extract user information once to avoid repeated calls to FirstOrDefault
                var buildingList = await _context.BRANCHINFO
                    .Where(b => b.ENABLEYN == "Y")
                    .ToListAsync();

                if (!buildingList.Any())
                {
                    return NotFound(new { Message = "Branch Information not found" });
                }

                return Ok(new
                {
                    Message = "Branch Information retrieved successfully",
                    Data = buildingList
                });
            }
            else if (_userLevel == 2 || _userLevel == 5 || _userLevel == 6)
            {
                // Extract user information once to avoid repeated calls to FirstOrDefault
                var user = loginInfo.First();
                int _USERCODE = user.USERCODE;
                var userMapping = await _context.USERMAPPING
                    .Where(m => m.USERCODE == _USERCODE && m.ENABLEYN == "Y")
                    .Select(m => m.BRANCHCODE) // Select branchcode directly to avoid a ToList later
                    .Distinct()
                    .ToListAsync();

                if (!userMapping.Any())
                {
                    return NotFound(new { Message = "User not allowed for any building management" });
                }

                var buildingList = await _context.BRANCHINFO
                    .Where(b => userMapping.Contains(b.BRANCHCODE) && b.ENABLEYN == "Y")
                    .Select(t => new
                    {
                        t.BRANCHCODE,
                        t.BLDNAME,
                        t.BLDADDRESS,
                        t.BLDCITY,
                        t.COUNTRYCODE,
                        t.PROJECTCODE
                    })
                    .ToListAsync();

                if (!buildingList.Any())
                {
                    return NotFound(new { Message = "User not allowed for any building management" });
                }

                return Ok(new
                {
                    Message = "Building Information retrieved successfully",
                    Data = buildingList
                });
            }
            else if (_userLevel == 3 || _userLevel == 4)
            {
                bool isEmail = signonname.Contains("@");

                // Query to fetch distinct branchcodes for apartments by either email or contact number
                var branchcodes = await _context.SALESMANTAB
                    .Where(a => a.ActiveYN == "Y")
                    .Select(a => a.BranchCode)
                    .Distinct()
                    .ToListAsync();

                if (!branchcodes.Any())
                {
                    return NotFound(new { Message = "No profiles found for the provided sign-in ID" });
                }

                // Fetch distinct PROJECTCODEs for the buildings associated with the retrieved branchcodes
                var buildingList = await _context.BRANCHINFO
                    .Where(b => branchcodes.Contains(b.BRANCHCODE) && b.ENABLEYN == "Y")
                    .Select(t => new
                    {
                        t.BRANCHCODE,
                        t.BLDNAME,
                        t.BLDADDRESS,
                        t.BLDCITY,
                        t.COUNTRYCODE,
                        t.PROJECTCODE
                    })
                    .ToListAsync();

                if (!buildingList.Any())
                {
                    return NotFound(new { Message = "User not allowed for any branch management" });
                }

                return Ok(new
                {
                    Message = "Branch Information retrieved successfully",
                    Data = buildingList
                });
            }

            return BadRequest(new
            {
                Message = $"Invalid request for fetching user profile"
            });
        }

        // GET: api/BranchInfos/Branches/projects/{projectCode}
        [HttpGet("Branches/projects/{projectCode}")]
        public async Task<ActionResult<object>> GetBuildingsDataBybranchcode(string projectCode)
        {
            // Extract user information once to avoid repeated calls to FirstOrDefault
            var buildingList = await _context.BRANCHINFO
                .Where(b => b.PROJECTCODE == projectCode)
                .ToListAsync();

            if (!buildingList.Any())
            {
                return NotFound(new { Message = "Branch Information not found" });
            }

            return Ok(new
            {
                Message = "Branch Information retrieved successfully",
                Data = buildingList
            });

        }

    }
}
