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
    public class TipoProcedimentoRepository : RepositoryBase<TIPO_PROCEDIMENTO>, ITipoProcedimentoRepository
    {
        public TIPO_PROCEDIMENTO CheckExist(TIPO_PROCEDIMENTO conta, Int32? idAss)
        {
            IQueryable<TIPO_PROCEDIMENTO> query = Db.TIPO_PROCEDIMENTO;
            query = query.Where(p => p.TIPR_NM_NOME == conta.TIPR_NM_NOME);
            query = query.Where(p => p.FILI_CD_ID == conta.FILI_CD_ID);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.FirstOrDefault();
        }

        public TIPO_PROCEDIMENTO GetItemById(Int32 id)
        {
            IQueryable<TIPO_PROCEDIMENTO> query = Db.TIPO_PROCEDIMENTO;
            query = query.Where(p => p.TIPR_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<TIPO_PROCEDIMENTO> GetAllItens(Int32 idAss)
        {
            IQueryable<TIPO_PROCEDIMENTO> query = Db.TIPO_PROCEDIMENTO.Where(p => p.TIPR_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.OrderByDescending(a => a.TIPR_SG_SIGLA);
            return query.ToList();
        }

        public List<TIPO_PROCEDIMENTO> GetAllItensAdm(Int32 idAss)
        {
            IQueryable<TIPO_PROCEDIMENTO> query = Db.TIPO_PROCEDIMENTO;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.OrderByDescending(a => a.TIPR_SG_SIGLA);
            return query.ToList();
        }

        public List<TIPO_PROCEDIMENTO> ExecuteFilter(String nome, String descricao, Int32 idAss)
        {
            List<TIPO_PROCEDIMENTO> lista = new List<TIPO_PROCEDIMENTO>();
            IQueryable<TIPO_PROCEDIMENTO> query = Db.TIPO_PROCEDIMENTO;
            if (!String.IsNullOrEmpty(nome))
            {
                query = query.Where(p => p.TIPR_NM_NOME.Contains(nome));
            }
            if (!String.IsNullOrEmpty(descricao))
            {
                query = query.Where(p => p.TIPR_DS_DESCRICAO.Contains(descricao));
            }
            if (query != null)
            {
                query = query.Where(p => p.ASSI_CD_ID == idAss);
                query = query.OrderBy(a => a.TIPR_SG_SIGLA);
                lista = query.ToList<TIPO_PROCEDIMENTO>();
            }
            return lista;
        }
    }
}
