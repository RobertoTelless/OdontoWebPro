using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
using AutoMapper;
using OdontoWeb.ViewModels;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Cms;

namespace SMS_Presentation.Controllers
{
    public class CargoController : Controller
    {
        private readonly ICargoAppService baseApp;
        private readonly ILogAppService logApp;

        private String msg;
        private Exception exception;
        List<CARGO> listaMaster= new List<CARGO>();
        CARGO objeto = new CARGO();
        CARGO objetoAntes = new CARGO();
        List<CARGO> CARGO = new List<CARGO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public CargoController(ICargoAppService baseApps, ILogAppService logApps)
        {
            baseApp = baseApps;
            logApp = logApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return View();
        }

        public ActionResult Voltar()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaCargo"] = baseApp.GetAllItens(idAss);
            return RedirectToAction("MontarTelaCargo");
        }

        [HttpGet]
        public ActionResult MontarTelaCargo()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCargo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List< CARGO>)Session["ListaCargo"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaCargo"] = listaMaster;
            }
            ViewBag.Listas = (List<CARGO>)Session["ListaCargo"];
            ViewBag.Title = "Cargos";

            // Indicadores
            ViewBag.Cargos = ((List<CARGO>)Session["ListaCargo"]).Count;

            // Mensagem
            if ((Int32)Session["MensCargo"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCargo"] == 3)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0031", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensCargo"] = 0;
            objeto = new CARGO();
            return View(objeto);
        }

        public ActionResult MostrarTudo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["ListaCargo"] = listaMaster;
            return RedirectToAction("MontarTelaCargo");
        }

        [HttpGet]
        public ActionResult IncluirCargo()
        {
            // Prepara listas
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCargo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            CARGO item = new CARGO();
            CargoViewModel vm = Mapper.Map<CARGO, CargoViewModel>(item);
            vm.CARG_IN_ATIVO = 1;
            vm.ASSI_CD_ID = idAss;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirCargo(CargoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CARGO item = Mapper.Map<CargoViewModel, CARGO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensCargo"] = 1;
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0030", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMaster = new List<CARGO>();
                    Session["ListaCargo"] = null;
                    Session["VoltaCargo"] = 1;
                    Session["IdAssinanteVolta"] = item.ASSI_CD_ID;
                    Session["Cargo"] = item;
                    Session["MensCargo"] = 0;
                    Session["IdVolta"] = item.CARG_CD_ID;
                    return RedirectToAction("MontarTelaCargo");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult EditarCargo(Int32 id)
        {
            // Prepara view
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCargo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CARGO item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Cargo"] = item;
            Session["IdVolta"] = id;
            Session["MensCargo"] = 0;
            CargoViewModel vm = Mapper.Map<CARGO, CargoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarCargo(CargoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CARGO item = Mapper.Map<CargoViewModel, CARGO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, (CARGO)Session["Cargo"], usuarioLogado);

                    // Sucesso
                    listaMaster = new List<CARGO>();
                    Session["ListaCargo"] = null;
                    Session["MensCargo"] = 0;
                    return RedirectToAction("MontarTelaCargo");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ExcluirCargo(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCargo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            CARGO item = baseApp.GetItemById(id);
            objetoAntes = item;
            item.CARG_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensCargo"] = 3;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0031", CultureInfo.CurrentCulture));
            }
            listaMaster = new List<CARGO>();
            Session["ListaCargo"] = null;
            return RedirectToAction("MontarTelaCargo");
        }

        [HttpGet]
        public ActionResult ReativarCargo(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCargo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            CARGO item = baseApp.GetItemById(id);
            objetoAntes = item;
            item.CARG_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            listaMaster = new List<CARGO>();
            Session["ListaCargo"] = null;
            return RedirectToAction("MontarTelaCargo");
        }
    }
}