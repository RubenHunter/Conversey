using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IWorkspaceManager
{
    IEnumerable<Workspace> GetAllWorkspaces();
    Workspace AddWorkspace(string name, string imageUrl = "");
    Workspace GetWorkspaceById(Slug id);
    void EditWorkspace(Workspace updatedWorkspace);
    void RemoveWorkspace(Slug id);

    Task<string> UploadWorkspaceImage(Stream stream, string fileName, string contentType);
}