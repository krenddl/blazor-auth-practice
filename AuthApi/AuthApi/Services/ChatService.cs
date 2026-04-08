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

        public async Task<IActionResult> SendMessageAsync(MessageReq request)
        {
            if(string.IsNullOrWhiteSpace(request.text) && request.file == null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                    message = "Сообщение пустое"
                });
            }

            string? relativePath = null;

            if(request.file != null && request.file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwrot", "images", "chat");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileExtension = Path.GetExtension(request.file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.file.CopyToAsync(stream);
                }

                relativePath = $"/images/chat/{fileName}";
            }

            request.createdAt = DateTime.UtcNow;

            Message message = new Message()
            {
                movieId = request.movieId,
                userId = request.userId,
                text = request.text,
                imageUrl = relativePath,
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

        public async Task<IActionResult> GetPrivateMessagesAsync(int userId1, int userId2)
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

        public async Task<IActionResult> SendPrivateMessageAsync(PrivateMessageRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.text) && request.file == null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                });
            }

            string? relativePath = null;

            if (request.file != null && request.file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwrot", "images", "privatechats");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileExtension = Path.GetExtension(request.file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.file.CopyToAsync(stream);
                }

                relativePath = $"/images/privatechats/{fileName}";
            }

            PrivateMessage privateMessage = new PrivateMessage()
            {
                senderId = request.senderId,
                receiverId = request.receiverId,
                text = request.text,
                imageUrl = relativePath,
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
