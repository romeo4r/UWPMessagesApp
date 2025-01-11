using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPMessagesApp.Data;
using UWPMessagesApp.Models;

namespace UWPMessagesApp.Services
{
    internal class MessageService
    {
        public static async Task<bool> AddMessageAsync(Message message)
        {
            try
            {
                using (var db = new AppDbContext()) // Create an instance of the database context
                {
                    db.Message.Add(message); // Add the message to the context
                    await db.SaveChangesAsync(); // Save changes to the database
                    return true; // Return true if the operation was successful
                }
            }
            catch (Exception ex)
            {
                // Log the error in the debug console
                System.Diagnostics.Debug.WriteLine($"Error adding the message: {ex.Message}");
                return false; // Return false if an error occurred
            }
        }

        public static async Task<List<string>> GetUniqueContactNumbersAsync()
        {
            try
            {
                using (var db = new AppDbContext()) // Create an instance of the database context
                {
                    // Fetch unique phone numbers (ToField), order them alphabetically, and return as a list
                    return await db.Message
                        .Select(m => m.ToField) 
                        .Distinct() 
                        .OrderBy(to => to) 
                        .ToListAsync(); 
                }
            }
            catch (Exception ex)
            {
                // Log the error in the debug console and return an empty list
                System.Diagnostics.Debug.WriteLine($"Error fetching contact numbers: {ex.Message}");
                return new List<string>();
            }
        }

        public static async Task<List<MessageDisplayModel>> GetMessagesWithLogsAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // Join between Message and MessageSendingLog
                    var query = from m in db.Message
                                join log in db.MessageSendingLog on m.Id equals log.MessageId into logs
                                from log in logs.DefaultIfEmpty() 
                                select new MessageDisplayModel
                                {
                                    CreateAt = m.CreatedAt.ToString("g"), // Creation date of the message
                                    To = m.ToField, // Phone number
                                    MessageText = m.MessageText, // Message content
                                    SendAt = log != null && log.SentAt != null ? log.SentAt.ToString("g") : "Not Sent", // Sent date or "Not Sent"
                                    TwilioConfirmationCode = log != null ? log.TwilioConfirmationCode : "N/A" // Twilio code or "N/A"
                                };

                    return await query.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error and return an empty list
                System.Diagnostics.Debug.WriteLine($"Error loading messages: {ex.Message}");
                return new List<MessageDisplayModel>();
            }
        }


    }
}
