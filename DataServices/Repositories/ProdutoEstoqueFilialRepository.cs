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
    public class ProdutoEstoqueFilialRepository : RepositoryBase<PRODUTO_ESTOQUE_FILIAL>, IProdutoEstoqueFilialRepository
    {
        public PRODUTO_ESTOQUE_FILIAL GetItemById(Int32 id)
        {
            IQueryable<PRODUTO_ESTOQUE_FILIAL> query = Db.PRODUTO_ESTOQUE_FILIAL;
            query = query.Where(p => p.PREF_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
