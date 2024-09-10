using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotMVC.Models
{
    public class CommentViewModel
    {
      
        public int ProductId { get; set; }
        public string Content { get; set; }
        public bool HasPurchased { get; set; }
    }
}
