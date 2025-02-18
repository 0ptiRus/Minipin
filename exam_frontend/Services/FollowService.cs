using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using exam_frontend.Entities;

namespace exam_frontend.Services
{
    public class FollowService
    {
        private readonly ApiDbContext context;

        public FollowService(ApiDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Follow>> GetFollowers()
        {
            return await context.Follows.ToListAsync();
        }

        public async Task<Follow> GetFollower(int id)
        {
            var follower = await context.Follows.FindAsync(id);

            return follower;
        }

        public async Task<Follow> PostFollower(Follow follow)
        {
            context.Follows.Add(follow);
            await context.SaveChangesAsync();

            return follow;

        }

        public async Task DeleteFollower(int id)
        {
            Follow? follower = await context.Follows.FindAsync(id);
            if (follower == null)
            {
               
            }

            context.Follows.Remove(follower);
            await context.SaveChangesAsync();
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