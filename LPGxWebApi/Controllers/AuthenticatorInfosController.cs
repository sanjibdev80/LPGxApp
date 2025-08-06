using LPGxWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticatorInfosController : ControllerBase
    {
        private readonly AppDbContext _context; // Replace with your actual DbContext class name

        public AuthenticatorInfosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AuthenticatorInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthenticatorInfos>>> GetAuthenticatorInfos()
        {
            return await _context.AUTHENTICATORS.ToListAsync();
        }

        // GET: api/AuthenticatorInfos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthenticatorInfos>> GetAuthenticatorInfo(int id)
        {
            var authenticatorInfo = await _context.AUTHENTICATORS.FindAsync(id);

            if (authenticatorInfo == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid Authenticator Information"
                });
            }

            return authenticatorInfo;
        }

        // POST: api/AuthenticatorInfos
        [HttpPost]
        public async Task<ActionResult<AuthenticatorInfos>> PostAuthenticatorInfo(AuthenticatorInfos authenticatorInfo)
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

            _context.AUTHENTICATORS.Add(authenticatorInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuthenticatorInfo), new { id = authenticatorInfo.AUTHENTICATORID }, authenticatorInfo);
        }

        // PUT: api/AuthenticatorInfos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuthenticatorInfo(int id, AuthenticatorInfos authenticatorInfo)
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

            if (id != authenticatorInfo.AUTHENTICATORID)
            {
                return BadRequest(new
                {
                    Message = $"Authenticator Information not Mathched"
                });
            }

            _context.Entry(authenticatorInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthenticatorInfoExists(id))
                {
                    return NotFound(new
                    {
                        Message = $"Invalid Authenticator Information"
                    });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/AuthenticatorInfos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthenticatorInfo(int id)
        {
            var authenticatorInfo = await _context.AUTHENTICATORS.FindAsync(id);
            if (authenticatorInfo == null)
            {
                return NotFound(new
                {
                    Message = $"Invalid Authenticator Information"
                });
            }

            _context.AUTHENTICATORS.Remove(authenticatorInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AuthenticatorInfoExists(int id)
        {
            return _context.AUTHENTICATORS.Any(e => e.AUTHENTICATORID == id);
        }
    }
}
