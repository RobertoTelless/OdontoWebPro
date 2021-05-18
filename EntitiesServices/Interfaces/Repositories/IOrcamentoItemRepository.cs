using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IOrcamentoItemRepository : IRepositoryBase<ORCAMENTO_ITEM>
    {
        List<ORCAMENTO_ITEM> GetAllItens(Int32? idAss);
        ORCAMENTO_ITEM GetItemById(Int32 id);
    }
}
