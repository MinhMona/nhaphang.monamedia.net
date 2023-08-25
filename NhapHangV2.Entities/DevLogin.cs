using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class DevLogin: DomainEntities.AppDomain
    {
        public string Name { get; set; }
        public string TelegramId { get; set; }
    }
}
