using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using LPGxWebApi.Model;
using LPGxWebApi.ViewModel;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectInfosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectInfosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ProjectInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectInfos>>> GetProjectInfos()
        {
            return await _context.PROJECTINFO.ToListAsync();
        }

        // GET: api/ProjectInfos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectInfos>> GetProjectInfo(string id)
        {
            var projectInfo = await _context.PROJECTINFO.FindAsync(id);

            if (projectInfo == null)
            {
                return NotFound(new { Message = "Project not found" });
            }

            return projectInfo;
        }

        // POST: api/ProjectInfos
        [HttpPost]
        public async Task<ActionResult<ProjectInfos>> PostProjectInfo(ProjectInfos projectInfo)
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

            if (ModelState.IsValid)
            {
                // Check user validation (SignonID must match after trimming and converting to lower case)
                var loginInfo = await _context.LOGIN
                    .Where(t => t.USERCODE == projectInfo.ENTRYUSER)
                    .Select(t => new { t.USERCODE, t.SIGNONID, t.USERLEVEL })
                    .FirstOrDefaultAsync();

                if (loginInfo == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Find the maximum BLDCODE for the given PROJECTCODE and increment
                var maxBldCode = _context.PROJECTINFO
                    .Select(a => a.PROJECTCODE)
                    .Max();

                int nextNumber = 1;
                if (!string.IsNullOrEmpty(maxBldCode) && int.TryParse(maxBldCode, out int maxNumber))
                {
                    nextNumber = maxNumber + 1;
                }

                // Format the new BLDCODE with leading zeros (e.g., 0001)
                var NEWCODE = nextNumber.ToString("D4"); // Adjust "D4" for the desired format

                // Check if the generated PROJECTCODE already exists
                if (_context.PROJECTINFO.Any(a => a.PROJECTCODE == NEWCODE))
                {
                    return BadRequest(new { Message = "An project with the generated PROJECTCODE already exists" });
                }

                projectInfo.PROJECTCODE = NEWCODE;
                projectInfo.ENTRYUSER = loginInfo.USERCODE;
                projectInfo.CREATEDATE = System.DateTime.Now;
                projectInfo.ENABLEYN = "Y";
                _context.PROJECTINFO.Add(projectInfo);
                await _context.SaveChangesAsync();

                // Return a custom response with a message
                return Ok(new
                {
                    Message = "Project information register successfully",
                    Data = new
                    {
                        PROJECTCODE = NEWCODE,
                        PROJECTNAME = projectInfo.PROJECTNAME,
                        COUNTRYCODE = projectInfo.COUNTRYCODE
                    }
                });
            }

            return BadRequest(new { Message = "This is not valid request. (ModelState failed)" });
        }

        // PUT: api/ProjectInfos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProjectInfo(string id, ProjectInfos projectInfo)
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

            if (id != projectInfo.PROJECTCODE)
            {
                return BadRequest(new { Message = "Project code mismatch." });
            }

            _context.Entry(projectInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectInfoExists(id))
                {
                    return NotFound(new { Message = "Project not found" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ProjectInfos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectInfo(string id)
        {
            var projectInfo = await _context.PROJECTINFO.FindAsync(id);
            if (projectInfo == null)
            {
                return NotFound(new { Message = "Project not found" });
            }

            _context.PROJECTINFO.Remove(projectInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper Method to check if a project exists
        private bool ProjectInfoExists(string id)
        {
            return _context.PROJECTINFO.Any(e => e.PROJECTCODE == id);
        }

        // GET: api/ProjectInfos/Projects/{signonname}
        [HttpGet("Projects/{signonname}")]
        public async Task<ActionResult<object>> GetProjectData(string signonname)
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
            //1   Super Admin
            //2   Supervisor
            //3   Owner
            //4   Tenant

            if (_userLevel == 1)
            {
                // Extract user information once to avoid repeated calls to FirstOrDefault
                var user = loginInfo.First();
                var projectList = await _context.PROJECTINFO
                    .Where(b => b.ENABLEYN == "Y")
                    .ToListAsync();

                if (!projectList.Any())
                {
                    return NotFound(new { Message = "Project Information not found" });
                }

                return Ok(new
                {
                    Message = "Project Information retrieved successfully",
                    Data = projectList
                });
            }
            else if (_userLevel == 2 || _userLevel == 5 || _userLevel == 6)
            {
                // Extract user information once to avoid repeated calls to FirstOrDefault
                var user = loginInfo.First();
                int _USERCODE = user.USERCODE;
                var userMapping = await _context.USERMAPPING
                    .Where(m => m.USERCODE == _USERCODE && m.ENABLEYN == "Y")
                    .Select(m => m.PROJECTCODE) // Select BLDCODE directly to avoid a ToList later
                    .Distinct()
                    .ToListAsync();

                if (!userMapping.Any())
                {
                    return NotFound(new { Message = "User not allowed for any project management" });
                }

                var projectList = await _context.PROJECTINFO
                    .Where(b => userMapping.Contains(b.PROJECTCODE) && b.ENABLEYN == "Y")
                    .ToListAsync();

                if (!projectList.Any())
                {
                    return NotFound(new { Message = "Project Information not found" });
                }

                return Ok(new
                {
                    Message = "Project Information retrieved successfully",
                    Data = projectList
                });
            }
            else if (_userLevel == 3 || _userLevel == 4)
            {
                bool isEmail = signonname.Contains("@");

                // Query to fetch distinct BLDCODEs for apartments by either email or contact number
                var bldCodes = await _context.SALESMANTAB
                    .Where(a => a.ActiveYN == "Y")
                    .Select(a => a.BranchCode)
                    .Distinct()
                    .ToListAsync();

                if (!bldCodes.Any())
                {
                    return NotFound(new { Message = "No profiles found for the provided sign-in ID" });
                }

                // Fetch distinct PROJECTCODEs for the buildings associated with the retrieved BLDCODEs
                var projectCodes = await _context.BRANCHINFO
                    .Where(b => bldCodes.Contains(b.BRANCHCODE) && b.ENABLEYN == "Y")
                    .Select(b => b.PROJECTCODE)
                    .Distinct()
                    .ToListAsync();

                if (!projectCodes.Any())
                {
                    return NotFound(new { Message = "User not allowed for any branch management" });
                }

                // Retrieve project information based on the PROJECTCODEs
                var projectList = await _context.PROJECTINFO
                    .Where(p => projectCodes.Contains(p.PROJECTCODE) && p.ENABLEYN == "Y")
                    .ToListAsync();

                if (!projectList.Any())
                {
                    return NotFound(new { Message = "Project Information not found" });
                }

                return Ok(new
                {
                    Message = "Project Information retrieved successfully",
                    Data = projectList
                });

            }

            return BadRequest(new
            {
                Message = $"Invalid request for fetching user profile"
            });
        }

    }
}
