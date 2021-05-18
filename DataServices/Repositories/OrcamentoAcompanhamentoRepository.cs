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
    public class OrcamentoAcompanhamentoRepository : RepositoryBase<ORCAMENTO_ACOMPANHAMENTO>, IOrcamentoAcompanhamentoRepository
    {
        public List<ORCAMENTO_ACOMPANHAMENTO> GetAllItens(Int32? idAss)
        {
            return Db.ORCAMENTO_ACOMPANHAMENTO.ToList();
        }

        public ORCAMENTO_ACOMPANHAMENTO GetItemById(Int32 id)
        {
            IQueryable<ORCAMENTO_ACOMPANHAMENTO> query = Db.ORCAMENTO_ACOMPANHAMENTO.Where(p => p.ORAC_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
