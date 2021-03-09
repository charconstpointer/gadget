using System;
using Gadget.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gadget.Notifications.Persistence
{
    public class NotificationsContext : DbContext
    {
        public NotificationsContext(DbContextOptions<NotificationsContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(s => s.HasKey(ss => ss.Id));
            modelBuilder.Entity<Notification>(s => s.Property(ss => ss.Agent));
            modelBuilder.Entity<Notification>(s => s.HasIndex(ss => ss.Service));
            modelBuilder.Entity<Notification>(s => s.OwnsMany(ss => ss.Notifiers, w =>
            {
                w.WithOwner().HasForeignKey("OwnerId");
                w.Property<Guid>("Id");
                w.Property(wh => wh.Destination);
                w.Property(wh => wh.CreatedAt);
                w.Property(wh => wh.NotifierType);
                w.HasKey("Id");
            }));
        }

        public DbSet<Notification> Notifications { get; set; }
    }
}