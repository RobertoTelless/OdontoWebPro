using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface ITipoTagService : IServiceBase<TIPO_TAG>
    {
        Int32 Create(TIPO_TAG item, LOG log);
        Int32 Create(TIPO_TAG item);
        Int32 Edit(TIPO_TAG item, LOG log);
        Int32 Edit(TIPO_TAG item);
        Int32 Delete(TIPO_TAG item, LOG log);

        TIPO_TAG GetItemById(Int32 id);
        List<TIPO_TAG> GetAllItens(Int32 idAss);
        List<TIPO_TAG> GetAllItensAdm(Int32 idAss);
    }
}
