using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Entities.Question;

public class Question
{
    [Required]
    public int Id { get; set; }

    private string Text { get; set; }
    private int Order { get; set; }
    public Image? Image { get; set; }

    /*
     public Question()
    {
        NotImplementedException();
    }
    */
}