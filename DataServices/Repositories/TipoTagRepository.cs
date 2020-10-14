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
    public class TipoTagRepository : RepositoryBase<TIPO_TAG>, ITipoTagRepository
    {
        public TIPO_TAG GetItemById(Int32 id)
        {
            IQueryable<TIPO_TAG> query = Db.TIPO_TAG;
            query = query.Where(p => p.TITA_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<TIPO_TAG> GetAllItensAdm(Int32 idAss)
        {
            IQueryable<TIPO_TAG> query = Db.TIPO_TAG;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

        public List<TIPO_TAG> GetAllItens(Int32 idAss)
        {
            IQueryable<TIPO_TAG> query = Db.TIPO_TAG.Where(p => p.TITA_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

    }
}
 