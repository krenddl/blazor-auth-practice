using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Interfaces
{
    public interface IChatServices
    {
        Task<IActionResult> GetMessagesAsync(int movieId);
        Task<IActionResult> SendMessageAsync(MessageReq request);
        Task<IActionResult> UpdateMessageAsync(UpdateMovieMessageRequest request);
        Task<IActionResult> DeleteMessageAsync(int messageId);

        Task<IActionResult> GetPrivateMessagesAsync(int user1Id, int user2Id);
        Task<IActionResult> SendPrivateMessageAsync(PrivateMessageRequest request);
        Task<IActionResult> UpdatePrivateMessageAsync(UpdatePrivateMessageRequest request);
        Task<IActionResult> DeletePrivateMessageAsync(int privateMessageId);

    }
}
