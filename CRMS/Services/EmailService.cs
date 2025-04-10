using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading.Tasks;

public class EmailService
{
    public async Task SendEmailAsync(string email, string password)
    {
        try
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("CrimeVault", "crimevault2@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Your Account Credentials";

            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Your account has been created.\n\nEmail: {email}\nPassword: {password}\n\nPlease change your password on first login."
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient()) // ✅ Explicitly specify MailKit
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("crimevault2@gmail.com", "jrmw qjez beny hrex");  // Use your App Password here
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }

            Console.WriteLine($"✅ Email sent successfully to {email}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Email sending failed: {ex.Message}");
        }
    }
}
