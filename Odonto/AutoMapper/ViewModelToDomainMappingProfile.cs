using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EntitiesServices.Model;
using OdontoWeb.ViewModels;

namespace MvcMapping.Mappers
{
    public class ViewModelToDomainMappingProfiles : Profile
    {
        public ViewModelToDomainMappingProfiles()
        {
            CreateMap<UsuarioViewModel, USUARIO>();
            CreateMap<UsuarioLoginViewModel, USUARIO>();
            CreateMap<LogViewModel, LOG>();
            CreateMap<ConfiguracaoViewModel, CONFIGURACAO>();
            CreateMap<MatrizViewModel, MATRIZ>();
            CreateMap<FilialViewModel, FILIAL>();
            CreateMap<CargoViewModel, CARGO>();
            CreateMap<NoticiaViewModel, NOTICIA>();
            CreateMap<NoticiaComentarioViewModel, NOTICIA_COMENTARIO>();
            CreateMap<NotificacaoViewModel, NOTIFICACAO>();
            CreateMap<SexoViewModel, SEXO>();
            CreateMap<TipoPessoaViewModel, TIPO_PESSOA>();
            CreateMap<RegimeTributarioViewModel, REGIME_TRIBUTARIO>();
            CreateMap<UnidadeViewModel, UNIDADE>();
            CreateMap<TemplateViewModel, TEMPLATE>();
            CreateMap<TarefaViewModel, TAREFA>();
            CreateMap<CategoriaAgendaViewModel, CATEGORIA_AGENDA>();
            CreateMap<AgendaViewModel, AGENDA>();
            CreateMap<TarefaAcompanhamentoViewModel, TAREFA_ACOMPANHAMENTO>();
        }
    }
}