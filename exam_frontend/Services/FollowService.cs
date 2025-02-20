using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using exam_frontend.Entities;
using exam_frontend.Models;

namespace exam_frontend.Services
{
    public class FollowService
    {
        private readonly AppDbContext context;

        public FollowService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IList<UserModel>> GetFollowers(string user_id) => await context.Follows
                .Include(f => f.Followed)
                .Where(f => f.FollowedId == user_id)
                .Select(f => new UserModel(f.FollowerId, f.Follower.UserName))
                .ToListAsync();

        public async Task<IList<UserModel>> GetFollowed(string user_id) => await context.Follows
            .Include(f => f.Follower)
            .Where(f => f.FollowerId == user_id)
            .Select((f => new UserModel(f.FollowedId, f.Followed.UserName)))
            .ToListAsync();

        public async Task<Follow> GetFollower(int id)
        {
            var follower = await context.Follows.FindAsync(id);

            return follower;
        }

        public async Task<Follow> PostFollower(string follower_id, string followed_id)
        {
            Follow follow = new(follower_id, followed_id);
            context.Follows.Add(follow);
            await context.SaveChangesAsync();

            return follow;

        }

        public async Task<bool> DeleteFollower(string follower_id, string followed_id)
        {
            Follow? follower = await context.Follows
                .SingleOrDefaultAsync(f => f.FollowerId == follower_id
                                           && f.FollowedId == followed_id);
            if (follower == null)
            {
                return false;
            }

            context.Follows.Remove(follower);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Follow> PutFollower(int id, Follow follow)
        {
            if (id != follow.Id)
            {
                return null;
            }

            context.Entry(follow).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FollowExists(id))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return follow;
        }

        private bool FollowExists(int id)
        {
            return context.Follows.Any(e => e.Id == id);
        }
    }
}