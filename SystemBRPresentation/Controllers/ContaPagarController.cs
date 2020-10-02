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
using ACBr.Net.Core.Extensions;
using System.Drawing;
using System.Runtime.Remoting.Messaging;

namespace SystemBRPresentation.Controllers
{
    public class ContaPagarController : Controller
    {
        private readonly IBancoAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IContaBancariaAppService contaApp;
        private readonly IContaPagarAppService cpApp;
        private readonly IFornecedorAppService forApp;
        private readonly IContaBancariaAppService cbApp;
        private readonly IContaPagarParcelaAppService ppApp;
        private readonly IUsuarioAppService usuApp;
        private readonly ICentroCustoAppService ccApp;
        private readonly IPeriodicidadeAppService perApp;
        private readonly IFormaPagamentoAppService fpApp;
        private readonly IContaPagarRateioAppService ratApp;

        private String msg;
        private Exception exception;
        String extensao = String.Empty;
        BANCO objetoBanco = new BANCO();
        BANCO objetoBancoAntes = new BANCO();
        List<BANCO> listaMasterBanco = new List<BANCO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        CONTA_BANCO objConta = new CONTA_BANCO();
        CONTA_BANCO objContaAntes = new CONTA_BANCO();
        List<CONTA_BANCO> listaMasterConta = new List<CONTA_BANCO>();
        CONTA_PAGAR objetoCP = new CONTA_PAGAR();
        CONTA_PAGAR objetoCPAntes = new CONTA_PAGAR();
        List<CONTA_PAGAR> listaCPMaster = new List<CONTA_PAGAR>();
        CONTA_PAGAR_PARCELA objetoCPP = new CONTA_PAGAR_PARCELA();
        CONTA_PAGAR_PARCELA objetoCPPAntes = new CONTA_PAGAR_PARCELA();
        List<CONTA_PAGAR_PARCELA> listaCPPMaster = new List<CONTA_PAGAR_PARCELA>();

        public ContaPagarController(IBancoAppService baseApps, ILogAppService logApps, IContaBancariaAppService contaApps, IContaPagarAppService cpApps, IFornecedorAppService forApps, IContaPagarParcelaAppService ppApps, IContaBancariaAppService cbApps, IUsuarioAppService usuApps, ICentroCustoAppService ccApps, IPeriodicidadeAppService perApps, IFormaPagamentoAppService fpApps, IContaPagarRateioAppService ratApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            contaApp = contaApps;
            forApp = forApps;
            cpApp = cpApps;
            cbApp = cbApps;
            ppApp = ppApps;
            usuApp = usuApps;
            ccApp = ccApps;
            perApp = perApps;
            fpApp = fpApps;
            ratApp = ratApps;
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

        public ActionResult VoltarDashboard()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult DashboardAdministracao()
        {
            listaCPMaster = new List<CONTA_PAGAR>();
            SessionMocks.contaPagar = null;
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaCP()
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
            if (SessionMocks.listaCP == null)
            {
                listaCPMaster = cpApp.GetAllItens();
                SessionMocks.listaCP = listaCPMaster;
            }
            ViewBag.Listas = SessionMocks.listaCP;
            ViewBag.Title = "Contas a Pagar";
            SessionMocks.Fornecedores = forApp.GetAllItens();
            ViewBag.Forn = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.CC = new SelectList(SessionMocks.CentroCustos, "CECU_CD_ID", "CECU_NM_EXIBE");
            List<SelectListItem> tipoFiltro = new List<SelectListItem>();
            tipoFiltro.Add(new SelectListItem() { Text = "Somente em Aberto", Value = "1" });
            tipoFiltro.Add(new SelectListItem() { Text = "Somente Fechados", Value = "2" });
            ViewBag.Filtro = new SelectList(tipoFiltro, "Value", "Text");
            if ((Int32)Session["ErroSoma"] == 2)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0083", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["ErroSoma"] == 3)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0017", CultureInfo.CurrentCulture));
            }
            Session["ErroSoma"] = 0;

            // Indicadores
            ViewBag.CPS = SessionMocks.listaCP.Count;
            ViewBag.Pago = SessionMocks.listaCP.Where(p => p.CAPA_IN_LIQUIDADA == 1 & p.CAPA_DT_VENCIMENTO.Value.Month == DateTime.Today.Date.Month).ToList().Sum(p => p.CAPA_VL_VALOR_PAGO).Value;
            ViewBag.APagar = SessionMocks.listaCP.Where(p => p.CAPA_IN_LIQUIDADA == 0 & p.CAPA_DT_VENCIMENTO.Value.Month == DateTime.Today.Date.Month).ToList().Sum(p => p.CAPA_VL_VALOR).Value;
            ViewBag.Atrasos = SessionMocks.listaCP.Where(p => p.CAPA_NR_ATRASO > 0 & p.CAPA_DT_VENCIMENTO < DateTime.Today.Date).ToList().Sum(p => p.CAPA_VL_VALOR).Value;
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;

            SessionMocks.Fornecedores = forApp.GetAllItens();
            SessionMocks.ContasBancarias = contaApp.GetAllItens();

            // Abre view
            objetoCP = new CONTA_PAGAR();
            objetoCP.CAPA_DT_LANCAMENTO = null;
            objetoCP.CAPA_DT_VENCIMENTO = null;
            objetoCP.CAPA_DT_LIQUIDACAO = null;
            return View(objetoCP);
        }

        public ActionResult RetirarFiltroCP()
        {
            SessionMocks.listaCP = null;
            SessionMocks.filtroCP = null;
            return RedirectToAction("MontarTelaCP");
        }

        public ActionResult MostrarTudoCP()
        {
            listaCPMaster = cpApp.GetAllItensAdm();
            SessionMocks.filtroCP = null;
            SessionMocks.listaCP = listaCPMaster;
            return RedirectToAction("MontarTelaCP");
        }

        [HttpPost]
        public ActionResult FiltrarCP(CONTA_PAGAR item)
        {
            try
            {
                // Executa a operação
                List<CONTA_PAGAR> listaObj = new List<CONTA_PAGAR>();
                SessionMocks.filtroCP = item;
                Int32 volta = cpApp.ExecuteFilter(item.FORN_CD_ID, item.CECU_CD_ID, item.CAPA_DT_LANCAMENTO, item.CAPA_DS_DESCRICAO, item.CAPA_IN_ABERTOS, item.CAPA_DT_VENCIMENTO, item.CAPA_DT_LIQUIDACAO, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["ErroSoma"] = 3;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0017", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaCPMaster = listaObj;
                SessionMocks.listaCP = listaObj;
                return RedirectToAction("MontarTelaCP");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaCP");
            }
        }

        public ActionResult VoltarBaseCP()
        {
            listaCPMaster = new List<CONTA_PAGAR>();
            SessionMocks.listaCP = null;
            return RedirectToAction("MontarTelaCP");
        }

        [HttpGet]
        public ActionResult VerPagamentosMes()
        {
            List<CONTA_PAGAR> lista = SessionMocks.listaCP.Where(p => p.CAPA_IN_LIQUIDADA == 1 & p.CAPA_DT_LIQUIDACAO.Value.Month == DateTime.Today.Date.Month).ToList();
            ViewBag.ListaCP = lista;
            ViewBag.LR = lista.Count;
            ViewBag.Valor = lista.Sum(x => x.CAPA_VL_VALOR_PAGO);
            return View();
        }

        [HttpGet]
        public ActionResult VerAPagarMes()
        {
            List<CONTA_PAGAR> lista = SessionMocks.listaCP.Where(p => p.CAPA_IN_LIQUIDADA == 0 & p.CAPA_DT_VENCIMENTO.Value.Month == DateTime.Today.Date.Month).ToList();
            ViewBag.ListaCP = lista;
            ViewBag.LR = lista.Count;
            ViewBag.Valor = lista.Sum(x => x.CAPA_VL_VALOR);
            return View();
        }

        [HttpGet]
        public ActionResult VerLancamentosAtrasoCP()
        {
            List<CONTA_PAGAR> lista = SessionMocks.listaCP.Where(p => p.CAPA_NR_ATRASO > 0 & p.CAPA_DT_VENCIMENTO < DateTime.Today.Date).ToList();
            ViewBag.ListaCP = lista;
            ViewBag.LR = lista.Count;
            ViewBag.Valor = lista.Sum(x => x.CAPA_VL_VALOR);
            return View();
        }

        [HttpGet]
        public ActionResult VerCP(Int32 id)
        {
            // Prepara view
            CONTA_PAGAR item = cpApp.GetItemById(id);
            SessionMocks.contaPagar = item;
            ContaPagarViewModel vm = Mapper.Map<CONTA_PAGAR, ContaPagarViewModel>(item);
            SessionMocks.idVolta = id;
            SessionMocks.idCPVolta = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult UploadFileLancamentoCP(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                Session["ErroSoma"] = 4;
                return RedirectToAction("VoltarAnexoCP");
            }

            CONTA_PAGAR item = cpApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);

            if (fileName.Length > 250)
            {
                Session["ErroSoma"] = 5;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoCP");
            }

            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/ContaPagar/" + item.CAPA_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            CONTA_PAGAR_ANEXO foto = new CONTA_PAGAR_ANEXO();
            foto.CPAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.CPAN_DT_ANEXO = DateTime.Today;
            foto.CPAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.CPAN_IN_TIPO = tipo;
            foto.CPAN_NM_TITULO = fileName;
            foto.CAPA_CD_ID = item.CAPA_CD_ID;

            item.CONTA_PAGAR_ANEXO.Add(foto);
            objetoCPAntes = item;
            Int32 volta = cpApp.ValidateEdit(item, objetoCPAntes, usu);
            if (SessionMocks.idCPVolta == 1)
            {
                return RedirectToAction("VoltarAnexoVerCP");
            }
            return RedirectToAction("VoltarAnexoCP");
        }

        [HttpPost]
        public JsonResult UploadFileCP_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
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
                    //UploadFotoCliente(file);

                    //count++;
                }
                else
                {
                    UploadFileLancamentoCP(file);
                }
            }

            return Json("1"); // VoltarAnexoCliente
        }

        [HttpGet]
        public ActionResult VerAnexoLancamentoCP(Int32 id)
        {
            // Prepara view
            CONTA_PAGAR_ANEXO item = cpApp.GetAnexoById(id);
            return View(item);
        }

        public FileResult DownloadLancamentoCP(Int32 id)
        {
            CONTA_PAGAR_ANEXO item = cpApp.GetAnexoById(id);
            String arquivo = item.CPAN_AQ_ARQUIVO;
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
        public ActionResult SlideShowCP()
        {
            // Prepara view
            CONTA_PAGAR item = cpApp.GetItemById(SessionMocks.idVolta);
            objetoCPAntes = item;
            ContaPagarViewModel vm = Mapper.Map<CONTA_PAGAR, ContaPagarViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult ExcluirCP(Int32 id)
        {
            USUARIO usu = SessionMocks.UserCredentials;
            CONTA_PAGAR item = cpApp.GetItemById(id);
            objetoCPAntes = SessionMocks.contaPagar;
            item.CAPA_IN_ATIVO = 0;
            Int32 volta = cpApp.ValidateDelete(item, usu);
            if (volta == 1)
            {
                Session["ErroSoma"] = 2;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0083", CultureInfo.CurrentCulture));
            }
            listaCPMaster = new List<CONTA_PAGAR>();
            SessionMocks.listaCP = null;
            return RedirectToAction("MontarTelaCP");
        }

        [HttpGet]
        public ActionResult ReativarCP(Int32 id)
        {
            USUARIO usu = SessionMocks.UserCredentials;
            CONTA_PAGAR item = cpApp.GetItemById(id);
            objetoCPAntes = SessionMocks.contaPagar;
            item.CAPA_IN_ATIVO = 1;
            Int32 volta = cpApp.ValidateReativar(item, usu);
            listaCPMaster = new List<CONTA_PAGAR>();
            SessionMocks.listaCP = null;
            return RedirectToAction("MontarTelaCP");
        }

        [HttpGet]
        public ActionResult VerParcelaCP(Int32 id)
        {
            // Prepara view
            CONTA_PAGAR_PARCELA item = cpApp.GetParcelaById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoVerCP()
        {
            return RedirectToAction("VerCP", new { id = SessionMocks.idVolta });
        }

        public ActionResult VoltarAnexoCP()
        {
            return RedirectToAction("EditarCP", new { id = SessionMocks.idVolta });
        }

        [HttpGet]
        public ActionResult IncluirCP()
        {
            // Prepara listas
            ViewBag.Forn = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllDespesas(), "CECU_CD_ID", "CECU_NM_EXIBE");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(fpApp.GetAllItens(1), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            List<SelectListItem> tipoDoc = new List<SelectListItem>();
            tipoDoc.Add(new SelectListItem() { Text = "Boleto", Value = "1" });
            tipoDoc.Add(new SelectListItem() { Text = "Nota", Value = "2" });
            tipoDoc.Add(new SelectListItem() { Text = "Recibo", Value = "3" });
            tipoDoc.Add(new SelectListItem() { Text = "Fatura", Value = "4" });
            tipoDoc.Add(new SelectListItem() { Text = "Crediário", Value = "5" });
            ViewBag.TipoDoc = new SelectList(tipoDoc, "Value", "Text");

            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            CONTA_PAGAR item = new CONTA_PAGAR();
            ContaPagarViewModel vm = Mapper.Map<CONTA_PAGAR, ContaPagarViewModel>(item);
            vm.CAPA_DT_LANCAMENTO = DateTime.Today.Date;
            vm.CAPA_IN_ATIVO = 1;
            vm.CAPA_DT_COMPETENCIA = DateTime.Today.Date;
            vm.CAPA_DT_VENCIMENTO = DateTime.Today.Date.AddDays(30);
            vm.CAPA_IN_LIQUIDADA = 0;
            vm.CAPA_IN_PAGA_PARCIAL = 0;
            vm.CAPA_IN_PARCELADA = 0;
            vm.CAPA_IN_PARCELAS = 0;
            vm.CAPA_VL_SALDO = 0;
            vm.USUA_CD_ID = usuario.USUA_CD_ID;
            vm.CAPA_IN_RECORRENTE = 0;
            vm.CAPA_DT_INICIO_RECORRENCIA = null;
            vm.CAPA_IN_CHEQUE = 0;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirCP(ContaPagarViewModel vm)
        {
            ViewBag.Forn = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllDespesas(), "CECU_CD_ID", "CECU_NM_EXIBE");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(fpApp.GetAllItens(1), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            List<SelectListItem> tipoDoc = new List<SelectListItem>();
            tipoDoc.Add(new SelectListItem() { Text = "Boleto", Value = "1" });
            tipoDoc.Add(new SelectListItem() { Text = "Nota", Value = "2" });
            tipoDoc.Add(new SelectListItem() { Text = "Recibo", Value = "3" });
            tipoDoc.Add(new SelectListItem() { Text = "Fatura", Value = "4" });
            tipoDoc.Add(new SelectListItem() { Text = "Crediário", Value = "5" });
            ViewBag.TipoDoc = new SelectList(tipoDoc, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    Int32 recorrencia = vm.CAPA_IN_RECORRENTE;
                    DateTime? data = vm.CAPA_DT_INICIO_RECORRENCIA;
                    CONTA_PAGAR item = Mapper.Map<ContaPagarViewModel, CONTA_PAGAR>(vm);
                    FORMA_PAGAMENTO forma = fpApp.GetItemById(item.FOPA_CD_ID.Value);
                    item.CAPA_IN_CHEQUE = forma.FOPA_IN_CHEQUE;
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = cpApp.ValidateCreate(item, recorrencia, data, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0084", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0085", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 3)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0095", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 4)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0096", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 5)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0107", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    // Cria pastas
                    String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/ContaPagar/" + item.CAPA_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaCPMaster = new List<CONTA_PAGAR>();
                    SessionMocks.listaCP = null;

                    SessionMocks.idVolta = item.CAPA_CD_ID;
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
        public ActionResult EditarCP(Int32 id)
        {
            // Prepara view
            ViewBag.Forn = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllDespesas(), "CECU_CD_ID", "CECU_NM_EXIBE");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(fpApp.GetAllItens(1), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.Tags = new SelectList(cpApp.GetAllTags(), "TITA_CD_ID", "TITA_NM_NOME");
            ViewBag.Liquida = 0;
            List<SelectListItem> tipoDoc = new List<SelectListItem>();
            tipoDoc.Add(new SelectListItem() { Text = "Boleto", Value = "1" });
            tipoDoc.Add(new SelectListItem() { Text = "Nota", Value = "2" });
            tipoDoc.Add(new SelectListItem() { Text = "Recibo", Value = "3" });
            tipoDoc.Add(new SelectListItem() { Text = "Fatura", Value = "4" });
            tipoDoc.Add(new SelectListItem() { Text = "Crediário", Value = "5" });
            ViewBag.TipoDoc = new SelectList(tipoDoc, "Value", "Text");
            SessionMocks.liquidaCP = 0;

            CONTA_PAGAR item = cpApp.GetItemById(id);
            objetoCPAntes = item;
            SessionMocks.contaPagar = item;
            SessionMocks.idVolta = id;
            SessionMocks.idCPVolta = 2;
            ContaPagarViewModel vm = Mapper.Map<CONTA_PAGAR, ContaPagarViewModel>(item);
            vm.CAPA_VL_PARCELADO = vm.CAPA_VL_VALOR;
            if (vm.CAPA_IN_PAGA_PARCIAL == 1)
            {
                vm.CAPA_VL_VALOR_PAGO = vm.CAPA_VL_SALDO;
            }
            else
            {
                vm.CAPA_VL_VALOR_PAGO = vm.CAPA_VL_VALOR;
            }
            if ((Int32)Session["ErroSoma"] == 1)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0097", CultureInfo.CurrentCulture));
                SessionMocks.idVoltaTab = 3;
            }
            if ((Int32)Session["ErroSoma"] == 3)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0086", CultureInfo.CurrentCulture));
                SessionMocks.idVoltaTab = 1;
            }
            if ((Int32)Session["ErroSoma"] == 4)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                SessionMocks.idVoltaTab = 1;
            }
            Session["ErroSoma"] = 0;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarCP(ContaPagarViewModel vm)
        {
            ViewBag.Forn = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllDespesas(), "CECU_CD_ID", "CECU_NM_EXIBE");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(fpApp.GetAllItens(1), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.Tags = new SelectList(cpApp.GetAllTags(), "TITA_CD_ID", "TITA_NM_NOME");
            ViewBag.Liquida = 0;
            SessionMocks.liquidaCP = 0;
            List<SelectListItem> tipoDoc = new List<SelectListItem>();
            tipoDoc.Add(new SelectListItem() { Text = "Boleto", Value = "1" });
            tipoDoc.Add(new SelectListItem() { Text = "Nota", Value = "2" });
            tipoDoc.Add(new SelectListItem() { Text = "Recibo", Value = "3" });
            tipoDoc.Add(new SelectListItem() { Text = "Fatura", Value = "4" });
            tipoDoc.Add(new SelectListItem() { Text = "Crediário", Value = "5" });
            ViewBag.TipoDoc = new SelectList(tipoDoc, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CONTA_PAGAR item = Mapper.Map<ContaPagarViewModel, CONTA_PAGAR>(vm);
                    SessionMocks.eParcela = 0;
                    Int32 volta = cpApp.ValidateEdit(item, SessionMocks.contaPagar, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0087", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0088", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 3)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0089", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 4)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0090", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 5)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0091", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 6)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0084", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 7)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0085", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 8)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0086", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 9)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0089", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 10)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0092", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaCPMaster = new List<CONTA_PAGAR>();
                    SessionMocks.listaCP = null;
                    return RedirectToAction("MontarTelaCP");
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

        public ActionResult DuplicarCP()
        {
            // Monta novo lançamento
            CONTA_PAGAR item = cpApp.GetItemById(SessionMocks.idVolta);
            CONTA_PAGAR novo = new CONTA_PAGAR();
            novo.CAPA_DS_DESCRICAO = "Lançamento Duplicado - " + item.CAPA_DS_DESCRICAO;
            novo.CAPA_DT_COMPETENCIA = item.CAPA_DT_COMPETENCIA;
            novo.CAPA_DT_LANCAMENTO = DateTime.Today.Date;
            novo.CAPA_DT_VENCIMENTO = DateTime.Today.Date.AddDays(30);
            novo.CAPA_IN_ATIVO = 1;
            novo.CAPA_IN_LIQUIDADA = 0;
            novo.CAPA_IN_PAGA_PARCIAL = 0;
            novo.CAPA_IN_PARCELADA = 0;
            novo.CAPA_IN_PARCELAS = 0;
            novo.CAPA_IN_TIPO_LANCAMENTO = 0;
            novo.CAPA_NM_FAVORECIDO = item.CAPA_NM_FAVORECIDO;
            novo.CAPA_NR_DOCUMENTO = item.CAPA_NR_DOCUMENTO;
            novo.CAPA_TX_OBSERVACOES = item.CAPA_TX_OBSERVACOES;
            novo.CAPA_VL_DESCONTO = 0;
            novo.CAPA_VL_JUROS = 0;
            novo.CAPA_VL_PARCELADO = 0;
            novo.CAPA_VL_PARCIAL = 0;
            novo.CAPA_VL_SALDO = 0;
            novo.CAPA_VL_TAXAS = 0;
            novo.CAPA_VL_VALOR = item.CAPA_VL_VALOR;
            novo.CAPA_VL_VALOR_PAGO = 0;
            novo.CECU_CD_ID = item.CECU_CD_ID;
            novo.FORN_CD_ID = item.FORN_CD_ID;
            novo.COBA_CD_ID = item.COBA_CD_ID;
            novo.USUA_CD_ID = item.USUA_CD_ID;
            novo.FOPA_CD_ID = item.FOPA_CD_ID;
            novo.PERI_CD_ID = item.PERI_CD_ID;
            novo.PLCO_CD_ID = item.PLCO_CD_ID;
            novo.TIFA_CD_ID = item.TIFA_CD_ID;
            novo.USUA_CD_ID = item.USUA_CD_ID;
            novo.CAPA_IN_CHEQUE = item.CAPA_IN_CHEQUE;
            novo.CAPA_NR_CHEQUE = item.CAPA_NR_CHEQUE;
            novo.COBA_CD_ID_1= item.COBA_CD_ID_1;

            // Grava
            USUARIO usuarioLogado = SessionMocks.UserCredentials;
            Int32 volta = cpApp.ValidateCreate(novo, 0, null, usuarioLogado);

            // Cria pastas
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/ContaPagar/" + novo.CAPA_CD_ID.ToString() + "/Anexos/";
            Directory.CreateDirectory(Server.MapPath(caminho));

            // Sucesso
            listaCPMaster = new List<CONTA_PAGAR>();
            SessionMocks.listaCP = null;
            return RedirectToAction("MontarTelaCP");
        }

        [HttpGet]
        public ActionResult LiquidarParcelaCP(Int32 id)
        {
            // Prepara view
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            CONTA_PAGAR_PARCELA item = ppApp.GetItemById(id);
            objetoCPPAntes = item;
            SessionMocks.contaPagarParcela = item;
            SessionMocks.idVoltaCPP = id;
            ContaPagarParcelaViewModel vm = Mapper.Map<CONTA_PAGAR_PARCELA, ContaPagarParcelaViewModel>(item);
            vm.CPPA_VL_DESCONTO = 0;
            vm.CPPA_VL_JUROS = 0;
            vm.CPPA_VL_TAXAS = 0;
            vm.CPPA_VL_VALOR_PAGO = vm.CPPA_VL_VALOR;
            vm.CPPA_DT_QUITACAO = DateTime.Today.Date;
            return View(vm);
        }

        [HttpPost]
        public ActionResult LiquidarParcelaCP(ContaPagarParcelaViewModel vm)
        {
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CONTA_PAGAR_PARCELA item = Mapper.Map<ContaPagarParcelaViewModel, CONTA_PAGAR_PARCELA>(vm);
                    Int32 volta = ppApp.ValidateEdit(item, SessionMocks.contaPagarParcela, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0052", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Acerta saldo
                    CONTA_PAGAR rec = cpApp.GetItemById(SessionMocks.idVolta);
                    rec.CAPA_VL_SALDO = rec.CAPA_VL_SALDO - item.CPPA_VL_VALOR_PAGO;

                    // Verifica se liquidou todas
                    List<CONTA_PAGAR_PARCELA> lista = rec.CONTA_PAGAR_PARCELA.Where(p => p.CPPA_IN_QUITADA == 0).ToList<CONTA_PAGAR_PARCELA>();
                    if (lista.Count == 0)
                    {
                        rec.CAPA_IN_LIQUIDADA = 1;
                        rec.CAPA_DT_LIQUIDACAO = DateTime.Today.Date;
                        rec.CAPA_VL_VALOR_PAGO = rec.CONTA_PAGAR_PARCELA.Sum(p => p.CPPA_VL_VALOR_PAGO);
                        rec.CAPA_VL_SALDO = 0;
                    }
                    SessionMocks.eParcela = 1;
                    volta = cpApp.ValidateEdit(rec, SessionMocks.contaPagar, usuarioLogado);

                    // Sucesso
                    listaCPPMaster = new List<CONTA_PAGAR_PARCELA>();
                    SessionMocks.listaCPP = null;
                    return RedirectToAction("VoltarAnexoCP");
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
        public ActionResult LiquidarCP(Int32 id)
        {
            // Prepara view
            ViewBag.Forn = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.CC = new SelectList(SessionMocks.CentroCustos, "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(SessionMocks.Formas, "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.Tags = new SelectList(cpApp.GetAllTags(), "TITA_CD_ID", "TITA_NM_NOME");
            ViewBag.Liquida = 1;
            SessionMocks.liquidaCP = 1;

            CONTA_PAGAR item = cpApp.GetItemById(id);
            objetoCPAntes = item;
            SessionMocks.contaPagar = item;
            SessionMocks.idVolta = id;
            ContaPagarViewModel vm = Mapper.Map<CONTA_PAGAR, ContaPagarViewModel>(item);
            vm.CAPA_VL_PARCELADO = vm.CAPA_VL_VALOR;
            if (vm.CAPA_IN_PAGA_PARCIAL == 1)
            {
                vm.CAPA_VL_VALOR_PAGO = vm.CAPA_VL_SALDO;
            }
            else
            {
                vm.CAPA_VL_VALOR_PAGO = vm.CAPA_VL_VALOR;
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LiquidarCP(ContaPagarViewModel vm)
        {
            ViewBag.Forn = new SelectList(SessionMocks.Fornecedores, "FORN_CD_ID", "FORN_NM_NOME");
            ViewBag.CC = new SelectList(SessionMocks.CentroCustos, "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Usuarios = new SelectList(SessionMocks.Usuarios, "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(SessionMocks.Formas, "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.Tags = new SelectList(cpApp.GetAllTags(), "TITA_CD_ID", "TITA_NM_NOME");
            ViewBag.Liquida = 1;
            SessionMocks.liquidaCP = 1;
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CONTA_PAGAR item = Mapper.Map<ContaPagarViewModel, CONTA_PAGAR>(vm);
                    Int32 volta = cpApp.ValidateEdit(item, SessionMocks.contaPagar, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0087", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0088", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 3)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0089", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 4)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0090", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 5)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0091", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 6)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0084", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 7)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0085", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 8)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0086", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 9)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0089", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 10)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0092", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaCPMaster = new List<CONTA_PAGAR>();
                    SessionMocks.listaCP = null;
                    return RedirectToAction("MontarTelaCP");
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
        public ActionResult IncluirTagCP()
        {
            CONTA_PAGAR item = cpApp.GetItemById(SessionMocks.idVolta);
            ViewBag.Tags = new SelectList(SessionMocks.Tags, "TITA_CD_ID", "TITA_NM_NOME");
            CONTA_PAGAR_TAG ct = new CONTA_PAGAR_TAG();
            ct.CAPA_CD_ID = item.CAPA_CD_ID;
            ct.CPTA_IN_ATIVO = 1;
            return View(ct);
        }

        [HttpPost]
        public ActionResult IncluirTagCP(CONTA_PAGAR_TAG ct)
        {
            CONTA_PAGAR item = cpApp.GetItemById(SessionMocks.idVolta);
            CONTA_PAGAR_TAG ch = new CONTA_PAGAR_TAG();
            ch.CAPA_CD_ID = item.CAPA_CD_ID;
            ch.CPTA_IN_ATIVO = 1;
            ch.CPTA_NM_DESCRICAO = ct.CPTA_NM_DESCRICAO;
            ch.TITA_CD_ID = ct.TITA_CD_ID;
            item.CONTA_PAGAR_TAG.Add(ch);
            Int32 volta = cpApp.ValidateEdit(item, item, SessionMocks.UserCredentials);
            return RedirectToAction("EditarCP", new { id = SessionMocks.idVolta });
        }

        [HttpPost]
        public ActionResult IncluirRateioCC(ContaPagarViewModel vm)
        {
            try
            {
                // Executa a operação
                Int32? cc = vm.CECU_CD_RATEIO;
                Int32? perc = vm.CAPA_VL_PERCENTUAL;
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                CONTA_PAGAR item = cpApp.GetItemById(vm.CAPA_CD_ID);
                Int32 volta = cpApp.IncluirRateioCC(item, cc, perc, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["ErroSoma"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0097", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoCP");
                }

                // Sucesso
                SessionMocks.idVoltaTab = 3;
                return RedirectToAction("VoltarAnexoCP");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("VoltarAnexoCP");
            }
        }

        [HttpGet]
        public ActionResult ExcluirRateio(Int32 id)
        {
            // Verifica se tem usuario logado
            CONTA_PAGAR cp = SessionMocks.contaPagar;
            CONTA_PAGAR_RATEIO rl = ratApp.GetItemById(id);
            Int32 volta = ratApp.ValidateDelete(rl);
            SessionMocks.idVoltaTab = 3;
            return RedirectToAction("VoltarAnexoCP");
        }
    }
}