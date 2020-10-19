using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ITelefoneRepository : IRepositoryBase<TELEFONE>
    {
        TELEFONE GetItemById(Int32 id);
        List<TELEFONE> GetAllItens(Int32 idAss);
        List<TELEFONE> GetAllItensAdm(Int32 idAss);
        List<TELEFONE> ExecuteFilter(Int32? tipo, String nome, String telefone, String celular, Int32 idAss);
    }
}
