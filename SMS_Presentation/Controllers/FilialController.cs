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
using Canducci.Zip;
using System.Collections;
using ApplicationServices.Services;

namespace Odonto.Controllers
{
    public class FilialController : Controller
    {
        private readonly IFilialAppService baseApp;
        private readonly ILogAppService logApp;
        private String msg;
        private Exception exception;
        FILIAL objeto = new FILIAL();
        FILIAL objetoAntes = new FILIAL();
        List<FILIAL> listaMaster = new List<FILIAL>();
        String extensao;

        public FilialController(IFilialAppService baseApps, ILogAppService logApps)
        {
            baseApp = baseApps;
            logApp = logApps;
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

        public ActionResult VoltarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaFilial");
        }

        [HttpGet]
        public ActionResult MontarTelaFilial()
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
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idMatriz = (Int32)Session["IdMatriz"];

            // Carrega listas
            if ((List<FILIAL>)Session["ListaFilial"] == null)
            {
                listaMaster = baseApp.GetAllItens(idMatriz);
                Session["ListaFilial"] = listaMaster;
                Session["FiltroFilial"] = null;
            }
            ViewBag.Listas = (List<FILIAL>)Session["ListaFilial"];
            ViewBag.Title = "Filiais";

            // Indicadores
            ViewBag.Filiais = ((List<FILIAL>)Session["ListaFilial"]).Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensFilial"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensFilial"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensFilial"] = 0;
            Session["VoltaFilial"] = 1;
            objeto = new FILIAL();
            return View(objeto);
        }

        [HttpGet]
        public ActionResult MontarTelaFilialCard()
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
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idMatriz = (Int32)Session["IdMatriz"];

            // Carrega listas
            if ((List<FILIAL>)Session["ListaFilial"] == null)
            {
                listaMaster = baseApp.GetAllItens(idMatriz);
                Session["ListaFilial"] = listaMaster;
                Session["FiltroFilial"] = null;
            }
            ViewBag.Listas = (List<FILIAL>)Session["ListaFilial"];
            ViewBag.Title = "Filiais";

            // Indicadores
            ViewBag.Filiais = ((List<FILIAL>)Session["ListaFilial"]).Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            // Mensagem
            if ((Int32)Session["MensFilial"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensFilial"] == 2)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensFilial"] = 0;
            Session["VoltaFilial"] = 2;
            objeto = new FILIAL();
            return View(objeto);
        }

        public ActionResult RetirarFiltroFilial()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaFilial"] = null;
            listaMaster = new List<FILIAL>();
            if ((Int32)Session["VoltaFilial"] == 2)
            {
                return RedirectToAction("MontarTelaFilialCard");
            }
            return RedirectToAction("MontarTelaFilial");
        }

        public ActionResult MostrarTudoFilial()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["ListaFilial"] = listaMaster;
            if ((Int32)Session["VoltaFilial"] == 2)
            {
                return RedirectToAction("MontarTelaFilialCard");
            }
            return RedirectToAction("MontarTelaFilial");
        }

        [HttpGet]
        public ActionResult VerFilial(Int32 id)
        {
            // Prepara view
            // Executa a operação
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            FILIAL item = baseApp.GetItemById(id);
            objetoAntes = item;
            FilialViewModel vm = Mapper.Map<FILIAL, FilialViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult IncluirFilial()
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
                    Session["MensFilial"] = 2;
                    return RedirectToAction("MontarTelaFilial", "Filial");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara listas
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            List<SelectListItem> tipoPessoa= new List<SelectListItem>();
            tipoPessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            tipoPessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.TiposPessoa = new SelectList(tipoPessoa, "Value", "Text");
            ViewBag.UFs = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");

            // Prepara view
            FILIAL item = new FILIAL();
            FilialViewModel vm = Mapper.Map<FILIAL, FilialViewModel>(item);
            vm.FILI_DT_CADASTRO = DateTime.Today.Date;
            vm.FILI_IN_ATIVO = 1;
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            vm.MATR_CD_ID = idMatriz;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirFilial(FilialViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            List<SelectListItem> tipoPessoa = new List<SelectListItem>();
            tipoPessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            tipoPessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.TiposPessoa = new SelectList(tipoPessoa, "Value", "Text");
            ViewBag.UFs = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    Int32 idAss = (Int32)Session["IdAssinante"];
                    FILIAL item = Mapper.Map<FilialViewModel, FILIAL>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = baseApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0042", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Carrega foto e processa alteracao
                    item.FILI_AQ_LOGOTIPO = "~/Images/LogoBase.png";
                    volta = baseApp.ValidateEdit(item, item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + idAss.ToString() + "/Filial/" + item.FILI_CD_ID.ToString() + "/Logos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + idAss.ToString() + "/Filial/" + item.FILI_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMaster = new List<FILIAL>();
                    Session["ListaFilial"] = null;
                    Session["VoltaFilial"] = 1;
                    Session["IdFilialVolta"] = item.FILI_CD_ID;
                    Session["Filial"] = item;
                    Session["MensFilial"] = 0;
                    return RedirectToAction("EditarFilial", new { id = item.FILI_CD_ID });
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
        public ActionResult EditarFilial(Int32 id)
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
                    Session["MensFilial"] = 2;
                    return RedirectToAction("MontarTelaFilial", "Filial");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            List<SelectListItem> tipoPessoa = new List<SelectListItem>();
            tipoPessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            tipoPessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.TiposPessoa = new SelectList(tipoPessoa, "Value", "Text");
            ViewBag.UFs = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");

            FILIAL item = baseApp.GetItemById(id);
            objetoAntes = item;
            Session["Filial"] = item;
            Session["IdVolta"] = id;
            FilialViewModel vm = Mapper.Map<FILIAL, FilialViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarFilial(FilialViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idMatriz = ((MATRIZ)Session["Matriz"]).MATR_CD_ID;
            List<SelectListItem> tipoPessoa = new List<SelectListItem>();
            tipoPessoa.Add(new SelectListItem() { Text = "Pessoa Física", Value = "1" });
            tipoPessoa.Add(new SelectListItem() { Text = "Pessoa Jurídica", Value = "2" });
            ViewBag.TiposPessoa = new SelectList(tipoPessoa, "Value", "Text");
            ViewBag.UFs = new SelectList(baseApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    FILIAL item = Mapper.Map<FilialViewModel, FILIAL>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<FILIAL>();
                    Session["ListaFilial"] = null;
                    Session["MensFilial"] = 0;
                    if ((Int32)Session["VoltaFilial"] == 2)
                    {
                        return RedirectToAction("MontarTelaFilialCard");
                    }
                    return RedirectToAction("MontarTelaFilial");
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
        public ActionResult DesativarFilial(Int32 id)
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
                    Session["MensFilial"] = 2;
                    return RedirectToAction("MontarTelaFilial", "Filial");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            FILIAL item = baseApp.GetItemById(id);
            objetoAntes = (FILIAL)Session["Filial"];
            item.FILI_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateDelete(item, usuario);
            listaMaster = new List<FILIAL>();
            Session["ListaFilial"] = null;
            if ((Int32)Session["VoltaFilial"] == 2)
            {
                return RedirectToAction("MontarTelaFilialCard");
            }
            return RedirectToAction("MontarTelaFilial");
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
                    Session["MensFilial"] = 2;
                    return RedirectToAction("MontarTelaFilial", "Filial");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Executar
            FILIAL item = baseApp.GetItemById(id);
            objetoAntes = (FILIAL)Session["Filial"];
            item.FILI_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateReativar(item, usuario);
            listaMaster = new List<FILIAL>();
            Session["ListaFilial"] = null;
            if ((Int32)Session["VoltaFilial"] == 2)
            {
                return RedirectToAction("MontarTelaFilialCard");
            }
            return RedirectToAction("MontarTelaFilial");
        }

        public ActionResult VoltarAnexoFilial()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Int32 idFilial = (Int32)Session["IdVolta"];
            return RedirectToAction("EditarFilial", new { id = idFilial });
        }

        [HttpPost]
        public ActionResult UploadFotoFilial(HttpPostedFileBase file)
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
            Int32 idVolta= (Int32)Session["IdVolta"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            FILIAL item = baseApp.GetById(idVolta);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoFilial");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Filial/" + item.FILI_CD_ID.ToString() + "/Logos/";
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
                item.FILI_AQ_LOGOTIPO = "~" + caminho + fileName;
                objetoAntes = item;
                Int32 volta = baseApp.ValidateEdit(item, item, usu);
            }
            else
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0021", CultureInfo.CurrentCulture));
            }
            return RedirectToAction("VoltarAnexoFilial");
        }

        //public ActionResult PesquisaCEP(FilialViewModel itemVolta)
        //{
        //    // Chama servico ECT
        //    FILIAL cli = baseApp.GetItemById((Int32)Session["IdVolta"]);
        //    FilialViewModel item = Mapper.Map<FILIAL, FilialViewModel>(cli);

        //    ZipCodeLoad zipLoad = new ZipCodeLoad();
        //    ZipCodeInfo end = new ZipCodeInfo();
        //    ZipCode zipCode = null;
        //    String cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(itemVolta.CLIE_NR_CEP_BUSCA);
        //    if (ZipCode.TryParse(cep, out zipCode))
        //    {
        //        end = zipLoad.Find(zipCode);
        //    }

        //    // Atualiza            
        //    item.FILI_NM_ENDERECO = end.Address + "/" + end.Complement;
        //    item.FILI_NM_BAIRRO = end.District;
        //    item.FILI_NM_CIDADE = end.City;
        //    item.UF_CD_ID = baseApp.GetUFBySigla(end.Uf).UF_CD_ID;
        //    //item.FILI_NR_CEP = itemVolta.CLIE_NR_CEP_BUSCA;

        //    // Retorna
        //    Session["VoltaCEP"] = 2;
        //    Session["Filial"] = Mapper.Map<FilialViewModel, FILIAL>(item);
        //    return RedirectToAction("BuscarCEPFilial");
        //}

        [HttpPost]
        public JsonResult PesquisaCEP_Javascript(String cep, int tipoEnd)
        {
            // Chama servico ECT
            FILIAL cli = baseApp.GetItemById((Int32)Session["IdVolta"]);

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
                hash.Add("FILI_NM_ENDERECO", end.Address + "/" + end.Complement);
                hash.Add("FILI_NM_BAIRRO", end.District);
                hash.Add("FILI_NM_CIDADE", end.City);
                hash.Add("UF_CD_ID", baseApp.GetUFBySigla(end.Uf).UF_CD_ID);
                hash.Add("FILI_NR_CEP", cep);
            }

            // Retorna
            Session["VoltaCEP"] = 2;
            return Json(hash);
        }



    }
}