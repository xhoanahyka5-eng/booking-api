using MediatR;

namespace Booking.Application.Features.Reviews.CreateReview;

public record CreateReviewCommand(
    Guid GuestId,
    int BookingId,
    int Rating,
    string? Comment
) : IRequest<int>;