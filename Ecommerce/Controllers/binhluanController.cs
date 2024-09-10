using dotMVC.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Net.Http;
using System.Text;
using dotMVC.Models;
using System.Data.Sql;
using System.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;
using System.Web.Services.Description;
using Microsoft.AspNet.Identity;

namespace dotMVC.Controllers
{
    public class binhluanController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();

        public ActionResult Comment(int productId, string commentText)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Nếu người dùng chưa đăng nhập, bạn có thể xử lý hoặc thông báo ở đây
                return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập
            }

            // Lấy Id của người dùng hiện tại
            string userId = User.Identity.GetUserId();

            // Tạo đối tượng bình luận mới
            binhluan1 newComment = new binhluan1
            {
                mahh = productId, // Mã sản phẩm mà bình luận thuộc về
                makh = userId,    // Mã người dùng đăng bình luận
                ngaybl = DateTime.Now, // Ngày bình luận (hiện tại)
                noidung = commentText // Nội dung bình luận từ form
            };

            // Thêm bình luận vào cơ sở dữ liệu
            db.binhluan1.Add(newComment);
            db.SaveChanges();

            // Sau khi thêm bình luận thành công, có thể cập nhật lại view chi tiết sản phẩm (tùy nhu cầu của bạn)

            // Chuyển hướng về action Details để hiển thị lại sản phẩm sau khi bình luận
            return RedirectToAction("Details", new { id = productId });
        }

      
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.hanghoas.Where(a => a.mahh == id).ToList();
            ViewData["CTHangHoa"] = db.cthanghoas.Where(s => s.idhanghoa == id).ToList();
            var loai = db.hanghoas.Where(s => s.mahh == id).Select(x => x.maloai).FirstOrDefault();
            ViewData["SpLienQuan"] = db.hanghoas.Where(a => a.maloai == loai).ToList();

            // Lấy đường dẫn hình ảnh từ cơ sở dữ liệu
            var productImage = db.cthanghoas.Where(x => x.idhanghoa == id).Select(x => x.hinh).FirstOrDefault();
            // Trích xuất tên hình ảnh từ đường dẫn
            var imageName = System.IO.Path.GetFileName(productImage);

            // Lấy giá của sản phẩm từ cơ sở dữ liệu
            var productPrice = db.cthanghoas.Where(x => x.idhanghoa == id).Select(x => x.dongia).FirstOrDefault();
            ViewData["ProductPrice"] = productPrice;

            // Lấy giảm giá của sản phẩm từ cơ sở dữ liệu
            var productDiscount = db.cthanghoas.Where(x => x.idhanghoa == id).Select(x => x.giamgia).FirstOrDefault();
            ViewData["ProductDiscount"] = productDiscount;

            // Lưu tên hình ảnh vào ViewData để sử dụng trong view
            ViewData["ProductImageName"] = imageName;

            // Lấy danh sách size và màu sắc của sản phẩm để hiển thị
            ViewData["Size"] = (from cth in db.cthanghoas
                                join sg in db.sizegiays on cth.idsize equals sg.Idsize
                                where cth.idhanghoa == id
                                select sg).Distinct().ToList();

            ViewData["Colors"] = (from cth in db.cthanghoas
                                  join sg in db.mausacs on cth.idmau equals sg.Idmau
                                  where cth.idhanghoa == id
                                  select sg).Distinct().ToList();

            ViewData["Image"] = (from cth in db.hanghoas
                                 join sg in db.anhcts on cth.mahh equals sg.mahh
                                 where cth.mahh == id
                                 select sg).Distinct().ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
    
    public ActionResult Index(int? page, string sortOrder, string[] prices)
        {
            var sp = db.hanghoas.ToList();

            // Convert prices array to a string for ViewData
            ViewData["CurrentPrices"] = prices;

            // Function to calculate display price
            Func<hanghoa, float> getDisplayPrice = h =>
            {
                var cthh = db.cthanghoas.Where(c => c.idhanghoa == h.mahh).FirstOrDefault();
                return cthh != null ? (float)(cthh.giamgia > 0 ? cthh.giamgia : cthh.dongia) : 0;
            };

            // Filter products based on selected price ranges
            if (prices != null && prices.Any())
            {
                var filteredProducts = new List<hanghoa>();

                foreach (var price in prices)
                {
                    switch (price)
                    {
                        case "duoi-150000":
                            filteredProducts.AddRange(sp.Where(h => getDisplayPrice(h) < 150000));
                            break;
                        case "200000-300000":
                            filteredProducts.AddRange(sp.Where(h => getDisplayPrice(h) >= 200000 && getDisplayPrice(h) <= 300000));
                            break;
                        case "300000-450000":
                            filteredProducts.AddRange(sp.Where(h => getDisplayPrice(h) >= 300000 && getDisplayPrice(h) <= 450000));
                            break;
                        case "tren-450000":
                            filteredProducts.AddRange(sp.Where(h => getDisplayPrice(h) > 450000));
                            break;
                    }
                }

                sp = filteredProducts.Distinct().ToList(); // Remove duplicates after all filters are applied
            }

            // Sort the products based on price
            switch (sortOrder)
            {
                case "price_desc":
                    sp = sp.OrderByDescending(getDisplayPrice).ToList();
                    break;
                case "price_asc":
                    sp = sp.OrderBy(getDisplayPrice).ToList();
                    break;
                default:
                    break;
            }

            int pageSize = 8; // Number of products per page
            int pageNumber = (page ?? 1); // Current page number, default is 1
            var pagedProducts = sp.ToPagedList(pageNumber, pageSize);

            ViewData["Loai"] = db.loais.Where(i => i.loai1 == "0").ToList();
            ViewData["CurrentSort"] = sortOrder;

            return View(pagedProducts);
        }
        [ChildActionOnly]
        public ActionResult itemProduct(hanghoa hh)
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

            var displayPrice = productDiscount > 0 ? productDiscount : productPrice;
            ViewData["DisplayPrice"] = displayPrice;

            return PartialView(cthh);
        }


     

    }

}