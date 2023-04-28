using api_infow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
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
        private readonly IMongoCollection<UserInfo> _userCollection;
        public TokensController(IConfiguration config)
        {
            _configuration = config;
            _configuration = config;
            var mongoClient = new MongoClient(_configuration["UserStoreDatabase:ConnectionString"]);

            var mongoDatabase = mongoClient.GetDatabase(_configuration["UserStoreDatabase:DatabaseName"]);

            _userCollection = mongoDatabase.GetCollection<UserInfo>(_configuration["UserStoreDatabase:UsersCollectionName"]);
        }
        [HttpPost]
        public async Task<IActionResult> Post(UserInfo _userData)
        {
            if (_userData != null && _userData.UserName != null && _userData.Password != null)
            {
                var user = await _userCollection.Find(x => x.UserName == _userData.UserName && x.Password == _userData.Password).FirstOrDefaultAsync();
                
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
