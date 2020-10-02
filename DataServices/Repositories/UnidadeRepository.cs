using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using ModelServices.Interfaces.Repositories;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class UnidadeRepository : RepositoryBase<UNIDADE>, IUnidadeRepository
    {
        public UNIDADE GetItemById(Int32 id)
        {
            IQueryable<UNIDADE> query = Db.UNIDADE;
            query = query.Where(p => p.UNID_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<UNIDADE> GetAllItensAdm()
        {
            IQueryable<UNIDADE> query = Db.UNIDADE;
            return query.ToList();
        }

        public List<UNIDADE> GetAllItens()
        {
            IQueryable<UNIDADE> query = Db.UNIDADE.Where(p => p.UNID_IN_ATIVO == 1);
            return query.ToList();
        }

    }
}
 