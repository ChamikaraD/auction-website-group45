using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

// Make sure the namespace matches your project structure
namespace Project_1.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        // Optional: Inject ILogger if you want to log success/failure
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Read the API key from secrets.json
            var apiKey = _configuration["SendGridKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGridKey is not configured in secrets.json.");
                // Decide how to handle: throw exception, log error, or silently fail
                return; // Silently fail for now if key is missing
            }

            var client = new SendGridClient(apiKey);

            // --- IMPORTANT: Replace with YOUR verified sender email ---
            var from = new EmailAddress("numislive2@gmail.com", "NumisLive Auctions");
            // -----------------------------------------------------------

            var to = new EmailAddress(email);
            // Create the email message (plain text version is empty here for simplicity)
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            try
            {
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Email to {email} queued successfully! Subject: {subject}");
                }
                else
                {
                    // Log detailed error from SendGrid
                    string responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError($"Failed to send email to {email}. Status Code: {response.StatusCode}. Response: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while sending email to {email}.");
            }
        }
    }
}