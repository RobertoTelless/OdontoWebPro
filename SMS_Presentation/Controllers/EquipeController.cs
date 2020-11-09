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
            if ((Int32)Session["VoltaUsuario"] == 2)
            {
                return RedirectToAction("MontarTelaEquipeCards");
            }
            return RedirectToAction("MontarTelaEquipeLista");
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
                listaMaster = listaMaster.Where(p => p.USUA_DT_DEMISSAO == null & p.USUA_IN_CATEGORIA == 1).ToList();
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
            if ((Int32)Session["MensEquipe"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensEquipe"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["FiltroUsuario"] = null;
            Session["ModoUsuario"] = 0;
            ViewBag.Modo = 0;
            Session["MensEquipe"] = 0;
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
                listaMaster = listaMaster.Where(p => p.USUA_DT_DEMISSAO == null & p.USUA_IN_CATEGORIA == 1).ToList();
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
            if ((Int32)Session["MensEquipe"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensEquipe"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["FiltroUsuario"] = null;
            Session["ModoUsuario"] = 0;
            ViewBag.Modo = 0;
            Session["MensEquipe"] = 0;
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
                return RedirectToAction("MontarTelaEquipeCards");
            }
            return RedirectToAction("MontarTelaEquipeLista");
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
                return RedirectToAction("MontarTelaEquipeCards");
            }
            return RedirectToAction("MontarTelaEquipeLista");
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
                    Session["MensEquipe"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaUsuario"] = listaObj;
                if ((Int32)Session["VoltaUsuario"] == 2)
                {
                    return RedirectToAction("MontarTelaEquipeCards");
                }
                return RedirectToAction("MontarTelaEquipeLista");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                if ((Int32)Session["VoltaUsuario"] == 2)
                {
                    return RedirectToAction("MontarTelaEquipeCards");
                }
                return RedirectToAction("MontarTelaEquipeLista");
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

        [HttpGet]
        public ActionResult IncluirEquipe()
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
                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensEquipe"] = 2;
                    return RedirectToAction("MontarTelaEquipeLista", "Equipe");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara listas
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAUS_CD_ID", "CAUS_NM_NOME");
            ViewBag.Cargos = new SelectList(carApp.GetAllItens(idAss), "CARG_CD_ID", "CARG_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Situacoes = new SelectList(baseApp.GetAllSituacao(idAss), "SITU_CD_ID", "SITU_NM_NOME");

            // Prepara view
            USUARIO item = new USUARIO();
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            vm.USUA_DT_CADASTRO = DateTime.Today;
            vm.USUA_IN_ATIVO = 1;
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirEquipe(UsuarioViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAUS_CD_ID", "CAUS_NM_NOME");
            ViewBag.Cargos = new SelectList(carApp.GetAllItens(idAss), "CARG_CD_ID", "CARG_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Situacoes = new SelectList(baseApp.GetAllSituacao(idAss), "SITU_CD_ID", "SITU_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO item = Mapper.Map<UsuarioViewModel, USUARIO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0012", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0013", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 3)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0022", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 4 )
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0023", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 5)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 6)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0028", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 7)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0029", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Carrega foto e processa alteracao
                    item.USUA_AQ_FOTO = "~/Imagens/Base/FotoBase.jpg";
                    volta = baseApp.ValidateEdit(item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + idAss.ToString() + "/Usuarios/" + item.USUA_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + idAss.ToString() + "/Usuarios/" + item.USUA_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + idAss.ToString() + "/Usuarios/" + item.USUA_CD_ID.ToString() + "/CC/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMaster = new List<USUARIO>();
                    Session["ListaUsuario"] = null;
                    Session["VoltaUsuario"] = 1;
                    Session["IdUsuarioVolta"] = item.USUA_CD_ID;
                    Session["Usuario"] = item;
                    Session["MensEquipe"] = 0;
                    return RedirectToAction("EditarEquipe", new { id = item.USUA_CD_ID });
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
        public ActionResult EditarEquipe(Int32 id)
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
                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensEquipe"] = 2;
                    return RedirectToAction("MontarTelaEquipeLista", "Equipe");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAUS_CD_ID", "CAUS_NM_NOME");
            ViewBag.Cargos = new SelectList(carApp.GetAllItens(idAss), "CARG_CD_ID", "CARG_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Situacoes = new SelectList(baseApp.GetAllSituacao(idAss), "SITU_CD_ID", "SITU_NM_NOME");

            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Usuario"] = item;
            Session["IdUsuario"] = id;
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarEquipe(UsuarioViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            ViewBag.Perfis = new SelectList((List<PERFIL>)Session["Perfis"], "PERF_CD_ID", "PERF_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAUS_CD_ID", "CAUS_NM_NOME");
            ViewBag.Cargos = new SelectList(carApp.GetAllItens(idAss), "CARG_CD_ID", "CARG_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Situacoes = new SelectList(baseApp.GetAllSituacao(idAss), "SITU_CD_ID", "SITU_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    USUARIO item = Mapper.Map<UsuarioViewModel, USUARIO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0013", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0022", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 3)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0023", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 4)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 5)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0028", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 6)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0029", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMaster = new List<USUARIO>();
                    Session["ListaUsuario"] = null;
                    Session["MensEquipe"] = 0;
                    return RedirectToAction("MontarTelaEquipeLista");
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
        public ActionResult DesativarEquipe(Int32 id)
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
                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("MontarTelaEquipeLista", "Equipe");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = (USUARIO)Session["Usuario"];
            item.USUA_IN_ATIVO = 0;
            item.USUA_DT_ALTERACAO = DateTime.Today;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            listaMaster = new List<USUARIO>();
            Session["ListaUsuario"] = null;
            return RedirectToAction("MontarTelaEquipeLista");
        }

        [HttpGet]
        public ActionResult ReativarEquipe(Int32 id)
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
                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensUsuario"] = 2;
                    return RedirectToAction("MontarTelaEquipeLista", "Equipe");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            USUARIO item = baseApp.GetItemById(id);
            objetoAntes = (USUARIO)Session["Usuario"];
            item.USUA_IN_ATIVO = 1;
            item.USUA_DT_ALTERACAO = DateTime.Today;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            listaMaster = new List<USUARIO>();
            Session["ListaUsuario"] = null;
            return RedirectToAction("MontarTelaEquipeLista");
        }

        [HttpGet]
        public ActionResult VerAnexoEquipe(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoEquipe()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idUsu = (Int32)Session["IdUsuario"];
            return RedirectToAction("EditarEquipe", new { id = idUsu });
        }

        public FileResult DownloadEquipe(Int32 id)
        {
            USUARIO_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.USAN_AQ_ARQUIVO;
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

        [HttpPost]
        public ActionResult UploadFileEquipe(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoEquipe");
            }

            Int32 idUsu = (Int32)Session["IdUsuario"];
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO item = baseApp.GetById(idUsu);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);

            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoEquipe");
            }

            String caminho = "/Imagens/" + idAss.ToString() + "/Usuarios/" + item.USUA_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            USUARIO_ANEXO foto = new USUARIO_ANEXO();
            foto.USAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.USAN_DT_ANEXO = DateTime.Today;
            foto.USAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.USAN_IN_TIPO = tipo;
            foto.USAN_NM_TITULO = fileName;
            foto.USUA_CD_ID = item.USUA_CD_ID;

            item.USUARIO_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoEquipe");
        }

        [HttpPost]
        public ActionResult UploadFotoEquipe(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoEquipe");
            }

            // Recupera arquivo
            Int32 idUsu = (Int32)Session["IdUsuario"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            USUARIO item = baseApp.GetById(idUsu);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoEquipe");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Usuarios/" + item.USUA_CD_ID.ToString() + "/Fotos/";
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
                Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            }
            else
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0021", CultureInfo.CurrentCulture));
            }
            return RedirectToAction("VoltarAnexoEquipe");
        }

        [HttpGet]
        public ActionResult SlideShowEquipe()
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idUsu = (Int32)Session["IdUsuario"];
            USUARIO item = baseApp.GetItemById(idUsu);
            objetoAntes = item;
            UsuarioViewModel vm = Mapper.Map<USUARIO, UsuarioViewModel>(item);
            return View(vm);
        }

    }
}