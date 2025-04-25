using System.Collections.Generic;

namespace exam_api.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Post> Posts { get; set; }
}
