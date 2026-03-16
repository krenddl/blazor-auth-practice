using BlazorPractice1.ApiRequests.Model;
using static BlazorPractice1.ApiRequests.Model.Auth;

namespace BlazorPractice1.ApiRequests.Services
{
    public class AuthService
    {
        private readonly ApiRequest _api;
        public string? Token { get; private set; }
        public UserResponse? CurrentUser { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

        public AuthService(ApiRequest api)
        {
            _api = api;
        }

        public async Task<bool> LoginAsync (LoginRequest request)
        {
            var result = await _api.AuthorizeResponse(request);
            
            if(result == null || !result.status || string.IsNullOrWhiteSpace(result.token))
            {
                return false;
            }

            Token = result.token;
            CurrentUser = result.user;

            return true;
        }

        //public async Task UpdateProfile (UpdateProfileRequest request, string token)
        //{
        //    var result = await _api.UpdateProfileAsyncResponse(request, token);
            
        //    if(result == null || !result.status || string.IsNullOrWhiteSpace(token))
        //    {
        //        return false;
        //    }
        //}

        public void Logout()
        {
            Token = null;
            CurrentUser = null;
        }
    }
}
