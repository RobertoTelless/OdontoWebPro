using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IProdutoFornecedorRepository : IRepositoryBase<PRODUTO_FORNECEDOR>
    {
        List<PRODUTO_FORNECEDOR> GetAllItens(Int32? idAss);
        PRODUTO_FORNECEDOR GetItemById(Int32 id);
    }
}
