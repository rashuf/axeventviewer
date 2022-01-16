using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Model
{
    internal class EventInboxConfiguration : EntityTypeConfiguration<EventInbox>
    {
        public EventInboxConfiguration()
        {
            ToTable("EventInboxes");
            HasKey(p => p.InboxId);
            Property(p => p.InboxId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }
}
