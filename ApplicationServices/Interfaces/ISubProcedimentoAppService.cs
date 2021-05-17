using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface ISubProcedimentoAppService : IAppServiceBase<SUB_PROCEDIMENTO>
    {
        Int32 ValidateCreate(SUB_PROCEDIMENTO item, USUARIO usuario);
        Int32 ValidateEdit(SUB_PROCEDIMENTO item, SUB_PROCEDIMENTO itemAntes, USUARIO usuario);
        Int32 ValidateEdit(SUB_PROCEDIMENTO item, SUB_PROCEDIMENTO itemAntes);
        Int32 ValidateDelete(SUB_PROCEDIMENTO item, USUARIO usuario);
        Int32 ValidateReativar(SUB_PROCEDIMENTO item, USUARIO usuario);

        SUB_PROCEDIMENTO CheckExist(SUB_PROCEDIMENTO obj, Int32? idAss);
        SUB_PROCEDIMENTO GetItemById(Int32 id);
        List<SUB_PROCEDIMENTO> GetAllItens(Int32 idAss);
        List<SUB_PROCEDIMENTO> GetAllItensAdm(Int32 idAss);
        Int32 ExecuteFilter(String nome, String descricao, Int32 idAss, out List<SUB_PROCEDIMENTO> objeto);
        SUB_PROCEDIMENTO_ANEXO GetAnexoById(Int32 id);
    }
}
