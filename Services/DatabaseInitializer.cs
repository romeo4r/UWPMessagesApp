using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UWPMessagesApp.Data;
using UWPMessagesApp.Models;

namespace UWPMessagesApp.Services
{
    public static class DatabaseInitializer
    {

        public static void InitializeData()
        {
            using (var context = new AppDbContext())
            {
                // Add a test Message if it doesn't exist
                var existingMessage = context.Message.FirstOrDefault(m => m.ToField == "1234567890" && m.MessageText == "Test message");
                if (existingMessage == null)
                {
                    var newMessage = new Message
                    {
                        ToField = "1234567890",
                        MessageText = "Test message",
                        CreatedAt = DateTime.Now
                    };

                    context.Message.Add(newMessage);
                    context.SaveChanges();

                    // Add a test MessageSendingLog associated with the new Message
                    var messageSendingLog = new MessageSendingLog
                    {
                        MessageId = newMessage.Id,
                        SentAt = DateTime.Now,
                        TwilioConfirmationCode = "TEST12345"
                    };

                    context.MessageSendingLog.Add(messageSendingLog);
                    context.SaveChanges();
                }


                // Check if there is already a Twilio credential in the database, in this case I just using only one record
                if (!context.TwilioCredential.Any())
                {
                    var twilioCredential = new TwilioCredential
                    {
                        Email = "romeo4ramos@gmail.com",
                        Password = HashPassword("Wr12tzqo+") // Encrypt the password
                    };

                    context.TwilioCredential.Add(twilioCredential);
                    context.SaveChanges();
                }
            }
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
