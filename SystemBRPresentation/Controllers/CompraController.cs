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
using Newtonsoft.Json.Linq;

namespace SystemBRPresentation.Controllers
{
    public class CompraController : Controller
    {
        private readonly IPedidoCompraAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IUsuarioAppService usuApp;
        private readonly ICentroCustoAppService ccApp;
        private readonly IFornecedorAppService forApp;
        private readonly IProdutoAppService proApp;
        private readonly IMateriaPrimaAppService insApp;
        private String msg;
        private Exception exception;
        PEDIDO_COMPRA objeto = new PEDIDO_COMPRA();
        PEDIDO_COMPRA objetoAntes = new PEDIDO_COMPRA();
        List<PEDIDO_COMPRA> listaMaster = new List<PEDIDO_COMPRA>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public CompraController(IPedidoCompraAppService baseApps, ILogAppService logApps, IUsuarioAppService usuApps, ICentroCustoAppService ccApps, IFornecedorAppService forApps, IProdutoAppService proApps, IMateriaPrimaAppService insApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            usuApp = usuApps;
            ccApp = ccApps;
            forApp = forApps;
            proApp = proApps;
            insApp = insApps;
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

        public FileResult DownloadPedidoCompra(Int32 id)
        {
            PEDIDO_COMPRA_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.PECA_AQ_ARQUIVO;
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

        [HttpGet]
        public ActionResult MontarTelaPedidoCompra()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            if (SessionMocks.listaCompra == null)
            {
                listaMaster = baseApp.GetAllItens();
                SessionMocks.listaCompra = listaMaster;
            }
            ViewBag.Listas = SessionMocks.listaCompra;
            ViewBag.Title = "Pedidos de Compraxxxx";

            // Indicadores
            ViewBag.Pedidos = SessionMocks.listaCompra.Count;
            ViewBag.Encerradas = SessionMocks.listaCompra.Where(p => p.PECO_IN_STATUS == 5).ToList().Count;
            ViewBag.Canceladas = SessionMocks.listaCompra.Where(p => p.PECO_IN_STATUS == 6).ToList().Count;
            ViewBag.Atrasadas = SessionMocks.listaCompra.Where(p => p.PECO_DT_PREVISTA < DateTime.Today.Date).ToList().Count;
            ViewBag.EncerradasLista = SessionMocks.listaCompra.Where(p => p.PECO_IN_STATUS == 5).ToList();
            ViewBag.CanceladasLista = SessionMocks.listaCompra.Where(p => p.PECO_IN_STATUS == 6).ToList();
            ViewBag.AtrasadasLista = SessionMocks.listaCompra.Where(p => p.PECO_DT_PREVISTA < DateTime.Today.Date).ToList();
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;

            SessionMocks.Fornecedores = forApp.GetAllItens();
            SessionMocks.Produtos = proApp.GetAllItens();
            SessionMocks.Insumos = insApp.GetAllItens();

            // Mensagens
            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Carrega listas
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Para Cotação", Value = "1" });
            status.Add(new SelectListItem() { Text = "Em Cotação", Value = "2" });
            status.Add(new SelectListItem() { Text = "Para Aprovação", Value = "3" });
            status.Add(new SelectListItem() { Text = "Aprovada", Value = "4" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "5" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "6" });
            ViewBag.Status = new SelectList(status, "Value", "Text");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            // Abre view
            Session["MensCompra"] = 0;
            objeto = new PEDIDO_COMPRA();
            objeto.PECO_DT_DATA = null;
            return View(objeto);
        }

        public ActionResult RetirarFiltroPedidoCompra()
        {
            SessionMocks.listaCompra = null;
            return RedirectToAction("MontarTelaPedidoCompra");
        }

        public ActionResult MostrarTudoPedidoCompra()
        {
            listaMaster = baseApp.GetAllItensAdm();
            SessionMocks.listaCompra = listaMaster;
            return RedirectToAction("MontarTelaPedidoCompra");
        }

        [HttpPost]
        public ActionResult FiltrarPedidoCompra(PEDIDO_COMPRA item)
        {
            try
            {
                // Executa a operação
                List<PEDIDO_COMPRA> listaObj = new List<PEDIDO_COMPRA>();
                Int32 volta = baseApp.ExecuteFilter(item.USUA_CD_ID, item.PECO_NM_NOME, item.PECO_NR_NUMERO, item.PECO_NR_NOTA_FISCAL, item.PECO_DT_DATA, item.PECO_IN_STATUS, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCompra"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaPedidoCompra");
                }

                // Sucesso
                listaMaster = listaObj;
                SessionMocks.listaCompra = listaObj;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaPedidoCompra");

            }
        }

        public ActionResult VoltarBaseMontarTelaPedidoCompra()
        {
            return RedirectToAction("MontarTelaPedidoCompra");
        }

        [HttpGet]
        public ActionResult IncluirPedidoCompra()
        {
            // Prepara listas
            ViewBag.CC = new SelectList(SessionMocks.CentroCustos, "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Formas = new SelectList(SessionMocks.Formas, "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Para Cotação", Value = "1" });
            status.Add(new SelectListItem() { Text = "Em Cotação", Value = "2" });
            status.Add(new SelectListItem() { Text = "Para Aprovação", Value = "3" });
            status.Add(new SelectListItem() { Text = "Aprovada", Value = "4" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "5" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "6" });
            ViewBag.Status = new SelectList(status, "Value", "Text");

            List<PRODUTO> lista = SessionMocks.Produtos.Where(p => p.PROD_IN_COMPOSTO == 0).ToList();
            ViewBag.Fornecedores = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.Produtos = new SelectList(lista, "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Insumos = new SelectList(SessionMocks.Insumos, "MAPR_CD_ID", "MAPR_NM_NOME");
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.Tipo = new SelectList(tipo, "Value", "Text");

            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0100", CultureInfo.CurrentCulture));
            }

            // Prepara view
            Session["MensCompra"] = 0;
            USUARIO usuario = SessionMocks.UserCredentials;
            PEDIDO_COMPRA item = new PEDIDO_COMPRA();
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            vm.ASSI_CD_ID = SessionMocks.IdAssinante;
            vm.PECO_DT_DATA = DateTime.Today.Date;
            vm.PECO_IN_ATIVO = 1;
            vm.PECO_IN_STATUS = 1;
            vm.MATR_CD_ID = SessionMocks.Matriz.MATR_CD_ID;
            vm.USUA_CD_ID = SessionMocks.UserCredentials.USUA_CD_ID;
            vm.PECO_DT_DATA = DateTime.Today.Date;
            vm.PECO_DT_PREVISTA = DateTime.Today.Date.AddDays(30);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirPedidoCompra(PedidoCompraViewModel vm, String tabelaItemPedido)
        {
            ViewBag.CC = new SelectList(SessionMocks.CentroCustos, "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Formas = new SelectList(SessionMocks.Formas, "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Para Cotação", Value = "1" });
            status.Add(new SelectListItem() { Text = "Em Cotação", Value = "2" });
            status.Add(new SelectListItem() { Text = "Para Aprovação", Value = "3" });
            status.Add(new SelectListItem() { Text = "Aprovada", Value = "4" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "5" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "6" });
            ViewBag.Status = new SelectList(status, "Value", "Text");

            List<PRODUTO> lista = SessionMocks.Produtos.Where(p => p.PROD_IN_COMPOSTO == 0).ToList();
            ViewBag.Fornecedores = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.Produtos = new SelectList(lista, "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Insumos = new SelectList(SessionMocks.Insumos, "MAPR_CD_ID", "MAPR_NM_NOME");
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.Tipo = new SelectList(tipo, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensCompra"] = 1;
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0100", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Acerta numero do pedido
                    item.PECO_NR_NUMERO = item.PECO_CD_ID.ToString();
                    volta = baseApp.ValidateEdit(item, item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/PedidoCompra/" + item.PECO_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    
                    // Sucesso
                    listaMaster = new List<PEDIDO_COMPRA>();
                    SessionMocks.listaCompra = null;

                    IncluirItemPedidoCompra_Inclusao(tabelaItemPedido, item.PECO_CD_ID);

                    SessionMocks.idVolta = item.PECO_CD_ID;
                    return Json(item.PECO_CD_ID); //RedirectToAction("EditarPedidoCompra", new { id = item.PECO_CD_ID});
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
        public ActionResult EditarPedidoCompra(Int32 id)
        {
            // Prepara view
            ViewBag.CC = new SelectList(SessionMocks.CentroCustos, "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Formas = new SelectList(SessionMocks.Formas, "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Para Cotação", Value = "1" });
            status.Add(new SelectListItem() { Text = "Em Cotação", Value = "2" });
            status.Add(new SelectListItem() { Text = "Para Aprovação", Value = "3" });
            status.Add(new SelectListItem() { Text = "Aprovada", Value = "4" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "5" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "6" });
            ViewBag.Status = new SelectList(status, "Value", "Text");

            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCompra"] == 2)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
            }
            Session["MensCompra"] = 0;
            Session["VoltaCompra"] = 2;

            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            objetoAntes = item;
            SessionMocks.pedidoCompra = item;
            SessionMocks.idVolta = id;
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarPedidoCompra(PedidoCompraViewModel vm)
        {
            ViewBag.CC = new SelectList(SessionMocks.CentroCustos, "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Formas = new SelectList(SessionMocks.Formas, "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Para Cotação", Value = "1" });
            status.Add(new SelectListItem() { Text = "Em Cotação", Value = "2" });
            status.Add(new SelectListItem() { Text = "Para Aprovação", Value = "3" });
            status.Add(new SelectListItem() { Text = "Aprovada", Value = "4" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "5" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "6" });
            ViewBag.Status = new SelectList(status, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<PEDIDO_COMPRA>();
                    SessionMocks.listaCompra = null;
                    return RedirectToAction("MontarTelaPedidoCompra");
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
        public ActionResult ExcluirPedidoCompra(Int32 id)
        {
            // Prepara view
            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0101", CultureInfo.CurrentCulture));
            }
            Session["MensCompra"] = 0;
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirPedidoCompra(PedidoCompraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCompra"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0101", CultureInfo.CurrentCulture));
                    return View(vm);
                }

                // Sucesso
                Session["MensCompra"] = 0;
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult ReativarPedidoCompra(Int32 id)
        {
            // Prepara view
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarPedidoCompra(PedidoCompraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult VerAnexoPedidoCompra(Int32 id)
        {
            // Prepara view
            PEDIDO_COMPRA_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoPedidoCompra()
        {
            if ((Int32)Session["VoltaCompra"] == 1)
            {
                return RedirectToAction("VerPedidoCompra", new { id = SessionMocks.idVolta });
            }
            if ((Int32)Session["VoltaCompra"] == 3)
            {
                return RedirectToAction("CancelarPedidoCompra", new { id = SessionMocks.idVolta });
            }
            if ((Int32)Session["VoltaCompra"] == 4)
            {
                return RedirectToAction("ProcessarCotacaoPedidoCompra", new { id = SessionMocks.idVolta });
            }
            return RedirectToAction("EditarPedidoCompra", new { id = SessionMocks.idVolta });
        }

        [HttpPost]
        public ActionResult UploadFilePedidoCompra(HttpPostedFileBase file)
        {
            if (file == null)
            {
                Session["MensCompra"] = 1;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPedidoCompra");
            }

            PEDIDO_COMPRA item = baseApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                Session["MensCompra"] = 2;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPedidoCompra");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/PedidoCompra/" + item.PECO_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            PEDIDO_COMPRA_ANEXO foto = new PEDIDO_COMPRA_ANEXO();
            foto.PECA_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.PECA_DT_ANEXO = DateTime.Today;
            foto.PECA_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.PECA_IN_TIPO = tipo;
            foto.PECA_NM_TITULO = fileName;
            foto.PECO_CD_ID = item.PECO_CD_ID;

            item.PEDIDO_COMPRA_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoPedidoCompra");
        }

        [HttpPost]
        public JsonResult UploadFilePedidoCompra_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
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
                    //UploadFotoPedidoCompra(file);

                    //count++;
                }
                else
                {
                    UploadFilePedidoCompra(file);
                }
            }

            return Json("1"); // VoltarAnexoCliente
        }

        [HttpGet]
        public ActionResult EnviarCotacaoPedidoCompra(Int32 id)
        {
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0102", CultureInfo.CurrentCulture));
            }
            Session["MensCompra"] = 0;
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EnviarCotacaoPedidoCompra(PedidoCompraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateCotacao(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCompra"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0102", CultureInfo.CurrentCulture));
                    return View(vm);
                }

                // Sucesso
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult EnviarAprovacaoPedidoCompra(Int32 id)
        {
            // Prepara view
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0103", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCompra"] == 2)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0104", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCompra"] == 3)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0105", CultureInfo.CurrentCulture));
            }
            Session["MensCompra"] = 0;
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EnviarAprovacaoPedidoCompra(PedidoCompraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCompra"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0103", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                if (volta == 2)
                {
                    Session["MensCompra"] = 2;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0104", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                if (volta == 3)
                {
                    Session["MensCompra"] = 3;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0105", CultureInfo.CurrentCulture));
                    return View(vm);
                }

                // Sucesso
                Session["MensCompra"] = 0;
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult AprovarPedidoCompra(Int32 id)
        {
            // Prepara view
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult AprovarPedidoCompra(PedidoCompraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateAprovacao(item);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult EncerrarPedidoCompra(Int32 id)
        {
            // Prepara view
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EncerrarPedidoCompra(PedidoCompraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateEncerramento(item);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult CancelarPedidoCompra(Int32 id)
        {
            // Prepara view
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0106", CultureInfo.CurrentCulture));
            }
            Session["MensCompra"] = 0;
            Session["VoltaCompra"] = 3;
            SessionMocks.idVolta = id;
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult CancelarPedidoCompra(PedidoCompraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateCancelamento(item);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCompra"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0106", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                // Sucesso
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        public ActionResult VerPedidoCompra(Int32 id)
        {
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            SessionMocks.idVolta = id;
            Session["VoltaCompra"] = 1;
            return View(vm);
        }

        public ActionResult VerItemPedidoCompra(Int32 id)
        {
            ITEM_PEDIDO_COMPRA item = baseApp.GetItemCompraById(id);
            ItemPedidoCompraViewModel vm = Mapper.Map<ITEM_PEDIDO_COMPRA, ItemPedidoCompraViewModel>(item);
            return View(vm);
        }

        public ActionResult VerAtrasados()
        {
            // Prepara view
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            // Abre view
            ViewBag.Pedidos = SessionMocks.listaCompra.Count;
            ViewBag.Atrasadas = SessionMocks.listaCompra.Where(p => p.PECO_DT_PREVISTA < DateTime.Today.Date).ToList().Count;
            ViewBag.AtrasadasLista = SessionMocks.listaCompra.Where(p => p.PECO_DT_PREVISTA < DateTime.Today.Date).ToList();
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;

            objeto = new PEDIDO_COMPRA();
            SessionMocks.voltaCompra = 1;
            SessionMocks.voltaConsulta = 3;
            return View(objeto);
        }

        public ActionResult VerEncerrados()
        {
            // Prepara view
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            // Abre view
            ViewBag.Pedidos = SessionMocks.listaCompra.Count;
            ViewBag.Encerradas = SessionMocks.listaCompra.Where(p => p.PECO_IN_STATUS == 5).ToList().Count;
            ViewBag.EncerradasLista = SessionMocks.listaCompra.Where(p => p.PECO_IN_STATUS == 5).ToList();
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;

            objeto = new PEDIDO_COMPRA();
            SessionMocks.voltaCompra = 1;
            SessionMocks.voltaConsulta = 3;
            return View(objeto);
        }

        public ActionResult VerCancelados()
        {
            // Prepara view
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            // Abre view
            ViewBag.Pedidos = SessionMocks.listaCompra.Count;
            ViewBag.Canceladas = SessionMocks.listaCompra.Where(p => p.PECO_IN_STATUS == 6).ToList().Count;
            ViewBag.CanceladasLista = SessionMocks.listaCompra.Where(p => p.PECO_IN_STATUS == 5).ToList();
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;

            objeto = new PEDIDO_COMPRA();
            SessionMocks.voltaCompra = 1;
            SessionMocks.voltaConsulta = 3;
            return View(objeto);
        }

        [HttpGet]
        public ActionResult EditarItemPedidoCompra(Int32 id)
        {
            // Prepara view
            ViewBag.Fornecedores = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.Produtos = new SelectList(SessionMocks.Produtos, "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Insumos = new SelectList(SessionMocks.Insumos, "MAPR_CD_ID", "MAPR_NM_NOME");
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.Tipo = new SelectList(tipo, "Value", "Text");
            ITEM_PEDIDO_COMPRA item = baseApp.GetItemCompraById(id);
            objetoAntes = SessionMocks.pedidoCompra;
            ItemPedidoCompraViewModel vm = Mapper.Map<ITEM_PEDIDO_COMPRA, ItemPedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarItemPedidoCompra(ItemPedidoCompraViewModel vm)
        {
            ViewBag.Fornecedores = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.Produtos = new SelectList(SessionMocks.Produtos, "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Insumos = new SelectList(SessionMocks.Insumos, "MAPR_CD_ID", "MAPR_NM_NOME");
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.Tipo = new SelectList(tipo, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    ITEM_PEDIDO_COMPRA item = Mapper.Map<ItemPedidoCompraViewModel, ITEM_PEDIDO_COMPRA>(vm);
                    Int32 volta = baseApp.ValidateEditItemCompra(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoPedidoCompra");
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
        public ActionResult ExcluirItemPedidoCompra(Int32 id)
        {
            ITEM_PEDIDO_COMPRA item = baseApp.GetItemCompraById(id);
            objetoAntes = SessionMocks.pedidoCompra;
            item.ITPC_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateEditItemCompra(item);
            return RedirectToAction("VoltarAnexoPedidoCompra");
        }

        [HttpGet]
        public ActionResult ReativarItemPedidoCompra(Int32 id)
        {
            ITEM_PEDIDO_COMPRA item = baseApp.GetItemCompraById(id);
            objetoAntes = SessionMocks.pedidoCompra;
            item.ITPC_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditItemCompra(item);
            return RedirectToAction("VoltarAnexoPedidoCompraUsuario");
        }

        [HttpGet]
        public ActionResult IncluirItemPedidoCompra()
        {
            // Prepara view
            List<PRODUTO> lista = SessionMocks.Produtos.Where(p => p.PROD_IN_COMPOSTO == 0).ToList();
            ViewBag.Fornecedores = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.Produtos = new SelectList(lista, "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Insumos = new SelectList(SessionMocks.Insumos, "MAPR_CD_ID", "MAPR_NM_NOME");
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.Tipo = new SelectList(tipo, "Value", "Text");

            USUARIO usuario = SessionMocks.UserCredentials;
            ITEM_PEDIDO_COMPRA item = new ITEM_PEDIDO_COMPRA();
            ItemPedidoCompraViewModel vm = Mapper.Map<ITEM_PEDIDO_COMPRA, ItemPedidoCompraViewModel>(item);
            vm.PECO_CD_ID = SessionMocks.idVolta;
            vm.ITPC_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirItemPedidoCompra(ItemPedidoCompraViewModel vm)
        {
            ViewBag.Fornecedores = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.Produtos = new SelectList(SessionMocks.Produtos, "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(SessionMocks.Unidades, "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Insumos = new SelectList(SessionMocks.Insumos, "MAPR_CD_ID", "MAPR_NM_NOME");
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Produto", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Insumo", Value = "2" });
            ViewBag.Tipo = new SelectList(tipo, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ITEM_PEDIDO_COMPRA item = Mapper.Map<ItemPedidoCompraViewModel, ITEM_PEDIDO_COMPRA>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = baseApp.ValidateCreateItemCompra(item);
                    // Verifica retorno
                    return RedirectToAction("IncluirItemPedidoCompra");
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

        [HttpPost]
        public JsonResult IncluirItemPedidoCompra_Inclusao(String tabelaItemPedido, Int32 id)
        {
            var jArray = JArray.Parse(tabelaItemPedido);

            foreach (var jObject in jArray)
            {
                ITEM_PEDIDO_COMPRA item = new ITEM_PEDIDO_COMPRA();
                item.PECO_CD_ID = id;
                item.ITPC_IN_TIPO = (Int32)jObject["ITPC_IN_TIPO"];

                if (item.ITPC_IN_TIPO == 1)
                {
                    item.PROD_CD_ID = (Int32)jObject["PROD_CD_ID"];
                }
                else
                {
                    item.MAPR_CD_ID = (Int32)jObject["MAPR_CD_ID"];
                }

                item.UNID_CD_ID = (Int32)jObject["UNID_CD_ID"];
                item.ITPC_QN_QUANTIDADE = (Int32)jObject["ITPC_QN_QUANTIDADE"];
                item.ITPC_TX_OBSERVACOES = (String)jObject["ITPC_TX_OBSERVACOES"];

                Int32 volta = baseApp.ValidateCreateItemCompra(item);
            }

            return Json(1);
        }

        [HttpGet]
        public ActionResult ProcessarCotacaoPedidoCompra(Int32 id)
        {
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            //PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0102", CultureInfo.CurrentCulture));
            }
            Session["MensCompra"] = 0;
            Session["VoltaCompra"] = 4;
            SessionMocks.idVolta = id;
            return View(item);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ProcessarCotacaoPedidoCompra(PEDIDO_COMPRA item)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                //PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateCotacao(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCompra"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0102", CultureInfo.CurrentCulture));
                    return View(item);
                }

                // Sucesso
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(item);
            }
        }

        [HttpGet]
        public ActionResult ProcessarEnviarAprovacaoPedidoCompra(Int32 id)
        {
            PEDIDO_COMPRA item = baseApp.GetItemById(id);
            if ((Int32)Session["MensCompra"] == 1)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0103", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCompra"] == 2)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0104", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCompra"] == 3)
            {
                Session["MensCompra"] = 0;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0105", CultureInfo.CurrentCulture));
            }
            Session["MensCompra"] = 0;
            PedidoCompraViewModel vm = Mapper.Map<PEDIDO_COMPRA, PedidoCompraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ProcessarEnviarAprovacaoPedidoCompra(PedidoCompraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PEDIDO_COMPRA item = Mapper.Map<PedidoCompraViewModel, PEDIDO_COMPRA>(vm);
                Int32 volta = baseApp.ValidateEnvioAprovacao(item);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCompra"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0103", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                if (volta == 2)
                {
                    Session["MensCompra"] = 2;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0104", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                if (volta == 3)
                {
                    Session["MensCompra"] = 3;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0105", CultureInfo.CurrentCulture));
                    return View(vm);
                }

                // Sucesso
                Session["MensCompra"] = 0;
                listaMaster = new List<PEDIDO_COMPRA>();
                SessionMocks.listaCompra = null;
                return RedirectToAction("MontarTelaPedidoCompra");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

    }
}