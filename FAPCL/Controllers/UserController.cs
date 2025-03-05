using FAPCL.Model;
using FAPCL.Model.CustomModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly SignInManager<AspNetUser> _signInManager;

        public UserController(
                    UserManager<AspNetUser> userManager,
                    SignInManager<AspNetUser> signInManager) // Thêm vào constructor
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                Address = model.Address, // Ánh xạ thủ công
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return Ok(new { Message = "User registered successfully" });
            }

            return BadRequest(result.Errors);
        }
}
}
