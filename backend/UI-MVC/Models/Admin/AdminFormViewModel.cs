namespace Conversey.UI_MVC.Models.Admin;

public class AdminFormViewModel<T>
{
    public T FormItem { get; init; }
    public string FormAction { get; init; }
    public string SubmitLabel { get; init; } 
}