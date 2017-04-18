using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Common;
using System.Collections.Generic;
using Microsoft.Owin.Security.Cookies;

namespace ADSyncApi.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public void SignIn()
        {
            var redir = Request.QueryString["redir"];
            if (redir == null) redir = "/";
            HttpContext.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = redir,
                },
                CustomAuthTypes.B2E);
        }

        public void SignOut()
        {
            // To sign out the user, you should issue an OpenIDConnect sign out request
            if (Request.IsAuthenticated)
            {
                IEnumerable<AuthenticationDescription> authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
                HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
                Request.GetOwinContext().Authentication.GetAuthenticationTypes();
            }
        }

        public void EndSession()
        {
            // If AAD sends a single sign-out message to the app, end the user's session, but don't redirect to AAD for sign out.
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        }

        [AllowAnonymous]
        public ActionResult SignedOut()
        {
            return View();
        }
    }
}