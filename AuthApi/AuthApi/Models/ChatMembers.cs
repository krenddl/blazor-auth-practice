using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Models
{
    public class ChatMembers
    {
        [Key]
        public int id_ChatMembers { get; set; }
        [ForeignKey("Chat")]
        public int id_Chat {  get; set; }
        public Chat Chat { get; set; }
        [ForeignKey("User")]
        public int id_User { get; set; }
        public User User { get; set; }
    }
}
