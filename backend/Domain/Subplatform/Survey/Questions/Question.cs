using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions;

public class Question
{
    [Required]
    public int Id { get; set; }

    [StringLength(500)]
    private string Text { get; set; }
    private int Order { get; set; }
    [NotMapped]
    public Image? Image { get; set; }

    /*
     public Question()
    {
        NotImplementedException();
    }
    */
}