using BlazorPractice1.ApiRequests.Model;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using static BlazorPractice1.ApiRequests.Model.Auth;
using static BlazorPractice1.ApiRequests.Model.Common;

namespace BlazorPractice1.ApiRequests
{
    public class ApiRequest
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiRequest> _logger;
        public ApiRequest(HttpClient httpClient, ILogger<ApiRequest> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", token);
        }

        public async Task<AuthorizeResponse> AuthorizeResponse(LoginRequest request)
        {
            var url = "Authorize";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var userAdd = JsonSerializer.Deserialize<AuthorizeResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return userAdd ?? new AuthorizeResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new AuthorizeResponse();
            }
        }

        public async Task<StatusResponse> RegistrationAsync(RegistrationRequest request)
        {
            var url = "Registration";

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var userAdd = JsonSerializer.Deserialize<StatusResponse>(content);

                return userAdd ?? new StatusResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new StatusResponse();
            }
        }


        public async Task<UsersListResponse> GetAllUsersAsyncResponse(string token)
        {
            var url = "GetAllUsers";
            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("Ответ от сервера пуст.");
                    return new UsersListResponse();
                }

                var userlist = JsonSerializer.Deserialize<UsersListResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }); 

                return userlist ?? new UsersListResponse();
            }
            catch(Exception ex)
            {
                    _logger.LogError(ex, "Ошибка при запросе");
                    return new UsersListResponse();
            }
        }

        public async Task<CreateUserResponse> CreateUserAsyncResponse(CreateUserRequest request, string token)
        {
            var url = "CreateNewUser";

            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.PostAsJsonAsync(url, request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var userAdd = JsonSerializer.Deserialize<CreateUserResponse>(content);

                return userAdd ?? new CreateUserResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new CreateUserResponse();
            }
        }



        public async Task<UpdateUserResponse> UpdateUserAsyncResponse(UpdateUserRequest request, string token)
        {
            var url = "UpdateUser";
            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.PutAsJsonAsync(url, request);
                var content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                var userUpdate = JsonSerializer.Deserialize<UpdateUserResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return userUpdate ?? new UpdateUserResponse();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new UpdateUserResponse();
            }
        }

        public async Task<StatusResponse?> DeleteUserAsync(int id, string token)
        {
            var url = $"/DeleteUsers/?user_id={id}";

            try
            {
                SetAuthorizationHeader(token);
                var resp = await _httpClient.DeleteAsync(url);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<StatusResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new StatusResponse();
            }
            
        }

        public async Task<UpdateProfileResponse?> UpdateProfileAsyncResponse(UpdateProfileRequest user, string token)
        {
            var url = "Profile";
            SetAuthorizationHeader(token);
            try
            {
                var resp = await _httpClient.PutAsJsonAsync(url, user);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<UpdateProfileResponse>();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return new UpdateProfileResponse();
            }
            
        }
    }
}
