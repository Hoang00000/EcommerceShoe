using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotMVC.Models
{
    public class YeuThich
    {
        public int MaHH { get; set; }
        public string Hinh { get; set; }
        public string TenHH { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public string MauSac { get; set; }
        public int Size { get; set; }
        public int IdSize { get; set; }
        public int IdMau { get; set; }
        public double TongTien { get; set; }

        public double TinhTongTien()
        {
            TongTien = SoLuong * DonGia;
            return TongTien;
        }
    }
}
