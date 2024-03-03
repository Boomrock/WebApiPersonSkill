using AuthApi.Models;
using AuthApi.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace AuthApi.Controllers
{
    public class HomeController : Controller
    {
        private readonly double TokenExpirationDate = 10;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly AuthDBContext _authDBContext;

        public HomeController(AuthDBContext authDBContext)
        {
            _authDBContext = authDBContext;
        }
        // GET: HomeController
        public ActionResult Index(string clientId, string redirect_uri)
        {
            var clientGuid = Guid.Parse(clientId);
            var client = _authDBContext.Clients.Find(clientGuid);
            if(client == null)
            {
                return NotFound();
            }

            var userVM = new UserVM(); 

            return View(userVM);
        }
        [HttpPost]
        public ActionResult Index(UserVM userVM ,string clientId, string redirect_uri)
        {
            if (ModelState.IsValid)
            {
                var clientGuid = Guid.Parse(clientId);
                var client = _authDBContext.Clients.Find(clientGuid);
                if (client == null)
                {
                    return NotFound();
                }
                _authDBContext.PermittedRedirect.Find(new { redirect_uri, clientGuid });
                var user = new User();

                user.LoginHash = SHA1.HashData(_encoding.GetBytes(userVM.Login)).ToString();
                user.PasswordHash = SHA1.HashData(_encoding.GetBytes(userVM.Password)).ToString();

                user = _authDBContext.Users.Find(user);

                if (user == null)
                {
                    return View();
                }

                var accessToken = new AccessToken();
                accessToken.Id = Guid.NewGuid();
                accessToken.TokenExpirationDate = DateTime.Now.AddMinutes(TokenExpirationDate);
                _authDBContext.AccessTokens.Add(accessToken);
                return Redirect(redirect_uri + "&access_token=" + accessToken.Id.ToString());

            }

            return View();
        }



    }
}
