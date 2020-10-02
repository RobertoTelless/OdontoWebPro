using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SystemBRPresentation.App_Start;
using EntitiesServices.Work_Classes;
using AutoMapper;
using SystemBRPresentation.ViewModels;
using System.IO;

namespace SystemBRPresentation.Controllers
{
    public class PatrimonioController : Controller
    {
        private readonly IPatrimonioAppService patrApp;
        private readonly ILogAppService logApp;
        private readonly ITipoPessoaAppService tpApp;
        private readonly ICategoriaPatrimonioAppService cpApp;
        private readonly IFilialAppService filApp;
        private readonly IMatrizAppService matrizApp;

        private String msg;
        private Exception exception;
        PATRIMONIO objetoPatr = new PATRIMONIO();
        PATRIMONIO objetoPatrAntes = new PATRIMONIO();
        List<PATRIMONIO> listaMasterPatr = new List<PATRIMONIO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public PatrimonioController(IPatrimonioAppService patrApps, ILogAppService logApps, ITipoPessoaAppService tpApps, ICategoriaPatrimonioAppService cpApps, IFilialAppService filApps, IMatrizAppService matrizApps)
        {
            patrApp = patrApps;
            logApp = logApps;
            tpApp = tpApps;
            cpApp = cpApps;
            filApp = filApps;
            matrizApp = matrizApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            PATRIMONIO item = new PATRIMONIO();
            PatrimonioViewModel vm = Mapper.Map<PATRIMONIO, PatrimonioViewModel>(item);
            return View(vm);
        }

        public ActionResult Voltar()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarGeral()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult DashboardAdministracao()
        {
            listaMasterPatr = new List<PATRIMONIO>();
            SessionMocks.patrimonio = null;
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaPatrimonio()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            usuario = SessionMocks.UserCredentials;
            //if (SessionMocks.UserCredentials != null)
            //{
            //    usuario = SessionMocks.UserCredentials;

            //    // Verfifica permissão
            //    if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
            //    {
            //        return RedirectToAction("CarregarBase", "BaseAdmin");
            //    }
            //}
            //else
            //{
            //    return RedirectToAction("Login", "ControleAcesso");
            //}

            // Carrega listas
            if (SessionMocks.listaPatrimonio == null)
            {
                listaMasterPatr = patrApp.GetAllItens();
                SessionMocks.listaPatrimonio = listaMasterPatr;
            }
            ViewBag.Listas = SessionMocks.listaPatrimonio;
            ViewBag.Title = "Patrimonio";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Tipos = new SelectList(SessionMocks.CatsPatrimonio, "CAPA_CD_ID", "CAPA_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");

            // Indicadores
            ViewBag.Depreciados = SessionMocks.listaPatrimonio.Where(p => p.PATR_DT_COMPRA.Value.AddDays(p.PATR_NR_VIDA_UTIL.Value) < DateTime.Today.Date & p.PATR_IN_ATIVO == 1 & p.ASSI_CD_ID == SessionMocks.IdAssinante).ToList().Count;
            ViewBag.Patrimonios = SessionMocks.listaPatrimonio.Count;
            ViewBag.Baixados = SessionMocks.listaPatrimonio.Where(p => p.PATR_DT_BAIXA != null & p.PATR_IN_ATIVO == 1 & p.ASSI_CD_ID == SessionMocks.IdAssinante).ToList().Count;

            // Abre view
            objetoPatr = new PATRIMONIO();
            SessionMocks.voltaPatrimonio = 1;
            return View(objetoPatr);
        }

        public ActionResult RetirarFiltroPatrimonio()
        {
            SessionMocks.listaPatrimonio = null;
            SessionMocks.filtroPatrimonio = null;
            return RedirectToAction("MontarTelaPatrimonio");
        }

        public ActionResult MostrarTudoPatrimonio()
        {
            listaMasterPatr = patrApp.GetAllItensAdm();
            SessionMocks.filtroPatrimonio = null;
            SessionMocks.listaPatrimonio = listaMasterPatr;
            return RedirectToAction("MontarTelaPatrimonio");
        }

        [HttpPost]
        public ActionResult FiltrarPatrimonio(PATRIMONIO item)
        {
            try
            {
                // Executa a operação
                List<PATRIMONIO> listaObj = new List<PATRIMONIO>();
                SessionMocks.filtroPatrimonio = item;
                Int32 volta = patrApp.ExecuteFilter(item.CAPA_CD_ID, item.PATR_NM_NOME, item.PATR_NR_NUMERO_PATRIMONIO, item.FILI_CD_ID, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0010", CultureInfo.CurrentCulture);
                }

                // Sucesso
                listaMasterPatr = listaObj;
                SessionMocks.listaPatrimonio = listaObj;
                return RedirectToAction("MontarTelaPatrimonio");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaPatrimonio");
            }
        }

        public ActionResult VoltarBasePatrimonio()
        {
            return RedirectToAction("MontarTelaPatrimonio");
        }

        [HttpGet]
        public ActionResult IncluirPatrimonio()
        {
            // Prepara listas
            ViewBag.Tipos = new SelectList(SessionMocks.CatsPatrimonio, "CAPA_CD_ID", "CAPA_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");

            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            PATRIMONIO item = new PATRIMONIO();
            PatrimonioViewModel vm = Mapper.Map<PATRIMONIO, PatrimonioViewModel>(item);
            vm.ASSI_CD_ID = SessionMocks.IdAssinante;
            vm.PATR_DT_CADASTRO = DateTime.Today;
            vm.PATR_IN_ATIVO = 1;
            vm.MATR_CD_ID = SessionMocks.Matriz.MATR_CD_ID;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirPatrimonio(PatrimonioViewModel vm)
        {
            ViewBag.Tipos = new SelectList(SessionMocks.CatsPatrimonio, "CAPA_CD_ID", "CAPA_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PATRIMONIO item = Mapper.Map<PatrimonioViewModel, PATRIMONIO>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = patrApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0063", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Carrega foto e processa alteracao
                    item.PATR_AQ_FOTO = "~/Imagens/Base/FotoBase.jpg";
                    volta = patrApp.ValidateEdit(item, item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Patrimonio/" + item.PATR_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Patrimonio/" + item.PATR_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    
                    // Sucesso
                    listaMasterPatr = new List<PATRIMONIO>();
                    SessionMocks.listaPatrimonio = null;

                    SessionMocks.idVolta = item.PATR_CD_ID;
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
        public ActionResult EditarPatrimonio(Int32 id)
        {
            // Prepara view
            ViewBag.Tipos = new SelectList(SessionMocks.CatsPatrimonio, "CAPA_CD_ID", "CAPA_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");

            // Objeto e indicadores
            PATRIMONIO item = patrApp.GetItemById(id);
            Int32 dias = patrApp.CalcularDiasDepreciacao(item);
            ViewBag.Dias = dias;
            ViewBag.Status = dias > 0 ? "Ativo" : "Depreciado";
            objetoPatrAntes = item;
            SessionMocks.patrimonio = item;
            SessionMocks.idVolta = id;
            PatrimonioViewModel vm = Mapper.Map<PATRIMONIO, PatrimonioViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPatrimonio(PatrimonioViewModel vm)
        {
            ViewBag.Tipos = new SelectList(SessionMocks.CatsPatrimonio, "CAPA_CD_ID", "CAPA_NM_NOME");
            ViewBag.Filiais = new SelectList(SessionMocks.Filiais, "FILI_CD_ID", "FILI_NM_NOME");
            //if (ModelState.IsValid)
            //{
            try
            {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    PATRIMONIO item = Mapper.Map<PatrimonioViewModel, PATRIMONIO>(vm);
                    Int32 volta = patrApp.ValidateEdit(item, objetoPatrAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterPatr = new List<PATRIMONIO>();
                    SessionMocks.listaPatrimonio = null;
                    return RedirectToAction("MontarTelaPatrimonio");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            //}
            //else
            //{
            //    return View(vm);
            //}
        }

        [HttpGet]
        public ActionResult ExcluirPatrimonio(Int32 id)
        {
            // Prepara view
            PATRIMONIO item = patrApp.GetItemById(id);
            PatrimonioViewModel vm = Mapper.Map<PATRIMONIO, PatrimonioViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirPatrimonio(PatrimonioViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PATRIMONIO item = Mapper.Map<PatrimonioViewModel, PATRIMONIO>(vm);
                Int32 volta = patrApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMasterPatr = new List<PATRIMONIO>();
                SessionMocks.listaPatrimonio = null;
                return RedirectToAction("MontarTelaPatrimonio");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarPatrimonio(Int32 id)
        {
            // Prepara view
            PATRIMONIO item = patrApp.GetItemById(id);
            PatrimonioViewModel vm = Mapper.Map<PATRIMONIO, PatrimonioViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarPatrimonio(PatrimonioViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                PATRIMONIO item = Mapper.Map<PatrimonioViewModel, PATRIMONIO>(vm);
                Int32 volta = patrApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMasterPatr = new List<PATRIMONIO>();
                SessionMocks.listaPatrimonio = null;
                return RedirectToAction("MontarTelaPatrimonio");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult VerAnexoPatrimonio(Int32 id)
        {
            // Prepara view
            PATRIMONIO_ANEXO item = patrApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoPatrimonio()
        {
            return RedirectToAction("EditarPatrimonio", new { id = SessionMocks.idVolta });
        }

        public FileResult DownloadPatrimonio(Int32 id)
        {
            PATRIMONIO_ANEXO item = patrApp.GetAnexoById(id);
            String arquivo = item.PAAN_AQ_ARQUIVO;
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
        public ActionResult UploadFilePatrimonio(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPatrimonio");
            }

            PATRIMONIO item = patrApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPatrimonio");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Patrimonio/" + item.PATR_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            PATRIMONIO_ANEXO foto = new PATRIMONIO_ANEXO();
            foto.PAAN_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.PAAN_DT_ANEXO = DateTime.Today;
            foto.PAAN_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.PAAN_IN_TIPO = tipo;
            foto.PAAN_NM_TITULO = fileName;
            foto.PATR_CD_ID = item.PATR_CD_ID;

            item.PATRIMONIO_ANEXO.Add(foto);
            objetoPatrAntes = item;
            Int32 volta = patrApp.ValidateEdit(item, objetoPatrAntes);
            return RedirectToAction("VoltarAnexoPatrimonio");
        }

        [HttpPost]
        public JsonResult UploadFilePatrimonio_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
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
                    UploadFotoPatrimonio(file);

                    count++;
                }
                else
                {
                    UploadFilePatrimonio(file);
                }
            }

            return Json("1"); // VoltarAnexoCliente
        }

        [HttpPost]
        public ActionResult UploadFotoPatrimonio(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPatrimonio");
            }

            PATRIMONIO item = patrApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0015", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPatrimonio");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Patrimonio/" + item.PATR_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.PATR_AQ_FOTO = "~" + caminho + fileName;
            objetoPatrAntes = item;
            Int32 volta = patrApp.ValidateEdit(item, objetoPatrAntes);
            return RedirectToAction("VoltarAnexoPatrimonio");
        }

        public ActionResult SlideShowPatrimonio()
        {
            // Prepara view
            PATRIMONIO item = patrApp.GetItemById(SessionMocks.idVolta);
            objetoPatrAntes = item;
            PatrimonioViewModel vm = Mapper.Map<PATRIMONIO, PatrimonioViewModel>(item);
            return View(vm);
        }

    }
}