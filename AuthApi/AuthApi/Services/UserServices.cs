using AuthApi.DatabaseContext;
using AuthApi.Interfaces;
using AuthApi.Models;
using AuthApi.Requests;
using AuthApi.UniversalMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace AuthApi.Services
{
    public class UserServices : IUserServices
    {
        private readonly ContextDb _context;
        private readonly JwtGenerator _jwtGenerator;

        public UserServices(ContextDb context, JwtGenerator jwtGenerator)
        {
            _context = context;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<IActionResult> Registration(Registration regUser)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == regUser.Email);

            if(existingUser != null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                    message = "Пользователь с таким email уже существует"
                });
            }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(regUser.Password);

            var user = new User()
            {
                Email = regUser.Email,
                Password = hashedPassword,
                Name = regUser.Name,
                Description = regUser.Description,
                Role_Id = 2
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Регистрация прошла успешна"
            });
        }

        public async Task<IActionResult> Authorize(Auth authUser)
        {

            var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == authUser.Email);
            if( user == null)
            {
                return new UnauthorizedObjectResult(new
                {
                    status = false,
                    message = "Неверный email или пароль"
                });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(authUser.Password, user.Password);

            if (!isPasswordValid)
            {
                return new UnauthorizedObjectResult(new
                {
                    status = false,
                    message = "Неверный email или пароль"
                });
            }

            string token = _jwtGenerator.GenerateToken(user.id_User, user.Role_Id);

            _context.Sessions.Add(new Session
            {
                Token = token,
                User_id = user.id_User,
            });

            await _context.SaveChangesAsync();
            return new OkObjectResult(new
            {
                status = true,
                token,
                user = new
                {
                    user.id_User,
                    user.Email,
                    user.Name,
                    user.Description,
                    user.Role_Id,
                    Role = user.Role?.Name
                }
            });
        }

        public async Task<IActionResult> UpdateUser(UpdateUser updateUser)
        {
            var existingEmail = await _context.Users.FirstOrDefaultAsync(x => x.Email == updateUser.Email);

            if(existingEmail != null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                    message = "Этот email уже занят"
                });
            }

            var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.id_User == updateUser.id_User);
            if (user == null)
            {
                return new NotFoundObjectResult(new
                {
                    status = false,
                    message = "Пользователь не найден"
                });
            }

            user.Email = updateUser.Email;
            user.Description = updateUser.Description;
            user.Name = updateUser.Name;

            if (!string.IsNullOrWhiteSpace(updateUser.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(updateUser.Password);
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                user = new
                {
                    user.id_User,
                    user.Email,
                    user.Name,
                    user.Description,
                    user.Role_Id,
                    Role = user.Role?.Name
                }
            });
        }

        public async Task<IActionResult> Profile(Profile profile, string token)
        {
            var existingEmail = await _context.Users.FirstOrDefaultAsync(x => x.Email == profile.Email);

            if(existingEmail != null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                    message = "Этот email уже занят"
                });
            }

            var session = await _context.Sessions.Include(x => x.User.Role).FirstOrDefaultAsync(x => x.Token == token);

            if (session == null || session.User == null)
            {
                return new UnauthorizedObjectResult(new
                {
                    status = false,
                    message = "Сессия не найдена"
                });
            }
            var user = session.User;

            user.Email = profile.Email;
            user.Description = profile.Description;
            user.Name = profile.Name;

            if (!string.IsNullOrWhiteSpace(profile.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(profile.Password);
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                user = new
                {
                    user.id_User,
                    user.Email,
                    user.Name,
                    user.Description,
                    user.Role_Id,
                    Role = user.Role?.Name
                }
            });
        }

        public async Task<IActionResult> CreateNewUser(CreateNewUser regUser)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == regUser.Email);

            if(existingUser != null)
            {
                return new BadRequestObjectResult(new
                {
                    status = false,
                    message = "Пользователь с таким email уже существует"
                });
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(regUser.Password);

            var user = new User()
            {
                Email = regUser.Email,
                Password = hashedPassword,
                Name = regUser.Name,
                Description = regUser.Description,
                Role_Id = regUser.Role_id
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.id_User == user.id_User);

            return new OkObjectResult(new
            {
                status = true,
                user = new
                {
                    createdUser.id_User,
                    createdUser.Email,
                    createdUser.Name,
                    createdUser.Description,
                    createdUser.Role_Id,
                    Role = createdUser.Role?.Name
                }
            });
        }

        public async Task<IActionResult> DeleteUser(int user_id)
        {
            var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.id_User == user_id);
            if (user == null)
            {
                return new NotFoundObjectResult(new
                {
                    status = false,
                    message = "Пользователь не найден"
                });
            }

            _context.Remove(user);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                message = "Пользователь удален"
            });
        }

        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.Include(x => x.Role).Select(user => new
            {
                user.id_User,
                user.Email,
                user.Name,
                user.Description,
                user.Role_Id,
                Role = user.Role.Name
            }).ToListAsync();
            return new OkObjectResult(new
            {
                status = true,
                users
            });
        }

    }
}
