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
    public class ProdutoController : Controller
    {
        private readonly IProdutoAppService baseApp;
        private readonly IMovimentoEstoqueProdutoAppService movApp;
        private readonly ILogAppService logApp;

        private String msg;
        private Exception exception;
        PRODUTO objeto = new PRODUTO();
        PRODUTO objetoAntes = new PRODUTO();
        List<PRODUTO> listaMaster = new List<PRODUTO>();
        List<MOVIMENTO_ESTOQUE_PRODUTO> listaMov = new List<MOVIMENTO_ESTOQUE_PRODUTO>();
        MOVIMENTO_ESTOQUE_PRODUTO objetoMov = new MOVIMENTO_ESTOQUE_PRODUTO();
        MOVIMENTO_ESTOQUE_PRODUTO objetoMovAntes = new MOVIMENTO_ESTOQUE_PRODUTO();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public ProdutoController(IProdutoAppService baseApps, ILogAppService logApps, IMovimentoEstoqueProdutoAppService movApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            movApp = movApps;
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
            Session["ListaProdutos"] = baseApp.GetAllItens(idAss);
            return RedirectToAction("MontarTela");
        }

        [HttpGet]
        public ActionResult MontarTelaConsumoProduto()
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
            if ((List<USUARIO>)Session["ListaConsumo"] == null)
            {
                listaMov = movApp.GetAllItensUserDataMes(usuario.USUA_CD_ID, DateTime.Today.Date, idAss);
                Session["ListaConsumo"] = listaMov;
            }
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = (List<MOVIMENTO_ESTOQUE_PRODUTO>)Session["ListaConsumo"];
            ViewBag.Listas = lista;
            ViewBag.Cont = lista.Count;
            ViewBag.Title = "Consumo";
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Listas
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Entrada", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Saída", Value = "2" });
            ViewBag.Tipos = new SelectList(tipo, "Value", "Text");

            // Mensagem
            if ((Int32)Session["MensConsumo"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["FiltroConsumo"] = null;
            Session["ModoConsumo"] = 0;
            ViewBag.Modo = 0;
            Session["MensConsumo"] = 0;
            Session["VoltaConsumo"] = 1;
            objetoMov = new MOVIMENTO_ESTOQUE_PRODUTO();
            return View(objeto);
        }





    }
}