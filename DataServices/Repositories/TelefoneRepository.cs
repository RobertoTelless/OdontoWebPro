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
    public class TelefoneRepository : RepositoryBase<TELEFONE>, ITelefoneRepository
    {
        public TELEFONE GetItemById(Int32 id)
        {
            IQueryable<TELEFONE> query = Db.TELEFONE;
            query = query.Where(p => p.TELE_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<TELEFONE> GetAllItens(Int32 idAss)
        {
            IQueryable<TELEFONE> query = Db.TELEFONE.Where(p => p.TELE_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.OrderBy(a => a.TELE_NM_NOME);
            return query.ToList();
        }

        public List<TELEFONE> GetAllItensAdm(Int32 idAss)
        {
            IQueryable<TELEFONE> query = Db.TELEFONE;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.OrderBy(a => a.TELE_NM_NOME);
            return query.ToList();
        }

        public List<TELEFONE> ExecuteFilter(Int32? tipo, String nome, String telefone, String celular, Int32 idAss)
        {
            List<TELEFONE> lista = new List<TELEFONE>();
            IQueryable<TELEFONE> query = Db.TELEFONE;
            if (!String.IsNullOrEmpty(nome))
            {
                query = query.Where(p => p.TELE_NM_NOME.Contains(nome));
            }
            if (tipo != null)
            {
                query = query.Where(p => p.CATE_CD_ID == tipo);
            }
            if (!String.IsNullOrEmpty(telefone))
            {
                query = query.Where(p => p.TELE_NR_TELEFONE.Contains(telefone));
            }
            if (!String.IsNullOrEmpty(celular))
            {
                query = query.Where(p => p.TELE_NR_CELULAR.Contains(celular));
            }
            if (query != null)
            {
                query = query.Where(p => p.ASSI_CD_ID == idAss);
                query = query.OrderBy(a => a.TELE_NM_NOME);
                lista = query.ToList<TELEFONE>();
            }
            return lista;
        }
    }
}
