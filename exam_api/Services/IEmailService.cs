using System.Threading.Tasks;

namespace exam_api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}