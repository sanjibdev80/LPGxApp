using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RdlcWebApi.Class;
using RdlcWebApi.Services;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace RdlcWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LPGxReportController : ControllerBase
    {
        private IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public LPGxReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        public class LPGxReportParamRequest
        {
            public DateTime? RPTDATE { get; set; }
            public string? BRANCHCODE { get; set; }
            public string? SIGNONNAME { get; set; }

        }

        public class BranchResponse
        {
            [JsonProperty("branchcode")]
            public string BranchCode { get; set; }

            [JsonProperty("projectcode")]
            public string ProjectCode { get; set; }

            [JsonProperty("bldname")]
            public string BldName { get; set; }

            [JsonProperty("bldaddress")]
            public string BldAddress { get; set; }

            [JsonProperty("bldcity")]
            public string BldCity { get; set; }

            [JsonProperty("countrycode")]
            public string CountryCode { get; set; }

            [JsonProperty("entryuser")]
            public int? EntryUser { get; set; }

            [JsonProperty("createdate")]
            public DateTime? CreateDate { get; set; }

            [JsonProperty("enableyn")]
            public string EnableYN { get; set; }
        }

        public class ReturnReportResponse
        {
            public string? ReportType { get; set; }
            public string? Name { get; set; }
            public int? NAV { get; set; }
            public int? DUB { get; set; }
            public int? UNI { get; set; }
            public int? OME { get; set; }
            public int? MAX { get; set; }
            public int? FRESH { get; set; }
            public int? KH { get; set; }
            public int? BM { get; set; }
            public int? DEL { get; set; }
            public int? JAM { get; set; }
            public int? BEX { get; set; }
            public int? BEN { get; set; }
            public int? SEN { get; set; }
            public int? BAS { get; set; }
            public int? JMI { get; set; }
            public int? GGAS { get; set; }
            public int? TMSS { get; set; }
            public int? AY { get; set; }
            public int? ETC { get; set; }
            public int? TOT { get; set; }
        }

        public class SaleReportResponse
        {
            public string? ReportType { get; set; }
            public string? Name { get; set; }
            public int? NAV { get; set; }
            public int? DUB { get; set; }
            public int? UNI { get; set; }
            public int? TOT { get; set; }
            public int? PKG { get; set; }
            public int? CD { get; set; }
            public int? CP { get; set; }
            public int? DT { get; set; }
            public int? TOTAL { get; set; }
        }

        public class SalesResponseDto
        {
            public string? NAME { get; set; }
            public int? NAV { get; set; }
            public int? DUB { get; set; }
            public int? UNI { get; set; }
            public int? TOTAL { get; set; }
            public int? PKG { get; set; }
            public int? CD { get; set; }
            public int? CP { get; set; }
            public int? DT { get; set; }
            public int? TOTAL1 { get; set; }
        }

        public class ReturnResponseDto
        {
            public string? NAME { get; set; }
            public int? NAV { get; set; }
            public int? DUB { get; set; }
            public int? UNI { get; set; }
            public int? OME { get; set; }
            public int? MAX { get; set; }
            public int? FRESH { get; set; }
            public int? KH { get; set; }
            public int? BM { get; set; }
            public int? DEL { get; set; }
            public int? JAM { get; set; }
            public int? BEX { get; set; }
            public int? BEN { get; set; }
            public int? SEN { get; set; }
            public int? BAS { get; set; }
            public int? JMI { get; set; }
            public int? GGAS { get; set; }
            public int? TMSS { get; set; }
            public int? AY { get; set; }
            public int? ETC { get; set; }
            public int? TOTAL2 { get; set; }
        }

        // POST: api/LPGxReport/SalesReturnRegister
        [HttpPost("SalesReturnRegister")]
        public async Task<ActionResult> SalesReturnRegisterReport([FromBody] LPGxReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "SalesReturnStockReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.SalesReturnReportAsync(reportNameWithLang, reportType, param);

                if (reportFileBytes == null || reportFileBytes.Length == 0)
                {
                    _logger.LogWarning("Report generation failed for {Request}", param);
                    return NotFound(new { Message = "Report generation failed or no data found." });
                }

                // Convert the byte array to Base64 string
                string base64String = Convert.ToBase64String(reportFileBytes);

                // Output or use the Base64 string
                return Ok(new
                {
                    Message = "File generated.",
                    Data = base64String
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for {Request}", param);
                return NotFound(new
                {
                    Message = "File not found.",
                    Data = ""
                });
            }

        }

    }
}
