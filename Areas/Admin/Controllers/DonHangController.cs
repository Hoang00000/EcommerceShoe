using dotMVC.Data;
using dotMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace dotMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = ("Admin"))]
    public class DonHangController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();
        // GET: Admin/DonHang
        public ActionResult Index()
        {
            ViewData["Cthoadon"] = db.cthoadons.ToList();
            return View();
        }
        [Route("HoaDon")]
        public ActionResult HoaDon()
        {
            var lstHoaDon = (from hd in db.hoadons
                             select new HoaDonViewModel
                             {
                                 MaSoHD = hd.masohd,
                                 diachi = hd.diachi,
                                 sdt = hd.sdt,
                                 Email = hd.AspNetUser.Email,
                                 TenTT = hd.tinhtrang.tentt,
                                 NgayDat = hd.ngaydat,
                                 TongTien = hd.tongtien,
                                 SanPham = (from cthd in db.cthoadons
                                            join hh in db.hanghoas on cthd.mahh equals hh.mahh
                                            where cthd.masohd == hd.masohd
                                            select new SanPhamViewModel
                                            {
                                                TenHH = hh.tenhh,
                                                MauSac = cthd.mausac,
                                                Size = cthd.size,
                                                SoLuongMua = cthd.soluongmua
                                            }).ToList()
                             }).ToList();
            return View(lstHoaDon);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.hoadons.Where(a => a.masohd == id).ToList();

            ViewData["TinhTrang"] = db.tinhtrangs.ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        [ValidateInput(false)]
        public ActionResult EditAction(int masohd, int matt)
        {
            // Logic để xử lý hành động chỉnh sửa, ví dụ:
            // - Tìm sản phẩm theo ID
            // - Cập nhật tên sản phẩm với giá trị mới từ 'tenhh'
            // - Lưu thay đổi vào cơ sở dữ liệu


            var product = db.hoadons.Find(masohd);
            if (product != null)
            {
                product.matt = matt;
               


                db.SaveChanges();
            }

            // Trả về một view hoặc chuyển hướng đến một action khác
            return RedirectToAction("HoaDon", "DonHang", new { area = "Admin", editedProductId = matt });
        }



    }
}
