using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSync.Common.Models
{
    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public string UserName { get; set; }
        public string STSConnectionId { get; set; }

        public StagedUser UserProperties { get; set; }

        public ValidationResponse()
        {
            UserProperties = new StagedUser();
        }
    }
}
