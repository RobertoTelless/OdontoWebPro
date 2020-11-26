using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface ITipoProcedimentoAppService : IAppServiceBase<TIPO_PROCEDIMENTO>
    {
        Int32 ValidateCreate(TIPO_PROCEDIMENTO item, USUARIO usuario);
        Int32 ValidateEdit(TIPO_PROCEDIMENTO item, TIPO_PROCEDIMENTO itemAntes, USUARIO usuario);
        Int32 ValidateEdit(TIPO_PROCEDIMENTO item, TIPO_PROCEDIMENTO itemAntes);
        Int32 ValidateDelete(TIPO_PROCEDIMENTO item, USUARIO usuario);
        Int32 ValidateReativar(TIPO_PROCEDIMENTO item, USUARIO usuario);

        TIPO_PROCEDIMENTO CheckExist(TIPO_PROCEDIMENTO obj, Int32? idAss);
        TIPO_PROCEDIMENTO GetItemById(Int32 id);
        List<TIPO_PROCEDIMENTO> GetAllItens(Int32 idAss);
        List<TIPO_PROCEDIMENTO> GetAllItensAdm(Int32 idAss);
        Int32 ExecuteFilter(String nome, String descricao, Int32 idAss, out List<TIPO_PROCEDIMENTO> objeto);
        TIPO_PROCEDIMENTO_ANEXO GetAnexoById(Int32 id);
    }
}
