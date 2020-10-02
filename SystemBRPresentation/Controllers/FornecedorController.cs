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
using Canducci.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;

namespace SystemBRPresentation.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly IFornecedorAppService fornApp;
        private readonly ILogAppService logApp;
        private readonly ITipoPessoaAppService tpApp;
        private readonly ICategoriaFornecedorAppService cfApp;
        private readonly IFilialAppService filApp;
        private readonly IMatrizAppService matrizApp;
        private readonly IContaPagarAppService cpApp;

        private String msg;
        private Exception exception;
        FORNECEDOR objetoForn = new FORNECEDOR();
        FORNECEDOR objetoFornAntes = new FORNECEDOR();
        List<FORNECEDOR> listaMasterForn = new List<FORNECEDOR>();
        List<CONTA_PAGAR> listaMasterPag = new List<CONTA_PAGAR>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public FornecedorController(IFornecedorAppService fornApps, ILogAppService logApps, ITipoPessoaAppService tpApps, ICategoriaFornecedorAppService cfApps, IFilialAppService filApps,IMatrizAppService matrizApps, IContaPagarAppService cpApps)
        {
            fornApp = fornApps;
            logApp = logApps;
            tpApp = tpApps;
            cfApp = cfApps;
            filApp = filApps;
            matrizApp = matrizApps;
            cpApp = cpApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            FORNECEDOR item = new FORNECEDOR();
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
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
            listaMasterForn = new List<FORNECEDOR>();
            SessionMocks.fornecedor = null;
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaFornecedor()
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
            if (SessionMocks.listaFornecedor == null)
            {
                listaMasterForn = fornApp.GetAllItens();
                SessionMocks.listaFornecedor = listaMasterForn;
            }
            ViewBag.Listas = SessionMocks.listaFornecedor;
            ViewBag.Title = "Fornecedores";
            ViewBag.Cats = new SelectList(cfApp.GetAllItens(), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            Session["IncluirForn"] = 0;

            // Indicadores
            ViewBag.Fornecedores = SessionMocks.listaFornecedor.Count;
            SessionMocks.listaCP = cpApp.GetItensAtrasoFornecedor().ToList();
            ViewBag.Atrasos = SessionMocks.listaCP.Select(x => x.FORN_CD_ID).Distinct().ToList().Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            ViewBag.Inativos = SessionMocks.listaFornecedor.Where(p => p.FORN_IN_ATIVO == 0).ToList().Count;
            ViewBag.SemPedidos = SessionMocks.listaFornecedor.Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList().Count;
            List<SelectListItem> ativo = new List<SelectListItem>();
            ativo.Add(new SelectListItem() { Text = "Ativo", Value = "0" });
            ativo.Add(new SelectListItem() { Text = "Inativo", Value = "1" });
            ViewBag.Ativos = new SelectList(ativo, "Value", "Text");

            // Abre view
            objetoForn = new FORNECEDOR();
            SessionMocks.voltaFornecedor = 1;
            return View(objetoForn);
        }

        public ActionResult RetirarFiltroFornecedor()
        {
            SessionMocks.listaFornecedor = null;
            SessionMocks.filtroFornecedor = null;
            if (SessionMocks.voltaFornecedor == 2)
            {
                return RedirectToAction("VerCardsFornecedor");
            }
            return RedirectToAction("MontarTelaFornecedor");
        }

        public ActionResult MostrarTudoFornecedor()
        {
            listaMasterForn = fornApp.GetAllItensAdm();
            SessionMocks.filtroFornecedor = null;
            SessionMocks.listaFornecedor = listaMasterForn;
            if (SessionMocks.voltaFornecedor == 2)
            {
                return RedirectToAction("VerCardsFornecedor");
            }
            return RedirectToAction("MontarTelaFornecedor");
        }

        [HttpPost]
        public ActionResult FiltrarFornecedor(FORNECEDOR item)
        {
            try
            {
                // Executa a operação
                List<FORNECEDOR> listaObj = new List<FORNECEDOR>();
                SessionMocks.filtroFornecedor = item;
                Int32 volta = fornApp.ExecuteFilter(item.CAFO_CD_ID, item.FORN_NM_NOME, item.FORN_NR_CPF, item.FORN_NR_CNPJ, item.FORN_NM_EMAIL, item.FORN_NM_CIDADE, item.UF_CD_ID, item.FORN_NM_REDES_SOCIAIS, item.FORN_IN_ATIVO, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0017", CultureInfo.CurrentCulture);
                }

                // Sucesso
                listaMasterForn = listaObj;
                SessionMocks.listaFornecedor = listaObj;
                if (SessionMocks.voltaFornecedor == 2)
                {
                    return RedirectToAction("VerCardsFornecedor");
                }
                return RedirectToAction("MontarTelaFornecedor");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaFornecedor");
            }
        }

        public ActionResult VoltarBaseFornecedor()
        {
            if (SessionMocks.voltaFornecedor == 2)
            {
                return RedirectToAction("VerCardsFornecedor");
            }
            return RedirectToAction("MontarTelaFornecedor");
        }

        [HttpGet]
        public ActionResult IncluirFornecedor()
        {
            // Prepara listas
            ViewBag.Cats = new SelectList(cfApp.GetAllItens(), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");

            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            FORNECEDOR item = new FORNECEDOR();
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            vm.FORN_DT_CADASTRO = DateTime.Today;
            vm.FORN_IN_ATIVO = 1;
            vm.ASSI_CD_ID = SessionMocks.IdAssinante;
            vm.MATR_CD_ID = SessionMocks.Matriz.MATR_CD_ID;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirFornecedor(FornecedorViewModel vm)
        {
            ViewBag.Cats = new SelectList(cfApp.GetAllItens(), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    FORNECEDOR item = Mapper.Map<FornecedorViewModel, FORNECEDOR>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = fornApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0062", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Carrega foto e processa alteracao
                    item.FORN_AQ_FOTO = "~/Imagens/Base/FotoBase.jpg";
                    volta = fornApp.ValidateEdit(item, item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMasterForn = new List<FORNECEDOR>();
                    SessionMocks.listaFornecedor = null;
                    Session["IncluirForn"] = 1;
                    SessionMocks.Fornecedores = fornApp.GetAllItens();
                    if (SessionMocks.voltaFornecedor == 2)
                    {
                        return RedirectToAction("IncluirFornecedor");
                    }

                    SessionMocks.idVolta = item.FORN_CD_ID;
                    return View(vm);
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
        public ActionResult EditarFornecedor(Int32 id)
        {
            // Prepara view
            ViewBag.Cats = new SelectList(cfApp.GetAllItens(), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Incluir = (Int32)Session["IncluirForn"];

            FORNECEDOR item = fornApp.GetItemById(id);
            objetoFornAntes = item;
            ViewBag.Compras = item.ITEM_PEDIDO_COMPRA.Count;
            SessionMocks.fornecedor = item;
            SessionMocks.idVolta = id;
            SessionMocks.voltaCEP = 1;
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarFornecedor(FornecedorViewModel vm)
        {
            ViewBag.Cats = new SelectList(cfApp.GetAllItens(), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    FORNECEDOR item = Mapper.Map<FornecedorViewModel, FORNECEDOR>(vm);
                    Int32 volta = fornApp.ValidateEdit(item, objetoFornAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterForn = new List<FORNECEDOR>();
                    SessionMocks.listaFornecedor = null;
                    Session["IncluirForn"] = 0;
                    if (SessionMocks.voltaFornecedor == 2)
                    {
                        return RedirectToAction("VerCardsFornecedor");
                    }
                    return RedirectToAction("MontarTelaFornecedor");
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
        public ActionResult ExcluirFornecedor(Int32 id)
        {
            // Prepara view
            FORNECEDOR item = fornApp.GetItemById(id);
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirFornecedor(FornecedorViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                FORNECEDOR item = Mapper.Map<FornecedorViewModel, FORNECEDOR>(vm);
                Int32 volta = fornApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0021", CultureInfo.CurrentCulture));
                    return View(vm);
                }

                // Sucesso
                listaMasterForn = new List<FORNECEDOR>();
                SessionMocks.listaFornecedor = null;
                return RedirectToAction("MontarTelaFornecedor");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objetoForn);
            }
        }

        [HttpGet]
        public ActionResult ReativarFornecedor(Int32 id)
        {
            // Prepara view
            FORNECEDOR item = fornApp.GetItemById(id);
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarFornecedor(FornecedorViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                FORNECEDOR item = Mapper.Map<FornecedorViewModel, FORNECEDOR>(vm);
                Int32 volta = fornApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMasterForn = new List<FORNECEDOR>();
                SessionMocks.listaFornecedor = null;
                return RedirectToAction("MontarTelaFornecedor");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objetoForn);
            }
        }

        [HttpGet]
        public ActionResult VerCardsFornecedor()
        {
            // Carrega listas
            if (SessionMocks.listaFornecedor == null)
            {
                listaMasterForn = fornApp.GetAllItens();
                SessionMocks.listaFornecedor = listaMasterForn;
            }
            ViewBag.Listas = SessionMocks.listaFornecedor;
            ViewBag.Title = "Fornecedores";
            ViewBag.Cats = new SelectList(SessionMocks.CatsForns, "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();

            // Indicadores
            ViewBag.Fornecedores = SessionMocks.listaFornecedor.Count;

            // Abre view
            objetoForn = new FORNECEDOR();
            SessionMocks.voltaFornecedor = 2;
            return View(objetoForn);
        }

        [HttpGet]
        public ActionResult VerAnexoFornecedor(Int32 id)
        {
            // Prepara view
            FORNECEDOR_ANEXO item = fornApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoFornecedor()
        {
            return RedirectToAction("EditarFornecedor", new { id = SessionMocks.idVolta });
        }

        public FileResult DownloadFornecedor(Int32 id)
        {
            FORNECEDOR_ANEXO item = fornApp.GetAnexoById(id);
            String arquivo = item.FOAN_AQ_ARQUVO;
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
        public ActionResult UploadFileFornecedor(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoFornecedor");
            }


            FORNECEDOR item = fornApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoFornecedor");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            FORNECEDOR_ANEXO foto = new FORNECEDOR_ANEXO();
            foto.FOAN_AQ_ARQUVO = "~" + caminho + fileName;
            foto.FOAN_DT_ANEXO = DateTime.Today;
            foto.FOAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.FOAN_IN_TIPO = tipo;
            foto.FOAN_NM_TITULO = fileName;
            foto.FORN_CD_ID = item.FORN_CD_ID;

            item.FORNECEDOR_ANEXO.Add(foto);
            objetoFornAntes = item;
            Int32 volta = fornApp.ValidateEdit(item, objetoFornAntes);
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpPost]
        public JsonResult UploadFileFornecedor_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
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
                    UploadFotoFornecedor(file);

                    count++;
                }
                else
                {
                    UploadFileFornecedor(file);
                }
            }

            return Json("1"); // VoltarAnexoCliente
        }

        [HttpPost]
        public ActionResult UploadFotoFornecedor(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoFornecedor");
            }

            FORNECEDOR item = fornApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoFornecedor");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.FORN_AQ_FOTO = "~" + caminho + fileName;
            objetoFornAntes = item;
            Int32 volta = fornApp.ValidateEdit(item, objetoFornAntes);
            listaMasterForn = new List<FORNECEDOR>();
            SessionMocks.listaFornecedor = null;
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpGet]
        public ActionResult SlideShowFornecedor()
        {
            // Prepara view
            FORNECEDOR item = fornApp.GetItemById(SessionMocks.idVolta);
            objetoFornAntes = item;
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult EditarContatoFornecedor(Int32 id)
        {
            // Prepara view
            FORNECEDOR_CONTATO item = fornApp.GetContatoById(id);
            objetoFornAntes = SessionMocks.fornecedor;
            FornecedorContatoViewModel vm = Mapper.Map<FORNECEDOR_CONTATO, FornecedorContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarContatoFornecedor(FornecedorContatoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    FORNECEDOR_CONTATO item = Mapper.Map<FornecedorContatoViewModel, FORNECEDOR_CONTATO>(vm);
                    Int32 volta = fornApp.ValidateEditContato(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoFornecedor");
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
        public ActionResult ExcluirContatoFornecedor(Int32 id)
        {
            FORNECEDOR_CONTATO item = fornApp.GetContatoById(id);
            objetoFornAntes = SessionMocks.fornecedor;
            item.FOCO_IN_ATIVO = 0;
            Int32 volta = fornApp.ValidateEditContato(item);
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpGet]
        public ActionResult ReativarContatoFornecedor(Int32 id)
        {
            FORNECEDOR_CONTATO item = fornApp.GetContatoById(id);
            objetoFornAntes = SessionMocks.fornecedor;
            item.FOCO_IN_ATIVO = 1;
            Int32 volta = fornApp.ValidateEditContato(item);
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpGet]
        public ActionResult IncluirContatoFornecedor()
        {
            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            FORNECEDOR_CONTATO item = new FORNECEDOR_CONTATO();
            FornecedorContatoViewModel vm = Mapper.Map<FORNECEDOR_CONTATO, FornecedorContatoViewModel>(item);
            vm.FORN_CD_ID = SessionMocks.idVolta;
            vm.FOCO_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirContatoFornecedor(FornecedorContatoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    FORNECEDOR_CONTATO item = Mapper.Map<FornecedorContatoViewModel, FORNECEDOR_CONTATO>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = fornApp.ValidateCreateContato(item);
                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoFornecedor");
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
        public ActionResult BuscarCEPFornecedor()
        {
            // Prepara view
            if (SessionMocks.voltaCEP == 1)
            {
                //CLIENTE item = baseApp.GetItemById(SessionMocks.idVolta);
                FORNECEDOR item = SessionMocks.fornecedor;
                FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
                vm.FORN_NR_CEP_BUSCA = String.Empty;
                vm.FORN_SG_UF = vm.UF.UF_SG_SIGLA;
                return View(vm);
            }
            else
            {
                FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(SessionMocks.fornecedor);
                vm.FORN_NR_CEP_BUSCA = String.Empty;
                return View(vm);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult BuscarCEPFornecedor(FornecedorViewModel vm)
        {
                try
                {
                    // Atualiza cliente
                    FORNECEDOR item = SessionMocks.fornecedor;
                    FORNECEDOR cli = new FORNECEDOR();
                    cli.ASSI_CD_ID = item.ASSI_CD_ID;
                    cli.ASSI_CD_ID = item.ASSI_CD_ID;
                    cli.CAFO_CD_ID = item.CAFO_CD_ID;
                    cli.FILI_CD_ID = item.FILI_CD_ID;
                    cli.FORN_AQ_FOTO = item.FORN_AQ_FOTO;
                    cli.FORN_CD_ID = item.FORN_CD_ID;
                    cli.FORN_DT_CADASTRO = item.FORN_DT_CADASTRO;
                    cli.FORN_IN_ATIVO = item.FORN_IN_ATIVO;
                    cli.FORN_NM_BAIRRO = item.FORN_NM_BAIRRO;
                    cli.FORN_NM_CIDADE = item.FORN_NM_CIDADE;
                    cli.FORN_NM_EMAIL = item.FORN_NM_EMAIL;
                    cli.FORN_NM_ENDERECO = item.FORN_NM_ENDERECO;
                    cli.FORN_NM_NOME = item.FORN_NM_NOME;
                    cli.FORN_NM_RAZAO = item.FORN_NM_RAZAO;
                    cli.FORN_NM_REDES_SOCIAIS = item.FORN_NM_REDES_SOCIAIS;
                    cli.FORN_NM_TELEFONES = item.FORN_NM_TELEFONES;
                    cli.FORN_NR_CEP = item.FORN_NR_CEP;
                    cli.FORN_NR_CNPJ = item.FORN_NR_CNPJ;
                    cli.FORN_NR_CPF = item.FORN_NR_CPF;
                    cli.FORN_NR_INSCRICAO_ESTADUAL = item.FORN_NR_INSCRICAO_ESTADUAL;
                    cli.FORN_SG_UF = item.FORN_SG_UF;
                    cli.FORN_TX_OBSERVACOES = item.FORN_TX_OBSERVACOES;
                    cli.UF_CD_ID = item.UF_CD_ID;
                    cli.MATR_CD_ID = item.MATR_CD_ID;
                    cli.TIPE_CD_ID = item.TIPE_CD_ID;

                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = fornApp.ValidateEdit(cli, cli);

                    // Verifica retorno

                    // Sucesso
                    listaMasterForn = new List<FORNECEDOR>();
                    SessionMocks.listaFornecedor = null;
                    return RedirectToAction("EditarFornecedor", new { id = SessionMocks.idVolta });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
        }

        public ActionResult PesquisaCEP(FornecedorViewModel itemVolta)
        {
            // Chama servico ECT
            FORNECEDOR cli = fornApp.GetItemById(SessionMocks.idVolta);
            FornecedorViewModel item = Mapper.Map<FORNECEDOR, FornecedorViewModel>(cli);

            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            String cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(itemVolta.FORN_NR_CEP_BUSCA);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza            
            item.FORN_NM_ENDERECO = end.Address + "/" + end.Complement;
            item.FORN_NM_BAIRRO = end.District;
            item.FORN_NM_CIDADE = end.City;
            item.FORN_SG_UF = end.Uf;
            item.UF_CD_ID = fornApp.GetUFbySigla(end.Uf).UF_CD_ID;
            item.FORN_NR_CEP = itemVolta.FORN_NR_CEP_BUSCA;

            // Retorna
            SessionMocks.voltaCEP = 2;
            SessionMocks.fornecedor = Mapper.Map<FornecedorViewModel, FORNECEDOR>(item);
            return RedirectToAction("BuscarCEPFornecedor");
        }

        [HttpPost]
        public JsonResult PesquisaCEP_Javascript(String cep, int tipoEnd)
        {
            // Chama servico ECT
            //Address end = ExternalServices.ECT_Services.GetAdressCEP(item.CLIE_NR_CEP_BUSCA);
            //Endereco end = ExternalServices.ECT_Services.GetAdressCEPService(item.CLIE_NR_CEP_BUSCA);
            FORNECEDOR cli = fornApp.GetItemById(SessionMocks.idVolta);

            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(cep);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza
            var hash = new Hashtable();

            if (tipoEnd == 1)
            {
                hash.Add("FORN_NM_ENDERECO", end.Address + "/" + end.Complement);
                hash.Add("FORN_NM_BAIRRO", end.District);
                hash.Add("FORN_NM_CIDADE", end.City);
                hash.Add("FORN_SG_UF", end.Uf);
                hash.Add("UF_CD_ID", fornApp.GetUFbySigla(end.Uf).UF_CD_ID);
                hash.Add("FORN_NR_CEP", cep);
            }

            // Retorna
            SessionMocks.voltaCEP = 2;
            return Json(hash);
        }

        public ActionResult GerarRelatorioLista()
        {
            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "FornecedorLista" + "_" + data + ".pdf";
            List<FORNECEDOR> lista = SessionMocks.listaFornecedor;
            FORNECEDOR filtro = SessionMocks.filtroFornecedor;
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

            cell = new PdfPCell(new Paragraph("Fornecedores - Listagem", meuFont2))
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
            table = new PdfPTable(new float[] { 70f, 150f, 60f, 60f, 150f, 50f, 50f, 20f});
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Fornecedores selecionados pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.Colspan = 8;
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Categoria", meuFont))
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
            cell = new PdfPCell(new Paragraph("CPF", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("CNPJ", meuFont))
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
            cell = new PdfPCell(new Paragraph("Cidade", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("UF", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            foreach (FORNECEDOR item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.CATEGORIA_FORNECEDOR.CAFO_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.FORN_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.FORN_NR_CPF != null)
                {
                    cell = new PdfPCell(new Paragraph(item.FORN_NR_CPF, meuFont))
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
                if (item.FORN_NR_CNPJ != null)
                {
                    cell = new PdfPCell(new Paragraph(item.FORN_NR_CNPJ, meuFont))
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
                cell = new PdfPCell(new Paragraph(item.FORN_NM_EMAIL, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.FORN_NM_TELEFONES != null)
                {
                    cell = new PdfPCell(new Paragraph(item.FORN_NM_TELEFONES, meuFont))
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
                if (item.FORN_NM_CIDADE != null)
                {
                    cell = new PdfPCell(new Paragraph(item.FORN_NM_CIDADE, meuFont))
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
                if (item.UF != null)
                {
                    cell = new PdfPCell(new Paragraph(item.UF.UF_SG_SIGLA, meuFont))
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
                if (filtro.CAFO_CD_ID > 0)
                {
                    parametros += "Categoria: " + filtro.CAFO_CD_ID;
                    ja = 1;
                }
                if (filtro.FORN_CD_ID > 0)
                {
                    FORNECEDOR cli = fornApp.GetItemById(filtro.FORN_CD_ID);
                    if (ja == 0)
                    {
                        parametros += "Nome: " + cli.FORN_NM_NOME;
                        ja = 1;
                    }
                    else
                    {
                        parametros +=  " e Nome: " + cli.FORN_NM_NOME;
                    }
                }
                if (filtro.FORN_NR_CPF != null)
                {
                    if (ja == 0)
                    {
                        parametros += "CPF: " + filtro.FORN_NR_CPF;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e CPF: " + filtro.FORN_NR_CPF;
                    }
                }
                if (filtro.FORN_NR_CNPJ != null)
                {
                    if (ja == 0)
                    {
                        parametros += "CNPJ: " + filtro.FORN_NR_CNPJ;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e CNPJ: " + filtro.FORN_NR_CNPJ;
                    }
                }
                if (filtro.FORN_NM_EMAIL != null)
                {
                    if (ja == 0)
                    {
                        parametros += "E-Mail: " + filtro.FORN_NM_EMAIL;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e E-Mail: " + filtro.FORN_NM_EMAIL;
                    }
                }
                if (filtro.FORN_NM_CIDADE != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Cidade: " + filtro.FORN_NM_CIDADE;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Cidade: " + filtro.FORN_NM_CIDADE;
                    }
                }
                if (filtro.UF != null)
                {
                    if (ja == 0)
                    {
                        parametros += "UF: " + filtro.UF.UF_SG_SIGLA;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e UF: " + filtro.UF.UF_SG_SIGLA;
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

            return RedirectToAction("MontarTelaCliente");
        }

        public ActionResult GerarRelatorioDetalhe()
        {
            // Prepara geração
            FORNECEDOR aten = fornApp.GetItemById(SessionMocks.idVolta);
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "Fornecedor" + aten.FORN_CD_ID.ToString() + "_" + data + ".pdf";
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

            cell = new PdfPCell(new Paragraph("FORNECEDOR - Detalhes", meuFont2))
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

            cell = new PdfPCell(new Paragraph("Tipo de Pessoa: " + aten.TIPO_PESSOA.TIPE_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Filial: " + aten.FILIAL.FILI_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Categoria: " + aten.CATEGORIA_FORNECEDOR.CAFO_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph(" ", meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);


            cell = new PdfPCell(new Paragraph("Nome: " + aten.FORN_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Razão Social: " + aten.FORN_NM_RAZAO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (aten.FORN_NR_CPF != null)
            {
                cell = new PdfPCell(new Paragraph("CPF: " + aten.FORN_NR_CPF, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }

            if (aten.FORN_NR_CNPJ != null)
            {
                cell = new PdfPCell(new Paragraph("CNPJ: " + aten.FORN_NR_CNPJ, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Ins.Estadual: " + aten.FORN_NR_INSCRICAO_ESTADUAL, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Endereços
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Endereço Principal", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Endereço: " + aten.FORN_NM_ENDERECO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Bairro: " + aten.FORN_NM_BAIRRO, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Cidade: " + aten.FORN_NM_CIDADE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            if (aten.UF != null)
            {
                cell = new PdfPCell(new Paragraph("UF: " + aten.UF.UF_SG_SIGLA, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("UF: -", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            cell = new PdfPCell(new Paragraph("CEP: " + aten.FORN_NR_CEP, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph(" ", meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph(" ", meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Contatos
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Contatos", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("E-Mail: " + aten.FORN_NM_EMAIL, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Redes Sociais: " + aten.FORN_NM_REDES_SOCIAIS, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Website: " , meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Telefones: " + aten.FORN_NM_TELEFONES, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Lista de Contatos
            if (aten.FORNECEDOR_CONTATO.Count > 0)
            {
                table = new PdfPTable(new float[] { 120f, 100f, 120f, 100f, 50f });
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
                cell = new PdfPCell(new Paragraph("Cargo", meuFont))
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

                foreach (FORNECEDOR_CONTATO item in aten.FORNECEDOR_CONTATO)
                {
                    cell = new PdfPCell(new Paragraph(item.FOCO_NM_NOME, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.FOCO_NM_CARGO, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.FOCO_NM_EMAIL, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.FOCO_NR_TELEFONES, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.FOCO_IN_ATIVO == 1)
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

            // Observações
            Chunk chunk1 = new Chunk("Observações: " + aten.FORN_TX_OBSERVACOES, FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            // Pedidos de Venda
            if (aten.ITEM_PEDIDO_COMPRA.Count > 0)
            {
                // Linha Horizontal
                line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
                pdfDoc.Add(line1);

                // Lista de Pedidos
                table = new PdfPTable(new float[] { 120f, 80f, 80f, 80f});
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
                cell = new PdfPCell(new Paragraph("Data", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Nome Produto", meuFont))
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

                foreach (ITEM_PEDIDO_COMPRA item in aten.ITEM_PEDIDO_COMPRA)
                {
                    cell = new PdfPCell(new Paragraph(item.PEDIDO_COMPRA.PECO_NM_NOME, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.PEDIDO_COMPRA.PECO_DT_DATA.Value.ToShortDateString(), meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.PRODUTO != null)
                    {
                        cell = new PdfPCell(new Paragraph(item.PRODUTO.PROD_NM_NOME, meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    if (item.MATERIA_PRIMA != null)
                    {
                        cell = new PdfPCell(new Paragraph(item.MATERIA_PRIMA.MAPR_NM_NOME, meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    cell = new PdfPCell(new Paragraph(item.ITPC_QN_QUANTIDADE.ToString(), meuFont))
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

            return RedirectToAction("VoltarAnexoCliente");
        }

        public ActionResult VerPagamentosAtraso()
        {
            // Prepara view
            SessionMocks.listaCP = cpApp.GetItensAtrasoFornecedor().ToList();
            ViewBag.Atrasos = SessionMocks.listaCP.Select(x => x.FORN_CD_ID).Distinct().ToList().Count;
            ViewBag.Fornecedores = new SelectList(fornApp.GetAllItens(), "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.ContasAtrasos = SessionMocks.listaCP;
            ViewBag.ContasAtraso = SessionMocks.listaCP.Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;

            if (SessionMocks.listaPag == null)
            {
                listaMasterPag = SessionMocks.listaCP;
                SessionMocks.listaPag = listaMasterPag;
            }
            ViewBag.Listas = SessionMocks.listaPag;
            CONTA_PAGAR cr = new CONTA_PAGAR();
            return View(cr);
        }

        public ActionResult VerFornecedorSemPedidos()
        {
            // Prepara view
            ViewBag.Listas = SessionMocks.listaFornecedor.Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList();
            return View();
        }

        public ActionResult VerLancamentoAtraso(Int32 id)
        {
            // Prepara view
            CONTA_PAGAR item = cpApp.GetItemById(id);
            SessionMocks.contaPagar = item;
            SessionMocks.idCPVolta = id;
            ContaPagarViewModel vm = Mapper.Map<CONTA_PAGAR, ContaPagarViewModel>(item);
            return View(vm);
        }

    }
}