using dotMVC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dotMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = ("Admin"))]
    public class AccountController : Controller
    {
        private ShopGNam3Context db = new ShopGNam3Context();

        // GET: Admin/Account
        public ActionResult Index()
        {
            return View();
        }
        [Route("Account")]
        public ActionResult Account()
        {
            var lstAspNetUsers = db.AspNetUsers.ToList();
            return View(lstAspNetUsers);
        }
        //them account
        
       


    }


}