using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailReminder.Shared.Models;
using EmailReminder.WebApi.Data;
using EmailReminder.WebApi.Services;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmailReminder.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IBackgroundJobClient backgroundJobClient)
        {
            _authenticationService = authenticationService;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmUser([FromBody]EmailConfirmation confirmation)
        {
            var result = await _authenticationService.ConfirmUserAsync(confirmation);

            if (!result)
            {
                return BadRequest("Could not confirm user.");
            }

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]EmailConfirmation confirmation)
        {
            var result = await _authenticationService.IsUserTokenValid(confirmation);

            if (!result)
            {
                return BadRequest("Could not login user.");
            }

            var jwt = _authenticationService.GenerateJwt(confirmation.Email);

            return Ok(jwt);
        }

        [HttpGet("loginToken")]
        public async Task<IActionResult> GetLoginToken(string email)
        {
            string loginToken = "";
            try
            {
                loginToken = await _authenticationService.GenerateLoginToken(email);
            }
            catch
            {
                return BadRequest("Could not generate login token.");
            }

            _backgroundJobClient.Enqueue<IMailSender>(x => x.SendLoginToken(email, loginToken));

            return NoContent();
        }
    }
}