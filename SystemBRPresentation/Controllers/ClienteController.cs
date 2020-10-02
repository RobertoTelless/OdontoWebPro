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
using Correios.Net;
using Canducci.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;

namespace SystemBRPresentation.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IClienteAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly ITipoPessoaAppService tpApp;
        private readonly ICategoriaClienteAppService ccApp;
        private readonly ITipoContribuinteAppService tcApp;
        private readonly IFilialAppService filApp;
        private readonly IMatrizAppService matrizApp;
        private readonly IContaReceberAppService crApp;
        private readonly IUsuarioAppService usuApp;

        private String msg;
        private Exception exception;
        CLIENTE objeto = new CLIENTE();
        CLIENTE objetoAntes = new CLIENTE();
        List<CLIENTE> listaMaster = new List<CLIENTE>();
        List<CONTA_RECEBER> listaMasterRec = new List<CONTA_RECEBER>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public ClienteController(IClienteAppService baseApps, ILogAppService logApps, ITipoPessoaAppService tpApps, ICategoriaClienteAppService ccApps, IFilialAppService filApps, IMatrizAppService matrizApps, ITipoContribuinteAppService tcApps, IContaReceberAppService crApps, IUsuarioAppService usuApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            tpApp = tpApps;
            ccApp = ccApps;
            filApp = filApps;
            matrizApp = matrizApps;
            tcApp = tcApps;
            crApp = crApps;
            usuApp = usuApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            CLIENTE item = new CLIENTE();
            ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(item);
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
            listaMaster = new List<CLIENTE>();
            SessionMocks.cliente = null;
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaCliente()
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
            if (SessionMocks.listaCliente == null)
            {
                listaMaster = baseApp.GetAllItens();
                SessionMocks.listaCliente = listaMaster;
            }
            ViewBag.Listas = SessionMocks.listaCliente;
            ViewBag.Title = "Clientes";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Tipos = new SelectList(SessionMocks.CatsClientes, "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            SessionMocks.cliente = null;
            List<SelectListItem> ativo = new List<SelectListItem>();
            ativo.Add(new SelectListItem() { Text = "Ativo", Value = "1" });
            ativo.Add(new SelectListItem() { Text = "Inativo", Value = "0" });
            ViewBag.Ativos = new SelectList(ativo, "Value", "Text");
            Session["IncluirCliente"] = 0;

            // Indicadores
            ViewBag.Clientes = SessionMocks.listaCliente.Count;
            SessionMocks.listaCR = crApp.GetItensAtrasoCliente().ToList();
            ViewBag.Atrasos = SessionMocks.listaCR.Select(x => x.CLIE_CD_ID).Distinct().ToList().Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            ViewBag.Inativos = SessionMocks.listaCliente.Where(p => p.CLIE_IN_ATIVO == 0).ToList().Count;
            ViewBag.SemPedidos = SessionMocks.listaCliente.Where(p => p.PEDIDO_VENDA.Count == 0 || p.PEDIDO_VENDA == null).ToList().Count;

            // Abre view
            ViewBag.Novo = (Int32)Session["ClienteNovo"];
            objeto = new CLIENTE();
            objeto.CLIE_IN_ATIVO = 1;
            SessionMocks.voltaCliente = 1;
            return View(objeto);
        }

        public ActionResult RetirarFiltroCliente()
        {
            SessionMocks.listaCliente = null;
            SessionMocks.filtroCliente = null;
            if (SessionMocks.voltaCliente == 2)
            {
                return RedirectToAction("VerCardsCliente");
            }
            return RedirectToAction("MontarTelaCliente");
        }

        public ActionResult MostrarTudoCliente()
        {
            listaMaster = baseApp.GetAllItensAdm();
            SessionMocks.filtroCliente = null;
            SessionMocks.listaCliente = listaMaster;
            if (SessionMocks.voltaCliente == 2)
            {
                return RedirectToAction("VerCardsCliente");
            }
            return RedirectToAction("MontarTelaCliente");
        }

        [HttpPost]
        public ActionResult FiltrarCliente(CLIENTE item)
        {
            try
            {
                // Executa a operação
                List<CLIENTE> listaObj = new List<CLIENTE>();
                SessionMocks.filtroCliente = item;
                Int32 volta = baseApp.ExecuteFilter(item.CACL_CD_ID, item.CLIE_NM_NOME, item.CLIE_NR_CPF, item.CLIE_NR_CNPJ, item.CLIE_NM_EMAIL, item.CLIE_NM_CIDADE, item.UF_CD_ID, item.CLIE_IN_ATIVO, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture);
                }

                // Sucesso
                listaMaster = listaObj;
                SessionMocks.listaCliente = listaObj;
                if (SessionMocks.voltaCliente == 2)
                {
                    return RedirectToAction("VerCardsCliente");
                }
                return RedirectToAction("MontarTelaCliente");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaCliente");
            }
        }

        public ActionResult VoltarBaseCliente()
        {
            if (SessionMocks.voltaCliente == 2)
            {
                return RedirectToAction("VerCardsCliente");
            }
            return RedirectToAction("MontarTelaCliente");
        }

        [HttpGet]
        public ActionResult IncluirCliente()
        {
            // Prepara listas
            ViewBag.Tipos = new SelectList(SessionMocks.CatsClientes, "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.TiposContribuinte = new SelectList(SessionMocks.TiposContribuintes, "TICO_CD_ID", "TICO_NM_NOME");
            ViewBag.TiposPessoa = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.Regimes = new SelectList(SessionMocks.Regimes, "RETR_CD_ID", "RETR_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            sexo.Add(new SelectListItem() { Text = "Outros", Value = "3" });
            ViewBag.sexo = new SelectList(sexo, "Value", "Text");

            // Prepara view
            Session["ClienteNovo"] = 0;
            USUARIO usuario = SessionMocks.UserCredentials;
            CLIENTE item = new CLIENTE();
            ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(item);
            vm.ASSI_CD_ID = SessionMocks.IdAssinante;
            vm.CLIE_DT_CADASTRO = DateTime.Today;
            vm.CLIE_IN_ATIVO = 1;
            vm.MATR_CD_ID = SessionMocks.Matriz.MATR_CD_ID;
            vm.USUA_CD_ID = usuario.USUA_CD_ID;
            vm.TIPE_CD_ID = 0;
            if (((MATRIZ)Session["Matriz"]).FILIAL.Count == 0)
            {
                vm.FILI_CD_ID = 9;
            }
            else
            {
                vm.FILI_CD_ID = usuario.FILI_CD_ID;
            }
            vm.TICO_CD_ID = 1;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirCliente(ClienteViewModel vm)
        {
            ViewBag.Tipos = new SelectList(SessionMocks.CatsClientes, "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.TiposContribuinte = new SelectList(SessionMocks.TiposContribuintes, "TICO_CD_ID", "TICO_NM_NOME");
            ViewBag.TiposPessoa = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.Regimes = new SelectList(SessionMocks.Regimes, "RETR_CD_ID", "RETR_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            sexo.Add(new SelectListItem() { Text = "Outros", Value = "3" });
            ViewBag.sexo = new SelectList(sexo, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CLIENTE item = Mapper.Map<ClienteViewModel, CLIENTE>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0018", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Carrega foto e processa alteracao
                    item.CLIE_AQ_FOTO = "~/Imagens/Base/FotoBase.jpg";
                    volta = baseApp.ValidateEdit(item, item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Clientes/" + item.CLIE_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Clientes/" + item.CLIE_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    
                    // Sucesso
                    listaMaster = new List<CLIENTE>();
                    SessionMocks.listaCliente = null;
                    Session["IncluirCliente"] = 1;
                    Session["ClienteNovo"] = item.CLIE_CD_ID;
                    SessionMocks.Clientes = baseApp.GetAllItens();
                    if (SessionMocks.voltaCliente == 2)
                    {
                        return RedirectToAction("EditarCliente", new { id = item.CLIE_CD_ID });
                    }

                    SessionMocks.idVolta = item.CLIE_CD_ID;
                    return Json(item.CLIE_CD_ID);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                vm.TIPE_CD_ID = 0;
                vm.SEXO_CD_ID = 0;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult EditarCliente(Int32 id)
        {
            // Prepara view
            ViewBag.Tipos = new SelectList(SessionMocks.CatsClientes, "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.TiposContribuinte = new SelectList(SessionMocks.TiposContribuintes, "TICO_CD_ID", "TICO_NM_NOME");
            ViewBag.TiposPessoa = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.Regimes = new SelectList(SessionMocks.Regimes, "RETR_CD_ID", "RETR_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            sexo.Add(new SelectListItem() { Text = "Outros", Value = "3" });
            ViewBag.sexo = new SelectList(sexo, "Value", "Text");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");
            CLIENTE item = baseApp.GetItemById(id);

            // Indicadores
            ViewBag.Vendas = item.PEDIDO_VENDA.Count;
            ViewBag.Servicos = 0;
            ViewBag.Atendimentos = item.ATENDIMENTO.Count;
            ViewBag.Incluir = (Int32)Session["IncluirCliente"];

            objetoAntes = item;
            SessionMocks.cliente = item;
            SessionMocks.idVolta = id;
            SessionMocks.voltaCEP = 1;
            ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarCliente(ClienteViewModel vm)
        {
            ViewBag.Tipos = new SelectList(SessionMocks.CatsClientes, "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.TiposContribuinte = new SelectList(SessionMocks.TiposContribuintes, "TICO_CD_ID", "TICO_NM_NOME");
            ViewBag.TiposPessoa = new SelectList(SessionMocks.TiposPessoas, "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.Regimes = new SelectList(SessionMocks.Regimes, "RETR_CD_ID", "RETR_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            sexo.Add(new SelectListItem() { Text = "Outros", Value = "3" });
            ViewBag.sexo = new SelectList(sexo, "Value", "Text");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");

            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CLIENTE item = Mapper.Map<ClienteViewModel, CLIENTE>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<CLIENTE>();
                    SessionMocks.listaCliente = null;
                    Session["IncluirCliente"] = 0;
                    if (SessionMocks.voltaCliente == 2)
                    {
                        return RedirectToAction("VerCardsCliente");
                    }
                    return RedirectToAction("MontarTelaCliente");
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
        public ActionResult ExcluirCliente(Int32 id)
        {
            // Prepara view
            CLIENTE item = baseApp.GetItemById(id);
            ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirCliente(ClienteViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                CLIENTE item = Mapper.Map<ClienteViewModel, CLIENTE>(vm);
                Int32 volta = baseApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0065", CultureInfo.CurrentCulture);
                    return View(vm);
                }

                // Sucesso
                listaMaster = new List<CLIENTE>();
                SessionMocks.listaCliente = null;
                return RedirectToAction("MontarTelaCliente");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult ReativarCliente(Int32 id)
        {
            // Prepara view
            CLIENTE item = baseApp.GetItemById(id);
            ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarCliente(ClienteViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                CLIENTE item = Mapper.Map<ClienteViewModel, CLIENTE>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<CLIENTE>();
                SessionMocks.listaCliente = null;
                return RedirectToAction("MontarTelaCliente");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        public ActionResult VerCardsCliente()
        {
            // Carrega listas
            if (SessionMocks.listaCliente == null)
            {
                listaMaster = baseApp.GetAllItens();
                SessionMocks.listaCliente = listaMaster;
            }
            ViewBag.Listas = SessionMocks.listaCliente;
            ViewBag.Title = "Clientes";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Tipos = new SelectList(SessionMocks.CatsClientes, "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");

            // Indicadores
            ViewBag.Clientes = SessionMocks.listaCliente.Count;

            // Abre view
            objeto = new CLIENTE();
            SessionMocks.voltaCliente = 2;
            return View(objeto);
        }

        [HttpGet]
        public ActionResult VerAnexoCliente(Int32 id)
        {
            // Prepara view
            CLIENTE_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoCliente()
        {
            return RedirectToAction("EditarCliente", new { id = SessionMocks.idVolta });
        }

        public FileResult DownloadCliente(Int32 id)
        {
            CLIENTE_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.CLAN_AQ_ARQUIVO;
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
        public ActionResult UploadFileCliente(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoCliente");
            }

            CLIENTE item = baseApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);

            if (fileName.Length > 250)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoCliente");
            }

            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Clientes/" + item.CLIE_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            CLIENTE_ANEXO foto = new CLIENTE_ANEXO();
            foto.CLAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.CLAN_DT_ANEXO = DateTime.Today;
            foto.CLAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.CLAN_IN_TIPO = tipo;
            foto.CLAN_NM_TITULO = fileName;
            foto.CLIE_CD_ID = item.CLIE_CD_ID;

            item.CLIENTE_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoCliente");
        }

        [HttpPost]
        public JsonResult UploadFileCliente_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
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
                    UploadFotoCliente(file);

                    count++;
                }
                else
                {
                    UploadFileCliente(file);
                }
            }

            return Json("1"); // VoltarAnexoCliente
        }

        [HttpPost]
        public ActionResult UploadFotoCliente(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoCliente");
            }

            // Recupera arquivo
            CLIENTE item = baseApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 250)
            {
                ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture);
                return RedirectToAction("VoltarAnexoCliente");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Clientes/" + item.CLIE_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Checa extensão
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                // Salva arquivo
                file.SaveAs(path);

                // Gravar registro
                item.CLIE_AQ_FOTO = "~" + caminho + fileName;
                objetoAntes = item;
                Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            }
            else
            {
                ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture);
            }
            return RedirectToAction("VoltarAnexoCliente");
        }

        [HttpGet]
        public ActionResult BuscarCEPCliente1()
        {
            // Prepara view
            if (SessionMocks.voltaCEP == 1)
            {
                //CLIENTE item = baseApp.GetItemById(SessionMocks.idVolta);
                CLIENTE item = SessionMocks.cliente;
                ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(item);
                vm.CLIE_NR_CEP_BUSCA = String.Empty;
                if (vm.UF != null)
                {
                    vm.CLIE_SG_UF = vm.UF.UF_SG_SIGLA;
                }
                else
                {
                    vm.CLIE_SG_UF = String.Empty;
                }
                return View(vm);
            }
            else
            {
                ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(SessionMocks.cliente);
                vm.CLIE_NR_CEP_BUSCA = String.Empty;
                return View(vm);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult BuscarCEPCliente1(ClienteViewModel vm)
        {
                try
                {
                    // Atualiza cliente
                    CLIENTE item = SessionMocks.cliente;
                    CLIENTE cli = new CLIENTE();
                    cli.ASSI_CD_ID = item.ASSI_CD_ID;
                    cli.ASSI_CD_ID = item.ASSI_CD_ID;
                    cli.CACL_CD_ID = item.CACL_CD_ID;
                    cli.CLIE_AQ_FOTO = item.CLIE_AQ_FOTO;
                    cli.CLIE_CD_ID = item.CLIE_CD_ID;
                    cli.CLIE_DT_CADASTRO = item.CLIE_DT_CADASTRO;
                    cli.CLIE_DT_NASCIMENTO = item.CLIE_DT_NASCIMENTO;
                    cli.CLIE_IN_ATIVO = item.CLIE_IN_ATIVO;
                    cli.CLIE_IN_TIPO_PESSOA = item.CLIE_IN_TIPO_PESSOA;
                    cli.CLIE_NM_BAIRRO = item.CLIE_NM_BAIRRO;
                    cli.CLIE_NM_BAIRRO_ENTREGA = item.CLIE_NM_BAIRRO_ENTREGA;
                    cli.CLIE_NM_CIDADE = item.CLIE_NM_CIDADE;
                    cli.CLIE_NM_CIDADE_ENTREGA = item.CLIE_NM_CIDADE_ENTREGA;
                    cli.CLIE_NM_EMAIL = item.CLIE_NM_EMAIL;
                    cli.CLIE_NM_EMAIL_DANFE = item.CLIE_NM_EMAIL_DANFE;
                    cli.CLIE_NM_ENDERECO = item.CLIE_NM_ENDERECO;
                    cli.CLIE_NM_ENDERECO_ENTREGA = item.CLIE_NM_ENDERECO_ENTREGA;
                    cli.CLIE_NM_MAE = item.CLIE_NM_MAE;
                    cli.CLIE_NM_NACIONALIDADE = item.CLIE_NM_NACIONALIDADE;
                    cli.CLIE_NM_NATURALIDADE = item.CLIE_NM_NATURALIDADE;
                    cli.CLIE_NM_NOME = item.CLIE_NM_NOME;
                    cli.CLIE_NM_PAI = item.CLIE_NM_PAI;
                    cli.CLIE_NM_RAZAO = item.CLIE_NM_RAZAO;
                    cli.CLIE_NM_REDES_SOCIAIS = item.CLIE_NM_REDES_SOCIAIS;
                    cli.CLIE_NM_WEBSITE = item.CLIE_NM_WEBSITE;
                    cli.CLIE_NR_CELULAR = item.CLIE_NR_CELULAR;
                    cli.CLIE_NR_CEP = item.CLIE_NR_CEP;
                    cli.CLIE_NR_CEP_ENTREGA = item.CLIE_NR_CEP_ENTREGA;
                    cli.CLIE_NR_CNPJ = item.CLIE_NR_CNPJ;
                    cli.CLIE_NR_CPF = item.CLIE_NR_CPF;
                    cli.CLIE_NR_INSCRICAO_ESTADUAL = item.CLIE_NR_INSCRICAO_ESTADUAL;
                    cli.CLIE_NR_INSCRICAO_MUNICIPAL = item.CLIE_NR_INSCRICAO_MUNICIPAL;
                    cli.CLIE_NR_SUFRAMA = item.CLIE_NR_SUFRAMA;
                    cli.CLIE_NR_TELEFONES = item.CLIE_NR_TELEFONES;
                    cli.CLIE_NR_TELEFONE_ADICIONAL = item.CLIE_NR_TELEFONE_ADICIONAL;
                    cli.CLIE_SG_NATURALIADE_UF = item.CLIE_SG_NATURALIADE_UF;
                    cli.CLIE_SG_UF = item.CLIE_SG_UF;
                    cli.CLIE_SG_UF_ENTREGA = item.CLIE_SG_UF_ENTREGA;
                    cli.CLIE_TX_OBSERVACOES = item.CLIE_TX_OBSERVACOES;
                    cli.CLIE_UF_CD_ENTREGA = item.CLIE_UF_CD_ENTREGA;
                    cli.CLIE_VL_LIMITE_CREDITO = item.CLIE_VL_LIMITE_CREDITO;
                    cli.CLIE_VL_SALDO = item.CLIE_VL_SALDO;
                    cli.FILI_CD_ID = item.FILI_CD_ID;
                    cli.UF_CD_ID = item.UF_CD_ID;
                    cli.USUA_CD_ID = item.USUA_CD_ID;
                    cli.MATR_CD_ID = item.MATR_CD_ID;
                    cli.RETR_CD_ID = item.RETR_CD_ID;
                    cli.SEXO_CD_ID = item.SEXO_CD_ID;
                    cli.TIPE_CD_ID = item.TIPE_CD_ID;
                    cli.TICO_CD_ID = item.TICO_CD_ID;

                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = baseApp.ValidateEdit(cli, cli);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<CLIENTE>();
                    SessionMocks.listaCliente = null;
                    return RedirectToAction("EditarCliente", new { id = SessionMocks.idVolta });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
        }

        [HttpGet]
        public ActionResult BuscarCEPCliente2()
        {
            // Prepara view
            if (SessionMocks.voltaCEP == 1)
            {
                //CLIENTE item = baseApp.GetItemById(SessionMocks.idVolta);
                CLIENTE item = SessionMocks.cliente;
                ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(item);
                vm.CLIE_NR_CEP_BUSCA = String.Empty;
                vm.CLIE_SG_UF_ENTREGA = vm.UF.UF_SG_SIGLA;
                return View(vm);
            }
            else
            {
                ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(SessionMocks.cliente);
                vm.CLIE_NR_CEP_BUSCA = String.Empty;
                return View(vm);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult BuscarCEPCliente2(ClienteViewModel vm)
        {
                try
                {
                    // Atualiza cliente
                    CLIENTE item = SessionMocks.cliente;
                    CLIENTE cli = new CLIENTE();
                    cli.ASSI_CD_ID = item.ASSI_CD_ID;
                    cli.ASSI_CD_ID = item.ASSI_CD_ID;
                    cli.CACL_CD_ID = item.CACL_CD_ID;
                    cli.CLIE_AQ_FOTO = item.CLIE_AQ_FOTO;
                    cli.CLIE_CD_ID = item.CLIE_CD_ID;
                    cli.CLIE_DT_CADASTRO = item.CLIE_DT_CADASTRO;
                    cli.CLIE_DT_NASCIMENTO = item.CLIE_DT_NASCIMENTO;
                    cli.CLIE_IN_ATIVO = item.CLIE_IN_ATIVO;
                    cli.CLIE_IN_TIPO_PESSOA = item.CLIE_IN_TIPO_PESSOA;
                    cli.CLIE_NM_BAIRRO = item.CLIE_NM_BAIRRO;
                    cli.CLIE_NM_BAIRRO_ENTREGA = item.CLIE_NM_BAIRRO_ENTREGA;
                    cli.CLIE_NM_CIDADE = item.CLIE_NM_CIDADE;
                    cli.CLIE_NM_CIDADE_ENTREGA = item.CLIE_NM_CIDADE_ENTREGA;
                    cli.CLIE_NM_EMAIL = item.CLIE_NM_EMAIL;
                    cli.CLIE_NM_EMAIL_DANFE = item.CLIE_NM_EMAIL_DANFE;
                    cli.CLIE_NM_ENDERECO = item.CLIE_NM_ENDERECO;
                    cli.CLIE_NM_ENDERECO_ENTREGA = item.CLIE_NM_ENDERECO_ENTREGA;
                    cli.CLIE_NM_MAE = item.CLIE_NM_MAE;
                    cli.CLIE_NM_NACIONALIDADE = item.CLIE_NM_NACIONALIDADE;
                    cli.CLIE_NM_NATURALIDADE = item.CLIE_NM_NATURALIDADE;
                    cli.CLIE_NM_NOME = item.CLIE_NM_NOME;
                    cli.CLIE_NM_PAI = item.CLIE_NM_PAI;
                    cli.CLIE_NM_RAZAO = item.CLIE_NM_RAZAO;
                    cli.CLIE_NM_REDES_SOCIAIS = item.CLIE_NM_REDES_SOCIAIS;
                    cli.CLIE_NM_WEBSITE = item.CLIE_NM_WEBSITE;
                    cli.CLIE_NR_CELULAR = item.CLIE_NR_CELULAR;
                    cli.CLIE_NR_CEP = item.CLIE_NR_CEP;
                    cli.CLIE_NR_CEP_ENTREGA = item.CLIE_NR_CEP_ENTREGA;
                    cli.CLIE_NR_CNPJ = item.CLIE_NR_CNPJ;
                    cli.CLIE_NR_CPF = item.CLIE_NR_CPF;
                    cli.CLIE_NR_INSCRICAO_ESTADUAL = item.CLIE_NR_INSCRICAO_ESTADUAL;
                    cli.CLIE_NR_INSCRICAO_MUNICIPAL = item.CLIE_NR_INSCRICAO_MUNICIPAL;
                    cli.CLIE_NR_SUFRAMA = item.CLIE_NR_SUFRAMA;
                    cli.CLIE_NR_TELEFONES = item.CLIE_NR_TELEFONES;
                    cli.CLIE_NR_TELEFONE_ADICIONAL = item.CLIE_NR_TELEFONE_ADICIONAL;
                    cli.CLIE_SG_NATURALIADE_UF = item.CLIE_SG_NATURALIADE_UF;
                    cli.CLIE_SG_UF = item.CLIE_SG_UF;
                    cli.CLIE_SG_UF_ENTREGA = item.CLIE_SG_UF_ENTREGA;
                    cli.CLIE_TX_OBSERVACOES = item.CLIE_TX_OBSERVACOES;
                    cli.CLIE_UF_CD_ENTREGA = item.CLIE_UF_CD_ENTREGA;
                    cli.CLIE_VL_LIMITE_CREDITO = item.CLIE_VL_LIMITE_CREDITO;
                    cli.CLIE_VL_SALDO = item.CLIE_VL_SALDO;
                    cli.FILI_CD_ID = item.FILI_CD_ID;
                    cli.UF_CD_ID = item.UF_CD_ID;
                    cli.USUA_CD_ID = item.USUA_CD_ID;
                    cli.MATR_CD_ID = item.MATR_CD_ID;
                    cli.RETR_CD_ID = item.RETR_CD_ID;
                    cli.SEXO_CD_ID = item.SEXO_CD_ID;
                    cli.TIPE_CD_ID = item.TIPE_CD_ID;
                    cli.TICO_CD_ID = item.TICO_CD_ID;

                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = baseApp.ValidateEdit(cli, cli);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<CLIENTE>();
                    SessionMocks.listaCliente = null;
                    return RedirectToAction("EditarCliente", new { id = SessionMocks.idVolta });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
        }

        public ActionResult PesquisaCEP(ClienteViewModel itemVolta)
        {
            // Chama servico ECT
            //Address end = ExternalServices.ECT_Services.GetAdressCEP(item.CLIE_NR_CEP_BUSCA);
            //Endereco end = ExternalServices.ECT_Services.GetAdressCEPService(item.CLIE_NR_CEP_BUSCA);
            CLIENTE cli = baseApp.GetItemById(SessionMocks.idVolta);
            ClienteViewModel item = Mapper.Map<CLIENTE, ClienteViewModel>(cli);

            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            String cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(itemVolta.CLIE_NR_CEP_BUSCA);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza            
            item.CLIE_NM_ENDERECO = end.Address + "/" + end.Complement;
            item.CLIE_NM_BAIRRO = end.District;
            item.CLIE_NM_CIDADE = end.City;
            item.CLIE_SG_UF = end.Uf;
            item.UF_CD_ID = baseApp.GetUFbySigla(end.Uf).UF_CD_ID;
            item.CLIE_NR_CEP = itemVolta.CLIE_NR_CEP_BUSCA;

            // Retorna
            SessionMocks.voltaCEP = 2;
            SessionMocks.cliente = Mapper.Map<ClienteViewModel, CLIENTE>(item);
            return RedirectToAction("BuscarCEPCliente1");
        }

        [HttpPost]
        public JsonResult PesquisaCEP_Javascript(String cep, int tipoEnd)
        {
            // Chama servico ECT
            //Address end = ExternalServices.ECT_Services.GetAdressCEP(item.CLIE_NR_CEP_BUSCA);
            //Endereco end = ExternalServices.ECT_Services.GetAdressCEPService(item.CLIE_NR_CEP_BUSCA);
            CLIENTE cli = baseApp.GetItemById(SessionMocks.idVolta);

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
                hash.Add("CLIE_NM_ENDERECO", end.Address + "/" + end.Complement);
                hash.Add("CLIE_NM_BAIRRO", end.District);
                hash.Add("CLIE_NM_CIDADE", end.City);
                hash.Add("CLIE_SG_UF", end.Uf);
                hash.Add("UF_CD_ID", baseApp.GetUFbySigla(end.Uf).UF_CD_ID);
                hash.Add("CLIE_NR_CEP", cep);
            }
            else if (tipoEnd == 2)
            {
                hash.Add("CLIE_NM_ENDERECO_ENTREGA", end.Address + "/" + end.Complement);
                hash.Add("CLIE_NM_BAIRRO_ENTREGA", end.District);
                hash.Add("CLIE_NM_CIDADE_ENTREGA", end.City);
                hash.Add("CLIE_SG_UF_ENTREGA", end.Uf);
                hash.Add("UF_CD_ID_ENTREGA", baseApp.GetUFbySigla(end.Uf).UF_CD_ID);
                hash.Add("CLIE_NR_CEP_ENTREGA", cep);
            }

            // Retorna
            SessionMocks.voltaCEP = 2;
            return Json(hash);
        }

        public ActionResult PesquisaCEPEntrega(ClienteViewModel itemVolta)
        {
            // Chama servico ECT
            //Address end = ExternalServices.ECT_Services.GetAdressCEP(item.CLIE_NR_CEP_BUSCA);
            //Endereco end = ExternalServices.ECT_Services.GetAdressCEPService(item.CLIE_NR_CEP_BUSCA);
            CLIENTE cli = baseApp.GetItemById(SessionMocks.idVolta);
            ClienteViewModel item = Mapper.Map<CLIENTE, ClienteViewModel>(cli);

            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            String cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(itemVolta.CLIE_NR_CEP_BUSCA);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza            
            item.CLIE_NM_ENDERECO_ENTREGA = end.Address + "/" + end.Complement;
            item.CLIE_NM_BAIRRO_ENTREGA = end.District;
            item.CLIE_NM_CIDADE_ENTREGA = end.City;
            item.CLIE_SG_UF_ENTREGA = end.Uf;
            item.CLIE_UF_CD_ENTREGA = baseApp.GetUFbySigla(end.Uf).UF_CD_ID;
            item.CLIE_NR_CEP_ENTREGA = itemVolta.CLIE_NR_CEP_BUSCA;

            // Retorna
            SessionMocks.voltaCEP = 2;
            SessionMocks.cliente = Mapper.Map<ClienteViewModel, CLIENTE>(item);
            return RedirectToAction("BuscarCEPCliente2");
        }

        [HttpGet]
        public ActionResult EditarContato(Int32 id)
        {
            // Prepara view
            CLIENTE_CONTATO item = baseApp.GetContatoById(id);
            objetoAntes = SessionMocks.cliente;
            ClienteContatoViewModel vm = Mapper.Map<CLIENTE_CONTATO, ClienteContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarContato(ClienteContatoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CLIENTE_CONTATO item = Mapper.Map<ClienteContatoViewModel, CLIENTE_CONTATO>(vm);
                    Int32 volta = baseApp.ValidateEditContato(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoCliente");
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
        public ActionResult ExcluirContato(Int32 id)
        {
            CLIENTE_CONTATO item = baseApp.GetContatoById(id);
            objetoAntes = SessionMocks.cliente;
            item.CLCO_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateEditContato(item);
            return RedirectToAction("VoltarAnexoCliente");
        }

        [HttpGet]
        public ActionResult ReativarContato(Int32 id)
        {
            CLIENTE_CONTATO item = baseApp.GetContatoById(id);
            objetoAntes = SessionMocks.cliente;
            item.CLCO_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditContato(item);
            return RedirectToAction("VoltarAnexoCliente");
        }

        [HttpGet]
        public ActionResult IncluirContato()
        {
            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            CLIENTE_CONTATO item = new CLIENTE_CONTATO();
            ClienteContatoViewModel vm = Mapper.Map<CLIENTE_CONTATO, ClienteContatoViewModel>(item);
            vm.CLIE_CD_ID = SessionMocks.idVolta;
            vm.CLCO_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirContato(ClienteContatoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CLIENTE_CONTATO item = Mapper.Map<ClienteContatoViewModel, CLIENTE_CONTATO>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = baseApp.ValidateCreateContato(item);
                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoCliente");
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
        public ActionResult EditarReferencia(Int32 id)
        {
            // Prepara view
            CLIENTE_REFERENCIA item = baseApp.GetReferenciaById(id);
            objetoAntes = SessionMocks.cliente;
            ClienteReferenciaViewModel vm = Mapper.Map<CLIENTE_REFERENCIA, ClienteReferenciaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarReferencia(ClienteReferenciaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CLIENTE_REFERENCIA item = Mapper.Map<ClienteReferenciaViewModel, CLIENTE_REFERENCIA>(vm);
                    Int32 volta = baseApp.ValidateEditReferencia(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoCliente");
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
        public ActionResult ExcluirReferencia(Int32 id)
        {
            CLIENTE_REFERENCIA item = baseApp.GetReferenciaById(id);
            objetoAntes = SessionMocks.cliente;
            item.CLRE_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateEditReferencia(item);
            return RedirectToAction("VoltarAnexoCliente");
        }

        [HttpGet]
        public ActionResult ReativarReferencia(Int32 id)
        {
            CLIENTE_REFERENCIA item = baseApp.GetReferenciaById(id);
            objetoAntes = SessionMocks.cliente;
            item.CLRE_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditReferencia(item);
            return RedirectToAction("VoltarAnexoCliente");
        }

        [HttpGet]
        public ActionResult IncluirReferencia()
        {
            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            CLIENTE_REFERENCIA item = new CLIENTE_REFERENCIA();
            ClienteReferenciaViewModel vm = Mapper.Map<CLIENTE_REFERENCIA, ClienteReferenciaViewModel>(item);
            vm.CLIE_CD_ID = SessionMocks.idVolta;
            vm.CLRE_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirReferencia(ClienteReferenciaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CLIENTE_REFERENCIA item = Mapper.Map<ClienteReferenciaViewModel, CLIENTE_REFERENCIA>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = baseApp.ValidateCreateReferencia(item);
                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoCliente");
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
        public ActionResult IncluirTag()
        {
            // Prepara view
            List<SelectListItem> tipoTag = new List<SelectListItem>();
            tipoTag.Add(new SelectListItem() { Text = "Padrão", Value = "1" });
            tipoTag.Add(new SelectListItem() { Text = "Aviso", Value = "2" });
            tipoTag.Add(new SelectListItem() { Text = "Alarme", Value = "1" });
            tipoTag.Add(new SelectListItem() { Text = "Elogio", Value = "2" });
            ViewBag.TipoTag = new SelectList(tipoTag, "Value", "Text");
            
            USUARIO usuario = SessionMocks.UserCredentials;
            CLIENTE_TAG item = new CLIENTE_TAG();
            ClienteTagViewModel vm = Mapper.Map<CLIENTE_TAG, ClienteTagViewModel>(item);
            vm.CLIE_CD_ID = SessionMocks.idVolta;
            vm.CLTA_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirTag(ClienteTagViewModel vm)
        {
            List<SelectListItem> tipoTag = new List<SelectListItem>();
            tipoTag.Add(new SelectListItem() { Text = "Padrão", Value = "1" });
            tipoTag.Add(new SelectListItem() { Text = "Aviso", Value = "2" });
            tipoTag.Add(new SelectListItem() { Text = "Alarme", Value = "1" });
            tipoTag.Add(new SelectListItem() { Text = "Elogio", Value = "2" });
            ViewBag.TipoTag = new SelectList(tipoTag, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CLIENTE_TAG item = Mapper.Map<ClienteTagViewModel, CLIENTE_TAG>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = baseApp.ValidateCreateTag(item);
                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoCliente");
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
        public ActionResult SlideShowCliente()
        {
            // Prepara view
            CLIENTE item = baseApp.GetItemById(SessionMocks.idVolta);
            objetoAntes = item;
            ClienteViewModel vm = Mapper.Map<CLIENTE, ClienteViewModel>(item);
            return View(vm);
        }

        public ActionResult VerClientesAtraso()
        {
            // Prepara view
            SessionMocks.listaCR = crApp.GetItensAtrasoCliente().ToList();
            ViewBag.Atrasos = SessionMocks.listaCR.Select(x => x.CLIE_CD_ID).Distinct().ToList().Count;
            ViewBag.Clientes = new SelectList(baseApp.GetAllItens(), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.ContasAtrasos = SessionMocks.listaCR;
            ViewBag.ContasAtraso = SessionMocks.listaCR.Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;

            if (SessionMocks.listaRec == null)
            {
                listaMasterRec = SessionMocks.listaCR;
                SessionMocks.listaRec = listaMasterRec;
            }
            ViewBag.Listas = SessionMocks.listaRec;
            CONTA_RECEBER cr = new CONTA_RECEBER();
            return View(cr);
        }

        public ActionResult VerClientesInativos()
        {
            // Carrega listas
            if (SessionMocks.listaInativos == null)
            {
                listaMaster = baseApp.GetAllItens();
                SessionMocks.listaCliente = listaMaster;
            }
            ViewBag.Listas = SessionMocks.listaCliente;
            ViewBag.Title = "Clientesxxx";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Tipos = new SelectList(SessionMocks.CatsClientes, "CACL_CD_ID", "CACL_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            SessionMocks.cliente = null;

            // Indicadores
            ViewBag.Clientes = SessionMocks.listaCliente.Count;
            SessionMocks.listaCR = crApp.GetItensAtrasoCliente().ToList();
            ViewBag.Atrasos = SessionMocks.listaCR.Select(x => x.CLIE_CD_ID).Distinct().ToList().Count;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            ViewBag.Inativos = SessionMocks.listaCliente.Where(p => p.CLIE_IN_ATIVO == 0).ToList().Count;
            ViewBag.SemPedidos = SessionMocks.listaCliente.Where(p => p.PEDIDO_VENDA.Count == 0 || p.PEDIDO_VENDA == null).ToList().Count;

            // Abre view
            objeto = new CLIENTE();
            SessionMocks.voltaCliente = 1;
            return View(objeto);
        }

        public ActionResult VerClientesSemPedidos()
        {
            // Prepara view
            ViewBag.Listas = SessionMocks.listaCliente.Where(p => p.PEDIDO_VENDA.Count == 0 || p.PEDIDO_VENDA == null).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult FiltrarAtrasos(CONTA_RECEBER item)
        {
            try
            {
                // Executa a operação
                List<CONTA_RECEBER> listaObj = new List<CONTA_RECEBER>();
                Int32 volta = crApp.ExecuteFilter(item.CLIE_CD_ID, item.COBA_CD_ID, item.CARE_DT_LANCAMENTO, item.CARE_DS_DESCRICAO, 0, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0010", CultureInfo.CurrentCulture);
                }

                // Sucesso
                listaMasterRec = listaObj;
                SessionMocks.listaRec = listaObj;
                return RedirectToAction("VerClientesAtraso");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("VerClientesAtraso");
            }
        }

        public ActionResult VerLancamentoAtraso(Int32 id)
        {
            // Prepara view
            CONTA_RECEBER item = crApp.GetItemById(id);
            SessionMocks.cr = item;
            SessionMocks.idCRVolta = id;
            ContaReceberViewModel vm = Mapper.Map<CONTA_RECEBER, ContaReceberViewModel>(item);
            return View(vm);
        }

        public ActionResult GerarRelatorioLista()
        {
            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "ClienteLista" + "_" + data + ".pdf";
            List<CLIENTE> lista = SessionMocks.listaCliente;
            CLIENTE filtro = SessionMocks.filtroCliente;
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

            cell = new PdfPCell(new Paragraph("Clientes - Listagem", meuFont2))
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

            cell = new PdfPCell(new Paragraph("Clientes selecionados pelos parametros de filtro abaixo", meuFont1))
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

            foreach (CLIENTE item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.CATEGORIA_CLIENTE.CACL_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.CLIE_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.CLIE_NR_CPF != null)
                {
                    cell = new PdfPCell(new Paragraph(item.CLIE_NR_CPF, meuFont))
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
                if (item.CLIE_NR_CNPJ != null)
                {
                    cell = new PdfPCell(new Paragraph(item.CLIE_NR_CNPJ, meuFont))
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
                cell = new PdfPCell(new Paragraph(item.CLIE_NM_EMAIL, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.CLIE_NR_TELEFONES != null)
                {
                    cell = new PdfPCell(new Paragraph(item.CLIE_NR_TELEFONES, meuFont))
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
                if (item.CLIE_NM_CIDADE != null)
                {
                    cell = new PdfPCell(new Paragraph(item.CLIE_NM_CIDADE, meuFont))
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
                if (filtro.CACL_CD_ID > 0)
                {
                    parametros += "Categoria: " + filtro.CACL_CD_ID;
                    ja = 1;
                }
                if (filtro.CLIE_CD_ID > 0)
                {
                    CLIENTE cli = baseApp.GetItemById(filtro.CLIE_CD_ID);
                    if (ja == 0)
                    {
                        parametros += "Nome: " + cli.CLIE_NM_NOME;
                        ja = 1;
                    }
                    else
                    {
                        parametros +=  " e Nome: " + cli.CLIE_NM_NOME;
                    }
                }
                if (filtro.CLIE_NR_CPF != null)
                {
                    if (ja == 0)
                    {
                        parametros += "CPF: " + filtro.CLIE_NR_CPF;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e CPF: " + filtro.CLIE_NR_CPF;
                    }
                }
                if (filtro.CLIE_NR_CNPJ != null)
                {
                    if (ja == 0)
                    {
                        parametros += "CNPJ: " + filtro.CLIE_NR_CNPJ;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e CNPJ: " + filtro.CLIE_NR_CNPJ;
                    }
                }
                if (filtro.CLIE_NM_EMAIL != null)
                {
                    if (ja == 0)
                    {
                        parametros += "E-Mail: " + filtro.CLIE_NM_EMAIL;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e E-Mail: " + filtro.CLIE_NM_EMAIL;
                    }
                }
                if (filtro.CLIE_NM_CIDADE != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Cidade: " + filtro.CLIE_NM_CIDADE;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Cidade: " + filtro.CLIE_NM_CIDADE;
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
            CLIENTE aten = baseApp.GetItemById(SessionMocks.idVolta);
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "Cliente" + aten.CLIE_CD_ID.ToString() + "_" + data + ".pdf";
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

            cell = new PdfPCell(new Paragraph("Cliente - Detalhes", meuFont2))
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

            if (aten.TIPO_CONTRIBUINTE != null)
            {
                cell = new PdfPCell(new Paragraph("Tipo Contribuinte: " + aten.TIPO_CONTRIBUINTE.TICO_NM_NOME, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Tipo Contribuinte: -", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            cell = new PdfPCell(new Paragraph("Categoria: " + aten.CATEGORIA_CLIENTE.CACL_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Nome: " + aten.CLIE_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Razão Social: " + aten.CLIE_NM_RAZAO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (aten.CLIE_NR_CPF != null)
            {
                cell = new PdfPCell(new Paragraph("CPF: " + aten.CLIE_NR_CPF, meuFont));
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

            if (aten.CLIE_NR_CNPJ != null)
            {
                cell = new PdfPCell(new Paragraph("CNPJ: " + aten.CLIE_NR_CNPJ, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Ins.Estadual: " + aten.CLIE_NR_INSCRICAO_ESTADUAL, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Ins.Municipal: " + aten.CLIE_NR_INSCRICAO_MUNICIPAL, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                if (aten.CLIE_VL_SALDO != null)
                {
                    cell = new PdfPCell(new Paragraph("Saldo: " + CrossCutting.Formatters.DecimalFormatter(aten.CLIE_VL_SALDO.Value), meuFont));
                    cell.Border = 0;
                    cell.Colspan = 1;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("Saldo: 0,00", meuFont));
                    cell.Border = 0;
                    cell.Colspan = 1;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(cell);
                }
            }

            if (aten.REGIME_TRIBUTARIO != null)
            {
                cell = new PdfPCell(new Paragraph("Regime Tributário: " + aten.REGIME_TRIBUTARIO.RETR_NM_NOME, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Regime Tributário: -", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            cell = new PdfPCell(new Paragraph("Ins.SUFRAMA: " + aten.CLIE_NR_SUFRAMA, meuFont));
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

            cell = new PdfPCell(new Paragraph("Endereço: " + aten.CLIE_NM_ENDERECO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Bairro: " + aten.CLIE_NM_BAIRRO, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Cidade: " + aten.CLIE_NM_CIDADE, meuFont));
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
            cell = new PdfPCell(new Paragraph("CEP: " + aten.CLIE_NR_CEP, meuFont));
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

            cell = new PdfPCell(new Paragraph("Endereço de Entrega", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Endereço: " + aten.CLIE_NM_ENDERECO_ENTREGA, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Bairro: " + aten.CLIE_NM_BAIRRO_ENTREGA, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Cidade: " + aten.CLIE_NM_CIDADE_ENTREGA, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            if (aten.UF1 != null)
            {
                cell = new PdfPCell(new Paragraph("UF: " + aten.UF1.UF_SG_SIGLA, meuFont));
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
            cell = new PdfPCell(new Paragraph("CEP: " + aten.CLIE_NR_CEP_ENTREGA, meuFont));
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

            cell = new PdfPCell(new Paragraph("E-Mail: " + aten.CLIE_NM_EMAIL, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("E-Mail DANFE: " + aten.CLIE_NM_EMAIL_DANFE, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Redes Sociais: " + aten.CLIE_NM_REDES_SOCIAIS, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Website: " + aten.CLIE_NM_WEBSITE, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Telefone: " + aten.CLIE_NR_TELEFONES, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Celular: " + aten.CLIE_NR_CELULAR, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Tel.Adicional: " + aten.CLIE_NR_TELEFONE_ADICIONAL, meuFont));
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

            // Lista de Contatos
            if (aten.CLIENTE_CONTATO.Count > 0)
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

                foreach (CLIENTE_CONTATO item in aten.CLIENTE_CONTATO)
                {
                    cell = new PdfPCell(new Paragraph(item.CLCO_NM_NOME, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.CLCO_NM_CARGO, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.CLCO_NM_EMAIL, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.CLCO_NM_TELEFONE, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.CLCO_IN_ATIVO == 1)
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

            // Dados Pessoais
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Dados Pessoais", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Nome do Pai: " + aten.CLIE_NM_PAI, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Nome da Mãe: " + aten.CLIE_NM_MAE, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (aten.CLIE_DT_NASCIMENTO != null)
            {
                cell = new PdfPCell(new Paragraph("Data Nascimento: " + aten.CLIE_DT_NASCIMENTO.Value.ToShortDateString(), meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Data Nascimento: ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            if (aten.SEXO != null)
            {
                cell = new PdfPCell(new Paragraph("Sexo: " + aten.SEXO.SEXO_NM_NOME, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Sexo: - ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
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

            cell = new PdfPCell(new Paragraph("Naturalidade: " + aten.CLIE_NM_NATURALIDADE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("UF Naturalidade: " + aten.CLIE_SG_NATURALIADE_UF, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Nacionalidade: " + aten.CLIE_NM_NACIONALIDADE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph(" " ,meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Dados Comerciais
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Dados Comerciais", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (aten.USUARIO != null)
            {
                cell = new PdfPCell(new Paragraph("Vendedor: " + aten.USUARIO.USUA_NM_NOME, meuFont));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Vendedor: -" ,meuFont));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            if (aten.CLIE_VL_LIMITE_CREDITO != null)
            {
                cell = new PdfPCell(new Paragraph("Limite de Crédito: " + CrossCutting.Formatters.DecimalFormatter(aten.CLIE_VL_LIMITE_CREDITO.Value), meuFont));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("Limite de Crédito: 0,00", meuFont));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Observações
            Chunk chunk1 = new Chunk("Observações: " + aten.CLIE_TX_OBSERVACOES, FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            // Pedidos de Venda
            if (aten.PEDIDO_VENDA.Count > 0)
            {
                // Linha Horizontal
                line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
                pdfDoc.Add(line1);

                cell = new PdfPCell(new Paragraph("Pedidos de Venda", meuFontBold));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);

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
                cell = new PdfPCell(new Paragraph("Emissão", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Status", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Aprovação", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                foreach (PEDIDO_VENDA item in aten.PEDIDO_VENDA)
                {
                    cell = new PdfPCell(new Paragraph(item.PEVE_NM_NOME, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.PEVE_DT_DATA.ToShortDateString(), meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.PEVE_IN_STATUS == 1)
                    {
                        cell = new PdfPCell(new Paragraph("Emissão", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else if (item.PEVE_IN_STATUS == 2)
                    {
                        cell = new PdfPCell(new Paragraph("Em Aprovação", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else if (item.PEVE_IN_STATUS == 3)
                    {
                        cell = new PdfPCell(new Paragraph("Aprovado", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else if (item.PEVE_IN_STATUS == 4)
                    {
                        cell = new PdfPCell(new Paragraph("Cancelado", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else if (item.PEVE_IN_STATUS == 5)
                    {
                        cell = new PdfPCell(new Paragraph("Encerrado", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    if (item.PEVE_DT_APROVACAO != null)
                    {
                        cell = new PdfPCell(new Paragraph(item.PEVE_DT_APROVACAO.Value.ToShortDateString(), meuFont))
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

            // Atendimento
            if (aten.PEDIDO_VENDA.Count > 0)
            {
                // Linha Horizontal
                line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
                pdfDoc.Add(line1);

                cell = new PdfPCell(new Paragraph("Pedidos de Venda", meuFontBold));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);

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
                cell = new PdfPCell(new Paragraph("Emissão", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Status", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Aprovação", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                foreach (PEDIDO_VENDA item in aten.PEDIDO_VENDA)
                {
                    cell = new PdfPCell(new Paragraph(item.PEVE_NM_NOME, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.PEVE_DT_DATA.ToShortDateString(), meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.PEVE_IN_STATUS == 1)
                    {
                        cell = new PdfPCell(new Paragraph("Emissão", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else if (item.PEVE_IN_STATUS == 2)
                    {
                        cell = new PdfPCell(new Paragraph("Em Aprovação", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else if (item.PEVE_IN_STATUS == 3)
                    {
                        cell = new PdfPCell(new Paragraph("Aprovado", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else if (item.PEVE_IN_STATUS == 4)
                    {
                        cell = new PdfPCell(new Paragraph("Cancelado", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else if (item.PEVE_IN_STATUS == 5)
                    {
                        cell = new PdfPCell(new Paragraph("Encerrado", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    if (item.PEVE_DT_APROVACAO != null)
                    {
                        cell = new PdfPCell(new Paragraph(item.PEVE_DT_APROVACAO.Value.ToShortDateString(), meuFont))
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

            return RedirectToAction("VoltarAnexoCliente");
        }
    }
}