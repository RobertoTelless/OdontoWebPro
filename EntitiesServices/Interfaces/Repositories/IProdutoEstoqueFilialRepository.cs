using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IProdutoEstoqueFilialRepository : IRepositoryBase<PRODUTO_ANEXO>
    {
        PRODUTO_ESTOQUE_FILIAL GetItemById(Int32 id);
    }
}
