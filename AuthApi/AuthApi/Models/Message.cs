using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Models
{
    public class Message
    {
        [Key]
        public int id_Message { get; set; }
        [ForeignKey("Chat")]
        public int id_Chat { get; set; }
        public Chat Chat { get; set; }
        [ForeignKey("User")]
        public int id_User { get; set; }
        public User User { get; set; }
        public string? text { get; set; }
        public string? imagePath { get; set; }
        public DateTime createdAt { get; set; }
        public bool isEdited { get; set; }
        public DateTime? editedAt { get; set; }
        public bool isDeleted { get; set; }
        public DateTime? deletedAt { get; set; }
    }
}
