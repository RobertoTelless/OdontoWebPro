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

namespace SMS_Presentation.Controllers
{
    public class DenteRegiaoController : Controller
    {
        private readonly IDenteRegiaoAppService denApp;

        private String msg;
        private Exception exception;
        REGIAO_DENTE objeto = new REGIAO_DENTE();
        REGIAO_DENTE objetoAntes = new REGIAO_DENTE();
        List<REGIAO_DENTE> listaMaster = new List<REGIAO_DENTE>();
        String extensao;

        public DenteRegiaoController(IDenteRegiaoAppService denApps)
        {
            denApp = denApps;
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

        [HttpGet]
        public ActionResult MontarTelaDenteRegiao()
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
                    Session["MensDente"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            if ((List<REGIAO_DENTE>)Session["ListaDente"] == null)
            {
                listaMaster= denApp.GetAllItens(idAss);
                Session["ListaDente"] = listaMaster;
            }
            ViewBag.Listas = (List<REGIAO_DENTE>)Session["ListaDente"];
            ViewBag.Title = "Dentes & Regiões";

            // Indicadores
            ViewBag.Dentes = ((List<REGIAO_DENTE>)Session["ListaDente"]).Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensDente"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensDente"] == 3)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0043", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensDente"] == 4)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0044", CultureInfo.CurrentCulture));
            }

            // Abre view
            objeto = new REGIAO_DENTE();
            Session["VoltaDente"] = 1;
            Session["MensDente"] = 0;
            return View(objeto);
        }

        public ActionResult MostrarTudoDente()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = denApp.GetAllItensAdm(idAss);
            Session["ListaDente"] = listaMaster;
            return RedirectToAction("MontarTelaDenteRegiao");
        }

        public ActionResult VoltarBaseDente()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaDente"] = denApp.GetAllItens(idAss);
            return RedirectToAction("MontarTelaDenteRegiao");
        }

        [HttpGet]
        public ActionResult IncluirDente(Int32? id)
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
                    Session["MensDente"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            REGIAO_DENTE item = new REGIAO_DENTE();
            if (id != null)
            {
                Session["VoltaDente"] = id.Value;
            }
            DenteRegiaoViewModel vm = Mapper.Map<REGIAO_DENTE, DenteRegiaoViewModel>(item);
            vm.REDE_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirDente(DenteRegiaoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    REGIAO_DENTE item = Mapper.Map<DenteRegiaoViewModel, REGIAO_DENTE>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    Int32 volta = denApp.ValidateCreate(item, usuarioLogado);

                    // Retorno
                    if (volta == 1)
                    {
                        Session["MensDente"] = 4;
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0044", CultureInfo.CurrentCulture));
                        return RedirectToAction("MontarTelaDenteRegiao");
                    }

                    // Sucesso
                    listaMaster = new List<REGIAO_DENTE>();
                    Session["ListaDente"] = null;
                    return RedirectToAction("EditarDente");
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
        public ActionResult EditarDente(Int32 id)
        {
            REGIAO_DENTE item = denApp.GetItemById(id);
            objetoAntes = item;
            Session["Dente"] = item;
            Session["idDente"] = id;
            DenteRegiaoViewModel vm = Mapper.Map<REGIAO_DENTE, DenteRegiaoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarDente(DenteRegiaoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    REGIAO_DENTE item = Mapper.Map<DenteRegiaoViewModel, REGIAO_DENTE>(vm);
                    Int32 volta = denApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<REGIAO_DENTE>();
                    Session["ListaDente"] = null;
                    return RedirectToAction("MontarTelaDenteRegião");
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
        public ActionResult ExcluirDente(Int32 id)
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
                    Session["MensDente"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            REGIAO_DENTE item = denApp.GetItemById(id);
            objetoAntes = item;
            item.REDE_IN_ATIVO = 0;
            Int32 volta = denApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensDente"] = 3;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0043", CultureInfo.CurrentCulture));
            }
            listaMaster = new List<REGIAO_DENTE>();
            Session["ListaDente"] = null;
            return RedirectToAction("MontarTelaDenteRegiao");
        }

        [HttpGet]
        public ActionResult ReativarDente(Int32 id)
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
                    Session["MensDente"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            REGIAO_DENTE item = denApp.GetItemById(id);
            objetoAntes = item;
            item.REDE_IN_ATIVO = 1;
            Int32 volta = denApp.ValidateReativar(item, usuario);
            listaMaster = new List<REGIAO_DENTE>();
            Session["ListaDente"] = null;
            return RedirectToAction("MontarTelaDenteRegiao");
        }

    }
}