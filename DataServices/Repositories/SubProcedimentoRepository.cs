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
    public class SubProcedimentoRepository : RepositoryBase<SUB_PROCEDIMENTO>, ISubProcedimentoRepository
    {
        public SUB_PROCEDIMENTO CheckExist(SUB_PROCEDIMENTO conta, Int32? idAss)
        {
            IQueryable<SUB_PROCEDIMENTO> query = Db.SUB_PROCEDIMENTO;
            query = query.Where(p => p.SUPR_NM_NOME == conta.SUPR_NM_NOME);
            query = query.Where(p => p.TIPR_CD_ID == conta.TIPR_CD_ID);
            return query.FirstOrDefault();
        }

        public SUB_PROCEDIMENTO GetItemById(Int32 id)
        {
            IQueryable<SUB_PROCEDIMENTO> query = Db.SUB_PROCEDIMENTO;
            query = query.Where(p => p.SUPR_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<SUB_PROCEDIMENTO> GetAllItens(Int32 idAss)
        {
            IQueryable<SUB_PROCEDIMENTO> query = Db.SUB_PROCEDIMENTO.Where(p => p.SUPR_IN_ATIVO == 1);
            query = query.OrderByDescending(a => a.SUPR_SG_SIGLA);
            return query.ToList();
        }

        public List<SUB_PROCEDIMENTO> GetAllItensAdm(Int32 idAss)
        {
            IQueryable<SUB_PROCEDIMENTO> query = Db.SUB_PROCEDIMENTO;
            query = query.OrderByDescending(a => a.SUPR_SG_SIGLA);
            return query.ToList();
        }

        public List<SUB_PROCEDIMENTO> ExecuteFilter(String nome, String descricao, Int32 idAss)
        {
            List<SUB_PROCEDIMENTO> lista = new List<SUB_PROCEDIMENTO>();
            IQueryable<SUB_PROCEDIMENTO> query = Db.SUB_PROCEDIMENTO;
            if (!String.IsNullOrEmpty(nome))
            {
                query = query.Where(p => p.SUPR_NM_NOME.Contains(nome));
            }
            if (!String.IsNullOrEmpty(descricao))
            {
                query = query.Where(p => p.SUPR_DS_DESCRICAO.Contains(descricao));
            }
            if (query != null)
            {
                query = query.OrderBy(a => a.SUPR_SG_SIGLA);
                lista = query.ToList<SUB_PROCEDIMENTO>();
            }
            return lista;
        }
    }
}
