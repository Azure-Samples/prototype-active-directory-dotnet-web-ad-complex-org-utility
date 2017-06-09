using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Common
{
    public static class Utils
    {
        public static string ApplicationName { get; set; }

        /// <summary>
        /// Retrieve a strongly-typed claim lookup result from a user's ClaimsIdentity
        /// </summary>
        /// <typeparam name="T">the type to return (coercing the claim string)</typeparam>
        /// <param name="claimsIdentity">the user's ClaimsIdentity object</param>
        /// <param name="claimName">string name of the claim to retrieve</param>
        /// <returns>the claim value</returns>
        public static T GetClaim<T>(ClaimsIdentity claimsIdentity, string claimName)
        {
            if (claimsIdentity == null)
                return default(T);

            try
            {
                var res = claimsIdentity.Claims.Where(cx => cx.Type == claimName).Select(c => c.Value).SingleOrDefault();
                return (res == null) ? default(T) : (T)Convert.ChangeType(res, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
        public static T GetClaim<T>(string claimName)
        {
            ClaimsPrincipal claimsPrincipal = HttpContext.Current.User as ClaimsPrincipal;

            if (claimsPrincipal == null)
                return default(T);

            try
            {
                var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
                var res = claimsIdentity.Claims.Where(cx => cx.Type == claimName).Select(c => c.Value).SingleOrDefault();
                return (T)Convert.ChangeType(res, typeof(T));
            }
            catch
            {
                return default(T);
            }

        }
        public static string GetClaim(string claimName)
        {
            return GetClaim<string>(claimName);
        }

        public static string GetDomain()
        {
            return ConfigurationManager.AppSettings["Domain"];
        }

        /// <summary>
        /// Retrieve a strongly-typed claim lookup result from a user's ClaimsPrincipal
        /// </summary>
        /// <typeparam name="T">the type to return (coercing the claim string)</typeparam>
        /// <param name="claimsPrincipal">the user's ClaimsPrincipal object</param>
        /// <param name="claimName">string name of the claim to retrieve</param>
        /// <returns>the claim value</returns>
        public static T GetClaim<T>(ClaimsPrincipal claimsPrincipal, string claimName)
        {
            if (claimsPrincipal == null)
                return default(T);

            try
            {
                var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
                return GetClaim<T>(claimsIdentity, claimName);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Retrieve a claim lookup result from a user's ClaimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">the user's ClaimsPrincipal object</param>
        /// <param name="claimName">string name of the claim to retrieve</param>
        /// <returns>the claim value string</returns>
        public static string GetClaim(ClaimsPrincipal claimsPrincipal, string claimName)
        {
            return GetClaim<string>(claimsPrincipal, claimName);
        }

        /// <summary>
        /// Retrieve a claim lookup result from a user's ClaimsIdentity
        /// </summary>
        /// <param name="claimsIdentity">the user's ClaimsIdentity</param>
        /// <param name="claimName">string name of the claim to retrieve</param>
        /// <returns>the claim value string</returns>
        public static string GetClaim(ClaimsIdentity claimsIdentity, string claimName)
        {
            return GetClaim<string>(claimsIdentity, claimName);
        }

        /// <summary>
        /// Return the collection of the user's current system roles
        /// </summary>
        /// <param name="claimsPrincipal">the user's ClaimsPrincipal object</param>
        /// <returns>collection of strings</returns>
        public static IEnumerable<String> GetRoles(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                throw new NullReferenceException("claimsPrinciple is null");

            return claimsPrincipal.Claims.Where(x => x.Type == ClaimTypes.Role).Select(n => n.Value);
        }

        /// <summary>
        /// Allows testing an object against a set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsAnyOf<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Insert spaces before camel-cased words in a token, i.e., "ThisIsAString" to "This Is A String"
        /// </summary>
        /// <param name="s">the token</param>
        /// <returns>string</returns>
        public static string SplitCamelCase(string s)
        {
            return Regex.Replace(s, "([a-z](?=[A-Z0-9])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        public static string GetTempPassword()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        public static string GetHtmlMessageWrapper(string title, string body)
        {
            var res = new StringBuilder("<html><head>");
            res.AppendFormat("<title>{0}</title>", title);
            res.AppendLine("<style type='text/css'>");
            res.AppendLine("    p, blockquote, div, span {font:11pt calibri, arial}");
            res.AppendLine("</style></head>");
            res.AppendLine("<body><div>");
            res.Append(body);
            res.AppendLine("</div></body></html>");
            return res.ToString();
        }

        public static string GenApiKey()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        public static T ConvertDynamic<T>(dynamic data)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data));
        }

        public static T ConvertDynamic<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static string GetFormattedException(Exception ex)
        {
            var message = " (original error: " + ex.Source + "/" + ex.Message + "\r\nStack Trace: " +
                ex.StackTrace + ")";
            if (ex.InnerException != null)
            {
                message += "\r\nInner Exception: " + ex.GetBaseException();
            }
            return message;
        }

        public static void AddLogEntry(string message)
        {
            AddLogEntry("Application", EventLogEntryType.Information, 0, null);
        }

        public static void AddLogEntry(string message, EventLogEntryType entryType)
        {
            AddLogEntry("Application", entryType, 0, null);
        }

        public static void AddLogEntry(string message, EventLogEntryType entryType, int eventId = 0, Exception ex = null)
        {
            AddLogEntry("Application", message, entryType, eventId, ex);
        }
        public static void AddLogEntry(string source, string message, EventLogEntryType entryType)
        {
            AddLogEntry(source, message, entryType, 0, null);
        }

        public static void AddLogEntry(string source, string message)
        {
            AddLogEntry(source, message, EventLogEntryType.Information, 0, null);
        }

        public static void AddLogEntry(string source, string message, EventLogEntryType entryType, int eventId, Exception ex)
        {
            if (ex != null)
            {
                message += GetFormattedException(ex);
            }

            EventLog.WriteEntry(source, message, entryType, eventId);
        }
        public static string SetupLog(string logSource)
        {
            if (!EventLog.Exists(logSource))
            {
                try
                {
                    var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
                    if (isAdmin)
                    {
                        EventLog.CreateEventSource(logSource, logSource);
                        AddLogEntry("Created event log " + logSource, EventLogEntryType.Information);
                    }
                    else
                    {
                        logSource = "Application";
                    }
                }
                catch (Exception)
                {
                    logSource = "Application";
                }
            }

            return logSource;
        }
    }
}