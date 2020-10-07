using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
//using EntitiesServices.Work_Classes;
using AutoMapper;
using SMS_Presentation.ViewModels;
using System.IO;

namespace SMS_Presentation.Controllers
{
    public class MensagemController : Controller
    {
        private readonly IMensagemAppService baseApp;
        private readonly IUsuarioAppService usuApp;
        private readonly IContatoAppService conApp;
        private readonly IGrupoAppService gruApp;
        private readonly ICampanhaAppService camApp;
        private readonly IMensagemContatoAppService mcApp;
        private readonly IMensagemGrupoAppService mgApp;
        private readonly IMensagemCampanhaAppService mpApp;
        private readonly ITemplateAppService temApp;

        private String msg;
        private Exception exception;
        MENSAGEM objetoAss = new MENSAGEM();
        MENSAGEM objetoAssAntes = new MENSAGEM();
        List<MENSAGEM> listaMasterAss = new List<MENSAGEM>();
        String extensao;

        public MensagemController(IMensagemAppService baseApps, IUsuarioAppService usuApps, IContatoAppService conApps, IGrupoAppService gruApps, ICampanhaAppService camApps, IMensagemContatoAppService mcApps, IMensagemGrupoAppService mgApps, IMensagemCampanhaAppService mpApps, ITemplateAppService temApps)
        {
            baseApp = baseApps;
            usuApp = usuApps;
            conApp = conApps;
            gruApp = gruApps;
            camApp = camApps;
            mcApp = mcApps;
            mgApp = mgApps;
            mpApp = mpApps;
            temApp = temApps;
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
        public ActionResult MontarTelaMensagens()
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
            if ((List<MENSAGEM>)Session["ListaMensagem"] == null)
            {
                listaMasterAss = baseApp.GetAllItens(idAss);
                Session["ListaMensagem"] = listaMasterAss;
            }
            ViewBag.Listas = (List<MENSAGEM>)Session["ListaMensagem"];
            ViewBag.Title = "Mensagens";
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem() { Text = "Todas", Value = "1" });
            status.Add(new SelectListItem() { Text = "Enviadas", Value = "2" });
            status.Add(new SelectListItem() { Text = "Agendadas", Value = "3" });
            ViewBag.Status = new SelectList(status, "Value", "Text");

            // Indicadores
            ViewBag.Mensagens = ((List<MENSAGEM>)Session["ListaMensagem"]).Count;

            // Mensagem
            if ((Int32)Session["MensMensagem"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensMensagem"] = 0;
            objetoAss = new MENSAGEM();
            objetoAss.MENS_DT_DATA = DateTime.Today.Date;
            return View(objetoAss);
        }

        public ActionResult RetirarFiltroMensagem()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaMensagem"] = null;
            return RedirectToAction("MontarTelaMensagens");
        }

        public ActionResult MostrarTudoMensagem()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterAss = baseApp.GetAllItensAdm(idAss);
            Session["ListaMensagem"] = listaMasterAss;
            return RedirectToAction("MontarTelaMensagens");
        }

        [HttpPost]
        public ActionResult FiltrarMensagem(MENSAGEM item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                Int32 enviada = 0;
                Int32 agendada = 0;
                if (item.MENS_IN_ENVIADA == 1)
                {
                    enviada = 0;
                    agendada = 0;
                }
                if (item.MENS_IN_ENVIADA == 2)
                {
                    enviada = 1;
                    agendada = 0;
                }
                if (item.MENS_IN_ENVIADA == 3)
                {
                    enviada = 0;
                    agendada = 1;
                }
                List<MENSAGEM> listaObj = new List<MENSAGEM>();
                Int32 idAss = (Int32)Session["IdAssinante"];
                Int32 volta = baseApp.ExecuteFilter(null, item.MENS_DT_DATA, enviada, agendada, item.MENS_TX_TEXTO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensMensagem"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaMensagens");
                }

                // Sucesso
                Session["MensMensagem"] = 0;
                listaMasterAss = listaObj;
                Session["ListaMensagem"] = listaObj;
                return RedirectToAction("MontarTelaMensagens");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaMensagens");
            }
        }

        public ActionResult VoltarBaseMensagem()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("MontarTelaMensagens");
        }

        [HttpGet]
        public ActionResult IncluirMensagem()
        {
            // Prepara listas
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Contatos = new SelectList(conApp.GetAllItens(idAss), "CONT_CD_ID", "CONT_NM_NOME");
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GRUP_NM_NOME");
            ViewBag.Campanhas = new SelectList(camApp.GetAllItens(idAss), "CAMP_CD_ID", "CAMP_NM_NOME");
            ViewBag.Templates = new SelectList(temApp.GetAllItens(idAss), "TEMP_CD_ID", "TEMP_NM_NOME");
            List<SelectListItem> tipoSMS = new List<SelectListItem>();
            tipoSMS.Add(new SelectListItem() { Text = "Long Code", Value = "0" });
            tipoSMS.Add(new SelectListItem() { Text = "Short Code", Value = "1" });
            ViewBag.Tipos = new SelectList(tipoSMS, "Value", "Text");
            List<SelectListItem> operacao = new List<SelectListItem>();
            operacao.Add(new SelectListItem() { Text = "Enviar", Value = "1" });
            operacao.Add(new SelectListItem() { Text = "Agendar", Value = "2" });
            ViewBag.Operacoes = new SelectList(operacao, "Value", "Text");

            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            MENSAGEM item = new MENSAGEM();
            MensagemViewModel vm = Mapper.Map<MENSAGEM, MensagemViewModel>(item);
            vm.MENS_IN_ATIVO = 1;
            vm.ASSI_CD_ID = idAss;
            vm.MENS_DT_DATA = DateTime.Today.Date;
            vm.MENS_DT_AGENDA = null;
            vm.MENS_DT_ENVIO = null;
            vm.MENS_IN_ENVIADA = 0;
            vm.MENS_IN_TIPO_SMS = 1;
            vm.USUA_CD_ID = usuarioLogado.USUA_CD_ID;
            vm.MENS_TX_RETORNOS = null;
            vm.MENS_NM_NOME = "-";
            if ((String)Session["Resposta"] != null)
            {
                vm.MENS_TX_RETORNOS = (String)Session["Resposta"];
                Session["Resposta"] = null;
            }
            else
            {
                vm.MENS_TX_RETORNOS = String.Empty;
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirMensagem(MensagemViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Contatos = new SelectList(conApp.GetAllItens(idAss), "CONT_CD_ID", "CONT_NM_NOME");
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GRUP_NM_NOME");
            ViewBag.Campanhas = new SelectList(camApp.GetAllItens(idAss), "CAMP_CD_ID", "CAMP_NM_NOME");
            ViewBag.Templates = new SelectList(temApp.GetAllItens(idAss), "TEMP_CD_ID", "TEMP_NM_NOME");
            List<SelectListItem> tipoSMS = new List<SelectListItem>();
            tipoSMS.Add(new SelectListItem() { Text = "Long Code", Value = "0" });
            tipoSMS.Add(new SelectListItem() { Text = "Short Code", Value = "1" });
            ViewBag.Tipos = new SelectList(tipoSMS, "Value", "Text");
            List<SelectListItem> operacao = new List<SelectListItem>();
            operacao.Add(new SelectListItem() { Text = "Enviar", Value = "1" });
            operacao.Add(new SelectListItem() { Text = "Agendar", Value = "2" });
            ViewBag.Operacoes = new SelectList(operacao, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    MENSAGEM item = Mapper.Map<MensagemViewModel, MENSAGEM>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    String volta = baseApp.ValidateCreate(item, usuarioLogado, idAss);

                    // Verifica retorno
                    if (volta == "1")
                    {
                        Session["MensMensagem"] = 1;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0041", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == "2")
                    {
                        Session["MensMensagem"] = 2;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0042", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == "3")
                    {
                        Session["MensMensagem"] = 3;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0043", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == "4")
                    {
                        Session["MensMensagem"] = 4;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0044", CultureInfo.CurrentCulture));
                        return View(vm);
                    }
                    if (volta == "5")
                    {
                        Session["MensMensagem"] = 5;
                        ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0045", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterAss = new List<MENSAGEM>();
                    Session["ListaMensagem"] = null;
                    Session["VoltaMensagem"] = 1;
                    Session["IdAssinanteVolta"] = item.ASSI_CD_ID;
                    Session["Mensagem"] = item;
                    Session["MensMensagem"] = 0;
                    Session["IdVolta"] = item.MENS_CD_ID;
                    Session["Resposta"] = volta;
                    return RedirectToAction("IncluirMensagem");
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
        public ActionResult MontarTelaAgendadas()
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
            if ((List<MENSAGEM>)Session["ListaMensagem"] == null)
            {
                List<MENSAGEM> lista = baseApp.GetAllItens(idAss);
                lista = lista.Where(p => p.MENS_DT_AGENDA != null & p.MENS_DT_AGENDA > DateTime.Today.Date).ToList();
                listaMasterAss = lista;
                Session["ListaMensagem"] = listaMasterAss;
            }
            ViewBag.Listas = (List<MENSAGEM>)Session["ListaMensagem"];
            ViewBag.Title = "Mensagens Agendadas";

            // Indicadores
            ViewBag.Mensagens = ((List<MENSAGEM>)Session["ListaMensagem"]).Count;

            // Mensagem
            if ((Int32)Session["MensMensagem"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensMensagem"] = 0;
            objetoAss = new MENSAGEM();
            objetoAss.MENS_DT_DATA = DateTime.Today.Date;
            return View(objetoAss);
        }

        [HttpPost]
        public ActionResult FiltrarMensagemAgendada(MENSAGEM item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                List<MENSAGEM> listaObj = new List<MENSAGEM>();
                Int32 idAss = (Int32)Session["IdAssinante"];
                Int32 volta = baseApp.ExecuteFilter(null, item.MENS_DT_DATA, null, 1, item.MENS_TX_TEXTO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensMensagem"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaAgendadas");
                }

                // Sucesso
                Session["MensMensagem"] = 0;
                listaMasterAss = listaObj;
                Session["ListaMensagem"] = listaObj;
                return RedirectToAction("MontarTelaAgendadas");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaAgendadas");
            }
        }

        public ActionResult RetirarFiltroMensagemAgendada()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaMensagem"] = null;
            return RedirectToAction("MontarTelaAgendadas");
        }

        [HttpGet]
        public ActionResult MontarTelaEnviadas()
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
            if ((List<MENSAGEM>)Session["ListaMensagem"] == null)
            {
                List<MENSAGEM> lista = baseApp.GetAllItens(idAss);
                lista = lista.Where(p => p.MENS_IN_ENVIADA == 1).ToList();
                listaMasterAss = lista;
                Session["ListaMensagem"] = listaMasterAss;
            }
            ViewBag.Listas = (List<MENSAGEM>)Session["ListaMensagem"];
            ViewBag.Title = "Mensagens Enviadas";

            // Indicadores
            ViewBag.Mensagens = ((List<MENSAGEM>)Session["ListaMensagem"]).Count;

            // Mensagem
            if ((Int32)Session["MensMensagem"] == 1)
            {
                ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }

            // Abre view
            Session["MensMensagem"] = 0;
            objetoAss = new MENSAGEM();
            objetoAss.MENS_DT_DATA = DateTime.Today.Date;
            return View(objetoAss);
        }

        [HttpPost]
        public ActionResult FiltrarMensagemEnviada(MENSAGEM item)
        {
            try
            {
                // Executa a operação
                if ((String)Session["Ativa"] == null)
                {
                    return RedirectToAction("Login", "ControleAcesso");
                }
                List<MENSAGEM> listaObj = new List<MENSAGEM>();
                Int32 idAss = (Int32)Session["IdAssinante"];
                Int32 volta = baseApp.ExecuteFilter(null, item.MENS_DT_DATA, 1, null, item.MENS_TX_TEXTO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensMensagem"] = 1;
                    ModelState.AddModelError("", SMS_Resource.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                    return RedirectToAction("MontarTelaEnviadas");
                }

                // Sucesso
                Session["MensMensagem"] = 0;
                listaMasterAss = listaObj;
                Session["ListaMensagem"] = listaObj;
                return RedirectToAction("MontarTelaEnviadas");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaEnviadas");
            }
        }

        public ActionResult RetirarFiltroMensagemEnviada()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaMensagem"] = null;
            return RedirectToAction("MontarTelaEnviadas");
        }

        [HttpGet]
        public ActionResult VerMensagem(Int32 id)
        {
            // Prepara view
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            MENSAGEM item = baseApp.GetItemById(id);

            // Recuperar pessoas
            Int32 pessoas = 0;
            String lista = String.Empty;
            if (item.CONTATO != null)
            {
                pessoas++;
                lista += item.CONTATO.CONT_NM_NOME;
            }
            if (item.GRUPO != null)
            {
                pessoas += item.GRUPO.GRUPO_CONTATO.Count;
                foreach (var pess in item.GRUPO.GRUPO_CONTATO)
                {
                    lista += pess.CONTATO.CONT_NM_NOME + "\r\n";
                }
            }
            if (item.CAMPANHA != null)
            {
                pessoas += item.CAMPANHA.CAMPANHA_CONTATO.Count;
                foreach (var pess in item.CAMPANHA.CAMPANHA_CONTATO)
                {
                    lista += pess.CONTATO.CONT_NM_NOME + "\r\n";
                }
            }

            ViewBag.Nomes = lista;
            ViewBag.Pessoas = pessoas;

            objetoAssAntes = item;
            Session["Mensagem"] = item;
            Session["IdVolta"] = id;
            MensagemViewModel vm = Mapper.Map<MENSAGEM, MensagemViewModel>(item);
            vm.MENS_TX_RETORNOS = lista;
            return View(vm);
        }

        [HttpPost]
        public JsonResult RecuperarTemplate(Int32? id)
        {
            var mensagem = String.Empty;

            // Filtro para caso o placeholder seja selecionado
            if (id == null)
            {
                mensagem = String.Empty;
            }
            else
            {
                mensagem = temApp.GetItemById(id.Value).TEMP_TX_TEXTO;
            }

            return Json(mensagem);
        }

        public ActionResult ImportarDados()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarDesenvolvimento", "BaseAdmin");
        }


    }
}