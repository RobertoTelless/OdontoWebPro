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
using Canducci.Zip;
using System.Collections;

namespace SystemBRPresentation.Controllers
{
    public class MatrizFilialController : Controller
    {
        private readonly IMatrizAppService matrizApp;
        private readonly ILogAppService logApp;
        private readonly IFilialAppService filialApp;
        private String msg;
        private Exception exception;
        MATRIZ objMatriz = new MATRIZ();
        MATRIZ objMatrizAntes = new MATRIZ();
        FILIAL objFilial = new FILIAL();
        FILIAL objFilialAntes = new FILIAL();
        List<FILIAL> listaMasterFilial = new List<FILIAL>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public MatrizFilialController(IMatrizAppService matrizApps, ILogAppService logApps, IFilialAppService filialApps)
        {
            matrizApp = matrizApps;
            logApp = logApps;
            filialApp = filialApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            MATRIZ item = new MATRIZ();
            MatrizViewModel vm = Mapper.Map<MATRIZ, MatrizViewModel>(item);
            return View(vm);
        }

        public ActionResult Voltar()
        {
            return RedirectToAction("CarregarAdmin", "BaseAdmin");
        }

        [HttpGet]
        public ActionResult MontarTelaMatriz()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            usuario = SessionMocks.UserCredentials;

            // Carrega listas
            if (SessionMocks.Matriz == null)
            {
                objMatriz = matrizApp.GetAllItens().FirstOrDefault();
                SessionMocks.Matriz = objMatriz;
            }
            ViewBag.Title = "Matriz";
            ViewBag.TipoPessoa = new SelectList(matrizApp.GetAllTipoPessoa(), "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(matrizApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            SessionMocks.idVolta = SessionMocks.Matriz.MATR_CD_ID;

            // Abre view
            MatrizViewModel vm = Mapper.Map<MATRIZ, MatrizViewModel>(SessionMocks.Matriz);
            objMatrizAntes = SessionMocks.Matriz;
            SessionMocks.voltaCEP = 1;
            ViewBag.TP = vm.TIPE_CD_ID;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MontarTelaMatriz(MatrizViewModel vm)
        {
            ViewBag.TipoPessoa = new SelectList(matrizApp.GetAllTipoPessoa(), "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(matrizApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    MATRIZ item = Mapper.Map<MatrizViewModel, MATRIZ>(vm);
                    Int32 volta = matrizApp.ValidateEdit(item, objMatrizAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    objMatriz = new MATRIZ();
                    SessionMocks.Matriz = null;
                    return RedirectToAction("MontarTelaMatriz");
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
        public ActionResult IncluirFilial()
        {
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara listas
            ViewBag.TipoPessoa = new SelectList(matrizApp.GetAllTipoPessoa(), "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(matrizApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");

            // Prepara view
            FILIAL item = new FILIAL();
            FilialViewModel vm = Mapper.Map<FILIAL, FilialViewModel>(item);
            vm.MATR_CD_ID = SessionMocks.Matriz.MATR_CD_ID;
            vm.FILI_IN_ATIVO = 1;
            vm.FILI_DT_CADASTRO = DateTime.Today;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirFilial(FilialViewModel vm)
        {
            ViewBag.TipoPessoa = new SelectList(matrizApp.GetAllTipoPessoa(), "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(matrizApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    FILIAL item = Mapper.Map<FilialViewModel, FILIAL>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = filialApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Carrega logo e processa alteracao
                    item.FILI_AQ_LOGOTIPO = "~/Imagens/Base/FotoBase.jpg";
                    volta = filialApp.ValidateEdit(item, item, usuarioLogado);

                    // Cria pastas
                    String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Filial/" + item.FILI_CD_ID.ToString() + "/Logo/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    objMatriz = new MATRIZ();
                    SessionMocks.Matriz = null;
                    return RedirectToAction("MontarTelaMatriz");
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
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara listas
            ViewBag.TipoPessoa = new SelectList(matrizApp.GetAllTipoPessoa(), "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(matrizApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");

            // Prepara view
            FILIAL item = filialApp.GetItemById(id);
            objFilialAntes = item;
            FilialViewModel vm = Mapper.Map<FILIAL, FilialViewModel>(item);
            SessionMocks.idVolta = id;
            return View(vm); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarFilial(FilialViewModel vm)
        {
            ViewBag.TipoPessoa = new SelectList(matrizApp.GetAllTipoPessoa(), "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(matrizApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    FILIAL item = Mapper.Map<FilialViewModel, FILIAL>(vm);
                    Int32 volta = filialApp.ValidateEdit(item, objFilialAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    objMatriz = new MATRIZ();
                    SessionMocks.Matriz = null;
                    return RedirectToAction("MontarTelaMatriz");
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
        public ActionResult ExcluirFilial(Int32 id)
        {
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            FILIAL item = filialApp.GetItemById(id);
            FilialViewModel vm = Mapper.Map<FILIAL, FilialViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirFilial(FilialViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                FILIAL item = Mapper.Map<FilialViewModel, FILIAL>(vm);
                Int32 volta = filialApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0061", CultureInfo.CurrentCulture));
                    return View(vm);
                }

                // Sucesso
                objMatriz = new MATRIZ();
                SessionMocks.Matriz = null;
                return RedirectToAction("MontarTelaMatriz");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarFilial(Int32 id)
        {
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "USU")
                {
                    return RedirectToAction("CarregarDashboardInicial", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Prepara view
            FILIAL item = filialApp.GetItemById(id);
            FilialViewModel vm = Mapper.Map<FILIAL, FilialViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarFilial(FilialViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                FILIAL item = Mapper.Map<FilialViewModel, FILIAL>(vm);
                Int32 volta = filialApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                objMatriz = new MATRIZ();
                SessionMocks.Matriz = null;
                return RedirectToAction("MontarTelaMatriz");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult VoltarBaseFilial()
        {
            SessionMocks.Filiais = filialApp.GetAllItens();
            objMatriz = new MATRIZ();
            SessionMocks.Matriz = null;
            return RedirectToAction("MontarTelaMatriz");
        }

        [HttpPost]
        public ActionResult UploadLogoMatriz(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarBaseFilial");
            }

            MATRIZ item = matrizApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Matriz/" + item.MATR_CD_ID.ToString() + "/Logo/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.MATR_AQ_LOGOTIPO = "~" + caminho + fileName;
            objMatrizAntes = item;
            Int32 volta = matrizApp.ValidateEdit(item, objMatrizAntes, usu);
            SessionMocks.Matriz = null;
            return RedirectToAction("MontarTelaMatriz");
        }

        [HttpPost]
        public ActionResult UploadLogoFilial(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarBaseFilial");
            }

            FILIAL item = filialApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Filial/" + item.FILI_CD_ID.ToString() + "/Logo/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.FILI_AQ_LOGOTIPO = "~" + caminho + fileName;
            objFilialAntes = item;
            Int32 volta = filialApp.ValidateEdit(item, objFilialAntes, usu);
            SessionMocks.Matriz = null;
            return RedirectToAction("EditarFilial", new { id = SessionMocks.idVolta });
        }

        [HttpGet]
        public ActionResult BuscarCEPMatriz()
        {
            // Prepara view
            if (SessionMocks.voltaCEP == 1)
            {
                //CLIENTE item = baseApp.GetItemById(SessionMocks.idVolta);
                MATRIZ item = SessionMocks.Matriz;
                MatrizViewModel vm = Mapper.Map<MATRIZ, MatrizViewModel>(item);
                vm.MATR_NR_CEP_BUSCA = String.Empty;
                vm.MATR_SG_UF = vm.UF.UF_SG_SIGLA;
                return View(vm);
            }
            else
            {
                MatrizViewModel vm = Mapper.Map<MATRIZ, MatrizViewModel>(SessionMocks.Matriz);
                vm.MATR_NR_CEP_BUSCA = String.Empty;
                return View(vm);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult BuscarCEPMatriz(MatrizViewModel vm)
        {
            try
            {
                // Atualiza cliente
                MATRIZ item = SessionMocks.Matriz;
                MATRIZ cli = new MATRIZ();
                cli.ASSI_CD_ID = item.ASSI_CD_ID;
                cli.ASSI_CD_ID = item.ASSI_CD_ID;
                cli.MATR_AQ_LOGOTIPO = item.MATR_AQ_LOGOTIPO;
                cli.MATR_CD_ID = item.MATR_CD_ID;
                cli.MATR_DT_CADASTRO = item.MATR_DT_CADASTRO;
                cli.MATR_IN_ATIVO = item.MATR_IN_ATIVO;
                cli.MATR_IN_IR_ISENTO = item.MATR_IN_IR_ISENTO;
                cli.MATR_NM_BAIRRO = item.MATR_NM_BAIRRO;
                cli.MATR_NM_CIDADE = item.MATR_NM_CIDADE;
                cli.MATR_NM_CONTATOS = item.MATR_NM_CONTATOS;
                cli.MATR_NM_EMAIL = item.MATR_NM_EMAIL;
                cli.MATR_NM_ENDERECO = item.MATR_NM_ENDERECO;
                cli.MATR_NM_NOME = item.MATR_NM_NOME;
                cli.MATR_NM_RAZAO = item.MATR_NM_RAZAO;
                cli.MATR_NM_WEBSITE = item.MATR_NM_WEBSITE;
                cli.MATR_NR_CELULAR = item.MATR_NR_CELULAR;
                cli.MATR_NR_CEP = item.MATR_NR_CEP;
                cli.MATR_NR_CNAE = item.MATR_NR_CNAE;
                cli.MATR_NR_CNPJ = item.MATR_NR_CNPJ;
                cli.MATR_NR_CPF = item.MATR_NR_CPF;
                cli.MATR_NR_INSCRICAO_ESTADUAL = item.MATR_NR_INSCRICAO_MUNICIPAL;
                cli.MATR_NR_RG = item.MATR_NR_RG;
                cli.MATR_NR_TELEFONES = item.MATR_NR_TELEFONES;
                cli.TIPE_CD_ID = item.TIPE_CD_ID;
                cli.UF_CD_ID = item.UF_CD_ID;
                cli.CRTR_CD_ID = item.CRTR_CD_ID;

                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                Int32 volta = matrizApp.ValidateEdit(cli, cli, usuarioLogado);

                // Verifica retorno

                // Sucesso
                return RedirectToAction("MontarTelaMatriz");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult PesquisaCEP(MatrizViewModel itemVolta)
        {
            // Chama servico ECT
            MATRIZ cli = matrizApp.GetItemById(SessionMocks.idVolta);
            MatrizViewModel item = Mapper.Map<MATRIZ, MatrizViewModel>(cli);

            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            String cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(itemVolta.MATR_NR_CEP_BUSCA);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza            
            item.MATR_NM_ENDERECO = end.Address + "/" + end.Complement;
            item.MATR_NM_BAIRRO = end.District;
            item.MATR_NM_CIDADE = end.City;
            item.MATR_SG_UF = end.Uf;
            item.UF_CD_ID = matrizApp.GetUFbySigla(end.Uf).UF_CD_ID;
            item.MATR_NR_CEP = itemVolta.MATR_NR_CEP_BUSCA;

            // Retorna
            SessionMocks.voltaCEP = 2;
            SessionMocks.Matriz = Mapper.Map<MatrizViewModel, MATRIZ>(item);
            return RedirectToAction("BuscarCEPMatriz");
        }

        [HttpPost]
        public JsonResult PesquisaCEP_Javascript(String cep, int tipoEnd)
        {
            // Chama servico ECT
            //Address end = ExternalServices.ECT_Services.GetAdressCEP(item.CLIE_NR_CEP_BUSCA);
            //Endereco end = ExternalServices.ECT_Services.GetAdressCEPService(item.CLIE_NR_CEP_BUSCA);
            MATRIZ cli = matrizApp.GetItemById(SessionMocks.idVolta);

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
                hash.Add("FILI_SG_UF", end.Uf);
                hash.Add("UF_CD_ID", matrizApp.GetUFbySigla(end.Uf).UF_CD_ID);
                hash.Add("FILI_NR_CEP", cep);
            }
            else if (tipoEnd == 2)
            {
                hash.Add("MATR_NM_ENDERECO", end.Address + "/" + end.Complement);
                hash.Add("MATR_NM_BAIRRO", end.District);
                hash.Add("MATR_NM_CIDADE", end.City);
                hash.Add("MATR_SG_UF", end.Uf);
                hash.Add("UF_CD_ID", matrizApp.GetUFbySigla(end.Uf).UF_CD_ID);
                hash.Add("MATR_NR_CEP", cep);
            }

            // Retorna
            SessionMocks.voltaCEP = 2;
            return Json(hash);
        }

    }
}