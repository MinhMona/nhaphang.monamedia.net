﻿using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NhapHangV2.Entities.Configuration
{
    public class SMSEmailTemplates : AppDomainCatalogue
    {
        /// <summary>
        /// Tiêu đề
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Mẫu là SMS
        /// </summary>
        public bool IsSMS { get; set; }
    }
}
