using Infrastructure.Entities;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public interface IReviewService
    {
        Task<IQueryable<Review>> GetReviews();
        Task<Review> GetReviewsByRatingId(int ratingId);
        Task<Review> GetReview(int id);
        Task InsertReview(Review review);
        Task UpdateReview(Review review);
        Task DeleteReview(Review review);
    }

    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task DeleteReview(Review review)
        {
            await _reviewRepository.DeleteAsync(review);
        }

        public async Task<IQueryable<Review>> GetReviews()
        {
            return await Task.FromResult(_reviewRepository.GetAll());
        }

        public async Task<Review> GetReview(int id)
        {
            return await _reviewRepository.GetByIdAsync(id);
        }

        public async Task InsertReview(Review review)
        {
            await _reviewRepository.InsertAsync(review);
        }

        public async Task UpdateReview(Review review)
        {
            await _reviewRepository.UpdateAsync(review);
        }

        public async Task<Review> GetReviewsByRatingId(int ratingId)
        {
            var review = _reviewRepository.GetAll().SingleOrDefault(e => e.RatingId == ratingId);
            return review;
        }
    }
}
