using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LPGxWebApi.Model;
using LPGxWebApi.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginInfosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoginInfosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LoginInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoginInfos>>> GetLoginInfos()
        {
            return await _context.LOGIN.ToListAsync();
        }

        // GET: api/LoginInfos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LoginInfos>> GetLoginInfo(int id)
        {
            var loginInfo = await _context.LOGIN.FindAsync(id);

            if (loginInfo == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid Login Id"
                });
            }

            return loginInfo;
        }

        // GET: api/LoginInfos/Login/branch/{branchcode}
        [HttpGet("Login/branch/{branchcode}")]
        public async Task<ActionResult<object>> GetLoginInfoBybranchcode(string branchcode)
        {
            // Fetch user mappings for the provided branchcode
            var userMappings = _context.USERMAPPING.Where(u => u.BRANCHCODE == branchcode).ToList();

            // If no user mappings are found, return NotFound
            if (userMappings == null || !userMappings.Any())
            {
                return NotFound(new
                {
                    Message = $"No user mappings found for building code {branchcode}"
                });
            }

            // Retrieve login info based on the user codes from user mappings
            var loginInfo = await _context.LOGIN
                .Where(l => userMappings.Select(um => um.USERCODE).Contains(l.USERCODE))
                .ToListAsync(); // Assuming LOGIN table has a field USERCODE that matches

            // If login info is not found, return NotFound
            if (loginInfo == null || !loginInfo.Any())
            {
                return NotFound(new
                {
                    Message = $"No login info found for user(s) associated with building code {branchcode}"
                });
            }

            // Return the login info
            return Ok(loginInfo);
        }

        // GET: api/LoginInfos/Login/signon/{signonname}
        [HttpGet("Login/signon/{signonname}")]
        public async Task<ActionResult<object>> GetLoginInfoBySignOn(string signonname)
        {
            var trimmedSignOnName = signonname.ToLower().Trim();

            // Fetch login information for the provided signonname
            var loginInfo = await _context.LOGIN
                .Where(t => t.SIGNONID.ToLower().Trim() == trimmedSignOnName)
                .Select(t => new
                {
                    t.USERNAME,
                    t.SIGNONID,
                    t.EMAILID,
                    t.TWOFA
                })
                .SingleOrDefaultAsync();  // Use SingleOrDefault instead of ToList() and checking count

            // Return 404 if loginInfo is not found
            if (loginInfo == null)
            {
                return NotFound(new { Message = "Invalid Login Id" });
            }

            // Generate a 4-digit OTP
            var random = new Random();
            var otpCode = random.Next(1000, 9999).ToString();

            // Create a new OTP entry
            var generateOTP = new GenerateOTP
            {
                SIGNONID = loginInfo.SIGNONID,
                CREATEDATE = DateTime.Now,
                EXPIRY = DateTime.UtcNow.AddMinutes(60), // OTP expiry after 60 minutes
                OTP = otpCode,
                VERIFYOTP = false
            };

            // Save OTP to the database
            _context.GENERATEOTP.Add(generateOTP);
            await _context.SaveChangesAsync();

            var newReqId = generateOTP.REQID;  // Assuming 'ReqId' is the identity column in GenerateOTP

            // Flag for whether the OTP was successfully sent
            bool sendemail = false;

            // Only send OTP if 2FA is enabled
            if (loginInfo.TWOFA)
            {
                var commonFunctionWithParams = new CommonFunction(
                    OTPMedia: "E",  // "E" stands for Email
                    ToEmailId: loginInfo.EMAILID.ToLower(),
                    plainTxtOTP: otpCode,
                    UserName: loginInfo.USERNAME,
                    ref sendemail
                );
            }
            else
            {
                sendemail = true; // If no 2FA, assume OTP is successfully sent
            }

            // Return appropriate response based on OTP send status
            if (sendemail)
            {
                return Ok(new
                {
                    Message = "OTP generated. You can check your registered email.",
                    ReqId = newReqId,
                    TwoFA = loginInfo.TWOFA
                });
            }
            else
            {
                return BadRequest(new
                {
                    Message = "Failed to send OTP to registered email due to an authentication problem."
                });
            }
        }


        // PUT: api/LoginInfos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoginInfo(int id, LoginInfos loginInfo)
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

            if (id != loginInfo.USERCODE)
            {
                return BadRequest(new
                {
                    Message = $"User code not matched"
                });
            }

            _context.Entry(loginInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoginInfoExists(id))
                {
                    return NotFound(new
                    {
                        Message = $"Invalid Login Id"
                    });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/LoginInfos
        [HttpPost]
        public async Task<ActionResult<LoginInfos>> PostLoginInfo(LoginInfos loginInfo)
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

            // Check if the user exists and get the SIGNONID and USERLEVEL
            var loginCheck = await _context.LOGIN
                .Where(t => t.USERCODE == loginInfo.ENTRYUSER)
                .Select(t => new { t.USERCODE, t.SIGNONID, t.USERLEVEL })
                .FirstOrDefaultAsync();

            // Check if the provided SIGNONID matches the one in the database
            var existsCheck = await _context.LOGIN
                .Where(t => t.SIGNONID.Trim() == loginInfo.SIGNONID.Trim())
                .Select(t => new { t.USERCODE, t.SIGNONID, t.USERLEVEL })
                .FirstOrDefaultAsync();

            if (existsCheck != null)
            {
                return BadRequest(new { Message = "Signonname is already exists" });
            }

            // Set the new values for the loginInfo object
            loginInfo.USERCODE = 0;
            loginInfo.CREATEDATE = DateTime.Now;
            loginInfo.ENABLEYN = "Y";

            var plainPass = "";

            // Encrypt password if provided
            if (!string.IsNullOrEmpty(loginInfo.PASSWORD))
            {
                plainPass = loginInfo.PASSWORD;
                var crypto = new Crypto(EncryptionSettings.Key);
                loginInfo.PASSWORD = crypto.Encrypt(loginInfo.PASSWORD);
            }

            // Add the new login info to the database and save changes
            _context.LOGIN.Add(loginInfo);
            await _context.SaveChangesAsync();

            // Flag for whether the OTP was successfully sent
            bool sendemail = false;
            var commonFunctionWithParams = new CommonFunction(
                OTPMedia: "E",  // "E" stands for Email
                ToEmailId: loginInfo.EMAILID?.ToLower(),
                passKey: plainPass,
                UserName: loginInfo.USERNAME,
                contactno: loginInfo.SIGNONID,
                urlLink: UrlLinks.webLink,
                ref sendemail
            );

            // Return success response with relevant data
            return Ok(new
            {
                Message = $"{loginInfo.USERNAME} registered successfully",
                Data = new
                {
                    USERCODE = loginInfo.USERCODE,
                    USERNAME = loginInfo.USERNAME,
                    SIGNONID = loginInfo.SIGNONID
                }
            });
        }

        // DELETE: api/LoginInfos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoginInfo(int id)
        {
            var loginInfo = await _context.LOGIN.FindAsync(id);
            if (loginInfo == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid Login Id"
                });
            }

            _context.LOGIN.Remove(loginInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoginInfoExists(int id)
        {
            return _context.LOGIN.Any(e => e.USERCODE == id);
        }

        // POST: api/LoginInfos/VerifyOtp
        [HttpPost("VerifyOtp")]
        public async Task<ActionResult<object>> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            // Validate the incoming request
            if (request == null || string.IsNullOrWhiteSpace(request.SignonName) || string.IsNullOrWhiteSpace(request.OtpCode) || request.ReqId <= 0)
            {
                return BadRequest(new { Message = "Invalid request" });
            }

            var trimmedSignOnName = request.SignonName.ToLower().Trim();
            var otpRequest = request.OtpCode.Trim();
            var reqidRequest = request.ReqId;

            // Fetch the login information for the user
            var loginInfo = await _context.LOGIN
                .Where(t => t.SIGNONID.ToLower().Trim() == trimmedSignOnName)
                .Select(t => new
                {
                    t.USERNAME,
                    t.SIGNONID,
                    t.USERLEVEL,
                    t.ENABLEYN,
                    t.PASSWORD,
                    t.EMAILID,
                    t.USERCODE,
                    t.TWOFA,
                })
                .FirstAsync();

            if (string.IsNullOrEmpty(loginInfo.SIGNONID))
            {
                return NotFound(new { Message = "User not found" });
            }

            if (loginInfo.ENABLEYN == "N")
            {
                return NotFound(new { Message = "User not active" });
            }

            // Retrieve existing mappings in one batch
            var mappingBLD = await _context.USERMAPPING
                .Where(m => m.USERCODE == loginInfo.USERCODE)
                .ToListAsync();

            if (mappingBLD == null)
            {
                return NotFound(new { Message = "User mapping not found" });
            }

            // Extract branchcode values from the mapping
            var branchcodes = mappingBLD.Select(m => m.BRANCHCODE).ToList();

            // Retrieve the building information based on multiple branchcodes
            var buildingInfos = await _context.BRANCHINFO
                .Where(building => branchcodes.Contains(building.BRANCHCODE))
                .ToListAsync();

            if (!buildingInfos.Any())
            {
                return NotFound(new { Message = "Building mapping not found" });
            }

            SuccessfulLoginModel viewModel = new SuccessfulLoginModel();
            viewModel.UserName = loginInfo.USERNAME;
            viewModel.SignonName = loginInfo.SIGNONID;
            viewModel.UserLevel = loginInfo.USERLEVEL;
            viewModel.UserCode = loginInfo.USERCODE;
            viewModel.Email = loginInfo.EMAILID;
            viewModel.ProjectCode = buildingInfos.First().PROJECTCODE;
            viewModel.BranchCode = buildingInfos.First().BRANCHCODE;
            viewModel.CountryCode = buildingInfos.First().COUNTRYCODE;
            viewModel.TwoFA = loginInfo.TWOFA;

            // Fetch the OTP record for validation
            var otpRecord = await _context.GENERATEOTP
                .Where(x => x.REQID == reqidRequest)
                .FirstOrDefaultAsync(); // Use FirstOrDefaultAsync to prevent exception if not found

            if (otpRecord == null)
            {
                return BadRequest(new { Message = "OTP not found" });
            }

            // Check if the OTP is valid and not expired
            if (otpRecord.OTP == otpRequest && otpRecord.EXPIRY > DateTime.UtcNow)
            {
                // OTP is valid, you can also mark it as verified if needed
                otpRecord.VERIFYOTP = true; // Optionally mark as verified
                otpRecord.VERIFYTIME = DateTime.Now; // Optionally mark as verified time
                await _context.SaveChangesAsync(); // Save changes if marking as verified

                return Ok(new
                {
                    Message = "OTP verified successfully",
                    Data = viewModel
                });
            }
            else if (otpRecord.EXPIRY <= DateTime.UtcNow)
            {
                // OTP expired
                return BadRequest(new { Message = "OTP has expired" });
            }

            // OTP is invalid
            return BadRequest(new { Message = "Invalid OTP" });
        }

        // POST: api/LoginInfos/VerifyPassword
        [HttpPost("VerifyPassword")]
        public async Task<ActionResult<object>> VerifyPassword([FromBody] PasswordVerificationRequest request)
        {
            // Validate the incoming request
            if (request == null || string.IsNullOrWhiteSpace(request.SignonName) || string.IsNullOrWhiteSpace(request.Password) || request.ReqId <= 0)
            {
                return BadRequest(new { Message = "Invalid request" });
            }

            var trimmedSignOnName = request.SignonName.ToLower().Trim();
            var passwordRequest = request.Password.Trim();
            var reqidRequest = request.ReqId;

            var crypto = new Crypto(EncryptionSettings.Key);
            string encryptedData = crypto.Encrypt(passwordRequest);

            passwordRequest = encryptedData;

            // Fetch the login information for the user
            var loginInfo = await _context.LOGIN
                .Where(t => t.SIGNONID.ToLower().Trim() == trimmedSignOnName)
                .Select(t => new
                {
                    t.USERNAME,
                    t.SIGNONID,
                    t.USERLEVEL,
                    t.ENABLEYN,
                    t.PASSWORD,
                    t.EMAILID,
                    t.USERCODE,
                    t.TWOFA,
                })
                .FirstAsync();

            if (string.IsNullOrEmpty(loginInfo.SIGNONID))
            {
                return NotFound(new { Message = "User not found" });
            }

            if (loginInfo.ENABLEYN == "N")
            {
                return NotFound(new { Message = "User not active" });
            }

            // Retrieve existing mappings in one batch
            var mappingBLD = await _context.USERMAPPING
                .Where(m => m.USERCODE == loginInfo.USERCODE)
                .ToListAsync();

            if (mappingBLD == null)
            {
                return NotFound(new { Message = "User mapping not found" });
            }

            // Extract branchcode values from the mapping
            var branchcodes = mappingBLD.Select(m => m.BRANCHCODE).ToList();

            // Retrieve the building information based on multiple branchcodes
            var buildingInfos = await _context.BRANCHINFO
                .Where(building => branchcodes.Contains(building.BRANCHCODE))
                .ToListAsync();

            if (!buildingInfos.Any())
            {
                return NotFound(new { Message = "Branch mapping not found" });
            }

            SuccessfulLoginModel viewModel = new SuccessfulLoginModel();
            viewModel.UserName = loginInfo.USERNAME;
            viewModel.SignonName = loginInfo.SIGNONID;
            viewModel.UserLevel = loginInfo.USERLEVEL;
            viewModel.UserCode = loginInfo.USERCODE;
            viewModel.Email = loginInfo.EMAILID;
            viewModel.ProjectCode = buildingInfos.First().PROJECTCODE;
            viewModel.BranchCode = buildingInfos.First().BRANCHCODE;
            viewModel.CountryCode = buildingInfos.First().COUNTRYCODE;
            viewModel.TwoFA = loginInfo.TWOFA;

            // Fetch the Request record for validation
            var passwordRecord = await _context.GENERATEOTP
                .Where(x => x.REQID == reqidRequest)
                .FirstOrDefaultAsync(); // Use FirstOrDefaultAsync to prevent exception if not found

            if (passwordRecord == null)
            {
                return BadRequest(new { Message = "Request not found" });
            }

            // Check if the Request is valid and not expired
            if (loginInfo.PASSWORD == passwordRequest && passwordRecord.EXPIRY > DateTime.UtcNow)
            {
                // Request is valid, you can also mark it as verified if needed
                passwordRecord.VERIFYOTP = true; // Optionally mark as verified
                passwordRecord.VERIFYTIME = DateTime.Now; // Optionally mark as verified time
                await _context.SaveChangesAsync(); // Save changes if marking as verified

                return Ok(new
                {
                    Message = "Password verified successfully",
                    Data = viewModel
                });
            }
            else if (passwordRecord.EXPIRY <= DateTime.UtcNow)
            {
                // Request expired
                return BadRequest(new { Message = "Request has expired" });
            }

            // Request is invalid
            return BadRequest(new { Message = "Invalid Password" });
        }



        // Model for OTP Verification request
        public class OtpVerificationRequest
        {
            public string SignonName { get; set; }
            public string OtpCode { get; set; }
            public decimal ReqId { get; set; }

        }

        // Model for Password Verification request
        public class PasswordVerificationRequest
        {
            public string SignonName { get; set; }
            public string Password { get; set; }
            public decimal ReqId { get; set; }

        }

    }

}
