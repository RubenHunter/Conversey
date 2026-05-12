using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IPromptRepository
{
    Task<AiPrompt> GetPromptAsync(string name);
    Task<IReadOnlyList<AiPrompt>> GetAllPromptsAsync();
    Task SavePromptAsync(AiPrompt prompt);
    Task DeletePromptAsync(int id);
}
