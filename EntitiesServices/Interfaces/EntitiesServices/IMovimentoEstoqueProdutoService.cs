using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IMovimentoEstoqueProdutoService : IServiceBase<MOVIMENTO_ESTOQUE_PRODUTO>
    {
        Int32 Create(MOVIMENTO_ESTOQUE_PRODUTO perfil, LOG log);
        Int32 Create(MOVIMENTO_ESTOQUE_PRODUTO perfil);
        List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItens(Int32? idAss);
        MOVIMENTO_ESTOQUE_PRODUTO GetItemById(Int32 id);
        List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensEntrada(Int32? idAss);
        List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensSaida(Int32? idAss);
        List<MOVIMENTO_ESTOQUE_PRODUTO> ExecuteFilter(Int32? catId, String nome, String barcode, Int32? filiId, DateTime? dtMov, Int32? idAss);
    }
}
