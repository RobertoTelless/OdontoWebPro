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
    public class FilialController : Controller
    {
        private readonly IFilialAppService baseApp;
        private readonly ILogAppService logApp;
        private String msg;
        private Exception exception;
        FILIAL objeto = new FILIAL();
        FILIAL objetoAntes = new FILIAL();
        List<FILIAL> listaMaster = new List<FILIAL>();
        String extensao;

        public FilialController(IFilialAppService baseApps, ILogAppService logApps)
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
            return RedirectToAction("MontarTelaFilial");
        }

        [HttpGet]
        public ActionResult MontarTelaFilial()
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
            Int32 idMatriz = (Int32)Session["IdMatriz"];

            // Carrega listas
            if ((List<FILIAL>)Session["ListaFilial"] == null)
            {
                listaMaster = baseApp.GetAllItens(idMatriz);
                Session["ListaFilial"] = listaMaster;
                Session["FiltroFilial"] = null;
            }
            ViewBag.Listas = (List<FILIAL>)Session["ListaFilial"];
            ViewBag.Title = "Filiais";
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





    }
}