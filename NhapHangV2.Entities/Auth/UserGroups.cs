﻿using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NhapHangV2.Entities.Auth
{
    /// <summary>
    /// Nhóm người dùng
    /// </summary>
    public class UserGroups : AppDomainCatalogue
    {

        #region Extension Properties

        [NotMapped]
        public List<int> UserIds { get; set; }

        /// <summary>
        /// Người dùng thuộc nhóm
        /// </summary>
        [NotMapped]
        public IList<UserInGroups> UserInGroups { get; set; }

        /// <summary>
        /// Chức năng + quyền của nhóm
        /// </summary>
        [NotMapped]
        public IList<PermitObjectPermissions> PermitObjectPermissions { get; set; }
        #endregion
    }
}
