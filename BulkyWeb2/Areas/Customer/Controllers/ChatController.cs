using BulkyWeb2.Services;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb2.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                var response = await _chatService.GetResponseFromOllama(message);
                return Json(new { success = true, response });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
} 