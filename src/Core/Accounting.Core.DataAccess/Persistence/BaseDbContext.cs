using Accounting.Core.Business.Interfaces;
using Accounting.Core.Domain.Common;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Accounting.Core.DataAccess.Persistence;

public abstract class BaseDbContext(
    DbContextOptions options ,
    IClock clock ,
    ICurrentUser currentUser) : DbContext(options)
{
    #region Properties
    protected abstract bool SupportsAuditLog { get; }
    #endregion Properties

    #region Operations
    public override int SaveChanges()
    {
        ApplyAuditInfo();
        ApplyAuditLog();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        ApplyAuditLog();
        return base.SaveChangesAsync(cancellationToken);
    }
    #endregion Operations

    #region Helpers
    private void ApplyAuditInfo()
    {
        var entries = ChangeTracker.Entries<IAuditable>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = clock.UtcNow;
                entry.Entity.CreatedBy = currentUser.UserName;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = clock.UtcNow;
                entry.Entity.UpdatedBy = currentUser.UserName;
            }
        }
    }

    private void ApplyAuditLog()
    {
        if (!SupportsAuditLog)
        {
            return;
        }

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog
                     && e.State is EntityState.Added
                                 or EntityState.Modified
                                 or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var auditLog = new AuditLog
            {
                UserId = currentUser.UserId,
                UserName = currentUser.UserName,
                Action = GetActionName(entry.State),
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = GetEntityId(entry),
                OldValues = entry.State == EntityState.Added
                    ? string.Empty
                    : SerializeValues(entry, useOriginalValues: true),
                NewValues = entry.State == EntityState.Deleted
                    ? string.Empty
                    : SerializeValues(entry, useOriginalValues: false),
                Timestamp = clock.UtcNow,
                IpAddress = currentUser.IpAddress
            };

            auditLogs.Add(auditLog);
        }

        Set<AuditLog>().AddRange(auditLogs);
    }

    private static string GetActionName(EntityState state)
    {
        return state switch
        {
            EntityState.Added => "Create",
            EntityState.Modified => "Update",
            EntityState.Deleted => "Delete",
            _ => state.ToString()
        };
    }

    private static string GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties
            .FirstOrDefault(p => p.Metadata.Name == "Id");

        return idProperty?.CurrentValue?.ToString() ?? string.Empty;
    }

    private static string SerializeValues(EntityEntry entry , bool useOriginalValues)
    {
        var properties = entry.Properties
            .Where(p => p.Metadata.Name != "Id")
            .Where(p => entry.State != EntityState.Modified || p.IsModified);

        var dictionary = new Dictionary<string, string>();

        foreach (var property in properties)
        {
            var value = useOriginalValues
                ? property.OriginalValue
                : property.CurrentValue;

            dictionary[property.Metadata.Name] = value?.ToString() ?? string.Empty;
        }

        return JsonSerializer.Serialize(dictionary);
    }
    #endregion Helpers
}