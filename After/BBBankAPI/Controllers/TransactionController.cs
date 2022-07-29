using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace BBBankAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;
        private readonly TelemetryClient _telemetryClient;
        public TransactionController(ILogger<TransactionController> logger, TelemetryClient telemetryClient, ITransactionService transactionService)

        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _transactionService = transactionService;
        }

        [HttpGet]
        [Route("GetLast12MonthBalances")]
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
           { "TotalFiguresReturned", res.Figures.Count().ToString() }
            , { "TotalBalance" , res.TotalBalance.ToString() }
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

        [HttpGet]
        [Route("GetLast12MonthBalances/{userId}")]
        public async Task<ActionResult> GetLast12MonthBalances(string userId)
        {
            try
            {
                return new OkObjectResult(await _transactionService.GetLast12MonthBalances(userId));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
