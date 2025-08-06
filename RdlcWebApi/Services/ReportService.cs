using AspNetCore.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RdlcgWebApi.ReportDataTable;
using RdlcWebApi.Class;
using RdlcWebApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static RdlcWebApi.Controllers.LPGxReportController;
using static RdlcWebApi.Controllers.ReportController;

namespace RdlcWebApi.Services
{
    public interface IReportService
    {
        byte[] GenerateReportAsync(string reportName, string reportType);
        Task<byte[]> MonthlyBillReportAsync(string reportName, string reportType, ReportParamRequest param);
        Task<byte[]> MonthlyDueReportAsync(string reportName, string reportType, ReportParamRequest param);
        Task<byte[]> MonthlyCollectionReportAsync(string reportName, string reportType, ReportParamRequest param);
        Task<byte[]> OutstandingReportAsync(string reportName, string reportType, ReportParamRequest param);
        Task<byte[]> CustomerLedgerReportAsync(string reportName, string reportType, EndUserCommonRequest param);
        Task<byte[]> MovementReportAsync(string reportName, string reportType, EndUserCommonRequest param);
        Task<byte[]> CollectionReportAsync(string reportName, string reportType, EndUserCommonRequest param);
        Task<byte[]> PaymentReportAsync(string reportName, string reportType, EndUserCommonRequest param);
        Task<byte[]> IncomeReportAsync(string reportName, string reportType, ReportParamRequest param);
        Task<byte[]> IncomeSummaryReportAsync(string reportName, string reportType, ReportParamRequest param);
        Task<byte[]> ExpenditureReportAsync(string reportName, string reportType, ReportParamRequest param);
        Task<byte[]> ExpenditureSummaryReportAsync(string reportName, string reportType, ReportParamRequest param);
        Task<byte[]> BillMatrixDueReportAsync(string reportName, string reportType, EndUserCommonRequest param);
        Task<byte[]> BillMatrixCollectionReportAsync(string reportName, string reportType, EndUserCommonRequest param);
        Task<byte[]> ReceiptPaymentReportAsync(string reportName, string reportType, EndUserCommonRequest param);

        #region LPGx Report Methods
        Task<byte[]> SalesReturnReportAsync(string reportName, string reportType, LPGxReportParamRequest param);
        #endregion
    }

    public class ReportService : IReportService
    {
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        public ReportService(IConfiguration configuration, ApiService apiService)
        {
            _configuration = configuration;
            _apiService = apiService;
        }

        public byte[] GenerateReportAsync(string reportName, string reportType)
        {
            reportName = "TestReport"; // "YearlyDataReport";
            reportType = "pdf";

            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("utf-8");

            LocalReport report = new LocalReport(rdlcFilePath);

            // prepare data for report
            List<TestDto> dataList = new List<TestDto>();
            var data1 = new TestDto { ID = 1, Name = "Tomal", Address = "Khilkhet", Quantity = 25000 };
            var data2 = new TestDto { ID = 2, Name = "Goutom", Address = "Uttara", Quantity = 29000 };
            var data3 = new TestDto { ID = 3, Name = "Soumitro", Address = "Kolabagan", Quantity = 20000 };
            var data4 = new TestDto { ID = 4, Name = "Ashik", Address = "Rampura", Quantity = 45000 };
            var data5 = new TestDto { ID = 5, Name = "Morshed", Address = "Noya Paltan", Quantity = 65000 };
            dataList.Add(data1);
            dataList.Add(data2);
            dataList.Add(data3);
            dataList.Add(data4);
            dataList.Add(data5);
            report.AddDataSource("TestDataSet", dataList);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var result = report.Execute(GetRenderType(reportType), 1, parameters);

            return result.MainStream;
        }

        public async Task<byte[]> MonthlyBillReportAsync(string reportName, string reportType, ReportParamRequest param)
        {
            try
            {
                reportName = "MonthlyDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var monthlyData = await RetriveMonthlyBillRegister(param);

                List<MonthlyDto> dataList = monthlyData.Select((item, index) => new MonthlyDto
                {
                    Serial = index + 1, // Use the index for the Serial number
                    Apartment = item.APTNO,
                    Name = item.NAME,
                    January = item.JANUARY,
                    February = item.FEBRUARY,
                    March = item.MARCH,
                    April = item.APRIL,
                    May = item.MAY,
                    June = item.JUNE,
                    July = item.JULY,
                    August = item.AUGUST,
                    September = item.SEPTEMBER,
                    October = item.OCTOBER,
                    November = item.NOVEMBER,
                    December = item.DECEMBER,
                    Total = item.TOTAL, // Total amount for the year
                    Year = item.YEAR, // Year as a string
                    BillType = item.TXNTYPENAME // Transaction Type name as Bill Type
                }).ToList();

                // Add data source
                report.AddDataSource("YearlyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Monthly Bill Register"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("BillType", param.BILLTYPE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> MonthlyDueReportAsync(string reportName, string reportType, ReportParamRequest param)
        {
            try
            {
                reportName = "MonthlyDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var monthlyData = await RetriveMonthlyDueRegister(param);

                monthlyData = monthlyData.Where(d => d.TOTAL != 0).ToList();

                List<MonthlyDto> dataList = monthlyData.Select((item, index) => new MonthlyDto
                {
                    Serial = index + 1, // Use the index for the Serial number
                    Apartment = item.APTNO,
                    Name = item.NAME,
                    January = item.JANUARY,
                    February = item.FEBRUARY,
                    March = item.MARCH,
                    April = item.APRIL,
                    May = item.MAY,
                    June = item.JUNE,
                    July = item.JULY,
                    August = item.AUGUST,
                    September = item.SEPTEMBER,
                    October = item.OCTOBER,
                    November = item.NOVEMBER,
                    December = item.DECEMBER,
                    Total = item.TOTAL, // Total amount for the year
                    Year = item.YEAR, // Year as a string
                    BillType = item.TXNTYPENAME // Transaction Type name as Bill Type
                }).ToList();

                // Add data source
                report.AddDataSource("YearlyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Monthly Due Register"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("BillType", param.BILLTYPE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> MonthlyCollectionReportAsync(string reportName, string reportType, ReportParamRequest param)
        {
            try
            {
                reportName = "MonthlyDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var monthlyData = await RetriveMonthlyCollectionRegister(param);

                List<MonthlyDto> dataList = monthlyData.Select((item, index) => new MonthlyDto
                {
                    Serial = index + 1, // Use the index for the Serial number
                    Apartment = item.APTNO,
                    Name = item.NAME,
                    January = item.JANUARY,
                    February = item.FEBRUARY,
                    March = item.MARCH,
                    April = item.APRIL,
                    May = item.MAY,
                    June = item.JUNE,
                    July = item.JULY,
                    August = item.AUGUST,
                    September = item.SEPTEMBER,
                    October = item.OCTOBER,
                    November = item.NOVEMBER,
                    December = item.DECEMBER,
                    Total = item.TOTAL, // Total amount for the year
                    Year = item.YEAR, // Year as a string
                    BillType = item.TXNTYPENAME // Transaction Type name as Bill Type
                }).ToList();

                // Add data source
                report.AddDataSource("YearlyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Monthly Collection Register"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("BillType", param.BILLTYPE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> OutstandingReportAsync(string reportName, string reportType, ReportParamRequest param)
        {
            try
            {
                reportName = "OutstandingReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var monthlyData = await RetriveMonthlyDueRegister(param);

                monthlyData = monthlyData.Where(d => d.TOTAL != 0).ToList();

                List<MonthlyDto> dataList;

                if (param.BILLTYPE == "All" && param.APTCODE == "0")
                {
                    dataList = monthlyData
                        .GroupBy(x => new { x.APTNO, x.BLDCODE, x.APTCODE, x.NAME })
                        .Select((g, index) => new MonthlyDto
                        {
                            Serial = index + 1,
                            Apartment = g.Key.APTNO,
                            Name = g.Key.NAME,
                            January = g.Sum(x => x.JANUARY),
                            February = g.Sum(x => x.FEBRUARY),
                            March = g.Sum(x => x.MARCH),
                            April = g.Sum(x => x.APRIL),
                            May = g.Sum(x => x.MAY),
                            June = g.Sum(x => x.JUNE),
                            July = g.Sum(x => x.JULY),
                            August = g.Sum(x => x.AUGUST),
                            September = g.Sum(x => x.SEPTEMBER),
                            October = g.Sum(x => x.OCTOBER),
                            November = g.Sum(x => x.NOVEMBER),
                            December = g.Sum(x => x.DECEMBER),
                            Total = g.Sum(x => x.TOTAL),
                            Year = "", // Optional: set if you want earliest/latest
                            BillType = "All"
                        })
                        .OrderBy(x => x.Apartment)
                        .ToList();
                }
                else
                {
                    dataList = monthlyData
                        .GroupBy(x => new { x.APTNO, x.BLDCODE, x.APTCODE, x.NAME, x.TXNCODE, x.TXNTYPENAME })
                        .Select((g, index) => new MonthlyDto
                        {
                            Serial = index + 1,
                            Apartment = g.Key.APTNO,
                            Name = g.Key.NAME,
                            January = g.Sum(x => x.JANUARY),
                            February = g.Sum(x => x.FEBRUARY),
                            March = g.Sum(x => x.MARCH),
                            April = g.Sum(x => x.APRIL),
                            May = g.Sum(x => x.MAY),
                            June = g.Sum(x => x.JUNE),
                            July = g.Sum(x => x.JULY),
                            August = g.Sum(x => x.AUGUST),
                            September = g.Sum(x => x.SEPTEMBER),
                            October = g.Sum(x => x.OCTOBER),
                            November = g.Sum(x => x.NOVEMBER),
                            December = g.Sum(x => x.DECEMBER),
                            Total = g.Sum(x => x.TOTAL),
                            Year = "", // Optional: set if you want earliest/latest
                            BillType = g.Key.TXNTYPENAME,
                        })
                        .OrderBy(x => x.Apartment)
                        .ToList();
                }

                // Add data source
                report.AddDataSource("YearlyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Customer Outstanding"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("BillType", param.BILLTYPE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> CustomerLedgerReportAsync(string reportName, string reportType, EndUserCommonRequest param)
        {
            try
            {
                reportName = "CustomerLedgerReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var ledgerData = await RetriveCustomerLedger(param);

                List<LedgerDto> dataList = ledgerData.Select((item, index) => new LedgerDto
                {
                    AptNo = item.AptNo,
                    Name = item.Name,
                    Particular = item.Particular,
                    TxnType = item.TxnType,
                    TxnDate = item.TxnDate,
                    Bill = item.Bill,
                    Collection = item.Collection,
                    DueBalance = item.DueBalance
                }).ToList();

                // Add data source
                report.AddDataSource("LedgerDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>8.5in</PageWidth>" +     // Narrower width for portrait
                    "<PageHeight>11in</PageHeight>" +    // Taller height for portrait
                    "<MarginTop>0.5in</MarginTop>" +     // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" +   // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Customer Ledger"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> MovementReportAsync(string reportName, string reportType, EndUserCommonRequest param)
        {
            try
            {
                reportName = "MovementReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var ledgerData = await RetriveCustomerLedger(param);

                List<LedgerDto> dataList = ledgerData.Select((item, index) => new LedgerDto
                {
                    AptNo = item.AptNo,
                    Name = item.Name,
                    Particular = item.Particular,
                    TxnType = item.TxnType,
                    TxnDate = item.TxnDate,
                    Bill = item.Bill,
                    Collection = item.Collection,
                    DueBalance = item.DueBalance
                }).ToList();

                // Add data source
                report.AddDataSource("LedgerDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>8.5in</PageWidth>" +     // Narrower width for portrait
                    "<PageHeight>11in</PageHeight>" +    // Taller height for portrait
                    "<MarginTop>0.5in</MarginTop>" +     // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" +   // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Customer Movement"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> CollectionReportAsync(string reportName, string reportType, EndUserCommonRequest param)
        {
            try
            {
                reportName = "CollectionDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetriveCollectionReport(param);

                List<DailyDto> dataList;

                dataList = dailyData
                    .Select((g, index) => new DailyDto
                    {
                        SlNo = index + 1, // ✅ Serial number starts from 1
                        Date = g.TXNDATE,
                        Type = g.TXNCODE?.Replace(" COLLECTION", ""),
                        TxnRef = g.RECTXNID.ToString(),
                        VoucherRef = g.MODENAME,
                        Particular = g.REMARKS,
                        Amount = g.TXNAMT,
                        AptNo = g.APTNO
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("DailyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Collection Statement"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> PaymentReportAsync(string reportName, string reportType, EndUserCommonRequest param)
        {
            try
            {
                reportName = "PaymentDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetrivePaymentReport(param);

                List<DailyDto> dataList;

                dataList = dailyData
                    .Select((g, index) => new DailyDto
                    {
                        SlNo = index + 1, // ✅ Serial number starts from 1
                        Date = g.TXNDATE,
                        Type = g.TXNNAME?.Replace(" PAYMENT", ""),
                        TxnRef = g.REFNO.ToString(),
                        VoucherRef = "",
                        Particular = g.REMARKS,
                        Amount = g.TXNAMT,
                        AptNo = ""
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("DailyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Payment Statement"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", "-");
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> IncomeReportAsync(string reportName, string reportType, ReportParamRequest param)
        {
            try
            {
                reportName = "DailyDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetriveIncomeReport(param);

                List<DailyDto> dataList;

                dataList = dailyData
                    .Select((g, index) => new DailyDto
                    {
                        SlNo = index + 1, // ✅ Serial number starts from 1
                        Date = g.TXNDATE,
                        Type = g.TXNCODE?.Replace(" COLLECTION", ""),
                        TxnRef = g.REFNO,
                        VoucherRef = g.TXNID.ToString(),
                        Particular = g.REMARKS,
                        Amount = g.TXNAMT,
                        AptNo = ""
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("DailyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Income Statement"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", "-");
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);
                parameters.Add("HeadName", param.BILLTYPE.Trim().ToUpper());

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> IncomeSummaryReportAsync(string reportName, string reportType, ReportParamRequest param)
        {
            try
            {
                reportName = "SummaryDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetriveIncomeReport(param);

                var summaryData = dailyData
                                .GroupBy(c => new { c.TXNCODE, c.MODENAME })
                                .Select(g => new CollectionStatementResponse
                                {
                                    TXNCODE = g.Key.TXNCODE,
                                    MODENAME = g.Key.MODENAME,
                                    TXNAMT = g.Sum(x => x.TXNAMT)
                                })
                                .OrderByDescending(x => x.TXNAMT)
                                .ToList();

                List<DailyDto> dataList;

                dataList = summaryData
                    .Select((g, index) => new DailyDto
                    {
                        SlNo = index + 1, // ✅ Serial number starts from 1
                        Date = System.DateTime.Now,
                        Type = g.TXNCODE?.Replace(" COLLECTION", ""),
                        TxnRef = "",
                        VoucherRef = "",
                        Particular = g.MODENAME,
                        Amount = g.TXNAMT,
                        AptNo = ""
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("DailyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Income Summary"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", "-");
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);
                parameters.Add("HeadName", param.BILLTYPE.Trim().ToUpper());

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> ExpenditureReportAsync(string reportName, string reportType, ReportParamRequest param)
        {
            try
            {
                reportName = "DailyDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetriveExpenditureReport(param);

                List<DailyDto> dataList;

                dataList = dailyData
                    .Select((g, index) => new DailyDto
                    {
                        SlNo = index + 1, // ✅ Serial number starts from 1
                        Date = g.TXNDATE,
                        Type = g.TXNNAME?.Replace(" PAYMENT", ""),
                        TxnRef = g.REFNO,
                        VoucherRef = g.TXNID.ToString(),
                        Particular = g.REMARKS,
                        Amount = g.TXNAMT,
                        AptNo = ""
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("DailyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Expenditure Statement"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", "-");
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);
                parameters.Add("HeadName", param.BILLTYPE.Trim().ToUpper());

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> ExpenditureSummaryReportAsync(string reportName, string reportType, ReportParamRequest param)
        {
            try
            {
                reportName = "SummaryDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetriveExpenditureReport(param);

                var summaryData = dailyData
                                .GroupBy(c => new { c.TXNNAME, c.TXNMODE })
                                .Select(g => new PaymentStatementResponse
                                {
                                    TXNNAME = g.Key.TXNNAME,
                                    TXNMODE = g.Key.TXNMODE,
                                    TXNAMT = g.Sum(x => x.TXNAMT)
                                })
                                .OrderByDescending(x => x.TXNAMT)
                                .ToList();

                List<DailyDto> dataList;

                dataList = summaryData
                    .Select((g, index) => new DailyDto
                    {
                        SlNo = index + 1, // ✅ Serial number starts from 1
                        Date = System.DateTime.Now,
                        Type = g.TXNNAME?.Replace(" PAYMENT", ""),
                        TxnRef = "",
                        VoucherRef = "",
                        Particular = g.TXNMODE,
                        Amount = g.TXNAMT,
                        AptNo = ""
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("DailyDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Expenditure Summary"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", "-");
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);
                parameters.Add("HeadName", param.BILLTYPE.Trim().ToUpper());

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> BillMatrixDueReportAsync(string reportName, string reportType, EndUserCommonRequest param)
        {
            try
            {
                reportName = "BillDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetriveBillMatrixDueReport(param);

                List<MatrixDto> dataList;

                dataList = dailyData
                    .Select((g, index) => new MatrixDto
                    {
                        SlNo = index + 1, // ✅ Serial number starts from 1
                        AptNo = g.APTNO,
                        Name = g.NAME,
                        Param1Header = g.PARAM1HEADER,
                        Param1Value = g.PARAM1VALUE,
                        Param2Header = g.PARAM2HEADER,
                        Param2Value = g.PARAM2VALUE,
                        Param3Header = g.PARAM3HEADER,
                        Param3Value = g.PARAM3VALUE,
                        Param4Header = g.PARAM4HEADER,
                        Param4Value = g.PARAM4VALUE,
                        Param5Header = g.PARAM5HEADER,
                        Param5Value = g.PARAM5VALUE
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("BillDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Due Bill Matrix"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> BillMatrixCollectionReportAsync(string reportName, string reportType, EndUserCommonRequest param)
        {
            try
            {
                reportName = "BillDataReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetriveBillMatrixCollectionReport(param);

                List<MatrixDto> dataList;

                dataList = dailyData
                    .Select((g, index) => new MatrixDto
                    {
                        SlNo = index + 1, // ✅ Serial number starts from 1
                        AptNo = g.APTNO,
                        Name = g.NAME,
                        Param1Header = g.PARAM1HEADER,
                        Param1Value = g.PARAM1VALUE,
                        Param2Header = g.PARAM2HEADER,
                        Param2Value = g.PARAM2VALUE,
                        Param3Header = g.PARAM3HEADER,
                        Param3Value = g.PARAM3VALUE,
                        Param4Header = g.PARAM4HEADER,
                        Param4Value = g.PARAM4VALUE,
                        Param5Header = g.PARAM5HEADER,
                        Param5Value = g.PARAM5VALUE
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("BillDataSet", dataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Collection Bill Matrix"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", param.APTCODE);
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> ReceiptPaymentReportAsync(string reportName, string reportType, EndUserCommonRequest param)
        {
            try
            {
                reportName = "ReceiptPaymentReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BindingInfo
                var buildingInfo = await RetriveBuildingInfo(param.BLDCODE);
                var buildingName = buildingInfo?.BldName ?? "";

                // prepare data for report
                var dailyData = await RetriveReceiptPaymentReport(param);

                var ReceiptDataList = dailyData
                    .Where(predicate => predicate.MAINHEADYN == "N" && predicate.BALANCE != 0 && predicate.ALIECODE == "I")
                    .Select((g, index) => new ReceiptPaymentResponse
                    {
                        ACCNAME = g.ACCNAME,
                        ALIECODE = g.ALIECODE,
                        BALANCE = g.BALANCE
                    })
                    .ToList();

                var PaymentDataList = dailyData
                    .Where(predicate => predicate.MAINHEADYN == "N" && predicate.BALANCE != 0 && predicate.ALIECODE == "E")
                    .Select((g, index) => new ReceiptPaymentResponse
                    {
                        ACCNAME = g.ACCNAME,
                        ALIECODE = g.ALIECODE,
                        BALANCE = g.BALANCE
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("ReceiptDataSet", ReceiptDataList);
                report.AddDataSource("PaymentDataSet", PaymentDataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Receipt Payment Report"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", "-");
                parameters.Add("StartDate", param.STARTDATE.ToString());
                parameters.Add("EndDate", param.ENDDATE.ToString());
                parameters.Add("BuildingName", buildingName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }

        public async Task<byte[]> SalesReturnReportAsync(string reportName, string reportType, LPGxReportParamRequest param)
        {
            try
            {
                reportName = "SalesReturnStockReport";
                reportType = "pdf";

                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("RdlcWebApi.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("utf-8");

                LocalReport report = new LocalReport(rdlcFilePath);

                //Load BranchInfo
                var branchInfo = await RetriveLPGxBranchInfo(param.BRANCHCODE);
                var branchName = branchInfo?.BldName ?? "";

                // prepare data for report
                var salesData = await RetriveSalesStockReport(param);
                var returnData = await RetriveReturnStockReport(param);

                var salesDataList = salesData
                    .Select((g, index) => new SalesResponseDto
                    {
                        NAME = g.Name,
                        NAV = g.NAV,
                        DUB = g.DUB,
                        UNI = g.UNI,
                        TOTAL = g.TOT,
                        PKG = g.PKG,
                        CD = g.CD,
                        CP = g.CP,
                        DT = g.DT,
                        TOTAL1 = g.TOTAL,
                    })
                    .ToList();

                var returnDataList = returnData
                    .Select((g, index) => new ReturnResponseDto
                    {
                        NAME = g.Name,
                        NAV = g.NAV,
                        DUB = g.DUB,
                        UNI = g.UNI,
                        OME = g.OME,
                        MAX = g.MAX,
                        FRESH = g.FRESH,
                        KH = g.KH,
                        BM = g.BM,
                        DEL = g.DEL,
                        JAM = g.JAM,
                        BEX = g.BEX,
                        BEN = g.BEN,
                        SEN = g.SEN,
                        BAS = g.BAS,
                        JMI = g.JMI,
                        GGAS = g.GGAS,
                        TMSS = g.TMSS,
                        AY = g.AY,
                        ETC = g.ETC,
                        TOTAL2 = g?.NAV + g?.DUB + g?.UNI + g?.OME + g?.MAX + g?.FRESH + g?.KH + g?.BM + g?.DEL + g?.JAM + g?.BEX + g?.BEN + g?.SEN + g?.BAS + g?.JMI + g?.GGAS + g?.TMSS + g?.AY + g?.ETC,
                    })
                    .ToList();

                // Add data source
                report.AddDataSource("SalesDataSet", salesDataList);
                report.AddDataSource("ReturnStockDataSet", returnDataList);

                // Add the current time as a parameter
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Current time in your desired format

                // Define DeviceInfo for landscape orientation and margins
                string deviceInfo =
                    "<DeviceInfo>" +
                    "<PageWidth>11in</PageWidth>" +  // Set page width for landscape
                    "<PageHeight>8.5in</PageHeight>" + // Set page height for landscape
                    "<MarginTop>0.5in</MarginTop>" +  // Set top margin
                    "<MarginLeft>0.5in</MarginLeft>" + // Set left margin
                    "<MarginRight>0.5in</MarginRight>" + // Set right margin
                    "<MarginBottom>0.5in</MarginBottom>" + // Set bottom margin
                    "</DeviceInfo>";

                // Render the report to a memory stream as PDF
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("ReportTitle", "Daily Stock Report"); // Add the report title as a parameter
                parameters.Add("PrintTime", currentTime); // Add the print date as a parameter
                parameters.Add("AptNo", "-");
                parameters.Add("StartDate", param.RPTDATE.ToString());
                parameters.Add("EndDate", param.RPTDATE.ToString());
                parameters.Add("BuildingName", branchName);

                var result = report.Execute(GetRenderType(reportType), 1, parameters, deviceInfo);

                return result.MainStream;
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                // Log the exception or return a meaningful error result.
                throw new Exception("An error occurred while generating the report.", ex);
            }
        }


        public async Task<BuildingResponse> RetriveBuildingInfo(string bldCode)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}BuidingInfos/{bldCode}";

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into CustomerLedgerResponse
                var buildingResponse = JsonConvert.DeserializeObject<BuildingResponse>(response);
                if (buildingResponse == null)
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return buildingResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<MonthlyRegisterResponse>> RetriveMonthlyBillRegister(ReportParamRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/monthlyregister/bill";

                // Create the request object
                var request = new EndUserCommonRequest
                {
                    STARTDATE = param.STARTDATE,
                    ENDDATE = param.ENDDATE,
                    BLDCODE = param.BLDCODE,
                    APTCODE = param.APTCODE,
                    SIGNONNAME = param.SIGNONNAME
                };

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into MonthlyRegisterResponse
                var MonthlyBillRegisterResponse = JsonConvert.DeserializeObject<List<MonthlyRegisterResponse>>(response);

                if (MonthlyBillRegisterResponse == null || !MonthlyBillRegisterResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                // Assign the data to MonthlyDueRegister based on billType
                List<MonthlyRegisterResponse> filteredMonthlyBillRegister;
                if (param.BILLTYPE.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    filteredMonthlyBillRegister = MonthlyBillRegisterResponse
                        .OrderBy(x => x.TXNCODE)
                        .ThenBy(x => x.APTNO)
                        .ThenBy(x => x.YEAR)
                        .ToList();
                }
                else
                {
                    filteredMonthlyBillRegister = MonthlyBillRegisterResponse
                        .Where(b => b.TXNTYPENAME.Equals(param.BILLTYPE, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => x.TXNCODE)
                        .ThenBy(x => x.APTNO)
                        .ThenBy(x => x.YEAR)
                        .ToList();
                }

                return filteredMonthlyBillRegister;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<MonthlyRegisterResponse>> RetriveMonthlyDueRegister(ReportParamRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/monthlyregister/due";

                // Create the request object
                var request = new EndUserCommonRequest
                {
                    STARTDATE = param.STARTDATE,
                    ENDDATE = param.ENDDATE,
                    BLDCODE = param.BLDCODE,
                    APTCODE = param.APTCODE,
                    SIGNONNAME = param.SIGNONNAME
                };

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into MonthlyRegisterResponse
                var MonthlyDueRegisterResponse = JsonConvert.DeserializeObject<List<MonthlyRegisterResponse>>(response);

                if (MonthlyDueRegisterResponse == null || !MonthlyDueRegisterResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                // Assign the data to MonthlyDueRegister based on billType
                List<MonthlyRegisterResponse> filteredMonthlyDueRegister;
                if (param.BILLTYPE.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    filteredMonthlyDueRegister = MonthlyDueRegisterResponse
                        .OrderBy(x => x.TXNCODE)
                        .ThenBy(x => x.APTNO)
                        .ThenBy(x => x.YEAR)
                        .ToList();
                }
                else
                {
                    filteredMonthlyDueRegister = MonthlyDueRegisterResponse
                        .Where(b => b.TXNTYPENAME.Equals(param.BILLTYPE, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => x.TXNCODE)
                        .ThenBy(x => x.APTNO)
                        .ThenBy(x => x.YEAR)
                        .ToList();
                }

                return filteredMonthlyDueRegister;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<MonthlyRegisterResponse>> RetriveMonthlyCollectionRegister(ReportParamRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/monthlyregister/collection";

                // Create the request object
                var request = new EndUserCommonRequest
                {
                    STARTDATE = param.STARTDATE,
                    ENDDATE = param.ENDDATE,
                    BLDCODE = param.BLDCODE,
                    APTCODE = param.APTCODE,
                    SIGNONNAME = param.SIGNONNAME
                };

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into MonthlyRegisterResponse
                var MonthlyDueRegisterResponse = JsonConvert.DeserializeObject<List<MonthlyRegisterResponse>>(response);

                if (MonthlyDueRegisterResponse == null || !MonthlyDueRegisterResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                // Assign the data to MonthlyDueRegister based on billType
                List<MonthlyRegisterResponse> filteredMonthlyDueRegister;
                if (param.BILLTYPE.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    filteredMonthlyDueRegister = MonthlyDueRegisterResponse
                        .OrderBy(x => x.TXNCODE)
                        .ThenBy(x => x.APTNO)
                        .ThenBy(x => x.YEAR)
                        .ToList();
                }
                else
                {
                    filteredMonthlyDueRegister = MonthlyDueRegisterResponse
                        .Where(b => b.TXNTYPENAME.Equals(param.BILLTYPE, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => x.TXNCODE)
                        .ThenBy(x => x.APTNO)
                        .ThenBy(x => x.YEAR)
                        .ToList();
                }

                return filteredMonthlyDueRegister;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<CustomerLedgerResponse>> RetriveCustomerLedger(EndUserCommonRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/customer/ledger";

                // Create the request object
                var request = new EndUserCommonRequest
                {
                    STARTDATE = param.STARTDATE,
                    ENDDATE = param.ENDDATE,
                    BLDCODE = param.BLDCODE,
                    APTCODE = param.APTCODE,
                    SIGNONNAME = param.SIGNONNAME
                };

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into CustomerLedgerResponse
                var customerLedgerResponse = JsonConvert.DeserializeObject<List<CustomerLedgerResponse>>(response);

                if (customerLedgerResponse == null || !customerLedgerResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                // Assign the data to MonthlyDueRegister based on billType
                List<CustomerLedgerResponse> filteredRegister;
                filteredRegister = customerLedgerResponse
                    .OrderBy(x => x.AptNo)
                    .ThenBy(x => x.TxnDate)
                    .ToList();

                return filteredRegister;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<CollectionStatementResponse>> RetriveCollectionReport(EndUserCommonRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/collectionlist";

                // Create the request object
                var request = new EndUserCommonRequest
                {
                    STARTDATE = param.STARTDATE,
                    ENDDATE = param.ENDDATE,
                    BLDCODE = param.BLDCODE,
                    APTCODE = param.APTCODE,
                    SIGNONNAME = param.SIGNONNAME
                };

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into CollectionStatementResponse
                var collectionStatementResponse = JsonConvert.DeserializeObject<List<CollectionStatementResponse>>(response);
                if (collectionStatementResponse == null || !collectionStatementResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return collectionStatementResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<PaymentStatementResponse>> RetrivePaymentReport(EndUserCommonRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/paymentlist";

                // Create the request object
                ReportCommonRequest request = new ReportCommonRequest();
                request.STARTDATE = param.STARTDATE;
                request.ENDDATE = param.ENDDATE;
                request.BLDCODE = param.BLDCODE;
                request.SIGNONNAME = param.SIGNONNAME;

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into paymentStatementResponse
                var paymentStatementResponse = JsonConvert.DeserializeObject<List<PaymentStatementResponse>>(response);
                if (paymentStatementResponse == null || !paymentStatementResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return paymentStatementResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<CollectionStatementResponse>> RetriveIncomeReport(ReportParamRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/collectionlist";

                // Create the request object
                ReportCommonRequest request = new ReportCommonRequest();
                request.STARTDATE = param.STARTDATE;
                request.ENDDATE = param.ENDDATE;
                request.BLDCODE = param.BLDCODE;
                request.SIGNONNAME = param.SIGNONNAME;

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into incomeStatementResponse
                var incomeStatementResponse = JsonConvert.DeserializeObject<List<CollectionStatementResponse>>(response);
                if (incomeStatementResponse == null || !incomeStatementResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                if (param.BILLTYPE == null || param.BILLTYPE == "All")
                {
                    // If BILLTYPE is null or "All", return the full list
                    return incomeStatementResponse.OrderBy(x => x.TXNDATE).ThenBy(x => x.REFNO).ToList();
                }

                var filteredIncomeStatementResponse = incomeStatementResponse
                    .Where(x => x.TXNCODE != null && x.TXNCODE.Trim() == param.BILLTYPE.Trim())
                    .OrderBy(x => x.TXNDATE)
                    .ThenBy(x => x.REFNO)
                    .ToList();

                return filteredIncomeStatementResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<PaymentStatementResponse>> RetriveExpenditureReport(ReportParamRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/paymentlist";

                // Create the request object
                ReportCommonRequest request = new ReportCommonRequest();
                request.STARTDATE = param.STARTDATE;
                request.ENDDATE = param.ENDDATE;
                request.BLDCODE = param.BLDCODE;
                request.SIGNONNAME = param.SIGNONNAME;

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into paymentStatementResponse
                var paymentStatementResponse = JsonConvert.DeserializeObject<List<PaymentStatementResponse>>(response);
                if (paymentStatementResponse == null || !paymentStatementResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                if (param.BILLTYPE == null || param.BILLTYPE == "All")
                {
                    // If BILLTYPE is null or "All", return the full list
                    return paymentStatementResponse.OrderBy(x => x.TXNDATE).ThenBy(x => x.REFNO).ToList();
                }

                var filteredPaymentStatementResponse = paymentStatementResponse
                    .Where(x => x.TXNNAME != null && x.TXNNAME.Trim() == param.BILLTYPE.Trim())
                    .OrderBy(x => x.TXNDATE)
                    .ThenBy(x => x.REFNO)
                    .ToList();

                return filteredPaymentStatementResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<BillMatrixResponse>> RetriveBillMatrixDueReport(EndUserCommonRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/billMatrix/due";

                // Create the request object
                EndUserCommonRequest request = new EndUserCommonRequest();
                request.STARTDATE = param.STARTDATE;
                request.ENDDATE = param.ENDDATE;
                request.BLDCODE = param.BLDCODE;
                request.APTCODE = param.APTCODE;
                request.SIGNONNAME = param.SIGNONNAME;

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into billMatrixResponse
                var billMatrixResponse = JsonConvert.DeserializeObject<List<BillMatrixResponse>>(response);
                if (billMatrixResponse == null || !billMatrixResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return billMatrixResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<BillMatrixResponse>> RetriveBillMatrixCollectionReport(EndUserCommonRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/billMatrix/collection";

                // Create the request object
                EndUserCommonRequest request = new EndUserCommonRequest();
                request.STARTDATE = param.STARTDATE;
                request.ENDDATE = param.ENDDATE;
                request.BLDCODE = param.BLDCODE;
                request.APTCODE = param.APTCODE;
                request.SIGNONNAME = param.SIGNONNAME;

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into billMatrixResponse
                var billMatrixResponse = JsonConvert.DeserializeObject<List<BillMatrixResponse>>(response);
                if (billMatrixResponse == null || !billMatrixResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return billMatrixResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<TrialBalanceResponse>> RetriveReceiptPaymentReport(EndUserCommonRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}Report/trialbalance";

                // Create the request object
                ReportCommonRequest request = new ReportCommonRequest();
                request.STARTDATE = param.STARTDATE;
                request.ENDDATE = param.ENDDATE;
                request.BLDCODE = param.BLDCODE;
                request.SIGNONNAME = param.SIGNONNAME;

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into TrialBalanceResponse
                var billMatrixResponse = JsonConvert.DeserializeObject<List<TrialBalanceResponse>>(response);
                if (billMatrixResponse == null || !billMatrixResponse.Any())
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return billMatrixResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }


        #region LPGx Reports
        public async Task<BranchResponse> RetriveLPGxBranchInfo(string branchCode)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:LPGxBaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}BranchInfos/{branchCode}";

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception("No data received from the API.");
                }

                // Handle case where no records are found
                if (response.Contains("No record found"))
                {
                    throw new Exception("No records found.");
                }

                // Deserialize the response into BranchResponse
                var buildingResponse = JsonConvert.DeserializeObject<BranchResponse>(response);
                if (buildingResponse == null)
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return buildingResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<ReturnReportResponse>> RetriveReturnStockReport(LPGxReportParamRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:LPGxBaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}ReturnInfos/Date/{param.BRANCHCODE}/{param.RPTDATE?.ToString("yyyy-MM-dd")}";

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    throw new Exception("No data received from the API.");
                }

                // Deserialize the response to SaleResponse
                var jsonObject = JObject.Parse(response);
                var ReturnStockJsonArray = jsonObject["data"].ToString(); // Extract the "data" array as a string
                var ReturnStockResponse = JsonConvert.DeserializeObject<List<ReturnReportResponse>>(ReturnStockJsonArray);
                if (ReturnStockResponse == null)
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return ReturnStockResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        public async Task<List<SaleReportResponse>> RetriveSalesStockReport(LPGxReportParamRequest param)
        {
            try
            {
                // Get the API BaseUrl from configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:LPGxBaseUrl");

                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Log and return bad request if BaseUrl is not configured
                    throw new Exception("API base URL is not configured.");
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}SaleInfos/Date/{param.BRANCHCODE}/{param.RPTDATE?.ToString("yyyy-MM-dd")}";

                // Call the API using the ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    throw new Exception("No data received from the API.");
                }

                // Deserialize the response to SaleResponse
                var jsonObject = JObject.Parse(response);
                var SalesJsonArray = jsonObject["data"].ToString(); // Extract the "data" array as a string
                var SaleResponse = JsonConvert.DeserializeObject<List<SaleReportResponse>>(SalesJsonArray);
                if (SaleResponse == null)
                {
                    throw new Exception("Failed to retrieve valid data.");
                }

                return SaleResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while processing your request: {ex.Message}");
            }
        }

        #endregion

        private RenderType GetRenderType(string reportType)
        {
            var renderType = RenderType.Pdf;

            switch (reportType.ToUpper())
            {
                default:
                case "PDF":
                    renderType = RenderType.Pdf;
                    break;
                case "XLS":
                    renderType = RenderType.Excel;
                    break;
                case "WORD":
                    renderType = RenderType.Word;
                    break;
            }

            return renderType;
        }

    }
}
