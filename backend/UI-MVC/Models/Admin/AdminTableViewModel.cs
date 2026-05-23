using Microsoft.AspNetCore.Html;

namespace Conversey.UI_MVC.Models.Admin;

public class AdminTableViewModel<T>
{
    public string SectionTitle { get; init; } = "Items";
    public string TableDescription { get; init; } = "Overview";
    public string CreateUrl { get; init; } = "#";
    public string CreateButtonLabel { get; init; } = "Add item";
    public string CreateButtonId { get; init; } = string.Empty;
    public string CreateButtonModalKey { get; init; } = string.Empty;
    public string TableBodyId { get; init; } = string.Empty;
    public string EmptyState { get; init; } = "No items yet.";
    
    public IReadOnlyList<AdminTableColumn<T>> Columns { get; init; } = [];
    public IReadOnlyList<T> Items { get; init; } = [];
    public Func<dynamic, string> RowId { get; init; }
}

public class AdminTableColumn<T>
{
    public string Header { get; init; } = "";
    public Func<T, IHtmlContent> Template { get; init; } = _ => HtmlString.Empty;
}
