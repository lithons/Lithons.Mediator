namespace Lithons.Mediator.Example.Blazor.Services;

public class AuditLog
{
    private readonly List<AuditEntry> _entries = [];
    private readonly Lock _lock = new();

    public IReadOnlyList<AuditEntry> Entries
    {
        get { lock (_lock) return [.. _entries]; }
    }

    public void Add(string action, string detail)
    {
        lock (_lock)
        {
            _entries.Add(new AuditEntry(DateTime.Now, action, detail));
        }
    }

    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }
}

public record AuditEntry(DateTime Timestamp, string Action, string Detail);
