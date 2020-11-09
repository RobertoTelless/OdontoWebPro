using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IProdutoAnexoRepository : IRepositoryBase<PRODUTO_ANEXO>
    {
        List<PRODUTO_ANEXO> GetAllItens(Int32? idAss);
        PRODUTO_ANEXO GetItemById(Int32 id);
    }
}
