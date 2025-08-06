using System.Reflection.Metadata;

namespace LPGxWebApi.ViewModel
{
    public class CommonVariable
    {
    }
}

public static class EncryptionSettings
{
    public const string Key = "1234567890123456"; // Must be 16 characters for AES-128
}

public static class EmailSettings
{
    //public const string FromAddress = "sujan.khan.fsl@gmail.com";
    public const string FromAddress = "info.seegmai@gmail.com"; 
    //public const string FromAddressPassword = "tbwtaaynvludaqxb";
    public const string FromAddressPassword = "oqsfcrzipdafppii"; 
    public const string SmtpServer = "smtp.gmail.com"; // Confirm this SMTP server
    public const string ProjectName = "LPGx";  // Confirm Project Name 
    public const int port = 587;  // Port for TLS/STARTTLS  587 or 465
}

public static class UrlLinks
{
    public const string webLink = "http://119.148.35.162:8006/"; // Live
}

public static class Setting
{
    public const string APIKEY = "AC63cd342e57c380aa6d9ff5fcbc2af9b5";
    public const string APISECRET = "435271e7e8441b111d216b9c55c05ceb";
    public const string PHONENUMBER = "+12702790007";
}
