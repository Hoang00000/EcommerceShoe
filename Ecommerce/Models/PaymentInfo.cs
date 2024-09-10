using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotMVC.Models
{
    public class PaymentInfo
    {
        public int IdHD { get; set; }
        public string Hoten { get; set; }
        public string Sodienthoai { get; set; }
        public string Diachi { get; set; }
        public decimal Tongtien { get; set; }
    }

}