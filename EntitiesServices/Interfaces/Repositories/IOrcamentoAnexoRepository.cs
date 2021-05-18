using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IOrcamentoAnexoRepository : IRepositoryBase<ORCAMENTO_ANEXO>
    {
        List<ORCAMENTO_ANEXO> GetAllItens(Int32? idAss);
        ORCAMENTO_ANEXO GetItemById(Int32 id);
    }
}
