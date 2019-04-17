using System;
using System.Collections.Generic;
namespace HeyChat.Models
{
    
    public class User
    {
        public User()
        {
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
