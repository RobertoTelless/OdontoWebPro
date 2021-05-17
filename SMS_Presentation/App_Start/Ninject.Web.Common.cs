using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;
using Ninject.Web.Common;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using ApplicationServices.Interfaces;
using ModelServices.Interfaces.EntitiesServices;
using ModelServices.Interfaces.Repositories;
using ApplicationServices.Services;
using ModelServices.EntitiesServices;
using DataServices.Repositories;
using Ninject.Web.Common.WebHost;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Presentation.Start.NinjectWebCommons), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Presentation.Start.NinjectWebCommons), "Stop")]

namespace Presentation.Start
{
    public class NinjectWebCommons
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind(typeof(IAppServiceBase<>)).To(typeof(AppServiceBase<>));
            kernel.Bind<IUsuarioAppService>().To<UsuarioAppService>();
            kernel.Bind<ILogAppService>().To<LogAppService>();
            kernel.Bind<IPerfilAppService>().To<PerfilAppService>();
            kernel.Bind<IConfiguracaoAppService>().To<ConfiguracaoAppService>();
            kernel.Bind<IMatrizAppService>().To<MatrizAppService>();
            kernel.Bind<IFilialAppService>().To<FilialAppService>();
            kernel.Bind<ICargoAppService>().To<CargoAppService>();
            kernel.Bind<INoticiaAppService>().To<NoticiaAppService>();
            kernel.Bind<INotificacaoAppService>().To<NotificacaoAppService>();
            kernel.Bind<ITipoPessoaAppService>().To<TipoPessoaAppService>();
            kernel.Bind<IRegimeTributarioAppService>().To<RegimeTributarioAppService>();
            kernel.Bind<IUnidadeAppService>().To<UnidadeAppService>();
            kernel.Bind<ITemplateAppService>().To<TemplateAppService>();
            kernel.Bind<ITarefaAppService>().To<TarefaAppService>();
            kernel.Bind<ITipoContribuinteAppService>().To<TipoContribuinteAppService>();
            kernel.Bind<IAgendaAppService>().To<AgendaAppService>();
            kernel.Bind<ITipoTagAppService>().To<TipoTagAppService>();
            kernel.Bind<ITelefoneAppService>().To<TelefoneAppService>();
            kernel.Bind<ICentroCustoAppService>().To<CentroCustoAppService>();
            kernel.Bind<IGrupoAppService>().To<GrupoAppService>();
            kernel.Bind<ISubgrupoAppService>().To<SubgrupoAppService>();
            kernel.Bind<IBancoAppService>().To<BancoAppService>();
            kernel.Bind<IContaBancariaAppService>().To<ContaBancariaAppService>();
            kernel.Bind<ITipoContaAppService>().To<TipoContaAppService>();
            kernel.Bind<IDenteRegiaoAppService>().To<DenteRegiaoAppService>();
            kernel.Bind<ITipoProcedimentoAppService>().To<TipoProcedimentoAppService>();
            kernel.Bind<IPacienteAppService>().To<PacienteAppService>();
            kernel.Bind<ICategoriaFornecedorAppService>().To<CategoriaFornecedorAppService>();
            kernel.Bind<IFornecedorAppService>().To<FornecedorAppService>();
            kernel.Bind<IFornecedorCnpjAppService>().To<FornecedorCnpjAppService>();
            kernel.Bind<IProdutoAppService>().To<ProdutoAppService>();
            kernel.Bind<IProdutoEstoqueFilialAppService>().To<ProdutoEstoqueFilialAppService>();
            kernel.Bind<IMovimentoEstoqueProdutoAppService>().To<MovimentoEstoqueProdutoAppService>();
            kernel.Bind<IProdutotabelaPrecoAppService>().To<ProdutoTabelaPrecoAppService>();
            kernel.Bind<ITipoImagemAppService>().To<TipoImagemAppService>();
            kernel.Bind<ISubProcedimentoAppService>().To<SubProcedimentoAppService>();

            kernel.Bind(typeof(IServiceBase<>)).To(typeof(ServiceBase<>));
            kernel.Bind<IUsuarioService>().To<UsuarioService>();
            kernel.Bind<ILogService>().To<LogService>();
            kernel.Bind<IPerfilService>().To<PerfilService>();
            kernel.Bind<IConfiguracaoService>().To<ConfiguracaoService>();
            kernel.Bind<IMatrizService>().To<MatrizService>();
            kernel.Bind<IFilialService>().To<FilialService>();
            kernel.Bind<ICargoService>().To<CargoService>();
            kernel.Bind<INotificacaoService>().To<NotificacaoService>();
            kernel.Bind<INoticiaService>().To<NoticiaService>();
            kernel.Bind<ISexoService>().To<SexoService>();
            kernel.Bind<ITipoPessoaService>().To<TipoPessoaService>();
            kernel.Bind<IRegimeTributarioService>().To<RegimeTributarioService>();
            kernel.Bind<IUnidadeService>().To<UnidadeService>();
            kernel.Bind<ITemplateService>().To<TemplateService>();
            kernel.Bind<ITarefaService>().To<TarefaService>();
            kernel.Bind<ITipoContribuinteService>().To<TipoContribuinteService>();
            kernel.Bind<IAgendaService>().To<AgendaService>();
            kernel.Bind<ITipoTagService>().To<TipoTagService>();
            kernel.Bind<ITelefoneService>().To<TelefoneService>();
            kernel.Bind<ICentroCustoService>().To<CentroCustoService>();
            kernel.Bind<IGrupoService>().To<GrupoService>();
            kernel.Bind<ISubgrupoService>().To<SubgrupoService>();
            kernel.Bind<IBancoService>().To<BancoService>();
            kernel.Bind<IContaBancariaService>().To<ContaBancariaService>();
            kernel.Bind<ITipoContaService>().To<TipoContaService>();
            kernel.Bind<IDenteRegiaoService>().To<DenteRegiaoService>();
            kernel.Bind<ITipoProcedimentoService>().To<TipoProcedimentoService>();
            kernel.Bind<IPacienteService>().To<PacienteService>();
            kernel.Bind<ICategoriaFornecedorService>().To<CategoriaFornecedorService>();
            kernel.Bind<IFornecedorService>().To<FornecedorService>();
            kernel.Bind<IFornecedorCnpjService>().To<FornecedorCnpjService>();
            kernel.Bind<IMovimentoEstoqueProdutoService>().To<MovimentoEstoqueProdutoService>();
            kernel.Bind<IProdutoService>().To<ProdutoService>();
            kernel.Bind<IProdutoEstoqueFilialService>().To<ProdutoEstoqueFilialService>();
            kernel.Bind<IProdutoTabelaPrecoService>().To<ProdutoTabelaPrecoService>();
            kernel.Bind<ITipoImagemService>().To<TipoImagemService>();
            kernel.Bind<ISubProcedimentoService>().To<SubProcedimentoService>();

            kernel.Bind(typeof(IRepositoryBase<>)).To(typeof(RepositoryBase<>));
            kernel.Bind<IConfiguracaoRepository>().To<ConfiguracaoRepository>();
            kernel.Bind<IUsuarioRepository>().To<UsuarioRepository>();
            kernel.Bind<ILogRepository>().To<LogRepository>();
            kernel.Bind<IPerfilRepository>().To<PerfilRepository>();
            kernel.Bind<ITemplateRepository>().To<TemplateRepository>();
            kernel.Bind<IMatrizRepository>().To<MatrizRepository>();
            kernel.Bind<IFilialRepository>().To<FilialRepository>();
            kernel.Bind<IUnidadeRepository>().To<UnidadeRepository>();
            kernel.Bind<ICargoRepository>().To<CargoRepository>();
            kernel.Bind<ITipoContribuinteRepository>().To<TipoContribuinteRepository>();
            kernel.Bind<ITipoPessoaRepository>().To<TipoPessoaRepository>();
            kernel.Bind<ICategoriaNotificacaoRepository>().To<CategoriaNotificacaoRepository>();
            kernel.Bind<INotificacaoRepository>().To<NotificacaoRepository>();
            kernel.Bind<INoticiaRepository>().To<NoticiaRepository>();
            kernel.Bind<INoticiaComentarioRepository>().To<NoticiaComentarioRepository>();
            kernel.Bind<INoticiaTagRepository>().To<NoticiaTagRepository>();
            kernel.Bind<ITipoTagRepository>().To<TipoTagRepository>();
            kernel.Bind<INotificacaoAnexoRepository>().To<NotificacaoAnexoRepository>();
            kernel.Bind<ISexoRepository>().To<SexoRepository>();
            kernel.Bind<IRegimeTributarioRepository>().To<RegimeTributarioRepository>();
            kernel.Bind<ITarefaRepository>().To<TarefaRepository>();
            kernel.Bind<ITipoTarefaRepository>().To<TipoTarefaRepository>();
            kernel.Bind<ITarefaAnexoRepository>().To<TarefaAnexoRepository>();
            kernel.Bind<ITarefaNotificacaoRepository>().To<TarefaNotificacaoRepository>();
            kernel.Bind<IUsuarioAnexoRepository>().To<UsuarioAnexoRepository>();
            kernel.Bind<IUFRepository>().To<UFRepository>();
            kernel.Bind<ISituacaoRepository>().To<SituacaoRepository>();
            kernel.Bind<IAgendaRepository>().To<AgendaRepository>();
            kernel.Bind<IAgendaAnexoRepository>().To<AgendaAnexoRepository>();
            kernel.Bind<ICategoriaAgendaRepository>().To<CategoriaAgendaRepository>();
            kernel.Bind<ICategoriaProdutoRepository>().To<CategoriaProdutoRepository>();
            kernel.Bind<ICategoriaTelefoneRepository>().To<CategoriaTelefoneRepository>();
            kernel.Bind<ICategoriaUsuarioRepository>().To<CategoriaUsuarioRepository>();
            kernel.Bind<ITelefoneRepository>().To<TelefoneRepository>();
            kernel.Bind<ICentroCustoRepository>().To<CentroCustoRepository>();
            kernel.Bind<IProdutoRepository>().To<ProdutoRepository>();
            kernel.Bind<IMovimentoEstoqueProdutoRepository>().To<MovimentoEstoqueProdutoRepository>();
            kernel.Bind<IProdutoFornecedorRepository>().To<ProdutoFornecedorRepository>();
            kernel.Bind<IProdutoAnexoRepository>().To<ProdutoAnexoRepository>();
            kernel.Bind<IUsuarioRemuneracaoRepository>().To<UsuarioRemuneracaoRepository>();
            kernel.Bind<IUsuarioContrachequeRepository>().To<UsuarioContrachequeRepository>();
            kernel.Bind<IGrupoRepository>().To<GrupoRepository>();
            kernel.Bind<ISubgrupoRepository>().To<SubgrupoRepository>();
            kernel.Bind<IBancoRepository>().To<BancoRepository>();
            kernel.Bind<IContaBancariaRepository>().To<ContaBancariaRepository>();
            kernel.Bind<IContaBancariaContatoRepository>().To<ContaBancariaContatoRepository>();
            kernel.Bind<IContaBancariaLancamentoRepository>().To<ContaBancariaLancamentoRepository>();
            kernel.Bind<ITipoContaRepository>().To<TipoContaRepository>();
            kernel.Bind<IDenteRegiaoRepository>().To<DenteRegiaoRepository>();
            kernel.Bind<ITipoProcedimentoRepository>().To<TipoProcedimentoRepository>();
            kernel.Bind<ITipoProcedimentoAnexoRepository>().To<TipoProcedimentoAnexoRepository>();
            kernel.Bind<IPacienteRepository>().To<PacienteRepository>();
            kernel.Bind<IPacienteAnexoRepository>().To<PacienteAnexoRepository>();
            kernel.Bind<IPacienteAcompanhamentoRepository>().To<PacienteAcompanhamentoRepository>();
            kernel.Bind<IPacientePrescricaoRepository>().To<PacientePrescricaoRepository>();
            kernel.Bind<IPacienteRecomendacaoRepository>().To<PacienteRecomendacaoRepository>();
            kernel.Bind<ICategoriaPacienteRepository>().To<CategoriaPacienteRepository>();
            kernel.Bind<ICategoriaFornecedorRepository>().To<CategoriaFornecedorRepository>();
            kernel.Bind<IFornecedorAnexoRepository>().To<FornecedorAnexoRepository>();
            kernel.Bind<IFornecedorCnpjRepository>().To<FornecedorCnpjRepository>();
            kernel.Bind<IFornecedorContatoRepository>().To<FornecedorContatoRepository>();
            kernel.Bind<IFornecedorRepository>().To<FornecedorRepository>();
            kernel.Bind<ISubcategoriaProdutoRepository>().To<SubcategoriaProdutoRepository>();
            kernel.Bind<IProdutoEstoqueFilialRepository>().To<ProdutoEstoqueFilialRepository>();
            kernel.Bind<IProdutoMovimentoEstoqueRepository>().To<ProdutoMovimentoEstoqueRepository>();
            kernel.Bind<IProdutoOrigemRepository>().To<ProdutoOrigemRepository>();
            kernel.Bind<IProdutoTabelaPrecoRepository>().To<ProdutoTabelaPrecoRepository>();
            kernel.Bind<ITipoImagemRepository>().To<TipoImagemRepository>();
            kernel.Bind<IPacientePerguntaRepository>().To<PacientePerguntaRepository>();
            kernel.Bind<IPacienteAnamneseImagemRepository>().To<PacienteAnamneseImagemRepository>();
            kernel.Bind<ISubProcedimentoRepository>().To<SubProcedimentoRepository>();
            kernel.Bind<ISubProcedimentoAnexoRepository>().To<SubProcedimentoAnexoRepository>();

        }
    }
}