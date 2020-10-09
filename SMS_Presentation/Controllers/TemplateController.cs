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

namespace OdontoWeb.Controllers
{
    public class TemplateController : Controller
    {
        private readonly ITemplateAppService baseApp;
        private readonly IUsuarioAppService usuApp;

        private String msg;
        private Exception exception;
        TEMPLATE objeto = new TEMPLATE();
        TEMPLATE objetoAntes = new TEMPLATE();
        List<TEMPLATE> listaMaster = new List<TEMPLATE>();
        String extensao;

        public TemplateController(ITemplateAppService baseApps, IUsuarioAppService usuApps)
        {
            baseApp = baseApps;
            usuApp = usuApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Voltar()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarGeral()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult DashboardAdministracao()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaTemplate()
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
                    Session["MensTemplate"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            if ((List<TEMPLATE>)Session["ListaTemplate"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaTemplate"] = listaMaster;
            }
            ViewBag.Listas = (List<TEMPLATE>)Session["ListaTemplate"];
            ViewBag.Title = "Templates";

            // Indicadores
            ViewBag.Templates = ((List<TEMPLATE>)Session["ListaTemplate"]).Count;

            // Mensagem
            if ((Int32)Session["MensTemplate"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensTemplate"] = 0;
            objeto = new TEMPLATE();
            return View(objeto);
        }

        public ActionResult RetirarFiltroTemplate()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaTemplate"] = null;
            return RedirectToAction("MontarTelaTemplate");
        }

        public ActionResult MostrarTudoTemplate()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["ListaTemplate"] = listaMaster;
            return RedirectToAction("MontarTelaTemplate");
        }

        [HttpPost]
        public ActionResult FiltrarTemplate(TEMPLATE item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                List<TEMPLATE> listaObj = new List<TEMPLATE>();
                Int32 idAss = (Int32)Session["IdAssinante"];
                Int32 volta = baseApp.ExecuteFilter(item.TEMP_NM_NOME, item.TEMP_TX_CORPO, item.TEMP_SG_SIGLA, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensTemplate"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaTemplate");
                }

                // Sucesso
                Session["MensTemplate"] = 0;
                listaMaster = listaObj;
                Session["ListaTemplate"] = listaObj;
                return RedirectToAction("MontarTelaTemplate");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaTemplate");
            }
        }

        public ActionResult VoltarBaseTemplate()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaTemplate");
        }

        [HttpGet]
        public ActionResult IncluirTemplate()
        {
            // Prepara listas
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            TEMPLATE item = new TEMPLATE();
            TemplateViewModel vm = Mapper.Map<TEMPLATE, TemplateViewModel>(item);
            vm.TEMP_IN_ATIVO = 1;
            vm.ASSI_CD_ID = idAss;
            vm.TEMP_DT_CRIACAO = DateTime.Today.Date;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirTemplate(TemplateViewModel vm)
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
                    TEMPLATE item = Mapper.Map<TemplateViewModel, TEMPLATE>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensTemplate"] = 1;
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0026", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMaster= new List<TEMPLATE>();
                    Session["ListaTemplate"] = null;
                    Session["VoltaTemplate"] = 1;
                    Session["IdAssinanteVolta"] = item.ASSI_CD_ID;
                    Session["Template"] = item;
                    Session["MensTemplate"] = 0;
                    Session["IdVolta"] = item.TEMP_CD_ID;
                    return RedirectToAction("MontarTelaTemplate");
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
        public ActionResult EditarTemplate(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            TEMPLATE item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Template"] = item;
            Session["IdVolta"] = id;
            Session["MensTemplate"] = 0;
            TemplateViewModel vm = Mapper.Map<TEMPLATE, TemplateViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarTemplate(TemplateViewModel vm)
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
                    TEMPLATE item = Mapper.Map<TemplateViewModel, TEMPLATE>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Sucesso
                    listaMaster = new List<TEMPLATE>();
                    Session["ListaTemplate"] = null;
                    Session["MensTemplate"] = 0;
                    return RedirectToAction("MontarTelaTemplate");
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
        public ActionResult ExcluirTemplate(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            TEMPLATE item = baseApp.GetItemById(id);
            TemplateViewModel vm = Mapper.Map<TEMPLATE, TemplateViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirTemplate(TemplateViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                TEMPLATE item = Mapper.Map<TemplateViewModel, TEMPLATE>(vm);
                Int32 volta = baseApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                
                // Sucesso
                listaMaster = new List<TEMPLATE>();
                Session["ListaTemplate"] = null;
                Session["MensTemplate"] = 0;
                return RedirectToAction("MontarTelaTemplate");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarTemplate(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            TEMPLATE item = baseApp.GetItemById(id);
            TemplateViewModel vm = Mapper.Map<TEMPLATE, TemplateViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarTemplate(TemplateViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                TEMPLATE item = Mapper.Map<TemplateViewModel, TEMPLATE>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuarioLogado);

                // Sucesso
                listaMaster = new List<TEMPLATE>();
                Session["ListaTemplate"] = null;
                Session["MensTemplate"] = 0;
                return RedirectToAction("MontarTelaTemplate");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult VoltarAnexoTemplate()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 volta = (Int32)Session["IdVolta"];
            return RedirectToAction("EditarTemplate", new { id = volta });
        }
    }
}