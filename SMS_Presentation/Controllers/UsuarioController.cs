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
using System.Data.Entity.Migrations.Model;

namespace Odonto.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioAppService baseApp;
        private readonly ILogAppService logApp;
        private String msg;
        private Exception exception;
        USUARIO objeto = new USUARIO();
        USUARIO objetoAntes = new USUARIO();
        List<USUARIO> listaMaster = new List<USUARIO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public UsuarioController(IUsuarioAppService baseApps, ILogAppService logApps)
        {
            baseApp = baseApps;
            logApp = logApps;
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

        [HttpGet]
        public ActionResult MontarTelaPerfilUsuario()
        {
            // Carrega listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            usuario = (USUARIO)Session["UserCredentials"];
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            ViewBag.UFs = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
            ViewBag.Title = "Usuário";

            // Abre view
            USUARIO usu = baseApp.GetItemById(usuario.USUA_CD_ID);
            objetoAntes = usu;
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(usu);
            return View(vm);
        }

        [HttpPost]
        public ActionResult MontarTelaPerfilUsuario(UsuarioViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            ViewBag.UFs = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    USUARIO item = Mapper.Map<UsuarioViewModel, USUARIO>(vm);
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    Session["UserCredentials"] = item;
                    return RedirectToAction("MontarTelaPerfilUsuario");
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

        public ActionResult Voltar()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarBaseUsuario()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["Usuarios"] = baseApp.GetAllUsuarios(idAss);
            return RedirectToAction("MontarTelaPerfilUsuario");
        }

        [HttpGet]
        public ActionResult SolicitarAlteracao()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["Usuarios"] = baseApp.GetAllUsuarios(idAss);
            NOTIFICACAO noti = new NOTIFICACAO();
            noti.ASSI_CD_ID = idAss;
            return View(noti);
        }

        [HttpPost]
        public ActionResult SolicitarAlteracao(NOTIFICACAO noti)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["Usuarios"] = baseApp.GetAllUsuarios(idAss);

            // Executa a operação
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
            Int32 volta = baseApp.CreateNotificacao(noti, usuarioLogado);

            // Sucesso
            return RedirectToAction("MontarTelaPerfilUsuario");
        }

        [HttpPost]
        public ActionResult UploadFotoUsuario(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoUsuario");
            }

            // Recupera arquivo
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO item = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                return RedirectToAction("MontarTelaPerfilUsuario");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/Usuarios/" + item.USUA_CD_ID.ToString() + "/Fotos/";
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
                item.USUA_AQ_FOTO = "~" + caminho + fileName;
                objetoAntes = item;
                Int32 volta = baseApp.ValidateEdit(item, item, item);
            }
            else
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0021", CultureInfo.CurrentCulture));
            }
            return RedirectToAction("MontarTelaPerfilUsuario");
        }
    }
}