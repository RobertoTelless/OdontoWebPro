using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IRegimeTributarioService : IServiceBase<REGIME_TRIBUTARIO>
    {
        Int32 Create(REGIME_TRIBUTARIO item, LOG log);
        Int32 Create(REGIME_TRIBUTARIO item);
        Int32 Edit(REGIME_TRIBUTARIO item, LOG log);
        Int32 Edit(REGIME_TRIBUTARIO item);
        Int32 Delete(REGIME_TRIBUTARIO item, LOG log);

        REGIME_TRIBUTARIO GetItemById(Int32 id);
        List<REGIME_TRIBUTARIO> GetAllItens(Int32 idAss);
        List<REGIME_TRIBUTARIO> GetAllItensAdm(Int32 idAss);
    }
}
