using dotMVC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotMVC.Models
{
    public class ProductDetailsViewModel
    {
        public int ProductId { get; set; }
        public IEnumerable<hanghoa> hanghoa { get; set; }
        // Các thuộc tính khác nếu cần
    }
}