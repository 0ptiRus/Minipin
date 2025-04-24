namespace exam_admin.Models;

public class AdminPostModel
{
        public int Id { get; set; }
        public int GalleryId { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsFlagged { get; set; }
        public bool IsDeleted { get; set; }
        public string ImageUrl { get; set; }
        public string UserId { get; set; }
}