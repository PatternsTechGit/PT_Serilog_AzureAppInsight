# Serilog and Azure Application Insight

## What is Serilog and Azure Application Insight?

Serilog is one of the l**ogging framework**. Logging is the information that tells us what is happening in application and it is fundamental to troubleshoot any application problems. Logging frameworks make it easy to send **logs to different places via simple configurations**, 

Serilog uses **sinks** which are the places where we store our log messages like text file, database, log management solutions(e.g. datadog), or some cloud service like **Azure Application Insight** or potentially dozens of [other places](https://github.com/serilog/serilog/wiki/Provided-Sinks), all without changing your code.

In Azure Application Insight we can also send [custom event](https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics#trackevent) data. A **custom event is a data point/metric in azure application insight that can be useful to identify a particular situation** like how often users choose a particular feature, how often they achieve particular goals, or maybe make particular types of mistake.


---------------

## About this exercise

In this lab we will be working on **Backend codebase**

### **Backend Codebase:**

Previously we developed a base structure of an api solution in asp.net core that have just two api functions **GetLast12MonthBalances** & **GetLast12MonthBalances/{userId}** which returns data of the last 12 months total balances.

![apimethods](/readme_assets/apimethods.jpg)


There are 4 Projects in the solution. 

*	**Entities** : This project contains DB models like *User* where each User has one *Account* and each Account can have one or many *Transaction*. There is also a Response Model of *LineGraphData* that will be returned as API Response. 

*	**Infrastructure**: This project contains *BBBankContext* that serves as fake DBContext that populates one User with its corresponding Account that has some Transactions dated of last twelve months with hardcoded data. 

* **Services**: This project contains *TransactionService* with the logic of converting Transactions into LineGraphData after fetching them from BBBankContext.

* **BBBankAPI**: This project contains *TransactionController* with two GET methods *GetLast12MonthBalances* & *GetLast12MonthBalances/{userId}* to call the *TransactionService*.

![apiStructure](/readme_assets/apistructure.png)

For more details about this base project see: [Service Oriented Architecture Lab](https://github.com/PatternsTechGit/PT_ServiceOrientedArchitecture)

---------------
## In this exercise

* **Provisioning application insight** in azure
* **Incorporating application insight and serilog** in asp.net core application
* **Enable serilog and application insight logging** within the app settings
* **Implementing logging using serilog and custom events**


Here are the steps to begin with 

 ## Step 1: Provision Azure Application Insight

 Open [Azure Portal](https://portal.azure.com/) and go to your subscription.

 Go to your Application Insights resource and if it is not already created then create new one and copy the instrumentation key:
 
 ![key](/readme_assets/appinsightkey.png)

 ## Step 2: Install Nuget Packages 

Install following nuget packages in API project **BBBankAPI** through nuget package manager

- **Microsoft.ApplicationInsights.AspNetCore** to add Application Insights support.
- **Serilog.AspNetCore** to support serilog logging for ASP.Net Core
- **Serilog.Sinks.ApplicationInsights** to log information in application insight

You may install using package manager console as well:

```csharp
Install-Package Microsoft.ApplicationInsights.AspNetCore -Version 2.20.0
```
```csharp
Install-Package Serilog.AspNetCore -Version 5.0.0
```
```csharp
Install-Package Serilog.Sinks.ApplicationInsights -Version 3.1.0
```
 ![key](/readme_assets/nugetpackages.png)

 ## Step 3: Configure Serilog and Set Logging Levels

We will configure the [logging levels](https://github.com/serilog/serilog/wiki/Configuration-Basics#minimum-level) after ***WebApplication.CreateBuilder(args)***.  Also add the configurations for **Serilog** and **AddApplicationInsightsTelemetry** in the `program.cs` file as given below 

 ```csharp
 var logger = new LoggerConfiguration()
.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
// ASP.NET Core infrastructure logs that are Information and below will be filtered out.
.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
// Picking up configuration from appsettings.json
.ReadFrom.Configuration(builder.Configuration)
.Enrich.FromLogContext()
// Creating Serilog logger object based on the configuration above. 
.CreateLogger();

 // clear all exsiting logging proveriders
 builder.Logging.ClearProviders();
 // Adding Serilog to ASP.net core's pipe line
 builder.Logging.AddSerilog(logger);
 // Adding App Inisghts to send custom events.
 builder.Services.AddApplicationInsightsTelemetry();
 ```

 ## Step 4: Setup ASP.NET Core Pipeline Within try catch Block

We will **implement try catch block** to enclose ASP.NET pipeline builder so that if, during pipeline building, application failed to launch then we may know what had happened.

The `program.cs` file will look like below

```csharp
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

```

## Step 5: Add Log File in API Project

  Create a new folder **Logs** in the **BBBankAPI** project and create a new file `log.txt` in this folder.This file will be used by the serilog to log information in it. We will configure this file in the app settings in the next step

## Step 6: Configure Sinks in App Settings

Now add following code in app settings for application insight so that custom events can be send by **telemetryClient**. 

 ```cs
  // To configure app insight custom events (custom events for User Specific Actions ( e.g. User Accessed Accounts Data))
  "ApplicationInsights": {
    "InstrumentationKey": "fe637258-ecc6-43d0-9fab-xxxxxxxxxxxx",
    "LogLevel": {
      // By default AppInsights only logs Warning so we have to override it.
      "Default": "Information"
    }
  }

 ```

  Also add following code in app settings to enable serilog sinks for file and application insights 

 ```cs
   "Serilog": {
    // Array of serilog sinks that will used.
    "Using": [ "Serilog.Sinks.ApplicationInsights", "Serilog.Sinks.File" ],
    "MinimumLevel": "Information",
    // Configuring each sink individually. 
    "WriteTo": [
      {
        "Name": "File",
        "Args": { "path": "Logs/log.txt" }
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "instrumentationKey": "fe637258-ecc6-43d0-9fab-xxxxxxxxxxxx",
          //"restrictedToMinimumLevel": "Information",
          "telemetryConverter": "Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights"
        }
      }
    ]
  }
 ```

### **Note**
We will **replace the instrumentation key** with the key we copied in the step 1.  

## Step 7: Logging in API controller

Now we will **add logging statements in GetLast12MonthBalances method**.

We have multiple options to log information of different type which are listed below
* **logger.LogInformation** for normal information to know what is happing the code
* **logger.LogError** to log exceptions
* **telemetryClient.TrackEvent** to know stack trace and reason of the exception

We have used all the options in the **GetLast12MonthBalances** method of **Transaction** controller. The code is given below

```csharp
private readonly ITransactionService _transactionService;
private readonly ILogger<TransactionController> _logger;
private readonly TelemetryClient _telemetryClient;

public TransactionController(ILogger<TransactionController> logger, TelemetryClient telemetryClient, ITransactionService transactionService)

{
  _logger = logger;
  _telemetryClient = telemetryClient;
  _transactionService = transactionService;
}

 public async Task<ActionResult> GetLast12MonthBalances()
 {
  try
  {
    // Logging the name of the function before entering the business logic.
    _logger.LogInformation("Executing GetLast12MonthBalances");
    //return new OkObjectResult(await _transactionService.GetLast12MonthBalances(null));
    var res = await _transactionService.GetLast12MonthBalances(null);
    // recording custom event with some custom attributes TotalFiguresReturned and TotaBalance
    _telemetryClient.TrackEvent("GetLast12MonthBalances Returned", new Dictionary<string, string>() {
    { "TotalFiguresReturned", res.Figures.Count().ToString() }, 
    { "TotalBalance" , res.TotalBalance.ToString() }
  });
  // Logging the name of the function after the business logic has executed.
  _logger.LogInformation("Executed GetLast12MonthBalances");
  return new OkObjectResult(res);

  }
  catch (Exception ex)
  {
    _logger.LogError(ex, "Exception Executing GetLast12MonthBalances");
     return new BadRequestObjectResult(ex);
  }
}
```

### **Note**
We have injected the **ILogger<TransactionController>** and **TelemetryClient** to use these the our method 

------

### Final Output:

Run the project and access API endpoint **GetLast12MonthBalances** and you would see log messages in `log.txt` file and in the Application Insights as well.

URL to access API endpoint would be http://localhost:5070/api/Transaction/GetLast12MonthBalances


>![logMesssages](/readme_assets/logmsg.png)
