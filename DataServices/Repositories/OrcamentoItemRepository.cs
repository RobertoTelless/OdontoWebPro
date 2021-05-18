using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;
using CrossCutting;

namespace DataServices.Repositories
{
    public class OrcamentoItemRepository : RepositoryBase<ORCAMENTO_ITEM>, IOrcamentoItemRepository
    {
        public List<ORCAMENTO_ITEM> GetAllItens(Int32? idAss)
        {
            return Db.ORCAMENTO_ITEM.ToList();
        }

        public ORCAMENTO_ITEM GetItemById(Int32 id)
        {
            IQueryable<ORCAMENTO_ITEM> query = Db.ORCAMENTO_ITEM.Where(p => p.ORIT_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
