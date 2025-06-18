using AutoMapper;
using GastroHub.Models;
using GastroHub.Dtos.Recipes;
using GastroHub.Dtos.Comments;
using GastroHub.Dtos.Categories;

namespace GastroHub.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Recipe, RecipeDto>()
                .ForMember(d => d.UserDisplayName, o => o.MapFrom(r => r.User.DisplayName))
                .ForMember(d => d.CategoryName, o => o.MapFrom(r => r.Category.Name))
                .ForMember(d => d.LikesCount, o => o.MapFrom(r => r.Likes.Count))
                .ForMember(d => d.IsFavorite, o => o.Ignore())
                .ForMember(d => d.LikedByCurrentUser, o => o.Ignore());

            CreateMap<Comment, CommentDto>()
                .ForMember(d => d.UserDisplayName, o => o.MapFrom(c => c.User.DisplayName))
                .ForMember(d => d.LikesCount, o => o.MapFrom(c => c.Likes.Count))
                .ForMember(d => d.LikedByCurrentUser, o => o.Ignore())
                .ForMember(d => d.Replies, o => o.MapFrom(c => c.Replies));

            CreateMap<Category, CategoryDto>();
        }
    }
}
