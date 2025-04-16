using System.Collections;
using Microsoft.AspNetCore.Identity;
namespace exam_frontend.Entities;
public class ApplicationUser : IdentityUser
{
    public IEnumerable Galleries { get; set; }
    public IEnumerable Posts { get; set; }
    public IEnumerable Followers { get; set; }
    public IEnumerable Followed { get; set; }
    
    public UploadedFile Pfp { get; set; }
}