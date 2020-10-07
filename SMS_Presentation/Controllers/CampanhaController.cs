using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
//using EntitiesServices.Work_Classes;
using AutoMapper;
using SMS_Presentation.ViewModels;
using System.IO;

namespace SMS_Presentation.Controllers
{
    public class CampanhaController : Controller
    {
        private readonly ICampanhaAppService baseApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IContatoAppService conApp;
        private readonly IGrupoAppService gruApp;
        private readonly ICampanhaContatoAppService ccApp;
        private readonly ICampanhaGrupoAppService cgApp;

        private String msg;
        private Exception exception;
        CAMPANHA objetoAss = new CAMPANHA();
        CAMPANHA objetoAssAntes = new CAMPANHA();
        List<CAMPANHA> listaMasterAss = new List<CAMPANHA>();
        String extensao;

        public CampanhaController(ICampanhaAppService baseApps, IUsuarioAppService usuApps, IContatoAppService conApps, IGrupoAppService gruApps, ICampanhaContatoAppService ccApps, ICampanhaGrupoAppService cgApps)
        {
            baseApp = baseApps;
            usuApp = usuApps;
            conApp = conApps;
            gruApp = gruApps;
            ccApp = ccApps;
            cgApp = cgApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
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

        public ActionResult VoltarGeral()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult DashboardAdministracao()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaCampanha()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            usuario = (USUARIO)Session["UserCredentials"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List<CAMPANHA>)Session["ListaCampanha"] == null)
            {
                listaMasterAss = baseApp.GetAllItens(idAss);
                Session["ListaCampanha"] = listaMasterAss;
            }
            ViewBag.Listas = (List<CAMPANHA>)Session["ListaCampanha"];
            ViewBag.Title = "Campanhas";

            // Indicadores
            ViewBag.Campanhas = ((List<CAMPANHA>)Session["ListaCampanha"]).Count;

            // Mensagem
            if ((Int32)Session["MensCampanha"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensCampanha"] = 0;
            objetoAss = new CAMPANHA();
            return View(objetoAss);
        }

        public ActionResult RetirarFiltroCampanha()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaCampanha"] = null;
            return RedirectToAction("MontarTelaCampanha");
        }

        public ActionResult MostrarTudoCampanha()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterAss = baseApp.GetAllItensAdm(idAss);
            Session["ListaCampanha"] = listaMasterAss;
            return RedirectToAction("MontarTelaCampanha");
        }

        [HttpPost]
        public ActionResult FiltrarCampanha(CAMPANHA item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                List<CAMPANHA> listaObj = new List<CAMPANHA>();
                Int32 idAss = (Int32)Session["IdAssinante"];
                Int32 volta = baseApp.ExecuteFilter(item.CAMP_NM_NOME, item.CAMP_DS_DESCRICAO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCampanha"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaCampanha");
                }

                // Sucesso
                Session["MensCampanha"] = 0;
                listaMasterAss = listaObj;
                Session["ListaCampanha"] = listaObj;
                return RedirectToAction("MontarTelaCampanha");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaCampanha");
            }
        }

        public ActionResult VoltarBaseCampanha()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaCampanha");
        }

        [HttpGet]
        public ActionResult IncluirCampanha()
        {
            // Prepara listas
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            CAMPANHA item = new CAMPANHA();
            CampanhaViewModel vm = Mapper.Map<CAMPANHA, CampanhaViewModel>(item);
            vm.CAMP_IN_ATIVO = 1;
            vm.ASSI_CD_ID = idAss;
            vm.CAMP_DT_INICIO = DateTime.Today.Date;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirCampanha(CampanhaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CAMPANHA item = Mapper.Map<CampanhaViewModel, CAMPANHA>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensCampanha"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0031", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterAss = new List<CAMPANHA>();
                    Session["ListaCampanha"] = null;
                    Session["VoltaCampanha"] = 1;
                    Session["IdAssinanteVolta"] = item.ASSI_CD_ID;
                    Session["Campanha"] = item;
                    Session["MensCampanha"] = 0;
                    Session["IdVolta"] = item.CAMP_CD_ID;
                    return RedirectToAction("EditarCampanha", new { id = item.CAMP_CD_ID });
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
        public ActionResult EditarCampanha(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Contatos = new SelectList(conApp.GetAllItens(idAss), "CONT_CD_ID", "CONT_NM_NOME");
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GRUP_NM_NOME");

            if ((Int32)Session["MensCampanha"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0032", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCampanha"] == 2)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0033", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCampanha"] == 3)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0047", CultureInfo.CurrentCulture));
            }

            CAMPANHA item = baseApp.GetItemById(id);
            objetoAssAntes = item;
            Session["Campanha"] = item;
            Session["IdVolta"] = id;
            Session["MensCampanha"] = 0;
            CampanhaViewModel vm = Mapper.Map<CAMPANHA, CampanhaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarCampanha(CampanhaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Contatos = new SelectList(conApp.GetAllItens(idAss), "CONT_CD_ID", "CONT_NM_NOME");
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GRUP_NM_NOME");
            if (ModelState.IsValid)
            {
                try
            {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CAMPANHA item = Mapper.Map<CampanhaViewModel, CAMPANHA>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAssAntes, usuarioLogado, idAss);

                    // Sucesso
                    listaMasterAss = new List<CAMPANHA>();
                    Session["ListaCampanha"] = null;
                    Session["MensCampanha"] = 0;
                    return RedirectToAction("MontarTelaCampanha");
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
        public ActionResult ExcluirCampanha(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            CAMPANHA item = baseApp.GetItemById(id);
            CampanhaViewModel vm = Mapper.Map<CAMPANHA, CampanhaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirCampanha(CampanhaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                CAMPANHA item = Mapper.Map<CampanhaViewModel, CAMPANHA>(vm);
                Int32 volta = baseApp.ValidateDelete(item, usuarioLogado, idAss);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCampanha"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0034", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                
                // Sucesso
                listaMasterAss = new List<CAMPANHA>();
                Session["ListaCampanha"] = null;
                Session["MensCampanha"] = 0;
                return RedirectToAction("MontarTelaCampanha");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarCampanha(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            CAMPANHA item = baseApp.GetItemById(id);
            CampanhaViewModel vm = Mapper.Map<CAMPANHA, CampanhaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarCampanha(CampanhaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                CAMPANHA item = Mapper.Map<CampanhaViewModel, CAMPANHA>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuarioLogado, idAss);

                // Sucesso
                listaMasterAss = new List<CAMPANHA>();
                Session["ListaCampanha"] = null;
                Session["MensCampanha"] = 0;
                return RedirectToAction("MontarTelaCampanha");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult VoltarAnexoCampanha()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 volta = (Int32)Session["IdVolta"];
            return RedirectToAction("EditarCampanha", new { id = volta });
        }

        [HttpPost]
        public ActionResult IncluirCampanhaContato(CampanhaViewModel vm)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                Int32 idAss = (Int32)Session["IdAssinante"];
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                CAMPANHA item = Mapper.Map<CampanhaViewModel, CAMPANHA>(vm);

                // verifica quantidade de contatos na campanha
                if (item.CAMPANHA_CONTATO.Count >= 200)
                {
                    Session["MensCampanha"] = 3;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0047", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoGrupo");
                }
                Int32 volta = baseApp.IncluirCampanhaContato(item, usuarioLogado, idAss);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCampanha"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0032", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoCampanha");
                }

                // Sucesso
                return RedirectToAction("VoltarAnexoCampanha");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("VoltarAnexoCampanha");
            }
        }

        [HttpGet]
        public ActionResult ReativarCampanhaContato(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
            CAMPANHA_CONTATO item = ccApp.GetItemById(id);
            item.CACT_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditCampanhaContato(item);
            return RedirectToAction("VoltarAnexoCampanha");
        }

        [HttpGet]
        public ActionResult ExcluirCampanhaContato(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];

            CAMPANHA rot = (CAMPANHA)Session["Campanha"];
            CAMPANHA_CONTATO rl = ccApp.GetItemById(id);
            Int32 volta = ccApp.ValidateDelete(rl);
            return RedirectToAction("VoltarAnexoCampanha");
        }

        [HttpPost]
        public ActionResult IncluirCampanhaGrupo(CampanhaViewModel vm)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                Int32 idAss = (Int32)Session["IdAssinante"];
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                CAMPANHA item = Mapper.Map<CampanhaViewModel, CAMPANHA>(vm);
                Int32 volta = baseApp.IncluirCampanhaGrupo(item, usuarioLogado, idAss);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCampanha"] = 2;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0033", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoCampanha");
                }

                // Sucesso
                return RedirectToAction("VoltarAnexoCampanha");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("VoltarAnexoCampanha");
            }
        }

        [HttpGet]
        public ActionResult ReativarCampanhaGrupo(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
            CAMPANHA_GRUPO item = cgApp.GetItemById(id);
            item.CAGR_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditCampanhaGrupo(item);
            return RedirectToAction("VoltarAnexoCampanha");
        }

        [HttpGet]
        public ActionResult ExcluirCampanhaGrupo(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];

            CAMPANHA rot = (CAMPANHA)Session["Campanha"];
            CAMPANHA_GRUPO rl = cgApp.GetItemById(id);
            Int32 volta = cgApp.ValidateDelete(rl);
            return RedirectToAction("VoltarAnexoCampanha");
        }

    }
}