using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class CategoriaPacienteRepository : RepositoryBase<CATEGORIA_PACIENTE>, ICategoriaPacienteRepository
    {
        public CATEGORIA_PACIENTE GetItemById(Int32 id)
        {
            IQueryable<CATEGORIA_PACIENTE> query = Db.CATEGORIA_PACIENTE;
            query = query.Where(p => p.CAPA_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<CATEGORIA_PACIENTE> GetAllItens(Int32 idAss)
        {
            IQueryable<CATEGORIA_PACIENTE> query = Db.CATEGORIA_PACIENTE.Where(p => p.CAPA_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

        public List<CATEGORIA_PACIENTE> GetAllItensAdm(Int32 idAss)
        {
            IQueryable<CATEGORIA_PACIENTE> query = Db.CATEGORIA_PACIENTE;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }
    }
}
