using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using dotMVC.Data;
using dotMVC.Models;

namespace dotMVC.Areas.Admin.Controllers
{
    public class TkKhachHangController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();

        // GET: Admin/TkKhachHang
        public ActionResult Index(string filterType, string filterValue)
        {
            var query = db.hoadons.AsQueryable();

            if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(filterValue))
            {
                DateTime filterDate;
                int filterYear;

                switch (filterType)
                {
                    case "day":
                        if (DateTime.TryParse(filterValue, out filterDate))
                        {
                            query = query.Where(hd => hd.ngaydat == filterDate);
                        }
                        break;
                    case "month":
                        if (DateTime.TryParse(filterValue + "-01", out filterDate))
                        {
                            query = query.Where(hd => hd.ngaydat.Year == filterDate.Year && hd.ngaydat.Month == filterDate.Month);
                        }
                        break;
                    case "year":
                        if (int.TryParse(filterValue, out filterYear))
                        {
                            query = query.Where(hd => hd.ngaydat.Year == filterYear);
                        }
                        break;
                }
            }

            var data = query
                .Join(db.cthoadons, hd => hd.masohd, cthd => cthd.masohd, (hd, cthd) => new { hd, cthd })
                .GroupBy(x => x.hd.makh)
                .Select(g => new UserPurchaseStatisticsViewModel
                {
                    Email = db.AspNetUsers.FirstOrDefault(u => u.Id == g.Key).Email,
                    TotalQuantity = g.Sum(x => x.cthd.soluongmua)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10) // Giới hạn top 10 người dùng, ví dụ
                .ToList();

            return View(data);
        }

        public ActionResult TkHangHoa(string filterType, string filterValue)
        {
            var query = db.cthoadons.AsQueryable();

            if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(filterValue))
            {
                switch (filterType)
                {
                    case "day":
                        if (DateTime.TryParse(filterValue, out DateTime filterDate))
                        {
                            query = query.Where(cthd => cthd.hoadon.ngaydat == filterDate);
                        }
                        break;
                    case "month":
                        if (DateTime.TryParseExact(filterValue, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime filterMonth))
                        {
                            int year = filterMonth.Year;
                            int month = filterMonth.Month;
                            query = query.Where(cthd => cthd.hoadon.ngaydat.Year == year && cthd.hoadon.ngaydat.Month == month);
                        }
                        break;
                    case "year":
                        if (int.TryParse(filterValue, out int filterYear))
                        {
                            query = query.Where(cthd => cthd.hoadon.ngaydat.Year == filterYear);
                        }
                        break;
                }
            }

            var data = query
                .GroupBy(cthd => cthd.mahh)
                .Select(g => new HangHoaStatisticsViewModel
                {
                    TenHH = db.hanghoas.FirstOrDefault(hh => hh.mahh == g.Key).tenhh,
                    TotalQuantity = g.Sum(cthd => cthd.soluongmua)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToList();

            return View(data);
        }

    }
}

