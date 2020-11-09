using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IMovimentoEstoqueProdutoRepository : IRepositoryBase<MOVIMENTO_ESTOQUE_PRODUTO>
    {
        MOVIMENTO_ESTOQUE_PRODUTO GetItemById(Int32? id);
        List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItens(Int32? idAss);
        List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensEntrada(Int32? idAss);
        List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensSaida(Int32? idAss);
        List<MOVIMENTO_ESTOQUE_PRODUTO> ExecuteFilter(Int32? catId, String nome, String barcode, Int32? filiId, DateTime? dtMov, Int32? idAss);
        List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensUserDataMes(Int32 idusu, DateTime data,  Int32? idAss);
        List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensUserDataDia(Int32 idusu, DateTime data, Int32? idAss);
    }
}
