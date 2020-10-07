using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using EntitiesServices.Work_Classes;
using AutoMapper;
using OdontoWeb.ViewModels;
using System.IO;

namespace SMS_Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUsuarioAppService baseApp;
        private readonly INotificacaoAppService notiApp;
        private readonly ILogAppService logApp;
        private String msg;
        private Exception exception;
        USUARIO objeto = new USUARIO();
        USUARIO objetoAntes = new USUARIO();
        List<USUARIO> listaMaster = new List<USUARIO>();

        public HomeController(IUsuarioAppService baseApps, ILogAppService logApps, INotificacaoAppService notApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            notiApp = notApps;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Login", "ControleAcesso");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}