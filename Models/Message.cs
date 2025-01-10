using System;
using System.Collections.Generic;

namespace UWPMessagesApp.Models
{
    public class Message
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string ToField { get; set; }
        public string MessageText { get; set; }

        // Navigation property
        public ICollection<MessageSendingLog> SendingLogs { get; set; }
    }
}
