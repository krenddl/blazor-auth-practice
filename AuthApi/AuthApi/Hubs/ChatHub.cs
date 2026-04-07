using Microsoft.AspNetCore.SignalR;

namespace AuthApi.Hubs
{
    public class ChatHub : Hub
    {
        public async Task Join(string key)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, key);
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
