using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IDenteRegiaoService : IServiceBase<REGIAO_DENTE>
    {
        Int32 Create(REGIAO_DENTE item, LOG log);
        Int32 Create(REGIAO_DENTE item);
        Int32 Edit(REGIAO_DENTE item, LOG log);
        Int32 Edit(REGIAO_DENTE item);
        Int32 Delete(REGIAO_DENTE item, LOG log);

        REGIAO_DENTE GetItemById(Int32 id);
        List<REGIAO_DENTE> GetAllItens(Int32 idAss);
        List<REGIAO_DENTE> GetAllItensAdm(Int32 idAss);
    }
}
