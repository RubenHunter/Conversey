namespace Conversey.UI_MVC.Models.Admin;

public class AdminTableViewModel
{
    public string SectionTitle { get; init; } = "Items";
    public string TableDescription { get; init; } = "Overview";
    public string CreateUrl { get; init; } = "#";
    public string CreateButtonLabel { get; init; } = "Add item";
    public string EmptyState { get; init; } = "No items yet.";
    public IReadOnlyList<string> ColumnHeaders { get; init; } = [];
    public IReadOnlyList<AdminTableRowViewModel> Rows { get; init; } = [];
}

public class AdminTableRowViewModel
{
    public IReadOnlyList<string> Cells { get; init; } = [];
}
