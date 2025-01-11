using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using UWPMessagesApp.Data;
using UWPMessagesApp.Models;

namespace UWPMessagesApp.Services
{
    internal class MessageSendingLogService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _twilioPhoneNumber;

        public MessageSendingLogService()
        {
            // Recovering environment variables
            _accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID", EnvironmentVariableTarget.Process);
            _authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN", EnvironmentVariableTarget.Process);
            _twilioPhoneNumber = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_twilioPhoneNumber))
            {
                throw new InvalidOperationException("The Twilio credentials are not configured correctly.");
            }
        }

        public async Task<bool> SendMessageAsync(Message message)
        {
            try
            {
                // Start Twilio client
                TwilioClient.Init(_accountSid, _authToken);

                // Send message using Twilio
                var twilioMessage = await Task.Run(() => MessageResource.Create(
                    body: message.MessageText,
                    from: new PhoneNumber(_twilioPhoneNumber),
                    to: new PhoneNumber(message.ToField)
                ));

                // Save the MessageSendingLog
                await SaveMessageLogAsync(message.Id, twilioMessage.Sid);

                return true;
            }
            catch (Exception ex)
            {
                // Register errors in console
                System.Diagnostics.Debug.WriteLine($"Error sending the message: {ex.Message}");
                return false;
            }
        }

        private async Task SaveMessageLogAsync(int messageId, string twilioConfirmationCode)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var log = new MessageSendingLog
                    {
                        MessageId = messageId,
                        TwilioConfirmationCode = twilioConfirmationCode,
                        SentAt = DateTime.Now
                    };

                    db.MessageSendingLog.Add(log);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Register errors in console
                //System.Diagnostics.Debug.WriteLine($"Error when saving the MessageSendingLog: {ex.Message}");
                await ShowMessageDialogAsync("The message was created but could not be sent:\n" + ex.Message);
                throw;
            }
        }

        private async Task ShowMessageDialogAsync(string message)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(message);
            await dialog.ShowAsync();
        }
    }
}
