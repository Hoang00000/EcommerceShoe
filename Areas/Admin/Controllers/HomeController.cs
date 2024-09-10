using dotMVC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Net;
using System.Web.UI.WebControls;
using System.Drawing;
using Antlr.Runtime.Misc;

namespace dotMVC.Areas.Admin.Controllers
{
    [Authorize(Roles =("Admin"))]
    public class HomeController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();


        // GET: Admin/Home
        public ActionResult Index()
        {
            //dem hang hoa
            int totalProducts = CountTotalcthangHoas();
            ViewBag.TotalProducts = totalProducts;
            //dem cthanghoa
            int totalHangHoa = CountTotalhangHoas();
            ViewBag.TotalHangHoa = totalHangHoa;

            int totalBackground = CountTotalBackgrounds();
            ViewBag.TotalBackground = totalBackground;

            int totalAbout = CountTotalAbouts();
            ViewBag.TotalAbout = totalAbout;

            int totalMausac = CountTotalMausacs();
            ViewBag.totalMausac = totalMausac;

            int totalSize = CountTotalSizes();
            ViewBag.totalSize = totalSize;

            int totalDaGiao = CountTotalDaGiaos();
            ViewBag.totalDaGiao = totalDaGiao;


            int totalHuy = CountTotalHuys();
            ViewBag.totalHuy = totalHuy;

            int totalCxyly = CountTotalCxulys();
            ViewBag.totalCxyly = totalCxyly;

            int totalDoanhThuThang = TinhTongTienDoanhThuThang();
            ViewBag.totalDoanhThuThang = totalDoanhThuThang;

            return View();
        }

        private int CountTotalcthangHoas()
        {
            int totalHangHoa = 0;
            totalHangHoa = db.cthanghoas.Count();
            return totalHangHoa;
        }
        private int CountTotalhangHoas()
        {
            int totalHangHoa = 0;
            totalHangHoa = db.hanghoas.Count();
            return totalHangHoa;
        }

        private int CountTotalBackgrounds()
        {
            int totalBackground = 0;
            totalBackground = db.backgrounds.Count();
            return totalBackground;
        }

        private int CountTotalAbouts()
        {
            int totalAbout = 0;
            totalAbout = db.abouts.Count();
            return totalAbout;
        }

        private int CountTotalMausacs()
        {
            int totalMausac = 0;
            totalMausac = db.mausacs.Count();
            return totalMausac;
        }

        private int CountTotalSizes()
        {
            int totalSize = 0;
            totalSize = db.sizegiays.Count();
            return totalSize;
        }

        private int CountTotalDaGiaos()
        {
            int totalDaGiao = 0;
            totalDaGiao = db.hoadons.Count(h => h.matt == 1);
            return totalDaGiao;
        }

        private int CountTotalHuys()
        {
            int totalHuy = 0;
            totalHuy = db.hoadons.Count(h => h.matt == 2);
            return totalHuy;
        }

        private int CountTotalCxulys()
        {
            int totalCxyly = 0;
            totalCxyly = db.hoadons.Count(h => h.matt == 3);
            return totalCxyly;
        }

        // Di chuyển phương thức ra khỏi phạm vi của phương thức khác (nếu có)
        private int TinhTongTienDoanhThuThang()
        {
            // Lấy ngày đầu tiên của tháng hiện tại
            DateTime ngayDauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Lấy ngày cuối cùng của tháng hiện tại
            DateTime ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);

            // Truy vấn cơ sở dữ liệu để lấy tổng tiền của các hóa đơn trong tháng hiện tại
            int tongTien = db.hoadons
                                .Where(hd => hd.ngaydat >= ngayDauThang && hd.ngaydat <= ngayCuoiThang && hd.matt == 1)
                                .Select(hd => hd.tongtien)
                                .DefaultIfEmpty(0)
                                .Sum();

            return tongTien;
        }





        [Route("danhmucsanpham")]
        public ActionResult DanhMucSanPham(string searchTerm, int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 8;

            var products = db.hanghoas.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.tenhh.Contains(searchTerm));
            }

            var lstSanPham = products.OrderBy(p => p.tenhh).ToPagedList(pageNumber, pageSize);

            ViewBag.CurrentFilter = searchTerm;

            // Log số lượng sản phẩm sau khi lọc
            System.Diagnostics.Debug.WriteLine($"Số lượng sản phẩm tìm thấy: {lstSanPham.Count}");

            return View(lstSanPham);
        }


        [Route("chitietsanpham")]
        public ActionResult ChiTietSanPham(string searchTerm, double? bd, double? kt, int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 8;

            var query = db.cthanghoas.AsQueryable();

            // Tìm kiếm theo tên nếu có
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.hanghoa.tenhh.Contains(searchTerm));
            }

            // Tìm kiếm theo khoảng giá nếu cả bd và kt đều có giá trị
            if (bd != null && kt != null && bd <= kt)
            {
                query = query.Where(p => p.dongia >= bd && p.dongia <= kt);
            }

            // Sắp xếp kết quả và phân trang
            var lstChiTietSanPham = query.OrderBy(p => p.hanghoa.tenhh).ToPagedList(pageNumber, pageSize);

            // Truyền searchTerm, bd, và kt đến view
            ViewBag.CurrentFilter = searchTerm;
            ViewBag.BD = bd;
            ViewBag.KT = kt;

            return View(lstChiTietSanPham);
        }


        // danh sách size giày

        [Route("size")]
        public ActionResult Size()
        {
            var lstSize = db.sizegiays.ToList();
            return View(lstSize);
        }
        public ActionResult ThemSize()
        {
            return View();
        }
        public ActionResult ThemSizeAction(int size)
        {
            int idsize = db.sizegiays.Count() + 1;
            var newProduct = new sizegiay
            {
                Idsize = idsize,
                size = size,
            };
            // Thêm đối tượng mới vào DbSet
            db.sizegiays.Add(newProduct);

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();


            // Trả về một view hoặc chuyển hướng đến một action khác
            return RedirectToAction("Size", "Home", new { area = "Admin", newProductId = idsize });
        }
        public ActionResult EditSize(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.sizegiays.Where(a => a.Idsize == id).ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        public ActionResult EditSizeAction(int idsize, int size)
        {
            var product = db.sizegiays.Find(idsize);
            if (product != null)
            {
                product.size = size;


                db.SaveChanges();
            }
            return RedirectToAction("Size", "Home", new { area = "Admin", editedProductId = idsize });
        }


        public ActionResult XoaSize(int id)
        {
            var sizeToDelete = db.sizegiays.FirstOrDefault(s => s.Idsize == id);
            if (sizeToDelete != null)
            {
                db.sizegiays.Remove(sizeToDelete);
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessage"] = "Size not found.";
            }
            return RedirectToAction("Size", "Home", new { area = "Admin" });
        }



        //them danh sach mau
        [Route("mau")]
        public ActionResult Mau()
        {
            var lstMau = db.mausacs.ToList();
            return View(lstMau);
        }
        public ActionResult ThemMau()
        {
            return View();
        }
        public ActionResult ThemMauAction(string mausac)
        {
            int idmau = db.mausacs.Count() + 1;
            var newProduct = new mausac
            {
                Idmau = idmau,
                mausac1 = mausac,
                an = 0,
            };
            // Thêm đối tượng mới vào DbSet
            db.mausacs.Add(newProduct);

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();


            // Trả về một view hoặc chuyển hướng đến một action khác
            return RedirectToAction("Mau", "Home", new { area = "Admin", newProductId = idmau });
        }
        public ActionResult Editmau(int? id) 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.mausacs.Where(a => a.Idmau == id).ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        public ActionResult DeleteMauAction(int? id)
        {
            var product = db.mausacs.Find(id);
            if (product != null)
            {
                product.an = 1;

                db.SaveChanges();
            }
            return RedirectToAction("Mau", "Home", new { area = "Admin"});
        }

        public ActionResult UpMauAction(int? id)
        {
            var product = db.mausacs.Find(id);
            if (product != null)
            {
                product.an = 0;

                db.SaveChanges();
            }
            return RedirectToAction("Mau", "Home", new { area = "Admin" });
        }
        
        public ActionResult EditMauAction(int idmau, string mausac)
        {
            var product = db.mausacs.Find(idmau);
            if (product != null)
            {
                product.mausac1 = mausac;
              

                db.SaveChanges();
            }
            return RedirectToAction("Mau", "Home", new { area = "Admin", editedProductId = idmau });
        }



        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.hanghoas.Where(a => a.mahh == id).ToList();

            ViewData["Loai"] = db.loais.ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        [ValidateInput(false)]
        public ActionResult EditAction(int mahh, string tenhh, int maloai, string mota)
        {
            // Logic để xử lý hành động chỉnh sửa, ví dụ:
            // - Tìm sản phẩm theo ID
            // - Cập nhật tên sản phẩm với giá trị mới từ 'tenhh'
            // - Lưu thay đổi vào cơ sở dữ liệu


            var product = db.hanghoas.Find(mahh);
            if (product != null)
            {
                product.tenhh = tenhh;
                product.maloai = maloai;
                product.mota = mota;
                

                db.SaveChanges();
            }

            // Trả về một view hoặc chuyển hướng đến một action khác
            return RedirectToAction("DanhMucSanPham", "Home", new { area = "Admin", editedProductId = mahh });
        }
        public ActionResult DeleteHangHoaAction(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.hanghoas.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            product.an = 1;
            db.SaveChanges();

            return RedirectToAction("danhmucsanpham", "Home", new { area = "Admin" });
        }

        public ActionResult UpHangHoaAction(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.hanghoas.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            product.an = 0;
            db.SaveChanges();

            return RedirectToAction("Danhmucsanpham", "Home", new { area = "Admin" });
        }

        public ActionResult Add()
        {
            ViewData["Loai"] = db.loais.ToList();
            return View();
        }
        private bool isDeleted = false;

        

        [ValidateInput(false)]
        public ActionResult AddAction(string tenhh, int maloai, string mota)
        {
            // Tạo một đối tượng mới của lớp hanghoa
            int mahh = db.hanghoas.Count() + 1;
            var newProduct = new hanghoa
            {
                mahh = mahh,
                tenhh = tenhh,
                maloai = maloai,
                dacbiet = true,
                soluotxem = 0,
                ngaylap = DateTime.Now,
                mota = mota,
                an = 1
                // Bạn có thể cần thiết lập thêm các thuộc tính khác tùy vào cấu trúc của bảng hanghoa
            };

            // Thêm đối tượng mới vào DbSet
            db.hanghoas.Add(newProduct);

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();


            // Trả về một view hoặc chuyển hướng đến một action khác
            return RedirectToAction("DanhMucSanPham", "Home", new { area = "Admin", newProductId = mahh });

        }




        //chitietsanpham
        public ActionResult AddChiTietHH()
        {
            ViewData["MauSac"] = db.mausacs.ToList();
            ViewData["Sizegiay"] = db.sizegiays.ToList();
            ViewData["Cthanghoa"] = db.hanghoas.Distinct().ToList();
            return View();
        }
        [ValidateInput(false)]
        public ActionResult AddChiTietHHAction(int idhh, int idmau, int idsize, float dongia, int soluongton, float giamgia, string hinh)
        {
            // Check if the record already exists
            var existingProduct = db.cthanghoas.FirstOrDefault(ct => ct.idhanghoa == idhh && ct.idmau == idmau && ct.idsize == idsize);

            if (existingProduct != null)
            {
                // Handle the case where the product already exists
                // You can return a view with an error message or update the existing record if appropriate
                ViewBag.ErrorMessage = "Product with the same color and size already exists.";
                return View("AddChiTietHH"); // Assuming this is the view where you show the form
            }

            // Create a new object of the cthanghoa class
            var newProduct = new cthanghoa
            {
                idhanghoa = idhh,
                idmau = idmau,
                idsize = idsize,
                dongia = dongia,
                soluongton = soluongton,
                hinh = hinh,
                giamgia = giamgia,
            };

            // Add the new object to the DbSet
            db.cthanghoas.Add(newProduct);

            // Save changes to the database
            db.SaveChanges();

            // Redirect to a different action after successful insertion
            return RedirectToAction("ChiTietSanPham", "Home", new { area = "Admin", newProductId = newProduct.idhanghoa });
        }
        public ActionResult EditCT(int? id, int idmau, int idsize)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.cthanghoas.Where(a => a.idhanghoa == id && a.idmau == idmau && a.idsize == idsize).ToList();

            ViewData["Size"] = db.sizegiays.ToList();
            ViewData["Mau"] = db.mausacs.Where(m => m.an == 0).ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }


        public ActionResult EditCTAction(int mahh,int mamaugoc, int masizegoc, int mamau, int masize, int dongia, int slton, int giamgia, string hinh)
        {
            var productToUpdate = db.cthanghoas.FirstOrDefault(p => p.idhanghoa == mahh && p.idmau == mamaugoc && p.idsize == masizegoc);
            if (productToUpdate != null)
            {
                // Kiểm tra xem sản phẩm với idmau và idsize mới đã tồn tại hay chưa
                var existingProduct = db.cthanghoas.FirstOrDefault(p => p.idhanghoa == mahh && p.idmau == mamau && p.idsize == masize);
                if (existingProduct != null)
                {
                    productToUpdate.dongia = dongia;
                    productToUpdate.soluongton = slton;
                    productToUpdate.giamgia = giamgia;
                    productToUpdate.hinh = hinh;

                    db.SaveChanges();
                    return RedirectToAction("ChiTietSanPham", "Home", new { area = "Admin", editedProductId = mahh });
                }

                // Xóa sản phẩm cũ
                db.cthanghoas.Remove(productToUpdate);
                db.SaveChanges();

                // Tạo sản phẩm mới với thông tin cập nhật
                var newProduct = new cthanghoa
                {
                    idhanghoa = mahh,
                    idmau = mamau,
                    idsize = masize,
                    dongia = dongia,
                    soluongton = slton,
                    giamgia = giamgia,
                    hinh = hinh
                };

                db.cthanghoas.Add(newProduct);
                db.SaveChanges();

                // Chuyển hướng đến action khác sau khi cập nhật thành công
                return RedirectToAction("ChiTietSanPham", "Home", new { area = "Admin", editedProductId = mahh });
            }

            // Nếu sản phẩm không tồn tại, báo lỗi
            return RedirectToAction("ChiTietSanPham", "Home", new { area = "Admin", editedProductId = mahh });
        }
        [Route("About")]
        public ActionResult About()
        {
            var lstAbout = db.abouts.ToList();
            return View(lstAbout);
        }
        [ValidateInput(false)]
        public ActionResult AddAbout()
        {
            return View();
        }
        [ValidateInput(false)]
        public ActionResult AddAboutAction(string tieude, string noidung)
        {
            var newProduct = new about
            {
               
                tieude = tieude,
                noidung = noidung,
               

            };
            // Thêm đối tượng mới vào DbSet
            db.abouts.Add(newProduct);

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();


            // Trả về một view hoặc chuyển hướng đến một action khác
            return RedirectToAction("About", "Home", new { area = "Admin", newProductId = newProduct.maab });
        }
        
        public ActionResult EditAbout(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.abouts.Where(a => a.maab == id).ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        [ValidateInput(false)]
        public ActionResult EditAboutAction(int maab, string tieude, string noidung)
        {
            var product = db.abouts.Find(maab);
            if (product != null)
            {

                product.tieude = tieude;
                product.noidung = noidung;


                db.SaveChanges();
            }
            return RedirectToAction("About", "Home", new { area = "Admin", editedProductId = maab });
        }
        public ActionResult XoaAbout(int id)
        {
            var backgroundToDelete = db.abouts.FirstOrDefault(s => s.maab == id);
            if (backgroundToDelete != null)
            {
                db.abouts.Remove(backgroundToDelete);
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessage"] = "Background not found.";
            }
            return RedirectToAction("About", "Home", new { area = "Admin" });
        }





        [Route("background")]

        public ActionResult Background()
        {
            var lstBackground = db.backgrounds.ToList();
            return View(lstBackground);
        }

        public ActionResult Addbackground()
        {
            return View();
        }
        [ValidateInput(false)]

      
        public JsonResult AddbackgroundAction(string ten, string tieude, string url)
        {
            try
            {
                var newProduct = new background
                {
                    ten = ten,
                    tieude = tieude,
                    url = url,
                    active = 0,
                    ngayactive = DateTime.Now
                };

                // Thêm đối tượng mới vào DbSet
                db.backgrounds.Add(newProduct);

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log lỗi ra console hoặc file để kiểm tra
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
            }
        }

        public ActionResult EditBackground(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.backgrounds.Where(a => a.matn == id).ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        public ActionResult EditBackgroundAction(int matn, string ten, string tieude,string url)
        {
            var product = db.backgrounds.Find(matn);
            if (product != null)
            {
                product.ten = ten;
                product.tieude = tieude;
                product.url = url;


                db.SaveChanges();
            }
            return RedirectToAction("Background", "Home", new { area = "Admin", editedProductId = matn });
        }

        public ActionResult XoaBackground(int id)
        {
            var backgroundToDelete = db.backgrounds.FirstOrDefault(s => s.matn == id);
            if (backgroundToDelete != null)
            {
                db.backgrounds.Remove(backgroundToDelete);
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessage"] = "Background not found.";
            }
            return RedirectToAction("Background", "Home", new { area = "Admin" });
        }
    }
}
