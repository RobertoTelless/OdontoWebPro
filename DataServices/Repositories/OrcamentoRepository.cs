using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;
using CrossCutting;
using System.ComponentModel.Design;

namespace DataServices.Repositories
{
    public class OrcamentoRepository : RepositoryBase<ORCAMENTO>, IOrcamentoRepository
    {
        public ORCAMENTO CheckExist(ORCAMENTO conta, Int32? idAss)
        {
            IQueryable<ORCAMENTO> query = Db.ORCAMENTO;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.FirstOrDefault();
        }

        public ORCAMENTO GetItemById(Int32 id)
        {
            IQueryable<ORCAMENTO> query = Db.ORCAMENTO;
            query = query.Where(p => p.ORCA_CD_ID == id);
            query = query.Include(p => p.ORCAMENTO_ACOMPANHAMENTO);
            query = query.Include(p => p.ORCAMENTO_ANEXO);
            query = query.Include(p => p.ORCAMENTO_ITEM);
            query = query.Include(p => p.PACIENTE);
            return query.FirstOrDefault();
        }

        public List<ORCAMENTO> GetAllItens(Int32? idAss)
        {
            IQueryable<ORCAMENTO> query = Db.ORCAMENTO.Where(p => p.ORCA_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

        public List<ORCAMENTO> GetAllItensAdm(Int32? idAss)
        {
            IQueryable<ORCAMENTO> query = Db.ORCAMENTO;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

        public List<ORCAMENTO> ExecuteFilter(Int32? paciId, Int32? status, DateTime data, String nome, Int32? idAss)
        {
            List<ORCAMENTO> lista = new List<ORCAMENTO>();
            IQueryable<ORCAMENTO> query = Db.ORCAMENTO;
            if (paciId != null)
            {
                query = query.Where(p => p.PACI_CD_ID == paciId);
            }
            if (status != null)
            {
                query = query.Where(p => p.ORCA_IN_STATUS == status);
            }
            if (nome != null)
            {
                query = query.Where(p => p.ORCA_NM_NOME.Contains(nome));
            }
            if (data != DateTime.MinValue)
            {
                query = query.Where(p => p.ORCA_DT_DATA == data);
            }
            if (query != null)
            {
                query = query.Where(p => p.ORCA_IN_ATIVO == 1);
                query = query.Where(p => p.ASSI_CD_ID == idAss);
                query = query.OrderBy(a => a.ORCA_DT_DATA);
                lista = query.ToList<ORCAMENTO>();
            }
            return lista;
        }
    }
}
