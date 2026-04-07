using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class ChatType
    {
        [Key]
        public int id_ChatType { get; set; }
        public string name { get; set; }
    }
}
