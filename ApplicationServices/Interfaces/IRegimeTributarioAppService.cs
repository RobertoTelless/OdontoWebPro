using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IRegimeTributarioAppService : IAppServiceBase<REGIME_TRIBUTARIO>
    {
        Int32 ValidateCreate(REGIME_TRIBUTARIO item, USUARIO usuario);
        Int32 ValidateEdit(REGIME_TRIBUTARIO item, REGIME_TRIBUTARIO itemAntes, USUARIO usuario);
        Int32 ValidateEdit(REGIME_TRIBUTARIO item, REGIME_TRIBUTARIO itemAntes);
        Int32 ValidateDelete(REGIME_TRIBUTARIO item, USUARIO usuario);
        Int32 ValidateReativar(REGIME_TRIBUTARIO item, USUARIO usuario);

        List<REGIME_TRIBUTARIO> GetAllItens();
        List<REGIME_TRIBUTARIO> GetAllItensAdm();
        REGIME_TRIBUTARIO GetItemById(Int32 id);
    }
}
