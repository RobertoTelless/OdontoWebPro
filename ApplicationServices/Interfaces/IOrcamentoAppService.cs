using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IOrcamentoAppService : IAppServiceBase<ORCAMENTO>
    {
        Int32 ValidateCreate(ORCAMENTO perfil, USUARIO usuario);
        Int32 ValidateEdit(ORCAMENTO perfil, ORCAMENTO perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(ORCAMENTO item, ORCAMENTO itemAntes);
        Int32 ValidateDelete(ORCAMENTO perfil, USUARIO usuario);
        Int32 ValidateReativar(ORCAMENTO perfil, USUARIO usuario);

        List<ORCAMENTO> GetAllItens(Int32? idAss);
        List<ORCAMENTO> GetAllItensAdm(Int32? idAss);
        ORCAMENTO GetItemById(Int32 id);
        ORCAMENTO CheckExist(ORCAMENTO conta, Int32? idAss);
        Int32 ExecuteFilter(Int32? paciId, Int32? status, DateTime data, String nome, Int32? idAss, out List<ORCAMENTO> objeto);

        List<TIPO_PROCEDIMENTO> GetAllProcs(Int32 idAss);
        List<SUB_PROCEDIMENTO> GetAllSubs(Int32 idAss);
        List<REGIAO_DENTE> GetAllRegioes(Int32 idAss);
        ORCAMENTO_ANEXO GetAnexoById(Int32 id);
        ORCAMENTO_ACOMPANHAMENTO GetAcompanhamentoById(Int32 id);

        ORCAMENTO_ITEM GetItemOrcamentoById(Int32 id);
        Int32 EditItemOrcamento(ORCAMENTO_ITEM item);
        Int32 CreateItemOrcamento(ORCAMENTO_ITEM item);
    }
}
