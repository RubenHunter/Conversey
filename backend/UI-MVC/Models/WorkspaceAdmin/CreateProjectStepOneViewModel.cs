using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Microsoft.AspNetCore.Http;

namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class CreateProjectStepOneViewModel
{
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(2048)]
    public string ImageUrl { get; set; } = string.Empty;

    public IFormFile ImageFile { get; set; }

    public InteractionType InteractionForm { get; set; } = InteractionType.Chat;

    [Range(1, 5)]
    public int NudgingStrength { get; set; } = 3;

    public Status Status { get; set; } = Status.Active;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today;
}
