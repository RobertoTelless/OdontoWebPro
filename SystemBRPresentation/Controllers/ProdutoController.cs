using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SystemBRPresentation.App_Start;
using EntitiesServices.Work_Classes;
using AutoMapper;
using SystemBRPresentation.ViewModels;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Runtime.Remoting.Lifetime;
using Newtonsoft.Json.Linq;
using System.Collections;
using Org.BouncyCastle.Math.Field;

namespace SystemBRPresentation.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly IProdutoAppService prodApp;
        private readonly ILogAppService logApp;
        private readonly IUnidadeAppService unApp;
        private readonly ICategoriaProdutoAppService cpApp;
        private readonly IFilialAppService filApp;
        private readonly IMatrizAppService matrizApp;
        private readonly IFornecedorAppService fornApp;
        private readonly IProdutotabelaPrecoAppService tpApp;

        private String msg;
        private Exception exception;
        PRODUTO objetoProd = new PRODUTO();
        PRODUTO objetoProdAntes = new PRODUTO();
        List<PRODUTO> listaMasterProd = new List<PRODUTO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public ProdutoController(IProdutoAppService prodApps, ILogAppService logApps, IUnidadeAppService unApps, ICategoriaProdutoAppService cpApps, IFilialAppService filApps, IMatrizAppService matrizApps, IFornecedorAppService fornApps, IProdutotabelaPrecoAppService tpApps)
        {
            prodApp = prodApps;
            logApp = logApps;
            unApp = unApps;
            cpApp = cpApps;
            filApp = filApps;
            matrizApp = matrizApps;
            fornApp = fornApps;
            tpApp = tpApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            PRODUTO item = new PRODUTO();
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        public ActionResult Voltar()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarGeral()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult DashboardAdministracao()
        {
            listaMasterProd = new List<PRODUTO>();
            SessionMocks.produto = null;
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpPost]
        public JsonResult GetValorGrafico(Int32 id, Int32? meses)
        {
            if (meses == null)
            {
                meses = 3;
            }

            var prod = prodApp.GetById(id);

            var m1 = prod.ITEM_PEDIDO_VENDA.Where(x => x.PEDIDO_VENDA.PEVE_DT_APROVACAO >= DateTime.Now.AddDays(DateTime.Now.Day * -1)).Sum(x => x.ITPE_QN_QUANTIDADE);
            var m2 = prod.ITEM_PEDIDO_VENDA.Where(x => x.PEDIDO_VENDA.PEVE_DT_APROVACAO >= DateTime.Now.AddDays(DateTime.Now.Day * -1).AddMonths(-1) && x.PEDIDO_VENDA.PEVE_DT_APROVACAO <= DateTime.Now.AddDays(DateTime.Now.Day * -1)).Sum(x => x.ITPE_QN_QUANTIDADE);
            var m3 = prod.ITEM_PEDIDO_VENDA.Where(x => x.PEDIDO_VENDA.PEVE_DT_APROVACAO >= DateTime.Now.AddDays(DateTime.Now.Day * -1).AddMonths(-2) && x.PEDIDO_VENDA.PEVE_DT_APROVACAO <= DateTime.Now.AddDays(DateTime.Now.Day * -1).AddMonths(-1)).Sum(x => x.ITPE_QN_QUANTIDADE);

            var hash = new Hashtable();
            hash.Add("m1", m1);
            hash.Add("m2", m2);
            hash.Add("m3", m3);

            return Json(hash);
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
            usuario = SessionMocks.UserCredentials;
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;

            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            if (SessionMocks.listaProduto == null)
            {
                listaMasterProd = prodApp.GetAllItens();
                SessionMocks.listaProduto = listaMasterProd;
            }
            ViewBag.Listas = SessionMocks.listaProduto;

            if (usuario.FILIAL != null)
            {
                ViewBag.FilialUsuario = usuario.FILIAL.FILI_NM_NOME;
            }

            ViewBag.Title = "Produtosxxx";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Tipos = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.PerfilUsu = usuario.PERF_CD_ID;
            ViewBag.Produtos = listaMasterProd.Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            SessionMocks.filtroProduto = null;

            // Indicadores
            //SessionMocks.pontoPedidoProd = SessionMocks.listaProduto.Where(p => p.PROD_QN_ESTOQUE < p.PROD_QN_QUANTIDADE_MINIMA & p.PROD_IN_ATIVO == 1 & p.ASSI_CD_ID == SessionMocks.IdAssinante & p.PROD_IN_COMPOSTO == 0).ToList();
            //SessionMocks.estoqueZeradoProd = SessionMocks.listaProduto.Where(p => p.PROD_QN_ESTOQUE <= 0 & p.PROD_IN_ATIVO == 1 & p.ASSI_CD_ID == SessionMocks.IdAssinante & p.PROD_IN_COMPOSTO == 0).ToList();

            //if (usuario.PERF_CD_ID == 1 || usuario.PERF_CD_ID == 2)
            //{
            //    ViewBag.PontoPedido = SessionMocks.pontoPedidoProd.Count;
            //    ViewBag.EstoqueZerado = SessionMocks.estoqueZeradoProd.Count;
            //}
            //else
            //{
            //    ViewBag.PontoPedido = SessionMocks.pontoPedidoProd.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList().Count;
            //    ViewBag.EstoqueZerado = SessionMocks.estoqueZeradoProd.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList().Count;
            //}
            //ViewBag.PontoPedidos = SessionMocks.pontoPedidoProd;
            //ViewBag.EstoqueZerados = SessionMocks.estoqueZeradoProd;

            // Novos indicadores
            Int32? idFilial = null;
            if (usuario.PERF_CD_ID > 2)
            {
                idFilial = usuario.FILI_CD_ID;
            }
            List<PRODUTO_ESTOQUE_FILIAL> listaBase = prodApp.RecuperarQuantidadesFiliais(idFilial);
            List<PRODUTO_ESTOQUE_FILIAL> pontoPedido = listaBase.Where(x => x.PREF_QN_ESTOQUE < x.PRODUTO.PROD_QN_QUANTIDADE_MINIMA).ToList();
            List<PRODUTO_ESTOQUE_FILIAL> estoqueZerado = listaBase.Where(x => x.PREF_QN_ESTOQUE == 0).ToList();
            List<PRODUTO_ESTOQUE_FILIAL> estoqueNegativo = listaBase.Where(x => x.PREF_QN_ESTOQUE < 0).ToList();
            Session["PontoPedido"] = pontoPedido;
            Session["EstoqueZerado"] = estoqueZerado;
            Session["EstoqueNegativo"] = estoqueNegativo;
            ViewBag.PontoPedido = pontoPedido.Count;
            ViewBag.EstoqueZerado = estoqueZerado.Count;
            ViewBag.EstoqueNegativo = estoqueNegativo.Count;

            // Abre view
            objetoProd = new PRODUTO();
            SessionMocks.voltaProduto = 1;
            SessionMocks.voltaConsulta = 1;
            SessionMocks.flagVoltaProd = 1;
            SessionMocks.clonar = 0;
            Session["VoltaConsulta"] = 1;
            return View(objetoProd);
        }

        public ActionResult RetirarFiltroProduto()
        {
            SessionMocks.listaProduto = null;
            SessionMocks.filtroProduto = null;
            if (SessionMocks.flagVoltaProd == 1)
            {
                if (SessionMocks.voltaProduto == 2)
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
            listaMasterProd = prodApp.GetAllItensAdm();
            SessionMocks.filtroProduto = null;
            SessionMocks.listaProduto = listaMasterProd;
            if (SessionMocks.voltaProduto == 2)
            {
                return RedirectToAction("VerCardsProduto");
            }
            return RedirectToAction("MontarTelaProduto");
        }

        [HttpPost]
        public ActionResult FiltrarProduto(PRODUTO item)
        {
            try
            {
                // Executa a operação
                List<PRODUTO> listaObj = new List<PRODUTO>();
                SessionMocks.filtroProduto = item;
                USUARIO usuario = SessionMocks.UserCredentials;
                if (usuario.PERF_CD_ID != 1)
                {
                    item.FILI_CD_ID = usuario.FILI_CD_ID;
                }
                Int32 volta = prodApp.ExecuteFilter(item.CAPR_CD_ID, item.SCPR_CD_ID, item.PROD_NM_NOME, item.PROD_NM_MARCA, item.PROD_NR_BARCODE, item.PROD_CD_CODIGO, item.FILI_CD_ID, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture);
                }

                // Sucesso
                listaMasterProd = listaObj;
                SessionMocks.listaProduto = listaObj;
                if (SessionMocks.voltaProduto == 2)
                {
                    return RedirectToAction("VerCardsProduto");
                }
                if (SessionMocks.voltaConsulta == 2)
                {
                    return RedirectToAction("VerProdutosPontoPedido");
                }
                if (SessionMocks.voltaConsulta == 3)
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
            try
            {
                // Executa a operação
                USUARIO usuario = SessionMocks.UserCredentials;
                Session["FiltroEstoque"] = item;
                Int32? idFilial = null;
                if (usuario.PERF_CD_ID != 1)
                {
                    idFilial = item.FILI_CD_ID;
                }
                List<PRODUTO_ESTOQUE_FILIAL> listaObj = new List<PRODUTO_ESTOQUE_FILIAL>();
                Int32 volta = prodApp.ExecuteFilterEstoque(idFilial, item.PRODUTO.PROD_NM_NOME, item.PRODUTO.PROD_NM_MARCA, item.PRODUTO.PROD_NR_BARCODE, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture);
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

            if (SessionMocks.clonar == 1)
            {
                SessionMocks.clonar = 0;
                listaMasterProd = new List<PRODUTO>();
                SessionMocks.listaProduto = null;
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

            if (SessionMocks.flagVoltaProd == 1)
            {
                if (SessionMocks.voltaProduto == 2)
                {
                    SessionMocks.listaProduto = null;
                    return RedirectToAction("VerCardsProduto");
                }
                SessionMocks.listaProduto = null;
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
            // Prepara listas
            ViewBag.Tipos = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoProduto = new List<SelectListItem>();
            tipoProduto.Add(new SelectListItem() { Text = "Simples", Value = "1" });
            tipoProduto.Add(new SelectListItem() { Text = "Kit", Value = "2" });
            tipoProduto.Add(new SelectListItem() { Text = "Fabricado", Value = "3" });
            ViewBag.TiposProduto = new SelectList(tipoProduto, "Value", "Text");
            List<SelectListItem> tipoEmbalagem = new List<SelectListItem>();
            tipoEmbalagem.Add(new SelectListItem() { Text = "Envelope", Value = "1" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Caixa", Value = "2" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Rolo", Value = "3" });
            ViewBag.TiposEmbalagem = new SelectList(tipoEmbalagem, "Value", "Text");

            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            PRODUTO item = new PRODUTO();
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            vm.ASSI_CD_ID = SessionMocks.IdAssinante;
            vm.PROD_DT_CADASTRO = DateTime.Today;
            vm.PROD_IN_ATIVO = 1;
            vm.MATR_CD_ID = SessionMocks.Matriz.MATR_CD_ID;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirProduto(ProdutoViewModel vm, String tabelaProduto)
        {
            vm.PROD_QN_QUANTIDADE_INICIAL = 0;

            ViewBag.Tipos = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoProduto = new List<SelectListItem>();
            tipoProduto.Add(new SelectListItem() { Text = "Simples", Value = "1" });
            tipoProduto.Add(new SelectListItem() { Text = "Kit", Value = "2" });
            tipoProduto.Add(new SelectListItem() { Text = "Fabricado", Value = "3" });
            ViewBag.TiposProduto = new SelectList(tipoProduto, "Value", "Text");
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
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = prodApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Acerta codigo do produto
                    item.PROD_CD_CODIGO = item.PROD_CD_ID.ToString();
                    volta = prodApp.ValidateEdit(item, item, usuarioLogado);

                    // Carrega foto e processa alteracao
                    item.PROD_AQ_FOTO = "~/Imagens/Base/FotoBase.jpg";
                    volta = prodApp.ValidateEdit(item, item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/QR/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    if (item.PROD_IN_COMPOSTO == 0)
                    {
                        listaMasterProd = new List<PRODUTO>();
                        SessionMocks.listaProduto = null;
                        if (SessionMocks.voltaProduto == 2)
                        {
                            return RedirectToAction("VerCardsProduto");
                        }

                        vm.PROD_CD_ID = item.PROD_CD_ID;

                        IncluirTabelaProduto(vm, tabelaProduto);

                        SessionMocks.idVolta = item.PROD_CD_ID;
                        return Json("1");
                    }
                    else
                    {
                        SessionMocks.idProduto = item.PROD_CD_ID;
                        SessionMocks.VoltaComposto = 1;
                        return RedirectToAction("IncluirFT", "FichaTecnica");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return Json(ex.Message);
                }
            }
            else
            {
                return View(vm);
            }
        }

        public ActionResult ClonarProduto(Int32 id)
        {
            // Prepara objeto
            USUARIO usuario = SessionMocks.UserCredentials;
            PRODUTO item = prodApp.GetItemById(id);
            PRODUTO novo = new PRODUTO();
            novo.ASSI_CD_ID = item.ASSI_CD_ID;
            novo.CAPR_CD_ID = item.CAPR_CD_ID;
            novo.FILI_CD_ID = item.FILI_CD_ID;
            novo.MATR_CD_ID = item.MATR_CD_ID;
            novo.PROD_AQ_FOTO = item.PROD_AQ_FOTO;
            novo.PROD_CD_GTIN_EAN = item.PROD_CD_GTIN_EAN;
            novo.PROD_DS_DESCRICAO = item.PROD_DS_DESCRICAO;
            novo.PROD_DS_INFORMACAO_NUTRICIONAL = item.PROD_DS_INFORMACAO_NUTRICIONAL;
            novo.PROD_DS_INFORMACOES = item.PROD_DS_INFORMACOES;
            novo.PROD_DT_CADASTRO = DateTime.Today;
            novo.PROD_IN_ATIVO = 1;
            novo.PROD_IN_AVISA_MINIMO = item.PROD_IN_AVISA_MINIMO;
            novo.PROD_IN_BALANCA_PDV = item.PROD_IN_BALANCA_PDV;
            novo.PROD_IN_BALANCA_RETAGUARDA = item.PROD_IN_BALANCA_RETAGUARDA;
            novo.PROD_IN_COBRAR_MAIOR = item.PROD_IN_COBRAR_MAIOR;
            novo.PROD_IN_DIVISAO = item.PROD_IN_DIVISAO;
            novo.PROD_IN_GERAR_ARQUIVO = item.PROD_IN_GERAR_ARQUIVO;
            novo.PROD_IN_OPCAO_COMBO = item.PROD_IN_OPCAO_COMBO;
            novo.PROD_IN_TIPO_COMBO = item.PROD_IN_TIPO_COMBO;
            novo.PROD_IN_TIPO_EMBALAGEM = item.PROD_IN_TIPO_EMBALAGEM;
            novo.PROD_IN_TIPO_PRODUTO = item.PROD_IN_TIPO_PRODUTO;
            novo.PROD_NM_LOCALIZACAO_ESTOQUE = item.PROD_NM_LOCALIZACAO_ESTOQUE;
            novo.PROD_NM_NOME = "====== PRODUTO DUPLICADO ======";
            novo.PROD_NM_ORIGEM = item.PROD_NM_ORIGEM;
            novo.PROD_NR_ALTURA = item.PROD_NR_ALTURA;
            novo.PROD_NR_COMPRIMENTO = item.PROD_NR_COMPRIMENTO;
            novo.PROD_NR_DIAMETRO = item.PROD_NR_DIAMETRO;
            novo.PROD_NR_DIAS_VALIDADE = item.PROD_NR_DIAS_VALIDADE;
            novo.PROD_NR_GARANTIA = item.PROD_NR_GARANTIA;
            novo.PROD_NR_LARGURA = item.PROD_NR_LARGURA;
            novo.PROD_NR_NCM = item.PROD_NR_NCM;
            novo.PROD_NR_REFERENCIA = item.PROD_NR_REFERENCIA;
            novo.PROD_NR_VALIDADE = item.PROD_NR_VALIDADE;
            novo.PROD_PC_MARKUP_MININO = item.PROD_PC_MARKUP_MININO;
            novo.PROD_QN_ESTOQUE = 0;
            novo.PROD_QN_PESO_BRUTO = item.PROD_QN_PESO_BRUTO;
            novo.PROD_QN_PESO_LIQUIDO = item.PROD_QN_PESO_LIQUIDO;
            novo.PROD_QN_QUANTIDADE_INICIAL = 0;
            novo.PROD_QN_QUANTIDADE_MAXIMA = 0;
            novo.PROD_QN_QUANTIDADE_MINIMA = 0;
            novo.PROD_QN_RESERVA_ESTOQUE = 0;
            novo.PROD_TX_OBSERVACOES = item.PROD_TX_OBSERVACOES;
            novo.PROD_VL_CUSTO = item.PROD_VL_CUSTO;
            novo.PROD_VL_MARKUP_PADRAO = item.PROD_VL_MARKUP_PADRAO;
            novo.PROD_VL_PRECO_MINIMO = item.PROD_VL_PRECO_MINIMO;
            novo.PROD_VL_PRECO_PROMOCAO = item.PROD_VL_PRECO_PROMOCAO;
            novo.PROD_VL_PRECO_VENDA = item.PROD_VL_PRECO_VENDA;
            novo.SCPR_CD_ID = item.SCPR_CD_ID;
            novo.UNID_CD_ID = item.UNID_CD_ID;
            novo.PROD_NR_BARCODE = item.PROD_NR_BARCODE;
            novo.PROD_QR_QRCODE = item.PROD_QR_QRCODE;

            Int32 volta = prodApp.ValidateCreateLeve(novo, usuario);
            SessionMocks.idVolta = novo.PROD_CD_ID;
            SessionMocks.clonar = 1;

            // Acerta codigo do produto
            novo.PROD_CD_CODIGO = novo.PROD_CD_ID.ToString();
            volta = prodApp.ValidateEdit(novo, novo, usuario);

            // Carrega foto e processa alteracao
            novo.PROD_AQ_FOTO = "~/Imagens/Base/FotoBase.jpg";
            volta = prodApp.ValidateEdit(novo, novo, usuario);

            // Cria pastas
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + novo.PROD_CD_ID.ToString() + "/Fotos/";
            Directory.CreateDirectory(Server.MapPath(caminho));
            caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + novo.PROD_CD_ID.ToString() + "/Anexos/";
            Directory.CreateDirectory(Server.MapPath(caminho));
            caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + novo.PROD_CD_ID.ToString() + "/QR/";
            Directory.CreateDirectory(Server.MapPath(caminho));
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult EditarProduto(Int32 id)
        {
            // Prepara view
            PRODUTO item = prodApp.GetItemById(id);
            ViewBag.Tipos = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoProduto = new List<SelectListItem>();
            tipoProduto.Add(new SelectListItem() { Text = "Simples", Value = "1" });
            tipoProduto.Add(new SelectListItem() { Text = "Kit", Value = "2" });
            tipoProduto.Add(new SelectListItem() { Text = "Fabricado", Value = "3" });
            ViewBag.TiposProduto = new SelectList(tipoProduto, "Value", "Text");
            List<SelectListItem> tipoEmbalagem = new List<SelectListItem>();
            tipoEmbalagem.Add(new SelectListItem() { Text = "Envelope", Value = "1" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Caixa", Value = "2" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Rolo", Value = "3" });
            ViewBag.TiposEmbalagem = new SelectList(tipoEmbalagem, "Value", "Text");

            Int32 venda = item.ITEM_PEDIDO_VENDA.Where(p => p.PEDIDO_VENDA.PEVE_DT_DATA.Month == DateTime.Today.Month).ToList().Sum(m => m.ITPE_QN_QUANTIDADE);
            ViewBag.Vendas = venda;

            // Recupera preços

            // Exibe mensagem
            if (item.PROD_IN_COMPOSTO == 1 & item.FICHA_TECNICA.Count == 0)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0098", CultureInfo.CurrentCulture));
            }

            if ((Int32)Session["MensProduto"] == 1)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0099", CultureInfo.CurrentCulture));
            }

            objetoProdAntes = item;
            SessionMocks.produto = item;
            SessionMocks.idVolta = id;
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarProduto(ProdutoViewModel vm)
        {
            ViewBag.Tipos = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoProduto = new List<SelectListItem>();
            tipoProduto.Add(new SelectListItem() { Text = "Simples", Value = "1" });
            tipoProduto.Add(new SelectListItem() { Text = "Kit", Value = "2" });
            tipoProduto.Add(new SelectListItem() { Text = "Fabricado", Value = "3" });
            ViewBag.TiposProduto = new SelectList(tipoProduto, "Value", "Text");
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
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                    Int32 volta = prodApp.ValidateEdit(item, objetoProdAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterProd = new List<PRODUTO>();
                    SessionMocks.listaProduto = null;
                    if ((Int32)Session["VoltaEstoque"] == 1)
                    {
                        return RedirectToAction("MontarTelaEstoqueProduto", "Estoque");
                    }
                    if (SessionMocks.voltaProduto == 2)
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
            // Prepara view
            PRODUTO item = prodApp.GetItemById(id);
            ViewBag.Tipos = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Origens = new SelectList(prodApp.GetAllOrigens(), "PROR_CD_ID", "PROR_NM_NOME");
            List<SelectListItem> tipoProduto = new List<SelectListItem>();
            tipoProduto.Add(new SelectListItem() { Text = "Simples", Value = "1" });
            tipoProduto.Add(new SelectListItem() { Text = "Kit", Value = "2" });
            tipoProduto.Add(new SelectListItem() { Text = "Fabricado", Value = "3" });
            ViewBag.TiposProduto = tipoProduto.Where(x => Convert.ToInt32(x.Value) == item.PROD_IN_TIPO_PRODUTO).Select(x => x.Text).FirstOrDefault();
            List<SelectListItem> tipoEmbalagem = new List<SelectListItem>();
            tipoEmbalagem.Add(new SelectListItem() { Text = "Envelope", Value = "1" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Caixa", Value = "2" });
            tipoEmbalagem.Add(new SelectListItem() { Text = "Rolo", Value = "3" });
            ViewBag.TiposEmbalagem = tipoEmbalagem.Where(x => Convert.ToInt32(x.Value) == item.PROD_IN_TIPO_EMBALAGEM).Select(x => x.Text).FirstOrDefault();

            Int32 venda = item.ITEM_PEDIDO_VENDA.Where(p => p.PEDIDO_VENDA.PEVE_DT_DATA.Month == DateTime.Today.Month).ToList().Sum(m => m.ITPE_QN_QUANTIDADE);
            ViewBag.Vendas = venda;

            var usuario = new USUARIO();
            ViewBag.PerfilUsuario = usuario.PERF_CD_ID;
            ViewBag.CdUsuario = usuario.USUA_CD_ID;

            // Recupera preços

            // Exibe mensagem
            if (item.PROD_IN_COMPOSTO == 1 & item.FICHA_TECNICA.Count == 0)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0098", CultureInfo.CurrentCulture));
            }

            if ((Int32)Session["MensProduto"] == 1)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0099", CultureInfo.CurrentCulture));
            }

            objetoProdAntes = item;
            SessionMocks.produto = item;
            SessionMocks.idVolta = id;
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        // Filtro em cascata de subcategoria
        [HttpPost]
        public JsonResult FiltrarSubCategoriaProduto(Int32? id)
        {
            var listaSubFiltrada = new List<SUBCATEGORIA_PRODUTO>();

            // Filtro para caso o placeholder seja selecionado
            if (id == null)
            {
                listaSubFiltrada = prodApp.GetAllSubs();
            }
            else
            {
                listaSubFiltrada = prodApp.GetAllSubs().Where(x => x.CATEGORIA_PRODUTO.CAPR_CD_ID == id).ToList();
            }

            return Json(listaSubFiltrada.Select(x => new { x.SCPR_CD_ID, x.SCPR_NM_NOME }));
        }

        [HttpGet]
        public ActionResult ExcluirProduto(Int32 id)
        {
            // Prepara view
            PRODUTO item = prodApp.GetItemById(id);
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirProduto(ProdutoViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                Int32 volta = prodApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0077", CultureInfo.CurrentCulture));
                    return View(vm);
                }

                // Sucesso
                listaMasterProd = new List<PRODUTO>();
                SessionMocks.listaProduto = null;
                return RedirectToAction("MontarTelaProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarProduto(Int32 id)
        {
            // Prepara view
            PRODUTO item = prodApp.GetItemById(id);
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarProduto(ProdutoViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                Int32 volta = prodApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMasterProd = new List<PRODUTO>();
                SessionMocks.listaProduto = null;
                return RedirectToAction("MontarTelaProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult VerCardsProduto()
        {
            // Carrega listas
            if (SessionMocks.listaProduto == null)
            {
                listaMasterProd = prodApp.GetAllItens();
                SessionMocks.listaProduto = listaMasterProd;
            }
            ViewBag.Listas = SessionMocks.listaProduto;
            ViewBag.Title = "Produtos";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Tipos = new SelectList(SessionMocks.CatsProduto, "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.Subs = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");

            // Indicadores
            ViewBag.Produtos = listaMasterProd.Count;

            // Abre view
            objetoProd = new PRODUTO();
            SessionMocks.voltaProduto = 2;
            return View(objetoProd);
        }

        [HttpGet]
        public ActionResult VerAnexoProduto(Int32 id)
        {
            // Prepara view
            PRODUTO_ANEXO item = prodApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoProduto()
        {
            return RedirectToAction("EditarProduto", new { id = SessionMocks.idVolta });
        }

        public FileResult DownloadProduto(Int32 id)
        {
            PRODUTO_ANEXO item = prodApp.GetAnexoById(id);
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
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }

            PRODUTO item = prodApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/Anexos/";
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
            foto.PRAN_IN_TIPO = tipo;
            foto.PRAN_NM_TITULO = fileName;
            foto.PROD_CD_ID = item.PROD_CD_ID;

            item.PRODUTO_ANEXO.Add(foto);
            objetoProdAntes = item;
            Int32 volta = prodApp.ValidateEdit(item, objetoProdAntes);
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

            return Json("1"); // VoltarAnexoCliente
        }

        [HttpPost]
        public ActionResult UploadFotoProduto(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }

            PRODUTO item = prodApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/Fotos/";
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
                item.PROD_AQ_FOTO = "~" + caminho + fileName;
                objetoProd = item;
                Int32 volta = prodApp.ValidateEdit(item, objetoProd);
            }
            else
            {
                ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture);
            }
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpPost]
        public ActionResult UploadQRCodeProduto(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }

            PRODUTO item = prodApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoProduto");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Produtos/" + item.PROD_CD_ID.ToString() + "/QR/";
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
                objetoProd = item;
                Int32 volta = prodApp.ValidateEdit(item, objetoProd);
            }
            else
            {
                ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture);
            }
            return RedirectToAction("VoltarAnexoProduto");
        }

        public ActionResult BuscarCEPProduto(PRODUTO item)
        {
            return RedirectToAction("IncluirProdutoEspecial", new { objeto = item});
        }

        public ActionResult VerMovimentacaoEstoqueProduto()
        {
            // Prepara view
            PRODUTO item = prodApp.GetItemById(SessionMocks.idVolta);
            objetoProdAntes = item;
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        public ActionResult VerProdutosPontoPedido()
        {
            // Prepara view
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");

            ViewBag.PontoPedidos = (List<PRODUTO_ESTOQUE_FILIAL>)Session["PontoPedido"];
            ViewBag.PontoPedido = ((List<PRODUTO_ESTOQUE_FILIAL>)Session["PontoPedido"]).Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            Session["ListaEstoque"] = (List<PRODUTO_ESTOQUE_FILIAL>)Session["PontoPedido"];

            // Abre view
            PRODUTO_ESTOQUE_FILIAL prod = new PRODUTO_ESTOQUE_FILIAL();
            SessionMocks.voltaProduto = 1;
            SessionMocks.voltaConsulta = 2;
            SessionMocks.clonar = 0;
            Session["VoltaConsulta"] = 2;
            Session["FiltroEstoque"] = null;
            ViewBag.Tipo = 2;
            return View(prod);
        }

        public ActionResult RetirarFiltroProdutoPontoPedido()
        {
            return RedirectToAction("VerProdutosPontoPedido");
        }

        public ActionResult RetirarFiltroProdutoEstoqueNegativo()
        {
            return RedirectToAction("VerProdutosEstoqueNegativo");
        }

        public ActionResult VerProdutosEstoqueZerado()
        {
            // Prepara view
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");

            ViewBag.EstoqueZerados = (List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueZerado"];
            ViewBag.EstoqueZerado = ((List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueZerado"]).Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            Session["ListaEstoque"] = (List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueZerado"];

            // Abre view
            PRODUTO_ESTOQUE_FILIAL prod = new PRODUTO_ESTOQUE_FILIAL();
            SessionMocks.voltaProduto = 1;
            SessionMocks.voltaConsulta = 3;
            SessionMocks.clonar = 0;
            Session["VoltaConsulta"] = 3;
            Session["FiltroEstoque"] = null;
            ViewBag.Tipo = 3;
            return View(prod);
        }

        public ActionResult VerProdutosEstoqueNegativo()
        {
            // Prepara view
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");

            ViewBag.EstoqueNegativos = (List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueNegativo"];
            ViewBag.EstoqueNegativo = ((List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueNegativo"]).Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            Session["ListaEstoque"] = (List<PRODUTO_ESTOQUE_FILIAL>)Session["EstoqueNegativo"];

            // Abre view
            PRODUTO_ESTOQUE_FILIAL prod = new PRODUTO_ESTOQUE_FILIAL();
            SessionMocks.voltaProduto = 1;
            SessionMocks.voltaConsulta = 2;
            SessionMocks.clonar = 0;
            Session["VoltaConsulta"] = 4;
            Session["FiltroEstoque"] = null;
            ViewBag.Tipo = 4;
            return View(prod);
        }

        public ActionResult RetirarFiltroProdutoEstoqueZerado()
        {
            return RedirectToAction("VerProdutosEstoqueZerado");
        }

        [HttpGet]
        public ActionResult EditarProdutoFornecedor(Int32 id)
        {
            // Prepara view
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(), "FORN_CD_ID", "FORN_NM_NOME");
            PRODUTO_FORNECEDOR item = prodApp.GetFornecedorById(id);
            objetoProdAntes = SessionMocks.produto;
            ProdutoFornecedorViewModel vm = Mapper.Map<PRODUTO_FORNECEDOR, ProdutoFornecedorViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarProdutoFornecedor(ProdutoFornecedorViewModel vm)
        {
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(), "FORN_CD_ID", "FORN_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
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
            PRODUTO_FORNECEDOR item = prodApp.GetFornecedorById(id);
            objetoProdAntes = SessionMocks.produto;
            item.PRFO_IN_ATIVO = 0;
            Int32 volta = prodApp.ValidateEditFornecedor(item);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult ReativarProdutoFornecedor(Int32 id)
        {
            PRODUTO_FORNECEDOR item = prodApp.GetFornecedorById(id);
            objetoProdAntes = SessionMocks.produto;
            item.PRFO_IN_ATIVO = 1;
            Int32 volta = prodApp.ValidateEditFornecedor(item);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult IncluirProdutoFornecedor()
        {
            // Prepara view
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(), "FORN_CD_ID", "FORN_NM_NOME");
            USUARIO usuario = SessionMocks.UserCredentials;
            PRODUTO_FORNECEDOR item = new PRODUTO_FORNECEDOR();
            ProdutoFornecedorViewModel vm = Mapper.Map<PRODUTO_FORNECEDOR, ProdutoFornecedorViewModel>(item);
            vm.PROD_CD_ID = SessionMocks.idVolta;
            vm.PRFO_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirProdutoFornecedor(ProdutoFornecedorViewModel vm)
        {
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(), "FORN_CD_ID", "FORN_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PRODUTO_FORNECEDOR item = Mapper.Map<ProdutoFornecedorViewModel, PRODUTO_FORNECEDOR>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
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

        [HttpGet]
        public ActionResult EditarProdutoGrade(Int32 id)
        {
            // Prepara view
            ViewBag.Tamanhos = new SelectList(prodApp.GetAllTamanhos(), "TAMA_CD_ID", "TAMA_NM_NOME");
            PRODUTO_GRADE item = prodApp.GetGradeById(id);
            objetoProdAntes = SessionMocks.produto;
            ProdutoGradeViewModel vm = Mapper.Map<PRODUTO_GRADE, ProdutoGradeViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarProdutoGrade(ProdutoGradeViewModel vm)
        {
            ViewBag.Tamanhos = new SelectList(prodApp.GetAllTamanhos(), "TAMA_CD_ID", "TAMA_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    PRODUTO_GRADE item = Mapper.Map<ProdutoGradeViewModel, PRODUTO_GRADE>(vm);
                    Int32 volta = prodApp.ValidateEditGrade(item);

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
        public ActionResult ExcluirProdutoGrade(Int32 id)
        {
            PRODUTO_GRADE item = prodApp.GetGradeById(id);
            objetoProdAntes = SessionMocks.produto;
            item.PRGR_IN_ATIVO = 0;
            Int32 volta = prodApp.ValidateEditGrade(item);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult ReativarProdutoGrade(Int32 id)
        {
            PRODUTO_GRADE item = prodApp.GetGradeById(id);
            objetoProdAntes = SessionMocks.produto;
            item.PRGR_IN_ATIVO = 1;
            Int32 volta = prodApp.ValidateEditGrade(item);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult IncluirProdutoGrade()
        {
            // Prepara view
            ViewBag.Tamanhos = new SelectList(prodApp.GetAllTamanhos(), "TAMA_CD_ID", "TAMA_NM_NOME");
            USUARIO usuario = SessionMocks.UserCredentials;
            PRODUTO_GRADE item = new PRODUTO_GRADE();
            ProdutoGradeViewModel vm = Mapper.Map<PRODUTO_GRADE, ProdutoGradeViewModel>(item);
            vm.PROD_CD_ID = SessionMocks.idVolta;
            vm.PRGR_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirProdutoGrade(ProdutoGradeViewModel vm)
        {
            ViewBag.Tamanhos = new SelectList(prodApp.GetAllTamanhos(), "TAMA_CD_ID", "TAMA_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PRODUTO_GRADE item = Mapper.Map<ProdutoGradeViewModel, PRODUTO_GRADE>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = prodApp.ValidateCreateGrade(item);
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
        public ActionResult SlideShow()
        {
            // Prepara view
            PRODUTO item = prodApp.GetItemById(SessionMocks.idVolta);
            objetoProdAntes = item;
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            return View(vm);
        }

        public ActionResult GerarRelatorioFiltro()
        {
            return RedirectToAction("GerarRelatorioLista", new { id = 1 });
        }

        public ActionResult GerarRelatorioZerado()
        {
            return RedirectToAction("GerarRelatorioEstoque", new { id = 2 });
        }

        public ActionResult GerarRelatorioPonto()
        {
            return RedirectToAction("GerarRelatorioEstoque", new { id = 1 });
        }

        public ActionResult GerarRelatorioNegativo()
        {
            return RedirectToAction("GerarRelatorioEstoque", new { id = 3 });
        }

        public ActionResult GerarRelatorioLista(Int32 id)
        {
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
                lista = SessionMocks.listaProduto;
            }
            else
            {
                return RedirectToAction("MontarTelaProduto");
            }
            PRODUTO filtro = SessionMocks.filtroProduto;
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
            table = new PdfPTable(new float[] { 100f, 100f, 150f, 80f, 40f, 100f, 100f, 40f});
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Produtos selecionados pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
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
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.SUBCATEGORIA_PRODUTO.SCPR_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
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
                if (filtro.SCPR_CD_ID > 0)
                {
                    if (ja == 0)
                    {
                        parametros += "Subcategoria: " + filtro.SCPR_CD_ID.ToString();
                        ja = 1;
                    }
                    else
                    {
                        parametros += "e Subcategoria: " + filtro.SCPR_CD_ID.ToString();
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
            table = new PdfPTable(new float[] { 90f, 100f, 100f, 150f, 80f, 40f, 100f, 100f, 50f, 40f});
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Produtos selecionados pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
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
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.PRODUTO.SUBCATEGORIA_PRODUTO.SCPR_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
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
            // Prepara geração
            PRODUTO aten = prodApp.GetItemById(SessionMocks.idVolta);
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
            cell = new PdfPCell(new Paragraph("Subcategoria: " + aten.SUBCATEGORIA_PRODUTO.SCPR_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            if (aten.PROD_IN_COMPOSTO == 1)
            {
                cell = new PdfPCell(new Paragraph("Composto? Sim ",meuFont));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Composto? Não ", meuFont));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Paragraph("Subcategoria: " + aten.SUBCATEGORIA_PRODUTO.SCPR_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
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
            cell.Colspan = 2;
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
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Unidade: -", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            //if (aten.PROD_VL_PRECO_VENDA != null)
            //{
            //    cell = new PdfPCell(new Paragraph("Preço de Venda: R$ " + CrossCutting.Formatters.DecimalFormatter(aten.PROD_VL_PRECO_VENDA.Value), meuFont));
            //    cell.Border = 0;
            //    cell.Colspan = 1;
            //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
            //    table.AddCell(cell);
            //}
            //else
            //{
            //    cell = new PdfPCell(new Paragraph("Preço de Venda: -", meuFont));
            //    cell.Border = 0;
            //    cell.Colspan = 1;
            //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
            //    table.AddCell(cell);
            //}
            //if (aten.PROD_VL_PRECO_PROMOCAO != null)
            //{
            //    cell = new PdfPCell(new Paragraph("Preço de Promõção: R$ " + CrossCutting.Formatters.DecimalFormatter(aten.PROD_VL_PRECO_PROMOCAO.Value), meuFont));
            //    cell.Border = 0;
            //    cell.Colspan = 1;
            //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
            //    table.AddCell(cell);
            //}
            //else
            //{
            //    cell = new PdfPCell(new Paragraph("Preço de Promoção: -", meuFont));
            //    cell.Border = 0;
            //    cell.Colspan = 1;
            //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
            //    table.AddCell(cell);
            //}
            if (aten.PROD_NR_GARANTIA != null)
            {
                cell = new PdfPCell(new Paragraph("Garantia (dias): " + aten.PROD_NR_GARANTIA.Value.ToString(), meuFont));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Garantia (dias): ", meuFont));
                cell.Border = 0;
                cell.Colspan = 3;
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

            //cell = new PdfPCell(new Paragraph("Estoque Atual: " + aten.PROD_QN_ESTOQUE, meuFont));
            //cell.Border = 0;
            //cell.Colspan = 2;
            //cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            //cell.HorizontalAlignment = Element.ALIGN_LEFT;
            //table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Estoque Inicial: " + aten.PROD_QN_QUANTIDADE_INICIAL, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
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
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Reserva de Estoque: " + aten.PROD_QN_RESERVA_ESTOQUE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            if (aten.PROD_DT_ULTIMA_MOVIMENTACAO != null)
            {
                cell = new PdfPCell(new Paragraph("Último Movimento: " + aten.PROD_DT_ULTIMA_MOVIMENTACAO.Value.ToShortDateString(), meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            cell = new PdfPCell(new Paragraph(" " , meuFont));
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

            // Ficha Técnica
            if (aten.PROD_IN_COMPOSTO == 1)
            {
                if (aten.FICHA_TECNICA.Count > 0)
                {
                    table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
                    table.WidthPercentage = 100;
                    table.HorizontalAlignment = 0;
                    table.SpacingBefore = 1f;
                    table.SpacingAfter = 1f;

                    cell = new PdfPCell(new Paragraph("Ficha Técnica", meuFontBold));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(cell);

                    if (System.IO.File.Exists(Server.MapPath(aten.FICHA_TECNICA.FirstOrDefault().FITE_AQ_APRESENTACAO)))
                    {
                        cell = new PdfPCell();
                        cell.Border = 0;
                        cell.Colspan = 2;
                        image = Image.GetInstance(Server.MapPath(aten.FICHA_TECNICA.FirstOrDefault().FITE_AQ_APRESENTACAO));
                        image.ScaleAbsolute(100, 100);
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

                    cell = new PdfPCell(new Paragraph("Nome: " + aten.FICHA_TECNICA.FirstOrDefault().FITE_NM_NOME, meuFont));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph("Modo de Preparo: " + aten.FICHA_TECNICA.FirstOrDefault().FITE_S_DESCRICAO, meuFont));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Paragraph("Modo de Apresentação: " + aten.FICHA_TECNICA.FirstOrDefault().FITE_DS_APRESENTACAO, meuFont));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(cell);
                    pdfDoc.Add(table);

                    if (aten.FICHA_TECNICA.FirstOrDefault().FICHA_TECNICA_DETALHE.Count > 0)
                    {
                        table = new PdfPTable(new float[] { 120f, 90f, 90f, 90f });
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
                        cell = new PdfPCell(new Paragraph("Unidade", meuFont))
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
                        cell = new PdfPCell(new Paragraph("Imagem", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);

                        foreach (FICHA_TECNICA_DETALHE item in aten.FICHA_TECNICA.FirstOrDefault().FICHA_TECNICA_DETALHE)
                        {
                            cell = new PdfPCell(new Paragraph(item.MATERIA_PRIMA.MAPR_NM_NOME, meuFont))
                            {
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                HorizontalAlignment = Element.ALIGN_LEFT
                            };
                            table.AddCell(cell);
                            cell = new PdfPCell(new Paragraph(item.MATERIA_PRIMA.UNIDADE.UNID_NM_NOME, meuFont))
                            {
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                HorizontalAlignment = Element.ALIGN_LEFT
                            };
                            table.AddCell(cell);
                            cell = new PdfPCell(new Paragraph(item.FITD_QN_QUANTIDADE.ToString(), meuFont))
                            {
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                HorizontalAlignment = Element.ALIGN_LEFT
                            };
                            table.AddCell(cell);
                            if (System.IO.File.Exists(Server.MapPath(item.MATERIA_PRIMA.MAPR_AQ_FOTO)))
                            {
                                cell = new PdfPCell();
                                image = Image.GetInstance(Server.MapPath(item.MATERIA_PRIMA.MAPR_AQ_FOTO));
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
                    }

                }
            }

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            //Varejo
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Varejo", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Referência: " + aten.PROD_NR_REFERENCIA, meuFont));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            if (aten.PRODUTO_GRADE.Count > 0)
            {
                table = new PdfPTable(new float[] { 120f, 80f, 90f, 50f });
                table.WidthPercentage = 100;
                table.HorizontalAlignment = 0;
                table.SpacingBefore = 1f;
                table.SpacingAfter = 1f;

                cell = new PdfPCell(new Paragraph("Cor", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Tamanho", meuFont))
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
                cell = new PdfPCell(new Paragraph("Ativo", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                foreach (PRODUTO_GRADE item in aten.PRODUTO_GRADE)
                {
                    cell = new PdfPCell(new Paragraph(item.PRGR_NM_COR, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.PRGR_NM_TAMANHO, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.PRGR_QN_QUANTIDADE.ToString(), meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.PRGR_IN_ATIVO == 1)
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

            //Food Service
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Food Service", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (aten.PROD_IN_BALANCA_PDV == 1)
            {
                cell = new PdfPCell(new Paragraph("Balança PDV: Sim", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Balança PDV: Não", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            if (aten.PROD_IN_BALANCA_RETAGUARDA == 1)
            {
                cell = new PdfPCell(new Paragraph("Balança Retaguarda: Sim", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Balança Retaguarda: Não", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            if (aten.PROD_IN_TIPO_COMBO == 1)
            {
                cell = new PdfPCell(new Paragraph("Produto Tipo Combo: Sim", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Produto Tipo Combo: Não", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            if (aten.PROD_IN_OPCAO_COMBO == 1)
            {
                cell = new PdfPCell(new Paragraph("Produto Opção Combo: Sim", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Produto Opção Combo: Não", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Paragraph("Validade (Dias): " + aten.PROD_NR_VALIDADE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Divisão: " + aten.PROD_IN_DIVISAO, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            if (aten.PROD_IN_COBRAR_MAIOR == 1)
            {
                cell = new PdfPCell(new Paragraph("Cobrar pelo Maior: Sim", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Cobrar pelo Maior: Não", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            if (aten.PROD_IN_GERAR_ARQUIVO == 1)
            {
                cell = new PdfPCell(new Paragraph("Gerar Arquivo Texto: Sim", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Gerar Arquivo Texto: Não", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Paragraph("Informações Nutricionais: " + aten.PROD_DS_INFORMACAO_NUTRICIONAL, meuFont));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

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
                cell.Colspan = 4 ;
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
                cell.Colspan = 4;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                pdfDoc.Add(table);

                // Movimentos
                table = new PdfPTable(new float[] { 80f, 80f, 80f});
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

                foreach (MOVIMENTO_ESTOQUE_PRODUTO item in aten.MOVIMENTO_ESTOQUE_PRODUTO.OrderByDescending(a => a.MOEP_DT_MOVIMENTO).Take(10))
                {
                    cell = new PdfPCell(new Paragraph(item.MOEP_DT_MOVIMENTO.ToShortDateString(), meuFont))
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
            try
            {
                // Executa a operação
                Int32 idAss = SessionMocks.IdAssinante;
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                Int32 volta = prodApp.IncluirTabelaPreco(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensProduto"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0099", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoProduto");
                }

                // Sucesso
                return RedirectToAction("VoltarAnexoProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("VoltarAnexoProduto");
            }
        }

        public void IncluirTabelaProduto(ProdutoViewModel vm, String tabelaProduto)
        {
            var jArray = JArray.Parse(tabelaProduto);

            foreach (var jObject in jArray)
            {
                try
                {
                    // Executa a operação
                    Int32 idAss = SessionMocks.IdAssinante;
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);

                    item.FILI_CD_ID = (Int32)jObject["fili_cd_id"];
                    item.PRTP_VL_PRECO = (Int32)jObject["preco"];
                    item.PRTP_VL_PRECO_PROMOCAO = (Int32)jObject["precoPromo"];

                    item.FILIAL = filApp.GetById(item.FILI_CD_ID);
                    Int32 volta = prodApp.IncluirTabelaPreco(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        //Session["MensProduto"] = 1;
                        Session["MensProduto"] = 1;
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0108", CultureInfo.CurrentCulture));
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
            // Verifica se tem usuario logado
            Int32 idAss = SessionMocks.IdAssinante;
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
            PRODUTO_TABELA_PRECO item = tpApp.GetItemById(id);
            item.PRTP_IN_ATIVO = 1;
            Int32 volta = prodApp.ValidateEditTabelaPreco(item);
            return RedirectToAction("VoltarAnexoProduto");
        }

        [HttpGet]
        public ActionResult ExcluirTabelaProduto(Int32 id)
        {
            // Verifica se tem usuario logado
            Int32 idAss = SessionMocks.IdAssinante;
            USUARIO usuarioLogado = SessionMocks.UserCredentials;

            PRODUTO rot = SessionMocks.produto;
            PRODUTO_TABELA_PRECO rl = tpApp.GetItemById(id);
            Int32 volta = tpApp.ValidateDelete(rl);
            return RedirectToAction("VoltarAnexoProduto");
        }

    }
}