using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface  IProdutoEstoqueFilialAppService : IAppServiceBase<PRODUTO_ESTOQUE_FILIAL>
    {
        Int32 ValidateCreate(PRODUTO_ESTOQUE_FILIAL item, USUARIO usuario);
        Int32 ValidateEdit(PRODUTO_ESTOQUE_FILIAL item, PRODUTO_ESTOQUE_FILIAL itemAntes, USUARIO usuario);
        PRODUTO_ESTOQUE_FILIAL CheckExist(PRODUTO_ESTOQUE_FILIAL item, Int32 idAss);
        PRODUTO_ESTOQUE_FILIAL GetByProdFilial(Int32 prod, Int32 fili);
        PRODUTO_ESTOQUE_FILIAL GetItemById(Int32 id);
        PRODUTO_ESTOQUE_FILIAL GetItemById(PRODUTO item);
    }
}
