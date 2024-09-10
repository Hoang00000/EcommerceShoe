using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotMVC.Models
{
    public class SanPhamViewModel
    {
        public string TenHH { get; set; }     // Tên hàng hóa
        public string MauSac { get; set; }    // Màu sắc của sản phẩm
        public int Size { get; set; }      // Kích thước của sản phẩm
        public int SoLuongMua { get; set; }   // Số lượng sản phẩm mua
    }
}