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
using System.Text;
using System.Text.Encodings.Web;
using IO = System.IO;

namespace Portfol.io.Identity.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("api/user")]
    public class UserController : BaseController
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
            var roles = _roleManager.Roles.Where(o => new List<string> { "author", "user" }.Contains(o.Name))
                .ProjectTo<RoleLookupDto>(_mapper.ConfigurationProvider).ToList();

            if (roles.Count() == 0) return NotFound(new Error { Message = "Роли не найдены" });

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
        /// <returns>Returns <see cref="UserDto"/></returns>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpGet("get_user_by_id")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(UserDto))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.ProfileImagePath = UrlRaw + user.ProfileImagePath;

            if (user is null) return NotFound(new Error { Message = "Пользователь не найден" });

            return Ok(_mapper.Map<UserDto>(user));
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
            var users = _userManager.Users.ProjectTo<UserDto>(_mapper.ConfigurationProvider).ToList();

            foreach(var user in users)
            {
                user.ProfileImagePath = UrlRaw + user.ProfileImagePath;
            }

            if (users.Count == 0) return NotFound(new Error { Message = "Пользователи не найдены" });

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
        ///     POST /api/user/change_email?newEmail=newUser@example.com
        /// </remarks>
        /// <param name="newEmail">New user email</param>
        /// <response code="400">If model is not valid.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpPost("change_email")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ChangeEmail(string newEmail)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            var user = await _userManager.FindByIdAsync(UserId);

            if (user is null) return NotFound(new Error { Message = "Пользователь не найден" });

            var code = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{UrlRaw}/confirmEmailChange?userId={UserId}&newEmail={newEmail}&code={code}";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Смена email",
                $"Для смены email <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>нажмите сюда</a>.");

            return Ok();
        }

        /// <summary>
        /// Email change confirmation
        /// </summary>
        /// <remarks> 
        /// The page address must contain ".../confirmEmailChange". Example: http://example.com/confirmEmailChange
        /// Sample request:
        /// 
        ///     POST: /api/user/confirm_change_email?newEmail=newUser@example.com&amp;code=your_code
        /// </remarks>
        /// <param name="newEmail">New user email</param>
        /// <param name="code">Confirmation code</param>
        /// <response code="400">If there were errors.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="200">Success</response>

        [HttpPost("confirm_change_email")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ConfirmChangeEmail(string newEmail, string code)
        {
            var user = await _userManager.FindByIdAsync(UserId);

            if (user is null) return NotFound(new Error { Message = "Пользователь не найден" });

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
        ///     GET /api/user/change_phone?newPhoneNumber=89121234567
        /// </remarks>
        /// <param name="newPhoneNumber">New user phone</param>
        /// <response code="400">If model is not valid. </response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpGet("change_phone")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ChangePhone(string newPhoneNumber)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            var user = await _userManager.FindByIdAsync(UserId);

            if (user is null) return NotFound(new Error { Message = "Пользователь не найден" });

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, newPhoneNumber);

            //TODO: Реализовать отправку кода на номер телефона

            return Ok(new {message = $"Ваш код подтверждения: {code}"});
        }

        /// <summary>
        /// Phone change confirmation
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/user/confirm_change_phone?newPhoneNumber=89121234567&amp;code=your_code
        /// </remarks>
        /// <param name="newPhoneNumber">New user phone</param>
        /// <param name="code">Confirmation code</param>
        /// <response code="400">If there were errors.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="200">Success</response>

        [HttpPost("confirm_change_phone")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ConfirmChangePhone(string newPhoneNumber, string code)
        { 
            var user = await _userManager.FindByIdAsync(UserId);

            if (user is null) return NotFound(new Error { Message = "Пользователь не найден" });

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
        ///         "name": "Ivanov Ivan Ivanovich",
        ///         "description": "I'm Ivan",
        ///         "dateOfBirth": "0000-00-00"
        ///     }
        /// </remarks>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">If there were errors.</response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="200">Success</response>

        [HttpPut("update_user_details")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDetailsViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Некорректные входные данные" });

            var user = await _userManager.FindByIdAsync(UserId);

            if (user is null) return NotFound(new Error { Message = "Пользователь не найден" });

            var names = model.Name.Split(" ");

            user.FullName = model.Name;
            user.FirstName = names[1];
            user.LastName = names[0];
            user.MiddleName = names[2];
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
        ///     PUT: /api/user/update_profile_image
        ///     Form object: file=file_object; type: image/jpeg
        /// </remarks>
        /// <param name="file">Image file. Support extensions: jpeg, png, jpg</param>
        /// <response code="400">If the file is empty. </response>
        /// <response code="400">If the file extension is wrong. </response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpPut("update_profile_image")]
        [ValidateFileExtension("jpeg, png, jpg")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> UpdateProfileImage(IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(UserId);

            if (user is null) return NotFound(new Error { Message = "Пользователь не найден" });

            if (user.ProfileImagePath != "/ProfileImages/default.png") IO.File.Delete(_environment.WebRootPath + user.ProfileImagePath);

            if (file is default(IFormFile)) return BadRequest(new {message = "Файл пустой"});

            _fileUploader.File = file;
            _fileUploader.AbsolutePath = $"/ProfileImages/{UserId}";
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
        ///     DELETE: /api/user/remove_profile_image
        /// </remarks>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>

        [HttpDelete("remove_profile_image")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var user = await _userManager.FindByIdAsync(UserId);

            if (user is null) return NotFound(new Error { Message = "Пользователь не найден" });

            var defaultImagePath = "/ProfileImages/default.png";

            if (user.ProfileImagePath != defaultImagePath) IO.File.Delete(_environment.WebRootPath + user.ProfileImagePath);

            user.ProfileImagePath = defaultImagePath;

            await _userManager.UpdateAsync(user);

            return Ok();
        }
    }
}
