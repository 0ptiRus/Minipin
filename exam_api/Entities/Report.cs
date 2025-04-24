namespace exam_api.Entities;

public class Report
{
    public int Id { get; set; }
    
    public ApplicationUser User { get; set; }
    public int ReporterId { get; set; }
    
    public int ReportedItemId { get; set; }
    public string ReportedItemType { get; set; }
    
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}