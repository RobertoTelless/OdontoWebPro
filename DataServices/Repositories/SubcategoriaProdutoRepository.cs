using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class SubcategoriaProdutoRepository : RepositoryBase<SUBCATEGORIA_PRODUTO>, ISubcategoriaProdutoRepository
    {
        public SUBCATEGORIA_PRODUTO GetItemById(Int32 id)
        {
            IQueryable<SUBCATEGORIA_PRODUTO> query = Db.SUBCATEGORIA_PRODUTO;
            query = query.Where(p => p.SUPR_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<SUBCATEGORIA_PRODUTO> GetAllItens(Int32 idAss)
        {
            IQueryable<SUBCATEGORIA_PRODUTO> query = Db.SUBCATEGORIA_PRODUTO.Where(p => p.SUPR_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

        public List<SUBCATEGORIA_PRODUTO> GetAllItensAdm(Int32 idAss)
        {
            IQueryable<SUBCATEGORIA_PRODUTO> query = Db.SUBCATEGORIA_PRODUTO;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }
    }
}
