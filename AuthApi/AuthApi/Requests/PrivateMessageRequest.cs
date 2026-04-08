namespace AuthApi.Requests
{
    public class PrivateMessageRequest
    {
        public int id_PrivateMessage { get; set; }

        public int senderId { get; set; }

        public int receiverId { get; set; }

        public string? text { get; set; }

        public IFormFile? file { get; set; }

        public DateTime createdAt { get; set; }

        public bool isEdited { get; set; } = false;
    }
}
