using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Interfaces
{
    public interface IChatServices
    {
        Task<IActionResult> GetMessagesAsync(int movieId);
        Task<IActionResult> SendMessageAsync(MessageReq request);

        Task<IActionResult> GetPrivateMessagesAsync(int user1Id, int user2Id);
        Task<IActionResult> SendPrivateMessageAsync(PrivateMessageRequest request);

    }
}
