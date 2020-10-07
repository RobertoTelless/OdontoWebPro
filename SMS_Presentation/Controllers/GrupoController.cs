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
    public class GrupoController : Controller
    {
        private readonly IGrupoAppService baseApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IContatoAppService conApp;
        private readonly IGrupoContatoAppService gcApp;

        private String msg;
        private Exception exception;
        GRUPO objetoAss = new GRUPO();
        GRUPO objetoAssAntes = new GRUPO();
        List<GRUPO> listaMasterAss = new List<GRUPO>();
        String extensao;

        public GrupoController(IGrupoAppService baseApps, IUsuarioAppService usuApps, IContatoAppService conApps, IGrupoContatoAppService gcApps)
        {
            baseApp = baseApps;
            usuApp = usuApps;
            conApp = conApps;
            gcApp = gcApps;
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
        public ActionResult MontarTelaGrupo()
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
            if ((List<GRUPO>)Session["ListaGrupo"] == null)
            {
                listaMasterAss = baseApp.GetAllItens(idAss);
                Session["ListaGrupo"] = listaMasterAss;
            }
            ViewBag.Listas = (List<GRUPO>)Session["ListaGrupo"];
            ViewBag.Title = "Grupos";

            ViewBag.Contatos = new SelectList(conApp.GetAllItens(idAss), "CONT_CD_ID", "CONT_NM_NOME");

            // Indicadores
            ViewBag.Grupos = ((List<GRUPO>)Session["ListaGrupo"]).Count;

            // Mensagem
            if ((Int32)Session["MensGrupo"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensGrupo"] == 2)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0029", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensGrupo"] = 0;
            objetoAss = new GRUPO();
            return View(objetoAss);
        }

        public ActionResult RetirarFiltroGrupo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaGrupo"] = null;
            return RedirectToAction("MontarTelaGrupo");
        }

        public ActionResult MostrarTudoGrupo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterAss = baseApp.GetAllItensAdm(idAss);
            Session["ListaGrupo"] = listaMasterAss;
            return RedirectToAction("MontarTelaGrupo");
        }

        [HttpPost]
        public ActionResult FiltrarGrupo(GRUPO item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                List<GRUPO> listaObj = new List<GRUPO>();
                Int32 idAss = (Int32)Session["IdAssinante"];
                Int32 volta = baseApp.ExecuteFilter(item.GRUP_NM_NOME, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensGrupo"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaGrupo");
                }

                // Sucesso
                Session["MensGrupo"] = 0;
                listaMasterAss = listaObj;
                Session["ListaGrupo"] = listaObj;
                return RedirectToAction("MontarTelaGrupo");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaGrupo");
            }
        }

        public ActionResult VoltarBaseGrupo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaGrupo");
        }

        [HttpGet]
        public ActionResult IncluirGrupo()
        {
            // Prepara listas
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            GRUPO item = new GRUPO();
            GrupoViewModel vm = Mapper.Map<GRUPO, GrupoViewModel>(item);
            vm.GRUP_IN_ATIVO = 1;
            vm.ASSI_CD_ID = idAss;
            vm.GRUP_DT_CADASTRO = DateTime.Today.Date;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirGrupo(GrupoViewModel vm)
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
                    GRUPO item = Mapper.Map<GrupoViewModel, GRUPO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensGrupo"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0028", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterAss = new List<GRUPO>();
                    Session["ListaGrupo"] = null;
                    Session["VoltaGrupo"] = 1;
                    Session["IdAssinanteVolta"] = item.ASSI_CD_ID;
                    Session["Grupo"] = item;
                    Session["MensGrupo"] = 0;
                    Session["IdVolta"] = item.GRUP_CD_ID;
                    return RedirectToAction("EditarGrupo", new { id = item.GRUP_CD_ID });
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
        public ActionResult EditarGrupo(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Contatos = new SelectList(conApp.GetAllItens(idAss), "CONT_CD_ID", "CONT_NM_NOME");

            if ((Int32)Session["MensGrupo"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0030", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensGrupo"] == 2)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0046", CultureInfo.CurrentCulture));
            }

            GRUPO item = baseApp.GetItemById(id);
            objetoAssAntes = item;
            Session["Grupo"] = item;
            Session["IdVolta"] = id;
            Session["MensGrupo"] = 0;
            GrupoViewModel vm = Mapper.Map<GRUPO, GrupoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarGrupo(GrupoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Contatos = new SelectList(conApp.GetAllItens(idAss), "CONT_CD_ID", "CONT_NM_NOME");
            if (ModelState.IsValid)
            {
                try
            {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    GRUPO item = Mapper.Map<GrupoViewModel, GRUPO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAssAntes, usuarioLogado, idAss);

                    // Sucesso
                    listaMasterAss = new List<GRUPO>();
                    Session["ListaGrupo"] = null;
                    Session["MensGrupo"] = 0;
                    return RedirectToAction("MontarTelaGrupo");
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
        public ActionResult ExcluirGrupo(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            GRUPO item = baseApp.GetItemById(id);
            GrupoViewModel vm = Mapper.Map<GRUPO, GrupoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirGrupo(GrupoViewModel vm)
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
                GRUPO item = Mapper.Map<GrupoViewModel, GRUPO>(vm);
                Int32 volta = baseApp.ValidateDelete(item, usuarioLogado, idAss);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensGrupo"] = 2;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0029", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                
                // Sucesso
                listaMasterAss = new List<GRUPO>();
                Session["ListaGrupo"] = null;
                Session["MensGrupo"] = 0;
                return RedirectToAction("MontarTelaGrupo");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarGrupo(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            GRUPO item = baseApp.GetItemById(id);
            GrupoViewModel vm = Mapper.Map<GRUPO, GrupoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarGrupo(GrupoViewModel vm)
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
                GRUPO item = Mapper.Map<GrupoViewModel, GRUPO>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuarioLogado, idAss);

                // Sucesso
                listaMasterAss = new List<GRUPO>();
                Session["ListaGrupo"] = null;
                Session["MensGrupo"] = 0;
                return RedirectToAction("MontarTelaGrupo");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult VoltarAnexoGrupo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 volta = (Int32)Session["IdVolta"];
            return RedirectToAction("EditarGrupo", new { id = volta });
        }

        [HttpPost]
        public ActionResult IncluirGrupoContato(GrupoViewModel vm)
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
                GRUPO item = Mapper.Map<GrupoViewModel, GRUPO>(vm);

                // verifica quantidade de contatos no grupo
                if (item.GRUPO_CONTATO.Count >= 200)
                {
                    Session["MensGrupo"] = 2;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0046", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoGrupo");
                }

                Int32 volta = baseApp.IncluirGrupoContato(item, usuarioLogado, idAss);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensGrupo"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0030", CultureInfo.CurrentCulture));
                    return RedirectToAction("VoltarAnexoGrupo");
                }

                // Sucesso
                return RedirectToAction("VoltarAnexoGrupo");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("VoltarAnexoGrupo");
            }
        }

        [HttpGet]
        public ActionResult ReativarGrupoContato(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
            GRUPO_CONTATO item = gcApp.GetItemById(id);
            item.GRCO_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditGrupoContato(item);
            return RedirectToAction("VoltarAnexoGrupo");
        }

        [HttpGet]
        public ActionResult ExcluirGrupoContato(Int32 id)
        {
            // Verifica se tem usuario logado
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];

            GRUPO rot = (GRUPO)Session["Grupo"];
            GRUPO_CONTATO rl = gcApp.GetItemById(id);
            Int32 volta = gcApp.ValidateDelete(rl);
            return RedirectToAction("VoltarAnexoGrupo");
        }
    }
}