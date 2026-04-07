using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Interfaces
{
    public interface IChatServices
    {
        Task<IActionResult> GetOrCreateMovieChatAsync(int movieId);
        Task<IActionResult> GetOrCreate(int movieId);
    }
}
