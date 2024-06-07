using Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApplication4.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IRepository _userRepository;

        public UserController(IRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> LoginAsync(string login, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password))

                    return BadRequest("Username and/or Password not specified");

                var user = await _userRepository.GetByLoginAsync(login);
                if (user.Login.Equals(login) && user.Password.Equals(password))
                {
                    var secretKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes("this is my custom Secret key for authentication"));

                    var signinCredentials = new SigningCredentials
                    (secretKey, SecurityAlgorithms.HmacSha256);

                    var role = user.Admin ? "admin" : "user";
                    var jwtSecurityToken = new JwtSecurityToken(
                        issuer: "ABCXYZ",
                        audience: "http://localhost:5053",
                        claims: new List<Claim> { new Claim(ClaimTypes.Name, user.Login),
                                                  new Claim(ClaimTypes.Role, role)
                        },
                        expires: DateTime.Now.AddMinutes(10),
                        signingCredentials: signinCredentials
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);


                    return Ok(new { Token = tokenString });

                }
            }
            catch
            {
                return BadRequest
                ("An error occurred in generating the token");
            }
            return Unauthorized();
        }

        [HttpGet("get-by-login")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<UserDTO>> Get(string login)
        {
            try
            {
                var result = await _userRepository.GetByLoginAsync(login);
                if (result == null)
                {
                    return NotFound(login);
                }
                var g = "";
                if(result.Gender == 0) 
                    g = "Женщина";

                if (result.Gender == 1) 
                    g = "Мужчина";
                if (result.Gender == 2)
                    g = "Неизвестно";
                
                var active = result.RevokedBy == null ? "active" : "not active";

                return Ok(new { Name = result.Name, Gender = g, Birthday = result.Birthday, active });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("get-older-than")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<UserDTO>> GetOlderThan(int age)
        {
            try
            {
                var result = await _userRepository.GetAllOlderThanAsync(age);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("get-by-login-and-password")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetByLoginAndPassword(string login, string password)
        {
            try
            {
                var result = await _userRepository.GetByLoginAsync(login);
                if (result == null)
                {
                    return NotFound(login);
                }
                if (!login.Equals(this.User.Identity.Name) || 
                    (result.RevokedBy != null) ||
                    !result.Password.Equals(password))
                {
                    return BadRequest();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-all-active")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<UserDTO>> GetAllActive()
        {
            try
            {
                var result = await _userRepository.GetAllActiveUsersAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<int>> Create(UserDTO user)
        {
            try
            {
                var currentUser = await _userRepository.GetByLoginAsync(user.Login);
                if (currentUser != null)
                {
                    return NotFound($"User {user.Login} already exists");
                }

                var name = this.User.Identity.Name;
                var res = await _userRepository.AddAsync(user, name);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<ActionResult> Change(string login, string password)
        {
            try
            {
                var currentUser = await _userRepository.GetByLoginAsync(login);
                if (currentUser == null)
                {
                    return NotFound($"User {login} not found");
                }

                var name = this.User.Identity.Name;
                var role = this.HttpContext.User.IsInRole("admin");
                if (!login.Equals(name) && !role && currentUser.RevokedOn!= null)
                {
                    return BadRequest("Forbidden");
                }
                await _userRepository.ChangePasswordAsync(login, password, name);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut("change-login")]
        [Authorize]
        public async Task<ActionResult> ChangeLogin(string login, string newLogin)
        {
            try
            {
                var currentUser = await _userRepository.GetByLoginAsync(newLogin);
                var user = await _userRepository.GetByLoginAsync(login);
                if (currentUser != null || user == null)
                {
                    return NotFound($"User {login} not found or {newLogin}  already exists");
                }

                var name = this.User.Identity.Name;
                var role = this.HttpContext.User.IsInRole("admin");
                if (!login.Equals(name) && !role && currentUser.RevokedOn!= null)
                {
                    return BadRequest("Forbidden");
                }
                await _userRepository.ChangeLoginAsync(login, name);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut("change-user")]
        [Authorize]
        public async Task<ActionResult> ChangeUser(string login, string name, int gender, DateTime bDay)
        {
            try
            {
                var currentUser = await _userRepository.GetByLoginAsync(login);
                if (currentUser == null)
                {
                    return NotFound($"User {login} not found");
                }

                var authName = this.User.Identity.Name;
                var role = this.HttpContext.User.IsInRole("admin");
                if (!login.Equals(authName) && !role && currentUser.RevokedOn!= null)
                {
                    return BadRequest("Forbidden");
                }
                await _userRepository.ChangeAsync(login, name, gender, bDay, authName);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("recover-user")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> RecoverUser(string login)
        {
            try
            {
                if (await _userRepository.GetByLoginAsync(login) == null)
                {
                    return NotFound($"User {login} not found");
                }

                var res = await _userRepository.RecoverUserAsync(login);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(string login)
        {
            try
            {
                var item = await _userRepository.GetByLoginAsync(login);
                if (item == null)
                {
                    return NotFound(login);
                }

                var result = await _userRepository.Delete(login);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete-soft")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteSoft(string login)
        {
            try
            {
                var item = await _userRepository.GetByLoginAsync(login);
                if (item == null)
                {
                    return NotFound(login);
                }

                var name = this.User.Identity.Name;
                var result = await _userRepository.DeleteSoft(login, name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
