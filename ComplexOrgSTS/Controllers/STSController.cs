using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WSFederationSecurityTokenService;
using System.IdentityModel;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using ComplexOrgSTS.Models;
using System.IdentityModel.Services;
using ComplexOrgSTS.Infrastructure;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using ADSync.Common.Models;

namespace WSFederationSecurityTokenService
{
    public class STSController : Controller
    {
        CustomSTSConfig stsConfiguration;
        SecurityTokenService securityTokenService;

        private void InitSTS(string domain)
        {
            stsConfiguration = new CustomSTSConfig(domain);
            securityTokenService = new CustomSTS(stsConfiguration);
        }

        public ActionResult Index(string username, string wa, string wtrealm, string realm, string wctx, string wct, string wreply)
        {
            ViewBag.UserName = username;
            ViewBag.QS = Request.Url.Query;
            ViewBag.Error = Session["Error"];
            Session.Remove("Error");
            ViewBag.KmsiDisplay = "block";
            return View();
        }

        [HttpPost]
        [Route("~/sts/trust")]
        public async Task<ActionResult> Trust(LoginModel login)
        {
            string domain = login.UserName.Split('@')[1];
            InitSTS(domain);
            //validate identity
            var user = await LoginValidate.ValidateAsync(login, HttpRuntime.Cache);

            if (!user.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Response.StatusDescription = HttpStatusCode.Forbidden.ToString();
                return null;
            }

            //TODO: Need WSTrust handler?

            var res = new ContentResult()
            {
                ContentType = "text/html",
                ContentEncoding = Encoding.UTF8,
                Content = ""
                //Content = Encoding.UTF8.GetString(stream.ToArray())
            };

            return res;
        }

        /// <summary>
        /// POST https://adfs.thehacker.com/adfs/ls/?client-request-id=6129003b-9dd2-4c3b-bfe7-e57e11e3b781&username=bhacker%40thehacker.com&wa=wsignin1.0&wtrealm=urn%3afederation%3aMicrosoftOnline&wctx=estsredirect%3d2%26estsrequest%3drQIIAY2SO2zTUBSG46aN2gqpjwm2DhUSoMTPPJyqgjQPN6ntpHnUtZfIj1s_El-79o1jgoTExtiJAQkJGDt2AbGwd6o6siIEQmJhQLDhIDGxsBzp6PzL-b7_bprMkeVtnWE0hijRWVrTiCzD6mRWY4pGli0yIE_rmnFC661MniAZmgg2V9d3UnfU6xc7_PvPnx5vfHzz5By7bSHkh2Uc970AqeOcOpsEIKd7Lh7aJrQhbkMDxPhbDLvCsC8Ydr4QFugCVSoRBMmyJTpPsTSVE7gBJUsyKUgCUrg6o_QIQnDGI15qTtv9ARL7JtWuWa7iiCOl1nAVqU4JziAW-jISHNGe50WuHid5WqBkpDj6Q9Gpk4IjUIrUHX1YWGtXJsii5sML7Bn4vrBy4gXu0PdC9Dz9DWv7ADaNqgch0FFuHgMQ2bqKbA92As8HAbJBuMvuFVpUt4JieCiLs2iIIrnP0m06qjX4I6taQ_s-J7JhexhTppQtcYXTyKo3u0ANogqVnxosz5QicrxHxLoGhhbX8nvjgslbYDit9AlwqAq66bKQGo21I3Yi8zEPj_VZ3JhWalZnUgjk3rHTGFaFgEdHkac5JicdeY7rdNRsk5fRWLIgRw3MqtUy3el-L3D7XdPwi5VorzfoVqFSnB1orHme3v6rzVWhagI3eTaRlpibJr68aZiDAOEX6Uwi0vXgZfrWJATB0HYTDqEH_1DZSqBA27hOb_57u1rEvi7eINLl5eXV9dTN1Fbq5yL2eilp0KsfTzfWLn7xL589kPOPllOXS3iHGwiSUa_wyO_fQ9XJKfBVETftzkH3WFR6lqd60DmpBtZA3y2VybMMdpbJvFvB_6959-dbE-6SvwE1&popupui=
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Login(LoginModel login)
        {
            if (login.UserName.IndexOf('@') < 0)
            {
                //incorrect format
                Session["Error"] = @"Enter your user ID in the format ""domain\user"" or ""user @domain"". ";
                return RedirectToAction("Index", new { Request.Url.Query });
            }

            string domain = login.UserName.Split('@')[1];
            InitSTS(domain);
            ValidationResponse user;

            try
            {
                //validate identity
                user = await LoginValidate.ValidateAsync(login, HttpRuntime.Cache);

                if (!user.IsValid)
                {
                    Session["Error"] = "Incorrect user ID or password. Type the correct user ID and password, and try again.";
                    return RedirectToAction("Index", new { Request.Url.Query });
                }
            }
            catch (Exception ex)
            {
                Common.Utils.AddLogEntry("Error during user authentication", System.Diagnostics.EventLogEntryType.Error, 0, ex);
                Session["Error"] = string.Format("An error occured during authentication ({0})", ex.Message);
                return RedirectToAction("Index", new { Request.Url.Query });
            }

            //identity validated
            string fullRequest = String.Format("{0}{1}{2}?{3}", 
                Settings.HttpLocalhost, 
                Settings.Port, 
                Settings.WSFedStsIssue, 
                Request.Url.Query
            );

            //todo: 
            var immutableId = user.UserProperties.MasterGuid;
            //var immutableId = user.UserProperties.LocalGuid;

            SignInRequestMessage requestMessage = (SignInRequestMessage)WSFederationMessage.CreateFromUri(new Uri(fullRequest));

            //todo:
            requestMessage.Reply = string.Format("https://login.microsoftonline.com:443/login.srf?client-request-id={0}", Request.QueryString["client-request-id"]);

            ClaimsIdentity identity = new ClaimsIdentity(AuthenticationTypes.Federation);
            identity.AddClaim(new Claim("http://schemas.microsoft.com/LiveID/Federation/2008/05/ImmutableID", immutableId));
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/claims/UPN", user.UserProperties.Upn));
            //TODO: verify the source of this flag in ADFS
            //identity.AddClaim(new Claim("http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork", "true", typeof(bool).ToString()));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            SignInResponseMessage responseMessage = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(requestMessage, principal, this.securityTokenService);
            
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);

            responseMessage.Write(writer);

            writer.Flush();
            stream.Position = 0;

            var res = new ContentResult()
            {
                ContentType = "text/html",
                ContentEncoding = Encoding.UTF8,
                Content = Encoding.UTF8.GetString(stream.ToArray())
            };

            return res;
        }

        [HttpGet]
        [Route("~/sts/{domain}/FederationMetadata/2007-06/FederationMetadata.xml")]
        public XElement FederationMetadata(string domain)
        {
            InitSTS(domain);
            Response.ContentType = "application/xml";
            return this.stsConfiguration.GetFederationMetadata(domain);
        }
    }
}
