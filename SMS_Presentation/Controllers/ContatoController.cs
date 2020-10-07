using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
using EntitiesServices.Work_Classes;
using AutoMapper;
using SMS_Presentation.ViewModels;
using System.IO;

namespace SMS_Presentation.Controllers
{
    public class ContatoController : Controller
    {
        private readonly IContatoAppService baseApp;
        private readonly IUsuarioAppService usuApp;

        private String msg;
        private Exception exception;
        CONTATO objetoAss = new CONTATO();
        CONTATO objetoAssAntes = new CONTATO();
        List<CONTATO> listaMasterAss = new List<CONTATO>();
        String extensao;

        public ContatoController(IContatoAppService baseApps, IUsuarioAppService usuApps)
        {
            baseApp = baseApps;
            usuApp = usuApps;
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
        public ActionResult MontarTelaContato()
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
            if ((List<CONTATO>)Session["ListaContato"] == null)
            {
                listaMasterAss = baseApp.GetAllItens(idAss);
                Session["ListaContato"] = listaMasterAss;
            }
            ViewBag.Listas = (List<CONTATO>)Session["ListaContato"];
            ViewBag.Title = "Contatos";

            ViewBag.Origens = new SelectList((List<ORIGEM>)Session["Origens"], "ORIG_CD_ID", "ORIG_NM_NOME");
            ViewBag.Profissoes = new SelectList((List<PROFISSAO>)Session["Profissoes"], "PROF_CD_ID", "PROF_NM_NOME");
            ViewBag.Clubes = new SelectList((List<CLUBE>)Session["Clubes"], "CLUB_CD_ID", "CLUB_NM_NOME");
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Cats = new SelectList((List<CATEGORIA_CONTATO>)Session["CatContatos"], "CACO_CD_ID", "CACO_NM_NOME");

            // Indicadores
            ViewBag.Contatos = ((List<CONTATO>)Session["ListaContato"]).Count;

            // Mensagem
            if ((Int32)Session["MensContato"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensContato"] = 0;
            objetoAss = new CONTATO();
            return View(objetoAss);
        }

        public ActionResult RetirarFiltroContato()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaContato"] = null;
            return RedirectToAction("MontarTelaContato");
        }

        public ActionResult MostrarTudoContato()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterAss = baseApp.GetAllItensAdm(idAss);
            Session["ListaContato"] = listaMasterAss;
            return RedirectToAction("MontarTelaContato");
        }

        [HttpPost]
        public ActionResult FiltrarContato(CONTATO item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                List<CONTATO> listaObj = new List<CONTATO>();
                Int32 idAss = (Int32)Session["IdAssinante"];
                Int32 volta = baseApp.ExecuteFilter(item.CONT_NM_NOME, item.ORIG_CD_ID, item.CACO_CD_ID, item.CONT_NM_CARGO, item.PROF_CD_ID, item.CACO_NM_CIDADE, item.UF_CD_ID, item.CONT_DT_NASCIMENTO, item.CLUB_CD_ID, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensContato"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaContato");
                }

                // Sucesso
                Session["MensContato"] = 0;
                listaMasterAss = listaObj;
                Session["ListaContato"] = listaObj;
                return RedirectToAction("MontarTelaContato");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaContato");
            }
        }

        public ActionResult VoltarBaseContato()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaContato");
        }

       [HttpGet]
        public ActionResult IncluirContato()
        {
            // Prepara listas
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            ViewBag.Origens = new SelectList((List<ORIGEM>)Session["Origens"], "ORIG_CD_ID", "ORIG_NM_NOME");
            ViewBag.Profissoes = new SelectList((List<PROFISSAO>)Session["Profissoes"], "PROF_CD_ID", "PROF_NM_NOME");
            ViewBag.Clubes = new SelectList((List<CLUBE>)Session["Clubes"], "CLUB_CD_ID", "CLUB_NM_NOME");
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Cats = new SelectList((List<CATEGORIA_CONTATO>)Session["CatContatos"], "CACO_CD_ID", "CACO_NM_NOME");
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            CONTATO item = new CONTATO();
            ContatoViewModel vm = Mapper.Map<CONTATO, ContatoViewModel>(item);
            vm.CONT_IN_ATIVO = 1;
            vm.ASSI_CD_ID = idAss;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirContato(ContatoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            ViewBag.Origens = new SelectList((List<ORIGEM>)Session["Origens"], "ORIG_CD_ID", "ORIG_NM_NOME");
            ViewBag.Profissoes = new SelectList((List<PROFISSAO>)Session["Profissoes"], "PROF_CD_ID", "PROF_NM_NOME");
            ViewBag.Clubes = new SelectList((List<CLUBE>)Session["Clubes"], "CLUB_CD_ID", "CLUB_NM_NOME");
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Cats = new SelectList((List<CATEGORIA_CONTATO>)Session["CatContatos"], "CACO_CD_ID", "CACO_NM_NOME");
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CONTATO item = Mapper.Map<ContatoViewModel, CONTATO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensContato"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0025", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterAss = new List<CONTATO>();
                    Session["ListaContato"] = null;
                    Session["VoltaContato"] = 1;
                    Session["IdAssinanteVolta"] = item.ASSI_CD_ID;
                    Session["Contato"] = item;
                    Session["MensContato"] = 0;
                    return RedirectToAction("IncluirContato");
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
        public ActionResult EditarContato(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            ViewBag.Origens = new SelectList((List<ORIGEM>)Session["Origens"], "ORIG_CD_ID", "ORIG_NM_NOME");
            ViewBag.Profissoes = new SelectList((List<PROFISSAO>)Session["Profissoes"], "PROF_CD_ID", "PROF_NM_NOME");
            ViewBag.Clubes = new SelectList((List<CLUBE>)Session["Clubes"], "CLUB_CD_ID", "CLUB_NM_NOME");
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Cats = new SelectList((List<CATEGORIA_CONTATO>)Session["CatContatos"], "CACO_CD_ID", "CACO_NM_NOME");
            Int32 idAss = (Int32)Session["IdAssinante"];

            CONTATO item = baseApp.GetItemById(id);
            objetoAssAntes = item;
            Session["Contato"] = item;
            Session["IdVolta"] = id;
            ContatoViewModel vm = Mapper.Map<CONTATO, ContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarContato(ContatoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            ViewBag.Origens = new SelectList((List<ORIGEM>)Session["Origens"], "ORIG_CD_ID", "ORIG_NM_NOME");
            ViewBag.Profissoes = new SelectList((List<PROFISSAO>)Session["Profissoes"], "PROF_CD_ID", "PROF_NM_NOME");
            ViewBag.Clubes = new SelectList((List<CLUBE>)Session["Clubes"], "CLUB_CD_ID", "CLUB_NM_NOME");
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Cats = new SelectList((List<CATEGORIA_CONTATO>)Session["CatContatos"], "CACO_CD_ID", "CACO_NM_NOME");
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
            {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CONTATO item = Mapper.Map<ContatoViewModel, CONTATO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAssAntes, usuarioLogado, idAss);

                    // Sucesso
                    listaMasterAss = new List<CONTATO>();
                    Session["ListaContato"] = null;
                    Session["MensContato"] = 0;
                    return RedirectToAction("MontarTelaContato");
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
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            CONTATO item = baseApp.GetItemById(id);
            ContatoViewModel vm = Mapper.Map<CONTATO, ContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirContato(ContatoViewModel vm)
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
                CONTATO item = Mapper.Map<ContatoViewModel, CONTATO>(vm);
                Int32 volta = baseApp.ValidateDelete(item, usuarioLogado, idAss);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensContato"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0026", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                
                // Sucesso
                listaMasterAss = new List<CONTATO>();
                Session["ListaContato"] = null;
                Session["MensContato"] = 0;
                return RedirectToAction("MontarTelaContato");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarContato(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            CONTATO item = baseApp.GetItemById(id);
            ContatoViewModel vm = Mapper.Map<CONTATO, ContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarContato(ContatoViewModel vm)
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
                CONTATO item = Mapper.Map<ContatoViewModel, CONTATO>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuarioLogado, idAss);

                // Sucesso
                listaMasterAss = new List<CONTATO>();
                Session["ListaContato"] = null;
                Session["MensContato"] = 0;
                return RedirectToAction("MontarTelaContato");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult VoltarAnexoContato()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 volta = (Int32)Session["IdVolta"];
            return RedirectToAction("EditarContato", new { id = volta });
        }

        [HttpGet]
        public ActionResult MontarTelaAniversariantes()
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
            if ((List<CONTATO>)Session["ListaContato"] == null)
            {
                List<CONTATO> lista = baseApp.GetAllItens(idAss);
                lista = lista.Where(p => p.CONT_DT_NASCIMENTO == DateTime.Today.Date).ToList();
                listaMasterAss = lista;
                Session["ListaContato"] = listaMasterAss;
            }
            ViewBag.Listas = (List<CONTATO>)Session["ListaContato"];
            ViewBag.Title = "Contatos";

            ViewBag.Origens = new SelectList((List<ORIGEM>)Session["Origens"], "ORIG_CD_ID", "ORIG_NM_NOME");
            ViewBag.Profissoes = new SelectList((List<PROFISSAO>)Session["Profissoes"], "PROF_CD_ID", "PROF_NM_NOME");
            ViewBag.Clubes = new SelectList((List<CLUBE>)Session["Clubes"], "CLUB_CD_ID", "CLUB_NM_NOME");
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Cats = new SelectList((List<CATEGORIA_CONTATO>)Session["CatContatos"], "CACO_CD_ID", "CACO_NM_NOME");

            // Indicadores
            ViewBag.Contatos = listaMasterAss.Count;

            // Mensagem
            if ((Int32)Session["MensContato"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensContato"] = 0;
            objetoAss = new CONTATO();
            objetoAss.CONT_DT_NASCIMENTO = DateTime.Today.Date;
            return View(objetoAss);
        }

        public ActionResult RetirarFiltroAniversariante()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaContato"] = null;
            return RedirectToAction("MontarTelaAniversariantes");
        }

        [HttpPost]
        public ActionResult FiltrarAniversariante(CONTATO item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                List<CONTATO> listaObj = new List<CONTATO>();
                Int32 idAss = (Int32)Session["IdAssinante"];
                Int32 volta = baseApp.ExecuteFilter(item.CONT_NM_NOME, item.ORIG_CD_ID, item.CACO_CD_ID, item.CONT_NM_CARGO, item.PROF_CD_ID, item.CACO_NM_CIDADE, item.UF_CD_ID, item.CONT_DT_NASCIMENTO, item.CLUB_CD_ID, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensContato"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaAniversariantes");
                }

                // Sucesso
                Session["MensContato"] = 0;
                listaMasterAss = listaObj;
                Session["ListaContato"] = listaObj;
                return RedirectToAction("MontarTelaAniversariantes");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaAniversariantes");
            }
        }

    }
}