using api_infow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace api_infow.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<UserInfo> _userCollection;
        public IConfiguration _configuration;
        public UsersController(IConfiguration config)
        {
            _configuration = config;
            var mongoClient = new MongoClient(_configuration["UserStoreDatabase:ConnectionString"]);

            var mongoDatabase = mongoClient.GetDatabase(_configuration["UserStoreDatabase:DatabaseName"]);

            _userCollection = mongoDatabase.GetCollection<UserInfo>(_configuration["UserStoreDatabase:UsersCollectionName"]);
        }
        [Authorize]
        [HttpGet("Get")]
        public async Task<List<UserInfo>> GetAsync() =>
        await _userCollection.Find(_ => true).ToListAsync();
        [HttpGet("GetById")]
        public async Task<UserInfo?> GetAsync(int id) =>
            await _userCollection.Find(x => x.UserId == id).FirstOrDefaultAsync();
        [HttpPost("CreateAsync")]
        public async Task CreateAsync(UserInfo newBook) =>
            await _userCollection.InsertOneAsync(newBook);
        [HttpPost("UpdateAsync")]
        public async Task UpdateAsync(int id, UserInfo usi) =>
            await _userCollection.ReplaceOneAsync(x => x.UserId == id, usi);
        [HttpPut("RemoveAsync")]
        public async Task RemoveAsync(int id) =>
            await _userCollection.DeleteOneAsync(x => x.UserId == id);
    }
}
