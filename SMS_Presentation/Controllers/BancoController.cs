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

namespace Odonto.Controllers
{
    public class BancoController : Controller
    {
        private readonly IBancoAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IContaBancariaAppService contaApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IFilialAppService filApp;
        private readonly ITipoContaAppService tcApp;

        //private readonly IContaPagarAppService pagApp;
        //private readonly IContaReceberAppService recApp;

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
        CONTA_BANCO contaPadrao = new CONTA_BANCO();

        //public BancoController(IBancoAppService baseApps, ILogAppService logApps, IContaBancariaAppService contaApps, IUsuarioAppService usuApps, IFilialAppService filApps, IContaPagarAppService pagApps, IContaReceberAppService recApps)
        public BancoController(IBancoAppService baseApps, ILogAppService logApps, IContaBancariaAppService contaApps, IUsuarioAppService usuApps, IFilialAppService filApps, ITipoContaAppService tcApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            contaApp = contaApps;
            usuApp = usuApps;
            filApp = filApps;
            tcApp = tcApps;
            //pagApp = pagApps;
            //recApp = recApps;
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

        [HttpGet]
        public ActionResult MontarTelaBanco()
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
            if (Session["ListaBanco"] == null)
            {
                listaMasterBanco = baseApp.GetAllItens(idAss);
                Session["ListaBanco"] = listaMasterBanco;
            }
            ViewBag.Listas = (List<BANCO>)Session["ListaBanco"];
            ViewBag.Title = "Bancos";

            // Indicadores
            ViewBag.Bancos = ((List<BANCO>)Session["ListaBanco"]).Count;
            ViewBag.Contas = contaApp.GetAllItens(idAss).Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensBanco"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensBanco"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensBanco"] == 4)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0039", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensBanco"] = 0;
            objetoBanco = new BANCO();
            return View(objetoBanco);
        }

        public ActionResult RetirarFiltroBanco()
        {
            Session["ListsBanco"] = null;
            return RedirectToAction("MontarTelaBanco");
        }

        public ActionResult MostrarTudoBanco()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterBanco = baseApp.GetAllItensAdm(idAss);
            Session["ListaBanco"] = listaMasterBanco;
            return RedirectToAction("MontarTelaBanco");
        }

        [HttpPost]
        public ActionResult FiltrarBanco(BANCO item)
        {
            try
            {
                // Executa a operação
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<BANCO> listaObj = new List<BANCO>();
                Int32 volta = baseApp.ExecuteFilter(item.BANC_SG_CODIGO, item.BANC_NM_NOME, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensBanco"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaBanco");
                }

                // Sucesso
                listaMasterBanco = listaObj;
                Session["ListaBanco"] = listaObj;
                return RedirectToAction("MontarTelaBanco");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaBanco");
            }
        }

        public ActionResult VoltarBaseBanco()
        {
            listaMasterBanco = new List<BANCO>();
            Session["ListsBanco"] = null;
            return RedirectToAction("MontarTelaBanco");
        }

        [HttpGet]
        public ActionResult IncluirBanco()
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");

            // Prepara view
            BANCO item = new BANCO();
            BancoViewModel vm = Mapper.Map<BANCO, BancoViewModel>(item);
            vm.BANC_IN_ATIVO = 1;
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirBanco(BancoViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    BANCO item = Mapper.Map<BancoViewModel, BANCO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensBanco"] = 3;
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0038", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    Session["Banco"] = item;
                    listaMasterBanco = new List<BANCO>();
                    Session["ListaBanco"] = null;
                    Session["VoltaConta"] = 1;
                    return RedirectToAction("IncluirConta");
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
        public ActionResult EditarBanco(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("MontarTelaBanco", "Banco");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Mensagem
            if ((Int32)Session["MensBanco"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensBanco"] == 6)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0041", CultureInfo.CurrentCulture));
            }

            // Prepara view
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            BANCO item = baseApp.GetItemById(id);
            objetoBancoAntes = item;
            Session["IdBanco"] = id;
            Session["Banco"] = item;
            BancoViewModel vm = Mapper.Map<BANCO, BancoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarBanco(BancoViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    BANCO item = Mapper.Map<BancoViewModel, BANCO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoBancoAntes, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0038", CultureInfo.CurrentCulture));
                        return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
                    }

                    // Sucesso
                    listaMasterBanco = new List<BANCO>();
                    Session["ListaBanco"] = null;
                    return RedirectToAction("MontarTelaBanco");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
                }
            }
            else
            {
                return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
            }
        }

        [HttpGet]
        public ActionResult VerBanco(Int32 id)
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

            // Mensagem
            if ((Int32)Session["MensBanco"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensBanco"] == 6)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0041", CultureInfo.CurrentCulture));
            }

            // Prepara view
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            BANCO item = baseApp.GetItemById(id);
            objetoBancoAntes = item;
            Session["IdBanco"] = id;
            Session["Banco"] = item;
            BancoViewModel vm = Mapper.Map<BANCO, BancoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerBanco(BancoViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];

                    // Sucesso
                    listaMasterBanco = new List<BANCO>();
                    Session["ListaBanco"] = null;
                    return RedirectToAction("MontarTelaBanco");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return RedirectToAction("VerBanco", new { id = (Int32)Session["IdBanco"] });
                }
            }
            else
            {
                return RedirectToAction("VerBanco", new { id = (Int32)Session["IdBanco"] });
            }
        }

        [HttpGet]
        public ActionResult ExcluirBanco(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("MontarTelaBanco", "Banco");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            BANCO item = baseApp.GetItemById(id);
            objetoBancoAntes = (BANCO)Session["Banco"];
            item.BANC_IN_ATIVO = 0;
            item.ASSINANTE = null;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensBanco"] = 4;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0039", CultureInfo.CurrentCulture));
                return RedirectToAction("MontarTelaBanco");
            }
            listaMasterBanco = new List<BANCO>();
            Session["ListaBanco"] = null;
            return RedirectToAction("MontarTelaBanco");
        }

        [HttpGet]
        public ActionResult ReativarBanco(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("MontarTelaBanco", "Banco");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            BANCO item = baseApp.GetItemById(id);
            objetoBancoAntes = (BANCO)Session["Banco"];
            item.BANC_IN_ATIVO = 1;
            item.ASSINANTE = null;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            listaMasterBanco = new List<BANCO>();
            Session["ListaBanco"] = null;
            return RedirectToAction("MontarTelaBanco");
        }

        [HttpGet]
        public ActionResult IncluirConta()
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            BANCO banco = (BANCO)Session["Banco"];

            // Prepara listas
            ViewBag.Tipos = new SelectList(tcApp.GetAllItens(), "TICO_CD_ID", "TICO_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");

            // Prepara view
            CONTA_BANCO item = new CONTA_BANCO();
            ContaBancariaViewModel vm = Mapper.Map<CONTA_BANCO, ContaBancariaViewModel>(item);
            vm.BANC_CD_ID = banco.BANC_CD_ID;
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            vm.COBA_DT_ABERTURA = DateTime.Today.Date;
            vm.COBA_VL_SALDO_INICIAL = 0;
            vm.COBA_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirConta(ContaBancariaViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Tipos = new SelectList(tcApp.GetAllItens(), "TICO_CD_ID", "TICO_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CONTA_BANCO item = Mapper.Map<ContaBancariaViewModel, CONTA_BANCO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = contaApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensBanco"] = 5;
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0040", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterConta = new List<CONTA_BANCO>();
                    Session["ListaContaBancaria"] = null;
                    Session["ContasBancarias"] = contaApp.GetAllItens(idAss);
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
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
        public ActionResult EditarConta(Int32 id)
        {
            // Verifica se tem usuario logado
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

            // Prepara listas
            ViewBag.Tipos = new SelectList(tcApp.GetAllItens(), "TICO_CD_ID", "TICO_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");

            // Prepara view
            CONTA_BANCO item = contaApp.GetItemById(id);
            ViewBag.Lanc = item.CONTA_BANCO_LANCAMENTO.Count;
            //ViewBag.Pagar = pagApp.GetAllItens().Where(p => p.COBA_CD_ID == id).ToList().Count;
            //ViewBag.Receber = recApp.GetAllItens().Where(p => p.COBA_CD_ID == id).ToList().Count;

            objContaAntes = item;
            Session["IdVolta"] = id;
            Session["ContaPadrao"] = item;
            ContaBancariaViewModel vm = Mapper.Map<CONTA_BANCO, ContaBancariaViewModel>(item);
            return View(vm); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarConta(ContaBancariaViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Tipos = new SelectList(tcApp.GetAllItens(), "TICO_CD_ID", "TICO_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CONTA_BANCO item = Mapper.Map<ContaBancariaViewModel, CONTA_BANCO>(vm);
                    Int32 volta = contaApp.ValidateEdit(item, objContaAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterConta = new List<CONTA_BANCO>();
                    Session["ListaContaBancaria"] = null;
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
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
        public ActionResult VerConta(Int32 id)
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

            // Prepara listas

            // Prepara view
            CONTA_BANCO item = contaApp.GetItemById(id);
            ViewBag.Lanc = item.CONTA_BANCO_LANCAMENTO.Count;
            //ViewBag.Pagar = pagApp.GetAllItens().Where(p => p.COBA_CD_ID == id).ToList().Count;
            //ViewBag.Receber = recApp.GetAllItens().Where(p => p.COBA_CD_ID == id).ToList().Count;

            objContaAntes = item;
            Session["IdVolta"] = id;
            Session["ContaPadrao"] = item;
            ContaBancariaViewModel vm = Mapper.Map<CONTA_BANCO, ContaBancariaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerConta(ContaBancariaViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação

                    // Verifica retorno

                    // Sucesso
                    listaMasterConta = new List<CONTA_BANCO>();
                    Session["ListaContaBancaria"] = null;
                    return RedirectToAction("VerBanco", new { id = (Int32)Session["IdBanco"] });
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

        public ActionResult ExcluirConta(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            CONTA_BANCO item = contaApp.GetItemById(id);
            objContaAntes = (CONTA_BANCO)Session["ContaBancaria"];
            item.COBA_IN_ATIVO = 0;
            item.ASSINANTE = null;
            Int32 volta = contaApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensBanco"] = 6;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0041", CultureInfo.CurrentCulture));
                return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
            }
            listaMasterConta = new List<CONTA_BANCO>();
            Session["ListaContaBancaria"] = null;
            return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
        }

        public ActionResult ReativarConta(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            CONTA_BANCO item = contaApp.GetItemById(id);
            objContaAntes = (CONTA_BANCO)Session["ContaBancaria"];
            item.COBA_IN_ATIVO = 1;
            item.ASSINANTE = null;
            Int32 volta = contaApp.ValidateReativar(item, usuario);
            listaMasterConta = new List<CONTA_BANCO>();
            Session["ListaContaBancaria"] = null;
            return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
        }

        public ActionResult VoltarBaseConta()
        {
            listaMasterConta = new List<CONTA_BANCO>();
            Session["ListaContaBancaria"] = null;
            return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
        }

        [HttpGet]
        public ActionResult EditarContato(Int32 id)
        {
            // Prepara view
            CONTA_BANCO_CONTATO item = contaApp.GetContatoById(id);
            ContaBancariaContatoViewModel vm = Mapper.Map<CONTA_BANCO_CONTATO, ContaBancariaContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarContato(ContaBancariaContatoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CONTA_BANCO_CONTATO item = Mapper.Map<ContaBancariaContatoViewModel, CONTA_BANCO_CONTATO>(vm);
                    Int32 volta = contaApp.ValidateEditContato(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoConta");
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
        public ActionResult VerContato(Int32 id)
        {
            // Prepara view
            CONTA_BANCO_CONTATO item = contaApp.GetContatoById(id);
            ContaBancariaContatoViewModel vm = Mapper.Map<CONTA_BANCO_CONTATO, ContaBancariaContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerContato(ContaBancariaContatoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoContaVer");
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            CONTA_BANCO_CONTATO item = contaApp.GetContatoById(id);
            item.CBCT_IN_ATIVO = 0;
            Int32 volta = contaApp.ValidateEditContato(item);
            return RedirectToAction("VoltarAnexoConta");
        }

        [HttpGet]
        public ActionResult ReativarContato(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            CONTA_BANCO_CONTATO item = contaApp.GetContatoById(id);
            item.CBCT_IN_ATIVO = 1;
            Int32 volta = contaApp.ValidateEditContato(item);
            return RedirectToAction("VoltarAnexoConta");
        }

        [HttpGet]
        public ActionResult IncluirContato()
        {
            // Prepara view
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            CONTA_BANCO_CONTATO item = new CONTA_BANCO_CONTATO();
            ContaBancariaContatoViewModel vm = Mapper.Map<CONTA_BANCO_CONTATO, ContaBancariaContatoViewModel>(item);
            vm.COBA_CD_ID = (Int32)Session["IdVolta"];
            vm.CBCT_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirContato(ContaBancariaContatoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CONTA_BANCO_CONTATO item = Mapper.Map<ContaBancariaContatoViewModel, CONTA_BANCO_CONTATO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = contaApp.ValidateCreateContato(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoConta");
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
        public ActionResult IncluirLancamento()
        {
            // Prepara view
            List<SelectListItem> tipoLancamento = new List<SelectListItem>();
            tipoLancamento.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoLancamento.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.TiposLancamento = new SelectList(tipoLancamento, "Value", "Text");
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            CONTA_BANCO_LANCAMENTO item = new CONTA_BANCO_LANCAMENTO();
            ContaBancariaLancamentoViewModel vm = Mapper.Map<CONTA_BANCO_LANCAMENTO, ContaBancariaLancamentoViewModel>(item);
            vm.COBA_CD_ID = (Int32)Session["IdVolta"];
            vm.CBLA_IN_ATIVO = 1;
            vm.CBLA_IN_ORIGEM = 1;
            vm.CBLA_DT_LANCAMENTO = DateTime.Today.Date;
            vm.CONTA_BANCO = (CONTA_BANCO)Session["ContaPadrao"];
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirLancamento(ContaBancariaLancamentoViewModel vm)
        {
            List<SelectListItem> tipoLancamento = new List<SelectListItem>();
            tipoLancamento.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoLancamento.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.TiposLancamento = new SelectList(tipoLancamento, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CONTA_BANCO_LANCAMENTO item = Mapper.Map<ContaBancariaLancamentoViewModel, CONTA_BANCO_LANCAMENTO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = contaApp.ValidateCreateLancamento(item, null);
                    Int32 volta1 = AcertaSaldo(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoConta");
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
        public ActionResult EditarLancamento(Int32 id)
        {
            // Prepara view
            List<SelectListItem> tipoLancamento = new List<SelectListItem>();
            tipoLancamento.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoLancamento.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.TiposLancamento = new SelectList(tipoLancamento, "Value", "Text");
            CONTA_BANCO_LANCAMENTO item = contaApp.GetLancamentoById(id);
            ContaBancariaLancamentoViewModel vm = Mapper.Map<CONTA_BANCO_LANCAMENTO, ContaBancariaLancamentoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarLancamento(ContaBancariaLancamentoViewModel vm)
        {
            List<SelectListItem> tipoLancamento = new List<SelectListItem>();
            tipoLancamento.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoLancamento.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.TiposLancamento = new SelectList(tipoLancamento, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CONTA_BANCO_LANCAMENTO item = Mapper.Map<ContaBancariaLancamentoViewModel, CONTA_BANCO_LANCAMENTO>(vm);
                    Int32 volta = contaApp.ValidateEditLancamento(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoConta");
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
        public ActionResult VerLancamento(Int32 id)
        {
            // Prepara view
            CONTA_BANCO_LANCAMENTO item = contaApp.GetLancamentoById(id);
            ContaBancariaLancamentoViewModel vm = Mapper.Map<CONTA_BANCO_LANCAMENTO, ContaBancariaLancamentoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerLancamento(ContaBancariaLancamentoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoContaVer");
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
        public ActionResult ExcluirLancamento(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CONTA_BANCO_LANCAMENTO item = contaApp.GetLancamentoById(id);
            item.CBLA_IN_ATIVO = 0;
            if (item.CBLA_IN_TIPO == 1)
            {
                item.CONTA_BANCO.COBA_VL_SALDO_ATUAL = item.CONTA_BANCO.COBA_VL_SALDO_ATUAL - item.CBLA_VL_VALOR.Value;
            }
            else
            {
                item.CONTA_BANCO.COBA_VL_SALDO_ATUAL = item.CONTA_BANCO.COBA_VL_SALDO_ATUAL + item.CBLA_VL_VALOR.Value;
            }
            
            Int32 volta = contaApp.ValidateEditLancamento(item);
            return RedirectToAction("VoltarAnexoConta");
        }

        [HttpGet]
        public ActionResult ReativarLancamento(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensBanco"] = 2;
                    return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            CONTA_BANCO_LANCAMENTO item = contaApp.GetLancamentoById(id);
            item.CBLA_IN_ATIVO = 1;
            if (item.CBLA_IN_TIPO == 1)
            {
                item.CONTA_BANCO.COBA_VL_SALDO_ATUAL = item.CONTA_BANCO.COBA_VL_SALDO_ATUAL + item.CBLA_VL_VALOR.Value;
            }
            else
            {
                item.CONTA_BANCO.COBA_VL_SALDO_ATUAL = item.CONTA_BANCO.COBA_VL_SALDO_ATUAL - item.CBLA_VL_VALOR.Value;
            }

            Int32 volta = contaApp.ValidateEditLancamento(item);
            return RedirectToAction("VoltarAnexoConta");
        }

        public ActionResult VoltarAnexoConta()
        {
            return RedirectToAction("EditarConta", new { id = (Int32)Session["IdVolta"] });
        }

        public ActionResult VoltarAnexoContaVer()
        {
            return RedirectToAction("VerConta", new { id = (Int32)Session["IdVolta"] });
        }

        public ActionResult VoltarAnexoBanco()
        {
            return RedirectToAction("EditarBanco", new { id = (Int32)Session["IdBanco"] });
        }

        public ActionResult VoltarAnexoBancoVer()
        {
            return RedirectToAction("VerBanco", new { id = (Int32)Session["IdBanco"] });
        }

        public Int32 AcertaSaldo(CONTA_BANCO_LANCAMENTO item)
        {
            try
            {
                // Acerta saldo
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                CONTA_BANCO_LANCAMENTO lanc = contaApp.GetLancamentoById(item.CBLA_CD_ID);
                if (item.CBLA_IN_TIPO == 1)
                {
                    lanc.CONTA_BANCO.COBA_VL_SALDO_ATUAL = lanc.CONTA_BANCO.COBA_VL_SALDO_ATUAL + item.CBLA_VL_VALOR.Value;
                }
                else
                {
                    lanc.CONTA_BANCO.COBA_VL_SALDO_ATUAL = lanc.CONTA_BANCO.COBA_VL_SALDO_ATUAL - item.CBLA_VL_VALOR.Value;
                }
                Int32 volta = contaApp.ValidateEditLancamento(lanc);
                return volta;
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }
    }
}