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
            if ((List<MOVIMENTO_ESTOQUE_PRODUTO>)Session["ListaConsumo"] == null)
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
            ViewBag.Cats = new SelectList(baseApp.GetAllTipos(idAss), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Nome = usuario.USUA_NM_NOME;

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
            return View(objetoMov);
        }

        public ActionResult RetirarFiltroConsumo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaConsumo"] = null;
            Session["FiltroConsumo"] = null;
            return RedirectToAction("MontarTelaConsumoProduto");
        }

        public ActionResult MostrarConsumoMes()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            listaMov = movApp.GetAllItensUserDataMes(usuario.USUA_CD_ID, DateTime.Today.Date, idAss);
            Session["ListaConsumo"] = listaMov;
            Session["FiltroConsumo"] = null;
            return RedirectToAction("MontarTelaConsumoProduto");
        }

        public ActionResult VerProdutoConsumo(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarDesenvolvimento", "BaseAdmin");
        }

        public ActionResult MostrarConsumoDia()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            listaMov = movApp.GetAllItensUserDataMes(usuario.USUA_CD_ID, DateTime.Today.Date, idAss);
            Session["ListaConsumo"] = listaMov;
            Session["FiltroConsumo"] = null;
            return RedirectToAction("MontarTelaConsumoProduto");
        }

        [HttpPost]
        public ActionResult FiltrarConsumo(MOVIMENTO_ESTOQUE_PRODUTO item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<MOVIMENTO_ESTOQUE_PRODUTO> listaObj = new List<MOVIMENTO_ESTOQUE_PRODUTO>();
                Session["FiltroConsumo"] = item;
                Int32 volta = movApp.ExecuteFilter(null, item.PRODUTO.PROD_NM_NOME, null, null, item.MOEP_DT_MOVIMENTO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensConsumo"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaMov = listaObj;
                Session["ListaConsumo"] = listaObj;
                return RedirectToAction("MontarTelaConsumoProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaConsumoProduto");
            }
        }


    }
}