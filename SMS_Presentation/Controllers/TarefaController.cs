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
using System.Collections;

namespace OdontoWeb.Controllers
{
    public class TarefaController : Controller
    {
        private readonly ITarefaAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IUsuarioAppService usuApp;
        private String msg;
        private Exception exception;
        TAREFA objeto = new TAREFA();
        TAREFA objetoAntes = new TAREFA();
        List<TAREFA> listaMaster = new List<TAREFA>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public TarefaController(ITarefaAppService baseApps, ILogAppService logApps, IUsuarioAppService usuApps)
        {
            baseApp = baseApps;
            logApp = logApps;
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

        [HttpPost]
        public JsonResult GetTarefaNaoExecutada()
        {
            var usu = (USUARIO)Session["UserCredentials"];
            listaMaster = baseApp.GetByUser(usu.USUA_CD_ID).Where(x => x.TARE_DT_REALIZADA == null).ToList();

            if (listaMaster.Count == 1)
            {
                var hash = new Hashtable();
                hash.Add("msg", "Você possui 1 tarefa não executada");
                return Json(hash);
            } 
            else if (listaMaster.Count > 1) 
            {
                var hash = new Hashtable();
                hash.Add("msg", "Você possui " + listaMaster.Count + " tarefas não executadas");

                return Json(hash);
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public ActionResult TarefaNaoRealizadaClick()
        {
            var usu = (USUARIO)Session["UserCredentials"];

            listaMaster = baseApp.GetByUser(usu.USUA_CD_ID).Where(x => x.TARE_DT_REALIZADA == null).ToList();

            if (listaMaster.Count == 1)
            {
                return Json(listaMaster.FirstOrDefault().TARE_CD_ID);
            }
            else
            {
                return Json(0);
            }
        }

        [HttpGet]
        public ActionResult MontarTelaTarefaKanban(Int32? id)
        {
            // Controle Acesso
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
            if (Session["ListaTarefa"] == null)
            {
                listaMaster = baseApp.GetByUser(usuario.USUA_CD_ID);
                Session["ListaTarefa"] = listaMaster;
            }

            if (id == null)
            {
                ViewBag.Listas = listaMaster;
            }
            else
            {
                ViewBag.Listas = baseApp.GetByUser(usuario.USUA_CD_ID).Where(x => x.TARE_DT_REALIZADA == null).ToList();
            }

            ViewBag.Title = "Tarefas";

            // Indicadores
            ViewBag.Tarefas = listaMaster.Count;
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "TITR_CD_ID", "TITR_NM_NOME");
            ViewBag.TarefasPendentes = baseApp.GetTarefaStatus(usuario.USUA_CD_ID, 1).Count;
            ViewBag.TarefasEncerradas = baseApp.GetTarefaStatus(usuario.USUA_CD_ID, 2).Count;

            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Pendente", Value = "1" });
            status.Add(new SelectListItem() { Text = "Suspensa", Value = "2" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "4" });
            ViewBag.Status = new SelectList(status, "Value", "Text");

            // Mensagem
            if ((Int32)Session["MensTarefa"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensTarefa"] = 0;
            objeto = new TAREFA();
            objeto.TARE_DT_CADASTRO = DateTime.Today.Date;
            return View(objeto);
        }

        [HttpPost]
        public JsonResult GetTarefas()
        {
            var usuario = (USUARIO)Session["UserCredentials"];
            listaMaster = baseApp.GetByUser(usuario.USUA_CD_ID);

            var listaHash = new List<Hashtable>();

            foreach (var item in listaMaster)
            {
                var hash = new Hashtable();
                hash.Add("TARE_IN_STATUS", item.TARE_IN_STATUS);
                hash.Add("TARE_CD_ID", item.TARE_CD_ID);
                hash.Add("TARE_NM_TITULO", item.TARE_NM_TITULO);
                hash.Add("TARE_DT_ESTIMADA", item.TARE_DT_ESTIMADA.ToString());

                listaHash.Add(hash);
            }

            return Json(listaHash);
        }

        [HttpGet]
        public ActionResult MontarTelaTarefa(Int32? id)
        {
            // Controle Acesso
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
            if (Session["ListaTarefa"] == null)
            {
                listaMaster = baseApp.GetByUser(usuario.USUA_CD_ID);
                Session["ListaTarefa"] = listaMaster;
            }

            if (id == null)
            {
                ViewBag.Listas = listaMaster;
            }
            else
            {
                ViewBag.Listas = baseApp.GetByUser(usuario.USUA_CD_ID).Where(x => x.TARE_DT_REALIZADA == null).ToList();
            }

            ViewBag.Title = "Tarefas";

            // Indicadores
            ViewBag.Tarefas = listaMaster.Count;
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "TITR_CD_ID", "TITR_NM_NOME");
            ViewBag.TarefasPendentes = baseApp.GetTarefaStatus(usuario.USUA_CD_ID, 1).Count;
            ViewBag.TarefasEncerradas = baseApp.GetTarefaStatus(usuario.USUA_CD_ID, 2).Count; 

            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Pendente", Value = "1" });
            status.Add(new SelectListItem() { Text = "Suspensa", Value = "2" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "4" });
            ViewBag.Status = new SelectList(status, "Value", "Text");

            // Mensagem
            if ((Int32)Session["MensTarefa"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensTarefa"] = 0;
            objeto = new TAREFA();
            objeto.TARE_DT_CADASTRO = DateTime.Today.Date;
            return View(objeto);
        }

        public ActionResult RetirarFiltroTarefa()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaTarefa"] = null;
            return RedirectToAction("MontarTelaTarefa");
        }

        public ActionResult MostrarTudoTarefa()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["ListaTarefa"] = listaMaster;
            return RedirectToAction("MontarTelaTarefa");
        }

        [HttpPost]
        public ActionResult FiltrarTarefa(TAREFA item)
        {
            try
            {
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                // Executa a operação
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<TAREFA> listaObj = new List<TAREFA>();
                Session["FiltroTarefa"] = item;
                Int32 volta = baseApp.ExecuteFilter(item.TITR_CD_ID, item.TARE_NM_TITULO, item.TARE_DT_CADASTRO, item.TARE_IN_STATUS, item.TARE_IN_PRIORIDADE, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensTarefa"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaTarefa");
                }

                // Sucesso
                Session["MensTarefa"] = 0;
                listaMaster = listaObj;
                Session["ListaTarefa"] = listaObj;
                return RedirectToAction("MontarTelaTarefa");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaTarefa");

            }
        }

        public ActionResult VoltarBaseTarefa()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaTarefa");
        }

        [HttpGet]
        public ActionResult IncluirTarefa()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            // Prepara listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "TITR_CD_ID", "TITR_NM_NOME");
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Pendente", Value = "1" });
            status.Add(new SelectListItem() { Text = "Suspensa", Value = "2" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "4" });
            ViewBag.Status = new SelectList(status, "Value", "Text");
            List<SelectListItem> prior = new List<SelectListItem>();
            prior.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            prior.Add(new SelectListItem() { Text = "Baixa", Value = "2" });
            prior.Add(new SelectListItem() { Text = "Alta", Value = "3" });
            prior.Add(new SelectListItem() { Text = "Urgente", Value = "4" });
            ViewBag.Prioridade = new SelectList(prior, "Value", "Text");

            // Prepara view
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            TAREFA item = new TAREFA();
            TarefaViewModel vm = Mapper.Map<TAREFA, TarefaViewModel>(item);
            vm.USUA_CD_ID = usuario.USUA_CD_ID;
            vm.TARE_IN_ATIVO = 1;
            vm.TARE_DT_CADASTRO = DateTime.Today.Date;
            vm.TARE_DT_ESTIMADA = DateTime.Today.Date.AddDays(5);
            vm.TARE_IN_PRIORIDADE = 1;
            vm.TARE_IN_AVISA = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirTarefa(TarefaViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "TITR_CD_ID", "TITR_NM_NOME");
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Pendente", Value = "1" });
            status.Add(new SelectListItem() { Text = "Suspensa", Value = "2" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "4" });
            ViewBag.Status = new SelectList(status, "Value", "Text");
            List<SelectListItem> prior = new List<SelectListItem>();
            prior.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            prior.Add(new SelectListItem() { Text = "Baixa", Value = "2" });
            prior.Add(new SelectListItem() { Text = "Alta", Value = "3" });
            prior.Add(new SelectListItem() { Text = "Urgente", Value = "4" });
            ViewBag.Prioridade = new SelectList(prior, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    TAREFA item = Mapper.Map<TarefaViewModel, TAREFA>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0017", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0018", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Cria pastas
                    String caminho = "/Imagens/" + idAss.ToString() + "/Tarefas/" + item.TARE_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMaster = new List<TAREFA>();
                    Session["ListaTarefa"] = null;
                    Session["IdVolta"] = item.TARE_CD_ID;
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
        public ActionResult EditarTarefa(Int32 id)
        {
            // Prepara view
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "TITR_CD_ID", "TITR_NM_NOME");
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Pendente", Value = "1" });
            status.Add(new SelectListItem() { Text = "Suspensa", Value = "2" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "4" });
            ViewBag.StatusX = new SelectList(status, "Value", "Text");
            List<SelectListItem> prior = new List<SelectListItem>();
            prior.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            prior.Add(new SelectListItem() { Text = "Baixa", Value = "2" });
            prior.Add(new SelectListItem() { Text = "Alta", Value = "3" });
            prior.Add(new SelectListItem() { Text = "Urgente", Value = "4" });
            ViewBag.Prioridade = new SelectList(prior, "Value", "Text");

            TAREFA item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Tarefa"] = item;
            Session["IdVolta"] = id;
            ViewBag.Status = (item.TARE_IN_STATUS == 1 ? "Pendente" : (item.TARE_IN_STATUS == 2 ? "Suspensa" : (item.TARE_IN_STATUS == 3 ? "Cancelada" : "Encerrada")));
            ViewBag.Prior = (item.TARE_IN_PRIORIDADE == 1 ? "Normal" : (item.TARE_IN_PRIORIDADE == 2 ? "Baixa" : (item.TARE_IN_PRIORIDADE == 3 ? "Alta" : "Urgente")));
            TarefaViewModel vm = Mapper.Map<TAREFA, TarefaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarTarefa(TarefaViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "TITR_CD_ID", "TITR_NM_NOME");
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Pendente", Value = "1" });
            status.Add(new SelectListItem() { Text = "Suspensa", Value = "2" });
            status.Add(new SelectListItem() { Text = "Cancelada", Value = "3" });
            status.Add(new SelectListItem() { Text = "Encerrada", Value = "4" });
            ViewBag.StatusX = new SelectList(status, "Value", "Text");
            List<SelectListItem> prior = new List<SelectListItem>();
            prior.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            prior.Add(new SelectListItem() { Text = "Baixa", Value = "2" });
            prior.Add(new SelectListItem() { Text = "Alta", Value = "3" });
            prior.Add(new SelectListItem() { Text = "Urgente", Value = "4" });
            ViewBag.Prioridade = new SelectList(prior, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    TAREFA item = Mapper.Map<TarefaViewModel, TAREFA>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0020", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == 2)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0025", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMaster = new List<TAREFA>();
                    Session["ListaTarefa"] = null;
                    return RedirectToAction("MontarTelaTarefa");
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

        [HttpPost]
        public JsonResult EditarStatusTarefa(Int32 id, Int32 status)
        {
            var tarefa = baseApp.GetById(id);
            tarefa.TARE_IN_STATUS = status;

            var item = new TAREFA();
            item.TARE_CD_ID = tarefa.TARE_CD_ID;
            item.TARE_DS_DESCRICAO = tarefa.TARE_DS_DESCRICAO;
            item.TARE_DT_CADASTRO = tarefa.TARE_DT_CADASTRO;
            item.TARE_DT_ESTIMADA = tarefa.TARE_DT_ESTIMADA;
            item.TARE_DT_REALIZADA = tarefa.TARE_DT_REALIZADA;
            item.TARE_IN_ATIVO = tarefa.TARE_IN_ATIVO;
            item.TARE_IN_AVISA = tarefa.TARE_IN_AVISA;
            item.TARE_IN_PRIORIDADE = tarefa.TARE_IN_PRIORIDADE;
            item.TARE_IN_STATUS = tarefa.TARE_IN_STATUS;
            item.TARE_NM_LOCAL = tarefa.TARE_NM_LOCAL;
            item.TARE_NM_TITULO = tarefa.TARE_NM_TITULO;
            item.TARE_TX_OBSERVACOES = tarefa.TARE_TX_OBSERVACOES;
            item.TITR_CD_ID = tarefa.TITR_CD_ID;
            item.USUA_CD_ID = tarefa.USUA_CD_ID;

            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    return Json(OdontoWeb_Resources.ResourceManager.GetString("M0020", CultureInfo.CurrentCulture));
                }
                if (volta == 2)
                {
                    return Json(OdontoWeb_Resources.ResourceManager.GetString("M0025", CultureInfo.CurrentCulture));
                }

                return Json("SUCESSO!!");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return Json(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult EditarTarefaCompartilhada(Int32 id)
        {
            // Prepara view
            TAREFA item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Tarefa"] = item;
            Session["IdVolta"] = id;
            TarefaViewModel vm = Mapper.Map<TAREFA, TarefaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarTarefaCompartilhada(TarefaViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    TAREFA item = Mapper.Map<TarefaViewModel, TAREFA>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<TAREFA>();
                    Session["ListaTarefa"] = null;
                    return RedirectToAction("MontarTelaTarefa");
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
        public ActionResult ExcluirTarefa(Int32 id)
        {
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            TAREFA item = baseApp.GetItemById(id);
            objetoAntes = (TAREFA)Session["Tarefa"];
            item.TARE_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usu);
            listaMaster = new List<TAREFA>();
            Session["ListaTarefa"] = null;
            return RedirectToAction("MontarTelaTarefa");
        }

        [HttpGet]
        public ActionResult ReativarTarefa(Int32 id)
        {
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            TAREFA item = baseApp.GetItemById(id);
            objetoAntes = (TAREFA)Session["Tarefa"];
            item.TARE_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usu);
            listaMaster = new List<TAREFA>();
            Session["ListaTarefa"] = null;
            return RedirectToAction("MontarTelaTarefa");
        }

        [HttpPost]
        public ActionResult UploadFileTarefa(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoTarefa");
            }

            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            TAREFA item = baseApp.GetById((Int32)Session["IdVolta"]);
            var fileName = Path.GetFileName(file.FileName);

            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoTarefa");
            }

            String caminho = "/Imagens/" + idAss.ToString() + "/Tarefas/" + item.TARE_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            TAREFA_ANEXO foto = new TAREFA_ANEXO();
            foto.TAAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.TAAN_DT_ANEXO = DateTime.Today;
            foto.TAAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.TAAN_IN_TIPO = tipo;
            foto.TAAN_NM_TITULO = fileName;
            foto.TARE_CD_ID = item.TARE_CD_ID;

            item.TAREFA_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoTarefa");
        }

        [HttpPost]
        public JsonResult UploadFileTarefa_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
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
                    UploadFileTarefa(file);
                }
            }

            return Json("1"); // VoltarAnexoCliente
        }

        public ActionResult VoltarAnexoTarefa()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("EditarTarefa", new { id = (Int32)Session["IdVolta"] });
        }

        public ActionResult VerKanbanTarefa()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarDesenvolvimento", "BaseAdmin");
        }

        public ActionResult GerarRelatorioLista()
        {
            // Prepara geração
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "TarefaLista" + "_" + data + ".pdf";
            List<TAREFA> lista = (List<TAREFA>)Session["ListaTarefa"];
            TAREFA filtro = (TAREFA)Session["FiltroTarefa"];
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // Linha horizontal
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line);

            // Cabeçalho
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Images/5.png"));
            image.ScaleAbsolute(50, 50);
            cell.AddElement(image);
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Tarefas - Listagem", meuFont2))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cell.Border = 0;
            cell.Colspan = 4;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);
            line1 = new Paragraph("  ");
            pdfDoc.Add(line1);

            // Grid
            table = new PdfPTable(new float[] { 80f, 60f, 120f, 60f, 80f, 60f, 60f});
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Tarefas selecionadas pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.Colspan = 7;
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Tipo", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Início", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Título", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Previsão", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Status", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Realizada", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Compartilhada", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            foreach (TAREFA item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.TIPO_TAREFA.TITR_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.TARE_DT_CADASTRO.ToShortDateString(), meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.TARE_NM_TITULO, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.TARE_DT_ESTIMADA != null)
                {
                    cell = new PdfPCell(new Paragraph(item.TARE_DT_ESTIMADA.Value.ToShortDateString(), meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("Sem estimativa", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                if (item.TARE_IN_STATUS == 1)
                {
                    cell = new PdfPCell(new Paragraph("Pendente", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else if (item.TARE_IN_STATUS == 2)
                {
                    cell = new PdfPCell(new Paragraph("Suspensa", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else if (item.TARE_IN_STATUS == 3)
                {
                    cell = new PdfPCell(new Paragraph("Cancelada", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else if (item.TARE_IN_STATUS == 4)
                {
                    cell = new PdfPCell(new Paragraph("Encerrada", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                if (item.TARE_DT_REALIZADA != null)
                {
                    cell = new PdfPCell(new Paragraph(item.TARE_DT_REALIZADA.Value.ToShortDateString(), meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("-", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                if(usu.USUA_CD_ID == item.TARE_CD_USUA_1 || usu.USUA_CD_ID == item.TARE_CD_USUA_2 || usu.USUA_CD_ID == item.TARE_CD_USUA_3)
                {
                    cell = new PdfPCell(new Paragraph("Sim", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("Não", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line2);

            // Rodapé
            Chunk chunk1 = new Chunk("Parâmetros de filtro: ", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            String parametros = String.Empty;
            Int32 ja = 0;
            if (filtro != null)
            {
                if (filtro.TITR_CD_ID != null)
                {
                    parametros += "Tipo: " + filtro.TIPO_TAREFA.TITR_NM_NOME;
                    ja = 1;
                }
                if (filtro.TARE_NM_TITULO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Título: " + filtro.TARE_NM_TITULO;
                        ja = 1;
                    }
                    else
                    {
                        parametros +=  " e Título: " + filtro.TARE_NM_TITULO;
                    }
                }
                if (filtro.TARE_DT_CADASTRO != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Data: " + filtro.TARE_DT_CADASTRO.ToShortDateString();
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Data: " + filtro.TARE_DT_CADASTRO.ToShortDateString();
                    }
                }
                if (filtro.TARE_IN_STATUS > 0)
                {
                    if (ja == 0)
                    {
                        parametros += "Status: " + (filtro.TARE_IN_STATUS == 1 ? "Pendente" : filtro.TARE_IN_STATUS == 2 ? "Suspensa" : filtro.TARE_IN_STATUS == 3 ? "Cancelada" : "Realizada");
                        ja = 1;
                    }
                    else
                    {
                        parametros += "e Status: " + (filtro.TARE_IN_STATUS == 1 ? "Pendente" : filtro.TARE_IN_STATUS == 2 ? "Suspensa" : filtro.TARE_IN_STATUS == 3 ? "Cancelada" : "Realizada");
                    }
                }
                if (ja == 0)
                {
                    parametros = "Nenhum filtro definido.";
                }
            }
            else
            {
                parametros = "Nenhum filtro definido.";
            }
            Chunk chunk = new Chunk(parametros, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);

            // Linha Horizontal
            Paragraph line3 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line3);

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

            return RedirectToAction("MontarTelaTarefa");
        }

        public FileResult DownloadTarefa(Int32 id)
        {
            TAREFA_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.TAAN_AQ_ARQUIVO;
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
        public ActionResult VerAnexoTarefa(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            // Prepara view
            TAREFA_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult IncluirAcompanhamento()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            TAREFA item = baseApp.GetItemById((Int32)Session["IdVolta"]);
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
            TAREFA_ACOMPANHAMENTO coment = new TAREFA_ACOMPANHAMENTO();
            TarefaAcompanhamentoViewModel vm = Mapper.Map<TAREFA_ACOMPANHAMENTO, TarefaAcompanhamentoViewModel>(coment);
            vm.TAAC_DT_ACOMPANHAMENTO = DateTime.Today;
            vm.TAAC_IN_ATIVO = 1;
            vm.TARE_CD_ID = item.TARE_CD_ID;
            vm.USUARIO = usuarioLogado;
            vm.USUA_CD_ID = usuarioLogado.USUA_CD_ID;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirAcompanhamento(TarefaAcompanhamentoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    TAREFA_ACOMPANHAMENTO item = Mapper.Map<TarefaAcompanhamentoViewModel, TAREFA_ACOMPANHAMENTO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    TAREFA not = baseApp.GetItemById((Int32)Session["IdVolta"]);

                    item.USUARIO = null;
                    not.TAREFA_ACOMPANHAMENTO.Add(item);
                    objetoAntes = not;
                    Int32 volta = baseApp.ValidateEdit(not, objetoAntes);

                    // Verifica retorno

                    // Sucesso
                    return RedirectToAction("EditarTarefa", new { id = (Int32)Session["IdVolta"] });
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
    }
}