using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPMessagesApp.Models
{
     class MessageDisplayModel
    {
        public string CreateAt { get; set; } // Creation date of the message
        public string To { get; set; } // Recipient's phone number
        public string MessageText { get; set; } // Message content
        public string SendAt { get; set; } // Date and time the message was sent
        public string TwilioConfirmationCode { get; set; } // Twilio confirmation code
    }
}
