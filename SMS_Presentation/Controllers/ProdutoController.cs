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
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Odonto.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly IProdutoAppService baseApp;
        private readonly IMovimentoEstoqueProdutoAppService movApp;
        private readonly ILogAppService logApp;
        private readonly IProdutoAppService prodApp;
        private readonly IUnidadeAppService unApp;
        private readonly IFilialAppService filApp;
        private readonly IMatrizAppService matrizApp;
        private readonly IFornecedorAppService fornApp;
        private readonly IProdutotabelaPrecoAppService tpApp;

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

        public ProdutoController(IProdutoAppService baseApps, ILogAppService logApps, IMovimentoEstoqueProdutoAppService movApps, IProdutoAppService prodApps, IUnidadeAppService unApps, IFilialAppService filApps, IMatrizAppService matrizApps, IFornecedorAppService fornApps, IProdutotabelaPrecoAppService tpApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            movApp = movApps;
            prodApp = prodApps;
            unApp = unApps;
            filApp = filApps;
            matrizApp = matrizApps;
            fornApp = fornApps;
            tpApp = tpApps;
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
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarBaseConsumo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaConsumo"] = null;
            return RedirectToAction("MontarTelaConsumoProduto");
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
                listaMov = listaMov.Where(p => p.MOEP_IN_TIPO_MOVIMENTO == 2).ToList();
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
            listaMov = listaMov.Where(p => p.MOEP_IN_TIPO_MOVIMENTO == 2).ToList();
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
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            ViewBag.Nome = usuario.USUA_NM_NOME;
            ViewBag.Filial = usuario.FILIAL.FILI_NM_NOME;
            MOVIMENTO_ESTOQUE_PRODUTO item = movApp.GetItemById(id);
            Session["Movimento"] = item;
            return View(item);
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
            listaMov = listaMov.Where(p => p.MOEP_IN_TIPO_MOVIMENTO == 2).ToList();
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
                Int32 volta = movApp.ExecuteFilter(null, null, item.PRODUTO.PROD_NM_NOME, null, null, item.MOEP_DT_MOVIMENTO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensConsumo"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaMov = listaObj;
                listaMov = listaMov.Where(p => p.MOEP_IN_TIPO_MOVIMENTO == 2).ToList();
                Session["ListaConsumo"] = listaMov;
                return RedirectToAction("MontarTelaConsumoProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaConsumoProduto");
            }
        }

        public ActionResult IncluirCategoriaProduto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["CategoriaToProduto"] = true;
            return RedirectToAction("IncluirCategoriaProduto", "TabelasAuxiliares");
        }

        public ActionResult IncluirSubCategoriaProduto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["SubcategoriaToProduto"] = true;
            return RedirectToAction("IncluirSubCategoriaProduto", "TabelasAuxiliares");
        }

        public JsonResult GetFornecedor(Int32 id)
        {
            var forn = fornApp.GetItemById(id);

            var hash = new Hashtable();
            hash.Add("cnpj", forn.FORN_NR_CNPJ);
            hash.Add("email", forn.FORN_NM_EMAIL);
            hash.Add("tel", forn.FORN_NM_TELEFONES);

            return Json(hash);
        }

        [HttpGet]
        public ActionResult MontarTelaProduto()
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
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if (Session["ListaProduto"] == null)
            {
                listaMaster = prodApp.GetAllItens(idAss);
                Session["ListaProduto"] = listaMaster;
            }
            ViewBag.Listas = (List<PRODUTO>)Session["ListaProduto"];

            if (usuario.FILIAL != null)
            {
                ViewBag.FilialUsuario = usuario.FILIAL.FILI_NM_NOME;
            }

            Session["IdFilial"] = usuario.FILI_CD_ID;
            ViewBag.Title = "Produtos";
            ViewBag.Tipos = new SelectList(prodApp.GetAllTipos(idAss), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(idAss), "SUPR_CD_ID", "SUPR_NM_NOME");
            ViewBag.Unidades = new SelectList(unApp.GetAllItens(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.PerfilUsu = usuario.PERF_CD_ID;
            ViewBag.Produtos = listaMaster.Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            Session["FiltroProduto"] = null;

            // Novos indicadores
            Int32? idFilial = null;
            if (usuario.PERF_CD_ID > 2)
            {
                idFilial = usuario.FILI_CD_ID;
            }
            List<PRODUTO_ESTOQUE_FILIAL> listaBase = prodApp.RecuperarQuantidadesFiliais(idFilial, idAss);
            List<PRODUTO_ESTOQUE_FILIAL> pontoPedido = listaBase.Where(x => x.PREF_QN_ESTOQUE < x.PRODUTO.PROD_QN_QUANTIDADE_MINIMA).ToList();
            List<PRODUTO_ESTOQUE_FILIAL> estoqueZerado = listaBase.Where(x => x.PREF_QN_ESTOQUE == 0).ToList();
            List<PRODUTO_ESTOQUE_FILIAL> estoqueNegativo = listaBase.Where(x => x.PREF_QN_ESTOQUE < 0).ToList();

            if (usuario.PERF_CD_ID == 1 || usuario.PERF_CD_ID == 2)
            {
                Session["PontoPedido"] = pontoPedido;
                Session["EstoqueZerado"] = estoqueZerado;
                Session["EstoqueNegativo"] = estoqueNegativo;
                ViewBag.PontoPedido = pontoPedido.Count;
                ViewBag.EstoqueZerado = estoqueZerado.Count;
                ViewBag.EstoqueNegativo = estoqueNegativo.Count;
            }
            else
            {
                pontoPedido = pontoPedido.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList<PRODUTO_ESTOQUE_FILIAL>();
                estoqueZerado = estoqueZerado.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList<PRODUTO_ESTOQUE_FILIAL>();
                estoqueNegativo = estoqueNegativo.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList<PRODUTO_ESTOQUE_FILIAL>();
                Session["PontoPedido"] = pontoPedido;
                Session["EstoqueZerado"] = estoqueZerado;
                Session["EstoqueNegativo"] = estoqueNegativo;
                ViewBag.PontoPedido = pontoPedido.Count;
                ViewBag.EstoqueZerado = estoqueZerado.Count;
                ViewBag.EstoqueNegativo = estoqueNegativo.Count;
            }

            // Mensagem
            if ((Int32)Session["MensProduto"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProduto"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0010", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProduto"] == 3)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0052", CultureInfo.CurrentCulture));
            }

            // Abre view
            objeto = new PRODUTO();
            Session["VoltaProduto"] = 1;
            Session["VoltaConsulta"] = 1;
            Session["FlagVoltaProd"] = 1;
            Session["Clonar"] = 0;
            Session["VoltaConsulta"] = 1;
            return View(objeto);
        }

        public ActionResult RetirarFiltroProduto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaProduto"] = null;
            Session["FiltroProduto"] = null;
            if ((Int32)Session["FlagVoltaProd"] == 1)
            {
                if ((Int32)Session["VoltaProduto"] == 2)
                {
                    return RedirectToAction("VerCardsProduto");
                }
                return RedirectToAction("MontarTelaProduto");
            }
            else
            {
                return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
            }
        }

        public ActionResult MostrarTudoProduto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = prodApp.GetAllItensAdm(idAss);
            Session["FiltroProduto"] = null;
            Session["ListaProduto"] = listaMaster;
            if ((Int32)Session["VoltaProduto"] == 2)
            {
                return RedirectToAction("VerCardsProduto");
            }
            return RedirectToAction("MontarTelaProduto");
        }

        [HttpPost]
        public ActionResult FiltrarProduto(PRODUTO item)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                List<PRODUTO> listaObj = new List<PRODUTO>();
                Session["FiltroProduto"] = item;
                USUARIO usuario = (USUARIO)Session["UserCredentials"];
                Int32 volta = prodApp.ExecuteFilter(item.CAPR_CD_ID, item.SUPR_CD_ID, item.PROD_NR_BARCODE, item.PROD_NM_NOME, item.PROD_NM_MARCA, item.PROD_CD_CODIGO, item.PROD_NM_MODELO, item.PROD_NM_FABRICANTE, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensProduto"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaProduto"] = listaObj;
                if ((Int32)Session["VoltaProduto"] == 2)
                {
                    return RedirectToAction("VerCardsProduto");
                }
                if ((Int32)Session["VoltaProduto"] == 2)
                {
                    return RedirectToAction("VerProdutosPontoPedido");
                }
                if ((Int32)Session["VoltaProduto"] == 3)
                {
                    return RedirectToAction("VerProdutosEstoqueZerado");
                }
                return RedirectToAction("MontarTelaProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaProduto");
            }
        }

        [HttpPost]
        public ActionResult FiltrarEstoqueProduto(PRODUTO_ESTOQUE_FILIAL item)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                USUARIO usuario = (USUARIO)Session["UserCredentials"];
                Session["FiltroEstoque"] = item;
                Int32? idFilial = null;
                if (usuario.PERF_CD_ID != 1)
                {
                    idFilial = item.FILI_CD_ID;
                }
                List<PRODUTO_ESTOQUE_FILIAL> listaObj = new List<PRODUTO_ESTOQUE_FILIAL>();
                Int32 volta = prodApp.ExecuteFilterEstoque(idFilial, item.PRODUTO.PROD_NM_NOME, item.PRODUTO.PROD_NM_MARCA, item.PRODUTO.PROD_NR_BARCODE, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensProduto"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                if ((Int32)Session["VoltaConsulta"] == 2)
                {
                    listaObj = listaObj.Where(x => x.PREF_QN_ESTOQUE < x.PRODUTO.PROD_QN_QUANTIDADE_MINIMA).ToList();
                    ViewBag.PontoPedidos = listaObj;
                    ViewBag.PontoPedido = listaObj.Count;
                    return RedirectToAction("VerProdutosPontoPedido");
                }
                if ((Int32)Session["VoltaConsulta"] == 3)
                {
                    listaObj = listaObj.Where(x => x.PREF_QN_ESTOQUE == 0).ToList();
                    ViewBag.EstoqueZerados = listaObj;
                    ViewBag.EstoqueZerado = listaObj.Count;
                    return RedirectToAction("VerProdutosEstoqueZerado");
                }
                if ((Int32)Session["VoltaConsulta"] == 4)
                {
                    listaObj = listaObj.Where(x => x.PREF_QN_ESTOQUE < 0).ToList();
                    ViewBag.EstoqueNegativos = listaObj;
                    ViewBag.EstoqueNegativo = listaObj.Count;
                    return RedirectToAction("VerProdutosEstoqueNegativo");
                }
                return RedirectToAction("MontarTelaProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaProduto");
            }
        }

        public ActionResult VoltarBaseProduto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if ((Int32)Session["Clonar"] == 1)
            {
                Session["Clonar"] = 0;
                listaMaster = new List<PRODUTO>();
                Session["ListaProduto"] = null;
            }
            if ((Int32)Session["VoltaEstoque"] == 1)
            {
                return RedirectToAction("MontarTelaEstoqueProduto", "Estoque");
            }

            if ((Int32)Session["VoltaConsulta"] == 1)
            {
                return RedirectToAction("MontarTelaProduto");
            }
            if ((Int32)Session["VoltaConsulta"] == 2)
            {
                return RedirectToAction("VerProdutosPontoPedido");
            }
            if ((Int32)Session["VoltaConsulta"] == 3)
            {
                return RedirectToAction("VerProdutosEstoqueZerado");
            }
            if ((Int32)Session["VoltaConsulta"] == 4)
            {
                return RedirectToAction("VerProdutosEstoqueNegativo");
            }

            if ((Int32)Session["FlagVoltaProd"] == 1)
            {
                if ((Int32)Session["VoltaProduto"] == 2)
                {
                    Session["ListaProduto"] = null;
                    return RedirectToAction("VerCardsProduto");
                }
                Session["ListaProduto"] = null;
                return RedirectToAction("MontarTelaProduto");
            }
            else
            {
                return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
            }
        }

        [HttpGet]
        public ActionResult IncluirProduto()
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensProduto"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Tipos = new SelectList(prodApp.GetAllItens(idAss), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(idAss), "SUPR_CD_ID", "SUPR_NM_NOME");
            ViewBag.Unidades = new SelectList(unApp.GetAllItens(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(idAss), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoEmbalagem = new List<SelectListItem>();
            tipoEmbalagem.Add(new SelectListItem() { Text = "Envelope", Value = "1" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Caixa", Value = "2" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Rolo", Value = "3" });
            ViewBag.TiposEmbalagem = new SelectList(tipoEmbalagem, "Value", "Text");

            // Prepara view
            PRODUTO item = new PRODUTO();
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            vm.PROD_DT_CADASTRO = DateTime.Today;
            vm.PROD_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirProduto(ProdutoViewModel vm, String tabelaProduto)
        {
            Hashtable result = new Hashtable();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Tipos = new SelectList(prodApp.GetAllItens(idAss), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(idAss), "SUPR_CD_ID", "SUPR_NM_NOME");
            ViewBag.Unidades = new SelectList(unApp.GetAllItens(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(idAss), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoEmbalagem = new List<SelectListItem>();
            tipoEmbalagem.Add(new SelectListItem() { Text = "Envelope", Value = "1" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Caixa", Value = "2" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Rolo", Value = "3" });
            ViewBag.TiposEmbalagem = new SelectList(tipoEmbalagem, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = prodApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0053", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Acerta codigo do produto
                    if (item.PROD_CD_CODIGO == null)
                    {
                        item.PROD_CD_CODIGO = item.PROD_CD_ID.ToString();
                        volta = prodApp.ValidateEdit(item, item, usuario);
                    }

                    // Carrega foto e processa alteracao
                    item.PROD_AQ_FOTO = "~/Imagens/Base/FotoBase.jpg";
                    volta = prodApp.ValidateEdit(item, item, usuario);

                    // Cria pastas
                    String caminho = "/Imagens/" + idAss.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + idAss.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + idAss.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/QR/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMaster = new List<PRODUTO>();
                    Session["ListaProduto"] = null;
                    if ((Int32)Session["VoltaProduto"] == 2)
                    {
                        return RedirectToAction("VerCardsProduto");
                    }
                    vm.PROD_CD_ID = item.PROD_CD_ID;
                    IncluirTabelaProduto(vm, tabelaProduto);
                    Session["IdVolta"] = item.PROD_CD_ID;
                    result.Add("id", item.PROD_CD_ID);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    result.Add("error", ex.Message);
                    return Json(result);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult EditarProduto(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensProduto"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            PRODUTO item = prodApp.GetItemById(id);
            ViewBag.Tipos = new SelectList(prodApp.GetAllItens(idAss), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(idAss), "SUPR_CD_ID", "SUPR_NM_NOME");
            ViewBag.Unidades = new SelectList(unApp.GetAllItens(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(idAss), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoEmbalagem = new List<SelectListItem>();
            tipoEmbalagem.Add(new SelectListItem() { Text = "Envelope", Value = "1" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Caixa", Value = "2" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Rolo", Value = "3" });
            ViewBag.TiposEmbalagem = new SelectList(tipoEmbalagem, "Value", "Text");
            ViewBag.Perfil = usuario.PERF_CD_ID;

            //Int32 venda = item.ITEM_PEDIDO_VENDA.Where(p => p.PEDIDO_VENDA.PEVE_DT_DATA.Month == DateTime.Today.Month).ToList().Sum(m => m.ITPE_QN_QUANTIDADE);
            //ViewBag.Vendas = venda;

            // Recupera preços

            // Exibe mensagem
            //if (item.PROD_IN_COMPOSTO == 1 & item.FICHA_TECNICA.Count == 0)
            //{
            //    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0098", CultureInfo.CurrentCulture));
            //}

            if ((Int32)Session["MensProduto"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0054", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProduto"] == 2)
            {
                ModelState.AddModelError("", "Campo PREÇO obrigatorio");
            }
            if ((Int32)Session["MensProduto"] == 3)
            {
                ModelState.AddModelError("", "Campo MARKUP obrigatorio");
            }
            if ((Int32)Session["MensProduto"] == 4)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProduto"] == 5)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProduto"] == 7)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0056", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProduto"] == 8)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0057", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProduto"] == 9)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0058", CultureInfo.CurrentCulture));
            }

            objetoAntes = item;
            Session["Produto"] = item;
            Session["IdVolta"] = id;
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarProduto(ProdutoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Tipos = new SelectList(prodApp.GetAllItens(idAss), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(idAss), "SUPR_CD_ID", "SUPR_NM_NOME");
            ViewBag.Unidades = new SelectList(unApp.GetAllItens(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(idAss), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoEmbalagem = new List<SelectListItem>();
            tipoEmbalagem.Add(new SelectListItem() { Text = "Envelope", Value = "1" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Caixa", Value = "2" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Rolo", Value = "3" });
            ViewBag.TiposEmbalagem = new SelectList(tipoEmbalagem, "Value", "Text");
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            ViewBag.Perfil = usuario.PERF_CD_ID;
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                    Int32 volta = prodApp.ValidateEdit(item, objetoAntes, usuario);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<PRODUTO>();
                    Session["ListaProduto"] = null;
                    if ((Int32)Session["VoltaEstoque"] == 1)
                    {
                        return RedirectToAction("MontarTelaEstoqueProduto", "Estoque");
                    }
                    if ((Int32)Session["VoltaProduto"] == 2)
                    {
                        return RedirectToAction("VerCardsProduto");
                    }
                    return RedirectToAction("MontarTelaProduto");
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
        public ActionResult ConsultarProduto(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            PRODUTO item = prodApp.GetItemById(id);
            ViewBag.Tipos = new SelectList(prodApp.GetAllItens(idAss), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(idAss), "SUPR_CD_ID", "SUPR_NM_NOME");
            ViewBag.Unidades = new SelectList(unApp.GetAllItens(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(idAss), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoEmbalagem = new List<SelectListItem>();
            tipoEmbalagem.Add(new SelectListItem() { Text = "Envelope", Value = "1" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Caixa", Value = "2" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Rolo", Value = "3" });
            ViewBag.TiposEmbalagem = new SelectList(tipoEmbalagem, "Value", "Text");
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            ViewBag.Perfil = usu.PERF_CD_ID;

            //Int32 venda = item.ITEM_PEDIDO_VENDA.Where(p => p.PEDIDO_VENDA.PEVE_DT_DATA.Month == DateTime.Today.Month).ToList().Sum(m => m.ITPE_QN_QUANTIDADE);
            //ViewBag.Vendas = venda;

            var usuario = new USUARIO();
            ViewBag.PerfilUsuario = usuario.PERF_CD_ID;
            ViewBag.CdUsuario = usuario.USUA_CD_ID;

            // Recupera preços

            // Exibe mensagem
            //if (item.PROD_IN_COMPOSTO == 1 & item.FICHA_TECNICA.Count == 0)
            //{
            //    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0098", CultureInfo.CurrentCulture));
            //}

            if ((Int32)Session["MensProduto"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0054", CultureInfo.CurrentCulture));
            }

            //if (usuario.PERF_CD_ID != 1 && usuario.PERF_CD_ID != 2)
            //{
            //    if (usuario.FILI_CD_ID == null)
            //    {
            //        usuario.FILI_CD_ID = SessionMocks.idFilial;
            //    }

            //    item.PROD_VL_PRECO_VENDA = item.PRODUTO_TABELA_PRECO.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).Select(x => x.PRTP_VL_PRECO).FirstOrDefault();
            //    item.PROD_VL_PRECO_PROMOCAO = item.PRODUTO_TABELA_PRECO.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).Select(x => x.PRTP_VL_PRECO_PROMOCAO).FirstOrDefault();
            //}

            objetoAntes = item;
            Session["Produto"] = item;
            Session["IdVolta"] = id;
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        // Filtro em cascata de subcategoria
        [HttpPost]
        public JsonResult FiltrarSubCategoriaProduto(Int32? id)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            var listaSubFiltrada = prodApp.GetAllSubs(idAss);

            // Filtro para caso o placeholder seja selecionado
            if (id != null)
            {
                listaSubFiltrada = prodApp.GetAllSubs(idAss).Where(x => x.CAPR_CD_ID == id).ToList();
            }

            return Json(listaSubFiltrada.Select(x => new { x.SUPR_CD_ID, x.SUPR_NM_NOME }));
        }

        [HttpPost]
        public JsonResult FiltrarCategoriaProduto(Int32? id)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            var listaFiltrada = prodApp.GetAllTipos(idAss);

            // Filtro para caso o placeholder seja selecionado
            if (id != null)
            {
                listaFiltrada = listaFiltrada.Where(x => x.SUBCATEGORIA_PRODUTO.Any(s => s.SUPR_CD_ID == id)).ToList();
            }

            return Json(listaFiltrada.Select(x => new { x.CAPR_CD_ID, x.CAPR_NM_NOME }));
        }

        [HttpGet]
        public ActionResult ExcluirProduto(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensProduto"] = 2;
                    return RedirectToAction("MontarTelaProduto", "Produto");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            PRODUTO item = baseApp.GetItemById(id);
            objetoAntes = (PRODUTO)Session["Produto"];
            item.PROD_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            // Verifica retorno
            if (volta == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0052", CultureInfo.CurrentCulture));
                Session["MensProduto"] = 3;
                return RedirectToAction("MontarTelaProduto", "Produto");
            }
            listaMaster = new List<PRODUTO>();
            Session["ListaProduto"] = null;
            return RedirectToAction("MontarTelaProduto");
        }


        [HttpGet]
        public ActionResult ReativarProduto(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensProduto"] = 2;
                    return RedirectToAction("MontarTelaProduto", "Produto");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            PRODUTO item = baseApp.GetItemById(id);
            objetoAntes = (PRODUTO)Session["Produto"];
            item.PROD_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            listaMaster = new List<PRODUTO>();
            Session["ListaProduto"] = null;
            return RedirectToAction("MontarTelaProduto");
        }

        public ActionResult VerCardsProduto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            // Carrega listas
            if (Session["ListaProduto"] == null)
            {
                listaMaster = prodApp.GetAllItens(idAss);
                Session["ListaProduto"] = listaMaster;
            }
            ViewBag.Listas = (List<PRODUTO>)Session["ListaProduto"];
           
            ViewBag.Title = "Produtos";
            ViewBag.Tipos = new SelectList(prodApp.GetAllTipos(idAss), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(idAss), "SUPR_CD_ID", "SUPR_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(unApp.GetAllItens(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.PerfilUsu = usuario.PERF_CD_ID;
            ViewBag.Produtos = listaMaster.Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            Session["FiltroProduto"] = null;

            // Indicadores
            ViewBag.Produtos = listaMaster.Count;

            // Abre view
            objeto = new PRODUTO();
            Session["VoltaProduto"] = 2;
            return View(objeto);
        }

        [HttpGet]
        public ActionResult VerAnexoProduto(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            PRODUTO_ANEXO item = prodApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoProduto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("EditarProduto", new { id = (Int32)Session["IdVolta"] });
        }

        public FileResult DownloadProduto(Int32 id)
        {
            PRODUTO_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.PRAN_AQ_ARQUIVO;
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

        [HttpPost]
        public ActionResult UploadFileProduto(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                Session["MensProduto"] = 4;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }

            PRODUTO item = baseApp.GetById((Int32)Session["IdVolta"]);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                Session["MensProduto"] = 5;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            PRODUTO_ANEXO foto = new PRODUTO_ANEXO();
            foto.PRAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.PRAN_DT_ANEXO = DateTime.Today;
            foto.PRAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            if (extensao.ToUpper() == ".PDF")
            {
                tipo = 3;
            }
            foto.PRAN_IN_TIPO = tipo;
            foto.PRAN_NM_TITULO = fileName;
            foto.PROD_CD_ID = item.PROD_CD_ID;

            item.PRODUTO_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpPost]
        public JsonResult UploadFileProduto_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
        {
            var count = 0;

            if (perfil == 0)
            {
                count++;
            }

            foreach (var file in files)
            {
                if (count == 0)
                {
                    UploadFotoProduto(file);

                    count++;
                }
                else
                {
                    UploadFileProduto(file);
                }
            }

            return Json("1"); 
        }

        [HttpPost]
        public ActionResult UploadFotoProduto(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                Session["MensProduto"] = 4;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }

            PRODUTO item = baseApp.GetById((Int32)Session["IdVolta"]);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                Session["MensProduto"] = 5;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.PROD_AQ_FOTO = "~" + caminho + fileName;
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            listaMaster = new List<PRODUTO>();
            Session["ListaProduto"] = null;
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpPost]
        public ActionResult UploadQRCodeProduto(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                Session["MensProduto"] = 4;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }


            PRODUTO item = baseApp.GetById((Int32)Session["IdVolta"]);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                Session["MensProduto"] = 5;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/QR/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Checa extensão
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                // Salva arquivo
                file.SaveAs(path);

                // Gravar registro
                item.PROD_QR_QRCODE = "~" + caminho + fileName;
                objeto = item;
                Int32 volta = prodApp.ValidateEdit(item, objeto);
            }
            else
            {
                Session["MensProduto"] = 6;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0055", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }
            return RedirectToAction("VoltarAnexoProduto");
        }

        public ActionResult VerMovimentacaoEstoqueProduto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            PRODUTO item = prodApp.GetItemById((Int32)Session["IdVolta"]);
            objetoAntes = item;
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        public ActionResult VerProdutosPontoPedido()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Prepara view
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            List<PRODUTO_ESTOQUE_FILIAL> lista = (List<PRODUTO_ESTOQUE_FILIAL>)Session["PontoPedido"];

            ViewBag.PontoPedidos = lista;
            ViewBag.PontoPedido = lista.Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            Session["ListaEstoque"] = Session["PontoPedido"];

            // Abre view
            PRODUTO_ESTOQUE_FILIAL prod = new PRODUTO_ESTOQUE_FILIAL();
            Session["VoltaProduto"] = 1;
            Session["Clonar"] = 0;
            Session["VoltaConsulta"] = 2;
            Session["FiltroEstoque"] = null;
            ViewBag.Tipo = 2;
            return View(prod);
        }

        public ActionResult RetirarFiltroProdutoPontoPedido()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("VerProdutosPontoPedido");
        }

        public ActionResult RetirarFiltroProdutoEstoqueNegativo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("VerProdutosEstoqueNegativo");
        }

        public ActionResult VerProdutosEstoqueZerado()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            List<PRODUTO_ESTOQUE_FILIAL> lista = (List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueZerado"];

            ViewBag.EstoqueZerados = lista;
            ViewBag.EstoqueZerado = lista.Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            Session["ListaEstoque"] = Session["EstoqueZerado"];

            // Abre view
            PRODUTO_ESTOQUE_FILIAL prod = new PRODUTO_ESTOQUE_FILIAL();
            Session["VoltaProduto"] = 1;
            Session["Clonar"] = 0;
            Session["VoltaConsulta"] = 3;
            Session["FiltroEstoque"] = null;
            ViewBag.Tipo = 3;
            return View(prod);
        }

        public ActionResult VerProdutosEstoqueNegativo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            List<PRODUTO_ESTOQUE_FILIAL> lista = (List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueNegativo"];

            ViewBag.EstoqueNegativos = lista;
            ViewBag.EstoqueNegativo = lista.Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            Session["ListaEstoque"] = Session["EstoqueNegativo"];

            // Abre view
            PRODUTO_ESTOQUE_FILIAL prod = new PRODUTO_ESTOQUE_FILIAL();
            Session["VoltaProduto"] = 1;
            Session["Clonar"] = 0;
            Session["VoltaConsulta"] = 4;
            Session["FiltroEstoque"] = null;
            ViewBag.Tipo = 4;
            return View(prod);
        }

        public ActionResult RetirarFiltroProdutoEstoqueZerado()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("VerProdutosEstoqueZerado");
        }

        [HttpGet]
        public ActionResult EditarProdutoFornecedor(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(idAss), "FORN_CD_ID", "FORN_NM_NOME");
            PRODUTO_FORNECEDOR item = prodApp.GetFornecedorById(id);
            objetoAntes = (PRODUTO)Session["Produto"];
            ProdutoFornecedorViewModel vm = Mapper.Map<PRODUTO_FORNECEDOR, ProdutoFornecedorViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarProdutoFornecedor(ProdutoFornecedorViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(idAss), "FORN_CD_ID", "FORN_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    PRODUTO_FORNECEDOR item = Mapper.Map<ProdutoFornecedorViewModel, PRODUTO_FORNECEDOR>(vm);
                    Int32 volta = prodApp.ValidateEditFornecedor(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoProduto");
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
        public ActionResult ExcluirProdutoFornecedor(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            PRODUTO_FORNECEDOR item = prodApp.GetFornecedorById(id);
            objetoAntes = (PRODUTO)Session["Produto"];
            item.PRFO_IN_ATIVO = 0;
            Int32 volta = prodApp.ValidateEditFornecedor(item);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult ReativarProdutoFornecedor(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            PRODUTO_FORNECEDOR item = prodApp.GetFornecedorById(id);
            objetoAntes = (PRODUTO)Session["Produto"];
            item.PRFO_IN_ATIVO = 1;
            Int32 volta = prodApp.ValidateEditFornecedor(item);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult IncluirProdutoFornecedor()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(idAss), "FORN_CD_ID", "FORN_NM_NOME");
            PRODUTO_FORNECEDOR item = new PRODUTO_FORNECEDOR();
            ProdutoFornecedorViewModel vm = Mapper.Map<PRODUTO_FORNECEDOR, ProdutoFornecedorViewModel>(item);
            vm.PROD_CD_ID = (Int32)Session["IdVolta"];
            vm.PRFO_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirProdutoFornecedor(ProdutoFornecedorViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(idAss), "FORN_CD_ID", "FORN_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PRODUTO_FORNECEDOR item = Mapper.Map<ProdutoFornecedorViewModel, PRODUTO_FORNECEDOR>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = prodApp.ValidateCreateFornecedor(item);
                    
                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoProduto");
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

        public ActionResult GerarRelatorioFiltro()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("GerarRelatorioLista", new { id = 1 });
        }

        public ActionResult GerarRelatorioZerado()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("GerarRelatorioEstoque", new { id = 2 });
        }

        public ActionResult GerarRelatorioPonto()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("GerarRelatorioEstoque", new { id = 1 });
        }

        public ActionResult GerarRelatorioNegativo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            return RedirectToAction("GerarRelatorioEstoque", new { id = 3 });
        }

        public ActionResult GerarRelatorioLista(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = String.Empty;
            String titulo = String.Empty;
            List<PRODUTO> lista = new List<PRODUTO>();
            if (id == 1)
            {
                nomeRel = "ProdutoLista" + "_" + data + ".pdf";
                titulo = "Produtos - Listagem";
                lista = (List<PRODUTO>)Session["ListaProduto"];
            }
            else
            {
                return RedirectToAction("MontarTelaProduto");
            }
            PRODUTO filtro = (PRODUTO)Session["FiltroProduto"];
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // Linha horizontal
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line);

            // Cabeçalho
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Images/5.png"));
            image.ScaleAbsolute(50, 50);
            cell.AddElement(image);
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph(titulo, meuFont2))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cell.Border = 0;
            cell.Colspan = 4;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);
            line1 = new Paragraph("  ");
            pdfDoc.Add(line1);

            // Grid
            table = new PdfPTable(new float[] { 100f, 100f, 150f, 80f, 40f, 100f, 100f, 40f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Produtos selecionados pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.Colspan = 9;
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Nome", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Categoria", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Sub-Categoria", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Código de Barras", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Código", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Marca", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Modelo", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Imagem", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            foreach (PRODUTO item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.PROD_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.CATEGORIA_PRODUTO.CAPR_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.SUBCATEGORIA_PRODUTO.SUPR_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PROD_NR_BARCODE, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PROD_CD_CODIGO, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PROD_NM_MARCA, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PROD_NM_MODELO, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (System.IO.File.Exists(Server.MapPath(item.PROD_AQ_FOTO)))
                {
                    cell = new PdfPCell();
                    image = Image.GetInstance(Server.MapPath(item.PROD_AQ_FOTO));
                    image.ScaleAbsolute(20, 20);
                    cell.AddElement(image);
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("-", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line2);

            // Rodapé
            Chunk chunk1 = new Chunk("Parâmetros de filtro: ", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            String parametros = String.Empty;
            Int32 ja = 0;
            if (filtro != null)
            {
                if (filtro.CAPR_CD_ID > 0)
                {
                    parametros += "Categoria: " + filtro.CAPR_CD_ID.ToString();
                    ja = 1;
                }
                if (filtro.SUPR_CD_ID > 0)
                {
                    if (ja == 0)
                    {
                        parametros += "Subcategoria: " + filtro.SUPR_CD_ID.ToString();
                        ja = 1;
                    }
                    else
                    {
                        parametros += "e Subcategoria: " + filtro.SUPR_CD_ID.ToString();
                    }
                }
                if (filtro.PROD_NR_BARCODE != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Código de Barras: " + filtro.PROD_NR_BARCODE;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Código de Barras: " + filtro.PROD_NR_BARCODE;
                    }
                }
                if (filtro.PROD_CD_CODIGO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Código: " + filtro.PROD_CD_CODIGO;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Código: " + filtro.PROD_CD_CODIGO;
                    }
                }
                if (filtro.PROD_NM_NOME != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Nome: " + filtro.PROD_NM_NOME;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Nome: " + filtro.PROD_NM_NOME;
                    }
                }
                if (filtro.PROD_NM_MARCA != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Marca: " + filtro.PROD_NM_MARCA;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Marca: " + filtro.PROD_NM_MARCA;
                    }
                }
                if (ja == 0)
                {
                    parametros = "Nenhum filtro definido.";
                }
            }
            else
            {
                parametros = "Nenhum filtro definido.";
            }
            Chunk chunk = new Chunk(parametros, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);

            // Linha Horizontal
            Paragraph line3 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line3);

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();
            return RedirectToAction("MontarTelaProduto");
        }

        public ActionResult GerarRelatorioEstoque(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = String.Empty;
            String titulo = String.Empty;
            List<PRODUTO_ESTOQUE_FILIAL> lista = new List<PRODUTO_ESTOQUE_FILIAL>();
            if (id == 1)
            {
                nomeRel = "PontoPedido" + "_" + data + ".pdf";
                titulo = "Produtos - Ponto de Pedido";
                lista = (List<PRODUTO_ESTOQUE_FILIAL>)Session["PontoPedido"];
            }
            if (id == 2)
            {
                nomeRel = "EstoqueZerado" + "_" + data + ".pdf";
                titulo = "Produtos - Estoque Zerado";
                lista = (List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueZerado"];
            }
            if (id == 3)
            {
                nomeRel = "EstoqueNegativo" + "_" + data + ".pdf";
                titulo = "Produtos - Estoque Negativo";
                lista = (List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueNegativo"];
            }

            PRODUTO_ESTOQUE_FILIAL filtro = (PRODUTO_ESTOQUE_FILIAL)Session["FiltroEstoque"];
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // Linha horizontal
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line);

            // Cabeçalho
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Images/5.png"));
            image.ScaleAbsolute(50, 50);
            cell.AddElement(image);
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph(titulo, meuFont2))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cell.Border = 0;
            cell.Colspan = 4;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);
            line1 = new Paragraph("  ");
            pdfDoc.Add(line1);

            // Grid
            table = new PdfPTable(new float[] { 90f, 100f, 100f, 150f, 80f, 40f, 100f, 100f, 50f, 40f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Produtos selecionados pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.Colspan = 9;
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Filial", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Nome", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Categoria", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Sub-Categoria", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Código de Barras", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Marca", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Modelo", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Fabricante", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Estoque", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Imagem", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            foreach (PRODUTO_ESTOQUE_FILIAL item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.FILIAL.FILI_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PRODUTO.PROD_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PRODUTO.CATEGORIA_PRODUTO.CAPR_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PRODUTO.SUBCATEGORIA_PRODUTO.SUPR_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PRODUTO.PROD_NR_BARCODE, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PRODUTO.PROD_NM_MARCA, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PRODUTO.PROD_NM_MODELO, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PRODUTO.PROD_NM_FABRICANTE, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(CrossCutting.Formatters.DecimalFormatter(Convert.ToDecimal(item.PREF_QN_ESTOQUE)), meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (System.IO.File.Exists(Server.MapPath(item.PRODUTO.PROD_AQ_FOTO)))
                {
                    cell = new PdfPCell();
                    image = Image.GetInstance(Server.MapPath(item.PRODUTO.PROD_AQ_FOTO));
                    image.ScaleAbsolute(20, 20);
                    cell.AddElement(image);
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("-", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line2);

            // Rodapé
            Chunk chunk1 = new Chunk("Parâmetros de filtro: ", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            String parametros = String.Empty;
            Int32 ja = 0;
            if (filtro != null)
            {
                if (filtro.PRODUTO.PROD_NR_BARCODE != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Código de Barras: " + filtro.PRODUTO.PROD_NR_BARCODE;
                        ja = 1;
                    }
                }
                if (filtro.FILI_CD_ID > 0)
                {
                    if (ja == 0)
                    {
                        parametros += "Filial: " + filtro.FILI_CD_ID.ToString();
                        ja = 1;
                    }
                    else
                    {
                        parametros += "e Filial: " + filtro.FILI_CD_ID.ToString();
                    }
                }
                if (filtro.PRODUTO.PROD_NM_NOME != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Nome: " + filtro.PRODUTO.PROD_NM_NOME;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Nome: " + filtro.PRODUTO.PROD_NM_NOME;
                    }
                }
                if (filtro.PRODUTO.PROD_NM_MARCA != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Marca: " + filtro.PRODUTO.PROD_NM_MARCA;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Marca: " + filtro.PRODUTO.PROD_NM_MARCA;
                    }
                }
                if (ja == 0)
                {
                    parametros = "Nenhum filtro definido.";
                }
            }
            else
            {
                parametros = "Nenhum filtro definido.";
            }
            Chunk chunk = new Chunk(parametros, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);

            // Linha Horizontal
            Paragraph line3 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line3);

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

            if ((Int32)Session["VoltaConsulta"] == 2)
            {
                return RedirectToAction("VerProdutosPontoPedido");
            }
            if ((Int32)Session["VoltaConsulta"] == 3)
            {
                return RedirectToAction("VerProdutosEstoqueZerado");
            }
            if ((Int32)Session["VoltaConsulta"] == 4)
            {
                return RedirectToAction("VerProdutosEstoqueNegativo");
            }
            return RedirectToAction("MontarTelaProduto");
        }

        public ActionResult GerarRelatorioDetalhe()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara geração
            PRODUTO aten = prodApp.GetItemById((Int32)Session["IdVolta"]);
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "Produto_" + aten.PROD_CD_ID.ToString() + "_" + data + ".pdf";
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFontBold = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // Linha horizontal
            Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Cabeçalho
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Images/5.png"));
            image.ScaleAbsolute(50, 50);
            cell.AddElement(image);
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Produto - Detalhes", meuFont2))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cell.Border = 0;
            cell.Colspan = 4;
            table.AddCell(cell);

            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);
            line1 = new Paragraph("  ");
            pdfDoc.Add(line1);

            // Dados Gerais
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Dados Gerais", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (System.IO.File.Exists(Server.MapPath(aten.PROD_AQ_FOTO)))
            {
                cell = new PdfPCell();
                cell.Border = 0;
                cell.Colspan = 1;
                image = Image.GetInstance(Server.MapPath(aten.PROD_AQ_FOTO));
                image.ScaleAbsolute(50, 50);
                cell.AddElement(image);
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("", meuFontBold));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            cell = new PdfPCell(new Paragraph(" ", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell();
            if (System.IO.File.Exists(Server.MapPath(aten.PROD_QR_QRCODE)))
            {
                cell.Border = 0;
                cell.Colspan = 1;
                image = Image.GetInstance(Server.MapPath(aten.PROD_QR_QRCODE));
                image.ScaleAbsolute(50, 50);
                cell.AddElement(image);
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("", meuFontBold));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Paragraph("Categoria: " + aten.CATEGORIA_PRODUTO.CAPR_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Subcategoria: " + aten.SUBCATEGORIA_PRODUTO.SUPR_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 3;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Código de Barras: " + aten.PROD_NR_BARCODE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Nome: " + aten.PROD_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 3;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Marca: " + aten.PROD_NM_MARCA, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Modelo: " + aten.PROD_NM_MODELO, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Fabricante: " + aten.PROD_NM_FABRICANTE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Referência: " + aten.PROD_NM_REFERENCIA_FABRICANTE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (aten.UNIDADE != null)
            {
                cell = new PdfPCell(new Paragraph("Unidade: " + aten.UNIDADE.UNID_NM_NOME, meuFont));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Unidade: -", meuFont));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Paragraph("Descrição: " + aten.PROD_DS_DESCRICAO, meuFont));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Informações do Produto: " + aten.PROD_DS_INFORMACOES, meuFont));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Estoque
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Estoque", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Estoque Máximo: " + aten.PROD_QN_QUANTIDADE_MAXIMA, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Estoque Mínimo: " + aten.PROD_QN_QUANTIDADE_MINIMA, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Reserva de Estoque: " + aten.PROD_QN_RESERVA_ESTOQUE, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Lista de Fornecedores
            if (aten.PRODUTO_FORNECEDOR.Count > 0)
            {
                table = new PdfPTable(new float[] { 120f, 120f, 120f, 50f });
                table.WidthPercentage = 100;
                table.HorizontalAlignment = 0;
                table.SpacingBefore = 1f;
                table.SpacingAfter = 1f;

                cell = new PdfPCell(new Paragraph("Nome", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("E-Mail", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Telefone", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Ativo", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                foreach (PRODUTO_FORNECEDOR item in aten.PRODUTO_FORNECEDOR)
                {
                    cell = new PdfPCell(new Paragraph(item.FORNECEDOR.FORN_NM_NOME, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.FORNECEDOR.FORN_NM_EMAIL, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.FORNECEDOR.FORN_NM_TELEFONES, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.PRFO_IN_ATIVO == 1)
                    {
                        cell = new PdfPCell(new Paragraph("Ativo", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else
                    {
                        cell = new PdfPCell(new Paragraph("Inativo", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                }
                pdfDoc.Add(table);
            }

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Expedição
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Expedição", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Localização no Estoque: " + aten.PROD_NM_LOCALIZACAO_ESTOQUE, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Peso Líquido (Kg): " + aten.PROD_QN_PESO_LIQUIDO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Peso Bruto (Kg): " + aten.PROD_QN_PESO_BRUTO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (aten.PROD_IN_TIPO_EMBALAGEM == 1)
            {
                cell = new PdfPCell(new Paragraph("Tipo de Embalagem: Envelope", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else if (aten.PROD_IN_TIPO_EMBALAGEM == 2)
            {
                cell = new PdfPCell(new Paragraph("Tipo de Embalagem: Caixa", meuFont));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Tipo de Embalagem: Rolo", meuFont));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Paragraph("Altura (cm): " + aten.PROD_NR_ALTURA, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Largura (cm): " + aten.PROD_NR_LARGURA, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Comprimento (cm): " + aten.PROD_NR_COMPRIMENTO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Diametro (cm): " + aten.PROD_NR_DIAMETRO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Observações
            Chunk chunk1 = new Chunk("Observações: " + aten.PROD_TX_OBSERVACOES, FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            // Movimentações 
            if (aten.MOVIMENTO_ESTOQUE_PRODUTO.Count > 0)
            {
                // Linha Horizontal
                line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
                pdfDoc.Add(line1);

                table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
                table.WidthPercentage = 100;
                table.HorizontalAlignment = 0;
                table.SpacingBefore = 1f;
                table.SpacingAfter = 1f;

                cell = new PdfPCell(new Paragraph("Movimentações de Estoque (Mais recentes)", meuFontBold));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                pdfDoc.Add(table);

                // Movimentos
                table = new PdfPTable(new float[] { 80f, 80f, 80f });
                table.WidthPercentage = 100;
                table.HorizontalAlignment = 0;
                table.SpacingBefore = 1f;
                table.SpacingAfter = 1f;

                cell = new PdfPCell(new Paragraph("Data", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Tipo", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Quantidade", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Filial", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                foreach (MOVIMENTO_ESTOQUE_PRODUTO item in aten.MOVIMENTO_ESTOQUE_PRODUTO.OrderByDescending(a => a.MOEP_DT_MOVIMENTO).Take(10))
                {
                    cell = new PdfPCell(new Paragraph(item.MOEP_DT_MOVIMENTO.Value.ToShortDateString(), meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.MOEP_IN_TIPO_MOVIMENTO == 1)
                    {
                        cell = new PdfPCell(new Paragraph("Entrada", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else
                    {
                        cell = new PdfPCell(new Paragraph("Saída", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    cell = new PdfPCell(new Paragraph(item.MOEP_QN_QUANTIDADE.ToString(), meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.FILIAL != null)
                    {
                        cell = new PdfPCell(new Paragraph(item.FILIAL.FILI_NM_NOME.ToString(), meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else
                    {
                        cell = new PdfPCell(new Paragraph("-", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                }
                pdfDoc.Add(table);
            }

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpPost]
        public ActionResult IncluirTabelaProduto(ProdutoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                if (vm.PRTP_VL_PRECO == null)
                {
                    Session["MensProduto"] = 7;
                }

                if (vm.PRO_VL_MARKUP_PADRAO == null)
                {
                    Session["MensProduto"] = 8;
                }

                // Executa a operação
                USUARIO usuario = (USUARIO)Session["UserCredentials"];
                PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                Int32 volta = prodApp.IncluirTabelaPreco(item, usuario);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensProduto"] = 9;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0099", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoProduto");
                }

                // Sucesso
                return RedirectToAction("VoltarAnexoProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("VoltarAnexoProduto");
            }
        }

        public void IncluirTabelaProduto(ProdutoViewModel vm, String tabelaProduto)
        {
            Session["VoltaConsulta"] = 1;
            var jArray = JArray.Parse(tabelaProduto);

            foreach (var jObject in jArray)
            {
                try
                {
                    // Executa a operação
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                    PRODUTO_TABELA_PRECO tblPreco = new PRODUTO_TABELA_PRECO();

                    item.PRTP_VL_PRECO = (decimal)jObject["preco"];
                    item.PRTP_VL_PRECO_PROMOCAO = (decimal)jObject["precoPromo"];
                    item.PRO_VL_MARKUP_PADRAO = (decimal)jObject["markup"];
                    item.PROD_VL_CUSTO = (decimal)jObject["custo"];

                    Int32 volta = prodApp.IncluirTabelaPreco(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0099", CultureInfo.CurrentCulture));
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                }
            }
        }

        [HttpGet]
        public ActionResult ReativarTabelaProduto(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Verifica se tem usuario logado
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            PRODUTO_TABELA_PRECO item = tpApp.GetItemById(id);
            item.PRTP_IN_ATIVO = 1;
            Int32 volta = prodApp.ValidateEditTabelaPreco(item);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult ExcluirTabelaProduto(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Verifica se tem usuario logado
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            PRODUTO rot = (PRODUTO)Session["Produto"];
            PRODUTO_TABELA_PRECO rl = tpApp.GetItemById(id);
            Int32 volta = tpApp.ValidateDelete(rl);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpPost]
        public ActionResult EditarPC(ProdutoViewModel vm, Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                USUARIO usuario = (USUARIO)Session["UserCredentials"];
                PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                PRODUTO_TABELA_PRECO prtp = new PRODUTO_TABELA_PRECO();
                PRODUTO_TABELA_PRECO prtpAntes = new PRODUTO_TABELA_PRECO();
                prtpAntes = tpApp.GetItemById(id);

                prtp.PRTP_CD_ID = id;
                prtp.PROD_CD_ID = item.PROD_CD_ID;
                prtp.FILI_CD_ID = usuario.FILI_CD_ID;
                prtp.PRTP_VL_CUSTO = item.PROD_VL_CUSTO;
                prtp.PRTP_NR_MARKUP = (Int32)item.PRO_VL_MARKUP_PADRAO;
                prtp.PRTP_VL_PRECO = item.PRTP_VL_PRECO;
                prtp.PRTP_VL_PRECO_PROMOCAO = item.PRTP_VL_PRECO_PROMOCAO;
                prtp.PRTP_DT_DATA_REAJUSTE = DateTime.Today.Date;
                prtp.PRTP_IN_ATIVO = 1;

                Int32 volta = tpApp.ValidateEdit(prtp, prtpAntes);
                return RedirectToAction("EditarProduto", new { id = item.PROD_CD_ID });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("EditarProduto", new { id = vm.PROD_CD_ID });
            }
        }




    }
}