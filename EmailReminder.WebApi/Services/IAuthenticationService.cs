using EmailReminder.Shared.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmailReminder.WebApi.Services
{
    public interface IAuthenticationService
    {
        Task<bool> RegisterUserAsync(string email);
        Task<bool> ConfirmUserAsync(EmailConfirmation confirmation);
        Task<bool> IsUserConfirmedAsync(string email);
        Task<bool> DoesUserExistAsync(string email);
        Task<string> GenerateLoginToken(string email);
        Task<bool> IsUserTokenValid(EmailConfirmation confirmation);
        string GenerateJwt(string email);
    }
}
