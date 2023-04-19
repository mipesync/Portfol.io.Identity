using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Portfol.io.Identity.Common.TokenIssue;
using Portfol.io.Identity.Interfaces;
using Portfol.io.Identity.Models;
using Portfol.io.Identity.Repositories;
using Portfol.io.Identity.ViewModels;
using Portfol.io.Identity.ViewModels.ResponseModels;
using Portfol.io.Identity.ViewModels.ResponseModels.AuthResponseModels;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Portfol.io.Identity.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthRepository _authRepository;
        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpGet("oauth-vk")]
        public async Task<IActionResult> OAuthVK()
        {
            var properties = new AuthenticationProperties()
            {
                RedirectUri = "/oauth-vk",
                Items =
                {
                    { "LoginProvider", "VK" },
                },
                AllowRefresh = true
            };

            return Challenge(properties, "VK");
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <remarks> 
        /// Issues an access token and, if "remember me" is true, issues a refresh token. Sample request:
        /// 
        ///     POST: /api/auth/login
        ///     {
        ///         "username": "user",
        ///         "password": "Abcd_123",
        ///         "rememberMe": "true",
        ///         "returnUrl": "http://example.com/catalog"
        ///     }
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Returns <see cref="LoginResponse"/></returns>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">If invalid login attempt. </response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="403">If email not confimed. </response>
        /// <response code="403">If user account locked out. </response>
        /// <response code="200">Success</response>

        [HttpPost("login")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(LoginResponse))]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            var result = await _authRepository.Login(model);

            return Ok(result);
        }

        /// <summary>
        /// Registers a user
        /// </summary>
        /// <remarks>
        /// Password must be at least 8 characters, contains non-alphanumeric, digit and uppercase. 
        /// The role field is taken from:<br/>
        /// - author: 2 <br/>
        /// - employer: 3 <br/>
        /// 
        /// Sample request:
        /// 
        ///     POST: /api/auth/register
        ///     {
        ///         "username": "user",
        ///         "email": "user@example.com",
        ///         "password": "Abcd_123",
        ///         "role": 1,
        ///         "hostUrl": "http://frontend.com/",
        ///         "returnUrl": "http://example.com/catalog"
        ///     }
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Returns <see cref="RegisterResponse"/></returns>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">If user with this email already exists. </response>
        /// <response code="400">If there were errors. </response>
        /// <response code="200">Success</response>
        /// <response code="204">If none of the conditions are met.</response>

        [HttpPost("register")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(RegisterResponse))]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            var result = await _authRepository.Register(model);

            return Ok(result);
        }

        /// <summary>
        /// When the user is forgot the password.
        /// </summary>
        /// <remarks>
        /// Sends a confirmation email. Sample request: 
        /// 
        ///     GET: /api/auth/forgot_password?email=user@example.com
        /// </remarks>
        /// <param name="email">The user's email address to which the confirmation email will be sent.</param>
        /// <param name="host">Frontend host url</param>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">If user does not exist or is not confirmed. </response>
        /// <response code="200">Success</response>

        [HttpGet("forgot_password")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        public async Task<IActionResult> ForgotPassword(string email, string host)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            await _authRepository.ForgotPassword(email, host);

            return Ok();
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <remarks>
        /// Accepts the code that was sent to the mail. 
        /// The page address must contain ".../resetPassword". Example: http://example.com/resetPassword
        /// Sample request:
        /// 
        ///     POST: /api/auth/reset_password
        ///     {
        ///         "email": "user@example.com",
        ///         "code": "your code",
        ///         "password": "Abcd_123"
        ///     }
        /// </remarks>
        /// <param name="model"></param>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">if there were errors during password reset. </response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>
        /// <response code="204">If none of the conditions are met.</response>

        [HttpPost("reset_password")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            await _authRepository.ResetPassword(model);

            return Ok();
        }

        /// <summary>
        /// Email confirmation.
        /// </summary>
        /// <remarks>
        /// The page address must contain ".../confirmEmail". Example: http://example.com/confirmEmail
        /// Sample request:
        /// 
        ///     POST: /api/auth/confirm_email?userId=4C2C522E-F785-4EB4-8ED7-260861453330&amp;code=your_code&amp;returnUrl=http://example.com/catalog
        /// </remarks>
        /// <returns>Returns <see cref="ConfirmEmailResponse"/></returns>
        /// <response code="400">if an error occurred while email confirmation.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="200">Success</response>
        /// <response code="204">If userId or code is null.</response>

        [HttpPost("confirm_email")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(ConfirmEmailResponse))]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string? returnUrl)
        {
            var result = await _authRepository.ConfirmEmail(userId, code, returnUrl);

            return Ok(result);
        }

        /// <summary>
        /// Resends the confirmation email.
        /// </summary>
        /// <remarks>
        /// Resends the confirmation email if the previous one was not delivered.
        /// Sample request:
        /// 
        ///     GET: /api/auth/reconfirm_email?email=user@example.com&amp;returnUrl=http://example.com/catalog
        /// </remarks>
        /// <param name="email">The user's email address to which the confirmation email will be sent.</param>
        /// <param name="returnUrl">The return URL to which the user will be returned after confirmation of the mail.</param>
        /// <param name="host">Frontend host url</param>
        /// <response code="400">If model is not valid.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="200">Success</response>

        [HttpGet("reconfirm_email")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ResendEmailConfirmation(string email, string? returnUrl, string host)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            await _authRepository.ResendEmailConfirmation(email, returnUrl, host);

            return Ok();
        }

        /// <summary>
        /// Refreshes the access token and refresh token.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT: /api/auth/refresh_token?token=your refresh token
        /// </remarks>
        /// <returns>Returns <see cref="RefreshTokenResponse"/></returns>
        /// <response code="400">Некорректные входные данные</response>
        /// <response code="400">Invalid token</response>
        /// <response code="400">Wrong refresh token</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="200">Success</response>
        [HttpPut("refresh_token")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(RefreshTokenResponse))]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> RefreshToken(string refresh)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            var result = await _authRepository.RefreshToken(refresh);

            return Ok(result);
        }

        /// <summary>
        /// Refresh token revocation.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE: /api/auth/revoke?userId=4C2C522E-F785-4EB4-8ED7-260861453330
        /// </remarks>
        /// <param name="userId">Id of the user to revoke the refresh token from.</param>
        /// <response code="200">Succeess</response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="400">if there were errors during password reset. </response>
        /// <response code="404">If the user is not found. </response>

        [HttpDelete("revoke")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> Revoke(string userId)
        {
            await _authRepository.Revoke(userId);

            return Ok();
        }
    }
}
