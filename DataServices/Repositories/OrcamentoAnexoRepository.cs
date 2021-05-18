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
    public class OrcamentoAnexoRepository : RepositoryBase<ORCAMENTO_ANEXO>, IOrcamentoAnexoRepository
    {
        public List<ORCAMENTO_ANEXO> GetAllItens(Int32? idAss)
        {
            return Db.ORCAMENTO_ANEXO.ToList();
        }

        public ORCAMENTO_ANEXO GetItemById(Int32 id)
        {
            IQueryable<ORCAMENTO_ANEXO> query = Db.ORCAMENTO_ANEXO.Where(p => p.ORAN_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
