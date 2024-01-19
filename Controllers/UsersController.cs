using PanierMVC.Data;
using PanierMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace PanierMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly PanierMVCContext _dbContextConnection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;



        private static string JwtKey = "C1CF4B7DC4C4175B6618DE4F55CA4";
        private static string JwtAudience = "SecureApiUser";
        private static string JwtIssuer = "https://localhost:44381";
        private static Double JwtExpireDays = 30;

        public UserController(PanierMVCContext dB, IConfiguration configuration, ILogger<UserController> logger)
        {
            this._dbContextConnection = dB;
            this._configuration = configuration;
            this._logger = logger;
        }

        // GET: User

        public async Task<IActionResult> Index()
        {
            List<User> users = await _dbContextConnection.User.ToListAsync();

            return View(users);
        } 
      
            public ActionResult login()
        {
            return View();
        }

        // GET: User/Create
        public IActionResult Create()
        {
            _logger.LogInformation("/Create :Formulaire de l'ajout du user est bien affiché");
            return View();
        }


        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("login,password")] User user)
        {
            try
            {
                    
                    user.role = "administrateur";
                    _dbContextConnection.Add(user);
                    _logger.LogInformation("Nouveau user ajouté");
                    await _dbContextConnection.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                

            }

            catch (Exception e)
            {
                _logger.LogError("Echec d'ajout du user");
                return Create();
            }

        }

        [HttpPost]
        public async Task<IActionResult> Authenticate([Bind("login","password")] User userA)
        {
            try
            {
                var user = _dbContextConnection.User.FirstOrDefault(u => u.login == userA.login && u.password == userA.password);

                if (user != null)
                {
                    _logger.LogInformation($"User '{user.login}' exist");

                    var token = GenerateJwtToken(user);
                    //ecrire dans le fichier de journalisation si le user arrive à s'authentifier
                    _logger.LogInformation($"User '{user.login}' successfully authenticated. Token: {token}");

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.login),
                new Claim("AccessToken", new JwtSecurityTokenHandler().WriteToken(token))
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    if (user.role == "user")
                    { return RedirectToAction("Index", "Produits"); }
                    else if (user.role == "administrateur")
                    {
                        return RedirectToAction("Index2", "Produits");
                    }
                    else
                    {
                        return View("login");
                    }

                }
                else
                {
                    //ecrire dans le fichier de journalisation si le user n'est pas trouve
                    _logger.LogWarning($"Failed authentication attempt for user '{userA.login}'.");

                    return View("login");
                }
            }
            catch (Exception ex)
            {
                //ecrire dans le fichier de journalisation qu'il y a une exception
                _logger.LogError($"An error occurred during authentication: {ex.Message}");
                return View("login");
            }
        }




        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //ecrire dans le fichier de journalisation que le user est deconnecté
            _logger.LogInformation("User logged out.");
            return RedirectToAction("login", "User");
        }


        private JwtSecurityToken GenerateJwtToken(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.Now.AddMinutes(Convert.ToDouble(JwtExpireDays));

                var token = new JwtSecurityToken(
                    issuer: JwtIssuer,
                    audience: JwtAudience,
                    claims: new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.login)
                    },
                    expires: expires,
                    signingCredentials: creds
                );

                return token;
            }
            catch (Exception ex)
            {
                //ecrire dans le fichier de journalisation qu'il y a un erreur qu niveau de la generation de jwt token
                _logger.LogError($"An error occurred while generating JWT token: {ex.Message}");
                throw;
            }
        }
    }
}