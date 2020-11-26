using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class DenteRegiaoRepository : RepositoryBase<REGIAO_DENTE>, IDenteRegiaoRepository
    {
        public REGIAO_DENTE GetItemById(Int32 id)
        {
            IQueryable<REGIAO_DENTE> query = Db.REGIAO_DENTE;
            query = query.Where(p => p.REDE_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<REGIAO_DENTE> GetAllItens(Int32 idAss)
        {
            IQueryable<REGIAO_DENTE> query = Db.REGIAO_DENTE.Where(p => p.REDE_IN_ATIVO == 1);
            return query.ToList();
        }

        public List<REGIAO_DENTE> GetAllItensAdm(Int32 idAss)
        {
            IQueryable<REGIAO_DENTE> query = Db.REGIAO_DENTE;
            return query.ToList();
        }

    }
}
