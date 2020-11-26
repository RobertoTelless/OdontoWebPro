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
    public class RemuneracaoController : Controller
    {
        private readonly IUsuarioAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly ICargoAppService carApp;
        private readonly IFilialAppService filApp;

        private String msg;
        private Exception exception;
        USUARIO objeto = new USUARIO();
        USUARIO objetoAntes = new USUARIO();
        List<USUARIO> listaMaster = new List<USUARIO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public RemuneracaoController(IUsuarioAppService baseApps, ILogAppService logApps, ICargoAppService carApps, IFilialAppService filApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            carApp = carApps;
            filApp = filApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            USUARIO item = new USUARIO();
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            return View(vm);
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
            return RedirectToAction("MontarTelaConsultaRemuneracao");
        }

        [HttpGet]
        public ActionResult MontarTelaConsultaRemuneracao()
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

            // Carrega listas
            USUARIO usu = baseApp.GetItemById(usuario.USUA_CD_ID);
            ViewBag.Usuario = usu;
            ViewBag.Contracheques = usu.USUARIO_CONTRACHEQUE.ToList();
            ViewBag.Remuneracao = usu.USUARIO_REMUNERACAO.ToList();

            ViewBag.Title = "Remuneração";
            ViewBag.Perfil = usu.PERFIL.PERF_SG_SIGLA;

            // Recupera numero de usuarios do assinante
            ViewBag.NumeroCC = usu.USUARIO_CONTRACHEQUE.Count;
            ViewBag.NumeroRemuneracao = usu.USUARIO_REMUNERACAO.Count;

            // Abre view
            Session["FiltroRemu"] = null;
            Session["MensRemu"] = 0;
            return View(usuario);
        }

        [HttpGet]
        public ActionResult VerContracheque(Int32 id)
        {
            // Executa a operação
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO_CONTRACHEQUE item = baseApp.GetContrachequeById(id);
            item.USCC_IN_VISUALIZACOES = item.USCC_IN_VISUALIZACOES + 1;
            Int32 volta = baseApp.ValidateEditContracheque(item);
            return View(item);
        }

        [HttpGet]
        public ActionResult VerRemuneracao(Int32 id)
        {
            // Executa a operação
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO_REMUNERACAO item = baseApp.GetRemuneracaoById(id);
            return View(item);
        }

        public ActionResult DownloadContrachequeGeral(Int32 id)
        {
            // Executa a operação
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            DownloadContracheque(id);

            USUARIO_CONTRACHEQUE item = baseApp.GetContrachequeById(id);
            item.USCC_IN_DOWNLOADS = item.USCC_IN_DOWNLOADS + 1;
            Int32 volta = baseApp.ValidateEditContracheque(item);
            return RedirectToAction("MontarTelaConsultaRemuneracao");
        }

        public FileResult DownloadContracheque(Int32 id)
        {
            USUARIO_CONTRACHEQUE item = baseApp.GetContrachequeById(id);
            item.USCC_IN_DOWNLOADS = item.USCC_IN_DOWNLOADS + 1;
            Int32 volta = baseApp.ValidateEditContracheque(item);

            String arquivo = item.USCC_AQ_ARQUIVO;
            Int32 pos = arquivo.LastIndexOf("/") + 1;
            String nomeDownload = arquivo.Substring(pos);
            String contentType = string.Empty;
            if (arquivo.Contains(".pdf"))
            {
                contentType = "application/pdf";
            }
            else if (arquivo.Contains(".jpg"))
            {
                contentType = "image/jpg";
            }
            else if (arquivo.Contains(".png"))
            {
                contentType = "image/png";
            }
            return File(arquivo, contentType, nomeDownload);
        }
    }
}