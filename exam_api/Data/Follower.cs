namespace exam_api.Data;

public class Follow
{
    public int Id { get; set; }
    
    public ApplicationUser Follower { get; set; }
    public string FollowerId { get; set; }
    
    public ApplicationUser Followed { get; set; }
    public string FollowedId { get; set; }

    public Follow(string followerId, string followedId)
    {
        FollowerId = followerId;
        FollowedId = followedId;
    }
}