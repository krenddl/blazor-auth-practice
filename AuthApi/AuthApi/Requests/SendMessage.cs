namespace AuthApi.Requests
{
    public class SendMessage
    {
        public int chatId { get; set; }
        public int userId { get; set; }
        public string? text { get; set; }
        public string? imagePath { get; set; }
    }
}
