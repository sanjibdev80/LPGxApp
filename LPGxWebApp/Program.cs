using LPGxWebApp.Class;
using LPGxWebApp.Model;

var builder = WebApplication.CreateBuilder(args);

// Register ApiSettings configuration (assuming it's in appsettings.json)
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Register HttpClient and ApiService
builder.Services.AddHttpClient<ApiService>(client =>
{
    // You can set the base URL from the ApiSettings configuration here
    var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>();
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set a timeout duration for session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddRazorPages();

// Register ApiService for dependency injection (use AddSingleton if it’s not stateful)
builder.Services.AddSingleton<ApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

// Redirect root URL ("/") to "/Login"
app.MapGet("/", () => Results.Redirect("/Login"));

// Fallback to /Login page
app.MapFallbackToPage("/Login/index");

//app.MapGet("/", () => Results.Redirect("/Dashboard"));

app.Run();
