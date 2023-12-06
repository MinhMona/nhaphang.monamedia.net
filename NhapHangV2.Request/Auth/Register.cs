using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NhapHangV2.Request.Auth
{
    public class Register
    {
        [MaxLength(128, ErrorMessage = "Tên đăng nhập tối đa 128 ký tự")]
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc nhập")]
        public string? UserName { set; get; }

        [StringLength(128, ErrorMessage = "Mật khẩu phải có ít nhất 8 kí tự và tối đa 128 ký tự", MinimumLength = 8)]
        [Required(ErrorMessage = "Mật khẩu là bắt buộc nhập")]
        [DataType(DataType.Password)]
        public string? Password { set; get; }

        [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khẩu")]
        [StringLength(128, ErrorMessage = "Mật khẩu xác nhận phải có ít nhất 8 kí tự và tối đa 128 ký tự", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không giống với mật khẩu")]
        public string? ConfirmPassword { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [StringLength(50, ErrorMessage = "Số kí tự của email phải nhỏ hơn 50!")]
        [Required(ErrorMessage = "Vui lòng nhập Email!")]
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string? Email { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        [StringLength(12, ErrorMessage = "Số kí tự của số điện thoại phải lớn hơn 8 và nhỏ hơn 12!", MinimumLength = 9)]
        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại!")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        [StringLength(255, ErrorMessage = "Tên của bạn quá dài", MinimumLength = 1)]
        //[Required(ErrorMessage = "Vui lòng nhập họ của bạn!")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Họ
        /// </summary>
        [StringLength(255, ErrorMessage = "Họ của bạn quá dài", MinimumLength = 1)]
        //[Required(ErrorMessage = "Vui lòng nhập tên của bạn!")]
        public string? LastName { get; set; }

        /// <summary>
        /// Họ và tên
        /// </summary>
        [StringLength(255, ErrorMessage = "Họ và tên của bạn quá dài", MinimumLength = 1)]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên của bạn!")]
        public string? FullName { get; set; }

        public int? WarehouseChinaId { get; set; }
        public int? WarehouseVietNamId { get; set; }
        public int? ShippingMethodId { get; set; }
        public string? Address { get; set; }

    }
}
