using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OAuth.Server.ExternalModels;
using OAuth.Server.Services;
using System.Security.Claims;

namespace OAuth.Server.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        public LoginController(IUserService userservice) 
        {
            _userService = userservice;
        }

        [Route("login-check")]
        [HttpGet]
        public IResult LoginCheck()
        {
            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                GetUser getUser = new GetUser();
                string provider = User.Claims.FirstOrDefault(x => x.Type.Equals("LoginProvider")).Value;
                
                if (provider.Equals("twitter"))
                {
                    var user = _userService.GetUserByUsername(User.Claims.FirstOrDefault(x => x.Type.Equals("Username")).Value, provider);
                    getUser.Email = user.Username;
                }
                else
                {
                    var user = _userService.GetUserByEmail(User.Claims.FirstOrDefault(x => x.Type.Equals("Email")).Value, provider);
                    getUser.Email = user.Email;
                }

                return Results.Ok(getUser);
            }

            return Results.Unauthorized();
        }

        [Route("login-github")]
        [HttpGet]
        public IResult Github()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            return Results.Challenge(properties, authenticationSchemes: [ "github" ]);
        }
        
        [Route("login-google")]
        [HttpGet]
        public IResult Google()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            return Results.Challenge(properties, authenticationSchemes: [ "google" ]);
        }
        
        [Route("login-twitter")]
        [HttpGet]
        public IResult Twitter()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            return Results.Challenge(properties, authenticationSchemes: [ "twitter" ]);
        }

        [Route("logout")]
        [HttpPost]
        public async Task<IResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { redirectUrl = "/login" });
        }

        [Route("signin")]
        [HttpPost]
        public IResult Signin([FromBody] UserSignIn entity)
        {
            if (entity == null)
                return Results.BadRequest("User credentials must be provided");

            if (string.IsNullOrEmpty(entity.Email))
                return Results.BadRequest("Email must be provided");

            if (string.IsNullOrEmpty(entity.Password))
                return Results.BadRequest("Password must be provided");

            var user = _userService.GetUserByEmail(entity.Email, "app");

            if (user == null)
                return Results.BadRequest($"User with email {entity.Email} does not exist");

            if (_userService.IsPasswordValid(user, entity.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim("Email", user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("LoginProvider", "app"),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, // Remember user if needed
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                };

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties).Wait();

                return Results.Ok(new { email = user.Email, redirectUrl = "/" });
            }

            return Results.BadRequest("Password is invalid");
        }

        [Route("register")]
        [HttpPost]
        public IResult Signup([FromBody] UserSignIn entity)
        {
            if (entity == null)
                return Results.BadRequest("User credentials must be provided");

            if (string.IsNullOrEmpty(entity.Email))
                return Results.BadRequest("Email must be provided");

            if (string.IsNullOrEmpty(entity.Password))
                return Results.BadRequest("Password must be provided");

            var user = _userService.SaveUserAsync(entity.Email, entity.Email, string.Empty, string.Empty, "app", null, entity.Password).Result;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("LoginProvider", "app"),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false, // Remember user if needed
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
            };

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties).Wait();

            return Results.Ok(new { email = user.Email, redirectUrl = "/" });
        }
    }
}
