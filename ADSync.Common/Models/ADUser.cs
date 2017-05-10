using System;
using Newtonsoft.Json;
using ADSync.Common.Interfaces;
using ADSync.Common.Enums;

namespace ADSync.Common.Models
{
    public class ADUser
    {
        public bool? Enabled { get; set; }
        public DateTime? AccountExpirationDate { get; set; }
        public DateTime? AccountLockoutTime { get; set; }
        public int BadLogonCount { get; set; }
        public DateTime? LastBadPasswordAttempt { get; set; }
        public DateTime? LastLogon { get; set; }
        public DateTime? LastPasswordSet { get; set; }
        public bool PasswordNeverExpires { get; set; }
        public string SamAccountName { get; set; }
        public string OrgSamAccountName { get; set; }
        public bool SmartcardLogonRequired { get; set; }
        public bool UserCannotChangePassword { get; set; }
    }
}