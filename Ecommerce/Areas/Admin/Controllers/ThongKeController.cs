using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using dotMVC.Data;
using dotMVC.Models;
using dotMVC.Areas.Admin.Model;
using static dotMVC.Areas.Admin.Model.ThongkeViewModel;

namespace dotMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DonHang()
        {
            // Lấy ngày đầu tiên của tuần hiện tại
            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

            // Tạo danh sách 7 ngày trong tuần
            var dateList = Enumerable.Range(0, 7).Select(i => startDate.AddDays(i));

            // Lấy dữ liệu từ cơ sở dữ liệu cho mỗi ngày trong tuần với điều kiện matt = 1
            var donHangList = dateList.Select(date =>
            {
                var totalTongTien = db.hoadons
                    .Where(hd => System.Data.Entity.DbFunctions.DiffDays(hd.ngaydat, date) == 0)
                    .Where(hd => hd.matt == 1)
                    .Sum(hd => (decimal?)hd.tongtien) ?? 0;
                return new { Ngay = date.ToString("ddd"), TongTien = totalTongTien };
            }).ToList();

            // Truyền dữ liệu sang view
            ViewBag.DonHangData = donHangList;

            int totalDoanhThuThang = TinhTongTienDoanhThuThang();
            ViewBag.totalDoanhThuThang = totalDoanhThuThang;
            int totalDaGiao = CountTotalDaGiaos();
            ViewBag.totalDaGiao = totalDaGiao;


            int totalHuy = CountTotalHuys();
            ViewBag.totalHuy = totalHuy;

            int totalCxyly = CountTotalCxulys();
            ViewBag.totalCxyly = totalCxyly;

            var DonHangThangData = db.hoadons.Where(h => h.ngaydat.Month == DateTime.Now.Month)
                                    .Select(h => new { Thang = h.ngaydat.Day, TongTien = h.tongtien })
                                    .ToList();

            // Pass the revenue data to ViewBag
            ViewBag.DonHangThangData = DonHangThangData;


            // Tạo danh sách 12 tháng trong năm
            var monthList = Enumerable.Range(1, 12);

            // Lấy dữ liệu từ cơ sở dữ liệu cho mỗi tháng trong năm với điều kiện matt = 1
            var donHangNamList = monthList.Select(month =>
            {
                var totalTongTien = db.hoadons
                   .Where(hd => hd.ngaydat.Month == month && hd.matt == 1)
                   .Sum(hd => (decimal?)hd.tongtien) ?? 0;

                var result = new { Thang = month, TongTien = totalTongTien };
                return result;
            }).ToList();

            // Truyền dữ liệu sang view
            ViewBag.donHangNamList = donHangNamList;
            return View();



        }

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


        public ActionResult ThongKeLoaiGiay()
        {
            try
            {
                var thongKeLoaiGiay = db.hanghoas
                                        .GroupBy(hh => hh.loai.tenloai)
                                        .Select(group => new ThongKeLoaiViewModel
                                        {
                                            TenLoaiGiay = group.Key,
                                            SoLuongSanPham = group.Count(),
                                            SanPhamTheoLoai = group.Select(hh => new ThongkeViewModel.SanPhamViewModel
                                            {
                                                mahh = hh.mahh,
                                                tenhh = hh.tenhh
                                            }).ToList()
                                        })
                                        .ToList();

                var viewModel = new ThongKeSanPhamViewModel
                {
                    ThongKeLoaiGiay = thongKeLoaiGiay
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi lấy dữ liệu thống kê loại giày.";
                ViewBag.ErrorDetails = ex.Message;
                return View("Error");
            }
        }
        // số lượt xemm
        public ActionResult SoLuotXem()
        {
            var hanghoaData = db.hanghoas
                .Select(hh => new
                {
                    hh.tenhh,
                    hh.soluotxem
                }).ToList();

            ViewBag.HangHoaData = hanghoaData;
            return View();
        }
    }
}
