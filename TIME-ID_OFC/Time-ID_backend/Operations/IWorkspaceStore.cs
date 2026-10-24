namespace TimeId.Operations;

/// <summary>
/// Executes an operation against a snapshot. A successful write persists all
/// changes together; a failed operation must leave the stored state unchanged.
/// </summary>
public interface IWorkspaceStore
{
    T Read<T>(Func<Database, T> action);
    T Write<T>(Func<Database, T> action);
}
