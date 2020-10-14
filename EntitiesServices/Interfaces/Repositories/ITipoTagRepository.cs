using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ITipoTagRepository : IRepositoryBase<TIPO_TAG>
    {
        List<TIPO_TAG> GetAllItens(Int32 idAss);
        TIPO_TAG GetItemById(Int32 id);
        List<TIPO_TAG> GetAllItensAdm(Int32 idAss);
    }
}
