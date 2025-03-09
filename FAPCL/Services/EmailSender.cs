using System.Net;
using System.Net.Mail;
using Ebay_Project_PRN.Helper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

public class EmailSender : IEmailSender
{
	private readonly EmailSettings _emailSettings;

	public EmailSender(IOptions<EmailSettings> emailSettings)
	{
		_emailSettings = emailSettings.Value;
	}

	public async Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
		{
			Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
			EnableSsl = true
		};

		var mailMessage = new MailMessage
		{
			From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
			Subject = subject,
			Body = htmlMessage,
			IsBodyHtml = true
		};

		mailMessage.To.Add(email);

		await client.SendMailAsync(mailMessage);
	}
}