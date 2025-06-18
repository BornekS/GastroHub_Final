using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GastroHub.Data;
using GastroHub.Dtos.Comments;
using GastroHub.Models;
using GastroHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CommentService(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<CommentDto>> GetForRecipeAsync(int recipeId, string? userEmail)
    {
        var currentUserId = userEmail == null
            ? null
            : await _db.Users
                       .Where(u => u.Email == userEmail)
                       .Select(u => u.Id)
                       .SingleOrDefaultAsync();

        var comments = await _db.Comments
            .Where(c => c.RecipeId == recipeId && c.ParentCommentId == null)
            .Include(c => c.User)
            .Include(c => c.Likes)
            .Include(c => c.Replies!)
                .ThenInclude(r => r.User)
            .Include(c => c.Replies!)
                .ThenInclude(r => r.Likes)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var dtos = _mapper.Map<List<CommentDto>>(comments);

        void SetLikes(IEnumerable<CommentDto> list, IEnumerable<Comment> backing)
        {
            foreach (var dto in list)
            {
                var entity = backing.First(c => c.Id == dto.Id);
                dto.LikedByCurrentUser = currentUserId != null &&
                                         entity.Likes.Any(l => l.UserId == currentUserId);

                if (dto.Replies?.Any() == true)
                    SetLikes(dto.Replies, entity.Replies);
            }
        }
        SetLikes(dtos, comments);

        return dtos;
    }
    public async Task<CommentDto> AddAsync(int recipeId, CreateCommentDto dto, string userEmail)
    {
        var user = await _db.Users
            .SingleOrDefaultAsync(u => u.Email == userEmail)
            ?? throw new KeyNotFoundException("User not found");

        var comment = new Comment
        {
            RecipeId = recipeId,
            Content = dto.Content,
            UserId = user.Id,
            ParentCommentId = dto.ParentCommentId
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var loaded = await _db.Comments
            .Include(c => c.User)
            .FirstAsync(c => c.Id == comment.Id);

        var result = _mapper.Map<CommentDto>(loaded);
        result.LikesCount = 0;
        result.LikedByCurrentUser = false;
        result.Replies = new List<CommentDto>();
        return result;
    }
    public async Task LikeAsync(int commentId, string userEmail)
    {
        var userId = await _db.Users
            .Where(u => u.Email == userEmail)
            .Select(u => u.Id)
            .SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("User not found");

        if (await _db.CommentLikes
                     .AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId))
            return;

        _db.CommentLikes.Add(new CommentLike { CommentId = commentId, UserId = userId });
        await _db.SaveChangesAsync();
    }

    public async Task UnlikeAsync(int commentId, string userEmail)
    {
        var userId = await _db.Users
            .Where(u => u.Email == userEmail)
            .Select(u => u.Id)
            .SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("User not found");

        var like = await _db.CommentLikes
            .SingleOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

        if (like is null) return;

        _db.CommentLikes.Remove(like);
        await _db.SaveChangesAsync();
    }
}
