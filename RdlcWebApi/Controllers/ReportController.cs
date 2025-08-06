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
    public class ReportController : ControllerBase
    {
        private IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("{reportName}/{reportType}/{lang}")]
        public ActionResult Get(string reportName, string reportType, string lang)
        {
            var reportNameWithLang = reportName;
            var reportFileByteString = _reportService.GenerateReportAsync(reportNameWithLang, reportType);
            return File(reportFileByteString, MediaTypeNames.Application.Octet, getReportName(reportNameWithLang, reportType));
        }

        private string getReportName(string reportName, string reportType)
        {
            var outputFileName = reportName + ".pdf";

            switch (reportType.ToUpper())
            {
                default:
                case "PDF":
                    outputFileName = reportName + ".pdf";
                    break;
                case "XLS":
                    outputFileName = reportName + ".xls";
                    break;
                case "WORD":
                    outputFileName = reportName + ".doc";
                    break;
            }

            return outputFileName;
        }

        [HttpGet("{reportName}/{reportType}")]
        public ActionResult Get(string reportName, string reportType)
        {
            try
            {
                var reportNameWithLang = reportName;

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = _reportService.GenerateReportAsync(reportNameWithLang, reportType);

                if (reportFileBytes == null || reportFileBytes.Length == 0)
                {
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
                return NotFound(new
                {
                    Message = "File not found.",
                    Data = ""
                });
            }
        }

        public class ReportCommonRequest
        {
            public DateTime? STARTDATE { get; set; }
            public DateTime? ENDDATE { get; set; }
            public string? BLDCODE { get; set; }
            public string? SIGNONNAME { get; set; }
        }

        public class EndUserCommonRequest
        {
            public DateTime? STARTDATE { get; set; }
            public DateTime? ENDDATE { get; set; }
            public string? BLDCODE { get; set; }
            public string? APTCODE { get; set; }
            public string? SIGNONNAME { get; set; }

        }

        public class ReportParamRequest
        {
            public DateTime? STARTDATE { get; set; }
            public DateTime? ENDDATE { get; set; }
            public string? BLDCODE { get; set; }
            public string? APTCODE { get; set; }
            public string? SIGNONNAME { get; set; }
            public string? BILLTYPE { get; set; }

        }

        public class MonthlyRegisterResponse
        {
            public string? BLDCODE { get; set; }
            public string? APTCODE { get; set; }
            public string? APTNO { get; set; }
            public string? NAME { get; set; }
            public string? TXNTYPENAME { get; set; }
            public string? TXNCODE { get; set; }
            public string? YEAR { get; set; }
            public decimal JANUARY { get; set; }
            public decimal FEBRUARY { get; set; }
            public decimal MARCH { get; set; }
            public decimal APRIL { get; set; }
            public decimal MAY { get; set; }
            public decimal JUNE { get; set; }
            public decimal JULY { get; set; }
            public decimal AUGUST { get; set; }
            public decimal SEPTEMBER { get; set; }
            public decimal OCTOBER { get; set; }
            public decimal NOVEMBER { get; set; }
            public decimal DECEMBER { get; set; }
            public decimal TOTAL { get; set; }
        }

        public class CustomerLedgerResponse
        {
            public string AptNo { get; set; }               // Apartment number
            public string Name { get; set; }               // Apartment number
            public string Particular { get; set; }         // Transaction type name (e.g., BILL, COLLECTION, OPENING B/F)
            public string TxnType { get; set; }              // BILL, COLLECTION, or OPENING
            public DateTime TxnDate { get; set; }           // Transaction date
            public decimal Bill { get; set; }               // Amount if it is a BILL
            public decimal Collection { get; set; }         // Amount if it is a COLLECTION
            public decimal DueBalance { get; set; }         // Running balance
        }

        public class BuildingResponse
        {
            [JsonProperty("bldcode")]
            public string BldCode { get; set; }

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

        public class CollectionStatementResponse
        {
            public decimal TXNID { get; set; }
            public decimal RECTXNID { get; set; }
            public DateTime TXNDATE { get; set; }
            public string TXNCODE { get; set; }
            public decimal TXNAMT { get; set; }
            public string REFNO { get; set; }
            public string REMARKS { get; set; }
            public string USERNAME { get; set; }
            public string CONTACTNO { get; set; }
            public string MODENAME { get; set; }
            public string APTNO { get; set; }
        }

        public class PaymentStatementResponse
        {
            public decimal TXNID { get; set; }
            public DateTime TXNDATE { get; set; }
            public decimal TXNAMT { get; set; }
            public string? TXNMODE { get; set; }
            public string? REFNO { get; set; }
            public string? REMARKS { get; set; }
            public string? USERNAME { get; set; }
            public string? CONTACTNO { get; set; }
            public string? BLDNAME { get; set; }
            public string? TXNNAME { get; set; }
        }

        public class BillMatrixResponse
        {
            public string BLDCODE { get; set; }
            public string APTCODE { get; set; }
            public string APTNO { get; set; }
            public string NAME { get; set; }
            public string PARAM1HEADER { get; set; }
            public decimal PARAM1VALUE { get; set; }
            public string PARAM2HEADER { get; set; }
            public decimal PARAM2VALUE { get; set; }
            public string PARAM3HEADER { get; set; }
            public decimal PARAM3VALUE { get; set; }
            public string PARAM4HEADER { get; set; }
            public decimal PARAM4VALUE { get; set; }
            public string PARAM5HEADER { get; set; }
            public decimal PARAM5VALUE { get; set; }
            public decimal TOTAL { get; set; }
        }

        public class TrialBalanceResponse
        {
            public string ACCOUNTNO { get; set; }
            public string BLDCODE { get; set; }
            public decimal SEQUENCENO { get; set; }
            public string ACCNAME { get; set; }
            public string HEADCODE { get; set; }
            public string MAINHEADYN { get; set; }
            public string ALIECODE { get; set; }
            public decimal BALANCE { get; set; }

        }

        public class ReceiptPaymentResponse
        {
            public string ACCNAME { get; set; }
            public string ALIECODE { get; set; }
            public decimal BALANCE { get; set; }
        }


        // POST: api/Report/MonthlyBillRegister
        [HttpPost("MonthlyBillRegister")]
        public async Task<ActionResult> MonthlyBillRegisterReport([FromBody] ReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "MonthlyDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.MonthlyBillReportAsync(reportNameWithLang, reportType, param);

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
                    Data = ex.Message.ToString()
                });
            }
        }

        // POST: api/Report/MonthlyDueRegister
        [HttpPost("MonthlyDueRegister")]
        public async Task<ActionResult> MonthlyDueRegisterReport([FromBody] ReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "MonthlyDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.MonthlyDueReportAsync(reportNameWithLang, reportType, param);

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
                    Data = ex.Message.ToString()
                });
            }
        }

        // POST: api/Report/MonthlyCollectionRegister
        [HttpPost("MonthlyCollectionRegister")]
        public async Task<ActionResult> MonthlyCollectionRegisterReport([FromBody] ReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "MonthlyDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.MonthlyCollectionReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/CustomerOutstanding
        [HttpPost("CustomerOutstanding")]
        public async Task<ActionResult> CustomerOutstandingReport([FromBody] ReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "OutstandingReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.OutstandingReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/CustomerLedger
        [HttpPost("CustomerLedger")]
        public async Task<ActionResult> CustomerLedgerReport([FromBody] EndUserCommonRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "CustomerLedgerReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.CustomerLedgerReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/CustomerMovement
        [HttpPost("CustomerMovement")]
        public async Task<ActionResult> CustomerMovementReport([FromBody] EndUserCommonRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "MovementReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.MovementReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/DailyCollectionRegister
        [HttpPost("DailyCollectionRegister")]
        public async Task<ActionResult> DailyCollectionRegisterReport([FromBody] EndUserCommonRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "CollectionDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.CollectionReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/DailyPaymentRegister
        [HttpPost("DailyPaymentRegister")]
        public async Task<ActionResult> DailyPaymentRegisterReport([FromBody] EndUserCommonRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "PaymentDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.PaymentReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/DailyIncomeRegister
        [HttpPost("DailyIncomeRegister")]
        public async Task<ActionResult> DailyIncomeRegisterReport([FromBody] ReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "DailyDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.IncomeReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/IncomeSummaryRegister
        [HttpPost("IncomeSummaryRegister")]
        public async Task<ActionResult> IncomeSummaryRegisterReport([FromBody] ReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "SummaryDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.IncomeSummaryReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/DailyExpenditureRegister
        [HttpPost("DailyExpenditureRegister")]
        public async Task<ActionResult> DailyExpenditureRegisterReport([FromBody] ReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "DailyDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.ExpenditureReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/ExpenditureSummaryRegister
        [HttpPost("ExpenditureSummaryRegister")]
        public async Task<ActionResult> ExpenditureSummaryRegisterReport([FromBody] ReportParamRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "SummaryDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.ExpenditureSummaryReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/BillMatrixCollection
        [HttpPost("BillMatrixCollection")]
        public async Task<ActionResult> BillMatrixCollectionReport([FromBody] EndUserCommonRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "BillDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.BillMatrixCollectionReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/BillMatrixDue
        [HttpPost("BillMatrixDue")]
        public async Task<ActionResult> BillMatrixDueReport([FromBody] EndUserCommonRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "BillDataReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.BillMatrixDueReportAsync(reportNameWithLang, reportType, param);

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

        // POST: api/Report/ReceiptPaymentRegister
        [HttpPost("ReceiptPaymentRegister")]
        public async Task<ActionResult> ReceiptPaymentRegisterReport([FromBody] EndUserCommonRequest param)
        {
            // Log request details
            _logger.LogInformation("Request received: {Request}", param);

            try
            {
                var reportNameWithLang = "ReceiptPaymentReport";
                var reportType = "pdf";

                // Generate the report asynchronously and get the byte array
                byte[] reportFileBytes = await _reportService.ReceiptPaymentReportAsync(reportNameWithLang, reportType, param);

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
