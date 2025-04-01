using FAPCL.Model;
using FAPCL.Model.CustomModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using FAPCL.Help;
using Microsoft.AspNetCore.Authorization;

namespace FAPCL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public UserController(
                    UserManager<AspNetUser> userManager,
                    SignInManager<AspNetUser> signInManager,
                    IConfiguration configuration,
                    EmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = new AspNetUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail", 
                "User", 
                new { userId = user.Id, code = WebUtility.UrlEncode(code) }, 
                Request.Scheme);
            try
            {
                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>."
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error sending email: " + ex.Message });
            }
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            if (!string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Student");
            }
            
            return Ok(new { Message = "User registered successfully with role. A confirmation email has been sent." });
        }

        [HttpPost("register-multiple")]
        public async Task<IActionResult> RegisterMultiple([FromBody] List<RegisterModel> models)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var model in models)
            {
                var user = new AspNetUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { Email = model.Email, Errors = result.Errors });
                }

                var role = string.IsNullOrEmpty(model.Role) ? "Student" : model.Role;
                await _userManager.AddToRoleAsync(user, role);
            }

            return Ok(new { Message = "All users registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid login attempt." });
            }

            // Kiểm tra nếu email chưa được xác nhận
            if (!user.EmailConfirmed)
            {
                return Unauthorized(new { Message = "Please confirm your email before logging in." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            return Unauthorized(new { Message = "Invalid login attempt." });
        }

        private string GenerateJwtToken(AspNetUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                notBefore: DateTime.Now,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        [Authorize]
        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {         
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (userId == null)
            {
                return Unauthorized(new { Message = "Token không hợp lệ." });
            }            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }        
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.Address
            });
        }
        
        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("User ID and code must be provided.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }
            var decodedCode = WebUtility.UrlDecode(code); 
            // user.EmailConfirmed = true;
            // var updateResult = await _userManager.UpdateAsync(user);
            var updateResult = await _userManager.ConfirmEmailAsync(user, decodedCode);
            if (updateResult.Succeeded)
            {
                // Nếu xác nhận thành công, redirect người dùng đến dự án FAPCLClient
                var clientUrl = "http://localhost:5163";  
                var callbackUrl = $"{clientUrl}/ConfirmEmail?userId={user.Id}&code={decodedCode}";
                return Redirect(callbackUrl); 
            }

            return BadRequest("The token is invalid or has expired. Please request a new confirmation email.");
        }
        
        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new { Message = "Email has already been confirmed." });
            }

            // Generate a new email confirmation token
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Create the callback URL
            var callbackUrl = Url.Action(
                "ConfirmEmail", 
                "User", 
                new { userId = user.Id, code = WebUtility.UrlEncode(code) }, 
                Request.Scheme);

            try
            {
                // Send confirmation email again
                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>."
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error sending email: " + ex.Message });
            }

            return Ok(new { Message = "A new confirmation email has been sent." });
        }
        
        [HttpPost("reset-password-request")]
        public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tìm người dùng theo email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Tạo token reset mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Tạo URL để gửi cho người dùng. Chú ý trỏ về Razor Page.
            var resetUrl = $"http://localhost:5163/ResetPassword?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

            try
            {
                // Gửi email với link reset mật khẩu
                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Reset Your Password",
                    $"Please reset your password by clicking <a href='{resetUrl}'>here</a>."
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error sending email: " + ex.Message });
            }

            return Ok(new { Message = "A reset password link has been sent to your email." });
        }

        
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Reset mật khẩu với token
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Password has been reset successfully." });
            }

            return BadRequest(new { Message = "Error resetting password." });
        }

    }
    
} 