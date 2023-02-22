using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Shop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Pages", "{page}", new { Controller = "Pages", action = "Index" }, new[] { "Shop.Controllers" });

            routes.MapRoute("Default", "", new { Controller = "Pages", action = "Index" }, new[] { "Shop.Controllers" });

            routes.MapRoute("PagesMenuPartial", "Pages/PagesMenuPartial", new { Controller = "Pages", action = "PagesMenuPartial" }, new[] { "Shop.Controllers" });

            routes.MapRoute("SidebarPartial", "Pages/SidebarPartial", new { Controller = "Pages", action = "SidebarPartial" }, new[] { "Shop.Controllers" });

            routes.MapRoute("Shop", "Shop/{action}/{name}", new { Controller = "Shop", action = "Index" , name =UrlParameter.Optional}, new[] { "Shop.Controllers" });

            routes.MapRoute("Cart", "Cart/{action}/{id}", new { Controller = "Cart", action = "CartPartial", id = UrlParameter.Optional }, new[] { "Shop.Controllers" });

            routes.MapRoute("Account", "Account/{action}/{id}", new { Controller = "Account", action = "Index", id = UrlParameter.Optional }, new[] { "Shop.Controllers" });

            /* routes.MapRoute(
                  name: "Default",
                 url: "{controller}/{action}/{id}",
                 defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
             );*/
        }
    }
}
