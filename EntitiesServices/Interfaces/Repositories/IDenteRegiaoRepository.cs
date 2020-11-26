using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IDenteRegiaoRepository : IRepositoryBase<REGIAO_DENTE>
    {
        List<REGIAO_DENTE> GetAllItens(Int32 idAss);
        List<REGIAO_DENTE> GetAllItensAdm(Int32 idAss);
        REGIAO_DENTE GetItemById(Int32 id);
    }
}
