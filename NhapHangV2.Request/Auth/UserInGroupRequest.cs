﻿using System;
using System.Collections.Generic;
using System.Text;
using NhapHangV2.Request.DomainRequests;

namespace NhapHangV2.Request.Auth
{
    public class UserInGroupRequest : AppDomainCatalogueRequest
    {
        /// <summary>
        /// Người dùng
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Nhóm người dùng
        /// </summary>
        public int UserGroupId { get; set; }

        #region Extension Properties

        /// <summary>
        /// Lấy thông tin Người dùng
        /// </summary>
        public UserRequest? Users { get; set; }

        /// <summary>
        /// Lấy thông tin Nhóm người dùng
        /// </summary>
        public UserGroupRequest? UserGroups { get; set; }

        #endregion
    }
}
