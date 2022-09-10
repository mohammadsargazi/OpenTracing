using MongoDB.Entities;

namespace User.Models
{
    public class User : Entity
    {
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
    }
}
