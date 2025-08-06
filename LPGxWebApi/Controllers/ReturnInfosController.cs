using LPGxWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnInfosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReturnInfosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ReturnInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReturnInfos>>> GetReturnInfos()
        {
            return await _context.RETURNENTRYTAB.ToListAsync();
        }

        // GET: api/ReturnInfos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReturnInfos>> GetReturnInfos(string id)
        {
            var ReturnInfos = await _context.RETURNENTRYTAB.FindAsync(id);
            if (ReturnInfos == null)
            {
                return NotFound();
            }

            return ReturnInfos;
        }

        // PUT: api/ReturnInfos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReturnInfos(long id, ReturnInfos ReturnInfos)
        {
            if (id != ReturnInfos.PersonId)
            {
                return BadRequest();
            }

            _context.Entry(ReturnInfos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReturnInfosExists(id))
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

        // POST: api/ReturnInfos
        [HttpPost]
        public async Task<ActionResult<ReturnInfos>> PostReturnInfos(ReturnInfos ReturnInfos)
        {
            // Ensure the ReturnSaleId starts from 1 if you want to handle it manually.
            if (ReturnInfos.ReturnSaleId == 0) // Check if not set manually
            {
                var maxId = _context.RETURNENTRYTAB.Any() ? _context.RETURNENTRYTAB.Max(x => x.ReturnSaleId) : 0;
                ReturnInfos.ReturnSaleId = maxId + 1;
            }

            _context.RETURNENTRYTAB.Add(ReturnInfos);
            await _context.SaveChangesAsync();

            // Return a custom response with a message
            return Ok(new
            {
                Message = "Return Stock information register successfully",
                Data = new
                {
                    ReturnSaleId = ReturnInfos.ReturnSaleId,
                    ReturnDate = ReturnInfos.SaleDate,
                    BranchCode = ReturnInfos.BranchCode
                }
            });
        }

        // DELETE: api/ReturnInfos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReturnInfos(long id)
        {
            var ReturnInfos = await _context.RETURNENTRYTAB.FindAsync(id);
            if (ReturnInfos == null)
            {
                return NotFound();
            }

            _context.RETURNENTRYTAB.Remove(ReturnInfos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReturnInfosExists(long id)
        {
            return _context.RETURNENTRYTAB.Any(e => e.PersonId == id);
        }

        // GET: api/ReturnInfos/Branches/{branchCode}
        [HttpGet("Branches/{branchCode}")]
        public async Task<ActionResult<object>> GetBranchData(string branchCode)
        {
            // Extract user information once to avoid repeated calls to FirstOrDefault
            var salesList = await _context.RETURNENTRYTAB
                .Where(b => b.BranchCode == branchCode)
                .ToListAsync();

            if (!salesList.Any())
            {
                return NotFound(new { Message = "Return Stock Information not found" });
            }

            return Ok(new
            {
                Message = "Return Stock Information retrieved successfully",
                Data = salesList
            });
        }

        // GET: api/ReturnInfos/Date/{branchCode}/{reportDate}
        [HttpGet("Date/{branchCode}/{reportDate}")]
        public async Task<ActionResult<object>> GetBranchDataByDate(string branchCode, string reportDate)
        {
            DateTime rptDate = DateTime.Parse(reportDate);
            var salesData = await (from se in _context.RETURNENTRYTAB
                                   where se.BranchCode == branchCode && se.SaleDate.Date == rptDate.Date
                                   select new
                                   {
                                       se.PersonId,
                                       se.ProductId,
                                       se.Unit,
                                       se.CylinderExchange,
                                       se.LoanPaid,
                                       se.CylinderSalePurchase,
                                       se.Today,
                                       se.PreviousStock,
                                       se.RefilePurchase,
                                       se.PackagePurchase,
                                       se.TotalStock
                                   }).ToListAsync();

            var aggregatedData = from sd in salesData
                                 group sd by new { sd.PersonId, sd.ProductId } into g
                                 select new
                                 {
                                     g.Key.PersonId,
                                     g.Key.ProductId,
                                     NAV = g.Where(x => x.ProductId == 1).Sum(x => x.ProductId == 1 ? x.Unit : 0),
                                     DUB = g.Where(x => x.ProductId == 2).Sum(x => x.ProductId == 2 ? x.Unit : 0),
                                     UNI = g.Where(x => x.ProductId == 3).Sum(x => x.ProductId == 3 ? x.Unit : 0),
                                     OME = g.Where(x => x.ProductId == 4).Sum(x => x.ProductId == 4 ? x.Unit : 0),
                                     MAX = g.Where(x => x.ProductId == 5).Sum(x => x.ProductId == 5 ? x.Unit : 0),
                                     FRESH = g.Where(x => x.ProductId == 6).Sum(x => x.ProductId == 6 ? x.Unit : 0),
                                     KH = g.Where(x => x.ProductId == 7).Sum(x => x.ProductId == 7 ? x.Unit : 0),
                                     BM = g.Where(x => x.ProductId == 8).Sum(x => x.ProductId == 8 ? x.Unit : 0),
                                     DEL = g.Where(x => x.ProductId == 9).Sum(x => x.ProductId == 9 ? x.Unit : 0),
                                     JAM = g.Where(x => x.ProductId == 10).Sum(x => x.ProductId == 10 ? x.Unit : 0),
                                     BEX = g.Where(x => x.ProductId == 11).Sum(x => x.ProductId == 11 ? x.Unit : 0),
                                     BEN = g.Where(x => x.ProductId == 12).Sum(x => x.ProductId == 12 ? x.Unit : 0),
                                     SEN = g.Where(x => x.ProductId == 13).Sum(x => x.ProductId == 13 ? x.Unit : 0),
                                     BAS = g.Where(x => x.ProductId == 14).Sum(x => x.ProductId == 14 ? x.Unit : 0),
                                     JMI = g.Where(x => x.ProductId == 15).Sum(x => x.ProductId == 15 ? x.Unit : 0),
                                     GGAS = g.Where(x => x.ProductId == 16).Sum(x => x.ProductId == 16 ? x.Unit : 0),
                                     TMSS = g.Where(x => x.ProductId == 17).Sum(x => x.ProductId == 17 ? x.Unit : 0),
                                     AY = g.Where(x => x.ProductId == 18).Sum(x => x.ProductId == 18 ? x.Unit : 0),
                                     ETC = g.Where(x => x.ProductId == 19).Sum(x => x.ProductId == 19 ? x.Unit : 0),
                                     TOT = g.Sum(x => x.Unit),
                                     CylinderExchangeNAV = g.Sum(x => x.ProductId == 1 ? x.CylinderExchange : 0),
                                     CylinderExchangeDUB = g.Sum(x => x.ProductId == 2 ? x.CylinderExchange : 0),
                                     CylinderExchangeUNI = g.Sum(x => x.ProductId == 3 ? x.CylinderExchange : 0),
                                     CylinderExchangeOME = g.Sum(x => x.ProductId == 4 ? x.CylinderExchange : 0),
                                     CylinderExchangeMAX = g.Sum(x => x.ProductId == 5 ? x.CylinderExchange : 0),
                                     CylinderExchangeFRESH = g.Sum(x => x.ProductId == 6 ? x.CylinderExchange : 0),
                                     CylinderExchangeKH = g.Sum(x => x.ProductId == 7 ? x.CylinderExchange : 0),
                                     CylinderExchangeBM = g.Sum(x => x.ProductId == 8 ? x.CylinderExchange : 0),
                                     CylinderExchangeDEL = g.Sum(x => x.ProductId == 9 ? x.CylinderExchange : 0),
                                     CylinderExchangeJAM = g.Sum(x => x.ProductId == 10 ? x.CylinderExchange : 0),
                                     CylinderExchangeBEX = g.Sum(x => x.ProductId == 11 ? x.CylinderExchange : 0),
                                     CylinderExchangeBEN = g.Sum(x => x.ProductId == 12 ? x.CylinderExchange : 0),
                                     CylinderExchangeSEN = g.Sum(x => x.ProductId == 13 ? x.CylinderExchange : 0),
                                     CylinderExchangeBAS = g.Sum(x => x.ProductId == 14 ? x.CylinderExchange : 0),
                                     CylinderExchangeJMI = g.Sum(x => x.ProductId == 15 ? x.CylinderExchange : 0),
                                     CylinderExchangeGGAS = g.Sum(x => x.ProductId == 16 ? x.CylinderExchange : 0),
                                     CylinderExchangeTMSS = g.Sum(x => x.ProductId == 17 ? x.CylinderExchange : 0),
                                     CylinderExchangeAY = g.Sum(x => x.ProductId == 18 ? x.CylinderExchange : 0),
                                     CylinderExchangeETC = g.Sum(x => x.ProductId == 19 ? x.CylinderExchange : 0),
                                     CylinderExchangeTOT = g.Sum(x => x.CylinderExchange),
                                     LoanPaidNAV = g.Sum(x => x.ProductId == 1 ? x.LoanPaid : 0),
                                     LoanPaidDUB = g.Sum(x => x.ProductId == 2 ? x.LoanPaid : 0),
                                     LoanPaidUNI = g.Sum(x => x.ProductId == 3 ? x.LoanPaid : 0),
                                     LoanPaidOME = g.Sum(x => x.ProductId == 4 ? x.LoanPaid : 0),
                                     LoanPaidMAX = g.Sum(x => x.ProductId == 5 ? x.LoanPaid : 0),
                                     LoanPaidFRESH = g.Sum(x => x.ProductId == 6 ? x.LoanPaid : 0),
                                     LoanPaidKH = g.Sum(x => x.ProductId == 7 ? x.LoanPaid : 0),
                                     LoanPaidBM = g.Sum(x => x.ProductId == 8 ? x.LoanPaid : 0),
                                     LoanPaidDEL = g.Sum(x => x.ProductId == 9 ? x.LoanPaid : 0),
                                     LoanPaidJAM = g.Sum(x => x.ProductId == 10 ? x.LoanPaid : 0),
                                     LoanPaidBEX = g.Sum(x => x.ProductId == 11 ? x.LoanPaid : 0),
                                     LoanPaidBEN = g.Sum(x => x.ProductId == 12 ? x.LoanPaid : 0),
                                     LoanPaidSEN = g.Sum(x => x.ProductId == 13 ? x.LoanPaid : 0),
                                     LoanPaidBAS = g.Sum(x => x.ProductId == 14 ? x.LoanPaid : 0),
                                     LoanPaidJMI = g.Sum(x => x.ProductId == 15 ? x.LoanPaid : 0),
                                     LoanPaidGGAS = g.Sum(x => x.ProductId == 16 ? x.LoanPaid : 0),
                                     LoanPaidTMSS = g.Sum(x => x.ProductId == 17 ? x.LoanPaid : 0),
                                     LoanPaidAY = g.Sum(x => x.ProductId == 18 ? x.LoanPaid : 0),
                                     LoanPaidETC = g.Sum(x => x.ProductId == 19 ? x.LoanPaid : 0),
                                     LoanPaidTOT = g.Sum(x => x.LoanPaid),
                                     CylinderSalePurchaseNAV = g.Sum(x => x.ProductId == 1 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseDUB = g.Sum(x => x.ProductId == 2 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseUNI = g.Sum(x => x.ProductId == 3 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseOME = g.Sum(x => x.ProductId == 4 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseMAX = g.Sum(x => x.ProductId == 5 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseFRESH = g.Sum(x => x.ProductId == 6 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseKH = g.Sum(x => x.ProductId == 7 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseBM = g.Sum(x => x.ProductId == 8 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseDEL = g.Sum(x => x.ProductId == 9 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseJAM = g.Sum(x => x.ProductId == 10 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseBEX = g.Sum(x => x.ProductId == 11 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseBEN = g.Sum(x => x.ProductId == 12 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseSEN = g.Sum(x => x.ProductId == 13 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseBAS = g.Sum(x => x.ProductId == 14 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseJMI = g.Sum(x => x.ProductId == 15 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseGGAS = g.Sum(x => x.ProductId == 16 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseTMSS = g.Sum(x => x.ProductId == 17 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseAY = g.Sum(x => x.ProductId == 18 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseETC = g.Sum(x => x.ProductId == 19 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseTOT = g.Sum(x => x.CylinderSalePurchase),
                                     TodayNAV = g.Sum(x => x.ProductId == 1 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayDUB = g.Sum(x => x.ProductId == 2 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayUNI = g.Sum(x => x.ProductId == 3 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayOME = g.Sum(x => x.ProductId == 4 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayMAX = g.Sum(x => x.ProductId == 5 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayFRESH = g.Sum(x => x.ProductId == 6 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayKH = g.Sum(x => x.ProductId == 7 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayBM = g.Sum(x => x.ProductId == 8 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayDEL = g.Sum(x => x.ProductId == 9 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayJAM = g.Sum(x => x.ProductId == 10 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayBEX = g.Sum(x => x.ProductId == 11 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayBEN = g.Sum(x => x.ProductId == 12 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodaySEN = g.Sum(x => x.ProductId == 13 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayBAS = g.Sum(x => x.ProductId == 14 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayJMI = g.Sum(x => x.ProductId == 15 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayGGAS = g.Sum(x => x.ProductId == 16 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayTMSS = g.Sum(x => x.ProductId == 17 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayAY = g.Sum(x => x.ProductId == 18 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayETC = g.Sum(x => x.ProductId == 19 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) : 0),
                                     TodayTOT = g.Sum(x => (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase)),
                                     PreviousStockNAV = g.Sum(x => x.ProductId == 1 ? x.PreviousStock : 0),
                                     PreviousStockDUB = g.Sum(x => x.ProductId == 2 ? x.PreviousStock : 0),
                                     PreviousStockUNI = g.Sum(x => x.ProductId == 3 ? x.PreviousStock : 0),
                                     PreviousStockOME = g.Sum(x => x.ProductId == 4 ? x.PreviousStock : 0),
                                     PreviousStockMAX = g.Sum(x => x.ProductId == 5 ? x.PreviousStock : 0),
                                     PreviousStockFRESH = g.Sum(x => x.ProductId == 6 ? x.PreviousStock : 0),
                                     PreviousStockKH = g.Sum(x => x.ProductId == 7 ? x.PreviousStock : 0),
                                     PreviousStockBM = g.Sum(x => x.ProductId == 8 ? x.PreviousStock : 0),
                                     PreviousStockDEL = g.Sum(x => x.ProductId == 9 ? x.PreviousStock : 0),
                                     PreviousStockJAM = g.Sum(x => x.ProductId == 10 ? x.PreviousStock : 0),
                                     PreviousStockBEX = g.Sum(x => x.ProductId == 11 ? x.PreviousStock : 0),
                                     PreviousStockBEN = g.Sum(x => x.ProductId == 12 ? x.PreviousStock : 0),
                                     PreviousStockSEN = g.Sum(x => x.ProductId == 13 ? x.PreviousStock : 0),
                                     PreviousStockBAS = g.Sum(x => x.ProductId == 14 ? x.PreviousStock : 0),
                                     PreviousStockJMI = g.Sum(x => x.ProductId == 15 ? x.PreviousStock : 0),
                                     PreviousStockGGAS = g.Sum(x => x.ProductId == 16 ? x.PreviousStock : 0),
                                     PreviousStockTMSS = g.Sum(x => x.ProductId == 17 ? x.PreviousStock : 0),
                                     PreviousStockAY = g.Sum(x => x.ProductId == 18 ? x.PreviousStock : 0),
                                     PreviousStockETC = g.Sum(x => x.ProductId == 19 ? x.PreviousStock : 0),
                                     PreviousStockTOT = g.Sum(x => x.PreviousStock),
                                     RefilePurchaseNAV = g.Sum(x => x.ProductId == 1 ? x.RefilePurchase : 0),
                                     RefilePurchaseDUB = g.Sum(x => x.ProductId == 2 ? x.RefilePurchase : 0),
                                     RefilePurchaseUNI = g.Sum(x => x.ProductId == 3 ? x.RefilePurchase : 0),
                                     RefilePurchaseOME = g.Sum(x => x.ProductId == 4 ? x.RefilePurchase : 0),
                                     RefilePurchaseMAX = g.Sum(x => x.ProductId == 5 ? x.RefilePurchase : 0),
                                     RefilePurchaseFRESH = g.Sum(x => x.ProductId == 6 ? x.RefilePurchase : 0),
                                     RefilePurchaseKH = g.Sum(x => x.ProductId == 7 ? x.RefilePurchase : 0),
                                     RefilePurchaseBM = g.Sum(x => x.ProductId == 8 ? x.RefilePurchase : 0),
                                     RefilePurchaseDEL = g.Sum(x => x.ProductId == 9 ? x.RefilePurchase : 0),
                                     RefilePurchaseJAM = g.Sum(x => x.ProductId == 10 ? x.RefilePurchase : 0),
                                     RefilePurchaseBEX = g.Sum(x => x.ProductId == 11 ? x.RefilePurchase : 0),
                                     RefilePurchaseBEN = g.Sum(x => x.ProductId == 12 ? x.RefilePurchase : 0),
                                     RefilePurchaseSEN = g.Sum(x => x.ProductId == 13 ? x.RefilePurchase : 0),
                                     RefilePurchaseBAS = g.Sum(x => x.ProductId == 14 ? x.RefilePurchase : 0),
                                     RefilePurchaseJMI = g.Sum(x => x.ProductId == 15 ? x.RefilePurchase : 0),
                                     RefilePurchaseGGAS = g.Sum(x => x.ProductId == 16 ? x.RefilePurchase : 0),
                                     RefilePurchaseTMSS = g.Sum(x => x.ProductId == 17 ? x.RefilePurchase : 0),
                                     RefilePurchaseAY = g.Sum(x => x.ProductId == 18 ? x.RefilePurchase : 0),
                                     RefilePurchaseETC = g.Sum(x => x.ProductId == 19 ? x.RefilePurchase : 0),
                                     RefilePurchaseTOT = g.Sum(x => x.RefilePurchase),
                                     PackagePurchaseNAV = g.Sum(x => x.ProductId == 1 ? x.PackagePurchase : 0),
                                     PackagePurchaseDUB = g.Sum(x => x.ProductId == 2 ? x.PackagePurchase : 0),
                                     PackagePurchaseUNI = g.Sum(x => x.ProductId == 3 ? x.PackagePurchase : 0),
                                     PackagePurchaseOME = g.Sum(x => x.ProductId == 4 ? x.PackagePurchase : 0),
                                     PackagePurchaseMAX = g.Sum(x => x.ProductId == 5 ? x.PackagePurchase : 0),
                                     PackagePurchaseFRESH = g.Sum(x => x.ProductId == 6 ? x.PackagePurchase : 0),
                                     PackagePurchaseKH = g.Sum(x => x.ProductId == 7 ? x.PackagePurchase : 0),
                                     PackagePurchaseBM = g.Sum(x => x.ProductId == 8 ? x.PackagePurchase : 0),
                                     PackagePurchaseDEL = g.Sum(x => x.ProductId == 9 ? x.PackagePurchase : 0),
                                     PackagePurchaseJAM = g.Sum(x => x.ProductId == 10 ? x.PackagePurchase : 0),
                                     PackagePurchaseBEX = g.Sum(x => x.ProductId == 11 ? x.PackagePurchase : 0),
                                     PackagePurchaseBEN = g.Sum(x => x.ProductId == 12 ? x.PackagePurchase : 0),
                                     PackagePurchaseSEN = g.Sum(x => x.ProductId == 13 ? x.PackagePurchase : 0),
                                     PackagePurchaseBAS = g.Sum(x => x.ProductId == 14 ? x.PackagePurchase : 0),
                                     PackagePurchaseJMI = g.Sum(x => x.ProductId == 15 ? x.PackagePurchase : 0),
                                     PackagePurchaseGGAS = g.Sum(x => x.ProductId == 16 ? x.PackagePurchase : 0),
                                     PackagePurchaseTMSS = g.Sum(x => x.ProductId == 17 ? x.PackagePurchase : 0),
                                     PackagePurchaseAY = g.Sum(x => x.ProductId == 18 ? x.PackagePurchase : 0),
                                     PackagePurchaseETC = g.Sum(x => x.ProductId == 19 ? x.PackagePurchase : 0),
                                     PackagePurchaseTOT = g.Sum(x => x.PackagePurchase),
                                     TotalStockNAV = g.Sum(x => x.ProductId == 1 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockDUB = g.Sum(x => x.ProductId == 2 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockUNI = g.Sum(x => x.ProductId == 3 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockOME = g.Sum(x => x.ProductId == 4 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockMAX = g.Sum(x => x.ProductId == 5 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockFRESH = g.Sum(x => x.ProductId == 6 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockKH = g.Sum(x => x.ProductId == 7 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockBM = g.Sum(x => x.ProductId == 8 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockDEL = g.Sum(x => x.ProductId == 9 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockJAM = g.Sum(x => x.ProductId == 10 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockBEX = g.Sum(x => x.ProductId == 11 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockBEN = g.Sum(x => x.ProductId == 12 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockSEN = g.Sum(x => x.ProductId == 13 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockBAS = g.Sum(x => x.ProductId == 14 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockJMI = g.Sum(x => x.ProductId == 15 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockGGAS = g.Sum(x => x.ProductId == 16 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockTMSS = g.Sum(x => x.ProductId == 17 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockAY = g.Sum(x => x.ProductId == 18 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockETC = g.Sum(x => x.ProductId == 19 ? (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) : 0),
                                     TotalStockTOT = g.Sum(x => (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase)),
                                 };

            var salesManData = await _context.SALESMANTAB.Where(x => x.BranchCode == branchCode && x.ActiveYN == "Y").ToListAsync();

            var salesSummary = (from sm in salesManData
                                join ad in aggregatedData on sm.PersonId equals ad.PersonId into adGroup
                                from ad in adGroup.DefaultIfEmpty()
                                select new
                                {
                                    sm.PersonName,
                                    ad?.NAV,
                                    ad?.DUB,
                                    ad?.UNI,
                                    ad?.OME,
                                    ad?.MAX,
                                    ad?.FRESH,
                                    ad?.KH,
                                    ad?.BM,
                                    ad?.DEL,
                                    ad?.JAM,
                                    ad?.BEX,
                                    ad?.BEN,
                                    ad?.SEN,
                                    ad?.BAS,
                                    ad?.JMI,
                                    ad?.GGAS,
                                    ad?.TMSS,
                                    ad?.AY,
                                    ad?.ETC,
                                    ad?.TOT,
                                    ad?.CylinderExchangeNAV,
                                    ad?.CylinderExchangeDUB,
                                    ad?.CylinderExchangeUNI,
                                    ad?.CylinderExchangeOME,
                                    ad?.CylinderExchangeMAX,
                                    ad?.CylinderExchangeFRESH,
                                    ad?.CylinderExchangeKH,
                                    ad?.CylinderExchangeBM,
                                    ad?.CylinderExchangeDEL,
                                    ad?.CylinderExchangeJAM,
                                    ad?.CylinderExchangeBEX,
                                    ad?.CylinderExchangeBEN,
                                    ad?.CylinderExchangeSEN,
                                    ad?.CylinderExchangeBAS,
                                    ad?.CylinderExchangeJMI,
                                    ad?.CylinderExchangeGGAS,
                                    ad?.CylinderExchangeTMSS,
                                    ad?.CylinderExchangeAY,
                                    ad?.CylinderExchangeETC,
                                    ad?.CylinderExchangeTOT,
                                    ad?.LoanPaidNAV,
                                    ad?.LoanPaidDUB,
                                    ad?.LoanPaidUNI,
                                    ad?.LoanPaidOME,
                                    ad?.LoanPaidMAX,
                                    ad?.LoanPaidFRESH,
                                    ad?.LoanPaidKH,
                                    ad?.LoanPaidBM,
                                    ad?.LoanPaidDEL,
                                    ad?.LoanPaidJAM,
                                    ad?.LoanPaidBEX,
                                    ad?.LoanPaidBEN,
                                    ad?.LoanPaidSEN,
                                    ad?.LoanPaidBAS,
                                    ad?.LoanPaidJMI,
                                    ad?.LoanPaidGGAS,
                                    ad?.LoanPaidTMSS,
                                    ad?.LoanPaidAY,
                                    ad?.LoanPaidETC,
                                    ad?.LoanPaidTOT,
                                    ad?.CylinderSalePurchaseNAV,
                                    ad?.CylinderSalePurchaseDUB,
                                    ad?.CylinderSalePurchaseUNI,
                                    ad?.CylinderSalePurchaseOME,
                                    ad?.CylinderSalePurchaseMAX,
                                    ad?.CylinderSalePurchaseFRESH,
                                    ad?.CylinderSalePurchaseKH,
                                    ad?.CylinderSalePurchaseBM,
                                    ad?.CylinderSalePurchaseDEL,
                                    ad?.CylinderSalePurchaseJAM,
                                    ad?.CylinderSalePurchaseBEX,
                                    ad?.CylinderSalePurchaseBEN,
                                    ad?.CylinderSalePurchaseSEN,
                                    ad?.CylinderSalePurchaseBAS,
                                    ad?.CylinderSalePurchaseJMI,
                                    ad?.CylinderSalePurchaseGGAS,
                                    ad?.CylinderSalePurchaseTMSS,
                                    ad?.CylinderSalePurchaseAY,
                                    ad?.CylinderSalePurchaseETC,
                                    ad?.CylinderSalePurchaseTOT,
                                    ad?.TodayNAV,
                                    ad?.TodayDUB,
                                    ad?.TodayUNI,
                                    ad?.TodayOME,
                                    ad?.TodayMAX,
                                    ad?.TodayFRESH,
                                    ad?.TodayKH,
                                    ad?.TodayBM,
                                    ad?.TodayDEL,
                                    ad?.TodayJAM,
                                    ad?.TodayBEX,
                                    ad?.TodayBEN,
                                    ad?.TodaySEN,
                                    ad?.TodayBAS,
                                    ad?.TodayJMI,
                                    ad?.TodayGGAS,
                                    ad?.TodayTMSS,
                                    ad?.TodayAY,
                                    ad?.TodayETC,
                                    ad?.TodayTOT,
                                    ad?.PreviousStockNAV,
                                    ad?.PreviousStockDUB,
                                    ad?.PreviousStockUNI,
                                    ad?.PreviousStockOME,
                                    ad?.PreviousStockMAX,
                                    ad?.PreviousStockFRESH,
                                    ad?.PreviousStockKH,
                                    ad?.PreviousStockBM,
                                    ad?.PreviousStockDEL,
                                    ad?.PreviousStockJAM,
                                    ad?.PreviousStockBEX,
                                    ad?.PreviousStockBEN,
                                    ad?.PreviousStockSEN,
                                    ad?.PreviousStockBAS,
                                    ad?.PreviousStockJMI,
                                    ad?.PreviousStockGGAS,
                                    ad?.PreviousStockTMSS,
                                    ad?.PreviousStockAY,
                                    ad?.PreviousStockETC,
                                    ad?.PreviousStockTOT,
                                    ad?.RefilePurchaseNAV,
                                    ad?.RefilePurchaseDUB,
                                    ad?.RefilePurchaseUNI,
                                    ad?.RefilePurchaseOME,
                                    ad?.RefilePurchaseMAX,
                                    ad?.RefilePurchaseFRESH,
                                    ad?.RefilePurchaseKH,
                                    ad?.RefilePurchaseBM,
                                    ad?.RefilePurchaseDEL,
                                    ad?.RefilePurchaseJAM,
                                    ad?.RefilePurchaseBEX,
                                    ad?.RefilePurchaseBEN,
                                    ad?.RefilePurchaseSEN,
                                    ad?.RefilePurchaseBAS,
                                    ad?.RefilePurchaseJMI,
                                    ad?.RefilePurchaseGGAS,
                                    ad?.RefilePurchaseTMSS,
                                    ad?.RefilePurchaseAY,
                                    ad?.RefilePurchaseETC,
                                    ad?.RefilePurchaseTOT,
                                    ad?.PackagePurchaseNAV,
                                    ad?.PackagePurchaseDUB,
                                    ad?.PackagePurchaseUNI,
                                    ad?.PackagePurchaseOME,
                                    ad?.PackagePurchaseMAX,
                                    ad?.PackagePurchaseFRESH,
                                    ad?.PackagePurchaseKH,
                                    ad?.PackagePurchaseBM,
                                    ad?.PackagePurchaseDEL,
                                    ad?.PackagePurchaseJAM,
                                    ad?.PackagePurchaseBEX,
                                    ad?.PackagePurchaseBEN,
                                    ad?.PackagePurchaseSEN,
                                    ad?.PackagePurchaseBAS,
                                    ad?.PackagePurchaseJMI,
                                    ad?.PackagePurchaseGGAS,
                                    ad?.PackagePurchaseTMSS,
                                    ad?.PackagePurchaseAY,
                                    ad?.PackagePurchaseETC,
                                    ad?.PackagePurchaseTOT,
                                    ad?.TotalStockNAV,
                                    ad?.TotalStockDUB,
                                    ad?.TotalStockUNI,
                                    ad?.TotalStockOME,
                                    ad?.TotalStockMAX,
                                    ad?.TotalStockFRESH,
                                    ad?.TotalStockKH,
                                    ad?.TotalStockBM,
                                    ad?.TotalStockDEL,
                                    ad?.TotalStockJAM,
                                    ad?.TotalStockBEX,
                                    ad?.TotalStockBEN,
                                    ad?.TotalStockSEN,
                                    ad?.TotalStockBAS,
                                    ad?.TotalStockJMI,
                                    ad?.TotalStockGGAS,
                                    ad?.TotalStockTMSS,
                                    ad?.TotalStockAY,
                                    ad?.TotalStockETC,
                                    ad?.TotalStockTOT,
                                }).ToList();

            // Final Results (Combining Sales Summary and Report Types)
            var finalResults = new List<dynamic>();

            // Salesman Summary
            finalResults.AddRange(salesSummary
            .GroupBy(s => s.PersonName)  // Group by PersonName
            .Select(g => new
            {
                ReportType = "Salesman",
                Name = g.Key,  // Use the PersonName from the group key
                NAV = g.Sum(s => s.NAV ?? 0),  // Sum NAV
                DUB = g.Sum(s => s.DUB ?? 0),  // Sum DUB
                UNI = g.Sum(s => s.UNI ?? 0),  // Sum UNI
                OME = g.Sum(s => s.OME ?? 0),  // Sum OME
                MAX = g.Sum(s => s.MAX ?? 0),  // Sum PKG
                FRESH = g.Sum(s => s.FRESH ?? 0),    // Sum FRESH
                KH = g.Sum(s => s.KH ?? 0),      // Sum CP
                BM = g.Sum(s => s.BM ?? 0),      // Sum BM
                DEL = g.Sum(s => s.DEL ?? 0),    // Sum DEL
                JAM = g.Sum(s => s.JAM ?? 0),    // Sum JAM
                BEX = g.Sum(s => s.BEX ?? 0),    // Sum BEX
                BEN = g.Sum(s => s.BEN ?? 0),    // Sum BEN
                SEN = g.Sum(s => s.SEN ?? 0),    // Sum SEN
                BAS = g.Sum(s => s.BAS ?? 0),    // Sum BAS
                JMI = g.Sum(s => s.JMI ?? 0),    // Sum JMI
                GGAS = g.Sum(s => s.GGAS ?? 0),  // Sum GGAS
                TMSS = g.Sum(s => s.TMSS ?? 0),  // Sum TMSS
                AY = g.Sum(s => s.AY ?? 0),      // Sum AY
                ETC = g.Sum(s => s.ETC ?? 0),    // Sum ETC
                TOTAL = g.Sum(s => s.TOT ?? 0)   // Sum TOTAL
            }));

            // Cylinder Exchange Report
            var navSum = salesSummary.Sum(s => s.CylinderExchangeNAV ?? 0);
            var dubSum = salesSummary.Sum(s => s.CylinderExchangeDUB ?? 0);
            var uniSum = salesSummary.Sum(s => s.CylinderExchangeUNI ?? 0);
            var OMESum = salesSummary.Sum(s => s.CylinderExchangeOME ?? 0);
            var MAXSum = salesSummary.Sum(s => s.CylinderExchangeMAX ?? 0);
            var FRESHSum = salesSummary.Sum(s => s.CylinderExchangeFRESH ?? 0);
            var KHSum = salesSummary.Sum(s => s.CylinderExchangeKH ?? 0);
            var BMSum = salesSummary.Sum(s => s.CylinderExchangeBM ?? 0);
            var DELSum = salesSummary.Sum(s => s.CylinderExchangeDEL ?? 0);
            var JAMSum = salesSummary.Sum(s => s.CylinderExchangeJAM ?? 0);
            var BEXSum = salesSummary.Sum(s => s.CylinderExchangeBEX ?? 0);
            var BENSum = salesSummary.Sum(s => s.CylinderExchangeBEN ?? 0);
            var SENSum = salesSummary.Sum(s => s.CylinderExchangeSEN ?? 0);
            var BASSum = salesSummary.Sum(s => s.CylinderExchangeBAS ?? 0);
            var JMISum = salesSummary.Sum(s => s.CylinderExchangeJMI ?? 0);
            var GGASSum = salesSummary.Sum(s => s.CylinderExchangeGGAS ?? 0);
            var TMSSSum = salesSummary.Sum(s => s.CylinderExchangeTMSS ?? 0);
            var AYSum = salesSummary.Sum(s => s.CylinderExchangeAY ?? 0);
            var ETCSum = salesSummary.Sum(s => s.CylinderExchangeETC ?? 0);
            var TOTSum = salesSummary.Sum(s => s.CylinderExchangeTOT ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "CylinderExchange",
                Name = "C-Exchange",
                NAV = navSum,
                DUB = dubSum,
                UNI = uniSum,
                OME = OMESum,
                MAX = MAXSum,
                FRESH = FRESHSum,
                KH = KHSum,
                BM = BMSum,
                DEL = DELSum,
                JAM = JAMSum,
                BEX = BEXSum,
                BEN = BENSum,
                SEN = SENSum,
                BAS = BASSum,
                JMI = JMISum,
                GGAS = GGASSum,
                TMSS = TMSSSum,
                AY = AYSum,
                ETC = ETCSum,
                TOT = TOTSum,  // Total sum
            }});

            // Loan Paid Report
            var navLoanPaidSum = salesSummary.Sum(s => s.LoanPaidNAV ?? 0);
            var dubLoanPaidSum = salesSummary.Sum(s => s.LoanPaidDUB ?? 0);
            var uniLoanPaidSum = salesSummary.Sum(s => s.LoanPaidUNI ?? 0);
            var OMELoanPaidSum = salesSummary.Sum(s => s.LoanPaidOME ?? 0);
            var MAXLoanPaidSum = salesSummary.Sum(s => s.LoanPaidMAX ?? 0);
            var FRESHLoanPaidSum = salesSummary.Sum(s => s.LoanPaidFRESH ?? 0);
            var KHLoanPaidSum = salesSummary.Sum(s => s.LoanPaidKH ?? 0);
            var BMLoanPaidSum = salesSummary.Sum(s => s.LoanPaidBM ?? 0);
            var DELLoanPaidSum = salesSummary.Sum(s => s.LoanPaidDEL ?? 0);
            var JAMLoanPaidSum = salesSummary.Sum(s => s.LoanPaidJAM ?? 0);
            var BEXLoanPaidSum = salesSummary.Sum(s => s.LoanPaidBEX ?? 0);
            var BENLoanPaidSum = salesSummary.Sum(s => s.LoanPaidBEN ?? 0);
            var SENLoanPaidSum = salesSummary.Sum(s => s.LoanPaidSEN ?? 0);
            var BASLoanPaidSum = salesSummary.Sum(s => s.LoanPaidBAS ?? 0);
            var JMILoanPaidSum = salesSummary.Sum(s => s.LoanPaidJMI ?? 0);
            var GGASLoanPaidSum = salesSummary.Sum(s => s.LoanPaidGGAS ?? 0);
            var TMSSLoanPaidSum = salesSummary.Sum(s => s.LoanPaidTMSS ?? 0);
            var AYLoanPaidSum = salesSummary.Sum(s => s.LoanPaidAY ?? 0);
            var ETCLoanPaidSum = salesSummary.Sum(s => s.LoanPaidETC ?? 0);
            var TOTLoanPaidSum = salesSummary.Sum(s => s.LoanPaidTOT ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "LoanPaid",
                Name = "Loan/Paid",
                NAV = navLoanPaidSum,
                DUB = dubLoanPaidSum,
                UNI = uniLoanPaidSum,
                OME = OMELoanPaidSum,
                MAX = MAXLoanPaidSum,
                FRESH = FRESHLoanPaidSum,
                KH = KHLoanPaidSum,
                BM = BMLoanPaidSum,
                DEL = DELLoanPaidSum,
                JAM = JAMLoanPaidSum,
                BEX = BEXLoanPaidSum,
                BEN = BENLoanPaidSum,
                SEN = SENLoanPaidSum,
                BAS = BASLoanPaidSum,
                JMI = JMILoanPaidSum,
                GGAS = GGASLoanPaidSum,
                TMSS = TMSSLoanPaidSum,
                AY = AYLoanPaidSum,
                ETC = ETCLoanPaidSum,
                TOT = TOTLoanPaidSum,  // Total LoanPaidSum
            }});

            // CylinderSalePurchase Report
            var navCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseNAV ?? 0);
            var dubCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseDUB ?? 0);
            var uniCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseUNI ?? 0);
            var OMECylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseOME ?? 0);
            var MAXCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseMAX ?? 0);
            var FRESHCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseFRESH ?? 0);
            var KHCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseKH ?? 0);
            var BMCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseBM ?? 0);
            var DELCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseDEL ?? 0);
            var JAMCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseJAM ?? 0);
            var BEXCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseBEX ?? 0);
            var BENCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseBEN ?? 0);
            var SENCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseSEN ?? 0);
            var BASCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseBAS ?? 0);
            var JMICylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseJMI ?? 0);
            var GGASCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseGGAS ?? 0);
            var TMSSCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseTMSS ?? 0);
            var AYCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseAY ?? 0);
            var ETCCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseETC ?? 0);
            var TOTCylinderSalePurchaseSum = salesSummary.Sum(s => s.CylinderSalePurchaseTOT ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "CylinderSalePurchase",
                Name = "C-Sale/Purchase",
                NAV = navCylinderSalePurchaseSum,
                DUB = dubCylinderSalePurchaseSum,
                UNI = uniCylinderSalePurchaseSum,
                OME = OMECylinderSalePurchaseSum,
                MAX = MAXCylinderSalePurchaseSum,
                FRESH = FRESHCylinderSalePurchaseSum,
                KH = KHCylinderSalePurchaseSum,
                BM = BMCylinderSalePurchaseSum,
                DEL = DELCylinderSalePurchaseSum,
                JAM = JAMCylinderSalePurchaseSum,
                BEX = BEXCylinderSalePurchaseSum,
                BEN = BENCylinderSalePurchaseSum,
                SEN = SENCylinderSalePurchaseSum,
                BAS = BASCylinderSalePurchaseSum,
                JMI = JMICylinderSalePurchaseSum,
                GGAS = GGASCylinderSalePurchaseSum,
                TMSS = TMSSCylinderSalePurchaseSum,
                AY = AYCylinderSalePurchaseSum,
                ETC = ETCCylinderSalePurchaseSum,
                TOT = TOTCylinderSalePurchaseSum,  // Total CylinderSalePurchaseSum
            }});

            // Today Report
            var navTodaySum = salesSummary.Sum(s => s.TodayNAV ?? 0);
            var dubTodaySum = salesSummary.Sum(s => s.TodayDUB ?? 0);
            var uniTodaySum = salesSummary.Sum(s => s.TodayUNI ?? 0);
            var OMETodaySum = salesSummary.Sum(s => s.TodayOME ?? 0);
            var MAXTodaySum = salesSummary.Sum(s => s.TodayMAX ?? 0);
            var FRESHTodaySum = salesSummary.Sum(s => s.TodayFRESH ?? 0);
            var KHTodaySum = salesSummary.Sum(s => s.TodayKH ?? 0);
            var BMTodaySum = salesSummary.Sum(s => s.TodayBM ?? 0);
            var DELTodaySum = salesSummary.Sum(s => s.TodayDEL ?? 0);
            var JAMTodaySum = salesSummary.Sum(s => s.TodayJAM ?? 0);
            var BEXTodaySum = salesSummary.Sum(s => s.TodayBEX ?? 0);
            var BENTodaySum = salesSummary.Sum(s => s.TodayBEN ?? 0);
            var SENTodaySum = salesSummary.Sum(s => s.TodaySEN ?? 0);
            var BASTodaySum = salesSummary.Sum(s => s.TodayBAS ?? 0);
            var JMITodaySum = salesSummary.Sum(s => s.TodayJMI ?? 0);
            var GGASTodaySum = salesSummary.Sum(s => s.TodayGGAS ?? 0);
            var TMSSTodaySum = salesSummary.Sum(s => s.TodayTMSS ?? 0);
            var AYTodaySum = salesSummary.Sum(s => s.TodayAY ?? 0);
            var ETCTodaySum = salesSummary.Sum(s => s.TodayETC ?? 0);
            var TOTTodaySum = salesSummary.Sum(s => s.TodayTOT ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "Today",
                Name = "Todays",
                NAV = navTodaySum,
                DUB = dubTodaySum,
                UNI = uniTodaySum,
                OME = OMETodaySum,
                MAX = MAXTodaySum,
                FRESH = FRESHTodaySum,
                KH = KHTodaySum,
                BM = BMTodaySum,
                DEL = DELTodaySum,
                JAM = JAMTodaySum,
                BEX = BEXTodaySum,
                BEN = BENTodaySum,
                SEN = SENTodaySum,
                BAS = BASTodaySum,
                JMI = JMITodaySum,
                GGAS = GGASTodaySum,
                TMSS = TMSSTodaySum,
                AY = AYTodaySum,
                ETC = ETCTodaySum,
                TOT = TOTTodaySum,  // Total TodaySum
            }});

            // Previous Stock Report
            var navPreviousStockSum = salesSummary.Sum(s => s.PreviousStockNAV ?? 0);
            var dubPreviousStockSum = salesSummary.Sum(s => s.PreviousStockDUB ?? 0);
            var uniPreviousStockSum = salesSummary.Sum(s => s.PreviousStockUNI ?? 0);
            var OMEPreviousStockSum = salesSummary.Sum(s => s.PreviousStockOME ?? 0);
            var MAXPreviousStockSum = salesSummary.Sum(s => s.PreviousStockMAX ?? 0);
            var FRESHPreviousStockSum = salesSummary.Sum(s => s.PreviousStockFRESH ?? 0);
            var KHPreviousStockSum = salesSummary.Sum(s => s.PreviousStockKH ?? 0);
            var BMPreviousStockSum = salesSummary.Sum(s => s.PreviousStockBM ?? 0);
            var DELPreviousStockSum = salesSummary.Sum(s => s.PreviousStockDEL ?? 0);
            var JAMPreviousStockSum = salesSummary.Sum(s => s.PreviousStockJAM ?? 0);
            var BEXPreviousStockSum = salesSummary.Sum(s => s.PreviousStockBEX ?? 0);
            var BENPreviousStockSum = salesSummary.Sum(s => s.PreviousStockBEN ?? 0);
            var SENPreviousStockSum = salesSummary.Sum(s => s.PreviousStockSEN ?? 0);
            var BASPreviousStockSum = salesSummary.Sum(s => s.PreviousStockBAS ?? 0);
            var JMIPreviousStockSum = salesSummary.Sum(s => s.PreviousStockJMI ?? 0);
            var GGASPreviousStockSum = salesSummary.Sum(s => s.PreviousStockGGAS ?? 0);
            var TMSSPreviousStockSum = salesSummary.Sum(s => s.PreviousStockTMSS ?? 0);
            var AYPreviousStockSum = salesSummary.Sum(s => s.PreviousStockAY ?? 0);
            var ETCPreviousStockSum = salesSummary.Sum(s => s.PreviousStockETC ?? 0);
            var TOTPreviousStockSum = salesSummary.Sum(s => s.PreviousStockTOT ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "PreviousStock",
                Name = "Previous Stock",
                NAV = navPreviousStockSum,
                DUB = dubPreviousStockSum,
                UNI = uniPreviousStockSum,
                OME = OMEPreviousStockSum,
                MAX = MAXPreviousStockSum,
                FRESH = FRESHPreviousStockSum,
                KH = KHPreviousStockSum,
                BM = BMPreviousStockSum,
                DEL = DELPreviousStockSum,
                JAM = JAMPreviousStockSum,
                BEX = BEXPreviousStockSum,
                BEN = BENPreviousStockSum,
                SEN = SENPreviousStockSum,
                BAS = BASPreviousStockSum,
                JMI = JMIPreviousStockSum,
                GGAS = GGASPreviousStockSum,
                TMSS = TMSSPreviousStockSum,
                AY = AYPreviousStockSum,
                ETC = ETCPreviousStockSum,
                TOT = TOTPreviousStockSum,  // Total PreviousStockSum
            }});

            // Refile Purchase Report
            var navRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseNAV ?? 0);
            var dubRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseDUB ?? 0);
            var uniRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseUNI ?? 0);
            var OMERefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseOME ?? 0);
            var MAXRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseMAX ?? 0);
            var FRESHRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseFRESH ?? 0);
            var KHRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseKH ?? 0);
            var BMRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseBM ?? 0);
            var DELRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseDEL ?? 0);
            var JAMRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseJAM ?? 0);
            var BEXRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseBEX ?? 0);
            var BENRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseBEN ?? 0);
            var SENRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseSEN ?? 0);
            var BASRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseBAS ?? 0);
            var JMIRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseJMI ?? 0);
            var GGASRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseGGAS ?? 0);
            var TMSSRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseTMSS ?? 0);
            var AYRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseAY ?? 0);
            var ETCRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseETC ?? 0);
            var TOTRefilePurchaseSum = salesSummary.Sum(s => s.RefilePurchaseTOT ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "RefilePurchase",
                Name = "Refil Purchase",
                NAV = navRefilePurchaseSum,
                DUB = dubRefilePurchaseSum,
                UNI = uniRefilePurchaseSum,
                OME = OMERefilePurchaseSum,
                MAX = MAXRefilePurchaseSum,
                FRESH = FRESHRefilePurchaseSum,
                KH = KHRefilePurchaseSum,
                BM = BMRefilePurchaseSum,
                DEL = DELRefilePurchaseSum,
                JAM = JAMRefilePurchaseSum,
                BEX = BEXRefilePurchaseSum,
                BEN = BENRefilePurchaseSum,
                SEN = SENRefilePurchaseSum,
                BAS = BASRefilePurchaseSum,
                JMI = JMIRefilePurchaseSum,
                GGAS = GGASRefilePurchaseSum,
                TMSS = TMSSRefilePurchaseSum,
                AY = AYRefilePurchaseSum,
                ETC = ETCRefilePurchaseSum,
                TOT = TOTRefilePurchaseSum,  // Total RefilePurchaseSum
            }});

            // Package Purchase Report
            var navPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseNAV ?? 0);
            var dubPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseDUB ?? 0);
            var uniPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseUNI ?? 0);
            var OMEPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseOME ?? 0);
            var MAXPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseMAX ?? 0);
            var FRESHPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseFRESH ?? 0);
            var KHPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseKH ?? 0);
            var BMPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseBM ?? 0);
            var DELPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseDEL ?? 0);
            var JAMPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseJAM ?? 0);
            var BEXPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseBEX ?? 0);
            var BENPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseBEN ?? 0);
            var SENPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseSEN ?? 0);
            var BASPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseBAS ?? 0);
            var JMIPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseJMI ?? 0);
            var GGASPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseGGAS ?? 0);
            var TMSSPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseTMSS ?? 0);
            var AYPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseAY ?? 0);
            var ETCPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseETC ?? 0);
            var TOTPackagePurchaseSum = salesSummary.Sum(s => s.PackagePurchaseTOT ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "PackagePurchase",
                Name = "Package Purchase",
                NAV = navPackagePurchaseSum,
                DUB = dubPackagePurchaseSum,
                UNI = uniPackagePurchaseSum,
                OME = OMEPackagePurchaseSum,
                MAX = MAXPackagePurchaseSum,
                FRESH = FRESHPackagePurchaseSum,
                KH = KHPackagePurchaseSum,
                BM = BMPackagePurchaseSum,
                DEL = DELPackagePurchaseSum,
                JAM = JAMPackagePurchaseSum,
                BEX = BEXPackagePurchaseSum,
                BEN = BENPackagePurchaseSum,
                SEN = SENPackagePurchaseSum,
                BAS = BASPackagePurchaseSum,
                JMI = JMIPackagePurchaseSum,
                GGAS = GGASPackagePurchaseSum,
                TMSS = TMSSPackagePurchaseSum,
                AY = AYPackagePurchaseSum,
                ETC = ETCPackagePurchaseSum,
                TOT = TOTPackagePurchaseSum,  // Total PackagePurchaseSum
            }});

            // TotalStock Report
            var navTotalStockSum = salesSummary.Sum(s => s.TotalStockNAV ?? 0);
            var dubTotalStockSum = salesSummary.Sum(s => s.TotalStockDUB ?? 0);
            var uniTotalStockSum = salesSummary.Sum(s => s.TotalStockUNI ?? 0);
            var OMETotalStockSum = salesSummary.Sum(s => s.TotalStockOME ?? 0);
            var MAXTotalStockSum = salesSummary.Sum(s => s.TotalStockMAX ?? 0);
            var FRESHTotalStockSum = salesSummary.Sum(s => s.TotalStockFRESH ?? 0);
            var KHTotalStockSum = salesSummary.Sum(s => s.TotalStockKH ?? 0);
            var BMTotalStockSum = salesSummary.Sum(s => s.TotalStockBM ?? 0);
            var DELTotalStockSum = salesSummary.Sum(s => s.TotalStockDEL ?? 0);
            var JAMTotalStockSum = salesSummary.Sum(s => s.TotalStockJAM ?? 0);
            var BEXTotalStockSum = salesSummary.Sum(s => s.TotalStockBEX ?? 0);
            var BENTotalStockSum = salesSummary.Sum(s => s.TotalStockBEN ?? 0);
            var SENTotalStockSum = salesSummary.Sum(s => s.TotalStockSEN ?? 0);
            var BASTotalStockSum = salesSummary.Sum(s => s.TotalStockBAS ?? 0);
            var JMITotalStockSum = salesSummary.Sum(s => s.TotalStockJMI ?? 0);
            var GGASTotalStockSum = salesSummary.Sum(s => s.TotalStockGGAS ?? 0);
            var TMSSTotalStockSum = salesSummary.Sum(s => s.TotalStockTMSS ?? 0);
            var AYTotalStockSum = salesSummary.Sum(s => s.TotalStockAY ?? 0);
            var ETCTotalStockSum = salesSummary.Sum(s => s.TotalStockETC ?? 0);
            var TOTTotalStockSum = salesSummary.Sum(s => s.TotalStockTOT ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "TotalStock",
                Name = "Total Stock",
                NAV = navTotalStockSum,
                DUB = dubTotalStockSum,
                UNI = uniTotalStockSum,
                OME = OMETotalStockSum,
                MAX = MAXTotalStockSum,
                FRESH = FRESHTotalStockSum,
                KH = KHTotalStockSum,
                BM = BMTotalStockSum,
                DEL = DELTotalStockSum,
                JAM = JAMTotalStockSum,
                BEX = BEXTotalStockSum,
                BEN = BENTotalStockSum,
                SEN = SENTotalStockSum,
                BAS = BASTotalStockSum,
                JMI = JMITotalStockSum,
                GGAS = GGASTotalStockSum,
                TMSS = TMSSTotalStockSum,
                AY = AYTotalStockSum,
                ETC = ETCTotalStockSum,
                TOT = TOTTotalStockSum,  // Total TotalStockSum
            }});


            if (!finalResults.Any())
            {
                return NotFound(new { Message = "Return Stock Information not found" });
            }

            return Ok(new
            {
                Message = "Return Stock Information retrieved successfully",
                Data = finalResults
            });
        }

    }
}
