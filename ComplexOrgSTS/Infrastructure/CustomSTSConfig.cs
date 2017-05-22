//----------------------------------------------------------------------------------------------
//    Copyright 2012 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

using ComplexOrgSTS.Infrastructure;
using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Metadata;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;

namespace WSFederationSecurityTokenService
{
    internal class CustomSTSConfig : SecurityTokenServiceConfiguration
    {
        public CustomSTSConfig(string domain)
        {
            try
            {
                this.TokenIssuerName = string.Format(Settings.IssuerUri, domain);
                string signingCertificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.SigningCertificate);
                X509Certificate2 signingCert = new X509Certificate2(
                        signingCertificatePath,
                        Settings.SigningCertificatePassword,
                        X509KeyStorageFlags.MachineKeySet |
                        X509KeyStorageFlags.PersistKeySet |
                        X509KeyStorageFlags.Exportable);

                this.SigningCredentials = new X509SigningCredentials(signingCert);
                this.ServiceCertificate = signingCert;

                this.SecurityTokenService = typeof(CustomSTS);
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error initializing STS", System.Diagnostics.EventLogEntryType.Error, ex);
                throw;
            }
        }

        public XElement GetFederationMetadata(string domain)
        {
            // hostname
            EndpointReference passiveEndpoint = new EndpointReference(Settings.HttpLocalhost + Settings.Port + Settings.WSFedStsIssue);
            EndpointReference activeEndpoint = new EndpointReference(Settings.HttpLocalhost + Settings.Port + Settings.WSTrustSTS);

            // metadata document 
            var uri = string.Format(Settings.IssuerUri, domain);
            EntityDescriptor entity = new EntityDescriptor(new EntityId(uri));
            SecurityTokenServiceDescriptor sts = new SecurityTokenServiceDescriptor();
            entity.RoleDescriptors.Add(sts);

            // signing key
            KeyDescriptor signingKey = new KeyDescriptor(this.SigningCredentials.SigningKeyIdentifier);
            signingKey.Use = KeyType.Signing;
            sts.Keys.Add(signingKey);

            // claim types
            sts.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.Name, "Name", "User name"));
            sts.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.Email, "Email Address", "User email address"));
            sts.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.NameIdentifier, "Name Identifier", "User name identifier"));
            sts.ClaimTypesOffered.Add(new DisplayClaim("http://schemas.microsoft.com/LiveID/Federation/2008/05/ImmutableID", "ImmutableID", "User ImmutableID"));
            sts.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.Upn, "Upn", "User Upn"));
            sts.ClaimTypesOffered.Add(new DisplayClaim("http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork", "Inside corp network", "Is user in the corp network"));

            // passive federation endpoint
            sts.PassiveRequestorEndpoints.Add(passiveEndpoint);

            // supported protocols

            //Inaccessable due to protection level
            //sts.ProtocolsSupported.Add(new Uri(WSFederationConstants.Namespace));
            sts.ProtocolsSupported.Add(new Uri("http://docs.oasis-open.org/wsfed/federation/200706"));

            // add active STS endpoint
            sts.SecurityTokenServiceEndpoints.Add(activeEndpoint);

            // metadata signing
            entity.SigningCredentials = this.SigningCredentials;

            // serialize 
            var serializer = new MetadataSerializer();
            XElement federationMetadata = null;

            using (var stream = new MemoryStream())
            {
                serializer.WriteMetadata(stream, entity);
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                XmlReaderSettings readerSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit, // prohibit DTD processing
                    XmlResolver = null, // disallow opening any external resources
                    // no need to do anything to limit the size of the input, given the input is crafted internally and it is of small size
                };

                XmlReader xmlReader = XmlTextReader.Create(stream, readerSettings);
                federationMetadata = XElement.Load(xmlReader);
            }

            return federationMetadata;
        }
    }
}