using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Forms;
using System.Diagnostics;

namespace ADSyncSiteSetup
{
    public partial class Setup: Form
    {
        private void LoadRemoteSiteList()
        {
            _siteConfig = _api.GetSiteConfig();
            lstRemoteDomainList.Items.AddRange(_siteConfig.SiteDomains.ToArray());
        }

        private void CheckConfig()
        {
            _hasConfig = (txtApiKey.Text.Length > 0 && txtSiteUrl.Text.Length > 0);
        }

        public List<ADDomain> GetADDomainList(string username, string password)
        {
            Domain domain = null;
            List<ADDomain> ret = new List<ADDomain>();
            Forest forest = null;
            string domainController = "thehack.webreunions.aih.local";
            try
            {
                var context = new DirectoryContext(DirectoryContextType.Domain, domainController, username, password);
                // Connect to Domain
                domain = Domain.GetDomain(context);

                // Get Current Domain Forest
                forest = domain.Forest;

                // Get all Domains in Forest
                foreach (Domain item in forest.Domains)
                {
                    ADDomain ad = new ADDomain();

                    // Create new class to get the Path
                    ad.Name = item.Name;
                    ad.Path = item.GetDirectoryEntry().Path;

                    ret.Add(ad);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }
    }
    public class ADDomain
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
