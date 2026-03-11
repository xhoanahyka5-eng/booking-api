using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Reviews.Persistence;
using Booking.Domain.Entities.Bookings;
using Booking.Domain.Entities.Reviews;
using MediatR;

namespace Booking.Application.Features.Reviews.CreateReview;

public class CreateReviewCommandHandler
    : IRequestHandler<CreateReviewCommand, int>
{
    private readonly IReviewRepository _reviewRepository;

    public CreateReviewCommandHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<int> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _reviewRepository.GetBookingByIdAsync(
            request.BookingId,
            cancellationToken);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        if (booking.GuestId != request.GuestId)
            throw new UnauthorizedException("You can review only your own booking.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (booking.EndDate > today)
            throw new ConflictException("Review can be added only after the stay ends.");

        if (booking.BookingStatus != BookingStatus.Confirmed &&
            booking.BookingStatus != BookingStatus.Completed)
        {
            throw new ConflictException("Only confirmed or completed bookings can be reviewed.");
        }

        var alreadyReviewed = await _reviewRepository.HasReviewForBookingAsync(
            request.BookingId,
            cancellationToken);

        if (alreadyReviewed)
            throw new ConflictException("This booking already has a review.");

        var review = new Review
        {
            BookingId = request.BookingId,
            GuestId = request.GuestId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviewRepository.AddReviewAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        return review.Id;
    }
}