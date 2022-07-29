using Infrastructure;
using Serilog;
using Serilog.Events;
using Services;
using Services.Contracts;


var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
// ASP.NET Core infrastructure logs that are Information and below will be filtered out.
.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
// Picking up configuration from appsettings.json
.ReadFrom.Configuration(builder.Configuration)
.Enrich.FromLogContext()
// Creating Serilog logger object based on the configuration above. 
.CreateLogger();
try
{

    // clear all exsiting logging proveriders
    builder.Logging.ClearProviders();
    // Adding Serilog to ASP.net core's pipe line
    builder.Logging.AddSerilog(logger);
    // Adding App Inisghts to send custom events.
    builder.Services.AddApplicationInsightsTelemetry();

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddScoped<ITransactionService, TransactionService>();
    builder.Services.AddSingleton<BBBankContext>();

    var app = builder.Build();

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
}
catch (Exception ex)
{

    logger.Fatal(ex, "Error Starting BBBank API");
}
finally
{
    logger.Dispose();
}