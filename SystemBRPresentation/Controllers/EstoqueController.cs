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
using Newtonsoft.Json.Linq;
using System.Collections;

namespace SystemBRPresentation.Controllers
{
    public class EstoqueController : Controller
    {
        private readonly IProdutoAppService prodApp;
        private readonly IMateriaPrimaAppService insApp;
        private readonly ICategoriaProdutoAppService cpApp;
        private readonly ICategoriaMateriaAppService ciApp;
        private readonly IFilialAppService filApp;
        private readonly IMovimentoEstoqueMateriaAppService moemApp;
        private readonly IMovimentoEstoqueProdutoAppService moepApp;
        private readonly ISubcategoriaMateriaAppService scmpApp;

        private String msg;
        private Exception exception;
        PRODUTO objetoProd = new PRODUTO();
        PRODUTO objetoProdAntes = new PRODUTO();
        List<PRODUTO> listaMasterProd = new List<PRODUTO>();
        MATERIA_PRIMA objetoIns = new MATERIA_PRIMA();
        MATERIA_PRIMA objetoInsAntes = new MATERIA_PRIMA();
        List<MATERIA_PRIMA> listaMasterIns = new List<MATERIA_PRIMA>();
        MOVIMENTO_ESTOQUE_PRODUTO objetoMOVP = new MOVIMENTO_ESTOQUE_PRODUTO();
        MOVIMENTO_ESTOQUE_PRODUTO objetoMOVPAntes = new MOVIMENTO_ESTOQUE_PRODUTO();
        List<MOVIMENTO_ESTOQUE_PRODUTO> listaMasterMOEP = new List<MOVIMENTO_ESTOQUE_PRODUTO>();
        MOVIMENTO_ESTOQUE_MATERIA_PRIMA objetoMOVM = new MOVIMENTO_ESTOQUE_MATERIA_PRIMA();
        MOVIMENTO_ESTOQUE_MATERIA_PRIMA objetoMOVMAntes = new MOVIMENTO_ESTOQUE_MATERIA_PRIMA();
        List<MOVIMENTO_ESTOQUE_MATERIA_PRIMA> listaMasterMOEM = new List<MOVIMENTO_ESTOQUE_MATERIA_PRIMA>();
        String extensao;

        public EstoqueController(IProdutoAppService prodApps, IMateriaPrimaAppService insApps, ICategoriaMateriaAppService ciApps, ICategoriaProdutoAppService cpApps, IFilialAppService filApps, IMovimentoEstoqueMateriaAppService moemApps, IMovimentoEstoqueProdutoAppService moepApps, ISubcategoriaMateriaAppService scmpApps)
        {
            prodApp = prodApps;
            insApp = insApps;
            ciApp = ciApps;
            cpApp = cpApps;
            filApp = filApps;
            moemApp = moemApps;
            moepApp = moepApps;
            scmpApp = scmpApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
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
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaEstoqueProduto()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
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
                listaMasterProd = listaMasterProd.Where(p => p.PROD_IN_COMPOSTO == 0).ToList();
                SessionMocks.listaProduto = listaMasterProd;
            }

            ViewBag.Listas = SessionMocks.listaProduto;

            ViewBag.Title = "Estoque";
            ViewBag.CatProd = new SelectList(SessionMocks.CatsProduto, "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.CatIns = new SelectList(SessionMocks.CatsInsumos, "CAMA_CD_ID", "CAMA_NM_NOME");

            // Indicadores
            ViewBag.Produtos = SessionMocks.listaProduto.Count;

            SessionMocks.pontoPedidoProd = SessionMocks.listaProduto.Where(p => p.PROD_QN_ESTOQUE < p.PROD_QN_QUANTIDADE_MINIMA & p.PROD_IN_ATIVO == 1 & p.ASSI_CD_ID == SessionMocks.IdAssinante).ToList();
            SessionMocks.estoqueZeradoProd = SessionMocks.listaProduto.Where(p => p.PROD_QN_ESTOQUE <= 0 & p.PROD_IN_ATIVO == 1 & p.ASSI_CD_ID == SessionMocks.IdAssinante).ToList();

            if (usuario.PERF_CD_ID == 1 || usuario.PERF_CD_ID == 2)
            {
                ViewBag.PontoPedido = SessionMocks.pontoPedidoProd.Count;
                ViewBag.EstoqueZerado = SessionMocks.estoqueZeradoProd.Count;
            }
            else
            {
                ViewBag.PontoPedido = SessionMocks.pontoPedidoProd.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList().Count;
                ViewBag.EstoqueZerado = SessionMocks.estoqueZeradoProd.Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList().Count;
            }

            if (usuario.FILIAL != null)
            {
                ViewBag.FilialUsuario = usuario.FILIAL.FILI_NM_NOME;
            }

            ViewBag.PontoPedidos = SessionMocks.pontoPedidoProd;
            ViewBag.EstoqueZerados = SessionMocks.estoqueZeradoProd;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            ViewBag.PerfilUsuario = usuario.PERF_CD_ID;
            ViewBag.FilialUsuario = usuario.FILI_CD_ID;

            // Mansagem
            if ((Int32)Session["MensEstoque"] == 1)
            {
                Session["MensEstoque"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["VoltaEstoque"] = 0;
            Session["MensEstoque"] = 0;
            objetoProd = new PRODUTO();
            return View(objetoProd);
        }

        public ActionResult RetirarFiltroProduto()
        {
            SessionMocks.listaProduto = null;
            return RedirectToAction("MontarTelaEstoqueProduto");
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
                    Session["MensEstoque"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaMasterProd = listaObj;
                SessionMocks.listaProduto = listaObj;
                return RedirectToAction("MontarTelaEstoqueProduto");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaEstoqueProduto");
            }
        }

        public ActionResult VoltarBaseProduto()
        {
            return RedirectToAction("MontarTelaEstoqueProduto");
        }

        [HttpGet]
        public ActionResult MontarTelaEstoqueInsumo()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            if (SessionMocks.listaMateria == null)
            {
                listaMasterIns = insApp.GetAllItens();
                SessionMocks.listaMateria = listaMasterIns;
            }
            
            if (usuario.PERF_CD_ID == 1 || usuario.PERF_CD_ID == 2)
            {
                ViewBag.Listas = SessionMocks.listaMateria;
            }
            else
            {
                ViewBag.Listas = SessionMocks.listaMateria.Where(x => x.FILI_CD_ID == usuario.PERF_CD_ID).ToList();
            }

            ViewBag.Title = "Estoque";
            ViewBag.CatProd = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.CatIns = new SelectList(ciApp.GetAllItens(), "CAMA_CD_ID", "CAMA_NM_NOME");

            // Indicadores
            ViewBag.Insumos = SessionMocks.listaMateria.Count;

            if (usuario.PERF_CD_ID == 1 || usuario.PERF_CD_ID == 2)
            {
                ViewBag.PontoPedido = insApp.GetPontoPedido().Count;
                ViewBag.EstoqueZerado = insApp.GetEstoqueZerado().Count;
            }
            else
            {
                ViewBag.PontoPedido = insApp.GetPontoPedido().Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList().Count;
                ViewBag.EstoqueZerado = insApp.GetEstoqueZerado().Where(x => x.FILI_CD_ID == usuario.FILI_CD_ID).ToList().Count;
            }

            if (usuario.FILIAL != null)
            {
                ViewBag.FilialUsuario = usuario.FILIAL.FILI_NM_NOME;
            }

            ViewBag.PontoPedidos = insApp.GetPontoPedido();
            ViewBag.EstoqueZerados = insApp.GetEstoqueZerado();
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            ViewBag.PerfilUsuario = usuario.PERF_CD_ID;
            ViewBag.FilialUsuario = usuario.FILI_CD_ID;

            // Mansagem
            if ((Int32)Session["MensEstoque"] == 1)
            {
                Session["MensEstoque"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["VoltaEstoque"] = 0;
            Session["MensEstoque"] = 0;
            objetoIns = new MATERIA_PRIMA();
            return View(objetoIns);
        }

        public ActionResult RetirarFiltroInsumo()
        {
            SessionMocks.listaMateria = null;
            return RedirectToAction("MontarTelaEstoqueInsumo");
        }

        [HttpPost]
        public ActionResult FiltrarInsumo(MATERIA_PRIMA item)
        {
            try
            {
                // Executa a operação
                List<MATERIA_PRIMA> listaObj = new List<MATERIA_PRIMA>();
                Int32 volta = insApp.ExecuteFilter(item.CAMA_CD_ID, item.MAPR_NM_NOME, item.MAPR_DS_DESCRICAO, item.MAPR_CD_CODIGO, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensEstoque"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaMasterIns = listaObj;
                SessionMocks.listaMateria = listaObj;
                return RedirectToAction("MontarTelaEstoqueInsumo");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaEstoqueInsumo");
            }
        }

        public ActionResult VoltarBaseInsumo()
        {
            return RedirectToAction("MontarTelaEstoqueInsumo");
        }

        [HttpGet]
        public ActionResult VerEstoqueProduto(Int32 id)
        {
            // Prepara view
            PRODUTO item = prodApp.GetItemById(id);
            Session["VoltaEstoque"] = 1;
            return RedirectToAction("EditarProduto", "Produto", new { id = id });

        }

        [HttpGet]
        public ActionResult AcertoManualProduto(Int32 id)
        {
            // Prepara view
            var usu = SessionMocks.UserCredentials;
            ViewBag.Filial = new SelectList(filApp.GetAllItens(), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.PerfilUsu = usu.PERF_CD_ID;
            PRODUTO item = prodApp.GetItemById(id);
            objetoProdAntes = item;
            SessionMocks.produto = item;
            SessionMocks.idVolta = id;
            ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
            vm.PROD_QN_NOVA_CONTAGEM = vm.PROD_QN_ESTOQUE;
            vm.PROD_DS_JUSTIFICATIVA = String.Empty;
            return View(vm);
        }

        [HttpPost]
        public ActionResult AcertoManualProduto(ProdutoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    PRODUTO item = Mapper.Map<ProdutoViewModel, PRODUTO>(vm);
                    Int32 volta = prodApp.ValidateAcertaEstoque(item, objetoProdAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterProd = new List<PRODUTO>();
                    SessionMocks.listaProduto = null;
                    return RedirectToAction("MontarTelaEstoqueProduto");
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
        public ActionResult AcertoManualInsumo(Int32 id)
        {
            // Prepara view
            var usu = SessionMocks.UserCredentials;
            ViewBag.Filial = new SelectList(filApp.GetAllItens(), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.PerfilUsu = usu.PERF_CD_ID;
            MATERIA_PRIMA item = insApp.GetItemById(id);
            objetoInsAntes = item;
            SessionMocks.materiaPrima = item;
            SessionMocks.idVolta = id;
            MateriaPrimaViewModel vm = Mapper.Map<MATERIA_PRIMA, MateriaPrimaViewModel>(item);
            vm.MAPR_QN_NOVA_CONTAGEM = vm.MAPR_QN_ESTOQUE;
            vm.MAPR_DS_JUSTIFICATIVA = String.Empty;
            return View(vm);
        }

        [HttpPost]
        public ActionResult AcertoManualInsumo(MateriaPrimaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    MATERIA_PRIMA item = Mapper.Map<MateriaPrimaViewModel, MATERIA_PRIMA>(vm);
                    Int32 volta = insApp.ValidateAcertaEstoque(item, objetoInsAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterIns = new List<MATERIA_PRIMA>();
                    SessionMocks.listaMateria = null;
                    return RedirectToAction("MontarTelaEstoqueInsumo");
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
        public ActionResult VerEstoqueInsumo(Int32 id)
        {
            // Prepara view
            MATERIA_PRIMA item = insApp.GetItemById(id);
            Session["VoltaEstoque"] = 1;
            return RedirectToAction("EditarMateria", "Insumo", new { id = id });
        }

        public ActionResult VerMovimentacaoEstoqueProduto(Int32 id)
        {
            USUARIO usu = SessionMocks.UserCredentials; 

            if (usu.PERF_CD_ID == 1 || usu.PERF_CD_ID == 2)
            {
                // Prepara view
                PRODUTO item = prodApp.GetItemById(id);
                objetoProdAntes = item;
                ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
                return View(vm);
            }
            else
            {
                // Prepara view
                PRODUTO item = prodApp.GetItemById(id);
                item.MOVIMENTO_ESTOQUE_PRODUTO = item.MOVIMENTO_ESTOQUE_PRODUTO.Where(x => x.FILI_CD_ID == usu.FILI_CD_ID).ToList();
                objetoProdAntes = item;
                ProdutoViewModel vm = Mapper.Map<PRODUTO, ProdutoViewModel>(item);
                return View(vm);
            }
        }

        public ActionResult VoltarAnexoProduto()
        {
            return RedirectToAction("VerEstoqueProduto", new { id = SessionMocks.idVolta });
        }

        public ActionResult VerMovimentacaoEstoqueInsumo(Int32 id)
        {
            USUARIO usu = SessionMocks.UserCredentials;

            if (usu.PERF_CD_ID == 1 || usu.PERF_CD_ID == 2)
            {
                // Prepara View
                MATERIA_PRIMA item = insApp.GetItemById(id);
                objetoInsAntes = item;
                MateriaPrimaViewModel vm = Mapper.Map<MATERIA_PRIMA, MateriaPrimaViewModel>(item);
                return View(vm);
            }
            else
            {
                // Prepara View
                MATERIA_PRIMA item = insApp.GetItemById(id);
                item.MOVIMENTO_ESTOQUE_MATERIA_PRIMA = item.MOVIMENTO_ESTOQUE_MATERIA_PRIMA.Where(x => x.FILI_CD_ID == usu.FILI_CD_ID).ToList();
                objetoInsAntes = item;
                MateriaPrimaViewModel vm = Mapper.Map<MATERIA_PRIMA, MateriaPrimaViewModel>(item);
                return View(vm);
            }
        }

        public ActionResult VoltarAnexoInsumo()
        {
            return RedirectToAction("VerEstoqueInsumo", new { id = SessionMocks.idVolta });
        }

        [HttpGet]
        public ActionResult MontarTelaInventario()
        {
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            if (SessionMocks.filtroInventario == null)
            {
                SessionMocks.listaMateria = null;
                SessionMocks.listaProduto = null;
            }

            ViewBag.Title = "Inventário";
            ViewBag.ListaProd = SessionMocks.listaProduto;
            ViewBag.ListaIns = SessionMocks.listaMateria;
            List<SelectListItem> filtroPM = new List<SelectListItem>();
            filtroPM.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            filtroPM.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.CatProd = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.SubCatProd = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.CatIns = new SelectList(ciApp.GetAllItens(), "CAMA_CD_ID", "CAMA_NM_NOME");
            ViewBag.SubCatIns= new SelectList(scmpApp.GetAllItens(), "SCMP_CD_ID", "SCMP_NM_NOME");
            ViewBag.ProdutoInsumo = new SelectList(filtroPM, "Value", "Text");
            ViewBag.IsProdIns = SessionMocks.filtroInventario;
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(), "FILI_CD_ID", "FILI_NM_NOME", "Selecionar");
            ViewBag.PerfilUsu = usuario.PERF_CD_ID;

            SessionMocks.filtroInventario = null;

            return View();
        }

        [HttpPost]
        public ActionResult FiltrarInventario(PRODUTO prod, MATERIA_PRIMA ins)
        {
            var retorno = new Hashtable();

            if (prod != null)
            {
                try
                {
                    // Executa a operação
                    List<PRODUTO> listaObj = new List<PRODUTO>();
                    SessionMocks.filtroProduto = prod;
                    USUARIO usuario = SessionMocks.UserCredentials;
                    if (usuario.PERF_CD_ID != 1)
                    {
                        prod.FILI_CD_ID = usuario.FILI_CD_ID;
                    }
                    Int32 volta = prodApp.ExecuteFilter(prod.CAPR_CD_ID, prod.SCPR_CD_ID, prod.PROD_NM_NOME, prod.PROD_NM_MARCA, prod.PROD_NR_BARCODE, prod.PROD_CD_CODIGO, prod.FILI_CD_ID, out listaObj);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensEstoque"] = 1;
                        retorno.Add("error", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                        return Json(retorno);
                    }

                    // Sucesso
                    listaMasterProd = listaObj;
                    if (prod.FILI_CD_ID != null)
                    {
                        SessionMocks.listaProduto = listaObj.Where(x => x.FILI_CD_ID == prod.FILI_CD_ID).ToList();
                    }
                    else
                    {
                        SessionMocks.listaProduto = listaObj;
                    }

                    SessionMocks.filtroInventario = 1;

                    retorno.Add("url", "../Estoque/MontarTelaInventario");
                    return Json(retorno);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    retorno.Add("error", ex.Message);
                    return Json(retorno);
                }
            }
            else
            {
                try
                {
                    // Executa a operação
                    List<MATERIA_PRIMA> listaObj = new List<MATERIA_PRIMA>();
                    Int32 volta = insApp.ExecuteFilter(ins.CAMA_CD_ID, ins.MAPR_NM_NOME, ins.MAPR_DS_DESCRICAO, ins.MAPR_CD_CODIGO, out listaObj);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensEstoque"] = 1;
                        retorno.Add("error", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                        return Json(retorno);
                    }

                    // Sucesso
                    listaMasterIns = listaObj;
                    if (ins.FILI_CD_ID != null)
                    {
                        SessionMocks.listaMateria = listaObj.Where(x => x.FILI_CD_ID == ins.FILI_CD_ID).ToList();
                    }
                    else
                    {
                        SessionMocks.listaMateria = listaObj;
                    }

                    SessionMocks.filtroInventario = 2;

                    retorno.Add("url", "../Estoque/MontarTelaInventario");
                    return Json(retorno);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    retorno.Add("error", ex.Message);
                    return Json(retorno);
                }
            }
        }

        [HttpPost]
        public void IncluirMovimentacaoEstoque(String grid, String tipo, String justificativa)
        {
            var usuario = SessionMocks.UserCredentials;
            var jArray = JArray.Parse(grid);

            if (tipo == "PROD")
            {
                foreach (var jObject in jArray)
                {
                    try
                    {
                        var prod = prodApp.GetById((Int32)jObject["id"]);
                        MOVIMENTO_ESTOQUE_PRODUTO moep = new MOVIMENTO_ESTOQUE_PRODUTO();
                        moep.ASSI_CD_ID = usuario.ASSI_CD_ID;
                        moep.MOEP_IN_ATIVO = 1;
                        moep.MATR_CD_ID = prod.MATR_CD_ID;
                        moep.FILI_CD_ID = prod.FILI_CD_ID;
                        moep.USUA_CD_ID = usuario.USUA_CD_ID;
                        moep.PROD_CD_ID = (Int32)jObject["id"];
                        moep.MOEP_DT_MOVIMENTO = DateTime.Now;
                        moep.MOEP_IN_ORIGEM = "INVENTARIO";

                        if ((Int32)jObject["qtde"] > prod.PROD_QN_ESTOQUE)
                        {
                            moep.MOEP_IN_TIPO_MOVIMENTO = 1;
                            moep.MOEP_QN_QUANTIDADE = (Int32)jObject["qtde"] - prod.PROD_QN_ESTOQUE;
                        }
                        else
                        {
                            moep.MOEP_IN_TIPO_MOVIMENTO = 2;
                            moep.MOEP_QN_QUANTIDADE = prod.PROD_QN_ESTOQUE - (Int32)jObject["qtde"];
                        }

                        moep.MOEP_IN_ORIGEM = "PROD";
                        moep.MOEP_DS_JUSTIFICATIVA = justificativa;

                        prod.PROD_QN_ESTOQUE = (Int32)jObject["qtde"];

                        Int32 voltaProd = prodApp.ValidateEdit(prod, prod);
                        Int32 volta = moepApp.ValidateCreate(moep, usuario);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                SessionMocks.listaProduto = prodApp.GetAllItens();
            }
            else if (tipo == "INS")
            {
                foreach (var jObject in jArray)
                {
                    try
                    {
                        var ins = insApp.GetById((Int32)jObject["id"]);
                        MOVIMENTO_ESTOQUE_MATERIA_PRIMA moem = new MOVIMENTO_ESTOQUE_MATERIA_PRIMA();
                        moem.ASSI_CD_ID = usuario.ASSI_CD_ID;
                        moem.MOEM_IN_ATIVO = 1;
                        moem.MATR_CD_ID = ins.MATR_CD_ID;
                        moem.FILI_CD_ID = ins.FILI_CD_ID;
                        moem.USUA_CD_ID = usuario.USUA_CD_ID;
                        moem.MAPR_CD_ID = (Int32)jObject["id"];
                        moem.MOEM_DT_MOVIMENTO = DateTime.Now;
                        moem.MOEM_NM_ORIGEM = "INVENTARIO";

                        if ((Int32)jObject["qtde"] > ins.MAPR_QN_ESTOQUE)
                        {
                            moem.MOEM_IN_TIPO_MOVIMENTO = 1;
                            moem.MOEM_QN_QUANTIDADE = (Int32)jObject["qtde"] - ins.MAPR_QN_ESTOQUE;
                        }
                        else
                        {
                            moem.MOEM_IN_TIPO_MOVIMENTO = 2;
                            moem.MOEM_QN_QUANTIDADE = ins.MAPR_QN_ESTOQUE - (Int32)jObject["qtde"];
                        }

                        moem.MOEM_NM_ORIGEM = "INS";
                        moem.MOEM_DS_JUSTIFICATIVA = justificativa;

                        ins.MAPR_QN_ESTOQUE = (Int32)jObject["qtde"];

                        Int32 voltaIns = insApp.ValidateEdit(ins, ins);
                        Int32 volta = moemApp.ValidateCreate(moem, usuario);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                SessionMocks.listaMateria = insApp.GetAllItens();
            }
        }

        [HttpGet]
        public ActionResult MontarTelaMovimentacaoEntrada()
        {
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            if (SessionMocks.filtroMovimentoEntrada == null)
            {
                SessionMocks.listaMovimentoProduto = null;
                SessionMocks.listaMovimentoInsumo = null;
            }

            ViewBag.Title = "Movimentações de Entrada";
            ViewBag.listaMvmtProduto = SessionMocks.listaMovimentoProduto;
            ViewBag.listaMvmtMateria = SessionMocks.listaMovimentoInsumo;
            List<SelectListItem> filtroPM = new List<SelectListItem>();
            filtroPM.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            filtroPM.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.CatProd = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.SubCatProd = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.CatIns = new SelectList(ciApp.GetAllItens(), "CAMA_CD_ID", "CAMA_NM_NOME");
            ViewBag.SubCatIns = new SelectList(scmpApp.GetAllItens(), "SCMP_CD_ID", "SCMP_NM_NOME");
            ViewBag.ProdutoInsumo = new SelectList(filtroPM, "Value", "Text");
            ViewBag.IsProdIns = SessionMocks.filtroMovimentoEntrada;
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(), "FILI_CD_ID", "FILI_NM_NOME", "Selecionar");
            ViewBag.PerfilUsu = usuario.PERF_CD_ID;

            SessionMocks.filtroMovimentoEntrada = null;

            return View();
        }

        [HttpGet]
        public ActionResult MontarTelaMovimentacaoSaida()
        {
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            if (SessionMocks.filtroMovimentoSaida == null)
            {
                SessionMocks.listaMovimentoProduto = null;
                SessionMocks.listaMovimentoInsumo = null;
            }

            ViewBag.Title = "Movimentações de Entrada";
            ViewBag.listaMvmtProduto = SessionMocks.listaMovimentoProduto;
            ViewBag.listaMvmtMateria = SessionMocks.listaMovimentoInsumo;
            List<SelectListItem> filtroPM = new List<SelectListItem>();
            filtroPM.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            filtroPM.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.CatProd = new SelectList(cpApp.GetAllItens(), "CAPR_CD_ID", "CAPR_NM_NOME");
            ViewBag.SubCatProd = new SelectList(prodApp.GetAllSubs(), "SCPR_CD_ID", "SCPR_NM_NOME");
            ViewBag.CatIns = new SelectList(ciApp.GetAllItens(), "CAMA_CD_ID", "CAMA_NM_NOME");
            ViewBag.SubCatIns = new SelectList(scmpApp.GetAllItens(), "SCMP_CD_ID", "SCMP_NM_NOME");
            ViewBag.ProdutoInsumo = new SelectList(filtroPM, "Value", "Text");
            ViewBag.IsProdIns = SessionMocks.filtroMovimentoSaida;
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(), "FILI_CD_ID", "FILI_NM_NOME", "Selecionar");
            ViewBag.PerfilUsu = usuario.PERF_CD_ID;

            SessionMocks.filtroMovimentoSaida = null;

            return View();
        }

        [HttpPost]
        public ActionResult FiltrarMovimentacao(MOVIMENTO_ESTOQUE_PRODUTO prod, MOVIMENTO_ESTOQUE_MATERIA_PRIMA ins, Int32 filtroES)
        {
            var retorno = new Hashtable();

            if (prod != null)
            {
                try
                {
                    // Executa a operação
                    List<MOVIMENTO_ESTOQUE_PRODUTO> listaObjProd = new List<MOVIMENTO_ESTOQUE_PRODUTO>();
                    Int32 volta = moepApp.ExecuteFilter(prod.PRODUTO.CAPR_CD_ID, prod.PRODUTO.SCPR_CD_ID, prod.PRODUTO.PROD_NM_NOME, prod.PRODUTO.PROD_NR_BARCODE, prod.FILI_CD_ID, prod.MOEP_DT_MOVIMENTO, out listaObjProd);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensEstoque"] = 1;
                        retorno.Add("error", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                        return Json(retorno);
                    }

                    // Filtra Entrada/Saída
                    listaObjProd = listaObjProd.Where(x => x.MOEP_IN_TIPO_MOVIMENTO == filtroES).ToList();

                    // Sucesso
                    if (listaObjProd.Count > 0)
                    {
                        SessionMocks.listaMovimentoProduto = listaObjProd;
                    }
                    else
                    {
                        retorno.Add("error", "Nenhum registro localizado");
                        return Json(retorno);
                    }

                    if (filtroES == 1)
                    {
                        SessionMocks.filtroMovimentoEntrada = 1;
                        retorno.Add("url", "../Estoque/MontarTelaMovimentacaoEntrada");
                    }
                    else
                    {
                        SessionMocks.filtroMovimentoSaida = 1;
                        retorno.Add("url", "../Estoque/MontarTelaMovimentacaoSaida");
                    }

                    return Json(retorno);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    retorno.Add("error", ex.Message);
                    return Json(retorno);
                }
            }
            else
            {
                try
                {
                    // Executa a operação
                    List<MOVIMENTO_ESTOQUE_MATERIA_PRIMA> listaObjIns = new List<MOVIMENTO_ESTOQUE_MATERIA_PRIMA>();
                    Int32 volta = moemApp.ExecuteFilter(ins.MATERIA_PRIMA.CAMA_CD_ID, ins.MATERIA_PRIMA.SCMP_CD_ID, ins.MATERIA_PRIMA.MAPR_NM_NOME, ins.FILI_CD_ID, ins.MOEM_DT_MOVIMENTO, out listaObjIns);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensEstoque"] = 1;
                        retorno.Add("error", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                        return Json(retorno);
                    }

                    // Filtra Entrada/Saída
                    listaObjIns = listaObjIns.Where(x => x.MOEM_IN_TIPO_MOVIMENTO == filtroES).ToList();

                    // Sucesso
                    if (listaObjIns.Count > 0)
                    {
                        SessionMocks.listaMovimentoInsumo = listaObjIns;
                    }
                    else
                    {
                        retorno.Add("error", "Nenhum registro localizado");
                        return Json(retorno);
                    }

                    if (filtroES == 1)
                    {
                        SessionMocks.filtroMovimentoEntrada = 2;
                        retorno.Add("url", "../Estoque/MontarTelaMovimentacaoEntrada");
                    }
                    else
                    {
                        SessionMocks.filtroMovimentoSaida = 2;
                        retorno.Add("url", "../Estoque/MontarTelaMovimentacaoSaida");
                    }

                    return Json(retorno);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    retorno.Add("error", ex.Message);
                    return Json(retorno);
                }
            }
        }

        [HttpPost]
        public void LimparListas()
        {
            SessionMocks.listaProduto = null;
            SessionMocks.listaMateria = null;
            SessionMocks.listaMovimentoProduto = null;
            SessionMocks.listaMovimentoInsumo = null;
        }
    }
}