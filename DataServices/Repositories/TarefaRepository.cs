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
    public class TarefaRepository : RepositoryBase<TAREFA>, ITarefaRepository
    {
        public TAREFA CheckExist(TAREFA tarefa, Int32 idUsu)
        {
            IQueryable<TAREFA> query = Db.TAREFA;
            query = query.Where(p => p.TARE_NM_TITULO == tarefa.TARE_NM_TITULO);
            query = query.Where(p => p.TARE_DT_CADASTRO == tarefa.TARE_DT_CADASTRO);
            query = query.Where(p => p.USUA_CD_ID == idUsu);
            return query.FirstOrDefault();
        }

        public List<TAREFA> GetByDate(DateTime data, Int32 idUsu)
        {
            IQueryable<TAREFA> query = Db.TAREFA.Where(p => p.TARE_IN_ATIVO == 1);
            query = query.Where(p => p.TARE_DT_CADASTRO == data);
            query = query.Where(p => p.USUA_CD_ID == idUsu);
            return query.ToList();
        }

        public List<TAREFA> GetByUser(Int32 user)
        {
            IQueryable<TAREFA> query = Db.TAREFA.Where(p => p.TARE_IN_ATIVO == 1);
            query = query.Where(p => p.USUA_CD_ID == user || p.TARE_CD_USUA_1 == user || p.TARE_CD_USUA_2 == user || p.TARE_CD_USUA_3 == user);
            return query.ToList();
        }

        public List<TAREFA> GetTarefaStatus(Int32 user, Int32 tipo)
        {
            IQueryable<TAREFA> query = Db.TAREFA.Where(p => p.TARE_IN_ATIVO == 1);
            query = query.Where(p => p.USUA_CD_ID == user || p.TARE_CD_USUA_1 == user || p.TARE_CD_USUA_2 == user || p.TARE_CD_USUA_3 == user);
            if (tipo == 1)
            {
                query = query.Where(p => p.TARE_IN_STATUS == 1);
            }
            if (tipo == 2)
            {
                query = query.Where(p => p.TARE_IN_STATUS == 4);
            }
            return query.ToList();
        }

        public TAREFA GetItemById(Int32 id)
        {
            IQueryable<TAREFA> query = Db.TAREFA;
            query = query.Where(p => p.TARE_CD_ID == id);
            query = query.Include(p => p.TAREFA_NOTIFICACAO);
            query = query.Include(p => p.USUARIO);
            return query.FirstOrDefault();
        }

        public List<TAREFA> GetAllItens(Int32 idUsu)
        {
            IQueryable<TAREFA> query = Db.TAREFA.Where(p => p.TARE_CD_ID == 1);
            query = query.Where(p => p.USUA_CD_ID == idUsu);
            return query.ToList();
        }

        public List<TAREFA> GetAllItensAdm(Int32 idUsu)
        {
            IQueryable<TAREFA> query = Db.TAREFA;
            query = query.Where(p => p.USUA_CD_ID == idUsu);
            return query.ToList();
        }

        public List<TAREFA> ExecuteFilter(Int32? tipoId, String titulo, DateTime? data, Int32 encerrado, Int32 prioridade, Int32 idUsu)
        {
            List<TAREFA> lista = new List<TAREFA>();
            IQueryable<TAREFA> query = Db.TAREFA;
            if (tipoId != null)
            {
                query = query.Where(p => p.TITR_CD_ID == tipoId);
            }
            if (!String.IsNullOrEmpty(titulo))
            {
                query = query.Where(p => p.TARE_NM_TITULO.Contains(titulo));
            }
            if (data != null)
            {
                query = query.Where(p => DbFunctions.TruncateTime(p.TARE_DT_CADASTRO) == DbFunctions.TruncateTime(data));
            }
            if (encerrado != 0)
            {
                query = query.Where(p => p.TARE_IN_STATUS == encerrado);
            }
            if (prioridade != 0)
            {
                query = query.Where(p => p.TARE_IN_PRIORIDADE == prioridade);
            }
            if (query != null)
            {
                query = query.Where(p => p.USUA_CD_ID == idUsu);
                query = query.OrderBy(a => a.TARE_DT_CADASTRO);
                lista = query.ToList<TAREFA>();
            }
            return lista;
        }
    }
}
