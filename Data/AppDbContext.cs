using Microsoft.EntityFrameworkCore;
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
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=UWPMessagesDB;User Id=UserUWPMessages;Password=Wr12azqo+;");
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
