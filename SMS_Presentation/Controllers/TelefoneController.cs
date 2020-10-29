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

namespace Odonto.Controllers
{
    public class TelefoneController : Controller
    {
        private readonly ITelefoneAppService baseApp;
        private readonly ILogAppService logApp;
        private String msg;
        private Exception exception;
        TELEFONE objeto = new TELEFONE();
        TELEFONE objetoAntes = new TELEFONE();
        List<TELEFONE> listaMaster = new List<TELEFONE>();
        String extensao;

        public TelefoneController(ITelefoneAppService baseApps, ILogAppService logApps)
        {
            baseApp = baseApps;
            logApp = logApps;
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

        public ActionResult VoltarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((Int32)Session["VoltaTelefone"] == 2)
            {
                return RedirectToAction("MontarTelaTelefoneContato");
            }
            return RedirectToAction("MontarTelaTelefone");
        }

        [HttpGet]
        public ActionResult MontarTelaTelefone()
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
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List<TELEFONE>)Session["ListaTelefone"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaTelefone"] = listaMaster;
                Session["FiltroTelefone"] = null;
            }
            ViewBag.Listas = (List<TELEFONE>)Session["ListaTelefone"];
            ViewBag.Title = "Telefones";
            ViewBag.Cats = new SelectList(baseApp.GetAllCategorias(idAss), "CATE_CD_ID", "CATE_NM_NOME");

            // Indicadores
            ViewBag.Tels = ((List<TELEFONE>)Session["ListaTelefone"]).Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensTelefone"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensTelefone"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensTelefone"] = 0;
            Session["VoltaTelefone"] = 1;
            objeto = new TELEFONE();
            return View(objeto);
        }

        [HttpGet]
        public ActionResult MontarTelaTelefoneContato()
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
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List<TELEFONE>)Session["ListaTelefone"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaTelefone"] = listaMaster;
                Session["FiltroTelefone"] = null;
            }
            ViewBag.Listas = (List<TELEFONE>)Session["ListaTelefone"];
            ViewBag.Title = "Telefones";
            ViewBag.Cats = new SelectList(baseApp.GetAllCategorias(idAss), "CATE_CD_ID", "CATE_NM_NOME");

            // Indicadores
            ViewBag.Tels = ((List<TELEFONE>)Session["ListaTelefone"]).Count;

            // Mensagem
            if ((Int32)Session["MensTelefone"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensTelefone"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensTelefone"] = 0;
            Session["VoltaTelefone"] = 2;
            objeto = new TELEFONE();
            return View(objeto);
        }

        public ActionResult RetirarFiltroTelefone()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaTelefone"] = null;
            listaMaster = new List<TELEFONE>();
            if ((Int32)Session["VoltaTelefone"] == 2)
            {
                return RedirectToAction("MontarTelaTelefoneContato");
            }
            return RedirectToAction("MontarTelaTelefone");
        }

        public ActionResult MostrarTudoTelefone()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["ListaTelefone"] = listaMaster;
            if ((Int32)Session["VoltaTelefone"] == 2)
            {
                return RedirectToAction("MontarTelaTelefoneContato");
            }
            return RedirectToAction("MontarTelaTelefone");
        }

        [HttpPost]
        public ActionResult FiltrarTelefone(TELEFONE item)
        {
            try
            {
                try
                {
                    // Executa a operação
                    if ((String)Session["Ativa"] == null)
                    {
                        return RedirectToAction("Login", "ControleAcesso");
                    }
                    Int32 idAss = (Int32)Session["IdAssinante"];

                    List<TELEFONE> listaObj = new List<TELEFONE>();
                    Int32 volta = baseApp.ExecuteFilter(item.CATE_CD_ID, item.TELE_NM_NOME, item.TELE_NR_TELEFONE, item.TELE_NR_CELULAR, idAss, out listaObj);
                    Session["FiltroTelefone"] = item;

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensTelefone"] = 1;
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    }

                    // Sucesso
                    Session["MensTelefone"] = 0;
                    listaMaster = listaObj;
                    Session["ListaTelefone"] = listaObj;
                    if ((Int32)Session["VoltaTelefone"] == 2)
                    {
                        return RedirectToAction("MontarTelaTelefoneContato");
                    }
                    return RedirectToAction("MontarTelaTelefone");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    if ((Int32)Session["VoltaTelefone"] == 2)
                    {
                        return RedirectToAction("MontarTelaTelefoneContato");
                    }
                    return RedirectToAction("MontarTelaTelefone");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                if ((Int32)Session["VoltaTelefone"] == 2)
                {
                    return RedirectToAction("MontarTelaTelefoneContato");
                }
                return RedirectToAction("MontarTelaTelefone");
            }
        }

        [HttpGet]
        public ActionResult IncluirTelefone()
        {
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
                    Session["MensTelefone"] = 2;
                    return RedirectToAction("MontarTelaTelefone", "Telefone");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Cats = new SelectList(baseApp.GetAllCategorias(idAss), "CATE_CD_ID", "CATE_NM_NOME");

            // Prepara view
            TELEFONE item = new TELEFONE();
            TelefoneViewModel vm = Mapper.Map<TELEFONE, TelefoneViewModel>(item);
            vm.ASSI_CD_ID = idAss;
            vm.TELE_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirTelefone(TelefoneViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Cats = new SelectList(baseApp.GetAllCategorias(idAss), "CATE_CD_ID", "CATE_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    TELEFONE item = Mapper.Map<TelefoneViewModel, TELEFONE>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<TELEFONE>();
                    Session["ListaTelefone"] = null;
                    Session["VoltaTelefone"] = 1;
                    Session["IdTelefoneVolta"] = item.TELE_CD_ID;
                    Session["Telefone"] = item;
                    Session["MensTelefone"] = 0;
                    if ((Int32)Session["VoltaTelefone"] == 2)
                    {
                        return RedirectToAction("MontarTelaTelefoneContato");
                    }
                    return RedirectToAction("MontarTelaTelefone");
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
        public ActionResult EditarTelefone(Int32 id)
        {
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
                    Session["MensTelefone"] = 2;
                    return RedirectToAction("MontarTelaTelefone", "Telefone");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ViewBag.Cats = new SelectList(baseApp.GetAllCategorias(idAss), "CATE_CD_ID", "CATE_NM_NOME");

            TELEFONE item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Telefone"] = item;
            Session["IdVolta"] = id;
            TelefoneViewModel vm = Mapper.Map<TELEFONE, TelefoneViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarTelefone(TelefoneViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Cats = new SelectList(baseApp.GetAllCategorias(idAss), "CATE_CD_ID", "CATE_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    TELEFONE item = Mapper.Map<TelefoneViewModel, TELEFONE>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<TELEFONE>();
                    Session["ListaTelefone"] = null;
                    Session["MensTelefone"] = 0;
                    if ((Int32)Session["VoltaTelefone"] == 2)
                    {
                        return RedirectToAction("MontarTelaTelefoneContato");
                    }
                    return RedirectToAction("MontarTelaTelefone");
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
        public ActionResult DesativarTelefone(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = new USUARIO();
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];
                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensTelefone"] = 2;
                    return RedirectToAction("MontarTelaTelefone", "Telefone");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            TELEFONE item = baseApp.GetItemById(id);
            objetoAntes = (TELEFONE)Session["Telefone"];
            item.TELE_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            listaMaster = new List<TELEFONE>();
            Session["ListaTelefone"] = null;
            if ((Int32)Session["VoltaTelefone"] == 2)
            {
                return RedirectToAction("MontarTelaTelefoneContato");
            }
            return RedirectToAction("MontarTelaTelefone");
        }

        [HttpGet]
        public ActionResult ReativarTelefone(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = new USUARIO();
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];
                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensTelefone"] = 2;
                    return RedirectToAction("MontarTelaTelefone", "Telefone");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            TELEFONE item = baseApp.GetItemById(id);
            objetoAntes = (TELEFONE)Session["Telefone"];
            item.TELE_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            listaMaster = new List<TELEFONE>();
            Session["ListaTelefone"] = null;
            if ((Int32)Session["VoltaTelefone"] == 2)
            {
                return RedirectToAction("MontarTelaTelefoneContato");
            }
            return RedirectToAction("MontarTelaTelefone");
        }

        [HttpGet]
        public ActionResult VerTelefone(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = new USUARIO();
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            TELEFONE item = baseApp.GetItemById(id);
            objetoAntes = item;
            TelefoneViewModel vm = Mapper.Map<TELEFONE, TelefoneViewModel>(item);
            return View(vm);
        }
    }
}