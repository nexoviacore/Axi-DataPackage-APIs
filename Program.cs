using AxExtend;
using AxExtend.Interface;
using AxiDataPackages.References;
using AxiDataPackages.Services;
using AxiDataPackages.Services.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Expressions;



//var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateBuilder(args);

string configuredLogPath = builder.Configuration["LogSettings:LogPath"];

string rootLogPath;

if (!string.IsNullOrWhiteSpace(configuredLogPath))
{
    rootLogPath = configuredLogPath;
}
else
{
    rootLogPath = Path.Combine
    (
        Directory.GetCurrentDirectory(),
        "logs"
    );
}


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.Map
    //(
    //    "LogFileName",
    //    "ApplicationStartUp",
    //    (logFileName, wt) =>
    //    {
    //        string dateFolder = Path.Combine
    //        (
    //            rootLogPath,
    //            "ExportPackage",
    //            DateTime.Now.ToString("yyyy-MM-dd")
    //        );

    //        if (!Directory.Exists(dateFolder))
    //        {
    //            Directory.CreateDirectory(dateFolder);
    //        }

    //        wt.File
    //        (
    //            Path.Combine(dateFolder, logFileName + ".log"),
    //            shared: true,
    //            outputTemplate:
    //            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    //        );
    //    }
    //)
    .WriteTo.Logger(lc => lc
   // .Filter.ByIncludingOnly("@Properties['TraceEnabled'] = true")
   //.Filter.ByIncludingOnly("TraceEnabled = true")
   .Filter.ByIncludingOnly("TraceEnabled")
    .WriteTo.Map
    (
        "LogFileName",
        "ApplicationStartUp",
        (logFileName, wt) =>
        {
            string dateFolder = Path.Combine
            (
                rootLogPath,
                DateTime.Now.ToString("yyyy-MM-dd")
            );

            if (!Directory.Exists(dateFolder))
            {
                Directory.CreateDirectory(dateFolder);
            }

            wt.File
            (
                Path.Combine(dateFolder, logFileName + ".log"),
                shared: true,
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
        }
    )
    )    
    .CreateLogger();

builder.Host.UseSerilog();

//builder.Host.UseSerilog();

// Add services to the container.

//builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();


builder.Services.AddHttpClient<AxARMApiHelperService>();

builder.Services.AddScoped<IAxExtend, AxExtend.AxExtend>();

builder.Services.AddScoped<AxDBService>();

builder.Services.AddScoped<PackageHelperService>();

builder.Services.AddScoped<ExportPackageService>();

builder.Services.AddScoped<AxPutConversion>();

builder.Services.AddScoped<PostgresScriptsLogic>();

builder.Services.AddScoped<ImportPackageHelperService>();

builder.Services.AddScoped<ImportPackageService>();

builder.Services.AddScoped<EncryptDecrypt>();

builder.Services.AddScoped<CustomPageFileCP>();

var app = builder.Build();

//app.Use(async (context, next) =>
//{
//    string requestId =
//        context.TraceIdentifier.Replace(":", "_");

//    //string logFileName =
//    //    $"ExportPackage_{DateTime.Now:HH-mm-ss}_{requestId}";

//    string logFileName =
//    $"ExportPackage_{DateTime.Now:HH-mm-ss-fff}";

//    using (LogContext.PushProperty("LogFileName", logFileName))
//    {
//        await next();
//    }
//});
app.Use(async (context, next) =>
{
    bool traceEnabled = false;

    string packageName = "UnknownPackage";

    string packageVersion = "UnknownVersion";

    string logType = "Application";

    if (context.Request.Path.Value?.Contains("ExportData") == true)
        logType = "ExportPackage";

    else if (context.Request.Path.Value?.Contains("ImportData") == true)
        logType = "ImportPackage";

    if (context.Request.Method == "POST")
    {
        context.Request.EnableBuffering();

        using StreamReader reader = new StreamReader
        (
            context.Request.Body,
            leaveOpen: true
        );

        string body = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                JObject json = JObject.Parse(body);

                traceEnabled = json.GetValue("trace", StringComparison.OrdinalIgnoreCase)?.Value<bool>() ?? false;

                packageName = json.GetValue("packageName", StringComparison.OrdinalIgnoreCase)?.ToString() ?? packageName;

                packageVersion = json.GetValue("packageVersion", StringComparison.OrdinalIgnoreCase)?.ToString() ?? packageVersion;
            }
            catch
            {
            }
        }
    }

    string logFileName =
        $"{logType}_{DateTime.Now:HH-mm-ss-fff}_({packageName}_{packageVersion})";

    using (LogContext.PushProperty("TraceEnabled", traceEnabled))
    using (LogContext.PushProperty("LogFileName", logFileName))
    {
        await next();
    }
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
