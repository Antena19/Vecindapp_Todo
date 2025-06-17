using Microsoft.AspNetCore.Mvc;
using REST_VECINDAPP.Servicios;
using System.Threading.Tasks;

namespace REST_VECINDAPP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly TransbankService _transbankService;

        public PaymentController(TransbankService transbankService)
        {
            _transbankService = transbankService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                var response = await _transbankService.CreateTransaction(
                    request.Amount,
                    request.BuyOrder,
                    request.SessionId
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("commit")]
        public async Task<IActionResult> CommitTransaction([FromBody] CommitTransactionRequest request)
        {
            try
            {
                var response = await _transbankService.CommitTransaction(request.Token);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("status/{token}")]
        public async Task<IActionResult> GetTransactionStatus(string token)
        {
            try
            {
                var response = await _transbankService.GetTransactionStatus(token);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class CreateTransactionRequest
    {
        public decimal Amount { get; set; }
        public string BuyOrder { get; set; }
        public string SessionId { get; set; }
    }

    public class CommitTransactionRequest
    {
        public string Token { get; set; }
    }
} 