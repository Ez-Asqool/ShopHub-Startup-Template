using AutoMapper;
using myshop.Application.Services.Review.Dto;
using myshop.Domain.Entities;

namespace myshop.Application.Mapping
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            CreateMap<Review, ReviewDto>();
            CreateMap<ReviewCreateDto, Review>();
        }
    }
}
