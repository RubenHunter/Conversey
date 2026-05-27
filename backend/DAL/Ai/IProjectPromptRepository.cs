using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IProjectPromptRepository
{
    Task<IReadOnlyList<ProjectAiPromptOverride>> GetOverridesForProjectAsync(string projectId);
    Task<ProjectAiPromptOverride> GetOverrideAsync(string projectId, string promptName);
    Task SaveOverrideAsync(ProjectAiPromptOverride projectOverride);
    Task DeleteOverridesForProjectAsync(string projectId);
}
