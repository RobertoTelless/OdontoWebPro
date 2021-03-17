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
    public class PacienteAcompanhamentoRepository : RepositoryBase<PACIENTE_ACOMPANHAMENTO>, IPacienteAcompanhamentoRepository
    {
        public List<PACIENTE_ACOMPANHAMENTO> GetAllItens(Int32? idAss)
        {
            return Db.PACIENTE_ACOMPANHAMENTO.ToList();
        }

        public PACIENTE_ACOMPANHAMENTO GetItemById(Int32 id)
        {
            IQueryable<PACIENTE_ACOMPANHAMENTO> query = Db.PACIENTE_ACOMPANHAMENTO.Where(p => p.PAAC_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
