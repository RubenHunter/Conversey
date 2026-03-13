namespace Conversey.BL.Subplatform.Survey;

public class ProjectNotFoundException(string projectIdentifier)
    : Exception($"Project {projectIdentifier} was not found.");