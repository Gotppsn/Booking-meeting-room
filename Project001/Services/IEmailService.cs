using Project001.Models.Entities;

namespace Project001.Services;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(Booking booking);
    Task SendBookingCancellationAsync(Booking booking);
    Task SendBookingReminderAsync(Booking booking);
    Task SendBookingUpdateAsync(Booking booking);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendBookingConfirmationAsync(Booking booking)
    {
        try
        {
            var subject = $"Booking Confirmation - {booking.Title}";
            var body = GenerateBookingConfirmationEmail(booking);
            
            await SendEmailAsync(booking.User.Email, subject, body);
            _logger.LogInformation("Booking confirmation email sent to {Email} for booking {BookingId}", 
                booking.User.Email, booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingId}", booking.Id);
        }
    }

    public async Task SendBookingCancellationAsync(Booking booking)
    {
        try
        {
            var subject = $"Booking Cancelled - {booking.Title}";
            var body = GenerateBookingCancellationEmail(booking);
            
            await SendEmailAsync(booking.User.Email, subject, body);
            _logger.LogInformation("Booking cancellation email sent to {Email} for booking {BookingId}", 
                booking.User.Email, booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking cancellation email for booking {BookingId}", booking.Id);
        }
    }

    public async Task SendBookingReminderAsync(Booking booking)
    {
        try
        {
            var subject = $"Meeting Reminder - {booking.Title}";
            var body = GenerateBookingReminderEmail(booking);
            
            await SendEmailAsync(booking.User.Email, subject, body);
            _logger.LogInformation("Booking reminder email sent to {Email} for booking {BookingId}", 
                booking.User.Email, booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking reminder email for booking {BookingId}", booking.Id);
        }
    }

    public async Task SendBookingUpdateAsync(Booking booking)
    {
        try
        {
            var subject = $"Booking Updated - {booking.Title}";
            var body = GenerateBookingUpdateEmail(booking);
            
            await SendEmailAsync(booking.User.Email, subject, body);
            _logger.LogInformation("Booking update email sent to {Email} for booking {BookingId}", 
                booking.User.Email, booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking update email for booking {BookingId}", booking.Id);
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // In a real implementation, you would use a service like SendGrid, SMTP, etc.
        // For now, we'll just log the email details
        
        _logger.LogInformation("EMAIL NOTIFICATION:\nTo: {ToEmail}\nSubject: {Subject}\nBody: {Body}", 
            toEmail, subject, body);
        
        // Simulate async email sending
        await Task.Delay(100);
        
        // TODO: Implement actual email sending logic here
        // Example with SMTP:
        // using var client = new SmtpClient(_configuration["Email:SmtpServer"], int.Parse(_configuration["Email:Port"]));
        // client.Credentials = new NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"]);
        // await client.SendMailAsync(new MailMessage(_configuration["Email:FromAddress"], toEmail, subject, body) { IsBodyHtml = true });
    }

    private string GenerateBookingConfirmationEmail(Booking booking)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2563EB; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .booking-details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Booking Confirmed</h1>
        </div>
        <div class=""content"">
            <p>Dear {booking.User.FullName},</p>
            <p>Your meeting room booking has been confirmed. Here are the details:</p>
            
            <div class=""booking-details"">
                <h3>{booking.Title}</h3>
                <p><strong>Room:</strong> {booking.Room.Name}</p>
                <p><strong>Date:</strong> {booking.StartDateTime:dddd, MMMM dd, yyyy}</p>
                <p><strong>Time:</strong> {booking.StartDateTime:h:mm tt} - {booking.EndDateTime:h:mm tt}</p>
                <p><strong>Duration:</strong> {booking.Duration.TotalHours:F1} hours</p>
                {(string.IsNullOrEmpty(booking.Description) ? "" : $"<p><strong>Description:</strong> {booking.Description}</p>")}
                {(booking.AttendeeCount.HasValue ? $"<p><strong>Expected Attendees:</strong> {booking.AttendeeCount} people</p>" : "")}
            </div>
            
            <p>If you need to make any changes or cancel this booking, please log in to the meeting room system.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br>Meeting Room Booking System</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingCancellationEmail(Booking booking)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #DC2626; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .booking-details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Booking Cancelled</h1>
        </div>
        <div class=""content"">
            <p>Dear {booking.User.FullName},</p>
            <p>Your meeting room booking has been cancelled:</p>
            
            <div class=""booking-details"">
                <h3>{booking.Title}</h3>
                <p><strong>Room:</strong> {booking.Room.Name}</p>
                <p><strong>Date:</strong> {booking.StartDateTime:dddd, MMMM dd, yyyy}</p>
                <p><strong>Time:</strong> {booking.StartDateTime:h:mm tt} - {booking.EndDateTime:h:mm tt}</p>
                {(string.IsNullOrEmpty(booking.CancellationReason) ? "" : $"<p><strong>Reason:</strong> {booking.CancellationReason}</p>")}
            </div>
            
            <p>The room is now available for other bookings. If you need to reschedule, please create a new booking.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br>Meeting Room Booking System</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingReminderEmail(Booking booking)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #F59E0B; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .booking-details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Meeting Reminder</h1>
        </div>
        <div class=""content"">
            <p>Dear {booking.User.FullName},</p>
            <p>This is a reminder for your upcoming meeting:</p>
            
            <div class=""booking-details"">
                <h3>{booking.Title}</h3>
                <p><strong>Room:</strong> {booking.Room.Name}</p>
                <p><strong>Date:</strong> {booking.StartDateTime:dddd, MMMM dd, yyyy}</p>
                <p><strong>Time:</strong> {booking.StartDateTime:h:mm tt} - {booking.EndDateTime:h:mm tt}</p>
                <p><strong>Location:</strong> {booking.Room.Location}</p>
                {(string.IsNullOrEmpty(booking.Description) ? "" : $"<p><strong>Description:</strong> {booking.Description}</p>")}
            </div>
            
            <p>Please ensure you arrive on time and that the room is properly set up for your meeting.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br>Meeting Room Booking System</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingUpdateEmail(Booking booking)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10B981; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .booking-details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Booking Updated</h1>
        </div>
        <div class=""content"">
            <p>Dear {booking.User.FullName},</p>
            <p>Your meeting room booking has been updated. Here are the current details:</p>
            
            <div class=""booking-details"">
                <h3>{booking.Title}</h3>
                <p><strong>Room:</strong> {booking.Room.Name}</p>
                <p><strong>Date:</strong> {booking.StartDateTime:dddd, MMMM dd, yyyy}</p>
                <p><strong>Time:</strong> {booking.StartDateTime:h:mm tt} - {booking.EndDateTime:h:mm tt}</p>
                <p><strong>Duration:</strong> {booking.Duration.TotalHours:F1} hours</p>
                {(string.IsNullOrEmpty(booking.Description) ? "" : $"<p><strong>Description:</strong> {booking.Description}</p>")}
            </div>
            
            <p>Please review the updated information and make note of any changes.</p>
        </div>
        <div class=""footer"">
            <p>Best regards,<br>Meeting Room Booking System</p>
        </div>
    </div>
</body>
</html>";
    }
}