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
using System.Data.Entity;

namespace SMS_Presentation.Controllers
{
    public class TabelaAuxiliarController : Controller
    {
        private readonly ICategoriaContatoAppService ccApp;
        private readonly ICategoriaNotificacaoAppService cnApp;
        private readonly ILogAppService logApp;
        private readonly IOrigemAppService orApp;
        private readonly IProfissaoAppService prApp;
        private readonly IClubeAppService clApp;

        private String msg;
        private Exception exception;
        CATEGORIA_CONTATO objetoCC = new CATEGORIA_CONTATO();
        CATEGORIA_CONTATO objetoAntesCC = new CATEGORIA_CONTATO();
        List<CATEGORIA_CONTATO> listaMasterCC = new List<CATEGORIA_CONTATO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        CATEGORIA_NOTIFICACAO objetoCN = new CATEGORIA_NOTIFICACAO();
        CATEGORIA_NOTIFICACAO objetoAntesCN = new CATEGORIA_NOTIFICACAO();
        List<CATEGORIA_NOTIFICACAO> listaMasterCN = new List<CATEGORIA_NOTIFICACAO>();
        ORIGEM objetoOR = new ORIGEM();
        ORIGEM objetoAntesOR = new ORIGEM();
        List<ORIGEM> listaMasterOR = new List<ORIGEM>();
        PROFISSAO objetoPR = new PROFISSAO();
        PROFISSAO objetoAntesPR = new PROFISSAO();
        List<PROFISSAO> listaMasterPR = new List<PROFISSAO>();
        CLUBE objetoCL = new CLUBE();
        CLUBE objetoAntesCL = new CLUBE();
        List<CLUBE> listaMasterCL = new List<CLUBE>();


        public TabelaAuxiliarController(ICategoriaContatoAppService ccApps, ICategoriaNotificacaoAppService cnApps, IOrigemAppService orApps, IProfissaoAppService prApps, IClubeAppService clApps)
        {
            ccApp = ccApps;
            cnApp = cnApps;
            orApp = orApps;
            prApp = prApps;
            clApp = clApps;
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

        [HttpGet]
        public ActionResult MontarTelaCategoriaContato()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            usuario = (USUARIO)Session["UserCredentials"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List<CATEGORIA_CONTATO>)Session["ListaCatCont"] == null)
            {
                listaMasterCC = ccApp.GetAllItens();
                Session["ListaCatCont"] = listaMasterCC;
            }
            ViewBag.Listas = (List<CATEGORIA_CONTATO>)Session["ListaCatCont"];
            ViewBag.Title = "Categorias de Contatos";

            // Indicadores
            ViewBag.Itens = ((List<CATEGORIA_CONTATO>)Session["ListaCatCont"]).Count;

            // Mensagem
            if ((Int32)Session["MensCatCont"] == 2)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensCatCont"] = 0;
            objetoCC = new CATEGORIA_CONTATO();
            return View(objetoCC);
        }

        public ActionResult MostrarTudoCategoriaContato()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            listaMasterCC = ccApp.GetAllItensAdm();
            Session["ListaCatCont"] = listaMasterCC;
            return RedirectToAction("MontarTelaCategoriaContato");
        }

        public ActionResult VoltarBaseCategoriaContato()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaCatCont"] = ccApp.GetAllItens();
            return RedirectToAction("MontarTelaCategoriaContato");
        }

        [HttpGet]
        public ActionResult IncluirCategoriaContato()
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            // Mensagem
            if ((Int32)Session["MensCatCont"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0037", CultureInfo.CurrentCulture));
            }

            Session["MensCatCont"] = 0;
            CATEGORIA_CONTATO item = new CATEGORIA_CONTATO();
            CategoriaContatoViewModel vm = Mapper.Map<CATEGORIA_CONTATO, CategoriaContatoViewModel>(item);
            vm.CACO_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirCategoriaContato(CategoriaContatoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CATEGORIA_CONTATO item = Mapper.Map<CategoriaContatoViewModel, CATEGORIA_CONTATO>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = ccApp.ValidateCreate(item, usuario, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensCatCont"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0037", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterCC = new List<CATEGORIA_CONTATO>();
                    Session["ListaCatCont"] = null;
                    Session["MensCatCont"] = 0;
                    return RedirectToAction("MontarTelaCategoriaContato");
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
        public ActionResult EditarCategoriaContato(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Mensagem
            if ((Int32)Session["MensCatCont"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0037", CultureInfo.CurrentCulture));
            }

            CATEGORIA_CONTATO item = ccApp.GetItemById(id);
            Session["MensCatCont"] = 0;
            Session["Antes"] = item;
            Session["CatContato"] = item;
            Session["IdVolta"] = id;
            CategoriaContatoViewModel vm = Mapper.Map<CATEGORIA_CONTATO, CategoriaContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarCategoriaContato(CategoriaContatoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    CATEGORIA_CONTATO item = Mapper.Map<CategoriaContatoViewModel, CATEGORIA_CONTATO>(vm);
                    Int32 volta = ccApp.ValidateEdit(item, (CATEGORIA_CONTATO)Session["Antes"], usuario, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensCatCont"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0037", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterCC = new List<CATEGORIA_CONTATO>();
                    Session["MensCatCont"] = 0;
                    Session["ListaCatCont"] = null;
                    return RedirectToAction("MontarTelaCategoriaContato");
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
        public ActionResult ExcluirCategoriaContato(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            CATEGORIA_CONTATO item = ccApp.GetItemById(id);
            objetoAntesCC = (CATEGORIA_CONTATO)Session["CatContato"];
            item.CACO_IN_ATIVO = 0;
            Int32 volta = ccApp.ValidateDelete(item, usuario, idAss);

            if (volta == 1)
            {
                Session["MensCatCont"] = 2;
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
                return RedirectToAction("MontarTelaCategoriaContato");
            }

            listaMasterCC = new List<CATEGORIA_CONTATO>();
            Session["ListaCatCont"] = null;
            return RedirectToAction("MontarTelaCategoriaContato");
        }

        [HttpGet]
        public ActionResult ReativarCategoriaContato(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            CATEGORIA_CONTATO item = ccApp.GetItemById(id);
            objetoAntesCC = (CATEGORIA_CONTATO)Session["CatContato"];
            item.CACO_IN_ATIVO = 1;
            Int32 volta = ccApp.ValidateReativar(item, usuario, idAss);
            listaMasterCC = new List<CATEGORIA_CONTATO>();
            Session["ListaCatCont"] = null;
            return RedirectToAction("MontarTelaCategoriaContato");
        }

        [HttpGet]
        public ActionResult MontarTelaClubes()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            usuario = (USUARIO)Session["UserCredentials"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List<CLUBE>)Session["ListaClube"] == null)
            {
                listaMasterCL = clApp.GetAllItens();
                Session["ListaClube"] = listaMasterCL;
            }
            ViewBag.Listas = (List<CLUBE>)Session["ListaClube"];
            ViewBag.Title = "Clube";

            // Indicadores
            ViewBag.Itens = ((List<CLUBE>)Session["ListaClube"]).Count;

            // Mensagem
            if ((Int32)Session["MensClube"] == 2)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensClube"] = 0;
            objetoCL = new CLUBE();
            return View(objetoCL);
        }

        public ActionResult MostrarTudoClubes()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            listaMasterCL = clApp.GetAllItensAdm();
            Session["ListaClube"] = listaMasterCL;
            return RedirectToAction("MontarTelaClubes");
        }

        public ActionResult VoltarBaseClubes()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaClube"] = clApp.GetAllItens();
            return RedirectToAction("MontarTelaClubes");
        }

        [HttpGet]
        public ActionResult IncluirClubes()
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            // Mensagem
            if ((Int32)Session["MensClube"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0038", CultureInfo.CurrentCulture));
            }

            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            Session["MensClube"] = 0;
            CLUBE item = new CLUBE();
            ClubeViewModel vm = Mapper.Map<CLUBE, ClubeViewModel>(item);
            vm.CLUB_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirClubes(ClubeViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CLUBE item = Mapper.Map<ClubeViewModel, CLUBE>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = clApp.ValidateCreate(item, usuario, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensClube"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0038", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterCC = new List<CATEGORIA_CONTATO>();
                    Session["ListaClube"] = null;
                    Session["MensClube"] = 0;
                    return RedirectToAction("MontarTelaClubes");
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
        public ActionResult EditarClubes(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Mensagem
            if ((Int32)Session["MensClube"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0038", CultureInfo.CurrentCulture));
            }

            CLUBE item = clApp.GetItemById(id);
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            Session["MensClube"] = 0;
            Session["Antes"] = item;
            Session["Clube"] = item;
            Session["IdVolta"] = id;
            ClubeViewModel vm = Mapper.Map<CLUBE, ClubeViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarClubes(ClubeViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.UFs = new SelectList((List<UF>)Session["UFs"], "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    CLUBE item = Mapper.Map<ClubeViewModel, CLUBE>(vm);
                    Int32 volta = clApp.ValidateEdit(item, (CLUBE)Session["Antes"], usuario, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensClube"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0038", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterCC = new List<CATEGORIA_CONTATO>();
                    Session["MensClube"] = 0;
                    Session["ListaClube"] = null;
                    return RedirectToAction("MontarTelaClubes");
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
        public ActionResult ExcluirClubes(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            CLUBE item = clApp.GetItemById(id);
            objetoAntesCL = (CLUBE)Session["Clube"];
            item.CLUB_IN_ATIVO = 0;
            Int32 volta = clApp.ValidateDelete(item, usuario, idAss);

            if (volta == 1)
            {
                Session["MensClube"] = 2;
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
                return RedirectToAction("MontarTelaClubes");
            }

            listaMasterCL = new List<CLUBE>();
            Session["ListaClube"] = null;
            return RedirectToAction("MontarTelaClubes");
        }

        [HttpGet]
        public ActionResult ReativarClubes(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            CLUBE item = clApp.GetItemById(id);
            objetoAntesCL = (CLUBE)Session["Clube"];
            item.CLUB_IN_ATIVO = 1;
            Int32 volta = clApp.ValidateReativar(item, usuario, idAss);
            listaMasterCL = new List<CLUBE>();
            Session["ListaClube"] = null;
            return RedirectToAction("MontarTelaClubes");
        }

        [HttpGet]
        public ActionResult MontarTelaOrigem()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            usuario = (USUARIO)Session["UserCredentials"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List<ORIGEM>)Session["ListaOrigem"] == null)
            {
                listaMasterOR = orApp.GetAllItens();
                Session["ListaOrigem"] = listaMasterOR;
            }
            ViewBag.Listas = (List<ORIGEM>)Session["ListaOrigem"];
            ViewBag.Title = "Origens";

            // Indicadores
            ViewBag.Itens = ((List<ORIGEM>)Session["ListaOrigem"]).Count;

            // Mensagem
            if ((Int32)Session["MensOrigem"] == 2)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensOrigem"] = 0;
            objetoOR = new ORIGEM();
            return View(objetoOR);
        }

        public ActionResult MostrarTudoOrigem()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            listaMasterOR = orApp.GetAllItensAdm();
            Session["ListaOrigem"] = listaMasterOR;
            return RedirectToAction("MontarTelaOrigem");
        }

        public ActionResult VoltarBaseOrigem()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaOrigem"] = orApp.GetAllItens();
            return RedirectToAction("MontarTelaOrigem");
        }

        [HttpGet]
        public ActionResult IncluirOrigem()
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            // Mensagem
            if ((Int32)Session["MensOrigem"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0039", CultureInfo.CurrentCulture));
            }

            Session["MensOrigem"] = 0;
            ORIGEM item = new ORIGEM();
            OrigemViewModel vm = Mapper.Map<ORIGEM, OrigemViewModel>(item);
            vm.ORIG_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirOrigem(OrigemViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ORIGEM item = Mapper.Map<OrigemViewModel, ORIGEM>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = orApp.ValidateCreate(item, usuario, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensOrigem"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0039", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterOR = new List<ORIGEM>();
                    Session["ListaOrigem"] = null;
                    Session["MensOrigem"] = 0;
                    return RedirectToAction("MontarTelaOrigem");
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
        public ActionResult EditarOrigem(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Mensagem
            if ((Int32)Session["MensCatCont"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0037", CultureInfo.CurrentCulture));
            }

            ORIGEM item = orApp.GetItemById(id);
            Session["MensOrigem"] = 0;
            Session["Antes"] = item;
            Session["Origem"] = item;
            Session["IdVolta"] = id;
            OrigemViewModel vm = Mapper.Map<ORIGEM, OrigemViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarOrigem(OrigemViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    ORIGEM item = Mapper.Map<OrigemViewModel, ORIGEM>(vm);
                    Int32 volta = orApp.ValidateEdit(item, (ORIGEM)Session["Antes"], usuario, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensOrigem"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0039", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterOR = new List<ORIGEM>();
                    Session["MensOrigem"] = 0;
                    Session["ListaOrigem"] = null;
                    return RedirectToAction("MontarTelaOrigem");
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
        public ActionResult ExcluirOrigem(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            ORIGEM item = orApp.GetItemById(id);
            objetoAntesOR = (ORIGEM)Session["Origem"];
            item.ORIG_IN_ATIVO = 0;
            Int32 volta = orApp.ValidateDelete(item, usuario, idAss);

            if (volta == 1)
            {
                Session["MensOrigem"] = 2;
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
                return RedirectToAction("MontarTelaOrigem");
            }

            listaMasterOR = new List<ORIGEM>();
            Session["ListaOrigem"] = null;
            return RedirectToAction("MontarTelaOrigem");
        }

        [HttpGet]
        public ActionResult ReativarOrigem(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            ORIGEM item = orApp.GetItemById(id);
            objetoAntesOR = (ORIGEM)Session["Origem"];
            item.ORIG_IN_ATIVO = 1;
            Int32 volta = orApp.ValidateReativar(item, usuario, idAss);
            listaMasterOR = new List<ORIGEM>();
            Session["ListaOrigem"] = null;
            return RedirectToAction("MontarTelaOrigem");
        }

        [HttpGet]
        public ActionResult MontarTelaProfissao()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            usuario = (USUARIO)Session["UserCredentials"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if ((List<PROFISSAO>)Session["ListaProfissao"] == null)
            {
                listaMasterPR = prApp.GetAllItens();
                Session["ListaProfissao"] = listaMasterPR;
            }
            ViewBag.Listas = (List<PROFISSAO>)Session["ListaProfissao"];
            ViewBag.Title = "Profissao";

            // Indicadores
            ViewBag.Itens = ((List<PROFISSAO>)Session["ListaProfissao"]).Count;

            // Mensagem
            if ((Int32)Session["MensProfissao"] == 2)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensProfissao"] = 0;
            objetoPR = new PROFISSAO();
            return View(objetoPR);
        }

        public ActionResult MostrarTudoProfissao()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            listaMasterPR = prApp.GetAllItensAdm();
            Session["ListaProfissao"] = listaMasterPR;
            return RedirectToAction("MontarTelaProfissao");
        }

        public ActionResult VoltarBaseProfissao()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaProfissao"] = prApp.GetAllItens();
            return RedirectToAction("MontarTelaProfissao");
        }

        [HttpGet]
        public ActionResult IncluirProfissao()
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            // Mensagem
            if ((Int32)Session["MensProfissao"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0040", CultureInfo.CurrentCulture));
            }

            Session["MensProfissao"] = 0;
            PROFISSAO item = new PROFISSAO();
            ProfissaoViewModel vm = Mapper.Map<PROFISSAO, ProfissaoViewModel>(item);
            vm.PROF_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirProfissao(ProfissaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PROFISSAO item = Mapper.Map<ProfissaoViewModel, PROFISSAO>(vm);
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    Int32 volta = prApp.ValidateCreate(item, usuario, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensProfissao"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0040", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterPR = new List<PROFISSAO>();
                    Session["ListaProfissao"] = null;
                    Session["MensProfissao"] = 0;
                    return RedirectToAction("MontarTelaProfissao");
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
        public ActionResult EditarProfissao(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Mensagem
            if ((Int32)Session["MensCatCont"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0037", CultureInfo.CurrentCulture));
            }

            PROFISSAO item = prApp.GetItemById(id);
            Session["MensProfissao"] = 0;
            Session["Antes"] = item;
            Session["Profissao"] = item;
            Session["IdVolta"] = id;
            ProfissaoViewModel vm = Mapper.Map<PROFISSAO, ProfissaoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarProfissao(ProfissaoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuario = (USUARIO)Session["UserCredentials"];
                    PROFISSAO item = Mapper.Map<ProfissaoViewModel, PROFISSAO>(vm);
                    Int32 volta = prApp.ValidateEdit(item, (PROFISSAO)Session["Antes"], usuario, idAss);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensProfissao"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0040", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterPR = new List<PROFISSAO>();
                    Session["MensProfissao"] = 0;
                    Session["ListaProfissao"] = null;
                    return RedirectToAction("MontarTelaProfissao");
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
        public ActionResult ExcluirProfissao(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            PROFISSAO item = prApp.GetItemById(id);
            objetoAntesPR = (PROFISSAO)Session["Profissao"];
            item.PROF_IN_ATIVO = 0;
            Int32 volta = prApp.ValidateDelete(item, usuario, idAss);

            if (volta == 1)
            {
                Session["MensProfissao"] = 2;
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
                return RedirectToAction("MontarTelaProfissao");
            }

            listaMasterPR = new List<PROFISSAO>();
            Session["ListaProfissao"] = null;
            return RedirectToAction("MontarTelaProfissao");
        }

        [HttpGet]
        public ActionResult ReativarProfissao(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            PROFISSAO item = prApp.GetItemById(id);
            objetoAntesPR = (PROFISSAO)Session["Profissao"];
            item.PROF_IN_ATIVO = 1;
            Int32 volta = prApp.ValidateReativar(item, usuario, idAss);
            listaMasterPR = new List<PROFISSAO>();
            Session["ListaProfissao"] = null;
            return RedirectToAction("MontarTelaProfissao");
        }


















    }
}