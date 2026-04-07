namespace AuthApi.Requests
{
    public class Message
    {
        public int id { get; set; }
        public int chatId { get; set; }
        public int userId { get; set; }
        public string userName { get; set; }
        public string? text { get; set; }
        public string? imagePath { get; set; }
        public DateTime createdAt { get; set; }
        public bool isEdited { get; set; }
        public DateTime? editedAt { get; set; }
        public bool isDeleted { get; set; }
    }
}
