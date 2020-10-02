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
    public class ConfiguracaoController : Controller
    {
        private readonly IConfiguracaoAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IUsuarioAppService usuApp;
        private String msg;
        private Exception exception;
        CONFIGURACAO objeto = new CONFIGURACAO();
        CONFIGURACAO objetoAntes = new CONFIGURACAO();
        List<CONFIGURACAO> listaMaster = new List<CONFIGURACAO>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();

        public ConfiguracaoController(IConfiguracaoAppService baseApps, ILogAppService logApps, IUsuarioAppService usuApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            usuApp = usuApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            CONFIGURACAO item = new CONFIGURACAO();
            ConfiguracaoViewModel vm = Mapper.Map<CONFIGURACAO, ConfiguracaoViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult MontarTelaConfiguracao()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            //SessionMocks.IdAssinante = 2;
            //objeto = baseApp.GetItemById(1);
            if (SessionMocks.Configuracao == null)
            {
                objeto = usuario.ASSINANTE.CONFIGURACAO.First();
                SessionMocks.Configuracao = objeto;
            }
            ViewBag.Title = "Configurações";

            // Abre view
            return View(objeto);
        }

        public ActionResult Voltar()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

    }
}