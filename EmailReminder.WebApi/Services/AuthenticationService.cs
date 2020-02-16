using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using EmailReminder.Shared.Models;
using EmailReminder.WebApi.Data;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EmailReminder.WebApi.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuthenticationService(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context,
            IBackgroundJobClient backgroundJobClient)
        {
            _userManager = userManager;
            _context = context;
            _backgroundJobClient = backgroundJobClient;
        }
        public async Task<bool> RegisterUserAsync(string email)
        {
            var userExists = await _context
               .Users
               .AnyAsync(u => u.Email == email);

            if (userExists)
            {
                return false;
            }

            var user = new IdentityUser() { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return false;
            }

            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            _backgroundJobClient.Enqueue<IMailSender>(x => x.SendVerification(email, emailToken));

            return true;
        }

        public async Task<bool> ConfirmUserAsync(EmailConfirmation confirmation)
        {
            var dbUser = await _userManager.FindByEmailAsync(confirmation.Email);

            if (dbUser == null)
            {
                return false;
            }

            var isAlreadyConfirmed = await _userManager.IsEmailConfirmedAsync(dbUser);

            if (isAlreadyConfirmed)
            {
                return false;
            }

            var result = await _userManager.ConfirmEmailAsync(dbUser, confirmation.Token);

            return result.Succeeded;
        }


        public async Task<bool> IsUserConfirmedAsync(string email)
        {
            var dbUser = await _userManager.FindByEmailAsync(email);
            if (dbUser == null)
            {
                return false;
            }

            var isUserConfirmed = await _userManager.IsEmailConfirmedAsync(dbUser);

            return isUserConfirmed;
        }

        public async Task<bool> DoesUserExistAsync(string email)
        {
            var exists = await _context
                .Users
                .AnyAsync(u => u.Email == email);

            return exists;
        }

        public async Task<string> GenerateLoginToken(string email)
        {
            var dbUser = await _userManager.FindByEmailAsync(email);

            if (dbUser == null)
            {
                throw new Exception("User could not be found.");
            }

            var token = await _userManager.GenerateUserTokenAsync(dbUser, "Default", "passwordless-login");
            return token;
        }

        public async Task<bool> IsUserTokenValid(EmailConfirmation confirmation)
        {
            var dbUser = await _userManager.FindByEmailAsync(confirmation.Email);

            if (dbUser == null)
            {
                return false;
            }

            var isValid = await _userManager.VerifyUserTokenAsync(dbUser, "Default", "passwordless-login", confirmation.Token);

            return isValid;
        }

        public string GenerateJwt(string email)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("placeholder-key-that-is-long-enough-for-sha256"));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
            };
            var jwt = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}
