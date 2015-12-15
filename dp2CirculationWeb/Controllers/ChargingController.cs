using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2CirculationWeb.Controllers
{
    public class ChargingController : Controller
    {
        //
        // GET: /Charging/

        public ActionResult Main()
        {
            /*
            string accountName = "";
            if (Session["AcountName"] != null)
                accountName = (string)Session["AcountName"];

            if (accountName == "")
            {
                return this.RedirectToAction("Login", "Account");
            }
            */
            return View();
        }

    }
}
