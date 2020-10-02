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
using System.Collections;

namespace SystemBRPresentation.Controllers
{
    public class BaseAdminController : Controller
    {
        private readonly IUsuarioAppService baseApp;
        private readonly INoticiaAppService notiApp;
        private readonly ILogAppService logApp;
        private readonly ITarefaAppService tarApp;
        private readonly INotificacaoAppService notfApp;
        private readonly IPedidoVendaAppService pvApp;
        private readonly IContaPagarAppService cpApp;
        private readonly IContaReceberAppService crApp;
        private readonly IProdutoAppService prodApp;
        private readonly IMateriaPrimaAppService insApp;
        private readonly ICargoAppService carApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IFilialAppService filApp;
        private readonly ITipoContaAppService tcApp;
        private readonly ICategoriaClienteAppService ccApp;
        private readonly IClienteAppService cliApp;
        private readonly ITipoContribuinteAppService tcoApp;
        private readonly ICentroCustoAppService cuApp;
        private readonly IUnidadeAppService uniApp;
        private readonly IFormaPagamentoAppService fpApp;
        private readonly IPeriodicidadeAppService perApp;
        private readonly ICategoriaFornecedorAppService cfApp;
        private readonly IFuncionarioAppService funApp;
        private readonly ICategoriaMateriaAppService ciApp;
        private readonly ICategoriaPatrimonioAppService cprApp;
        private readonly ICategoriaProdutoAppService cppApp;
        private readonly IAgendaAppService ageApp;
        private readonly IEquipamentoAppService equiApp;
        private readonly ITipoTagAppService tagApp;
        private readonly IPatrimonioAppService patApp;
        private readonly IConfiguracaoAppService confApp;

        private String msg;
        private Exception exception;
        USUARIO objeto = new USUARIO();
        USUARIO objetoAntes = new USUARIO();
        List<USUARIO> listaMaster = new List<USUARIO>();

        public BaseAdminController(IUsuarioAppService baseApps, ILogAppService logApps, INoticiaAppService notApps, ITarefaAppService tarApps, INotificacaoAppService notfApps, IPedidoVendaAppService pvApps, IContaReceberAppService crApps, IContaPagarAppService cpApps, IProdutoAppService prodApps, IMateriaPrimaAppService insApps, ICargoAppService carApps, IUsuarioAppService usuApps, IFilialAppService filApps, ITipoContaAppService tcApps, ICategoriaClienteAppService ccApps, IClienteAppService cliApps, ITipoContribuinteAppService tcoApps, ICentroCustoAppService cuApps, IUnidadeAppService uniApps, IFormaPagamentoAppService fpApps, IPeriodicidadeAppService perApps, ICategoriaFornecedorAppService cfApps, IFuncionarioAppService funApps, ICategoriaMateriaAppService ciApps, ICategoriaPatrimonioAppService cprApps, ICategoriaProdutoAppService cppApps, IAgendaAppService ageApps, IEquipamentoAppService equiApps, ITipoTagAppService tagApps, IPatrimonioAppService patApps, IConfiguracaoAppService confApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            notiApp = notApps;
            tarApp = tarApps;
            notfApp = notfApps;
            pvApp = pvApps;
            cpApp = cpApps;
            crApp = crApps;
            prodApp = prodApps;
            insApp = insApps;
            carApp = carApps;
            usuApp = usuApps;
            filApp = filApps;
            tcApp = tcApps;
            ccApp = ccApps;
            cliApp = cliApps;
            tcoApp = tcoApps;
            cuApp = cuApps;
            uniApp = uniApps;
            fpApp = fpApps;
            perApp = perApps;
            cfApp = cfApps;
            funApp = funApps;
            ciApp = ciApps;
            cprApp = cprApps;
            cppApp = cppApps;
            ageApp = ageApps;
            equiApp = equiApps;
            tagApp = tagApps;
            patApp = patApps;
            confApp = confApps;
        }

        public ActionResult CarregarAdmin()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            ViewBag.Usuarios = baseApp.GetAllUsuarios().Count;
            ViewBag.Logs = logApp.GetAllItens().Count;
            ViewBag.UsuariosLista = baseApp.GetAllUsuarios();
            ViewBag.LogsLista = logApp.GetAllItens();
            return View();
        }

        public ActionResult CarregarLandingPage()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return View();
        }

        public JsonResult GetRefreshTime()
        {
            return Json(confApp.GetById(1).CONF_NR_REFRESH_DASH);
        }

        public JsonResult GetConfigNotificacoes()
        {
            bool hasNotf;
            var hash = new Hashtable();
            USUARIO usu = usuApp.GetItemById(SessionMocks.UserCredentials.USUA_CD_ID);
            CONFIGURACAO conf = confApp.GetById(1);

            if (baseApp.GetAllItensUser(usu.USUA_CD_ID).Count > 0)
            {
                hasNotf = true;
            }
            else
            {
                hasNotf = false;
            }

            hash.Add("CONF_NM_ARQUIVO_ALARME", conf.CONF_NM_ARQUIVO_ALARME);
            hash.Add("NOTIFICACAO", hasNotf);


            return Json(hash);
        }

        public ActionResult CarregarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            if ((Int32)Session["Login"] == 1)
            {
                SessionMocks.Perfis = baseApp.GetAllPerfis();
                SessionMocks.Cargos = carApp.GetAllItens();
                SessionMocks.Usuarios = usuApp.GetAllUsuarios();
                SessionMocks.Filiais = filApp.GetAllItens();
                SessionMocks.TipoContas = tcApp.GetAllItens();
                SessionMocks.CatsClientes = ccApp.GetAllItens();
                SessionMocks.UFs = cliApp.GetAllUF();
                SessionMocks.TiposContribuintes = tcoApp.GetAllItens();
                SessionMocks.TiposPessoas = cliApp.GetAllTiposPessoa();
                SessionMocks.Regimes = cliApp.GetAllRegimes();
                SessionMocks.CentroCustos = cuApp.GetAllItens();
                SessionMocks.Unidades = uniApp.GetAllItens();
                SessionMocks.Formas = fpApp.GetAllItens();
                SessionMocks.Periodicidades = perApp.GetAllItens();
                SessionMocks.CatsForns = cfApp.GetAllItens();
                SessionMocks.Situacoes = funApp.GetAllSituacao();
                SessionMocks.Funcoes = funApp.GetAllFuncao();
                SessionMocks.Sexos = funApp.GetAllSexo();
                SessionMocks.Escolaridades = funApp.GetAllEscolaridade();
                SessionMocks.CatsInsumos = ciApp.GetAllItens();
                SessionMocks.CatsPatrimonio = cprApp.GetAllItens();
                SessionMocks.CatsProduto = cppApp.GetAllItens();
                SessionMocks.Tags = tagApp.GetAllItens();
            }
            Session["MensProduto"] = 0;
            Session["MensDept"] = 0;
            Session["MensCompra"] = 0;
            Session["MensTarefa"] = 0;
            Session["MensAtendimento"] = 0;
            Session["MensMateriaPrima"] = 0;
            Session["ClienteNovo"] = 0;
            Session["MensEstoque"] = 0;
            Session["VoltaEstoque"] = 0;

            USUARIO usu = usuApp.GetItemById(SessionMocks.UserCredentials.USUA_CD_ID);
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(usu);

            SessionMocks.Notificacoes = usu.NOTIFICACAO.ToList();
            SessionMocks.listaNovas = SessionMocks.Notificacoes.Where(p => p.NOTI_IN_VISTA == 0).ToList();
            SessionMocks.NovasNotificacoes = SessionMocks.Notificacoes.Where(p => p.NOTI_IN_VISTA == 0).Count();
            SessionMocks.Nome = usu.USUA_NM_NOME;
            //if (SessionMocks.NovasNotificacoes > 0)
            //{
            //    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0093", CultureInfo.CurrentCulture));
            //}

            SessionMocks.Noticias = notiApp.GetAllItensValidos();
            SessionMocks.NoticiasNumero = SessionMocks.Noticias.Count;

            SessionMocks.listaPendentes = tarApp.GetTarefaStatus(usu.USUA_CD_ID, 1);
            SessionMocks.TarefasPendentes = SessionMocks.listaPendentes.Count;
            SessionMocks.TarefasLista = tarApp.GetByUser(usu.USUA_CD_ID);
            SessionMocks.Tarefas = SessionMocks.TarefasLista.Count;
            //if (SessionMocks.TarefasPendentes > 0)
            //{
            //    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0094", CultureInfo.CurrentCulture));
            //}

            SessionMocks.Agendas = usu.AGENDA.ToList();
            SessionMocks.numAgendas = SessionMocks.Agendas.Count;
            SessionMocks.AgendasHoje = SessionMocks.Agendas.Where(p => p.AGEN_DT_DATA == DateTime.Today.Date).ToList();
            SessionMocks.numAgendasHoje = SessionMocks.AgendasHoje.Count;

            SessionMocks.Logs = usu.LOG.Count;

            String frase = String.Empty;
            String nome = usu.USUA_NM_NOME.Substring(0, usu.USUA_NM_NOME.IndexOf(" "));
            if (DateTime.Now.Hour <= 12)
            {
                frase = "Bom dia, " + nome;
            }
            else if (DateTime.Now.Hour > 12 & DateTime.Now.Hour <= 18)
            {
                frase = "Boa tarde, " + nome;
            }
            else
            {
                frase = "Boa noite, " + nome;
            }
            SessionMocks.Greeting = frase;
            SessionMocks.Foto = usu.USUA_AQ_FOTO;

            // Checagem de contas a pagar
            List<NOTIFICACAO> notHoje = SessionMocks.Notificacoes.Where(p => p.NOTI_DT_EMISSAO.Value.Date == DateTime.Today.Date & p.NOTI_IN_NIVEL == 4).ToList();
            if (notHoje.Count == 0)
            {
                List<CONTA_PAGAR> cps = cpApp.GetAllItens();
                List<CONTA_PAGAR> cpHoje = cps.Where(p => p.CAPA_DT_VENCIMENTO.Value.Date == DateTime.Today.Date & p.CAPA_IN_LIQUIDADA == 0).ToList();
                List<CONTA_PAGAR> cpAtraso = cps.Where(p => p.CAPA_DT_VENCIMENTO.Value.Date < DateTime.Today.Date & p.CAPA_IN_LIQUIDADA == 0).ToList();
                List<CONTA_PAGAR> cpAviso = cps.Where(p => p.CAPA_DT_VENCIMENTO.Value.Date > DateTime.Today.Date & p.CAPA_DT_VENCIMENTO.Value.Date < DateTime.Today.Date.AddDays(5) & p.CAPA_IN_LIQUIDADA == 0).ToList();

                foreach (CONTA_PAGAR item in cpHoje)
                {
                    NOTIFICACAO noti2 = new NOTIFICACAO();
                    noti2.NOTI_DT_EMISSAO = DateTime.Now;
                    noti2.NOTI_DT_VALIDADE = DateTime.Today.Date.AddDays(30);
                    noti2.NOTI_IN_NIVEL = 4;
                    noti2.NOTI_IN_VISTA = 0;
                    noti2.NOTI_NM_TITULO = "Contas a Pagar - Vencimento Hoje";
                    noti2.NOTI_IN_ATIVO = 1;
                    noti2.NOTI_TX_TEXTO = "O lan�amento " + item.CAPA_NR_DOCUMENTO + " vence HOJE,  " + DateTime.Today.Date.ToLongDateString() + ".";
                    noti2.USUA_CD_ID = item.USUA_CD_ID.Value;
                    noti2.ASSI_CD_ID = SessionMocks.IdAssinante;
                    noti2.CANO_CD_ID = 1;
                    noti2.NOTI_DT_DATA = DateTime.Today.Date;
                    noti2.NOTI_IN_ENVIADA = 1;
                    noti2.NOTI_IN_STATUS = 0;

                    // Persiste notifica��o
                    Int32 volta2 = notfApp.ValidateCreate(noti2, SessionMocks.UserCredentials);
                }
                foreach (CONTA_PAGAR item in cpAtraso)
                {
                    NOTIFICACAO noti2 = new NOTIFICACAO();
                    noti2.NOTI_DT_EMISSAO = DateTime.Now;
                    noti2.NOTI_DT_VALIDADE = DateTime.Today.Date.AddDays(30);
                    noti2.NOTI_IN_NIVEL = 4;
                    noti2.NOTI_IN_VISTA = 0;
                    noti2.NOTI_NM_TITULO = "Contas a Pagar - Atrasadas";
                    noti2.NOTI_IN_ATIVO = 1;
                    noti2.NOTI_TX_TEXTO = "O lan�amento " + item.CAPA_NR_DOCUMENTO + " venceu em " + item.CAPA_DT_VENCIMENTO.Value.ToLongDateString() + " e se encontra atrasado.";
                    noti2.USUA_CD_ID = item.USUA_CD_ID.Value;
                    noti2.ASSI_CD_ID = SessionMocks.IdAssinante;
                    noti2.CANO_CD_ID = 1;
                    noti2.NOTI_DT_DATA = DateTime.Today.Date;
                    noti2.NOTI_IN_ENVIADA = 1;
                    noti2.NOTI_IN_STATUS = 0;

                    // Persiste notifica��o
                    Int32 volta2 = notfApp.ValidateCreate(noti2, SessionMocks.UserCredentials);
                }
                foreach (CONTA_PAGAR item in cpAviso)
                {
                    NOTIFICACAO noti2 = new NOTIFICACAO();
                    noti2.NOTI_DT_EMISSAO = DateTime.Now;
                    noti2.NOTI_DT_VALIDADE = DateTime.Today.Date.AddDays(30);
                    noti2.NOTI_IN_NIVEL = 4;
                    noti2.NOTI_IN_VISTA = 0;
                    noti2.NOTI_NM_TITULO = "Contas a Pagar - Vencimento Pr�ximo";
                    noti2.NOTI_IN_ATIVO = 1;
                    noti2.NOTI_TX_TEXTO = "O lan�amento " + item.CAPA_NR_DOCUMENTO + " vencer� em " + DateTime.Today.Date.ToLongDateString() + ".";
                    noti2.USUA_CD_ID = item.USUA_CD_ID.Value;
                    noti2.ASSI_CD_ID = SessionMocks.IdAssinante;
                    noti2.CANO_CD_ID = 1;
                    noti2.NOTI_DT_DATA = DateTime.Today.Date;
                    noti2.NOTI_IN_ENVIADA = 1;
                    noti2.NOTI_IN_STATUS = 0;

                    // Persiste notifica��o
                    Int32 volta2 = notfApp.ValidateCreate(noti2, SessionMocks.UserCredentials);
                }

                // Checagem de parcelas de contas a pagar
                foreach (CONTA_PAGAR item in cps)
                {
                    List<CONTA_PAGAR_PARCELA> cpParcela = item.CONTA_PAGAR_PARCELA.Where(p => p.CPPA_DT_VENCIMENTO.Value.Date == DateTime.Today.Date & p.CPPA_IN_QUITADA == 0).ToList();
                    foreach (CONTA_PAGAR_PARCELA parc in cpParcela)
                    {
                        NOTIFICACAO noti2 = new NOTIFICACAO();
                        noti2.NOTI_DT_EMISSAO = DateTime.Now;
                        noti2.NOTI_DT_VALIDADE = DateTime.Today.Date.AddDays(30);
                        noti2.NOTI_IN_NIVEL = 4;
                        noti2.NOTI_IN_VISTA = 0;
                        noti2.NOTI_NM_TITULO = "Contas a Pagar - Parcela - Vencimento Hoje";
                        noti2.NOTI_IN_ATIVO = 1;
                        noti2.NOTI_TX_TEXTO = "A parcela " + parc.CPPA_NR_PARCELA.ToString() + "/" + parc.CONTA_PAGAR.CAPA_IN_PARCELAS.ToString() + " do lan�amento " + parc.CONTA_PAGAR.CAPA_NR_DOCUMENTO + " vence HOJE, " + DateTime.Today.Date.ToLongDateString() + ".";
                        noti2.USUA_CD_ID = item.USUA_CD_ID.Value;
                        noti2.ASSI_CD_ID = SessionMocks.IdAssinante;
                        noti2.CANO_CD_ID = 1;
                        noti2.NOTI_DT_DATA = DateTime.Today.Date;
                        noti2.NOTI_IN_ENVIADA = 1;
                        noti2.NOTI_IN_STATUS = 0;

                        // Persiste notifica��o
                        Int32 volta2 = notfApp.ValidateCreate(noti2, SessionMocks.UserCredentials);
                    }
                    List<CONTA_PAGAR_PARCELA> cpParcelaAtraso = item.CONTA_PAGAR_PARCELA.Where(p => p.CPPA_DT_VENCIMENTO.Value.Date < DateTime.Today.Date & p.CPPA_IN_QUITADA == 0).ToList();
                    foreach (CONTA_PAGAR_PARCELA parc in cpParcelaAtraso)
                    {
                        NOTIFICACAO noti2 = new NOTIFICACAO();
                        noti2.NOTI_DT_EMISSAO = DateTime.Now;
                        noti2.NOTI_DT_VALIDADE = DateTime.Today.Date.AddDays(30);
                        noti2.NOTI_IN_NIVEL = 4;
                        noti2.NOTI_IN_VISTA = 0;
                        noti2.NOTI_NM_TITULO = "Contas a Pagar - Parcela - Atraso";
                        noti2.NOTI_IN_ATIVO = 1;
                        noti2.NOTI_TX_TEXTO = "A parcela " + parc.CPPA_NR_PARCELA.ToString() + "/" + parc.CONTA_PAGAR.CAPA_IN_PARCELAS.ToString() + " do lan�amento " + parc.CONTA_PAGAR.CAPA_NR_DOCUMENTO + " venceu em " + DateTime.Today.Date.ToLongDateString() + " e est� atrasada.";
                        noti2.USUA_CD_ID = item.USUA_CD_ID.Value;
                        noti2.ASSI_CD_ID = SessionMocks.IdAssinante;
                        noti2.CANO_CD_ID = 1;
                        noti2.NOTI_DT_DATA = DateTime.Today.Date;
                        noti2.NOTI_IN_ENVIADA = 1;
                        noti2.NOTI_IN_STATUS = 0;

                        // Persiste notifica��o
                        Int32 volta2 = notfApp.ValidateCreate(noti2, SessionMocks.UserCredentials);
                    }
                    List<CONTA_PAGAR_PARCELA> cpParcelaAviso = item.CONTA_PAGAR_PARCELA.Where(p => p.CPPA_DT_VENCIMENTO.Value.Date > DateTime.Today.Date  & p.CPPA_DT_VENCIMENTO.Value.Date < DateTime.Today.Date.AddDays(5) & p.CPPA_IN_QUITADA == 0).ToList();
                    foreach (CONTA_PAGAR_PARCELA parc in cpParcelaAtraso)
                    {
                        NOTIFICACAO noti2 = new NOTIFICACAO();
                        noti2.NOTI_DT_EMISSAO = DateTime.Now;
                        noti2.NOTI_DT_VALIDADE = DateTime.Today.Date.AddDays(30);
                        noti2.NOTI_IN_NIVEL = 4;
                        noti2.NOTI_IN_VISTA = 0;
                        noti2.NOTI_NM_TITULO = "Contas a Pagar - Parcela - Aviso";
                        noti2.NOTI_IN_ATIVO = 1;
                        noti2.NOTI_TX_TEXTO = "A parcela " + parc.CPPA_NR_PARCELA.ToString() + "/" + parc.CONTA_PAGAR.CAPA_IN_PARCELAS.ToString() + " do lan�amento " + parc.CONTA_PAGAR.CAPA_NR_DOCUMENTO + " vencer� em " + DateTime.Today.Date.ToLongDateString() + ".";
                        noti2.USUA_CD_ID = item.USUA_CD_ID.Value;
                        noti2.ASSI_CD_ID = SessionMocks.IdAssinante;
                        noti2.CANO_CD_ID = 1;
                        noti2.NOTI_DT_DATA = DateTime.Today.Date;
                        noti2.NOTI_IN_ENVIADA = 1;
                        noti2.NOTI_IN_STATUS = 0;

                        // Persiste notifica��o
                        Int32 volta2 = notfApp.ValidateCreate(noti2, SessionMocks.UserCredentials);
                    }
                }
            }

            // Checagem de manuten��o
            List<NOTIFICACAO> equiHoje = SessionMocks.Notificacoes.Where(p => p.NOTI_DT_EMISSAO.Value.Date == DateTime.Today.Date & p.NOTI_IN_NIVEL == 5).ToList();
            if (equiHoje.Count == 0)
            {
                List<EQUIPAMENTO> equipamentos = equiApp.GetAllItens().Where(p => p.EQUI_DT_MANUTENCAO.Value.AddDays(p.PERIODICIDADE.PERI_NR_DIAS) <= DateTime.Today.Date).ToList();
                foreach (EQUIPAMENTO item in equipamentos)
                {
                    NOTIFICACAO noti2 = new NOTIFICACAO();
                    noti2.NOTI_DT_EMISSAO = DateTime.Now;
                    noti2.NOTI_DT_VALIDADE = DateTime.Today.Date.AddDays(30);
                    noti2.NOTI_IN_NIVEL = 5;
                    noti2.NOTI_IN_VISTA = 0;
                    noti2.NOTI_NM_TITULO = "Equipamentos - Manuten��o em atraso";
                    noti2.NOTI_IN_ATIVO = 1;
                    noti2.NOTI_TX_TEXTO = "O equipamento " + item.EQUI_NM_NOME + " est� com manuten��o vencida desde " + item.EQUI_DT_MANUTENCAO.Value.ToLongDateString() + ".";
                    noti2.USUA_CD_ID = SessionMocks.UserCredentials.USUA_CD_ID;
                    noti2.ASSI_CD_ID = SessionMocks.IdAssinante;
                    noti2.CANO_CD_ID = 1;
                    noti2.NOTI_DT_DATA = DateTime.Today.Date;
                    noti2.NOTI_IN_ENVIADA = 1;
                    noti2.NOTI_IN_STATUS = 0;

                    // Persiste notifica��o
                    Int32 volta2 = notfApp.ValidateCreate(noti2, SessionMocks.UserCredentials);
                }
            }
            Session["ErroSoma"] = 0;
            return View(vm);
        }

        public ActionResult  CarregarDashboardInicial()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            if ((Int32)Session["Login"] == 1)
            {
                SessionMocks.Perfis = baseApp.GetAllPerfis();
                SessionMocks.Cargos = carApp.GetAllItens();
                SessionMocks.Usuarios = usuApp.GetAllUsuarios();
                SessionMocks.Filiais = filApp.GetAllItens();
                SessionMocks.TipoContas = tcApp.GetAllItens();
                SessionMocks.CatsClientes = ccApp.GetAllItens();
                SessionMocks.UFs = cliApp.GetAllUF();
                SessionMocks.TiposContribuintes = tcoApp.GetAllItens();
                SessionMocks.TiposPessoas = cliApp.GetAllTiposPessoa();
                SessionMocks.Regimes = cliApp.GetAllRegimes();
                SessionMocks.CentroCustos = cuApp.GetAllItens();
                SessionMocks.Unidades = uniApp.GetAllItens();
                SessionMocks.Formas = fpApp.GetAllItens();
                SessionMocks.Periodicidades = perApp.GetAllItens();
                SessionMocks.CatsForns = cfApp.GetAllItens();
                SessionMocks.Situacoes = funApp.GetAllSituacao();
                SessionMocks.Funcoes = funApp.GetAllFuncao();
                SessionMocks.Sexos = funApp.GetAllSexo();
                SessionMocks.Escolaridades = funApp.GetAllEscolaridade();
                SessionMocks.CatsInsumos = ciApp.GetAllItens();
                SessionMocks.CatsPatrimonio = cprApp.GetAllItens();
                SessionMocks.CatsProduto = cppApp.GetAllItens();
            }

            USUARIO usu = SessionMocks.UserCredentials;
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(usu);

            SessionMocks.Notificacoes = baseApp.GetAllItensUser(usu.USUA_CD_ID);
            SessionMocks.NovasNotificacoes = SessionMocks.Notificacoes.Where(p => p.NOTI_IN_VISTA == 0).Count();
            SessionMocks.Nome = usu.USUA_NM_NOME;

            SessionMocks.Noticias = notiApp.GetAllItensValidos();
            SessionMocks.NoticiasNumero = SessionMocks.Noticias.Count;

            SessionMocks.TarefasPendentes = tarApp.GetTarefaStatus(usu.USUA_CD_ID, 1).Count;
            SessionMocks.TarefasLista = tarApp.GetByUser(usu.USUA_CD_ID);
            SessionMocks.Tarefas = SessionMocks.TarefasLista.Count;

            SessionMocks.Logs = logApp.GetAllItensUsuario(usu.USUA_CD_ID).Count;

            // Manuten��es
            List<EQUIPAMENTO> equipamentos = equiApp.GetAllItens().Where(p => p.EQUI_DT_MANUTENCAO.Value.AddDays(p.PERIODICIDADE.PERI_NR_DIAS) <= DateTime.Today.Date).ToList();
            SessionMocks.listaManutencaoVencida = equipamentos;
            SessionMocks.numEquipamentos = equipamentos.Count;
            
            // Pedidos
            List<PEDIDO_VENDA> pedidos = pvApp.GetAllItens();
            SessionMocks.PedidosTotal = pedidos.Count;
            List<PEDIDO_VENDA> pvDia = pedidos.Where(p => p.PEVE_DT_DATA.Date == DateTime.Today.Date).ToList();
            List<PEDIDO_VENDA> pvMes = pedidos.Where(p => p.PEVE_DT_DATA.Month == DateTime.Today.Date.Month).ToList();
            List<PEDIDO_VENDA> pvAno = pedidos.Where(p => p.PEVE_DT_DATA.Year == DateTime.Today.Date.Year).ToList();
            SessionMocks.NumPedidosDia = pvDia.Count;
            SessionMocks.NumPedidosMes = pvMes.Count;
            SessionMocks.NumPedidosAno = pvAno.Count;

            Decimal valor = 0;
            foreach (var itemdia in pvDia)
            {
                foreach (var item in itemdia.ITEM_PEDIDO_VENDA)
                {
                    valor += item.ITPE_QN_QUANTIDADE * item.PRODUTO.PROD_VL_PRECO_VENDA.Value;

                }
            }
            SessionMocks.PedidosDia = valor;
            valor = 0;
            foreach (var itemdia in pvMes)
            {
                foreach (var item in itemdia.ITEM_PEDIDO_VENDA)
                {
                    valor += item.ITPE_QN_QUANTIDADE * item.PRODUTO.PROD_VL_PRECO_VENDA.Value;

                }
            }
            SessionMocks.PedidosMes = valor;
            valor = 0;
            foreach (var itemdia in pvAno)
            {
                foreach (var item in itemdia.ITEM_PEDIDO_VENDA)
                {
                    valor += item.ITPE_QN_QUANTIDADE * item.PRODUTO.PROD_VL_PRECO_VENDA.Value;

                }
            }
            SessionMocks.PedidosAno = valor;

            // Contas a pagar
            List<CONTA_PAGAR> cps = cpApp.GetAllItens();
            SessionMocks.CPTotal = cps.Count;
            SessionMocks.CPDia = cps.Where(p => p.CAPA_DT_LANCAMENTO.Value.Date == DateTime.Today.Date).ToList().Sum(p => p.CAPA_VL_VALOR).Value;
            SessionMocks.CPMes = cps.Where(p => p.CAPA_DT_LANCAMENTO.Value.Month == DateTime.Today.Date.Month).ToList().Sum(p => p.CAPA_VL_VALOR).Value;
            SessionMocks.CPAno = cps.Where(p => p.CAPA_DT_LANCAMENTO.Value.Year == DateTime.Today.Date.Year).ToList().Sum(p => p.CAPA_VL_VALOR).Value;

            // Contas a Receber
            List<CONTA_RECEBER> crs = crApp.GetAllItens();
            SessionMocks.CRTotal = crs.Count;
            SessionMocks.CRDia = crs.Where(p => p.CARE_DT_LANCAMENTO.Date == DateTime.Today.Date).ToList().Sum(p => p.CARE_VL_VALOR);
            SessionMocks.CRMes = crs.Where(p => p.CARE_DT_LANCAMENTO.Month == DateTime.Today.Date.Month).ToList().Sum(p => p.CARE_VL_VALOR);
            SessionMocks.CRAno = crs.Where(p => p.CARE_DT_LANCAMENTO.Year == DateTime.Today.Date.Year).ToList().Sum(p => p.CARE_VL_VALOR);

            // Caixa
            SessionMocks.CaixaDia = SessionMocks.CRDia - SessionMocks.CPDia;
            SessionMocks.CaixaMes = SessionMocks.CRMes - SessionMocks.CPMes;
            SessionMocks.CaixaAno = SessionMocks.CRAno - SessionMocks.CPAno;
            SessionMocks.flagInicial = 1;

            // Estoque
            List<PRODUTO> listaProd = prodApp.GetAllItens();
            SessionMocks.estoqueProd = listaProd.Sum(p => p.PROD_QN_ESTOQUE * p.PROD_VL_PRECO_VENDA.Value);
            SessionMocks.estoqueProdMinimo = listaProd.Where(p => p.PROD_QN_ESTOQUE < p.PROD_QN_QUANTIDADE_MINIMA).ToList().Count; 

            List<MATERIA_PRIMA> listaIns = insApp.GetAllItens();
            SessionMocks.estoqueInsumo = listaIns.Sum(p => p.MAPR_QN_ESTOQUE * p.MAPR_VL_CUSTO.Value);
            SessionMocks.estoqueInsMinimo = listaIns.Where(p => p.MAPR_QN_ESTOQUE < p.MAPR_QN_ESTOQUE_MINIMO).ToList().Count;

            // Produtos
            SessionMocks.pontoPedidoProd = listaProd.Where(p => p.PROD_QN_ESTOQUE < p.PROD_QN_QUANTIDADE_MINIMA & p.PROD_IN_ATIVO == 1 & p.ASSI_CD_ID == SessionMocks.IdAssinante & p.PROD_IN_COMPOSTO == 0).ToList();
            SessionMocks.estoqueZeradoProd = listaProd.Where(p => p.PROD_QN_ESTOQUE <= 0 & p.PROD_IN_ATIVO == 1 & p.ASSI_CD_ID == SessionMocks.IdAssinante & p.PROD_IN_COMPOSTO == 0).ToList();
            SessionMocks.flagVoltaProd = 2;

            // Insumos
            SessionMocks.InsPontoPedido = listaIns.Where(p => p.MAPR_QN_ESTOQUE < p.MAPR_QN_ESTOQUE_MINIMO & p.MAPR_IN_ATIVO == 1).ToList().Count;
            SessionMocks.InsEstoqueZerado = listaIns.Where(p => p.MAPR_QN_ESTOQUE <= 0 & p.MAPR_IN_ATIVO == 1).ToList().Count;
            SessionMocks.flagVoltaIns = 2;

            // Pedidos - Grid
            List<PEDIDO_VENDA> listaPed = null;
            if (SessionMocks.filtroData == 1)
            {
                listaPed = pedidos.Where(p => p.PEVE_DT_DATA.Month == DateTime.Today.Date.Month).ToList();
            }
            else if (SessionMocks.filtroData == 2)
            {
                listaPed = pedidos.Where(p => p.PEVE_DT_DATA.Year == DateTime.Today.Date.Year).ToList();
            }
            if (SessionMocks.filtroStatus == 1)
            {
                listaPed = listaPed.Where(p => p.PEVE_IN_STATUS == 4).ToList();
            }
            else if (SessionMocks.filtroStatus == 2)
            {
                listaPed = listaPed.Where(p => p.PEVE_IN_STATUS == 3).ToList();
            }
            else
            {
                listaPed = listaPed.Where(p => p.PEVE_IN_STATUS == 5).ToList();
            }

            List<RESUMO_VENDA> resumos = pvApp.GetResumos();
            foreach (var item in resumos)
            {
                Int32 volta = pvApp.DeleteResumoVenda(item);
            }            

            listaPed = listaPed.OrderBy(a => a.PEVE_DT_DATA).ToList();
            if (listaPed.Count == 0)
            {
                SessionMocks.listaResumos = new List<RESUMO_VENDA>();
                SessionMocks.numeroTotal = 0;
                SessionMocks.valorTotal = 0;
            }
            else
            {
                Int32 cont = listaPed.Count;
                Int32 J = 0;
                DateTime dataComp = listaPed.First().PEVE_DT_DATA;
                Decimal soma = 0;
                Decimal somaData = 0;
                Int32 numeroData = 0;
                Decimal somaTotal = 0;
                Int32 numeroTotal = 0;
                foreach (var item in listaPed)
                {
                    if (item.PEVE_DT_DATA == dataComp)
                    {
                        J++;
                        numeroData++;
                        numeroTotal++;
                        soma = 0;
                        foreach (var ped in item.ITEM_PEDIDO_VENDA)
                        {
                            soma += ped.ITPE_QN_QUANTIDADE * ped.PRODUTO.PROD_VL_PRECO_VENDA.Value;
                        }
                        somaData += soma;
                        somaTotal += soma;
                    }
                    else
                    {
                        RESUMO_VENDA res1 = new RESUMO_VENDA();
                        res1.ASSI_CD_ID = SessionMocks.IdAssinante;
                        res1.REVE_DT_DATA = dataComp;
                        res1.REVE_NR_NUMERO = numeroData;
                        res1.REVE_VL_VALOR = somaData;
                        Int32 volta1 = pvApp.CreateResumoVenda(res1);

                        J++;
                        dataComp = item.PEVE_DT_DATA;
                        numeroData = 1;
                        numeroTotal++;
                        soma = 0;
                        foreach (var ped in item.ITEM_PEDIDO_VENDA)
                        {
                            soma += ped.ITPE_QN_QUANTIDADE * ped.PRODUTO.PROD_VL_PRECO_VENDA.Value;
                        }
                        somaData = soma;
                        somaTotal += soma;
                    }
                }

                RESUMO_VENDA res = new RESUMO_VENDA();
                res.ASSI_CD_ID = SessionMocks.IdAssinante;
                res.REVE_DT_DATA = dataComp;
                res.REVE_NR_NUMERO = numeroData;
                res.REVE_VL_VALOR = somaData;
                Int32 volta2 = pvApp.CreateResumoVenda(res);

                SessionMocks.listaResumos = pvApp.GetResumos().OrderBy(p => p.REVE_DT_DATA).ToList();
                SessionMocks.numeroTotal = numeroTotal;
                SessionMocks.valorTotal = somaTotal;
            }


            // Interface
            String frase = String.Empty;
            String nome = usu.USUA_NM_NOME.Substring(0, usu.USUA_NM_NOME.IndexOf(" "));
            if (DateTime.Now.Hour <= 12)
            {
                frase = "Bom dia, " + nome;
            }
            else if (DateTime.Now.Hour > 12 & DateTime.Now.Hour <= 18)
            {
                frase = "Boa tarde, " + nome;
            }
            else
            {
                frase = "Boa noite, " + nome;
            }
            SessionMocks.Greeting = frase;
            SessionMocks.Foto = usu.USUA_AQ_FOTO;
            return View(vm);
        }

        public ActionResult CarregarDesenvolvimento()
        {
            return View();
        }

        public ActionResult VoltarDashboard()
        {
            return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
        }

        public ActionResult DashMesCorrente()
        {
            SessionMocks.filtroData = 1;
            return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
        }

        public ActionResult DashAnoCorrente()
        {
            SessionMocks.filtroData = 2;
            return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
        }

        public ActionResult DashParaAprovacao()
        {
            SessionMocks.filtroStatus = 1;
            return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
        }

        public ActionResult DashAprovados()
        {
            SessionMocks.filtroStatus = 2;
            return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
        }

        public ActionResult DashEncerrados()
        {
            SessionMocks.filtroStatus = 3;
            return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
        }

        public ActionResult MontarFaleConosco()
        {
            return View();
        }

    }
}