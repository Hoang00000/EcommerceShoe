using dotMVC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotMVC.Areas.Admin.Model
{
    public class ThongkeViewModel
    {
        public class ThongKeSanPhamViewModel
        {
            public List<ThongKeLoaiViewModel> ThongKeLoaiGiay { get; set; }
        }

        public class ThongKeLoaiViewModel
        {
            public string TenLoaiGiay { get; set; }
            public int SoLuongSanPham { get; set; }
            public List<SanPhamViewModel> SanPhamTheoLoai { get; set; }
        }

        public class SanPhamViewModel
        {
            public int mahh { get; set; }
            public string tenhh { get; set; }
        }

    }
}