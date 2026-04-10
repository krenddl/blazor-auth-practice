using AuthApi.Interfaces;
using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.AspNetCore.SignalR;

namespace AuthApi.Hubs
{
    public class ChatHub : Hub
    {
        public IChatServices _chatServices;
        public ChatHub(IChatServices chatServices)
        {
            _chatServices = chatServices;
        }

        public async Task Join(string key)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, key);
        }

        public async Task SendMovieMessage(SendMessageDto dtomesstage)
        {
            var response = await _chatServices.SendMessageAsync(dtomesstage);
            await Clients.Group($"movie_{dtomesstage.movieId}").SendAsync("ReceiveMovieMessage", response);

        }

        public async Task Leave(string key)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, key);
        }

        public async Task JoinPrivateChat(int userId1, int userId2)
        {
            int minId = Math.Min(userId1, userId2);
            int maxId = Math.Max(userId1, userId2);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"private_{minId}_{maxId}");
            
        }

        public async Task LeavePrivateChat(int userId1, int userId2)
        {
            int minId = Math.Min(userId1, userId2);
            int maxId = Math.Max(userId1, userId2);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"private_{minId}_{maxId}");
        }

    }
}
