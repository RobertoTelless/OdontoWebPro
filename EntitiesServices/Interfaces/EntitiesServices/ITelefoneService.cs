using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface ITelefoneService : IServiceBase<TELEFONE>
    {
        Int32 Create(TELEFONE item, LOG log);
        Int32 Create(TELEFONE item);
        Int32 Edit(TELEFONE item, LOG log);
        Int32 Edit(TELEFONE item);
        Int32 Delete(TELEFONE item, LOG log);

        TELEFONE GetItemById(Int32 id);
        List<TELEFONE> GetAllItens(Int32 idAss);
        List<TELEFONE> GetAllItensAdm(Int32 idAss);
        List<TELEFONE> ExecuteFilter(Int32? tipo, String nome, String telefone, String celular, Int32 idAss);
        List<CATEGORIA_TELEFONE> GetAllCategorias(Int32 idAss);
    }
}
