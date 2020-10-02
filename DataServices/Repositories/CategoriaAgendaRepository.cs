using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class CategoriaAgendaRepository : RepositoryBase<CATEGORIA_AGENDA>, ICategoriaAgendaRepository
    {
        public CATEGORIA_AGENDA GetItemById(Int32 id)
        {
            IQueryable<CATEGORIA_AGENDA> query = Db.CATEGORIA_AGENDA;
            query = query.Where(p => p.CAAG_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<CATEGORIA_AGENDA> GetAllItens()
        {
            IQueryable<CATEGORIA_AGENDA> query = Db.CATEGORIA_AGENDA.Where(p => p.CAAG_IN_ATIVO == 1);
            return query.ToList();
        }

        public List<CATEGORIA_AGENDA> GetAllItensAdm()
        {
            IQueryable<CATEGORIA_AGENDA> query = Db.CATEGORIA_AGENDA;
            return query.ToList();
        }
    }
}
