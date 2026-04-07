namespace AuthApi.Requests
{
    public class EditMessage
    {
        public int messageId { get; set; }
        public int userId { get; set; }
        public string userRole { get; set; }
        public string newText { get; set; }
    }
}
