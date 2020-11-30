using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface ITipoProcedimentoService : IServiceBase<TIPO_PROCEDIMENTO>
    {
        Int32 Create(TIPO_PROCEDIMENTO item, LOG log);
        Int32 Create(TIPO_PROCEDIMENTO item);
        Int32 Edit(TIPO_PROCEDIMENTO item, LOG log);
        Int32 Edit(TIPO_PROCEDIMENTO item);
        Int32 Delete(TIPO_PROCEDIMENTO item, LOG log);

        TIPO_PROCEDIMENTO CheckExist(TIPO_PROCEDIMENTO item, Int32? idAss);
        TIPO_PROCEDIMENTO GetItemById(Int32 id);
        List<TIPO_PROCEDIMENTO> GetAllItens(Int32 idAss);
        List<TIPO_PROCEDIMENTO> GetAllItensAdm(Int32 idAss);
        List<TIPO_PROCEDIMENTO> ExecuteFilter(String nome, String descricao, Int32? idFilial, Int32 idAss);
        TIPO_PROCEDIMENTO_ANEXO GetAnexoById(Int32 id);
    }
}
