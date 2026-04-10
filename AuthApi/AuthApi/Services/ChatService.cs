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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatService(ContextDb context, IHubContext<ChatHub> hubContext, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> GetMessagesAsync(int movieId)
        {
            var result = await _context.Messages
                .Where(x => x.movieId == movieId)
                .OrderBy(x => x.createdAt)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                result
            });
        }

        public async Task<IActionResult> SendMessageAsync(SendMessageDto dtomessage)
        {
            if (string.IsNullOrWhiteSpace(dtomessage.text))
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                    message = "Сообщение пустое"
                });
            }


            dtomessage.createdAt = DateTime.UtcNow;

            Message message = new Message()
            {
                movieId = dtomessage.movieId,
                userId = dtomessage.userId,
                text = dtomessage.text,
                imageUrl = null,
                createdAt = dtomessage.createdAt,
                isEdited = false
            };


            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();


            return new OkObjectResult(new
            {
                status = true,
                message
            });
        }

        public async Task<IActionResult> UpdateMessageAsync(UpdateMovieMessageRequest request)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            var session = await _context.Sessions
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return new UnauthorizedObjectResult(new
                {
                    status = false
                });
            }

            var message = await _context.Messages.FirstOrDefaultAsync(x => x.id_Message == request.id_Message);

            if (message == null)
            {
                return new NotFoundObjectResult(new
                {
                    status = false,
                    message = "Сообщение не найдено"
                });
            }

            var user = session.User;

            if (user.Role_Id != 1 && user.id_User != message.userId)
            {
                return new ObjectResult(new
                {
                    status = false,
                    message = "Нет доступа"
                })
                { StatusCode = 403 };
            }

            if (request.text != null)
            {
                message.text = request.text.Trim();
            }

            if (request.removeCurrentImage && message.imageUrl != null)
            {
                var path = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                message.imageUrl = null;
            }

            if (request.newFile != null && request.newFile.Length > 0)
            {
                if (message.imageUrl != null)
                {
                    var oldPath = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                    if (File.Exists(oldPath))
                    {
                        File.Delete(oldPath);
                    }
                }

                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "chat");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.newFile.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.newFile.CopyToAsync(stream);
                }

                message.imageUrl = $"/images/chat/{fileName}";
            }

            if (string.IsNullOrWhiteSpace(message.text) && message.imageUrl == null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                    message = "Сообщение не может быть пустым"
                });
            }

            message.isEdited = true;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"movie_{message.movieId}")
                .SendAsync("UpdateMovieMessage", message);

            return new OkObjectResult(new
            {
                status = true,
                message
            });
        }

        public async Task<IActionResult> DeleteMessageAsync(int messageId)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            var session = await _context.Sessions
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return new UnauthorizedObjectResult(new
                {
                    status = false
                });
            }

            var message = await _context.Messages.FirstOrDefaultAsync(x => x.id_Message == messageId);

            if (message == null)
            {
                return new NotFoundObjectResult(new
                {
                    status = false,
                    message = "Сообщение не найдено"
                });
            }

            var user = session.User;

            if (user.Role_Id != 1 && user.id_User != message.userId)
            {
                return new ObjectResult(new
                {
                    status = false,
                    message = "Нет доступа"
                })
                { StatusCode = 403 };
            }

            if (message.imageUrl != null)
            {
                var path = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            int movieId = message.movieId;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"movie_{movieId}")
                .SendAsync("DeleteMovieMessage", messageId);

            return new OkObjectResult(new
            {
                status = true
            });
        }

        public async Task<IActionResult> GetPrivateMessagesAsync(int userId1, int userId2)
        {
            var result = await _context.PrivateMessages
                .Where(x =>
                    (x.senderId == userId1 && x.receiverId == userId2) ||
                    (x.senderId == userId2 && x.receiverId == userId1))
                .OrderBy(x => x.createdAt)
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                result
            });
        }

        public async Task<IActionResult> SendPrivateMessageAsync(PrivateMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.text) && request.file == null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false
                });
            }

            string? relativePath = null;

            if (request.file != null && request.file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "privatechats");

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
                createdAt = DateTime.UtcNow,
                isEdited = false
            };

            await _context.PrivateMessages.AddAsync(privateMessage);
            await _context.SaveChangesAsync();

            int minId = Math.Min(request.senderId, request.receiverId);
            int maxId = Math.Max(request.senderId, request.receiverId);

            await _hubContext.Clients.Group($"private_{minId}_{maxId}")
                .SendAsync("ReceivePrivateMessage", privateMessage);

            return new OkObjectResult(new
            {
                status = true,
                privateMessage
            });
        }

        public async Task<IActionResult> UpdatePrivateMessageAsync(UpdatePrivateMessageRequest request)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            var session = await _context.Sessions
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return new UnauthorizedObjectResult(new
                {
                    status = false
                });
            }

            var message = await _context.PrivateMessages.FirstOrDefaultAsync(x => x.id_PrivateMessage == request.id_PrivateMessage);

            if (message == null)
            {
                return new NotFoundObjectResult(new
                {
                    status = false
                });
            }

            var user = session.User;

            if (user.Role_Id != 1 && user.id_User != message.senderId)
            {
                return new ObjectResult(new
                {
                    status = false
                })
                { StatusCode = 403 };
            }

            if (request.text != null)
            {
                message.text = request.text.Trim();
            }

            if (request.removeCurrentImage && message.imageUrl != null)
            {
                var path = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                message.imageUrl = null;
            }

            if (request.newFile != null && request.newFile.Length > 0)
            {
                if (message.imageUrl != null)
                {
                    var oldPath = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                    if (File.Exists(oldPath))
                    {
                        File.Delete(oldPath);
                    }
                }

                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "privatechats");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.newFile.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.newFile.CopyToAsync(stream);
                }

                message.imageUrl = $"/images/privatechats/{fileName}";
            }

            if (string.IsNullOrWhiteSpace(message.text) && message.imageUrl == null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false
                });
            }

            message.isEdited = true;

            await _context.SaveChangesAsync();

            int minId = Math.Min(message.senderId, message.receiverId);
            int maxId = Math.Max(message.senderId, message.receiverId);

            await _hubContext.Clients.Group($"private_{minId}_{maxId}")
                .SendAsync("UpdatePrivateMessage", message);

            return new OkObjectResult(new
            {
                status = true,
                message
            });
        }

        public async Task<IActionResult> DeletePrivateMessageAsync(int privateMessageId)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            var session = await _context.Sessions
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return new UnauthorizedObjectResult(new
                {
                    status = false
                });
            }

            var message = await _context.PrivateMessages.FirstOrDefaultAsync(x => x.id_PrivateMessage == privateMessageId);

            if (message == null)
            {
                return new NotFoundObjectResult(new
                {
                    status = false
                });
            }

            var user = session.User;

            if (user.Role_Id != 1 && user.id_User != message.senderId)
            {
                return new ObjectResult(new
                {
                    status = false
                })
                { StatusCode = 403 };
            }

            if (message.imageUrl != null)
            {
                var path = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            int minId = Math.Min(message.senderId, message.receiverId);
            int maxId = Math.Max(message.senderId, message.receiverId);

            _context.PrivateMessages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"private_{minId}_{maxId}")
                .SendAsync("DeletePrivateMessage", privateMessageId);

            return new OkObjectResult(new
            {
                status = true
            });
        }
    }
}