using dotMVC.Data;
using dotMVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace dotMVC.Controllers
{
    public class HomeController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();
        // [Authorize(Roles =("Admin"))]
        public ActionResult Index()
        {
            // Phân quyền
            var roleManager = new RoleManager<IdentityRole, string>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

            // Thêm quyền
            roleManager.Create(new IdentityRole("Admin"));
            roleManager.Create(new IdentityRole("Member"));
            roleManager.Create(new IdentityRole("Author"));

            // Thêm quyền cho user
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            // B1. Tìm user cần cấp quyền
            // Lấy user hiện tại
            var userCurrent = HttpContext.User.Identity.GetUserId();

            // Tìm user qua email
            var user = userManager.Users.SingleOrDefault(u => u.Email == "Admin123@gmail.com");

            // B2. Cấp quyền cho user tìm thấy
            if (user != null)
            {
                userManager.AddToRole(user.Id, "Admin");
            }
            // Lấy tất cả user
            var us = userManager.Users.ToList();
            ViewData["Loai"] = db.loais.Where(i => i.loai1 == "0").ToList();

            // Lấy danh sách sản phẩm
            //var sp = db.hanghoas.Take(3).OrderBy(i => i.soluotxem).ToList();
            var sp = db.hanghoas.OrderByDescending(i => i.soluotxem).Take(3).ToList();
            return View(sp);
        }
        [ChildActionOnly]
        public ActionResult Navbar()
        {
            return PartialView();
        }

        public ActionResult About()
        {
            ViewData["Title"] = "Giới thiệu";

            // Thực hiện truy vấn LINQ để lấy dữ liệu từ bảng about
            var aboutData = db.abouts.ToList();

            // Tạo một list chứa các đối tượng với maab, tieude và noidung tương ứng
            List<dynamic> aboutItems = new List<dynamic>();

            foreach (var data in aboutData)
            {
                dynamic item = new System.Dynamic.ExpandoObject();
                item.Maab = data.maab;
                item.Tieude = data.tieude;
                item.Noidung = data.noidung;
                aboutItems.Add(item);
            }

            return View(aboutItems);
        }
        public class News
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
            public string Content { get; set; }
            public DateTime PublishedDate { get; set; }
            public string ImageUrl { get; set; }
        }

        private List<News> GetSampleNews()
        {
            return new List<News>
            {
                new News { Id = 1, Title = "Giày Nike mới ra mắt", Summary = "Giày Nike với công nghệ mới nhất.", Content = "Nội dung chi tiết về giày Nike.", PublishedDate = DateTime.Now, ImageUrl = "~/Assets/img/gln2_nau1.jpg" },
                new News { Id = 2, Title = "Adidas UltraBoost", Summary = "Đánh giá về Adidas UltraBoost.", Content = "Nội dung chi tiết về Adidas UltraBoost.", PublishedDate = DateTime.Now, ImageUrl = "/Assets/img/ttn4_kem5.jpg" }
            };
        }

        public ActionResult Contact()
        {
            ViewData["title"] = "Liên hệ";
            return View();
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
            return PartialView();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public ActionResult ItemProduct(hanghoa hh)
        {
            ViewData["tenhh"] = hh.tenhh;
            var cthh_db = db.cthanghoas.Where(m => m.idhanghoa == hh.mahh).ToList();
            var cthh = cthh_db.Take(1).SingleOrDefault();
            cthh_db = cthh_db.Where(m => m.idhanghoa == hh.mahh && m.mausac == cthh.mausac).ToList();
            String size = "";
            foreach (var i in cthh_db)
            {
                size = i.idsize.ToString();
            }
            ViewData["size"] = size;

            var mausacs = (from cth in db.cthanghoas
                           join mau in db.mausacs on cth.idmau equals mau.Idmau
                           where cth.idhanghoa == hh.mahh
                           select new
                           {
                               mau.Idmau,
                           }).Distinct().Count();

            ViewData["color"] = mausacs.ToString();

            var productPrice = db.cthanghoas.Where(x => x.idhanghoa == hh.mahh).Select(x => x.dongia).FirstOrDefault();
            ViewData["ProductPrice"] = productPrice;

            var productDiscount = db.cthanghoas.Where(x => x.idhanghoa == hh.mahh).Select(x => x.giamgia).FirstOrDefault();
            ViewData["ProductDiscount"] = productDiscount;

            return PartialView(cthh);
        }
    }
}
