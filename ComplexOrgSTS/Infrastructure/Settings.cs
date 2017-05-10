using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ComplexOrgSTS.Infrastructure
{
    public static class Settings
    {
        public static string SiteName { get; set; }
        public static string SigningCertificate { get; set; }
        public static string SigningCertificatePassword { get; set; }
        public static string Port { get; set; }
        public static string HttpLocalhost { get; set; }
        public static string AdminSiteUrl { get; set; }
        public static string STSApiKey { get; set; }

        public static string IssuerUri { get; set; }

        public const string WSTrustSTS = "/sts/trust/";
        public const string WSFedSts = "/sts/";
        public const string WSFedStsIssue = WSFedSts + "login/";
        public const string LocalStsExeConfig = "ComplexOrgSTS.dll.config";
        public const string FederationMetadataAddress = "FederationMetadata/2007-06/FederationMetadata.xml";
        public const string FederationMetadataEndpoint = WSFedSts + FederationMetadataAddress;
    }
}