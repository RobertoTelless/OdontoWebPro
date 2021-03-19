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
using Canducci.Zip;

namespace Odonto.Controllers
{
    public class PacienteController : Controller
    {
        private readonly IPacienteAppService baseApp;

        private String msg;
        private Exception exception;
        PACIENTE objeto = new PACIENTE();
        PACIENTE objetoAntes = new PACIENTE();
        List<PACIENTE> listaMaster = new List<PACIENTE>();
        String extensao;

        public PacienteController(IPacienteAppService baseApps)
        {
            baseApp = baseApps;
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
        public ActionResult MontarTelaPaciente()
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
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];


            // Carrega listas
            if ((List<PACIENTE>)Session["ListaPaciente"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaPaciente"] = listaMaster;
            }

            ViewBag.Listas = (List<PACIENTE>)Session["ListaPaciente"];
            ViewBag.Title = "Pacientes";
            ViewBag.Filiais = new SelectList(baseApp.GetAllFiliais(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAPA_CD_ID", "CAPA_NM_NOME");
            ViewBag.UF = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Indicadores
            ViewBag.Pacientes = listaMaster.Count;
            
            // Mensagem
            if ((Int32)Session["MensPaciente"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensPaciente"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0010", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensPaciente"] == 3)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0047", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensPaciente"] = 0;
            objeto = new PACIENTE();
            Session["VoltaPaciente"] = 1;
            return View(objeto);
        }

        [HttpGet]
        public ActionResult VerCardsPaciente()
        {
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List<PACIENTE>)Session["ListaPaciente"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaPaciente"] = listaMaster;
            }

            ViewBag.Listas = (List<PACIENTE>)Session["ListaPaciente"];
            ViewBag.Title = "Pacientes";
            ViewBag.Filiais = new SelectList(baseApp.GetAllFiliais(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.UF = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAPA_CD_ID", "CAPA_NM_NOME");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Indicadores
            ViewBag.Pacientes = listaMaster.Count;

            // Abre view
            Session["MensPaciente"] = 0;
            objeto = new PACIENTE();
            Session["VoltaPaciente"] = 2;
            return View(objeto);
        }

        public ActionResult RetirarFiltroPaciente()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaPaciente"] = null;
            Session["FiltroPaciente"] = null;
            return RedirectToAction("MontarTelaPaciente");
        }

        public ActionResult MostrarTudoPaciente()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["FiltroPaciente"] = null;
            Session["ListaPaciente"] = listaMaster;
            return RedirectToAction("MontarTelaPaciente");
        }

        [HttpPost]
        public ActionResult FiltrarPaciente(PACIENTE item)
        {
            // Verificar login
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Processo
            try
            {
                // Executa a operação
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<PACIENTE> listaObj = new List<PACIENTE>();
                Session["FiltroPaciente"] = item;
                Int32 volta = baseApp.ExecuteFilter(item.CAPA_CD_ID, item.FILI_CD_ID, item.PACI_NM_NOME, item.PACI_NR_CPF, item.PACI_NR_TELEFONE, item.PACI_NR_CELULAR, item.PACI_NM_CIDADE, item.PACI_DT_NASCIMENTO.Value, item.PACI_NM_EMAIL, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensPaciente"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                Session["MensPaciente"] = 0;
                listaMaster = listaObj;
                Session["ListaPaciente"] = listaObj;
                return RedirectToAction("MontarTelaPaciente");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaPaciente");
            }
        }

        public ActionResult VoltarBasePaciente()
        {
            // Verificar login
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaPaciente");
        }

        [HttpGet]
        public ActionResult IncluirPaciente()
        {
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara listas
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.UF = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
            ViewBag.Filiais = new SelectList(baseApp.GetAllFiliais(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAPA_CD_ID", "CAPA_NM_NOME");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            ViewBag.Sexo = new SelectList(sexo, "Value", "Text");

            // Prepara view
            PACIENTE item = new PACIENTE();
            PacienteViewModel vm = Mapper.Map<PACIENTE, PacienteViewModel>(item);
            vm.PACI_IN_ATIVO = 1;
            vm.ASSI_CD_ID = idAss;
            vm.PACI_DT_CADASTRO = DateTime.Today.Date;
            vm.PACI_VL_SALDO_FINANCEIRO = 0;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirPaciente(PacienteViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            var result = new Hashtable();
            ViewBag.UF = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
            ViewBag.Filiais = new SelectList(baseApp.GetAllFiliais(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAPA_CD_ID", "CAPA_NM_NOME");
            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            ViewBag.Sexos = new SelectList(sexo, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PACIENTE item = Mapper.Map<PacienteViewModel, PACIENTE>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0048", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Carrega foto e processa alteracao
                    if (item.PACI_AQ_FOTO == null)
                    {
                        item.PACI_AQ_FOTO = "~/Imagens/Base/FotoBase.jpg";
                        volta = baseApp.ValidateEdit(item, item, usuario);
                    }

                    // Cria pastas
                    String caminho = "/Imagens/" + idAss.ToString() + "/Pacientes/" + item.PACI_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + idAss.ToString() + "/Pacientes/" + item.PACI_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + idAss.ToString() + "/Pacientes/" + item.PACI_CD_ID.ToString() + "/Imagens/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMaster = new List<PACIENTE>();
                    Session["ListaPaciente"] = null;
                    Session["Pacientes"] = baseApp.GetAllItens(idAss);
                    return RedirectToAction("MontarTelaPaciente");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    result.Add("error", ex.Message);
                    return Json(result);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult VerPaciente(Int32 id)
        {
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

            // Prepara view
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            ViewBag.Incluir = 0;

            // Mensagem
            PACIENTE item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Paciente"] = item;
            Session["IdVolta"] = id;
            PacienteViewModel vm = Mapper.Map<PACIENTE, PacienteViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult EditarPaciente(Int32 id)
        {
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }


            // Prepara view
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.UF = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
            ViewBag.Filiais = new SelectList(baseApp.GetAllFiliais(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAPA_CD_ID", "CAPA_NM_NOME");
            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            ViewBag.Sexo = new SelectList(sexo, "Value", "Text");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensPaciente"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensPaciente"] == 4)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensPaciente"] == 5)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
            }

            PACIENTE item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Paciente"] = item;
            Session["IdVolta"] = id;
            PacienteViewModel vm = Mapper.Map<PACIENTE, PacienteViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarPaciente(PacienteViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.UF = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
            ViewBag.Filiais = new SelectList(baseApp.GetAllFiliais(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Tipos = new SelectList(baseApp.GetAllTipos(idAss), "CAPA_CD_ID", "CAPA_NM_NOME");
            List<SelectListItem> sexo = new List<SelectListItem>();
            sexo.Add(new SelectListItem() { Text = "Masculino", Value = "1" });
            sexo.Add(new SelectListItem() { Text = "Feminino", Value = "2" });
            ViewBag.Sexos = new SelectList(sexo, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    PACIENTE item = Mapper.Map<PacienteViewModel, PACIENTE>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuario);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<PACIENTE>();
                    Session["ListaPaciente"] = null;
                    Session["MensPaciente"] = 0;
                    return RedirectToAction("MontarTelaPaciente");
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
        public ActionResult ExcluirPaciente(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("MontarTelaPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            PACIENTE item = baseApp.GetItemById(id);
            objetoAntes = (PACIENTE)Session["Paciente"];
            item.PACI_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            // Verifica retorno
            if (volta == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0047", CultureInfo.CurrentCulture));
                Session["MensPaciente"] = 3;
                return RedirectToAction("MontarTelaPaciente", "Paciente");
            }
            listaMaster = new List<PACIENTE>();
            Session["ListaPaciente"] = null;
            return RedirectToAction("MontarTelaPaciente");
        }


        [HttpGet]
        public ActionResult ReativarPaciente(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("MontarTelaPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            PACIENTE item = baseApp.GetItemById(id);
            objetoAntes = (PACIENTE)Session["Paciente"];
            item.PACI_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            listaMaster = new List<PACIENTE>();
            Session["ListaPaciente"] = null;
            return RedirectToAction("MontarTelaPaciente");
        }

        [HttpGet]
        public ActionResult VerAnexoPaciente(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            // Prepara view
            PACIENTE_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoPaciente()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("EditarPaciente", new { id = (Int32)Session["IdVolta"]});
        }

        public FileResult DownloadPaciente(Int32 id)
        {
            PACIENTE_ANEXO item = baseApp.GetAnexoById(id);
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
        public ActionResult UploadFilePaciente(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                Session["MensPaciente"] = 4;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPaciente");
            }

            PACIENTE item = baseApp.GetById((Int32)Session["idVolta"]);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                Session["MensPaciente"] = 5;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPaciente");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Pacientes/" + item.PACI_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            PACIENTE_ANEXO foto = new PACIENTE_ANEXO();
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
            if (extensao.ToUpper() == ".PDF")
            {
                tipo = 3;
            }
            foto.PAAN_IN_TIPO = tipo;
            foto.PAAN_NM_TITULO = fileName;
            foto.PACI_CD_ID = item.PACI_CD_ID;

            item.PACIENTE_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoPaciente");
        }

        [HttpPost]
        public JsonResult UploadFilePaciente_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
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
                    UploadFotoPaciente(file);

                    count++;
                }
                else
                {
                    UploadFilePaciente(file);
                }
            }

            return Json("1"); 
        }

        [HttpPost]
        public ActionResult UploadFotoPaciente(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (file == null)
            {
                Session["MensPaciente"] = 4;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0049", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPaciente");
            }

            PACIENTE item = baseApp.GetById((Int32)Session["idVolta"]);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                Session["MensPaciente"] = 5;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0050", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoPaciente");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Pacientes/" + item.PACI_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.PACI_AQ_FOTO = "~" + caminho + fileName;
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            listaMaster = new List<PACIENTE>();
            Session["ListaPaciente"] = null;
            return RedirectToAction("VoltarAnexoPaciente");
        }

        [HttpPost]
        public JsonResult PesquisaCEP_Javascript(String cep, int tipoEnd)
        {
            // Chama servico ECT
            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(cep);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza
            var hash = new Hashtable();

            if (tipoEnd == 1)
            {
                hash.Add("PACI_NM_ENDERECO", end.Address + "/" + end.Complement);
                hash.Add("PACI_NM_BAIRRO", end.District);
                hash.Add("PACI_NM_CIDADE", end.City);
                hash.Add("UF_CD_ID", baseApp.GetUFBySigla(end.Uf).UF_CD_ID);
                hash.Add("PACI_NR_CEP", cep);
            }

            // Retorna
            Session["VoltaCEP"] = 2;
            return Json(hash);
        }

        //public ActionResult GerarRelatorioLista()
        //{
        //    if ((String)Session["Ativa"] == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }
        //    // Prepara geração
        //    String data = DateTime.Today.Date.ToShortDateString();
        //    data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
        //    String nomeRel = "FornecedorLista" + "_" + data + ".pdf";
        //    List<FORNECEDOR> lista = SessionMocks.listaFornecedor;
        //    FORNECEDOR filtro = SessionMocks.filtroFornecedor;
        //    Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        //    Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        //    Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

        //    // Cria documento
        //    Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
        //    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
        //    pdfDoc.Open();

        //    // Linha horizontal
        //    Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line);

        //    // Cabeçalho
        //    PdfPTable table = new PdfPTable(5);
        //    table.WidthPercentage = 100;
        //    table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
        //    table.SpacingBefore = 1f;
        //    table.SpacingAfter = 1f;

        //    PdfPCell cell = new PdfPCell();
        //    cell.Border = 0;
        //    Image image = Image.GetInstance(Server.MapPath("~/Images/5.png"));
        //    image.ScaleAbsolute(50, 50);
        //    cell.AddElement(image);
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph("Fornecedores - Listagem", meuFont2))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_CENTER
        //    };
        //    cell.Border = 0;
        //    cell.Colspan = 4;
        //    table.AddCell(cell);
        //    pdfDoc.Add(table);

        //    // Linha Horizontal
        //    Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line1);
        //    line1 = new Paragraph("  ");
        //    pdfDoc.Add(line1);

        //    // Grid
        //    table = new PdfPTable(new float[] { 70f, 150f, 60f, 60f, 150f, 50f, 50f, 20f});
        //    table.WidthPercentage = 100;
        //    table.HorizontalAlignment = 0;
        //    table.SpacingBefore = 1f;
        //    table.SpacingAfter = 1f;

        //    cell = new PdfPCell(new Paragraph("Fornecedores selecionados pelos parametros de filtro abaixo", meuFont1))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.Colspan = 8;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph("Categoria", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Nome", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("CPF", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("CNPJ", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("E-Mail", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Telefone", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Cidade", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("UF", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);

        //    foreach (FORNECEDOR item in lista)
        //    {
        //        cell = new PdfPCell(new Paragraph(item.CATEGORIA_FORNECEDOR.CAFO_NM_NOME, meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(item.FORN_NM_NOME, meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        table.AddCell(cell);
        //        if (item.FORN_NR_CPF != null)
        //        {
        //            cell = new PdfPCell(new Paragraph(item.FORN_NR_CPF, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        else
        //        {
        //            cell = new PdfPCell(new Paragraph("-", meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        if (item.FORN_NR_CNPJ != null)
        //        {
        //            cell = new PdfPCell(new Paragraph(item.FORN_NR_CNPJ, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        else
        //        {
        //            cell = new PdfPCell(new Paragraph("-", meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        cell = new PdfPCell(new Paragraph(item.FORN_NM_EMAIL, meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        table.AddCell(cell);
        //        if (item.FORN_NM_TELEFONES != null)
        //        {
        //            cell = new PdfPCell(new Paragraph(item.FORN_NM_TELEFONES, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        else
        //        {
        //            cell = new PdfPCell(new Paragraph("-", meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        if (item.FORN_NM_CIDADE != null)
        //        {
        //            cell = new PdfPCell(new Paragraph(item.FORN_NM_CIDADE, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        else
        //        {
        //            cell = new PdfPCell(new Paragraph("-", meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        if (item.UF != null)
        //        {
        //            cell = new PdfPCell(new Paragraph(item.UF.UF_SG_SIGLA, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        else
        //        {
        //            cell = new PdfPCell(new Paragraph("-", meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //    }
        //    pdfDoc.Add(table);

        //    // Linha Horizontal
        //    Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line2);

        //    // Rodapé
        //    Chunk chunk1 = new Chunk("Parâmetros de filtro: ", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
        //    pdfDoc.Add(chunk1);

        //    String parametros = String.Empty;
        //    Int32 ja = 0;
        //    if (filtro != null)
        //    {
        //        if (filtro.CAFO_CD_ID > 0)
        //        {
        //            parametros += "Categoria: " + filtro.CAFO_CD_ID;
        //            ja = 1;
        //        }
        //        if (filtro.FORN_CD_ID > 0)
        //        {
        //            FORNECEDOR cli = fornApp.GetItemById(filtro.FORN_CD_ID);
        //            if (ja == 0)
        //            {
        //                parametros += "Nome: " + cli.FORN_NM_NOME;
        //                ja = 1;
        //            }
        //            else
        //            {
        //                parametros +=  " e Nome: " + cli.FORN_NM_NOME;
        //            }
        //        }
        //        if (filtro.FORN_NR_CPF != null)
        //        {
        //            if (ja == 0)
        //            {
        //                parametros += "CPF: " + filtro.FORN_NR_CPF;
        //                ja = 1;
        //            }
        //            else
        //            {
        //                parametros += " e CPF: " + filtro.FORN_NR_CPF;
        //            }
        //        }
        //        if (filtro.FORN_NR_CNPJ != null)
        //        {
        //            if (ja == 0)
        //            {
        //                parametros += "CNPJ: " + filtro.FORN_NR_CNPJ;
        //                ja = 1;
        //            }
        //            else
        //            {
        //                parametros += " e CNPJ: " + filtro.FORN_NR_CNPJ;
        //            }
        //        }
        //        if (filtro.FORN_NM_EMAIL != null)
        //        {
        //            if (ja == 0)
        //            {
        //                parametros += "E-Mail: " + filtro.FORN_NM_EMAIL;
        //                ja = 1;
        //            }
        //            else
        //            {
        //                parametros += " e E-Mail: " + filtro.FORN_NM_EMAIL;
        //            }
        //        }
        //        if (filtro.FORN_NM_CIDADE != null)
        //        {
        //            if (ja == 0)
        //            {
        //                parametros += "Cidade: " + filtro.FORN_NM_CIDADE;
        //                ja = 1;
        //            }
        //            else
        //            {
        //                parametros += " e Cidade: " + filtro.FORN_NM_CIDADE;
        //            }
        //        }
        //        if (filtro.UF != null)
        //        {
        //            if (ja == 0)
        //            {
        //                parametros += "UF: " + filtro.UF.UF_SG_SIGLA;
        //                ja = 1;
        //            }
        //            else
        //            {
        //                parametros += " e UF: " + filtro.UF.UF_SG_SIGLA;
        //            }
        //        }
        //        if (ja == 0)
        //        {
        //            parametros = "Nenhum filtro definido.";
        //        }
        //    }
        //    else
        //    {
        //        parametros = "Nenhum filtro definido.";
        //    }
        //    Chunk chunk = new Chunk(parametros, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK));
        //    pdfDoc.Add(chunk);

        //    // Linha Horizontal
        //    Paragraph line3 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line3);

        //    // Finaliza
        //    pdfWriter.CloseStream = false;
        //    pdfDoc.Close();
        //    Response.Buffer = true;
        //    Response.ContentType = "application/pdf";
        //    Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
        //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //    Response.Write(pdfDoc);
        //    Response.End();

        //    return RedirectToAction("MontarTelaCliente");
        //}

        //public ActionResult GerarRelatorioDetalhe()
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }
        //    // Prepara geração
        //    FORNECEDOR aten = fornApp.GetItemById(SessionMocks.idVolta);
        //    String data = DateTime.Today.Date.ToShortDateString();
        //    data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
        //    String nomeRel = "Fornecedor" + aten.FORN_CD_ID.ToString() + "_" + data + ".pdf";
        //    Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        //    Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        //    Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        //    Font meuFontBold = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

        //    // Cria documento
        //    Document pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);
        //    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
        //    pdfDoc.Open();

        //    // Linha horizontal
        //    Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line1);

        //    // Cabeçalho
        //    PdfPTable table = new PdfPTable(5);
        //    table.WidthPercentage = 100;
        //    table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
        //    table.SpacingBefore = 1f;
        //    table.SpacingAfter = 1f;

        //    PdfPCell cell = new PdfPCell();
        //    cell.Border = 0;
        //    Image image = Image.GetInstance(Server.MapPath("~/Images/5.png"));
        //    image.ScaleAbsolute(50, 50);
        //    cell.AddElement(image);
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph("FORNECEDOR - Detalhes", meuFont2))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_CENTER
        //    };
        //    cell.Border = 0;
        //    cell.Colspan = 4;
        //    table.AddCell(cell);

        //    pdfDoc.Add(table);

        //    // Linha Horizontal
        //    line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line1);
        //    line1 = new Paragraph("  ");
        //    pdfDoc.Add(line1);

        //    // Dados Gerais
        //    table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
        //    table.WidthPercentage = 100;
        //    table.HorizontalAlignment = 0;
        //    table.SpacingBefore = 1f;
        //    table.SpacingAfter = 1f;

        //    cell = new PdfPCell(new Paragraph("Dados Gerais", meuFontBold));
        //    cell.Border = 0;
        //    cell.Colspan = 4;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph("Tipo de Pessoa: " + aten.TIPO_PESSOA.TIPE_NM_NOME, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph("Filial: " + aten.FILIAL.FILI_NM_NOME, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph("Categoria: " + aten.CATEGORIA_FORNECEDOR.CAFO_NM_NOME, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph(" ", meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);


        //    cell = new PdfPCell(new Paragraph("Nome: " + aten.FORN_NM_NOME, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 2;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Razão Social: " + aten.FORN_NM_RAZAO, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 2;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);

        //    if (aten.FORN_NR_CPF != null)
        //    {
        //        cell = new PdfPCell(new Paragraph("CPF: " + aten.FORN_NR_CPF, meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(" ", meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(" ", meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(" ", meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //    }

        //    if (aten.FORN_NR_CNPJ != null)
        //    {
        //        cell = new PdfPCell(new Paragraph("CNPJ: " + aten.FORN_NR_CNPJ, meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph("Ins.Estadual: " + aten.FORN_NR_INSCRICAO_ESTADUAL, meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(" ", meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(" ", meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //    }
        //    pdfDoc.Add(table);

        //    // Linha Horizontal
        //    line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line1);

        //    // Endereços
        //    table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
        //    table.WidthPercentage = 100;
        //    table.HorizontalAlignment = 0;
        //    table.SpacingBefore = 1f;
        //    table.SpacingAfter = 1f;

        //    cell = new PdfPCell(new Paragraph("Endereço Principal", meuFontBold));
        //    cell.Border = 0;
        //    cell.Colspan = 4;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph("Endereço: " + aten.FORN_NM_ENDERECO, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 2;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Bairro: " + aten.FORN_NM_BAIRRO, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Cidade: " + aten.FORN_NM_CIDADE, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    if (aten.UF != null)
        //    {
        //        cell = new PdfPCell(new Paragraph("UF: " + aten.UF.UF_SG_SIGLA, meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //    }
        //    else
        //    {
        //        cell = new PdfPCell(new Paragraph("UF: -", meuFont));
        //        cell.Border = 0;
        //        cell.Colspan = 1;
        //        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        table.AddCell(cell);
        //    }
        //    cell = new PdfPCell(new Paragraph("CEP: " + aten.FORN_NR_CEP, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph(" ", meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph(" ", meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 1;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    pdfDoc.Add(table);

        //    // Linha Horizontal
        //    line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line1);

        //    // Contatos
        //    table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
        //    table.WidthPercentage = 100;
        //    table.HorizontalAlignment = 0;
        //    table.SpacingBefore = 1f;
        //    table.SpacingAfter = 1f;

        //    cell = new PdfPCell(new Paragraph("Contatos", meuFontBold));
        //    cell.Border = 0;
        //    cell.Colspan = 4;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph("E-Mail: " + aten.FORN_NM_EMAIL, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 2;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Redes Sociais: " + aten.FORN_NM_REDES_SOCIAIS, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 2;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Website: " , meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 2;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Telefones: " + aten.FORN_NM_TELEFONES, meuFont));
        //    cell.Border = 0;
        //    cell.Colspan = 2;
        //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    table.AddCell(cell);
        //    pdfDoc.Add(table);

        //    // Lista de Contatos
        //    if (aten.FORNECEDOR_CONTATO.Count > 0)
        //    {
        //        table = new PdfPTable(new float[] { 120f, 100f, 120f, 100f, 50f });
        //        table.WidthPercentage = 100;
        //        table.HorizontalAlignment = 0;
        //        table.SpacingBefore = 1f;
        //        table.SpacingAfter = 1f;

        //        cell = new PdfPCell(new Paragraph("Nome", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph("Cargo", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph("E-Mail", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph("Telefone", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph("Ativo", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);

        //        foreach (FORNECEDOR_CONTATO item in aten.FORNECEDOR_CONTATO)
        //        {
        //            cell = new PdfPCell(new Paragraph(item.FOCO_NM_NOME, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //            cell = new PdfPCell(new Paragraph(item.FOCO_NM_CARGO, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //            cell = new PdfPCell(new Paragraph(item.FOCO_NM_EMAIL, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //            cell = new PdfPCell(new Paragraph(item.FOCO_NR_TELEFONES, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //            if (item.FOCO_IN_ATIVO == 1)
        //            {
        //                cell = new PdfPCell(new Paragraph("Ativo", meuFont))
        //                {
        //                    VerticalAlignment = Element.ALIGN_MIDDLE,
        //                    HorizontalAlignment = Element.ALIGN_LEFT
        //                };
        //                table.AddCell(cell);
        //            }
        //            else
        //            {
        //                cell = new PdfPCell(new Paragraph("Inativo", meuFont))
        //                {
        //                    VerticalAlignment = Element.ALIGN_MIDDLE,
        //                    HorizontalAlignment = Element.ALIGN_LEFT
        //                };
        //                table.AddCell(cell);
        //            }
        //        }
        //        pdfDoc.Add(table);
        //    }

        //    // Linha Horizontal
        //    line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line1);

        //    // Observações
        //    Chunk chunk1 = new Chunk("Observações: " + aten.FORN_TX_OBSERVACOES, FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK));
        //    pdfDoc.Add(chunk1);

        //    // Pedidos de Venda
        //    if (aten.ITEM_PEDIDO_COMPRA.Count > 0)
        //    {
        //        // Linha Horizontal
        //        line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //        pdfDoc.Add(line1);

        //        // Lista de Pedidos
        //        table = new PdfPTable(new float[] { 120f, 80f, 80f, 80f});
        //        table.WidthPercentage = 100;
        //        table.HorizontalAlignment = 0;
        //        table.SpacingBefore = 1f;
        //        table.SpacingAfter = 1f;

        //        cell = new PdfPCell(new Paragraph("Nome", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph("Data", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph("Nome Produto", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph("Quantidade", meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        table.AddCell(cell);

        //        foreach (ITEM_PEDIDO_COMPRA item in aten.ITEM_PEDIDO_COMPRA)
        //        {
        //            cell = new PdfPCell(new Paragraph(item.PEDIDO_COMPRA.PECO_NM_NOME, meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //            cell = new PdfPCell(new Paragraph(item.PEDIDO_COMPRA.PECO_DT_DATA.Value.ToShortDateString(), meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //            if (item.PRODUTO != null)
        //            {
        //                cell = new PdfPCell(new Paragraph(item.PRODUTO.PROD_NM_NOME, meuFont))
        //                {
        //                    VerticalAlignment = Element.ALIGN_MIDDLE,
        //                    HorizontalAlignment = Element.ALIGN_LEFT
        //                };
        //                table.AddCell(cell);
        //            }
        //            if (item.MATERIA_PRIMA != null)
        //            {
        //                cell = new PdfPCell(new Paragraph(item.MATERIA_PRIMA.MAPR_NM_NOME, meuFont))
        //                {
        //                    VerticalAlignment = Element.ALIGN_MIDDLE,
        //                    HorizontalAlignment = Element.ALIGN_LEFT
        //                };
        //                table.AddCell(cell);
        //            }
        //            cell = new PdfPCell(new Paragraph(item.ITPC_QN_QUANTIDADE.ToString(), meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            table.AddCell(cell);
        //        }
        //        pdfDoc.Add(table);
        //    }

        //    // Finaliza
        //    pdfWriter.CloseStream = false;
        //    pdfDoc.Close();
        //    Response.Buffer = true;
        //    Response.ContentType = "application/pdf";
        //    Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
        //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //    Response.Write(pdfDoc);
        //    Response.End();

        //    return RedirectToAction("VoltarAnexoCliente");
        //}

        [HttpGet]
        public ActionResult EditarPrescricao(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("VoltarAnexoPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            PACIENTE_PRESCRICAO item = baseApp.GetPrescricaoById(id);
            PacientePrescricaoViewModel vm = Mapper.Map<PACIENTE_PRESCRICAO, PacientePrescricaoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPrescricao(PacientePrescricaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    PACIENTE_PRESCRICAO item = Mapper.Map<PacientePrescricaoViewModel, PACIENTE_PRESCRICAO>(vm);
                    Int32 volta = baseApp.EditPrescricao(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoPaciente");
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
        public ActionResult ExcluirPrescricao(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("VoltarAnexoPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            PACIENTE_PRESCRICAO item = baseApp.GetPrescricaoById(id);
            item.PRES_IN_ATIVO = 0;
            Int32 volta = baseApp.EditPrescricao(item);
            return RedirectToAction("VoltarAnexoPaciente");
        }

        [HttpGet]
        public ActionResult ReativarPrescricao(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("VoltarAnexoPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            PACIENTE_PRESCRICAO item = baseApp.GetPrescricaoById(id);
            item.PRES_IN_ATIVO = 1;
            Int32 volta = baseApp.EditPrescricao(item);
            return RedirectToAction("VoltarAnexoPaciente");
        }

        [HttpGet]
        public ActionResult IncluirPrescricao()
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("VoltarAnexoPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            PACIENTE pac = baseApp.GetItemById((Int32)Session["IdVolta"]);
            PACIENTE_PRESCRICAO item = new PACIENTE_PRESCRICAO();
            PacientePrescricaoViewModel vm = Mapper.Map<PACIENTE_PRESCRICAO, PacientePrescricaoViewModel>(item);
            vm.PACI_CD_ID = (Int32)Session["IdVolta"]; ;
            vm.PRES_IN_ATIVO = 1;
            vm.PRES_NM_NOME = "Prescrição para " + pac.PACI_NM_NOME;
            vm.PRES_DT_DATA = DateTime.Today.Date;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirPrescricao(PacientePrescricaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    PACIENTE_PRESCRICAO item = Mapper.Map<PacientePrescricaoViewModel, PACIENTE_PRESCRICAO>(vm);
                    Int32 volta = baseApp.CreatePrescricao(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoPaciente");
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
        public ActionResult EditarRecomendacao(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("VoltarAnexoPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            PACIENTE_RECOMENDACAO item = baseApp.GetRecomendacaoById(id);
            PacienteRecomendacaoViewModel vm = Mapper.Map<PACIENTE_RECOMENDACAO, PacienteRecomendacaoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarRecomendacao(PacienteRecomendacaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    PACIENTE_RECOMENDACAO item = Mapper.Map<PacienteRecomendacaoViewModel, PACIENTE_RECOMENDACAO>(vm);
                    Int32 volta = baseApp.EditRecomendacao(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoPaciente");
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
        public ActionResult ExcluirRecomendacao(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("VoltarAnexoPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            PACIENTE_RECOMENDACAO item = baseApp.GetRecomendacaoById(id);
            item.RECO_IN_ATIVO = 0;
            Int32 volta = baseApp.EditRecomendacao(item);
            return RedirectToAction("VoltarAnexoPaciente");
        }

        [HttpGet]
        public ActionResult ReativarRecomendacao(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("VoltarAnexoPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            PACIENTE_RECOMENDACAO item = baseApp.GetRecomendacaoById(id);
            item.RECO_IN_ATIVO = 1;
            Int32 volta = baseApp.EditRecomendacao(item);
            return RedirectToAction("VoltarAnexoPaciente");
        }

        [HttpGet]
        public ActionResult IncluirRecomendacao()
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    Session["MensPaciente"] = 2;
                    return RedirectToAction("VoltarAnexoPaciente", "Paciente");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            PACIENTE pac = baseApp.GetItemById((Int32)Session["IdVolta"]);
            PACIENTE_RECOMENDACAO item = new PACIENTE_RECOMENDACAO();
            PacienteRecomendacaoViewModel vm = Mapper.Map<PACIENTE_RECOMENDACAO, PacienteRecomendacaoViewModel>(item);
            vm.PACI_CD_ID = (Int32)Session["IdVolta"]; ;
            vm.RECO_IN_ATIVO = 1;
            vm.RECO_NM_NOME = "Recomendação para " + pac.PACI_NM_NOME;
            vm.RECO_DT_DATA = DateTime.Today.Date;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirRecomendacao(PacienteRecomendacaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    PACIENTE_RECOMENDACAO item = Mapper.Map<PacienteRecomendacaoViewModel, PACIENTE_RECOMENDACAO>(vm);
                    Int32 volta = baseApp.CreateRecomendacao(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoPaciente");
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
        public ActionResult IncluirAcompanhamento()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 id = (Int32)Session["IdVolta"];
            PACIENTE item = baseApp.GetItemById(id);
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
            PACIENTE_ACOMPANHAMENTO coment = new PACIENTE_ACOMPANHAMENTO();
            PacienteAcompanhamentoViewModel vm = Mapper.Map<PACIENTE_ACOMPANHAMENTO, PacienteAcompanhamentoViewModel>(coment);
            vm.PAAC_DT_DATA = DateTime.Today;
            vm.PAAC_IN_ATIVO = 1;
            vm.PACI_CD_ID = item.PACI_CD_ID;
            vm.USUARIO = usuarioLogado;
            vm.USUA_CD_ID = usuarioLogado.USUA_CD_ID;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirAcompanhamento(PacienteAcompanhamentoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdVolta"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PACIENTE_ACOMPANHAMENTO item = Mapper.Map<PacienteAcompanhamentoViewModel, PACIENTE_ACOMPANHAMENTO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    PACIENTE not = baseApp.GetItemById(idNot);

                    item.USUARIO = null;
                    not.PACIENTE_ACOMPANHAMENTO.Add(item);
                    objetoAntes = not;
                    Int32 volta = baseApp.ValidateEdit(not, objetoAntes);

                    // Verifica retorno

                    // Sucesso
                    return RedirectToAction("VoltarAnexoPaciente");
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