using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotMVC.Models
{
    public class HoaDonViewModel
    {
        public int MaSoHD { get; set; }
        public string diachi { get; set; }
        public string sdt { get; set; }

        public string Email { get; set; }
        public string TenTT { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }

        // Thêm thuộc tính này
        public List<SanPhamViewModel> SanPham { get; set; }
    }
}