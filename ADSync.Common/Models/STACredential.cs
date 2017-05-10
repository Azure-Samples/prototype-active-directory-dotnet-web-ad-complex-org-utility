using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSync.Common.Models
{
    public class STSCredential
    {
        public string StagedUserId { get; set; }
        public string RemoteSiteId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string Domain { get; set; }
        public string STSConnectionId { get; set; }
    }
    public class PasswordReset
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public bool Unlock { get; set; }
        public bool SetChangePasswordAtNextLogon { get; set; }
    }
}
