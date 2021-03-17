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
    public class PacienteAnexoRepository : RepositoryBase<PACIENTE_ANEXO>, IPacienteAnexoRepository
    {
        public List<PACIENTE_ANEXO> GetAllItens(Int32? idAss)
        {
            return Db.PACIENTE_ANEXO.ToList();
        }

        public PACIENTE_ANEXO GetItemById(Int32 id)
        {
            IQueryable<PACIENTE_ANEXO> query = Db.PACIENTE_ANEXO.Where(p => p.PAAN_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
