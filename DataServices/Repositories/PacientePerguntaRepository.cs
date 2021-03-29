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
    public class PacientePerguntaRepository : RepositoryBase<PACIENTE_ANAMNESE_PERGUNTA>, IPacientePerguntaRepository
    {
        public List<PACIENTE_ANAMNESE_PERGUNTA> GetAllItens(Int32? idAss)
        {
            return Db.PACIENTE_ANAMNESE_PERGUNTA.ToList();
        }

        public PACIENTE_ANAMNESE_PERGUNTA GetItemById(Int32 id)
        {
            IQueryable<PACIENTE_ANAMNESE_PERGUNTA> query = Db.PACIENTE_ANAMNESE_PERGUNTA.Where(p => p.PCAN_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
