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
    public class PacientePrescricaoRepository : RepositoryBase<PACIENTE_PRESCRICAO>, IPacientePrescricaoRepository
    {
        public List<PACIENTE_PRESCRICAO> GetAllItens(Int32? idAss)
        {
            return Db.PACIENTE_PRESCRICAO.ToList();
        }

        public PACIENTE_PRESCRICAO GetItemById(Int32 id)
        {
            IQueryable<PACIENTE_PRESCRICAO> query = Db.PACIENTE_PRESCRICAO.Where(p => p.PRES_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
