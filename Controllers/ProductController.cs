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
    public class ProductController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();
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

            int pageSize = 9; // Number of products per page
            int pageNumber = (page??1); // Current page number, default is 1
            var pagedProducts = sp.ToPagedList(pageNumber, pageSize);

            ViewData["Loai"] = db.loais.Where(i => i.loai1 == "0").ToList();
            ViewData["CurrentSort"] = sortOrder;

            return View(pagedProducts);
        }

        public ActionResult MuaNgay()
        {
            
            return View();
        }
      
           public ActionResult GioHang()
        {

            //var gh = new List<GioHang>();
            //    gh.Add(new GioHang(1, "gln1_den.jpg", "Giay nam nu",348000,1, "den","39"));
            //    gh.Add(new GioHang(1, "gln1_den1.jpg", "Giay nam nu a", 328000, 3, "den", "39"));
            //    gh.Add(new GioHang(1, "gln1_den2. jpg", "Giay nam nu b", 348000, 1, "den", "39"));

            //    Session["Giohang"]= gh;


            return View();
        }
        public ActionResult YeuThich()
        {
            return View();
        }
        public ActionResult ThemGioHang(int? id, int? color, int? sl, int? size)
        {
            // Kiểm tra các tham số đầu vào
            if (id == null || color == null || sl == null || size == null)
            {
                return Json(new { message = "false", error = "Tham số không hợp lệ" }, JsonRequestBehavior.AllowGet);
            }

            // Lấy thông tin chi tiết của sản phẩm từ cơ sở dữ liệu
            var productDetails = (from hh in db.hanghoas
                                  join cthh in db.cthanghoas on hh.mahh equals cthh.idhanghoa
                                  join mau in db.mausacs on cthh.idmau equals mau.Idmau
                                  join sizeGiay in db.sizegiays on cthh.idsize equals sizeGiay.Idsize
                                  where hh.mahh == id && cthh.idmau == color && cthh.idsize == size
                                  select new
                                  {
                                      hh.tenhh,
                                      cthh.dongia,
                                      cthh.giamgia,
                                      cthh.hinh,
                                      cthh.idmau,
                                      cthh.idsize,
                                      mau.mausac1,
                                      sizeGiay.size,
                                      cthh.soluongton // Lấy số lượng tồn kho
                                  }).FirstOrDefault();

            if (productDetails == null)
            {
                return Json(new { message = "false", error = "Sản phẩm không tồn tại" }, JsonRequestBehavior.AllowGet);
            }

            // Kiểm tra số lượng tồn kho
            if (productDetails.soluongton < sl)
            {
                return Json(new { message = "false", error = $"Số lượng sản phẩm không đủ. Còn lại: {productDetails.soluongton}" }, JsonRequestBehavior.AllowGet);
            }

            // Kiểm tra xem giỏ hàng đã được khởi tạo chưa
            List<Models.GioHang> gioHang;
            if (Session["Giohang"] != null)
            {
                gioHang = Session["Giohang"] as List<Models.GioHang>;
            }
            else
            {
                gioHang = new List<Models.GioHang>();
            }

            // Kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng chưa
            var existingItem = gioHang.FirstOrDefault(i => i.mahh == id && i.Idsize == size && i.Idmau == color);
            if (existingItem != null)
            {
                // Sản phẩm đã tồn tại trong giỏ hàng, cập nhật số lượng
                if (existingItem.soluong + sl > productDetails.soluongton)
                {
                    return Json(new { message = "false", error = $"Số lượng sản phẩm không đủ. Còn lại: {productDetails.soluongton}" }, JsonRequestBehavior.AllowGet);
                }
                existingItem.soluong += sl ?? 1;
                existingItem.tinhtongtien();
            }
            else
            {
                // Sản phẩm chưa tồn tại trong giỏ hàng, thêm mới
                var newGioHangItem = new Models.GioHang
                {
                    mahh = id.Value,
                    tenhh = productDetails.tenhh,
                    dongia = productDetails.dongia,
                    giamgia = productDetails.giamgia,
                    hinh = productDetails.hinh,
                    Idmau = productDetails.idmau,
                    Idsize = productDetails.idsize,
                    soluong = sl.Value,
                    mausac = productDetails.mausac1,
                    size = productDetails.size,
                    tongtien = sl.Value * productDetails.dongia
                };

                gioHang.Add(newGioHangItem);
            }

            // Cập nhật session
            Session["Giohang"] = gioHang;

            return Json(new { message = "true" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ThemYeuThich(int? id, int? color, int? size)
        {
            // Kiểm tra các tham số đầu vào
            if (id == null || color == null || size == null)
            {
                return Json(new { message = "false", error = "Tham số không hợp lệ" }, JsonRequestBehavior.AllowGet);
            }

            // Lấy thông tin chi tiết của sản phẩm từ cơ sở dữ liệu
            var chiTietSanPham = (from hh in db.hanghoas
                                  join cthh in db.cthanghoas on hh.mahh equals cthh.idhanghoa
                                  join mau in db.mausacs on cthh.idmau equals mau.Idmau
                                  join sizeGiay in db.sizegiays on cthh.idsize equals sizeGiay.Idsize
                                  where hh.mahh == id && cthh.idmau == color && cthh.idsize == size
                                  select new
                                  {
                                      hh.tenhh,
                                      cthh.dongia,
                                      cthh.hinh,
                                      cthh.idmau,
                                      cthh.idsize,
                                      mau.mausac1,
                                      sizeGiay.size
                                  }).FirstOrDefault();

            if (chiTietSanPham == null)
            {
                return Json(new { message = "false", error = "Sản phẩm không tồn tại" }, JsonRequestBehavior.AllowGet);
            }

            // Kiểm tra xem danh sách yêu thích đã được khởi tạo chưa
            List<Models.YeuThich> yeuThich;
            if (Session["Yeuthich"] != null)
            {
                yeuThich = Session["Yeuthich"] as List<Models.YeuThich>;
            }
            else
            {
                yeuThich = new List<Models.YeuThich>();
            }

            // Kiểm tra xem sản phẩm đã tồn tại trong danh sách yêu thích chưa
            var sanPhamTonTai = yeuThich.FirstOrDefault(i => i.MaHH == id && i.IdSize == size && i.IdMau == color);
            if (sanPhamTonTai != null)
            {
                return Json(new { message = "false", error = "Sản phẩm đã tồn tại trong danh sách yêu thích" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Sản phẩm chưa tồn tại trong danh sách yêu thích, thêm mới
                var sanPhamYeuThichMoi = new Models.YeuThich
                {
                    MaHH = id.Value,
                    TenHH = chiTietSanPham.tenhh,
                    DonGia = chiTietSanPham.dongia,
                    Hinh = chiTietSanPham.hinh,
                    IdMau = chiTietSanPham.idmau,
                    IdSize = chiTietSanPham.idsize,
                    SoLuong = 1, // Mặc định 1 cho danh sách yêu thích
                    MauSac = chiTietSanPham.mausac1,
                    Size = chiTietSanPham.size,
                    TongTien = chiTietSanPham.dongia // Tổng tiền = đơn giá vì số lượng mặc định là 1
                };

                sanPhamYeuThichMoi.TinhTongTien();
                yeuThich.Add(sanPhamYeuThichMoi);
            }

            // Cập nhật session
            Session["Yeuthich"] = yeuThich;

            return Json(new { message = "true" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LaySoLuongYeuThich()
        {
            var yeuThich = Session["Yeuthich"] as List<Models.YeuThich>;
            int count = (yeuThich != null) ? yeuThich.Count : 0;

            return Json(new { count }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult XoaYeuThich(int? id, int? color, int? size)
        {
            if (id == null || color == null || size == null)
            {
                return Json(new { message = "false", error = "Tham số không hợp lệ" }, JsonRequestBehavior.AllowGet);
            }

            var yeuThich = Session["Yeuthich"] as List<Models.YeuThich>;
            if (yeuThich != null)
            {
                var itemToRemove = yeuThich.FirstOrDefault(i => i.MaHH == id && i.IdSize == size && i.IdMau == color);
                if (itemToRemove != null)
                {
                    yeuThich.Remove(itemToRemove);
                    Session["Yeuthich"] = yeuThich; // Update the session
                    return Json(new { message = "true" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { message = "false", error = "Sản phẩm không tồn tại trong danh sách yêu thích" }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult CapnhatGh(int id, int size, int color, int quantity)
        {
            // Kiểm tra số lượng tồn kho trong cơ sở dữ liệu
            var cthanghoa = db.cthanghoas
                              .Where(c => c.idhanghoa == id && c.idsize == size && c.idmau == color)
                              .FirstOrDefault();

            if (cthanghoa == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong kho." });
            }

            if (cthanghoa.soluongton < quantity)
            {
                return Json(new { success = false, message = $"Số lượng tồn kho không đủ. Hiện chỉ còn {cthanghoa.soluongton} sản phẩm." });
            }

            // Cập nhật giỏ hàng trong Session
            var cart = Session["Giohang"] as List<GioHang>;
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn trống." });
            }

            var item = cart.FirstOrDefault(i => i.mahh == id && i.Idsize == size && i.Idmau == color);
            if (item != null)
            {
                item.soluong = quantity;
                item.tinhtongtien();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
        }

        [HttpPost]
        public ActionResult XoaGh(int id, int size, int color)
        {
            var cart = Session["Giohang"] as List<GioHang>;
            var item = cart.FirstOrDefault(i => i.mahh == id && i.Idsize == size && i.Idmau == color);
            if (item != null)
            {
                cart.Remove(item);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        public ActionResult tongtien()
        {
            List<Models.GioHang> gioHang = Session["Giohang"] as List<Models.GioHang>;
            double totalAmount = gioHang.Sum(item => item.tongtien);
            ViewBag.TotalAmount = totalAmount;
            return View(gioHang);
        }
        public ActionResult GetCartItemCount()
        {
            // Lấy số lượng sản phẩm từ Session
            int itemCount = 0;
            if (Session["CartItemCount"] != null)
            {
                itemCount = (int)Session["CartItemCount"];
            }

            // Trả về số lượng sản phẩm dưới dạng JSON
            return Json(new { itemCount }, JsonRequestBehavior.AllowGet);
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

            var luotxem = db.hanghoas.Where(x => x.mahh == hh.mahh).Select(x => x.soluotxem).FirstOrDefault();
            ViewData["soluotxem"] = luotxem;

            var displayPrice = productDiscount > 0 ? productDiscount : productPrice;
            ViewData["DisplayPrice"] = displayPrice;

            return PartialView(cthh);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.hanghoas.Where(a => a.mahh == id).ToList();
            //  var x = db.cthanghoas.Where(s => s.idhanghoa == id).ToList();
            ViewData["CTHangHoa"] = db.cthanghoas.Where(s => s.idhanghoa == id).ToList();
            var loai = db.hanghoas.Where(s => s.mahh == id).Select(x => x.maloai).FirstOrDefault();
            ViewData["SpLienQuan"] = db.hanghoas.Where(a => a.maloai == loai).Take(4).ToList();
            // Lấy đường dẫn hình ảnh từ cơ sở dữ liệu
            var productImage = db.cthanghoas.Where(x => x.idhanghoa == id).Select(x => x.hinh).FirstOrDefault();

            // Trích xuất tên hình ảnh từ đường dẫn
            var imageName = System.IO.Path.GetFileName(productImage);
            // Lấy giá của sản phẩm từ cơ sở dữ liệu
            var productPrice = db.cthanghoas.Where(x => x.idhanghoa == id).Select(x => x.dongia).FirstOrDefault();

            // Lưu giá vào ViewData để sử dụng trong view
            ViewData["ProductPrice"] = productPrice;

            // Lấy giảm giá của sản phẩm từ cơ sở dữ liệu
            var productDiscount = db.cthanghoas.Where(x => x.idhanghoa == id).Select(x => x.giamgia).FirstOrDefault();

            // Lưu giảm giá vào ViewData để sử dụng trong view
            ViewData["ProductDiscount"] = productDiscount;

            // Lưu tên hình ảnh vào ViewData để sử dụng trong view
            ViewData["ProductImageName"] = imageName;

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
            var luotxem = db.hanghoas.Find(id);
            if (luotxem != null)
            {
                luotxem.soluotxem++;

                db.SaveChanges();
            }
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }
        public ActionResult Search(string query)
        {
            if (query != null)
            {
                ViewBag.CurrentQuery = query; // Store query in ViewBag
                query = query.ToLower();

                // Perform search based on the query
                var searchResults = db.hanghoas
                    .Where(p => p.tenhh.ToLower().Contains(query) || p.mota.ToLower().Contains(query))
                    .Select(p => new
                    {
                        p.mahh,
                        p.tenhh,
                        p.mota,
                        cthanghoas = p.cthanghoas.Select(ct => new { ct.dongia, ct.giamgia, ct.hinh }).FirstOrDefault()
                    })
                    .ToList()
                    .Select(p => new hanghoa
                    {
                        mahh = p.mahh,
                        tenhh = p.tenhh,
                        mota = p.mota,
                        cthanghoas = new List<cthanghoa>
                        {
                    new cthanghoa
                    {
                        dongia = p.cthanghoas.dongia,
                        giamgia = p.cthanghoas.giamgia,
                        hinh = p.cthanghoas.hinh
                    }
                        }
                    })
                    .ToList(); // Convert to a list

                // Return the view with the search results (not paginated)
                return View(searchResults);
            }
            else
            {
                // Handle the case where query is null
                return RedirectToAction("Index"); // Example: Redirect to the main page
            }
        }

        public ActionResult categories(int? loai)
        {
            if (loai == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.hanghoas.Where(a => a.maloai == loai).ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            ViewData["Loai"] = db.loais.Where(i => i.loai1 == "0").ToList();
            return View(product);
        }

      
    }

}