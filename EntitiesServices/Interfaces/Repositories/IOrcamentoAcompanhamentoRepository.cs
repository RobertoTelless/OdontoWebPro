using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IOrcamentoAcompanhamentoRepository : IRepositoryBase<ORCAMENTO_ACOMPANHAMENTO>
    {
        List<ORCAMENTO_ACOMPANHAMENTO> GetAllItens(Int32? idAss);
        ORCAMENTO_ACOMPANHAMENTO GetItemById(Int32 id);
    }
}
