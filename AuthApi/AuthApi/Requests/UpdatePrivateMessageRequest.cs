namespace AuthApi.Requests
{
    public class UpdatePrivateMessageRequest
    {
        public int id_PrivateMessage { get; set; }
        public string? text { get; set; }
        public IFormFile? newFile { get; set; }
        public bool removeCurrentImage { get; set; } = false;
    }
}
