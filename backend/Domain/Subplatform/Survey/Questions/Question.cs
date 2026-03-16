using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions;

public class Question
{
    [Required]
    public int Id { get; set; }

    [StringLength(500)]
    public string Text { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    [NotMapped]
    public Image? Image { get; set; }

    public Project Project { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}

public class QuestionOption
{
    [Required]
    public int Id { get; set; }

    [StringLength(250)]
    public string Text { get; set; }

    public int Order { get; set; }
    public Question Question { get; set; }
}
