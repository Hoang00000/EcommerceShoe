using dotMVC.Models;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace dotMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Session["Giohang"] = null;
            Session["Yeuthich"] = null;


        }
    }
}
