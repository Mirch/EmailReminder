using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using EmailReminder.Shared.Models;
using EmailReminder.WebApi.Data;
using EmailReminder.WebApi.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmailReminder.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemindersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IAuthenticationService _authenticationService;

        public RemindersController(
            ApplicationDbContext context,
            IBackgroundJobClient backgroundJobClient,
            IAuthenticationService authenticationService)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _authenticationService = authenticationService;
        }

        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserReminders()
        {
            var claim = User
                .Claims
                .Where(c => c.Type == JwtRegisteredClaimNames.Sub)
                .FirstOrDefault();

            if (claim == null)
            {
                return Unauthorized();
            }

            var user = await _context
                .Users
                .SingleOrDefaultAsync(u => u.Email == claim.Value);

            if (user == null)
            {
                return Unauthorized();
            }

            var reminders = await _context
                .Reminders
                .Where(r => r.EmailAddress == user.Email && r.DateTime > DateTime.Now.Date)
                .ToListAsync();

            return Ok(reminders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReminder([FromBody]Reminder reminder)
        {
            try
            {
                _context.Reminders.Add(reminder);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Could not save reminder.");
            }

            var exists = await _authenticationService.DoesUserExistAsync(reminder.EmailAddress);

            if (!exists)
            {
                await _authenticationService.RegisterUserAsync(reminder.EmailAddress);
            }

            _backgroundJobClient.Schedule<IMailSender>(x => x.SendReminderAsync(reminder), new DateTimeOffset(reminder.DateTime));
            return Ok(reminder);
        }
    }
}