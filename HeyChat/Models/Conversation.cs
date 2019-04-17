using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeyChat.Models
{
    public class Conversation
    {
        public Conversation()
        {
            Status = MessageStatus.Sent;
        }

        public enum MessageStatus
        {
            Sent, 
            Delivered
        }

        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string SecretKey { get; set; }
        public string Message { get; set; }
        public MessageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
