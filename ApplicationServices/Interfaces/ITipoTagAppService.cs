using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface ITipoTagAppService : IAppServiceBase<TIPO_TAG>
    {
        Int32 ValidateCreate(TIPO_TAG item, USUARIO usuario);
        Int32 ValidateEdit(TIPO_TAG item, TIPO_TAG itemAntes, USUARIO usuario);
        Int32 ValidateEdit(TIPO_TAG item, TIPO_TAG itemAntes);
        Int32 ValidateDelete(TIPO_TAG item, USUARIO usuario);
        Int32 ValidateReativar(TIPO_TAG item, USUARIO usuario);

        List<TIPO_TAG> GetAllItens(Int32 idAss);
        List<TIPO_TAG> GetAllItensAdm(Int32 idAss);
        TIPO_TAG GetItemById(Int32 id);
    }
}
