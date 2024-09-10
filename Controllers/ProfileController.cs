using dotMVC.Data;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.IO;
using dotMVC.Models;
using System.Linq; // Thêm dòng này
using System.Web;
using System.Web.Mvc;

namespace dotMVC.Controllers
{
    public class ProfileController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();

        [Authorize]
        public ActionResult Profile()
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userProfile = db.AspNetUsers.FirstOrDefault(u => u.Id == userId);

            if (userProfile == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserImage = userProfile.Image; // Thiết lập ViewBag.UserImage

            return View(userProfile);
        }

        [Authorize]
        public ActionResult EditProfile()
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userProfile = db.AspNetUsers.FirstOrDefault(u => u.Id == userId);

            if (userProfile == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserImage = userProfile.Image; // Thiết lập ViewBag.UserImage

            return View(userProfile);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(AspNetUser updatedUser, HttpPostedFileBase Image)
        {
            if (!ModelState.IsValid)
            {
                // Xử lý khi ModelState không hợp lệ
                return View(updatedUser);
            }

            var userId = User.Identity.GetUserId();
            var user = db.AspNetUsers.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Cập nhật thông tin từ updatedUser vào user
            user.UserName = updatedUser.UserName;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.Address = updatedUser.Address;

            // Xử lý ảnh người dùng
            if (Image != null && Image.ContentLength > 0)
            {
                var fileName = Path.GetFileName(Image.FileName);
                var filePath = Path.Combine(Server.MapPath("~/assets/img/"), fileName);

                try
                {
                    Image.SaveAs(filePath);
                    user.Image = "~/assets/img/" + fileName; // Lưu đường dẫn ảnh vào user.Image
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save image file. " + ex.Message);
                    return View(updatedUser);
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator. " + ex.Message);
                return View(updatedUser);
            }

            return RedirectToAction("Profile", "Profile");
        }


        [Authorize]
        public ActionResult DonHang()
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var donhang = db.hoadons.Where(i => i.makh == userId).OrderByDescending(i => i.ngaydat).ToList();

            var tinhtrangList = db.tinhtrangs.ToList();

            ViewBag.TinhTrangList = tinhtrangList;

            return View(donhang);
        }


        [Authorize]
        public ActionResult ChiTiet(int id)
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find the specific order by id
            var ctdon = db.hoadons
                         .Where(hd => hd.masohd == id)
                         .Select(hd => new HoaDonViewModel
                         {
                             MaSoHD = hd.masohd,
                             diachi = hd.diachi,
                             sdt = hd.sdt,
                             Email = hd.AspNetUser.Email,
                             TenTT = hd.tinhtrang.tentt,
                             NgayDat = hd.ngaydat,
                             TongTien = hd.tongtien,
                             SanPham = db.cthoadons
                                         .Where(cthd => cthd.masohd == id)
                                         .Join(db.hanghoas,
                                               cthd => cthd.mahh,
                                               hh => hh.mahh,
                                               (cthd, hh) => new SanPhamViewModel
                                               {
                                                   TenHH = hh.tenhh,
                                                   MauSac = cthd.mausac,
                                                   Size = cthd.size,
                                                   SoLuongMua = cthd.soluongmua
                                               }).ToList()
                         }).FirstOrDefault(); // Use FirstOrDefault to return a single order

            if (ctdon == null)
            {
                return HttpNotFound();
            }

            return View(ctdon);
        }
    }
}
