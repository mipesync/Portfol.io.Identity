using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Org.BouncyCastle.Asn1.Ocsp;
using Portfol.io.Identity.Interfaces;
using Portfol.io.Identity.Models;
using Portfol.io.Identity.ViewModels;
using System.Text;
using System.Text.Encodings.Web;
using IO = System.IO;

namespace Portfol.io.Identity.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AuthController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IFileUploader _fileUploader;
        private readonly IWebHostEnvironment _environment;

        public UserController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AuthController> logger, IMapper mapper, IEmailSender emailSender, IFileUploader fileUploader, IWebHostEnvironment environment)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _mapper = mapper;
            _emailSender = emailSender;
            _fileUploader = fileUploader;
            _environment = environment;
        }

        /// <summary>
        /// Get a list of available roles
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// GET /get_roles
        /// </remarks>
        /// <returns>Returns
        /// {
        ///     "roles": [
        ///         {
        ///             "id": "string",
        ///             "name": "string"
        ///         },
        ///         ...
        ///     ]
        /// }
        /// </returns>
        /// <response code="404">If roles not found. With JSON message.</response>
        /// <response code="200">Success</response>

        [HttpGet("get_roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public IActionResult GetRolesList()
        {
            var roles = _roleManager.Roles.Where(o => new List<string> { "employee", "employer" }.Contains(o.Name))
                .ProjectTo<RoleLookupDto>(_mapper.ConfigurationProvider).ToList();

            if (roles.Count() == 0) return NotFound(new { message = "Roles not found." });

            return Ok(new RoleViewModel
            {
                Roles = roles
            });
        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// GET /get_user_by_id?userId="guid"
        /// </remarks>
        /// <param name="userId"></param>
        /// <returns>Returns
        /// {
        ///     "id": "string",
        ///     "username": "string",
        ///     "name": "string",
        ///     "email": "string",
        ///     "phone": "string",
        ///     "description": "string",
        ///     "profileImagePath": "string",
        ///     "dateOfBirth": "dateTime",
        ///     "dateOfCreation": "dateTime"
        /// }
        /// </returns>
        /// <response code="404">If user not found. With JSON message.</response>
        /// <response code="200">Success</response>

        [HttpGet("get_user_by_id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new { message = "User not found." });

            return Ok(_mapper.Map<UserLookupDto>(user));
        }

        /// <summary>
        /// Get a list of user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// GET /get_users
        /// </remarks>
        /// <returns>Returns
        /// {
        ///     "users": [
        ///         {
        ///             "id": "string",
        ///             "username": "string",
        ///             "name": "string",
        ///             "email": "string",
        ///             "phone": "string",
        ///             "description": "string",
        ///             "profileImagePath": "string",
        ///             "dateOfBirth": "dateTime",
        ///             "dateOfCreation": "dateTime"
        ///         },
        ///         ...
        ///     ]
        /// }
        /// </returns>
        /// <response code="404">If user not found. With JSON message.</response>
        /// <response code="200">Success</response>

        [HttpGet("get_users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users.ProjectTo<UserLookupDto>(_mapper.ConfigurationProvider).ToList();

            if (users.Count == 0) return NotFound(new { message = "Users not found." });

            return Ok(new UsersViewModel
            {
                Users = users
            });
        }

        [HttpGet("change_email")]
        public async Task<IActionResult> ChangeEmail(string userId, string newEmail)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new { message = "User is found." });

            var code = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var actionDesc = ControllerContext.ActionDescriptor;
            var callbackUrl = Url.ActionLink(nameof(ConfirmChangeEmail), $"{actionDesc.ControllerName}",
                values: new { userId = userId, newEmail = newEmail, code = code });

            await _emailSender.SendEmailAsync(
                user.Email,
                "Change email",
                $"Please change your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            return Ok();
        }

        [HttpPost("confirm_change_email")]
        public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string code, IFormFile formFile)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new { message = "User not found." });

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ChangeEmailAsync(user, newEmail, code);

            if (result.Succeeded)
            {
                _logger.LogInformation("Email has been changed.");
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new { message = error.Description });
            }

            return NoContent();
        }

        [HttpGet("change_phone")]
        public async Task<IActionResult> ChangePhone(string userId, string newPhoneNumber)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new { message = "User not found." });

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, newPhoneNumber);

            //TODO: Реализовать отправку кода на номер телефона

            return Ok(new {message = $"Your confirm code: {code}"});
        }

        [HttpPost("confirm_change_phone")]
        public async Task<IActionResult> ConfirmChangePhone([FromBody] ConfirmChangePhoneViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null) return NotFound(new { message = "User not found." });

            var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);

            if (result.Succeeded)
            {
                _logger.LogInformation("Phone number has been changed.");
                return Ok();
            }
            foreach(var error in result.Errors)
            {
                return BadRequest(new { message = error.Description });
            }

            return NoContent();
        }

        [HttpPut("update_user_details")]
        public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDetailsViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null) return NotFound(new { message = "User not found." });

            user.Name = model.Name;
            user.Description = model.Description;
            user.DateOfBirth = model.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User details has been updated.");
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new { message = error.Description });
            }

            return NoContent();
        }

        [HttpPut("update_profile_image")]
        public async Task<IActionResult> UpdateProfileImage(IFormFile file, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new { message = "User not found." });

            if (user.ProfileImagePath != "/ProfileImages/default.png") IO.File.Delete(user.ProfileImagePath);

            if (file is default(IFormFile)) return BadRequest(new {message = "File is empty."});

            _fileUploader.File = file;
            _fileUploader.AbsolutePath = $"{_environment.WebRootPath}/ProfileImages/{userId}/";

            var filePath = await _fileUploader.UploadFileAsync();

            user.ProfileImagePath = filePath;

            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [HttpDelete("remove_profile_image")]
        public async Task<IActionResult> RemoveProfileImage(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new { message = "User not found." });

            var defaultImagePath = "/ProfileImages/default.png";

            if (user.ProfileImagePath != defaultImagePath) IO.File.Delete(user.ProfileImagePath);

            user.ProfileImagePath = defaultImagePath;

            await _userManager.UpdateAsync(user);

            return Ok();
        }
    }
}
