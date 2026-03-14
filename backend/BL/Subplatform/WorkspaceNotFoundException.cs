namespace Conversey.BL.Subplatform;

public class WorkspaceNotFoundException(string workspaceIdentifier)
    : Exception($"Workspace {workspaceIdentifier} was not found.");

