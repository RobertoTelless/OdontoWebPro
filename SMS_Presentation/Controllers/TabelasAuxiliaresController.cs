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
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SMS_Presentation.Controllers
{
    public class TabelasAuxiliaresController : Controller
    {
        private readonly IGrupoAppService gruApp;
        private readonly ISubgrupoAppService subApp;

        private String msg;
        private Exception exception;
        GRUPO objetoGru = new GRUPO();
        GRUPO objetoGruAntes = new GRUPO();
        List<GRUPO> listaMasterGru = new List<GRUPO>();
        SUBGRUPO objetoSub = new SUBGRUPO();
        SUBGRUPO objetoSubAntes = new SUBGRUPO();
        List<SUBGRUPO> listaMasterSub = new List<SUBGRUPO>();
        String extensao;

        public TabelasAuxiliaresController(IGrupoAppService gruApps, ISubgrupoAppService subApps)
        {
            gruApp = gruApps;
            subApp = subApps;
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
        public ActionResult MontarTelaGrupo()
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
                    Session["MensGrupo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            if ((List<GRUPO>)Session["ListaGrupo"] == null)
            {
                listaMasterGru = gruApp.GetAllItens(idAss);
                Session["ListaGrupo"] = listaMasterGru;
            }
            ViewBag.Listas = (List<GRUPO>)Session["ListaGrupo"];
            ViewBag.Title = "Grupos";

            // Indicadores
            ViewBag.Grupos = ((List<GRUPO>)Session["ListaGrupo"]).Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensGrupo"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensGrupo"] == 3)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0032", CultureInfo.CurrentCulture));
            }

            // Abre view
            objetoGru = new GRUPO();
            Session["VoltaGrupo"] = 1;
            Session["MensGrupo"] = 0;
            return View(objetoGru);
        }

        public ActionResult MostrarTudoGrupo()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterGru = gruApp.GetAllItensAdm(idAss);
            Session["ListaGrupo"] = listaMasterGru;
            return RedirectToAction("MontarTelaGrupo");
        }

        public ActionResult VoltarBaseGrupo()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaGrupo"] = gruApp.GetAllItens(idAss);
            if ((Int32)Session["VoltaGrupo"] == 2)
            {
                return RedirectToAction("IncluirCC", "CentroCusto");
            }
            if ((Int32)Session["VoltaGrupo"] == 3)
            {
                return RedirectToAction("EditarCC", "CentroCusto");
            }
            return RedirectToAction("MontarTelaGrupo");
        }

        [HttpGet]
        public ActionResult IncluirGrupo(Int32? id)
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
                    Session["MensGrupo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            GRUPO item = new GRUPO();
            if (id != null)
            {
                Session["VoltaGrupo"] = id.Value;
            }
            GrupoViewModel vm = Mapper.Map<GRUPO, GrupoViewModel>(item);
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            vm.GRUP_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirGrupo(GrupoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    GRUPO item = Mapper.Map<GrupoViewModel, GRUPO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    Int32 volta = gruApp.ValidateCreate(item, usuarioLogado);

                    // Sucesso
                    listaMasterGru = new List<GRUPO>();
                    Session["ListaGrupo"] = null;
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
        public ActionResult EditarGrupo(Int32 id)
        {
            GRUPO item = gruApp.GetItemById(id);
            objetoGruAntes = item;
            Session["Grupo"] = item;
            Session["idVolta"] = id;
            GrupoViewModel vm = Mapper.Map<GRUPO, GrupoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarGrupo(GrupoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    GRUPO item = Mapper.Map<GrupoViewModel, GRUPO>(vm);
                    Int32 volta = gruApp.ValidateEdit(item, objetoGruAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterGru = new List<GRUPO>();
                    Session["ListaGrupo"] = null;
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
                    Session["MensGrupo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            GRUPO item = gruApp.GetItemById(id);
            objetoGruAntes = item;
            item.GRUP_IN_ATIVO = 0;
            Int32 volta = gruApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensGrupo"] = 3;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0034", CultureInfo.CurrentCulture));
            }
            listaMasterGru = new List<GRUPO>();
            Session["ListaGrupo"] = null;
            return RedirectToAction("MontarTelaGrupo");
        }

        [HttpGet]
        public ActionResult ReativarGrupo(Int32 id)
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
                    Session["MensGrupo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            GRUPO item = gruApp.GetItemById(id);
            objetoGruAntes = item;
            item.GRUP_IN_ATIVO = 1;
            Int32 volta = gruApp.ValidateReativar(item, usuario);
            listaMasterGru = new List<GRUPO>();
            Session["ListaGrupo"] = null;
            return RedirectToAction("MontarTelaGrupo");
        }

        [HttpGet]
        public ActionResult MontarTelaSubGrupo()
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
                    Session["MensSubGrupo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            if ((List<SUBGRUPO>)Session["ListaSubGrupo"] == null)
            {
                listaMasterSub = subApp.GetAllItens(idAss);
                Session["ListaSubGrupo"] = listaMasterSub;
            }
            ViewBag.Listas = (List<SUBGRUPO>)Session["ListaSubGrupo"];
            ViewBag.Title = "SubGrupos";

            // Indicadores
            ViewBag.SubGrupos = ((List<SUBGRUPO>)Session["ListaSubGrupo"]).Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensSubGrupo"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensSubGrupo"] == 3)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0033", CultureInfo.CurrentCulture));
            }

            // Abre view
            objetoSub = new SUBGRUPO();
            Session["VoltaSubGrupo"] = 1;
            Session["MensSubGrupo"] = 0;
            return View(objetoSub);
        }

        public ActionResult MostrarTudoSubGrupo()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterSub = subApp.GetAllItensAdm(idAss);
            Session["ListaSubGrupo"] = listaMasterSub;
            return RedirectToAction("MontarTelaSubGrupo");
        }

        public ActionResult VoltarBaseSubGrupo()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaSubGrupo"] = gruApp.GetAllItens(idAss);
            if ((Int32)Session["VoltaSubGrupo"] == 2)
            {
                return RedirectToAction("IncluirCC", "CentroCusto");
            }
            if ((Int32)Session["VoltaSubGrupo"] == 3)
            {
                return RedirectToAction("EditarCC", "CentroCusto");
            }
            return RedirectToAction("MontarTelaSubGrupo");
        }

        [HttpGet]
        public ActionResult IncluirSubGrupo(Int32? id)
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
                    Session["MensSubGrupo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            SUBGRUPO item = new SUBGRUPO();
            if (id != null)
            {
                Session["VoltaGrupo"] = id.Value;
            }
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            SubgrupoViewModel vm = Mapper.Map<SUBGRUPO, SubgrupoViewModel>(item);
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            vm.SUBG_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirSubGrupo(SubgrupoViewModel vm)
        {
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    SUBGRUPO item = Mapper.Map<SubgrupoViewModel, SUBGRUPO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    Int32 volta = subApp.ValidateCreate(item, usuarioLogado);

                    // Sucesso
                    listaMasterSub = new List<SUBGRUPO>();
                    Session["ListaSubGrupo"] = null;
                    return RedirectToAction("MontarTelaSubGrupo");
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
        public ActionResult EditarSubGrupo(Int32 id)
        {
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            SUBGRUPO item = subApp.GetItemById(id);
            objetoSubAntes = item;
            Session["SubGrupo"] = item;
            Session["idVolta"] = id;
            SubgrupoViewModel vm = Mapper.Map<SUBGRUPO, SubgrupoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarSubGrupo(SubgrupoViewModel vm)
        {
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    SUBGRUPO item = Mapper.Map<SubgrupoViewModel, SUBGRUPO>(vm);
                    Int32 volta = subApp.ValidateEdit(item, objetoSubAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterSub = new List<SUBGRUPO>();
                    Session["ListaSubGrupo"] = null;
                    return RedirectToAction("MontarTelaSubGrupo");
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
        public ActionResult ExcluirSubGrupo(Int32 id)
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
                    Session["MensSubGrupo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            SUBGRUPO item = subApp.GetItemById(id);
            objetoSubAntes = item;
            item.SUBG_IN_ATIVO = 0;
            Int32 volta = subApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensGrupo"] = 3;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0035", CultureInfo.CurrentCulture));
            }
            listaMasterSub = new List<SUBGRUPO>();
            Session["ListaSubGrupo"] = null;
            return RedirectToAction("MontarTelaSubGrupo");
        }

        [HttpGet]
        public ActionResult ReativarSubGrupo(Int32 id)
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
                    Session["MensSubGrupo"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            SUBGRUPO item = subApp.GetItemById(id);
            objetoSubAntes = item;
            item.SUBG_IN_ATIVO = 1;
            Int32 volta = subApp.ValidateReativar(item, usuario);
            listaMasterSub = new List<SUBGRUPO>();
            Session["ListaSubGrupo"] = null;
            return RedirectToAction("MontarTelaSubGrupo");
        }




    }
}