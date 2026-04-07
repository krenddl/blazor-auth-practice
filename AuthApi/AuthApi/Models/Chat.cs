using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthApi.Models
{
    public class Chat
    {
        [Key]
        public int id_Chat { get; set; }
        [ForeignKey("ChatType")]
        public int id_ChatType { get; set; }
        public ChatType ChatType { get; set; }
        [ForeignKey("Movie")]
        public int id_Movie { get; set; }
        public Movies Movie { get; set; }
        public DateTime createdAt { get; set; }
        [JsonIgnore]
        public ICollection<ChatMembers> chatMembers { get; set; }
        [JsonIgnore]
        public ICollection<Message> messages { get; set; }

    }
}
