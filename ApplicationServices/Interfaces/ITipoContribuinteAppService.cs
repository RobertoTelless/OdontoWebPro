using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface ITipoContribuinteAppService : IAppServiceBase<TIPO_CONTRIBUINTE>
    {
        Int32 ValidateCreate(TIPO_CONTRIBUINTE item, USUARIO usuario);
        Int32 ValidateEdit(TIPO_CONTRIBUINTE item, TIPO_CONTRIBUINTE itemAntes, USUARIO usuario);
        Int32 ValidateEdit(TIPO_CONTRIBUINTE item, TIPO_CONTRIBUINTE itemAntes);
        Int32 ValidateDelete(TIPO_CONTRIBUINTE item, USUARIO usuario);
        Int32 ValidateReativar(TIPO_CONTRIBUINTE item, USUARIO usuario);

        List<TIPO_CONTRIBUINTE> GetAllItens(Int32 idAss);
        List<TIPO_CONTRIBUINTE> GetAllItensAdm(Int32 idAss);
        TIPO_CONTRIBUINTE GetItemById(Int32 id);
    }
}
