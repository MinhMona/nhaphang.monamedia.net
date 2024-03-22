using NhapHangV2.Models.DomainModels;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class ShipRequestModel : AppDomainModel
    {
        public int? UID { get; set; }
        public string MainOrderId { get; set; }
        public string TransportrationOrderId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public int? Status { get; set; }
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case (int)RequestShipStatus.Cancel:
                        return "Đã hủy";
                    case (int)RequestShipStatus.UnAccept:
                        return "Chờ duyệt";
                    case (int)RequestShipStatus.Accept:
                        return "Đã duyệt";
                    default:
                        return "Không xác định";
                }
            }
        }
        public string UserName { get; set; }
    }
}
