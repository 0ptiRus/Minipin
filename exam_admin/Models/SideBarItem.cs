using Microsoft.AspNetCore.Components;

namespace exam_admin.Models;

// Models/SidebarItem.cs
public class SidebarItem
{
    public string Text { get; set; }
    public string Icon { get; set; } = "fas fa-circle";
    public string? Url { get; set; }      // for navigation
    public string OnClick { get; set; } // optional callback
    public int? Count { get; set; }       // optional count/badge
    public bool IsActive { get; set; }    // for custom active styling
}
