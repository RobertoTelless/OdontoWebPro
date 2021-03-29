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
    public class PacienteAnamneseImagemRepository : RepositoryBase<PACIENTE_ANAMESE_IMAGEM>, IPacienteAnamneseImagemRepository
    {
        public List<PACIENTE_ANAMESE_IMAGEM> GetAllItens(Int32? idAss)
        {
            return Db.PACIENTE_ANAMESE_IMAGEM.ToList();
        }

        public PACIENTE_ANAMESE_IMAGEM GetItemById(Int32 id)
        {
            IQueryable<PACIENTE_ANAMESE_IMAGEM> query = Db.PACIENTE_ANAMESE_IMAGEM.Where(p => p.PCAI_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
