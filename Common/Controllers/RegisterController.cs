using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Common.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private IConfiguration _configuration;
        private UserManager<IdentityUser> userManager;

        public RegisterController(IConfiguration configuration,UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            this.userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var result = await userManager.FindByNameAsync(model.Login);
            if(result != null)
                return BadRequest("Bu foydalanuvchi allaqachon ro'yxatdan utgan");
            
            var user = new IdentityUser()
            {
                UserName = model.Login,
                Email = model.Login
            };
            var createdUser = await userManager.CreateAsync(user, model.Password);

            if (!createdUser.Succeeded)
                return Ok(createdUser.Errors);

            var header = new JwtHeader(
                new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["secretKey"])),
                    SecurityAlgorithms.HmacSha256));
            var payload = new JwtPayload(new Claim[]
            {
                new Claim("userId",user.Id)
            });

            var token = new JwtSecurityToken(header,payload);
            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(RegisterModel model)
        {
            var result = await userManager.FindByNameAsync(model.Login);
            if (result == null)
                return BadRequest("Bu foydalanuvchi ro'yxatdan utmagan");

            var logining = await userManager.CheckPasswordAsync(result, model.Password);

            if (!logining)
                return Ok("Parol xato");

            var header = new JwtHeader(
                new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["secretKey"])),
                    SecurityAlgorithms.HmacSha256));
            var payload = new JwtPayload(new Claim[]
            {
                new Claim("userId",result.Id)
            });

            var token = new JwtSecurityToken(header, payload);
            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
