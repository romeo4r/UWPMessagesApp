using System;

namespace UWPMessagesApp.Models
{
    public class MessageSendingLog
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public string TwilioConfirmationCode { get; set; }

        // Navigation property
        public Message Message { get; set; }
    }
}
