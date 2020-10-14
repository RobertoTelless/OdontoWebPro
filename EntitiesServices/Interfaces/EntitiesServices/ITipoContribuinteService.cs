using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface ITipoContribuinteService : IServiceBase<TIPO_CONTRIBUINTE>
    {
        Int32 Create(TIPO_CONTRIBUINTE item, LOG log);
        Int32 Create(TIPO_CONTRIBUINTE item);
        Int32 Edit(TIPO_CONTRIBUINTE item, LOG log);
        Int32 Edit(TIPO_CONTRIBUINTE item);
        Int32 Delete(TIPO_CONTRIBUINTE item, LOG log);

        TIPO_CONTRIBUINTE GetItemById(Int32 id);
        List<TIPO_CONTRIBUINTE> GetAllItens(Int32 idAss);
        List<TIPO_CONTRIBUINTE> GetAllItensAdm(Int32 idAss);
    }
}
