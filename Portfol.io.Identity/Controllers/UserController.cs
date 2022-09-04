using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Portfol.io.Identity.Common.Attributes;
using Portfol.io.Identity.Interfaces;
using Portfol.io.Identity.Models;
using Portfol.io.Identity.ViewModels;
using Portfol.io.Identity.ViewModels.ResponseModels;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using IO = System.IO;

namespace Portfol.io.Identity.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("api/user")]
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
        /// Get a list of available roles.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET: /api/user/get_roles
        /// </remarks>
        /// <returns>Returns <see cref="RoleViewModel"/></returns>
        /// <response code="404">If roles not found. </response>
        /// <response code="200">Success</response>

        [HttpGet("get_roles")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(RoleViewModel))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        [AllowAnonymous]
        public IActionResult GetRolesList()
        {
            var roles = _roleManager.Roles.Where(o => new List<string> { "employee", "employer" }.Contains(o.Name))
                .ProjectTo<RoleLookupDto>(_mapper.ConfigurationProvider).ToList();

            if (roles.Count() == 0) return NotFound(new Error { Message = "Roles not found." });

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
        /// 
        ///     GET /api/user/get_user_by_id?userId=33A5A12A-99A4-4770-80C4-C140F28B6E61
        /// </remarks>
        /// <param name="userId">User Id to find</param>
        /// <returns>Returns <see cref="UserLookupDto"/></returns>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpGet("get_user_by_id")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(UserLookupDto))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.ProfileImagePath = _environment.WebRootPath + user.ProfileImagePath;

            if (user is null) return NotFound(new Error { Message = "User not found." });

            return Ok(_mapper.Map<UserLookupDto>(user));
        }

        /// <summary>
        /// Get a list of user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/user/get_users
        /// </remarks>
        /// <returns>Returns <see cref="UsersViewModel"/></returns>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpGet("get_users")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(UsersViewModel))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        [AllowAnonymous]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users.ProjectTo<UserLookupDto>(_mapper.ConfigurationProvider).ToList();

            foreach(var user in users)
            {
                user.ProfileImagePath = _environment.WebRootPath + user.ProfileImagePath;
            }

            if (users.Count == 0) return NotFound(new Error { Message = "Users not found." });

            return Ok(new UsersViewModel
            {
                Users = users
            });
        }

        /// <summary>
        /// Change email
        /// </summary>
        /// <remarks>
        /// Sends a message with a link to change email. Sample request:
        /// 
        ///     GET /api/user/change_email?userId=user@example.com&amp;newEmail=newUser@example.com
        /// </remarks>
        /// <param name="userId">The id of the user whose email needs to be changed.</param>
        /// <param name="newEmail">New user email</param>
        /// <response code="400">If model is not valid.</response>
        /// <response code="403">If the user is wrong.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpGet("change_email")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ChangeEmail(string userId, string newEmail)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });

            var identityUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (userId != identityUserId) return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "Wrong user."});

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new Error { Message = "User is found." });

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

        /// <summary>
        /// Email change confirmation
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST: /api/user/confirm_change_email?userId=33A5A12A-99A4-4770-80C4-C140F28B6E61&amp;newEmail=newUser@example.com&amp;code=your_code
        /// </remarks>
        /// <param name="userId">The id of the user whose email needs to be changed.</param>
        /// <param name="newEmail">New user email</param>
        /// <param name="code">Confirmation code</param>
        /// <response code="400">If there were errors.</response>
        /// <response code="403">If the user is wrong.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="200">Success</response>

        [HttpPost("confirm_change_email")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string code)
        {
            var identityUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (userId != identityUserId) return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "Wrong user." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new Error { Message = "User not found." });

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ChangeEmailAsync(user, newEmail, code);

            if (result.Succeeded)
            {
                _logger.LogInformation("Email has been changed.");
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new Error { Message = error.Description });
            }

            return NoContent();
        }

        /// <summary>
        /// Change phone
        /// </summary>
        /// <remarks>
        /// Sends an SMS message with a confirmation code. Sample request:
        /// 
        ///     GET /api/user/change_phone?userId=33A5A12A-99A4-4770-80C4-C140F28B6E61&amp;newPhoneNumber=89121234567
        /// </remarks>
        /// <param name="userId">The id of the user whose phone needs to be changed.</param>
        /// <param name="newPhoneNumber">New user phone</param>
        /// <response code="400">If model is not valid. </response>
        /// <response code="403">If the user is wrong.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpGet("change_phone")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ChangePhone(string userId, string newPhoneNumber)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });

            var identityUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (userId != identityUserId) return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "Wrong user." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new Error { Message = "User not found." });

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, newPhoneNumber);

            //TODO: Реализовать отправку кода на номер телефона

            return Ok(new {message = $"Your confirm code: {code}"});
        }

        /// <summary>
        /// Phone change confirmation
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/user/change_phone?userId=33A5A12A-99A4-4770-80C4-C140F28B6E61&amp;newPhoneNumber=89121234567&amp;code=your_code
        /// </remarks>
        /// <param name="userId">The id of the user whose phone needs to be changed.</param>
        /// <param name="newPhoneNumber">New user phone</param>
        /// <param name="code">Confirmation code</param>
        /// <response code="400">If there were errors.</response>
        /// <response code="403">If the user is wrong.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="200">Success</response>

        [HttpPost("confirm_change_phone")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ConfirmChangePhone(string userId, string newPhoneNumber, string code)
        {
            var identityUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (userId != identityUserId) return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "Wrong user." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new Error { Message = "User not found." });

            var result = await _userManager.ChangePhoneNumberAsync(user, newPhoneNumber, code);

            if (result.Succeeded)
            {
                _logger.LogInformation("Phone number has been changed.");
                return Ok();
            }
            foreach(var error in result.Errors)
            {
                return BadRequest(new Error { Message = error.Description });
            }

            return NoContent();
        }

        /// <summary>
        /// User details update
        /// </summary>
        /// <remarks>
        /// To update user data: name, description, date of birth. Sample request:
        /// 
        ///     PUT /api/user/update_user_details
        ///     {
        ///         id: 33A5A12A-99A4-4770-80C4-C140F28B6E61,
        ///         name: Ivanov Ivan Ivanovich,
        ///         description: I'm Ivan,
        ///         dateOfBirth: 0000-00-00
        ///     }
        /// </remarks>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">If there were errors.</response>
        /// <response code="403">If the user is wrong.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="200">Success</response>

        [HttpPut("update_user_details")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDetailsViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });

            var identityUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (model.Id != identityUserId) return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "Wrong user." });

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null) return NotFound(new Error { Message = "User not found." });

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
                return BadRequest(new Error { Message = error.Description });
            }

            return NoContent();
        }

        /// <summary>
        /// Update profile image
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT: /api/user/update_profile_image?userId=33A5A12A-99A4-4770-80C4-C140F28B6E61
        ///     Form object: file=file_object; type: image/jpeg
        /// </remarks>
        /// <param name="userId">The id of the user whose profile image needs to be updated.</param>
        /// <param name="file">Image file. Support extensions: jpeg, png, jpg</param>
        /// <response code="400">If the file is empty. </response>
        /// <response code="400">If the file extension is wrong. </response>
        /// <response code="403">If the user is wrong.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpPut("update_profile_image")]
        [ValidateFileExtension("jpeg, png, jpg")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> UpdateProfileImage(IFormFile file, string userId)
        {
            var identityUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (userId != identityUserId) return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "Wrong user." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new Error { Message = "User not found." });

            if (user.ProfileImagePath != "/ProfileImages/default.png") IO.File.Delete(_environment.WebRootPath + user.ProfileImagePath);

            if (file is default(IFormFile)) return BadRequest(new {message = "File is empty."});

            _fileUploader.File = file;
            _fileUploader.AbsolutePath = $"/ProfileImages/{userId}/";
            _fileUploader.WebRootPath = _environment.WebRootPath;

            var filePath = await _fileUploader.UploadFileAsync();

            user.ProfileImagePath = filePath;

            await _userManager.UpdateAsync(user);

            return Ok();
        }

        /// <summary>
        /// Remove profile image
        /// </summary>
        /// <remarks>
        /// Sets the default profile image. Sample request:
        /// 
        ///     DELETE: /api/user/remove_profile_image?userId=33A5A12A-99A4-4770-80C4-C140F28B6E61
        /// </remarks>
        /// <param name="userId">The id of the user whose profile image needs to be deleted.</param>
        /// <response code="403">If the user is wrong.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpDelete("remove_profile_image")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> RemoveProfileImage(string userId)
        {
            var identityUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (userId != identityUserId) return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "Wrong user." });

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new Error { Message = "User not found." });

            var defaultImagePath = "/ProfileImages/default.png";

            if (user.ProfileImagePath != defaultImagePath) IO.File.Delete(_environment.WebRootPath + user.ProfileImagePath);

            user.ProfileImagePath = defaultImagePath;

            await _userManager.UpdateAsync(user);

            return Ok();
        }
    }
}
