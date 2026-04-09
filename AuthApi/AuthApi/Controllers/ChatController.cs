using AuthApi.CustomAtributes;
using AuthApi.Interfaces;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IChatServices _chatServices;

        public ChatController(IChatServices chatServices)
        {
            _chatServices = chatServices;
        }

        [HttpGet]
        [Route("GetMovieMessages")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> GetMessagesAsync(int movieId)
        {
            return await _chatServices.GetMessagesAsync(movieId);
        }

        [HttpPost]
        [Route("SendMovieMessage")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> SendMessageAsync([FromForm] MessageReq request)
        {
            return await _chatServices.SendMessageAsync(request);
        }

        [HttpPut]
        [Route("UpdateMovieMessage")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> UpdateMessageAsync([FromForm] UpdateMovieMessageRequest request)
        {
            return await _chatServices.UpdateMessageAsync(request);
        }

        [HttpDelete]
        [Route("DeleteMovieMessage")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> DeleteMessageAsync(int messageId)
        {
            return await _chatServices.DeleteMessageAsync(messageId);
        }


        [HttpGet]
        [Route("GetPrivateMessages")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> GetPrivateMessagesAsync(int userId1, int userId2)
        {
            return await _chatServices.GetPrivateMessagesAsync(userId1, userId2);
        }

        [HttpPost]
        [Route("SendPrivateMessage")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> SendPrivateMessageAsync([FromForm] PrivateMessageRequest request)
        {
            return await _chatServices.SendPrivateMessageAsync(request);
        }

        [HttpPut]
        [Route("UpdatePrivateMessage")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> UpdatePrivateMessageAsync([FromForm] UpdatePrivateMessageRequest request)
        {
            return await _chatServices.UpdatePrivateMessageAsync(request);
        }

        [HttpDelete]
        [Route("DeletePrivateMessage")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> DeletePrivateMessageAsync(int privateMessageId)
        {
            return await _chatServices.DeletePrivateMessageAsync(privateMessageId);
        }
    }
}