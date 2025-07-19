using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Taskify.Api.Models;
using Taskify.Domain;
using Taskify.Infrastructure;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace Taskify.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly TaskifyDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthController(TaskifyDbContext dbContext, IConfiguration configuration){
            _dbContext = dbContext;
            _configuration = configuration;
        }
        // TENTANDO CRIAR A PEMBA DO MÉTODO DE REGISTRO
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request){
            // email ja existe cai nisso
            if(await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("User already exists");
            // hash da senha sem misterio
            var passwordHash = HashPassword(request.Password);
            // Usuario de cria
            var user = new User{
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHash,
            };
            //tacando pro banco
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            // TETANDO DEBUGAR ESSA BOMBA HAHAHAHAH
            return Ok(new { message = "User successfully"});
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request){
            // acha o user pelo email do cidadão 
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized(new {message = "incorrect password or email!"});
            // verifica se a senha do caboco está certa
            if (!VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new {message = "incorrect password or email!"});
            // gerar essa bomba de jwt
            var token = GenerateJwtToken(user);
            // volta a bomba do token
            return Ok(new { token });
        }
        //hora de fazer o hash vulgo "#" da senha
        private string HashPassword(string password){
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        } 
        // ver se a senha ta correta
        private bool VerifyPassword(string password, string hash){
            return HashPassword(password) == hash;
        }
        // gerar o jwt token (q q eu to fazendo ja mds)
        private string GenerateJwtToken(User user){
            var keyString = _configuration["Jwt:Key"] ?? throw new Exception("JWT key not configured!");
            var key = Encoding.ASCII.GetBytes(keyString);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(new[]{
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(2), // Corrigido aqui
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), // Corrigido aqui
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}