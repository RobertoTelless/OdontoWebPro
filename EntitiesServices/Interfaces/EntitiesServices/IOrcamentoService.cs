using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IOrcamentoService : IServiceBase<ORCAMENTO>
    {
        Int32 Create(ORCAMENTO perfil, LOG log);
        Int32 Create(ORCAMENTO perfil);
        Int32 Edit(ORCAMENTO perfil, LOG log);
        Int32 Edit(ORCAMENTO perfil);
        Int32 Delete(ORCAMENTO perfil, LOG log);

        ORCAMENTO CheckExist(ORCAMENTO conta, Int32? idAss);
        ORCAMENTO GetItemById(Int32 id);
        List<ORCAMENTO> GetAllItens(Int32? idAss);
        List<ORCAMENTO> GetAllItensAdm(Int32? idAss);
        List<ORCAMENTO> ExecuteFilter(Int32? paciId, Int32? status, DateTime data, String nome, Int32? idAss);

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
