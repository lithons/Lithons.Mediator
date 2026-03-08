namespace Lithons.Mediator.Example.Blazor.Services;

public class ExceptionLog
{
    private readonly List<string> _entries = [];

    public IReadOnlyList<string> Entries => _entries;

    public void Add(string entry) => _entries.Add(entry);

    public void Clear() => _entries.Clear();
}
