using System.Web;
using System.Web.Optimization;

namespace ADSyncApi
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/scripts/jquery-{version}.js",
                        "~/scripts/lib/moment.min.js",
                        "~/scripts/lib/moment-timezone.min.js",
                        "~/scripts/lib/moment-tzData.js",
                        "~/scripts/lib/jstz-1.0.4.min.js",
                        "~/scripts/App/global.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/scripts/bootstrap.js",
                      "~/scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                    "~/scripts/lib/jquery.dataTables.js",
                    "~/scripts/lib/dataTables.bootstrap.js"));

            bundles.Add(new StyleBundle("~/content/datatablescss").Include(
                    "~/Content/datatables/css/dataTables.bootstrap.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
