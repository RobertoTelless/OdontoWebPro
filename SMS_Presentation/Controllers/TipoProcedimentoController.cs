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
    public class TipoProcedimentoController : Controller
    {
        private readonly ITipoProcedimentoAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IFilialAppService filApp;

        private String msg;
        private Exception exception;
        TIPO_PROCEDIMENTO objeto = new TIPO_PROCEDIMENTO();
        TIPO_PROCEDIMENTO objetoAntes = new TIPO_PROCEDIMENTO();
        List<TIPO_PROCEDIMENTO> listaMaster = new List<TIPO_PROCEDIMENTO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public TipoProcedimentoController(ITipoProcedimentoAppService baseApps, ILogAppService logApps, IFilialAppService filApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            filApp = filApps;
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
        public ActionResult MontarTelaTipoProcedimento()
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
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idMatriz = (Int32)Session["IdMatriz"];

            if ((List<TIPO_PROCEDIMENTO>)Session["ListaProc"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    listaMaster = listaMaster.Where(p => p.FILI_CD_ID == usuario.FILI_CD_ID).ToList();
                }
                Session["ListaProc"] = listaMaster;
            }
            ViewBag.Listas = (List<TIPO_PROCEDIMENTO>)Session["ListaProc"];
            ViewBag.Title = "Tipos de Procedimento";
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");

            // Indicadores
            ViewBag.Procs = ((List<TIPO_PROCEDIMENTO>)Session["ListaProc"]).Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensProc"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProc"] == 3)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0046", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensProc"] == 4)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0044", CultureInfo.CurrentCulture));
            }

            // Abre view
            objeto = new TIPO_PROCEDIMENTO();
            Session["VoltaProc"] = 1;
            Session["MensProc"] = 0;
            return View(objeto);
        }

        public ActionResult RetirarFiltro()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaProc"] = null;
            Session["FiltroProc"] = null;
            return RedirectToAction("MontarTelaTipoProcedimento");
        }

        public ActionResult MostrarTudo()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            listaMaster = baseApp.GetAllItensAdm(idAss);
            if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
            {
                listaMaster = listaMaster.Where(p => p.FILI_CD_ID == usuario.FILI_CD_ID).ToList();
            }
            Session["ListaProc"] = listaMaster;
            Session["FiltroProc"] = null;
            return RedirectToAction("MontarTelaTipoProcedimento");
        }

        [HttpPost]
        public ActionResult FiltrarTipoProcedimento(TIPO_PROCEDIMENTO item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                Int32 idAss = (Int32)Session["IdAssinante"];
                USUARIO usuario = (USUARIO)Session["UserCredentials"];
                List<TIPO_PROCEDIMENTO> listaObj = new List<TIPO_PROCEDIMENTO>();
                Session["FiltroProc"] = item;
                Int32 volta = baseApp.ExecuteFilter(item.TIPR_NM_NOME, item.TIPR_DS_DESCRICAO, item.FILI_CD_ID, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensProc"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    listaMaster = listaMaster.Where(p => p.FILI_CD_ID == usuario.FILI_CD_ID).ToList();
                }
                listaMaster = listaObj;
                Session["ListaProc"] = listaObj;
                Session["IdProc"] = item.TIPR_CD_ID;
                return RedirectToAction("VoltarAnexoTipoProcedimento");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaTipoProcedimento");
            }
        }

        [HttpGet]
        public ActionResult VerTipoProcedimento(Int32 id)
        {
            // Prepara view
            // Executa a operação
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            TIPO_PROCEDIMENTO item = baseApp.GetItemById(id);
            objetoAntes = item;
            TipoProcedimentoViewModel vm = Mapper.Map<TIPO_PROCEDIMENTO, TipoProcedimentoViewModel>(item);
            Session["VoltaProc"] = 2;
            return View(vm);
        }

        [HttpGet]
        public ActionResult IncluirTipoProcedimento()
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
                    Session["MensProc"] = 2;
                    return RedirectToAction("MontarTelaTipoProcedimento", "TipoProcedimento");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara listas
            Int32 idMatriz = (Int32)Session["IdMatriz"];
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Prepara view
            TIPO_PROCEDIMENTO item = new TIPO_PROCEDIMENTO();
            TipoProcedimentoViewModel vm = Mapper.Map<TIPO_PROCEDIMENTO, TipoProcedimentoViewModel>(item);
            vm.TIPR_IN_ATIVO = 1;
            if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
            {
                vm.FILI_CD_ID = usuario.FILI_CD_ID;
            }
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirTipoProcedimento(TipoProcedimentoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idMatriz = (Int32)Session["IdMatriz"];
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    TIPO_PROCEDIMENTO item = Mapper.Map<TipoProcedimentoViewModel, TIPO_PROCEDIMENTO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0045", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Cria pastas
                    String caminho = "/Imagens/" + idAss.ToString() + "/Procedimentos/" + item.TIPR_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMaster = new List<TIPO_PROCEDIMENTO>();
                    Session["ListaProc"] = null;
                    Session["VoltaProc"] = 1;
                    Session["IdProcVolta"] = item.TIPR_CD_ID;
                    Session["TipoProcedimento"] = item;
                    Session["MensProc"] = 0;
                    return RedirectToAction("EditarTipoProcedimento", new { id = item.TIPR_CD_ID });
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
        public ActionResult EditarTipoProcedimento(Int32 id)
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
                    Session["MensProc"] = 2;
                    return RedirectToAction("MontarTelaTipoProcedimento", "TipoProcedimento");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");

            TIPO_PROCEDIMENTO item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["TipoProcedimento"] = item;
            Session["IdProc"] = id;
            Session["VoltaProc"] = 1;
            TipoProcedimentoViewModel vm = Mapper.Map<TIPO_PROCEDIMENTO, TipoProcedimentoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarTipoProcedimento(TipoProcedimentoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idMatriz), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    TIPO_PROCEDIMENTO item = Mapper.Map<TipoProcedimentoViewModel, TIPO_PROCEDIMENTO>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0045", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMaster = new List<TIPO_PROCEDIMENTO>();
                    Session["ListaProc"] = null;
                    Session["MensProc"] = 0;
                    return RedirectToAction("MontarTelaTipoProcedimento");
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

        public ActionResult VoltarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaProc"] = baseApp.GetAllItens(idAss);
            return RedirectToAction("MontarTelaTipoProcedimento");
        }

        [HttpGet]
        public ActionResult ExcluirTipoProcedimento(Int32 id)
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
                    Session["MensProc"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            TIPO_PROCEDIMENTO item = baseApp.GetItemById(id);
            objetoAntes = item;
            item.TIPR_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensProc"] = 3;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0046", CultureInfo.CurrentCulture));
            }
            listaMaster = new List<TIPO_PROCEDIMENTO>();
            Session["ListaProc"] = null;
            return RedirectToAction("MontarTelaTipoProcedimento");
        }

        [HttpGet]
        public ActionResult ReativarTipoProcedimento(Int32 id)
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
                    Session["MensProc"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            TIPO_PROCEDIMENTO item = baseApp.GetItemById(id);
            objetoAntes = item;
            item.TIPR_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            listaMaster = new List<TIPO_PROCEDIMENTO>();
            Session["ListaProc"] = null;
            return RedirectToAction("MontarTelaTipoProcedimento");
        }

        public ActionResult VoltarAnexoTipoProcedimento()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idProc= (Int32)Session["IdProc"];
            if ((Int32)Session["VoltaProc"] == 1)
            {
                return RedirectToAction("EditarTipoProcedimento", new { id = idProc });
            }
            return RedirectToAction("VerTipoProcedimento", new { id = idProc });
        }

        [HttpGet]
        public ActionResult VerAnexoTipoProcedimento(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            TIPO_PROCEDIMENTO_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public FileResult DownloadTipoProcedimento(Int32 id)
        {
            TIPO_PROCEDIMENTO_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.TPAN_AQ_ARQUIVO;
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
        public ActionResult UploadFileTipoProcedimento(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoTipoProcedimento");
            }

            Int32 idProc = (Int32)Session["IdProc"];
            Int32 idAss = (Int32)Session["IdAssinante"];
            TIPO_PROCEDIMENTO item = baseApp.GetById(idProc);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);

            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoTipoProcedimento");
            }

            String caminho = "/Imagens/" + idAss.ToString() + "/Procedimentos/" + item.TIPR_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            TIPO_PROCEDIMENTO_ANEXO foto = new TIPO_PROCEDIMENTO_ANEXO();
            foto.TPAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.TPAN_DT_ANEXO = DateTime.Today;
            foto.TPAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.TPAN_IN_TIPO = tipo;
            foto.TPAN_NM_TITULO = fileName;
            foto.TIPR_CD_ID = item.TIPR_CD_ID;

            item.TIPO_PROCEDIMENTO_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoTipoProcedimento");
        }






    }
}