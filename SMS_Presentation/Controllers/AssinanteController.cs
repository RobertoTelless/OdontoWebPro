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
    public class AssinanteController : Controller
    {
        private readonly IAssinanteAppService assApp;
        private readonly IUsuarioAppService usuApp;

        private String msg;
        private Exception exception;
        ASSINANTE objetoAss = new ASSINANTE();
        ASSINANTE objetoAssAntes = new ASSINANTE();
        List<ASSINANTE> listaMasterAss = new List<ASSINANTE>();
        String extensao;

        public AssinanteController(IAssinanteAppService assApps, IUsuarioAppService usuApps)
        {
            assApp = assApps;
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
        public ActionResult MontarTelaAssinante()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            usuario = (USUARIO)Session["UserCredentials"];

            // Carrega listas
            if ((List<ASSINANTE>)Session["ListaAssinante"] == null)
            {
                listaMasterAss = assApp.GetAllItens();
                Session["ListaAssinante"] = listaMasterAss;
            }
            ViewBag.Listas = (List<ASSINANTE>)Session["ListaAssinante"];
            ViewBag.Title = "Assinantes";
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Especial", Value = "2" });
            tipo.Add(new SelectListItem() { Text = "Pro", Value = "4" });
            ViewBag.Tipos = new SelectList(tipo, "Value", "Text");
            List<SelectListItem> pessoa = new List<SelectListItem>();
            pessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            pessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.Pessoas = new SelectList(pessoa, "Value", "Text");

            // Indicadores
            ViewBag.Assinantes = ((List<ASSINANTE>)Session["ListaAssinante"]).Count;

            // Mensagem
            if ((Int32)Session["MensAssinante"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensAssinante"] = 0;
            objetoAss = new ASSINANTE();
            objetoAss.ASSI_IN_TIPO = 0;
            return View(objetoAss);
        }

        public ActionResult RetirarFiltroAssinante()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaAssinante"] = null;
            return RedirectToAction("MontarTelaAssinante");
        }

        public ActionResult MostrarTudoAssinante()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            listaMasterAss = assApp.GetAllItensAdm();
            Session["ListaAssinante"] = listaMasterAss;
            return RedirectToAction("MontarTelaAssinante");
        }

        [HttpPost]
        public ActionResult FiltrarAssinante(ASSINANTE item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                List<ASSINANTE> listaObj = new List<ASSINANTE>();
                Int32 volta = assApp.ExecuteFilter(0, item.ASSI_NM_NOME, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensAssinante"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaAssinante");
                }

                // Sucesso
                Session["MensAssinante"] = 0;
                listaMasterAss = listaObj;
                Session["ListaAssinante"] = listaObj;
                return RedirectToAction("MontarTelaAssinante");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaAssinante");
            }
        }

        public ActionResult VoltarBaseAssinante()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaAssinante");
        }

        [HttpGet]
        public ActionResult IncluirAssinante()
        {
            // Prepara listas
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Especial", Value = "2" });
            tipo.Add(new SelectListItem() { Text = "Pro", Value = "4" });
            ViewBag.Tipos = new SelectList(tipo, "Value", "Text");
            List<SelectListItem> pessoa = new List<SelectListItem>();
            pessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            pessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.Pessoas = new SelectList(pessoa, "Value", "Text");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");

            // Prepara view
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            ASSINANTE item = new ASSINANTE();
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            vm.ASSI_DT_INICIO = DateTime.Today.Date;
            vm.ASSI_IN_ATIVO = 1;
            vm.ASSI_IN_STATUS = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirAssinante(AssinanteViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Especial", Value = "2" });
            tipo.Add(new SelectListItem() { Text = "Pro", Value = "4" });
            ViewBag.Tipos = new SelectList(tipo, "Value", "Text");
            List<SelectListItem> pessoa = new List<SelectListItem>();
            pessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            pessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.Pessoas = new SelectList(pessoa, "Value", "Text");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ASSINANTE item = Mapper.Map<AssinanteViewModel, ASSINANTE>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = assApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensAssinante"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0017", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Criar pasta assinante
                    String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/Assinantes/" + item.ASSI_CD_ID.ToString() + "/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/Assinantes/" + item.ASSI_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Criar usuario Master
                    USUARIO usu = new USUARIO();
                    usu.ASSI_CD_ID = item.ASSI_CD_ID;
                    usu.PERF_CD_ID = 2;
                    usu.USUA_DT_ACESSO = DateTime.Today.Date;
                    usu.USUA_DT_ALTERACAO = DateTime.Today.Date;
                    usu.USUA_DT_BLOQUEADO = null;
                    usu.USUA_DT_CADASTRO = DateTime.Today.Date;
                    usu.USUA_DT_TROCA_SENHA = null;
                    usu.USUA_DT_ULTIMA_FALHA = null;
                    usu.USUA_IN_ATIVO = 1;
                    usu.USUA_IN_BLOQUEADO = 0;
                    usu.USUA_IN_LOGIN_PROVISORIO = 0;
                    usu.USUA_IN_PROVISORIO = 0;
                    usu.USUA_NM_EMAIL = item.ASSI_NM_EMAIL;
                    usu.USUA_NM_LOGIN = item.ASSI_NM_LOGIN;
                    usu.USUA_NM_NOME = item.ASSI_NM_NOME;
                    usu.USUA_NM_NOVA_SENHA = "11112222";
                    usu.USUA_NM_SENHA = item.ASSI_NM_SENHA;
                    usu.USUA_NM_SENHA_CONFIRMA = "11112222";
                    usu.USUA_NR_ACESSOS = 0;
                    usu.USUA_NR_CELULAR = item.ASSI_NR_CELULAR;
                    usu.USUA_NR_FALHAS = 0;
                    usu.USUA_NR_TELEFONE = item.ASSI_NR_TELEFONE;
                    usu.USUA_TX_OBSERVACOES = item.ASSI_TX_OBSERVACOES;
                    Int32 volta1 = usuApp.ValidateCreate(usu, usuarioLogado, item.ASSI_CD_ID);

                    // Criar pasta usuario
                    caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Usuarios/" + usu.USUA_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Usuarios/" + usu.USUA_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Criar foto usuario
                    usu.USUA_AQ_FOTO = "~/Imagens/" + item.ASSI_CD_ID.ToString() + "/Usuarios/" + usu.USUA_CD_ID.ToString() + "/Fotos/a5.jpg";
                    volta1 = usuApp.ValidateEdit(usu, usu, item.ASSI_CD_ID);

                    // Sucesso
                    listaMasterAss = new List<ASSINANTE>();
                    Session["ListaAssinante"] = null;
                    Session["VoltaAssinante"] = 1;
                    Session["IdAssinanteVolta"] = item.ASSI_CD_ID;
                    Session["Assinante"] = item;
                    Session["MensAssinante"] = 0;
                    return RedirectToAction("MontarTelaAssinante");
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
        public ActionResult EditarAssinante(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Especial", Value = "2" });
            tipo.Add(new SelectListItem() { Text = "Pro", Value = "4" });
            ViewBag.Tipos = new SelectList(tipo, "Value", "Text");
            List<SelectListItem> pessoa = new List<SelectListItem>();
            pessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            pessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.Pessoas = new SelectList(pessoa, "Value", "Text");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            ASSINANTE item = assApp.GetItemById(id);
            objetoAssAntes = item;
            Session["Assinante"] = item;
            Session["IdVolta"] = id;
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarAssinante(AssinanteViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            List<SelectListItem> tipo = new List<SelectListItem>();
            tipo.Add(new SelectListItem() { Text = "Normal", Value = "1" });
            tipo.Add(new SelectListItem() { Text = "Especial", Value = "2" });
            tipo.Add(new SelectListItem() { Text = "Pro", Value = "4" });
            ViewBag.Tipos = new SelectList(tipo, "Value", "Text");
            List<SelectListItem> pessoa = new List<SelectListItem>();
            pessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            pessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.Pessoas = new SelectList(pessoa, "Value", "Text");
            ViewBag.UF = new SelectList(SessionMocks.UFs, "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
            {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    ASSINANTE item = Mapper.Map<AssinanteViewModel, ASSINANTE>(vm);
                    Int32 volta = assApp.ValidateEdit(item, objetoAssAntes, usuarioLogado);

                    // Verifica cancelamento
                    if (item.ASSI_DT_FINAL != null)
                    {
                        // Atualiza usuarios
                        List<USUARIO> lista = usuApp.GetAllItens(item.ASSI_CD_ID);
                        if (lista.Count > 0)
                        {
                            foreach (USUARIO usu in lista)
                            {
                                usu.USUA_IN_BLOQUEADO = 1;
                                Int32 volta1 = usuApp.ValidateEdit(usu, usuarioLogado, item.ASSI_CD_ID);
                            }
                        }
                    }

                    // Sucesso
                    listaMasterAss = new List<ASSINANTE>();
                    Session["ListaAssinante"] = null;
                    Session["MensAssinante"] = 0;
                    return RedirectToAction("MontarTelaAssinante");
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
        public ActionResult ExcluirAssinante(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            ASSINANTE item = assApp.GetItemById(id);
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirAssinante(AssinanteViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                ASSINANTE item = Mapper.Map<AssinanteViewModel, ASSINANTE>(vm);
                Int32 volta = assApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensAssinante"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0018", CultureInfo.CurrentCulture));
                    return View(vm);
                }
                
                // Atualiza usuarios
                List<USUARIO> lista = usuApp.GetAllItens(item.ASSI_CD_ID);
                if (lista.Count > 0)
                {
                    foreach (USUARIO usu in lista)
                    {
                        usu.USUA_IN_ATIVO = 0;
                        Int32 volta1 = usuApp.ValidateEdit(usu, usuarioLogado, item.ASSI_CD_ID);
                    }
                }

                // Sucesso
                listaMasterAss = new List<ASSINANTE>();
                Session["ListaAssinante"] = null;
                Session["MensAssinante"] = 0;
                return RedirectToAction("MontarTelaAssinante");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarAssinante(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            ASSINANTE item = assApp.GetItemById(id);
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarAssinante(AssinanteViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                ASSINANTE item = Mapper.Map<AssinanteViewModel, ASSINANTE>(vm);
                Int32 volta = assApp.ValidateReativar(item, usuarioLogado);

                // Atualiza usuarios
                List<USUARIO> lista = usuApp.GetAllItens(item.ASSI_CD_ID);
                if (lista.Count > 0)
                {
                    foreach (USUARIO usu in lista)
                    {
                        usu.USUA_IN_ATIVO = 1;
                        Int32 volta1 = usuApp.ValidateEdit(usu, usuarioLogado, item.ASSI_CD_ID);
                    }
                }

                // Sucesso
                listaMasterAss = new List<ASSINANTE>();
                Session["ListaAssinante"] = null;
                Session["MensAssinante"] = 0;
                return RedirectToAction("MontarTelaAssinante");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult VerAnexoAssinante(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            ASSINANTE_ANEXO item = assApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoAssinante()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 volta = (Int32)Session["IdVolta"];
            return RedirectToAction("EditarAssinante", new { id = volta });
        }

        public FileResult DownloadAssinante(Int32 id)
        {
            ASSINANTE_ANEXO item = assApp.GetAnexoById(id);
            String arquivo = item.ASAN_AQ_ARQUIVO;
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
        public ActionResult UploadFileAssinante(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoAssinante");
            }

            Int32 id = (Int32)Session["IdVolta"];
            ASSINANTE item = assApp.GetById(id);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0020", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoAssinante");
            }
            String caminho = "/Imagens/Assinantes/" + item.ASSI_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            ASSINANTE_ANEXO foto = new ASSINANTE_ANEXO();
            foto.ASAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.ASAN_DT_ANEXO = DateTime.Today;
            foto.ASA_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.ASAN_IN_TIPO = tipo;
            foto.ASAN_NM_TITULO = fileName;
            foto.ASSI_CD_ID = item.ASSI_CD_ID;

            item.ASSINANTE_ANEXO.Add(foto);
            Int32 volta = assApp.ValidateEdit(item);
            return RedirectToAction("VoltarAnexoAssinante");
        }

        [HttpGet]
        public ActionResult SlideShow()
        {
            // Prepara view
            ASSINANTE item = assApp.GetItemById((Int32)Session["IdVolta"]);
            AssinanteViewModel vm = Mapper.Map<ASSINANTE, AssinanteViewModel>(item);
            return View(vm);
        }

    }
}