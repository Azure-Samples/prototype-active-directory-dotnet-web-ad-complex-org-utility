using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Infrastructure;

namespace WSFederationSecurityTokenService
{
    public class CustomSTS : SecurityTokenService
    {
        public CustomSTS(SecurityTokenServiceConfiguration securityTokenServiceConfiguration)
            : base(securityTokenServiceConfiguration)
        {
        }

        protected override ClaimsIdentity GetOutputClaimsIdentity(ClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
        {
            if (principal == null)
            {
                throw new InvalidRequestException("The caller's principal is null.");
            }

            ClaimsIdentity outputIdentity = new ClaimsIdentity(principal.Identity);
            //ClaimsIdentity outputIdentity = new ClaimsIdentity();

            //outputIdentity.AddClaim(new Claim(ClaimTypes.Email, "terry@contoso.com"));
            //outputIdentity.AddClaim(new Claim(ClaimTypes.Surname, "Adams"));
            //outputIdentity.AddClaim(new Claim(ClaimTypes.Name, "Terry"));
            //outputIdentity.AddClaim(new Claim(ClaimTypes.Role, "developer"));
            //outputIdentity.AddClaim(new Claim("http://schemas.xmlsoap.org/claims/Group", "Sales"));
            //outputIdentity.AddClaim(new Claim("http://schemas.xmlsoap.org/claims/Group", "Marketing"));

            return outputIdentity;
        }

        protected override Scope GetScope(ClaimsPrincipal principal, RequestSecurityToken request)
        {
            this.ValidateAppliesTo(request.AppliesTo);
            Scope scope = new Scope(request.AppliesTo.Uri.OriginalString, SecurityTokenServiceConfiguration.SigningCredentials);

            scope.TokenEncryptionRequired = false;
            scope.SymmetricKeyEncryptionRequired = false;

            if (string.IsNullOrEmpty(request.ReplyTo))
            {
                scope.ReplyToAddress = scope.AppliesToAddress;
            }
            else
            {
                scope.ReplyToAddress = request.ReplyTo;
            }

            return scope;
        }
        private void ValidateAppliesTo(EndpointReference appliesTo)
        {
            if (appliesTo == null)
            {
                throw new InvalidRequestException("The AppliesTo is null.");
            }
        }
    }
}