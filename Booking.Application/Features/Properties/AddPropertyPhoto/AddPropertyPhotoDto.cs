namespace Booking.Application.Features.Properties.AddPropertyPhoto;

public class AddPropertyPhotoDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Base64Data { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}