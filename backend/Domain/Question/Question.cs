using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conversey.BL.Domain.Entities.Question;

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