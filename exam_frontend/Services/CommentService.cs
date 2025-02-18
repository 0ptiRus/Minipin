using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_frontend.Services;

public class CommentService 
{
    private readonly AppDbContext context;

    public CommentService(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Comment>> GetComments()
    {
        return await context.Comments.ToListAsync();
    }

    public async Task<Comment> GetComment(int id)
    {
        Comment comment = await context.Comments.FindAsync(id);

        return comment;
    }

    // [HttpPost]
    // public async Task<ActionResult<Comment>> CreateComment(Comment comment)
    // {
    //     context.Comments.Add(comment);
    //     await context.SaveChangesAsync();
    //
    //     return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
    // }

    public async Task<Comment> UpdateComment(int id, Comment comment)
    {
        if (id != comment.Id)
            return null;

        context.Comments.Update(comment);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CommentExists(id))
                return null;
            else
                throw;
        }

        return comment;
    }

    public async Task<Comment> PostComment(int id, string user_id, string text)
    {
        Image image = await context.Images.FindAsync(id);
        if (image == null)
            return null;

        Comment comment = new Comment
        {
            ImageId = id,
            Text = text,
            UserId = user_id
        };

        image.Comments.Add(comment);
        await context.SaveChangesAsync();

        return comment;
    }

    public async Task DeleteComment(int id)
    {
        Comment comment = await context.Comments.FindAsync(id);
        if (comment == null)

        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
    }

    private bool CommentExists(int id)
    {
        return context.Comments.Any(e => e.Id == id);
    }
}