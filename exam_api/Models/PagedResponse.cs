namespace exam_api.Models;


public class PagedResponse<T>
{
    public List<T> Items { get; set; }
    public int TotalItems { get; set; }
}