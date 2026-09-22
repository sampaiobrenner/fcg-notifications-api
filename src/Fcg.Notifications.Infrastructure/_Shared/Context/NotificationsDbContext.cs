using Fcg.Notifications.Application._Shared.Contexts;
using Fcg.Notifications.Domain._Shared.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notifications.Infrastructure._Shared.Context;

public sealed class NotificationsDbContext : DbContext, INotificationsDbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)
    {
    }

    public IQueryable<T> DataSet<T>() where T : PersistenceModelBase => Set<T>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
