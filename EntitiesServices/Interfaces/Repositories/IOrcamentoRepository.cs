using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IOrcamentoRepository : IRepositoryBase<ORCAMENTO>
    {
        ORCAMENTO CheckExist(ORCAMENTO item, Int32? idAss);
        ORCAMENTO GetItemById(Int32 id);
        List<ORCAMENTO> GetAllItens(Int32? idAss);
        List<ORCAMENTO> GetAllItensAdm(Int32? idAss);
        List<ORCAMENTO> ExecuteFilter(Int32? PaciId, Int32? status, DateTime data, String nome, Int32? idAss);
    }
}
