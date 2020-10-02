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

namespace SystemBRPresentation.Controllers
{
    public class ContaReceberController : Controller
    {
        private readonly IContaReceberAppService crApp;
        private readonly IClienteAppService cliApp;
        private readonly ILogAppService logApp;
        private readonly IMatrizAppService matrizApp;
        private readonly IContaPagarAppService cpApp;
        private readonly IFornecedorAppService forApp;
        private readonly ICentroCustoAppService ccApp;
        private readonly IContaBancariaAppService cbApp;
        private readonly IFormaPagamentoAppService fpApp;
        private readonly IPeriodicidadeAppService perApp;
        private readonly IContaReceberParcelaAppService pcApp;
        private readonly IContaPagarParcelaAppService ppApp;
        private readonly IContaReceberRateioAppService ratApp;

        private String msg;
        private Exception exception;
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao = String.Empty;
        CONTA_RECEBER objetoCR = new CONTA_RECEBER();
        CONTA_RECEBER objetoCRAntes = new CONTA_RECEBER();
        List<CONTA_RECEBER> listaCRMaster = new List<CONTA_RECEBER>();
        CONTA_PAGAR objetoCP = new CONTA_PAGAR();
        CONTA_PAGAR objetoCPAntes = new CONTA_PAGAR();
        List<CONTA_PAGAR> listaCPMaster = new List<CONTA_PAGAR>();
        CONTA_RECEBER_PARCELA objetoCRP = new CONTA_RECEBER_PARCELA();
        CONTA_RECEBER_PARCELA objetoCRPAntes = new CONTA_RECEBER_PARCELA();
        List<CONTA_RECEBER_PARCELA> listaCRPMaster = new List<CONTA_RECEBER_PARCELA>();
        CONTA_PAGAR_PARCELA objetoCPP = new CONTA_PAGAR_PARCELA();
        CONTA_PAGAR_PARCELA objetoCPPAntes = new CONTA_PAGAR_PARCELA();
        List<CONTA_PAGAR_PARCELA> listaCPPMaster = new List<CONTA_PAGAR_PARCELA>();
        CONTA_BANCO contaPadrao = new CONTA_BANCO();

        public ContaReceberController(IContaReceberAppService crApps, ILogAppService logApps, IMatrizAppService matrizApps, IClienteAppService cliApps, IContaPagarAppService cpApps, IFornecedorAppService forApps, ICentroCustoAppService ccApps, IContaBancariaAppService cbApps, IFormaPagamentoAppService fpApps, IPeriodicidadeAppService perApps, IContaReceberParcelaAppService pcApps, IContaPagarParcelaAppService ppApps)
        {
            logApp = logApps;
            matrizApp = matrizApps;
            crApp = crApps;
            cliApp = cliApps;
            cpApp = cpApps;
            forApp = forApps;
            ccApp = ccApps;
            cbApp = cbApps;
            fpApp = fpApps;
            perApp = perApps;
            pcApp = pcApps;
            ppApp = ppApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult Voltar()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarDashboard()
        {
            listaCRMaster = new List<CONTA_RECEBER>();
            SessionMocks.listaCR = null;
            listaCPMaster = new List<CONTA_PAGAR>();
            SessionMocks.listaCP = null;
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaCR()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            usuario = SessionMocks.UserCredentials;

            // Carrega listas
            if (SessionMocks.listaCR == null)
            {
                listaCRMaster = crApp.GetAllItens();
                SessionMocks.listaCR = listaCRMaster;
            }
            ViewBag.Listas = SessionMocks.listaCR;
            ViewBag.Title = "Contas a Receberxxx";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            SessionMocks.Clientes = cliApp.GetAllItens();
            ViewBag.Clientes = new SelectList(SessionMocks.Clientes, "CLIE_CD_ID", "CLIE_NM_NOME");

            // Indicadores
            ViewBag.CRS = listaCRMaster.Count;
            ViewBag.Recebido = SessionMocks.listaCR.Where(p => p.CARE_IN_LIQUIDADA == 1 & p.CARE_DT_VENCIMENTO.Value.Month == DateTime.Today.Date.Month).ToList().Sum(p => p.CARE_VL_VALOR_LIQUIDADO).Value;
            ViewBag.AReceber = SessionMocks.listaCR.Where(p => p.CARE_IN_LIQUIDADA == 0 & p.CARE_DT_VENCIMENTO.Value.Month == DateTime.Today.Date.Month).ToList().Sum(p => p.CARE_VL_VALOR);
            ViewBag.Atrasos = SessionMocks.listaCR.Where(p => p.CARE_NR_ATRASO > 0 & p.CARE_DT_VENCIMENTO < DateTime.Today.Date).ToList().Sum(p => p.CARE_VL_VALOR);
            ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
            List<SelectListItem> tipoFiltro = new List<SelectListItem>();
            tipoFiltro.Add(new SelectListItem() { Text = "Somente em Aberto", Value = "1" });
            tipoFiltro.Add(new SelectListItem() { Text = "Somente Fechados", Value = "2" });
            ViewBag.Filtro = new SelectList(tipoFiltro, "Value", "Text");

            SessionMocks.ContasBancarias = cbApp.GetAllItens();
            if ((Int32)Session["ErroSoma"] == 2)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0083", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["ErroSoma"] == 3)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0017", CultureInfo.CurrentCulture));
            }
            Session["ErroSoma"] = 0;

            // Abre view
            objetoCR = new CONTA_RECEBER();
            objetoCR.CARE_DT_LANCAMENTO = DateTime.Now.Date;
            return View(objetoCR);
        }

        public ActionResult RetirarFiltroCR()
        {
            SessionMocks.listaCR = null;
            SessionMocks.filtroCR = null;
            return RedirectToAction("MontarTelaCR");
        }

        public ActionResult MostrarTudoCR()
        {
            listaCRMaster = crApp.GetAllItensAdm();
            SessionMocks.filtroCR = null;
            SessionMocks.listaCR = listaCRMaster;
            return RedirectToAction("MontarTelaCR");
        }

        [HttpPost]
        public ActionResult FiltrarCR(CONTA_RECEBER item)
        {
            try
            {
                // Executa a operação
                List<CONTA_RECEBER> listaObj = new List<CONTA_RECEBER>();
                SessionMocks.filtroCR = item;
                Int32 volta = crApp.ExecuteFilter(item.CLIE_CD_ID, item.CECU_CD_ID, item.CARE_DT_LANCAMENTO, item.CARE_DS_DESCRICAO, item.CARE_IN_ABERTOS, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["ErroSoma"] = 3;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0017", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaCRMaster = listaObj;
                SessionMocks.listaCR = listaObj;
                return RedirectToAction("MontarTelaCR");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaCR");
            }
        }

        public ActionResult VoltarBaseCR()
        {
            listaCRMaster = new List<CONTA_RECEBER>();
            SessionMocks.listaCR = null;
            return RedirectToAction("MontarTelaCR");
        }

        [HttpGet]
        public ActionResult VerRecebimentosMes()
        {
            List<CONTA_RECEBER> lista = SessionMocks.listaCR.Where(p => p.CARE_IN_LIQUIDADA == 1 & p.CARE_DT_DATA_LIQUIDACAO.Value.Month == DateTime.Today.Date.Month).ToList();
            ViewBag.ListaCR = lista;
            ViewBag.LR = lista.Count;
            ViewBag.Valor = lista.Sum(x => x.CARE_VL_VALOR_LIQUIDADO);
            return View();
        }

        [HttpGet]
        public ActionResult VerAReceberMes()
        {
            List<CONTA_RECEBER> lista = SessionMocks.listaCR.Where(p => p.CARE_IN_LIQUIDADA == 0 & p.CARE_DT_VENCIMENTO.Value.Month == DateTime.Today.Date.Month).ToList();
            ViewBag.ListaCR = lista;
            ViewBag.LR = lista.Count;
            ViewBag.Valor = lista.Sum(x => x.CARE_VL_VALOR);
            return View();
        }

        [HttpGet]
        public ActionResult VerLancamentosAtraso()
        {
            List<CONTA_RECEBER> lista = SessionMocks.listaCR.Where(p => p.CARE_NR_ATRASO > 0 & p.CARE_DT_VENCIMENTO < DateTime.Today.Date).ToList();
            ViewBag.ListaCR = lista;
            ViewBag.LR = lista.Count;
            ViewBag.Valor = lista.Sum(x => x.CARE_VL_VALOR);
            return View();
        }

        [HttpGet]
        public ActionResult VerCR(Int32 id)
        {
            // Prepara view
            CONTA_RECEBER item = crApp.GetItemById(id);
            SessionMocks.contaReceber = item;
            ContaReceberViewModel vm = Mapper.Map<CONTA_RECEBER, ContaReceberViewModel>(item);
            SessionMocks.idVolta = id;
            SessionMocks.idCRVolta = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult UploadFileLancamentoCR(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                Session["ErroSoma"] = 4;
                return RedirectToAction("VoltarAnexoCR");
            }

            CONTA_RECEBER item = crApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 250)
            {
                Session["ErroSoma"] = 5;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoCR");
            }

            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/ContaReceber/" + item.CARE_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            CONTA_RECEBER_ANEXO foto = new CONTA_RECEBER_ANEXO();
            foto.CRAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.CRAN_DT_ANEXO = DateTime.Today;
            foto.CRAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.CRAN_IN_TIPO = tipo;
            foto.CRAN_NM_TITULO = fileName;
            foto.CARE_CD_ID = item.CARE_CD_ID;

            item.CONTA_RECEBER_ANEXO.Add(foto);
            objetoCRAntes = item;
            Int32 volta = crApp.ValidateEdit(item, objetoCRAntes, usu);
            if (SessionMocks.idCRVolta == 1)
            {
                return RedirectToAction("VerCR");
            }
            return RedirectToAction("VoltarAnexoCR");
        }

        [HttpPost]
        public JsonResult UploadFileCR_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
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
                    UploadFileLancamentoCR(file);
                }
            }

            return Json("1"); // VoltarAnexoCliente
        }

        [HttpGet]
        public ActionResult VerAnexoLancamentoCR(Int32 id)
        {
            // Prepara view
            CONTA_RECEBER_ANEXO item = crApp.GetAnexoById(id);
            return View(item);
        }

        public FileResult DownloadLancamentoCR(Int32 id)
        {
            CONTA_RECEBER_ANEXO item = crApp.GetAnexoById(id);
            String arquivo = item.CRAN_AQ_ARQUIVO;
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
        public ActionResult SlideShowCR()
        {
            // Prepara view
            CONTA_RECEBER item = crApp.GetItemById(SessionMocks.idVolta);
            objetoCRAntes = item;
            ContaReceberViewModel vm = Mapper.Map<CONTA_RECEBER, ContaReceberViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult ExcluirCR(Int32 id)
        {
            USUARIO usu = SessionMocks.UserCredentials;
            CONTA_RECEBER item = crApp.GetItemById(id);
            objetoCRAntes = SessionMocks.contaReceber;
            item.CARE_IN_ATIVO = 0;
            Int32 volta = crApp.ValidateDelete(item, usu);
            if (volta == 1)
            {
                Session["ErroSoma"] = 2;
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0083", CultureInfo.CurrentCulture));
            }
            listaCRMaster = new List<CONTA_RECEBER>();
            SessionMocks.listaCR = null;
            return RedirectToAction("MontarTelaCR");
        }

        [HttpGet]
        public ActionResult ReativarCR(Int32 id)
        {
            USUARIO usu = SessionMocks.UserCredentials;
            CONTA_RECEBER item = crApp.GetItemById(id);
            objetoCRAntes = SessionMocks.contaReceber;
            item.CARE_IN_ATIVO = 1;
            Int32 volta = crApp.ValidateReativar(item, usu);
            listaCRMaster = new List<CONTA_RECEBER>();
            SessionMocks.listaCR = null;
            return RedirectToAction("MontarTelaCR");
        }

        [HttpGet]
        public ActionResult VerParcelaCR(Int32 id)
        {
            // Prepara view
            CONTA_RECEBER_PARCELA item = crApp.GetParcelaById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoVerCR()
        {
            return RedirectToAction("VerCR", new { id = SessionMocks.idVolta });
        }

        public ActionResult VoltarAnexoCR()
        {
            return RedirectToAction("EditarCR", new { id = SessionMocks.idVolta });
        }

        [HttpGet]
        public ActionResult IncluirCR()
        {
            // Prepara listas
            ViewBag.Clientes = new SelectList(SessionMocks.Clientes, "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllReceitas(), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(fpApp.GetAllItens(2), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");

            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            CONTA_RECEBER item = new CONTA_RECEBER();
            ContaReceberViewModel vm = Mapper.Map<CONTA_RECEBER, ContaReceberViewModel>(item);
            vm.ASSI_CD_ID = SessionMocks.IdAssinante;
            vm.CARE_DT_LANCAMENTO = DateTime.Today.Date;
            vm.CARE_IN_ATIVO = 1;
            vm.MATR_CD_ID = SessionMocks.Matriz.MATR_CD_ID;
            vm.FILI_CD_ID = 1;
            vm.CARE_DT_COMPETENCIA = DateTime.Today.Date;
            vm.CARE_DT_VENCIMENTO = DateTime.Today.Date.AddDays(30);
            vm.CARE_IN_LIQUIDADA = 0;
            vm.CARE_IN_PAGA_PARCIAL = 0;
            vm.CARE_IN_PARCELADA = 0;
            vm.CARE_IN_PARCELAS = 0;
            vm.CARE_VL_SALDO = 0;
            vm.USUA_CD_ID = usuario.USUA_CD_ID;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirCR(ContaReceberViewModel vm)
        {
            ViewBag.Clientes = new SelectList(SessionMocks.Clientes, "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllReceitas(), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(fpApp.GetAllItens(2), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    Int32 recorrencia = vm.CARE_IN_RECORRENTE;
                    DateTime? data = vm.CARE_DT_INICIO_RECORRENCIA;
                    CONTA_RECEBER item = Mapper.Map<ContaReceberViewModel, CONTA_RECEBER>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = crApp.ValidateCreate(item, recorrencia, data, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0053", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0054", CultureInfo.CurrentCulture));
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


                    // Cria pastas
                    String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/ContaReceber/" + item.CARE_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaCRMaster = new List<CONTA_RECEBER>();
                    SessionMocks.listaCR = null;

                    SessionMocks.idVolta = item.CARE_CD_ID;
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
        public ActionResult EditarCR(Int32 id)
        {
            // Prepara view
            ViewBag.Clientes = new SelectList(SessionMocks.Clientes, "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllReceitas(), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(fpApp.GetAllItens(2), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.Tags = new SelectList(crApp.GetAllTags(), "TITA_CD_ID", "TITA_NM_NOME");
            ViewBag.Liquida = 0;
            SessionMocks.liquidaCR = 0;

            CONTA_RECEBER item = crApp.GetItemById(id);
            objetoCRAntes = item;
            SessionMocks.contaReceber = item;
            SessionMocks.idVolta = id;
            SessionMocks.idCRVolta = 2;
            ContaReceberViewModel vm = Mapper.Map<CONTA_RECEBER, ContaReceberViewModel>(item);
            //vm.CARE_VL_PARCELADO = vm.CARE_VL_VALOR;
            //if (vm.CARE_IN_PAGA_PARCIAL == 1)
            //{
            //    vm.CARE_VL_VALOR_LIQUIDADO = vm.CARE_VL_SALDO;
            //}
            //else
            //{
            //    vm.CARE_VL_VALOR_LIQUIDADO = vm.CARE_VL_VALOR;
            //}
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
        public ActionResult EditarCR(ContaReceberViewModel vm)
        {
            ViewBag.Clientes = new SelectList(SessionMocks.Clientes, "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllReceitas(), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(fpApp.GetAllItens(2), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.Tags = new SelectList(crApp.GetAllTags(), "TITA_CD_ID", "TITA_NM_NOME");
            ViewBag.Liquida = 0;
            SessionMocks.liquidaCR = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CONTA_RECEBER item = Mapper.Map<ContaReceberViewModel, CONTA_RECEBER>(vm);
                    Int32 volta = crApp.ValidateEdit(item, SessionMocks.contaReceber, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0048", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 3)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 4)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0051", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 5)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0052", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 6)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0053", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 7)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0054", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 8)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0055", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 9)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0056", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 10)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0057", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaCRMaster = new List<CONTA_RECEBER>();
                    SessionMocks.listaCR = null;
                    return RedirectToAction("MontarTelaCR");
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
        public ActionResult ParcelarCR(Int32 id)
        {
            // Prepara view
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            CONTA_RECEBER item = crApp.GetItemById(id);
            objetoCRAntes = item;
            SessionMocks.contaReceber = item;
            SessionMocks.idVolta = id;
            ContaReceberViewModel vm = Mapper.Map<CONTA_RECEBER, ContaReceberViewModel>(item);
            vm.CARE_DT_INICIO_PARCELA = DateTime.Today.Date;
            vm.CARE_IN_PARCELADA = 1;
            vm.CARE_IN_PARCELAS = 2;
            vm.CARE_VL_PARCELADO = vm.CARE_VL_VALOR;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ParcelarCR(ContaReceberViewModel vm)
        {
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
            {
                    // Processa parcelas
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CONTA_RECEBER item = Mapper.Map<ContaReceberViewModel, CONTA_RECEBER>(vm);
                    DateTime dataParcela = item.CARE_DT_INICIO_PARCELA.Value;
                    PERIODICIDADE period = perApp.GetItemById(item.PERI_CD_ID.Value);
                    if (dataParcela.Date <= DateTime.Today.Date)
                    {
                        dataParcela = dataParcela.AddMonths(1);
                    }

                    for (int i = 1; i <= item.CARE_IN_PARCELAS; i++)
                    {
                        CONTA_RECEBER_PARCELA parc = new CONTA_RECEBER_PARCELA();
                        parc.CARE_CD_ID = item.CARE_CD_ID;
                        parc.CRPA_DT_QUITACAO = null;
                        parc.CRPA_DT_VENCIMENTO = dataParcela;
                        parc.CRPA_IN_ATIVO = 1;
                        parc.CRPA_IN_QUITADA = 0;
                        parc.CRPA_NR_PARCELA = i.ToString() + "/" + item.CARE_IN_PARCELAS.Value.ToString();
                        parc.CRPA_VL_RECEBIDO = 0;
                        parc.CRPA_VL_VALOR = item.CARE_VL_PARCELADO;
                        item.CONTA_RECEBER_PARCELA.Add(parc);
                        dataParcela = dataParcela.AddDays(period.PERI_NR_DIAS);
                    }

                    item.CARE_IN_PARCELADA = 1;
                    objetoCRAntes = item;
                    Int32 volta = crApp.ValidateEdit(item, objetoCRAntes, usuarioLogado);
                    listaCRMaster = new List<CONTA_RECEBER>();
                    SessionMocks.listaCR = null;
                    return RedirectToAction("MontarTelaCR");
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

        public ActionResult DuplicarCR()
        {
            // Monta novo lançamento
            CONTA_RECEBER item = crApp.GetItemById(SessionMocks.idVolta);
            CONTA_RECEBER novo = new CONTA_RECEBER();
            novo.ASSI_CD_ID = SessionMocks.IdAssinante;
            novo.CARE_DS_DESCRICAO = "Lançamento Duplicado - " + item.CARE_DS_DESCRICAO;
            novo.CARE_DT_COMPETENCIA = item.CARE_DT_COMPETENCIA;
            novo.CARE_DT_LANCAMENTO = DateTime.Today.Date;
            novo.CARE_DT_VENCIMENTO = DateTime.Today.Date.AddDays(30);
            novo.CARE_IN_ATIVO = 1;
            novo.CARE_IN_LIQUIDADA = 0;
            novo.CARE_IN_PAGA_PARCIAL = 0;
            novo.CARE_IN_PARCELADA = 0;
            novo.CARE_IN_PARCELAS = 0;
            novo.CARE_IN_TIPO_LANCAMENTO = 0;
            novo.CARE_NM_FAVORECIDO = item.CARE_NM_FAVORECIDO;
            novo.CARE_NR_DOCUMENTO = item.CARE_NR_DOCUMENTO;
            novo.CARE_TX_OBSERVACOES = item.CARE_TX_OBSERVACOES;
            novo.CARE_VL_DESCONTO = 0;
            novo.CARE_VL_JUROS = 0;
            novo.CARE_VL_PARCELADO = 0;
            novo.CARE_VL_PARCIAL = 0;
            novo.CARE_VL_SALDO = 0;
            novo.CARE_VL_TAXAS = 0;
            novo.CARE_VL_VALOR = item.CARE_VL_VALOR;
            novo.CARE_VL_VALOR_LIQUIDADO = 0;
            novo.CECU_CD_ID = item.CECU_CD_ID;
            novo.CLIE_CD_ID = item.CLIE_CD_ID;
            novo.COBA_CD_ID = item.COBA_CD_ID;
            novo.FILI_CD_ID = item.FILI_CD_ID;
            novo.FOPA_CD_ID = item.FOPA_CD_ID;
            novo.MATR_CD_ID = item.MATR_CD_ID;
            novo.PERI_CD_ID = item.PERI_CD_ID;
            novo.PEVE_CD_ID = item.PEVE_CD_ID;
            novo.PLCO_CD_ID = item.PLCO_CD_ID;
            novo.TIFA_CD_ID = item.TIFA_CD_ID;
            novo.USUA_CD_ID = item.USUA_CD_ID;

            // Grava
            USUARIO usuarioLogado = SessionMocks.UserCredentials;
            Int32 volta = crApp.ValidateCreate(novo, 0, null, usuarioLogado);

            // Cria pastas
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/ContaReceber/" + novo.CARE_CD_ID.ToString() + "/Anexos/";
            Directory.CreateDirectory(Server.MapPath(caminho));

            // Sucesso
            listaCRMaster = new List<CONTA_RECEBER>();
            SessionMocks.listaCR = null;
            return RedirectToAction("MontarTelaCR");
        }

        [HttpGet]
        public ActionResult LiquidarParcelaCR(Int32 id)
        {
            // Prepara view
            CONTA_RECEBER_PARCELA item = pcApp.GetItemById(id);
            objetoCRPAntes = item;
            SessionMocks.contaReceberParcela = item;
            SessionMocks.idVoltaCRP = id;
            ContaReceberParcelaViewModel vm = Mapper.Map<CONTA_RECEBER_PARCELA, ContaReceberParcelaViewModel>(item);
            vm.CRPA_VL_DESCONTO = 0;
            vm.CRPA_VL_JUROS = 0;
            vm.CRPA_VL_TAXAS = 0;
            vm.CRPA_VL_RECEBIDO = vm.CRPA_VL_VALOR;
            vm.CRPA_DT_QUITACAO = DateTime.Today.Date;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LiquidarParcelaCR(ContaReceberParcelaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CONTA_RECEBER_PARCELA item = Mapper.Map<ContaReceberParcelaViewModel, CONTA_RECEBER_PARCELA>(vm);
                    Int32 volta = pcApp.ValidateEdit(item, SessionMocks.contaReceberParcela, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0048", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Acerta saldo
                    CONTA_RECEBER rec = crApp.GetItemById(SessionMocks.idVolta);
                    rec.CARE_VL_SALDO = rec.CARE_VL_SALDO - item.CRPA_VL_RECEBIDO;                    
                    
                    // Verifica se liquidou todas
                    List<CONTA_RECEBER_PARCELA> lista = rec.CONTA_RECEBER_PARCELA.Where(p => p.CRPA_IN_QUITADA == 0).ToList<CONTA_RECEBER_PARCELA>();
                    if (lista.Count == 0)
                    {
                        rec.CARE_IN_LIQUIDADA = 1;
                        rec.CARE_DT_DATA_LIQUIDACAO = DateTime.Today.Date;
                        rec.CARE_VL_VALOR_LIQUIDADO = rec.CONTA_RECEBER_PARCELA.Sum(p => p.CRPA_VL_RECEBIDO);
                        rec.CARE_VL_SALDO = 0;
                    }
                    volta = crApp.ValidateEdit(rec, SessionMocks.contaReceber, usuarioLogado);

                    // Sucesso
                    listaCRPMaster = new List<CONTA_RECEBER_PARCELA>();
                    SessionMocks.listaCRP = null;
                    return RedirectToAction("VoltarAnexoCR");
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
        public ActionResult LiquidarCR(Int32 id)
        {
            // Prepara view
            ViewBag.Clientes = new SelectList(SessionMocks.Clientes, "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllReceitas(), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(SessionMocks.Formas, "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.Tags = new SelectList(crApp.GetAllTags(), "TITA_CD_ID", "TITA_NM_NOME");
            ViewBag.Liquida = 1;
            SessionMocks.liquidaCR = 1;

            CONTA_RECEBER item = crApp.GetItemById(id);
            objetoCRAntes = item;
            SessionMocks.contaReceber = item;
            SessionMocks.idVolta = id;
            ContaReceberViewModel vm = Mapper.Map<CONTA_RECEBER, ContaReceberViewModel>(item);
            vm.CARE_VL_PARCELADO = vm.CARE_VL_VALOR;
            if (vm.CARE_IN_PAGA_PARCIAL == 1)
            {
                vm.CARE_VL_VALOR_LIQUIDADO = vm.CARE_VL_SALDO;
            }
            else
            {
                vm.CARE_VL_VALOR_LIQUIDADO = vm.CARE_VL_VALOR;
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LiquidarCR(ContaReceberViewModel vm)
        {
            ViewBag.Clientes = new SelectList(SessionMocks.Clientes, "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.CC = new SelectList(ccApp.GetAllReceitas(), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Contas = new SelectList(SessionMocks.ContasBancarias, "COBA_CD_ID", "COBA_NM_NOME_EXIBE");
            ViewBag.Formas = new SelectList(SessionMocks.Formas, "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(SessionMocks.Periodicidades, "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.Tags = new SelectList(crApp.GetAllTags(), "TITA_CD_ID", "TITA_NM_NOME");
            ViewBag.Liquida = 1;
            SessionMocks.liquidaCR = 1;
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    CONTA_RECEBER item = Mapper.Map<ContaReceberViewModel, CONTA_RECEBER>(vm);
                    Int32 volta = crApp.ValidateEdit(item, SessionMocks.contaReceber, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0048", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 3)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 4)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0051", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 5)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0052", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 6)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0053", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 7)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0054", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 8)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0055", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 9)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0056", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 10)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0057", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaCRMaster = new List<CONTA_RECEBER>();
                    SessionMocks.listaCR = null;
                    return RedirectToAction("MontarTelaCR");
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

        public ActionResult IncluirRateioCC(ContaReceberViewModel vm)
        {
            try
            {
                // Executa a operação
                Int32? cc = vm.CECU_CD_RATEIO;
                Int32? perc = vm.CARE_VL_PERCENTUAL;
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                CONTA_RECEBER item = crApp.GetItemById(vm.CARE_CD_ID);
                Int32 volta = crApp.IncluirRateioCC(item, cc, perc, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["ErroSoma"] = 1;
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0097", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoCR");
                }

                // Sucesso
                SessionMocks.idVoltaTab = 3;
                return RedirectToAction("VoltarAnexoCR");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("VoltarAnexoCR");
            }
        }

        [HttpGet]
        public ActionResult ExcluirRateio(Int32 id)
        {
            // Verifica se tem usuario logado
            CONTA_RECEBER cp = SessionMocks.contaReceber;
            CONTA_RECEBER_RATEIO rl = ratApp.GetItemById(id);
            Int32 volta = ratApp.ValidateDelete(rl);
            SessionMocks.idVoltaTab = 3;
            return RedirectToAction("VoltarAnexoCR");
        }

    }
}