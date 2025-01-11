using Microsoft.EntityFrameworkCore;
using System;
using UWPMessagesApp.Models;

namespace UWPMessagesApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Message> Message { get; set; }
        public DbSet<MessageSendingLog> MessageSendingLog { get; set; }
        public DbSet<TwilioCredential> TwilioCredential { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Connection string to SQL Server

            var connectionString = Environment.GetEnvironmentVariable("UWPMessagesDB_CONNECTION_STRING");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string is not configured.");
            }

            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define relationships manually
            modelBuilder.Entity<MessageSendingLog>()
                .HasOne(log => log.Message)
                .WithMany(message => message.SendingLogs)
                .HasForeignKey(log => log.MessageId)
                .HasConstraintName("FK_MessageSendingLog_Message");
        }
    }
}
