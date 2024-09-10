using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace dotMVC.Models
{
    public class GioHang
    {
        public int mahh { get; set; }
        public string hinh { get; set; }
        public string tenhh { get; set; }
        public double dongia { get; set; }

        public double giamgia { get; set; }
        public int soluong { get; set; }
        public string mausac { get; set; } // Add this property
        public int size { get; set; }
        public int Idsize { get; set; }
        public int Idmau { get; set; }
        public double tongtien { get; set; }

        public double tinhtongtien()
        {
            if (giamgia != 0)
            {
                tongtien = soluong * giamgia;
            }
            else
            {
                tongtien = soluong * dongia;
            }
            return tongtien;
        }
    }
}
