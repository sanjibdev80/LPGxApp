using LPGxWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LPGxWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleInfosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SaleInfosController(AppDbContext context)
        {
            _context = context;
        }


        // GET: api/SaleInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleInfos>>> GetSaleInfos()
        {
            return await _context.SALEENTRYTAB.ToListAsync();
        }

        // GET: api/SaleInfos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SaleInfos>> GetSaleInfos(string id)
        {
            var SaleInfos = await _context.SALEENTRYTAB.FindAsync(id);
            if (SaleInfos == null)
            {
                return NotFound();
            }

            return SaleInfos;
        }

        // PUT: api/SaleInfos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSaleInfos(long id, SaleInfos SaleInfos)
        {
            if (id != SaleInfos.PersonId)
            {
                return BadRequest();
            }

            _context.Entry(SaleInfos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SaleInfosExists(id))
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

        // POST: api/SaleInfos
        [HttpPost]
        public async Task<ActionResult<SaleInfos>> PostSaleInfos(SaleInfos SaleInfos)
        {
            // Ensure the SaleId starts from 1 if you want to handle it manually.
            if (SaleInfos.SaleId == 0) // Check if not set manually
            {
                var maxId = _context.SALEENTRYTAB.Any() ? _context.SALEENTRYTAB.Max(x => x.SaleId) : 0;
                SaleInfos.SaleId = maxId + 1;
            }

            _context.SALEENTRYTAB.Add(SaleInfos);
            await _context.SaveChangesAsync();

            // Return a custom response with a message
            return Ok(new
            {
                Message = "Sale information register successfully",
                Data = new
                {
                    SaleId = SaleInfos.SaleId,
                    SaleDate = SaleInfos.SaleDate,
                    BranchCode = SaleInfos.BranchCode
                }
            });
        }

        // DELETE: api/SaleInfos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSaleInfos(long id)
        {
            var SaleInfos = await _context.SALEENTRYTAB.FindAsync(id);
            if (SaleInfos == null)
            {
                return NotFound();
            }

            _context.SALEENTRYTAB.Remove(SaleInfos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SaleInfosExists(long id)
        {
            return _context.SALEENTRYTAB.Any(e => e.PersonId == id);
        }

        // GET: api/SaleInfos/Branches/{branchCode}
        [HttpGet("Branches/{branchCode}")]
        public async Task<ActionResult<object>> GetBranchData(string branchCode)
        {
            // Extract user information once to avoid repeated calls to FirstOrDefault
            var salesList = await _context.SALEENTRYTAB
                .Where(b => b.BranchCode == branchCode)
                .ToListAsync();

            if (!salesList.Any())
            {
                return NotFound(new { Message = "Sales Information not found" });
            }

            return Ok(new
            {
                Message = "Sales Information retrieved successfully",
                Data = salesList
            });
        }

        // GET: api/SaleInfos/Date/{branchCode}/{reportDate}
        [HttpGet("Date/{branchCode}/{reportDate}")]
        public async Task<ActionResult<object>> GetBranchDataByDate(string branchCode, string reportDate)
        {
            DateTime rptDate = DateTime.Parse(reportDate);
            var salesData = await (from se in _context.SALEENTRYTAB
                                   where se.BranchCode == branchCode && se.SaleDate.Date == rptDate.Date
                                   select new
                                   {
                                       se.PersonId,
                                       se.ProductId,
                                       se.Unit,
                                       se.Packages,
                                       se.CylinderDue,
                                       se.CylinderPaid,
                                       se.OnDate,
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
                                     TOT = g.Sum(x => x.Unit),
                                     PKG = g.Sum(x => x.Packages),
                                     CD = g.Sum(x => x.CylinderDue),
                                     CP = g.Sum(x => x.CylinderPaid),
                                     DT = g.Sum(x => x.OnDate),
                                     TOTAL = g.Sum(x => x.Unit - (x.Packages + x.CylinderDue + x.CylinderPaid + x.OnDate)),
                                     CylinderExchangeNAV = g.Sum(x => x.ProductId == 1 ? x.CylinderExchange : 0),
                                     CylinderExchangeDUB = g.Sum(x => x.ProductId == 2 ? x.CylinderExchange : 0),
                                     CylinderExchangeUNI = g.Sum(x => x.ProductId == 3 ? x.CylinderExchange : 0),
                                     LoanPaidNAV = g.Sum(x => x.ProductId == 1 ? x.LoanPaid : 0),
                                     LoanPaidDUB = g.Sum(x => x.ProductId == 2 ? x.LoanPaid : 0),
                                     LoanPaidUNI = g.Sum(x => x.ProductId == 3 ? x.LoanPaid : 0),
                                     CylinderSalePurchaseNAV = g.Sum(x => x.ProductId == 1 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseDUB = g.Sum(x => x.ProductId == 2 ? x.CylinderSalePurchase : 0),
                                     CylinderSalePurchaseUNI = g.Sum(x => x.ProductId == 3 ? x.CylinderSalePurchase : 0),
                                     TodayNAV = g.Sum(x => x.ProductId == 1 ? x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase : 0),
                                     TodayDUB = g.Sum(x => x.ProductId == 2 ? x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase : 0),
                                     TodayUNI = g.Sum(x => x.ProductId == 3 ? x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase : 0),
                                     TodayTOTAL = g.Sum(x => (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase) - (x.Packages + x.CylinderDue + x.CylinderPaid + x.OnDate)),
                                     PreviousStockNAV = g.Sum(x => x.ProductId == 1 ? x.PreviousStock : 0),
                                     PreviousStockDUB = g.Sum(x => x.ProductId == 2 ? x.PreviousStock : 0),
                                     PreviousStockUNI = g.Sum(x => x.ProductId == 3 ? x.PreviousStock : 0),
                                     RefilePurchaseNAV = g.Sum(x => x.ProductId == 1 ? x.RefilePurchase : 0),
                                     RefilePurchaseDUB = g.Sum(x => x.ProductId == 2 ? x.RefilePurchase : 0),
                                     RefilePurchaseUNI = g.Sum(x => x.ProductId == 3 ? x.RefilePurchase : 0),
                                     PackagePurchaseNAV = g.Sum(x => x.ProductId == 1 ? x.PackagePurchase : 0),
                                     PackagePurchaseDUB = g.Sum(x => x.ProductId == 2 ? x.PackagePurchase : 0),
                                     PackagePurchaseUNI = g.Sum(x => x.ProductId == 3 ? x.PackagePurchase : 0),
                                     TotalStockNAV = g.Sum(x => x.ProductId == 1 ? x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase : 0),
                                     TotalStockDUB = g.Sum(x => x.ProductId == 2 ? x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase : 0),
                                     TotalStockUNI = g.Sum(x => x.ProductId == 3 ? x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase : 0),
                                     TotalStockTOTAL = g.Sum(x => (x.Unit + x.CylinderExchange + x.LoanPaid + x.CylinderSalePurchase + x.PreviousStock + x.RefilePurchase + x.PackagePurchase) - (x.Packages + x.CylinderDue + x.CylinderPaid + x.OnDate)),
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
                                    ad?.TOT,
                                    ad?.PKG,
                                    ad?.CD,
                                    ad?.CP,
                                    ad?.DT,
                                    ad?.TOTAL,
                                    ad?.CylinderExchangeNAV,
                                    ad?.CylinderExchangeDUB,
                                    ad?.CylinderExchangeUNI,
                                    ad?.LoanPaidNAV,
                                    ad?.LoanPaidDUB,
                                    ad?.LoanPaidUNI,
                                    ad?.CylinderSalePurchaseNAV,
                                    ad?.CylinderSalePurchaseDUB,
                                    ad?.CylinderSalePurchaseUNI,
                                    ad?.TodayNAV,
                                    ad?.TodayDUB,
                                    ad?.TodayUNI,
                                    ad?.TodayTOTAL,
                                    ad?.PreviousStockNAV,
                                    ad?.PreviousStockDUB,
                                    ad?.PreviousStockUNI,
                                    ad?.RefilePurchaseNAV,
                                    ad?.RefilePurchaseDUB,
                                    ad?.RefilePurchaseUNI,
                                    ad?.PackagePurchaseNAV,
                                    ad?.PackagePurchaseDUB,
                                    ad?.PackagePurchaseUNI,
                                    ad?.TotalStockNAV,
                                    ad?.TotalStockDUB,
                                    ad?.TotalStockUNI,
                                    ad?.TotalStockTOTAL,
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
                TOT = g.Sum(s => s.TOT ?? 0),  // Sum TOT
                PKG = g.Sum(s => s.PKG ?? 0),  // Sum PKG
                CD = g.Sum(s => s.CD ?? 0),    // Sum CD
                CP = g.Sum(s => s.CP ?? 0),    // Sum CP
                DT = g.Sum(s => s.DT ?? 0),    // Sum DT
                TOTAL = g.Sum(s => s.TOTAL ?? 0)  // Sum TOTAL
            }));

            // Cylinder Exchange Report
            var navSum = salesSummary.Sum(s => s.CylinderExchangeNAV ?? 0);
            var dubSum = salesSummary.Sum(s => s.CylinderExchangeDUB ?? 0);
            var uniSum = salesSummary.Sum(s => s.CylinderExchangeUNI ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "CylinderExchange",
                Name = "C-Exchange",
                NAV = navSum,
                DUB = dubSum,
                UNI = uniSum,
                TOT = navSum + dubSum + uniSum,  // Total sum
                PKG = 0,
                CD = 0,
                CP = 0,
                DT = 0,
                TOTAL = navSum + dubSum + uniSum  // Total sum
            }});

            // Loan Paid Report
            var navLoanSum = salesSummary.Sum(s => s.LoanPaidNAV ?? 0);
            var dubLoanSum = salesSummary.Sum(s => s.LoanPaidDUB ?? 0);
            var uniLoanSum = salesSummary.Sum(s => s.LoanPaidUNI ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "LoanPaid",
                Name = "Loan/Paid",
                NAV = navLoanSum,
                DUB = dubLoanSum,
                UNI = uniLoanSum,
                TOT = navLoanSum + dubLoanSum + uniLoanSum,  // Total sum
                PKG = 0,
                CD = 0,
                CP = 0,
                DT = 0,
                TOTAL = navLoanSum + dubLoanSum + uniLoanSum  // Total sum
            }});

            // Cylinder Sale Purchase Report
            var navCSaleSum = salesSummary.Sum(s => s.CylinderSalePurchaseNAV ?? 0);
            var dubCSaleSum = salesSummary.Sum(s => s.CylinderSalePurchaseDUB ?? 0);
            var uniCSaleSum = salesSummary.Sum(s => s.CylinderSalePurchaseUNI ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "CylinderSalePurchase",
                Name = "C-Sale/Purchase",
                NAV = navCSaleSum,
                DUB = dubCSaleSum,
                UNI = uniCSaleSum,
                TOT = navCSaleSum + dubCSaleSum + uniCSaleSum,  // Total sum
                PKG = 0,
                CD = 0,
                CP = 0,
                DT = 0,
                TOTAL = navCSaleSum + dubCSaleSum + uniCSaleSum  // Total sum
            }});

            // Today Report
            var navTodaySum = salesSummary.Sum(s => s.TodayNAV ?? 0);
            var dubTodaySum = salesSummary.Sum(s => s.TodayDUB ?? 0);
            var uniTodaySum = salesSummary.Sum(s => s.TodayUNI ?? 0);
            var PKGTodaykSum = salesSummary.Sum(s => s.PKG ?? 0);
            var CDTodaySum = salesSummary.Sum(s => s.CD ?? 0);
            var CPTodaySum = salesSummary.Sum(s => s.CP ?? 0);
            var DTTodaySum = salesSummary.Sum(s => s.DT ?? 0);
            var TotalTodaySum = salesSummary.Sum(s => s.TodayTOTAL ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "Today",
                Name = "Todays",
                NAV = navTodaySum,
                DUB = dubTodaySum,
                UNI = uniTodaySum,
                TOT = navTodaySum + dubTodaySum + uniTodaySum,  // Total sum
                PKG = PKGTodaykSum,
                CD = CDTodaySum,
                CP = CPTodaySum,
                DT = DTTodaySum,
                TOTAL = TotalTodaySum  // Total sum
            }});

            // Previous Stock Report
            var navPStockSum = salesSummary.Sum(s => s.PreviousStockNAV ?? 0);
            var dubPStockSum = salesSummary.Sum(s => s.PreviousStockDUB ?? 0);
            var uniPStockSum = salesSummary.Sum(s => s.PreviousStockUNI ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "PreviousStock",
                Name = "Previous Stock",
                NAV = navPStockSum,
                DUB = dubPStockSum,
                UNI = uniPStockSum,
                TOT = navPStockSum + dubPStockSum + uniPStockSum,  // Total sum
                PKG = 0,
                CD = 0,
                CP = 0,
                DT = 0,
                TOTAL = navPStockSum + dubPStockSum + uniPStockSum  // Total sum
            }});

            // Refile Purchase Report
            var navRPurchaseSum = salesSummary.Sum(s => s.RefilePurchaseNAV ?? 0);
            var dubRPurchaseSum = salesSummary.Sum(s => s.RefilePurchaseDUB ?? 0);
            var uniRPurchaseSum = salesSummary.Sum(s => s.RefilePurchaseUNI ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "RefilePurchase",
                Name = "Refil Purchase",
                NAV = navRPurchaseSum,
                DUB = dubRPurchaseSum,
                UNI = uniRPurchaseSum,
                TOT = navRPurchaseSum + dubRPurchaseSum + uniRPurchaseSum,  // Total sum
                PKG = 0,
                CD = 0,
                CP = 0,
                DT = 0,
                TOTAL = navRPurchaseSum + dubRPurchaseSum + uniRPurchaseSum  // Total sum
            }});

            // Package Purchase Report
            var navPackPurchaseSum = salesSummary.Sum(s => s.PackagePurchaseNAV ?? 0);
            var dubPackPurchaseSum = salesSummary.Sum(s => s.PackagePurchaseDUB ?? 0);
            var uniPackPurchaseSum = salesSummary.Sum(s => s.PackagePurchaseUNI ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "PackagePurchase",
                Name = "Package Purchase",
                NAV = navPackPurchaseSum,
                DUB = dubPackPurchaseSum,
                UNI = uniPackPurchaseSum,
                TOT = navPackPurchaseSum + dubPackPurchaseSum + uniPackPurchaseSum,  // Total sum
                PKG = 0,
                CD = 0,
                CP = 0,
                DT = 0,
                TOTAL = navPackPurchaseSum + dubPackPurchaseSum + uniPackPurchaseSum  // Total sum
            }});

            // Total Stock Report
            var navTotalStockSum = salesSummary.Sum(s => s.TotalStockNAV ?? 0);
            var dubTotalStockSum = salesSummary.Sum(s => s.TotalStockDUB ?? 0);
            var uniTotalStockSum = salesSummary.Sum(s => s.TotalStockUNI ?? 0);
            var PKGStockSum = salesSummary.Sum(s => s.PKG ?? 0);
            var CDStockSum = salesSummary.Sum(s => s.CD ?? 0);
            var CPStockSum = salesSummary.Sum(s => s.CP ?? 0);
            var DTStockSum = salesSummary.Sum(s => s.DT ?? 0);
            var GTotalStockSum = salesSummary.Sum(s => s.TotalStockTOTAL ?? 0);

            finalResults.AddRange(new[] { new
            {
                ReportType = "TotalStock",
                Name = "Total Stock",
                NAV = navTotalStockSum,
                DUB = dubTotalStockSum,
                UNI = uniTotalStockSum,
                TOT = navTotalStockSum + dubTotalStockSum + uniTotalStockSum,  // Total sum
                PKG = PKGStockSum,
                CD = CDStockSum,
                CP = CPStockSum,
                DT = DTStockSum,
                TOTAL = GTotalStockSum  // Total sum
            }});

            // Now you can order the final result list if required
            var orderedResults = finalResults.OrderBy(f => f.ReportType).ToList();

            if (!finalResults.Any())
            {
                return NotFound(new { Message = "Sales Information not found" });
            }

            return Ok(new
            {
                Message = "Sales Information retrieved successfully",
                Data = finalResults
            });
        }

    }
}
