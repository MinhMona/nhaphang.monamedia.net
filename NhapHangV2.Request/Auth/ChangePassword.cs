﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NhapHangV2.Request.Auth
{
    /// <summary>
    /// Model đổi mật khẩu
    /// </summary>
    public class ChangePassword
    {
        /// <summary>
        /// Id user
        /// </summary>
        public int userId { get; set; }

        /// <summary>
        /// Mật khẩu cũ
        /// </summary>
        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }

        /// <summary>
        /// Mật khẩu mới
        /// </summary>
        [StringLength(128, ErrorMessage = "Mật khẩu phải có ít nhất 8 kí tự và tối đa 128 ký tự", MinimumLength = 8)]
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc nhập")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        /// <summary>
        /// Xác nhận mật khẩu mới
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khẩu mới")]
        [StringLength(128, ErrorMessage = "Mật khẩu xác nhận phải có ít nhất 8 kí tự và tối đa 128 ký tự", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string? ConfirmNewPassword { get; set; }
    }
}
