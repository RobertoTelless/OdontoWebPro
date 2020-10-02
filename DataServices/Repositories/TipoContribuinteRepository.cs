using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class TipoContribuinteRepository : RepositoryBase<TIPO_CONTRIBUINTE>, ITipoContribuinteRepository
    {
        public TIPO_CONTRIBUINTE GetItemById(Int32 id)
        {
            IQueryable<TIPO_CONTRIBUINTE> query = Db.TIPO_CONTRIBUINTE;
            query = query.Where(p => p.TICO_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<TIPO_CONTRIBUINTE> GetAllItens()
        {
            IQueryable<TIPO_CONTRIBUINTE> query = Db.TIPO_CONTRIBUINTE.Where(p => p.TICO_IN_ATIVO == 1);
            return query.ToList();
        }

        public List<TIPO_CONTRIBUINTE> GetAllItensAdm()
        {
            IQueryable<TIPO_CONTRIBUINTE> query = Db.TIPO_CONTRIBUINTE;
            return query.ToList();
        }

    }
}
