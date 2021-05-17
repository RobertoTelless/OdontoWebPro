using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface ISubProcedimentoService : IServiceBase<SUB_PROCEDIMENTO>
    {
        Int32 Create(SUB_PROCEDIMENTO item, LOG log);
        Int32 Create(SUB_PROCEDIMENTO item);
        Int32 Edit(SUB_PROCEDIMENTO item, LOG log);
        Int32 Edit(SUB_PROCEDIMENTO item);
        Int32 Delete(SUB_PROCEDIMENTO item, LOG log);

        SUB_PROCEDIMENTO CheckExist(SUB_PROCEDIMENTO item, Int32? idAss);
        SUB_PROCEDIMENTO GetItemById(Int32 id);
        List<SUB_PROCEDIMENTO> GetAllItens(Int32 idAss);
        List<SUB_PROCEDIMENTO> GetAllItensAdm(Int32 idAss);
        List<SUB_PROCEDIMENTO> ExecuteFilter(String nome, String descricao, Int32 idAss);
        SUB_PROCEDIMENTO_ANEXO GetAnexoById(Int32 id);
    }
}
