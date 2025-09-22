using System;
using System.Collections.Generic;
using System.Text;

namespace MyModularMonolith.Modules.Users.Application.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string firstName, string lastName, string? temporaryPassword = null, CancellationToken cancellationToken = default);
}
