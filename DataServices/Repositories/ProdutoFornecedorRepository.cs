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
    public class ProdutoFornecedorRepository : RepositoryBase<PRODUTO_FORNECEDOR>, IProdutoFornecedorRepository
    {
        public List<PRODUTO_FORNECEDOR> GetAllItens(Int32? idAss)
        {
            return Db.PRODUTO_FORNECEDOR.ToList();
        }

        public PRODUTO_FORNECEDOR GetItemById(Int32 id)
        {
            IQueryable<PRODUTO_FORNECEDOR> query = Db.PRODUTO_FORNECEDOR.Where(p => p.PRFO_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
