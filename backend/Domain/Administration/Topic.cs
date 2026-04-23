using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Domain.Administration;

public class Topic
{
    [Required]
    public int Id { get; set; }
    public Project Project { get; set; }
    public string Name { get; set; }
    public string Context { get; set; }
    public IEnumerable<Idea> Ideas { get; set; }
    [Range(0, 20)]
    public int MaxBroadSelectionLoads { get; set; } = 3;
}