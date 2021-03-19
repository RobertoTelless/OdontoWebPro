using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using ModelServices.Interfaces.Repositories;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class FilialRepository : RepositoryBase<FILIAL>, IFilialRepository
    {
        public FILIAL CheckExist(FILIAL conta)
        {
            IQueryable<FILIAL> query = Db.FILIAL;
            query = query.Where(p => p.FILI_NM_NOME == conta.FILI_NM_NOME);
            query = query.Where(p => p.FILI_NR_CNPJ == conta.FILI_NR_CNPJ);
            return query.FirstOrDefault();
        }

        public FILIAL GetItemById(Int32 id)
        {
            IQueryable<FILIAL> query = Db.FILIAL;
            query = query.Where(p => p.FILI_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<FILIAL> GetAllItens(Int32 idMatriz)
        {
            IQueryable<FILIAL> query = Db.FILIAL.Where(p => p.FILI_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idMatriz);
            return query.ToList();
        }

        public List<FILIAL> GetAllItensAdm(Int32 idMatriz)
        {
            IQueryable<FILIAL> query = Db.FILIAL;
            query = query.Where(p => p.ASSI_CD_ID == idMatriz);
            return query.ToList();
        }
    }
}
 