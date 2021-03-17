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
    public class PacienteRecomendacaoRepository : RepositoryBase<PACIENTE_RECOMENDACAO>, IPacienteRecomendacaoRepository
    {
        public List<PACIENTE_RECOMENDACAO> GetAllItens(Int32? idAss)
        {
            return Db.PACIENTE_RECOMENDACAO.ToList();
        }

        public PACIENTE_RECOMENDACAO GetItemById(Int32 id)
        {
            IQueryable<PACIENTE_RECOMENDACAO> query = Db.PACIENTE_RECOMENDACAO.Where(p => p.RECO_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
