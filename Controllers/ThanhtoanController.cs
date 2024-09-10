using dotMVC.Data;
using dotMVC.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dotMVC.Controllers
{
    public class ThanhtoanController : Controller
    {
        // GET: Thanhtoan
        private ShopGNam3Context db = new ShopGNam3Context();




        [HttpPost]
        public ActionResult ThanhToanAction(string hoten, string sodienthoai, string diachi, string ghichu, int idsp, int Idmau, int Idsize, string mau, int size, int soluong, int tongtien)
        {
            int idhoadon = db.hoadons.Count() + 1;
            var hoaDon = new hoadon
            {
                masohd = idhoadon,
                makh = hoten,
                ngaydat = DateTime.Now,
                tongtien = tongtien,
                mavc = null,
                matt = 3,
            };
            db.hoadons.Add(hoaDon);

            var cthoadon = new cthoadon
            {
                masohd = idhoadon,
                mahh = idsp,
                soluongmua = soluong,
                mausac = mau,
                size = size,
                thanhtien = tongtien,
            };
            db.cthoadons.Add(cthoadon);

            // Lấy số lượng tồn đầu tiên từ truy vấn
            var slton = db.cthanghoas.Where(m => m.idhanghoa == idsp && m.idmau == Idmau && m.idsize == Idsize).Select(m => m.soluongton).FirstOrDefault();

            // Thực hiện phép trừ - giả sử slton không phải null
            if (slton != null)
            {
                var slgiam = slton - soluong;
                var slcthanghoa = db.cthanghoas.Where(m => m.idhanghoa == idsp && m.idmau == Idmau && m.idsize == Idsize).FirstOrDefault();

                if (slcthanghoa != null)
                {
                    slcthanghoa.soluongton = slgiam;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ThanhtoanSL()
        {
            var userId = User.Identity.GetUserId();
            ViewBag.userId = userId;
            ViewBag.name = User.Identity.GetUserName();

            // Fetch user information from AspNetUsers table
            var userInfo = db.AspNetUsers.Where(i => i.Id == userId).FirstOrDefault();
            if (userInfo != null)
            {
                ViewBag.PhoneNumber = userInfo.PhoneNumber;
                ViewBag.Address = userInfo.Address; // Ensure Address exists in the AspNetUsers table
            }
            return View();
        }

        [HttpPost]
        public ActionResult ThanhToanSLAction(string hoten, string sodienthoai, string diachi, string ghichu, int total)
        {
            // Lấy danh sách sản phẩm từ session Giohang
            var gh = Session["Giohang"] as List<dotMVC.Models.GioHang>;

            // Tạo hóa đơn cho từng sản phẩm
            var hoaDon = new hoadon
            {
                masohd = db.hoadons.Count() + 1,
                makh = hoten, // Lấy thông tin từ sản phẩm trong giỏ hàng
                ngaydat = DateTime.Now,
                tongtien = total, // Lấy thông tin từ sản phẩm trong giỏ hàng
                diachi = diachi,
                sdt = sodienthoai,
                mavc = null,
                matt = 3,
            };
            db.hoadons.Add(hoaDon);

            foreach (var i in gh)
            {
                // Tạo chi tiết hóa đơn cho từng sản phẩm
                var cthoadon = new cthoadon
                {
                    masohd = hoaDon.masohd,
                    mahh = i.mahh, // Lấy thông tin từ sản phẩm trong giỏ hàng
                    soluongmua = i.soluong, // Lấy thông tin từ sản phẩm trong giỏ hàng
                    mausac = i.mausac, // Lấy thông tin từ sản phẩm trong giỏ hàng
                    size = i.size, // Lấy thông tin từ sản phẩm trong giỏ hàng
                    thanhtien = (int)i.tongtien, // Lấy thông tin từ sản phẩm trong giỏ hàng
                };
                db.cthoadons.Add(cthoadon);
            }

            // Xóa giỏ hàng sau khi thanh toán thành công
            Session["Giohang"] = null;

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();

            return View();
        }

        [HttpPost]
        public ActionResult VnpayPaymentAction(string hoten, string sodienthoai, string diachi, decimal tongtien)
        {
            try
            {
                int idhoadon = db.hoadons.Count() + 1;
                var paymentInfo = new PaymentInfo
                {
                    IdHD = idhoadon,
                    Hoten = hoten,
                    Sodienthoai = sodienthoai,
                    Diachi = diachi,
                    Tongtien = tongtien
                };

                // Store the instance in session
                Session["PaymentInfo"] = paymentInfo;

                string transactionRef = DateTime.Now.Ticks.ToString();

                string vnpHashSecret = "ETFH3Y1NZKWO5F53B6KAWDLM07RNKT6H";
                var pay = new VnPayLibrary();
                pay.AddRequestData("vnp_Version", "2.1.0");
                pay.AddRequestData("vnp_Command", "pay");
                pay.AddRequestData("vnp_TmnCode", "H7CHX8V5");
                pay.AddRequestData("vnp_Amount", (tongtien * 100).ToString("0")); // Amount in VND cents
                pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_CurrCode", "VND");
                pay.AddRequestData("vnp_Locale", "vn");
                pay.AddRequestData("vnp_IpAddr", Request.UserHostAddress); // Client IP address
                pay.AddRequestData("vnp_OrderInfo", "Payment for order #" + transactionRef);
                pay.AddRequestData("vnp_OrderType", "other");
                pay.AddRequestData("vnp_ReturnUrl", Url.Action("PaymentResult", "ThanhToan", null, Request.Url.Scheme));
                pay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(5).ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_TxnRef", transactionRef);

                string paymentUrl = pay.CreateRequestUrl("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", vnpHashSecret);

                return Json(new { paymentUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                return new HttpStatusCodeResult(500, "Internal Server Error: " + ex.Message);
            }
        }



        public ActionResult PaymentResult()
        {
            var pay = new VnPayLibrary();
            foreach (string key in Request.QueryString.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    pay.AddRequestData(key, Request.QueryString[key]);
                }
            }

            string vnpHashSecret = "ETFH3Y1NZKWO5F53B6KAWDLM07RNKT6H";
            string vnpSecureHash = Request.QueryString["vnp_SecureHash"];
            bool isValid = pay.ValidateSignature(vnpSecureHash, vnpHashSecret);

            var paymentInfo = Session["PaymentInfo"] as PaymentInfo;
            var gh = Session["Giohang"] as List<dotMVC.Models.GioHang>;

            if (paymentInfo == null || gh == null)
            {
                // Handle missing session data
                return new HttpStatusCodeResult(400, "Invalid session data.");
            }

            if (isValid)
            {
                 return View();
            }
            else
            {
                var hoaDon = new hoadon
                {
                    masohd = paymentInfo.IdHD,
                    makh = paymentInfo.Hoten, // Lấy thông tin từ sản phẩm trong giỏ hàng
                    ngaydat = DateTime.Now,
                    tongtien = (int)paymentInfo.Tongtien, // Lấy thông tin từ sản phẩm trong giỏ hàng
                    diachi = paymentInfo.Diachi,
                    sdt = paymentInfo.Sodienthoai,
                    mavc = null,
                    matt = 5, // Payment status
                };
                db.hoadons.Add(hoaDon);

                foreach (var i in gh)
                {
                    // Tạo chi tiết hóa đơn cho từng sản phẩm
                    var cthoadon = new cthoadon
                    {
                        masohd = hoaDon.masohd,
                        mahh = i.mahh, // Lấy thông tin từ sản phẩm trong giỏ hàng
                        soluongmua = i.soluong, // Lấy thông tin từ sản phẩm trong giỏ hàng
                        mausac = i.mausac, // Lấy thông tin từ sản phẩm trong giỏ hàng
                        size = i.size, // Lấy thông tin từ sản phẩm trong giỏ hàng
                        thanhtien = (int)i.tongtien, // Lấy thông tin từ sản phẩm trong giỏ hàng
                    };
                    db.cthoadons.Add(cthoadon);
                }

                // Save changes to the database
                db.SaveChanges();

                // Clear the session
                Session["PaymentInfo"] = null;
                Session["Giohang"] = null;

                // Return a view or success message
                return View();
            }
        }



    }
}