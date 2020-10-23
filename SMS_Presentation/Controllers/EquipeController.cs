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

namespace Odonto.Controllers
{
    public class EquipeController : Controller
    {
        private readonly IUsuarioAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly ICargoAppService carApp;
        private readonly IFilialAppService filApp;

        private String msg;
        private Exception exception;
        USUARIO objeto = new USUARIO();
        USUARIO objetoAntes = new USUARIO();
        List<USUARIO> listaMaster = new List<USUARIO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public EquipeController(IUsuarioAppService baseApps, ILogAppService logApps, ICargoAppService carApps, IFilialAppService filApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            carApp = carApps;
            filApp = filApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            USUARIO item = new USUARIO();
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            return View(vm);
        }

        public ActionResult Voltar()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaUsuarios"] = baseApp.GetAllUsuarios(idAss);
            return RedirectToAction("MontarTelaUsuario");
        }

        [HttpGet]
        public ActionResult MontarTelaEquipeLista()
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
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            if ((List<USUARIO>)Session["ListaUsuario"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                listaMaster = listaMaster.Where(p => p.USUA_DT_DEMISSAO == null).ToList();
                Session["ListaUsuario"] = listaMaster;
            }
            List<USUARIO> lista = (List<USUARIO>)Session["ListaUsuario"];
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            ViewBag.Listas = lista;
            ViewBag.Usuarios = lista.Count;
            ViewBag.Title = "Usuários";
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAUS_CD_ID", "CAUS_NM_NOME");
            ViewBag.Cargos = new SelectList(carApp.GetAllItens(idAss), "CARG_CD_ID", "CARG_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Situacoes = new SelectList(baseApp.GetAllSituacao(idAss), "SITU_CD_ID", "SITU_NM_NOME");

            // Recupera numero de usuarios do assinante
            Session["NumUsuarios"] = baseApp.GetAllUsuarios(idAss).Count;

            // Mensagem
            if ((Int32)Session["MensUsuario"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensUsuario"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["FiltroUsuario"] = null;
            Session["ModoUsuario"] = 0;
            ViewBag.Modo = 0;
            Session["MensUsuario"] = 0;
            Session["VoltaUsuario"] = 1;
            objeto = new USUARIO();
            return View(objeto);
        }

        [HttpGet]
        public ActionResult MontarTelaEquipeCards()
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
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            if ((List<USUARIO>)Session["ListaUsuario"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                listaMaster = listaMaster.Where(p => p.USUA_DT_DEMISSAO == null).ToList();
                Session["ListaUsuario"] = listaMaster;
            }
            List<USUARIO> lista = (List<USUARIO>)Session["ListaUsuario"];
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            ViewBag.Listas = lista;
            ViewBag.Usuarios = lista.Count;
            ViewBag.Title = "Usuários";
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAUS_CD_ID", "CAUS_NM_NOME");
            ViewBag.Cargos = new SelectList(carApp.GetAllItens(idAss), "CARG_CD_ID", "CARG_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Situacoes = new SelectList(baseApp.GetAllSituacao(idAss), "SITU_CD_ID", "SITU_NM_NOME");

            // Recupera numero de usuarios do assinante
            Session["NumUsuarios"] = baseApp.GetAllUsuarios(idAss).Count;

            // Mensagem
            if ((Int32)Session["MensUsuario"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensUsuario"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["FiltroUsuario"] = null;
            Session["ModoUsuario"] = 0;
            ViewBag.Modo = 0;
            Session["MensUsuario"] = 0;
            Session["VoltaUsuario"] = 2;
            objeto = new USUARIO();
            return View(objeto);
        }

        public ActionResult RetirarFiltro()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaUsuario"] = null;
            Session["FiltroUsuario"] = null;
            if ((Int32)Session["VoltaUsuario"] == 2)
            {
                return RedirectToAction("MontarTelaUsuarioCards");
            }
            return RedirectToAction("MontarTelaUsuarioLista");
        }

        public ActionResult MostrarTudo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllUsuariosAdm(idAss);
            Session["ListaUsuario"] = listaMaster;
            Session["FiltroUsuario"] = null;
            if ((Int32)Session["VoltaUsuario"] == 2)
            {
                return RedirectToAction("MontarTelaUsuarioCards");
            }
            return RedirectToAction("MontarTelaUsuarioLista");
        }

        [HttpPost]
        public ActionResult FiltrarUsuario(USUARIO item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<USUARIO> listaObj = new List<USUARIO>();
                Session["FiltroUsuario"] = item;
                Int32 volta = baseApp.ExecuteFilter(item.CAUS_CD_ID, item.CARG_CD_ID, item.FILI_CD_ID, item.USUA_NM_NOME, item.USUA_NM_LOGIN, item.USUA_NM_EMAIL, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensUsuario"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaUsuario"] = listaObj;
                if ((Int32)Session["VoltaUsuario"] == 2)
                {
                    return RedirectToAction("MontarTelaUsuarioCards");
                }
                return RedirectToAction("MontarTelaUsuarioLista");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                if ((Int32)Session["VoltaUsuario"] == 2)
                {
                    return RedirectToAction("MontarTelaUsuarioCards");
                }
                return RedirectToAction("MontarTelaUsuarioLista");
            }
        }

        [HttpGet]
        public ActionResult VerUsuario(Int32 id)
        {
            // Prepara view
            // Executa a operação
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = item;
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            return View(vm);
        }


    }
}