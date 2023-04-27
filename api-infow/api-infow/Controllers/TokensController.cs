using api_infow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace api_infow.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        public IConfiguration _configuration;
        
        public TokensController(IConfiguration config)
        {
            _configuration = config;
        }
        [HttpPost]
        public async Task<IActionResult> Post(UserInfo _userData)
        {
            if (_userData != null && _userData.Email != null && _userData.Password != null)
            {
                //var user = await GetUser(_userData.Email, _userData.Password);
                var user = _userData;
                if (user != null)
                {
                    //create claims details based on the user information
                    var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("UserId", user.UserId.ToString()),
                        new Claim("DisplayName", user.DisplayName),
                        new Claim("UserName", user.UserName),
                        new Claim("Email", user.Email)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var expires=DateTime.UtcNow.AddDays(1);
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: expires,
                        signingCredentials: signIn);

                    return Ok(new {Token= new JwtSecurityTokenHandler().WriteToken(token) ,expr= expires });
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
