using AuthApi.DatabaseContext;
using AuthApi.Hubs;
using AuthApi.Interfaces;
using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Services
{
    public class ChatService : IChatServices
    {
        private readonly ContextDb _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(ContextDb context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> GetMessagesAsync(int movieId)
        {
            var result = await _context.Messages.Where(x => x.movieId == movieId).OrderBy(x => x.createdAt).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                result  
            });
        }

        public async Task<IActionResult> DeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(x => x.id_Message == messageId);
            if( message == null)
            {
                return new NotFoundObjectResult(new
                {
                    status = false,
                    message = "Сообщение не найдено"
                });
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"movie_{message.movieId}").SendAsync("DeleteMovieMessage", messageId);

            return new OkObjectResult(new
            {
                status = true
            });
        }

        public async Task<IActionResult> SendMessageAsync(MessageRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.text) && string.IsNullOrWhiteSpace(request.imageUrl))
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                    message = "Сообщение пустое"
                });
            }

            request.createdAt = DateTime.UtcNow;

            Message message = new Message()
            {
                movieId = request.movieId,
                userId = request.userId,
                text = request.text,
                imageUrl = request.imageUrl,
                createdAt = request.createdAt
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"movie_{message.movieId}").SendAsync("ReceiveMovieMessage", message);


            return new OkObjectResult(new
            {
                status = true,
                message
            });
        }

        public async Task<IActionResult> GetPrivateMessages(int userId1, int userId2)
        {
            var result = await _context.PrivateMessages.Where(x =>
            (x.senderId == userId1 && x.receiverId == userId2) ||
            (x.senderId == userId2 && x.receiverId == userId1)).OrderBy(x => x.createdAt).ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                result
            });
        }

        public async Task<IActionResult> SendPrivateMessage(PrivateMessage request)
        {
            if(string.IsNullOrWhiteSpace(request.text) && string.IsNullOrWhiteSpace(request.imageUrl))
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                });
            }

            PrivateMessage privateMessage = new PrivateMessage()
            {
                senderId = request.senderId,
                receiverId = request.receiverId,
                text = request.text,
                imageUrl = request.imageUrl,
                createdAt = DateTime.UtcNow
            };

            await _context.PrivateMessages.AddAsync(privateMessage);
            await _context.SaveChangesAsync();

            int minId = Math.Min(request.senderId, request.receiverId);
            int maxId = Math.Max(request.senderId, request.receiverId);

            await _hubContext.Clients.Group($"private_{minId}_{maxId}").SendAsync("ReceivePrivateMessage", privateMessage);

            return new OkObjectResult(new
            {
                status = true,
                privateMessage
            });

                
        }
    }
}
