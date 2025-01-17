﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UWPMessagesApp.Data;
using UWPMessagesApp.Models;
using UWPMessagesApp.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace UWPMessagesApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Variables
        private List<string> ContactNumbers { get; set; } = new List<string>();
        private ObservableCollection<MessageDisplayModel> Messages { get; set; } = new ObservableCollection<MessageDisplayModel>();
        #endregion

        #region Control variables
        private bool IsAscending = true; // Track sort direction
        private bool isFocusHandled = false; // Flag to preves bucles for phone number list

        #endregion

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize basic data in the database
            DatabaseInitializer.InitializeData();

            // Load unique contact numbers
            _ = LoadContactNumbersAsync();

            // Bind the ObservableCollection to the ListView
            MessagesListView.ItemsSource = Messages;

            // Load messages on startup
            _ = LoadMessagesAsync();
        }

        #region Events and code for the To (phone number)

        // Loads unique contact numbers from the database
        private async Task LoadContactNumbersAsync()
        {
            try
            {
                ContactNumbers = await MessageService.GetUniqueContactNumbersAsync();
            }
            catch (Exception ex)
            {
                // Log or handle the error as needed
                System.Diagnostics.Debug.WriteLine($"Error loading contacts: {ex.Message}");
            }
        }

        //Show all the previous phone numbers
        private void PhoneNumberAutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (isFocusHandled) return;

            isFocusHandled = true;

            var autoSuggestBox = sender as AutoSuggestBox;

            if (autoSuggestBox != null)
            {
                // Filter suggestions based on the entered text
                var enteredText = autoSuggestBox.Text;

                // If no text is entered, show the full list
                if (string.IsNullOrWhiteSpace(enteredText))
                {
                    autoSuggestBox.ItemsSource = ContactNumbers;
                }
                else
                {
                    // Filter the contact list based on the entered text
                    autoSuggestBox.ItemsSource = ContactNumbers
                        .Where(c => c.Contains(enteredText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Manually open the suggestion list dropdown
                autoSuggestBox.IsSuggestionListOpen = true;
            }

            // Restore the flag after a short delay
            Task.Delay(100).ContinueWith(_ =>
            {
                isFocusHandled = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // Handles the TextChanged event to filter suggestions
        private void PhoneNumberAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Filter the contact numbers based on user input
                var filteredContacts = ContactNumbers
                    .Where(c => c.Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                sender.ItemsSource = filteredContacts;
            }
        }

        // Handles the SuggestionChosen event when a user selects a suggestion
        private void PhoneNumberAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set the selected contact in the text box
            sender.Text = args.SelectedItem.ToString();
        }

        // Handles the QuerySubmitted event when the user presses Enter or selects a suggestion
        private void PhoneNumberAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // Set the chosen suggestion as the text
                sender.Text = args.ChosenSuggestion.ToString();
            }
        }

        #endregion

        #region Summit button code

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the values from the AutoSuggestBox (phone number) and TextBox (message content)
            string to = PhoneNumberAutoSuggestBox.Text;
            string messageContent = MessageTextBox.Text;

            // Input validation
            if (string.IsNullOrWhiteSpace(to))
            {
                await ShowMessageDialogAsync("The Phone Number is required.");
                return;
            }

            if (!IsValidPhoneNumber(to))
            {
                await ShowMessageDialogAsync("The Phone Number is invalid. Please check the correct format (e.g., +503 7040 6366).");
                return;
            }

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                await ShowMessageDialogAsync("The Message is required.");
                return;
            }

            try
            {
                // Create a new message (do not manually set the Id as it is auto-generated by the database)
                var newMessage = new Message
                {
                    ToField = to, // Recipient's phone number
                    MessageText = messageContent, // Message content
                    CreatedAt = DateTime.Now // Current date and time
                };

                // Save the message to the database using the MessageService
                bool resultMessage = await MessageService.AddMessageAsync(newMessage);

                var messageSendingLogService = new MessageSendingLogService();
                bool resultMessageSending = await messageSendingLogService.SendMessageAsync(newMessage);

                if (resultMessage & resultMessageSending)
                {
                    // Notify the user that the message was successfully saved
                    await ShowMessageDialogAsync("The message was sent and saved successfully.");

                    // Clear the form fields
                    PhoneNumberAutoSuggestBox.Text = string.Empty;
                    MessageTextBox.Text = string.Empty;

                    // Optional: Refresh the contact list after saving a new message
                    await LoadContactNumbersAsync();

                    // Reload messages
                    await LoadMessagesAsync();
                }
                else
                {
                    // Notify the user if the message could not be saved
                    await ShowMessageDialogAsync("Failed to save the message. Please try again.");
                }
            }
            catch (Exception ex)
            {
                // Error handling: show a message if an unexpected exception occurs
                await ShowMessageDialogAsync($"An error occurred: {ex.Message}");
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Regex to validate the phone number starts with '+' and contains only digits (spaces allowed between digits)
            string pattern = @"^\+[0-9 ]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, pattern);
        }

        // Helper method to show popup messages to the user
        private async Task ShowMessageDialogAsync(string message)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(message);
            await dialog.ShowAsync();
        }

        #endregion

        #region Message list code
        private async Task LoadMessagesAsync()
        {
            try
            {
                // Fetch messages with logs
                var messages = await MessageService.GetMessagesWithLogsAsync();
                messages = messages.OrderByDescending(m => DateTime.Parse(m.CreateAt)).ToList();

                // Clear the existing collection and add the new data
                Messages.Clear();

                foreach (var message in messages)
                {
                    Messages.Add(message);
                }

                // Replace the entire collection
                MessagesListView.ItemsSource = null; // Clear the link temporarily
                MessagesListView.ItemsSource = new ObservableCollection<MessageDisplayModel>(Messages);
            }
            catch (Exception ex)
            {
                // Handle and log errors
                System.Diagnostics.Debug.WriteLine($"Error loading messages: {ex.Message}");
            }
        }

        private void SortByColumn(object sender, TappedRoutedEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag != null)
            {
                string column = textBlock.Tag.ToString();
                System.Diagnostics.Debug.WriteLine($"Sorting by column: {column}");

                // Verifica si la lista tiene datos
                if (Messages == null || Messages.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Messages collection is empty. Nothing to sort.");
                    return;
                }

                // Toggle sort direction
                IsAscending = !IsAscending;

                IEnumerable<MessageDisplayModel> sortedMessages;

                switch (column)
                {
                    case "CreateAt":
                        sortedMessages = IsAscending
                            ? Messages.OrderBy(m => DateTime.Parse(m.CreateAt))
                            : Messages.OrderByDescending(m => DateTime.Parse(m.CreateAt));
                        break;

                    case "To":
                        sortedMessages = IsAscending
                            ? Messages.OrderBy(m => m.To)
                            : Messages.OrderByDescending(m => m.To);
                        break;

                    case "MessageText":
                        sortedMessages = IsAscending
                            ? Messages.OrderBy(m => m.MessageText)
                            : Messages.OrderByDescending(m => m.MessageText);
                        break;

                    case "SendAt":
                        sortedMessages = IsAscending
                            ? Messages.OrderBy(m => m.SendAt)
                            : Messages.OrderByDescending(m => m.SendAt);
                        break;

                    case "TwilioConfirmationCode":
                        sortedMessages = IsAscending
                            ? Messages.OrderBy(m => m.TwilioConfirmationCode)
                            : Messages.OrderByDescending(m => m.TwilioConfirmationCode);
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"Column {column} is not valid.");
                        return;
                }

                // Replace the entire collection
                MessagesListView.ItemsSource = null; // Clear the link temporarily
                MessagesListView.ItemsSource = new ObservableCollection<MessageDisplayModel>(sortedMessages);

                System.Diagnostics.Debug.WriteLine($"Sorted {sortedMessages.Count()} messages and updated the list.");
            }
        }

        private void MessagesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure an item is selected
            if (MessagesListView.SelectedItem is MessageDisplayModel selectedMessage)
            {
                // Set the values of PhoneNumberAutoSuggestBox and MessageTextBox
                PhoneNumberAutoSuggestBox.Text = selectedMessage.To;
                MessageTextBox.Text = selectedMessage.MessageText;
            }
        }

        #endregion
    }
}

