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
    public class ProdutoRepository : RepositoryBase<PRODUTO>, IProdutoRepository
    {
        public PRODUTO CheckExist(PRODUTO conta, Int32? idAss)
        {
            IQueryable<PRODUTO> query = Db.PRODUTO;
            query = query.Where(p => p.PROD_NM_NOME == conta.PROD_NM_NOME);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.FirstOrDefault();
        }

        public PRODUTO GetByNome(String nome, Int32? idAss)
        {
            IQueryable<PRODUTO> query = Db.PRODUTO.Where(p => p.PROD_IN_ATIVO == 1);
            query = query.Where(p => p.PROD_NM_NOME == nome);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Include(p => p.PRODUTO_FORNECEDOR);
            query = query.Include(p => p.PRODUTO_ANEXO);
            query = query.Include(p => p.MOVIMENTO_ESTOQUE_PRODUTO);
            return query.FirstOrDefault();
        }

        public PRODUTO GetItemById(Int32 id)
        {
            IQueryable<PRODUTO> query = Db.PRODUTO;
            query = query.Where(p => p.PROD_CD_ID == id);
            query = query.Include(p => p.PRODUTO_FORNECEDOR);
            query = query.Include(p => p.PRODUTO_ANEXO);
            query = query.Include(p => p.MOVIMENTO_ESTOQUE_PRODUTO);
            return query.FirstOrDefault();
        }

        public List<PRODUTO> GetAllItens(Int32? idAss)
        {
            IQueryable<PRODUTO> query = Db.PRODUTO.Where(p => p.PROD_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Include(p => p.MOVIMENTO_ESTOQUE_PRODUTO);
            return query.ToList();
        }

        public List<PRODUTO> GetAllItensAdm(Int32? idAss)
        {
            IQueryable<PRODUTO> query = Db.PRODUTO;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Include(p => p.MOVIMENTO_ESTOQUE_PRODUTO);
            return query.ToList();
        }

        //public List<PRODUTO> GetPontoPedido(Int32? idAss)
        //{
        //    IQueryable<PRODUTO> query = Db.PRODUTO.Where(p => p.PROD_IN_ATIVO == 1);
        //    query = query.Where(p => (p.PRODUTO_ESTOQUE_FILIAL.Where(m => m.FILI_CD_ID == m.FILI_CD_ID)) < p.PROD_QN_QUANTIDADE_MINIMA);
        //    query = query.Where(p => p.ASSI_CD_ID == idAss);
        //    query = query.Where(p => p.PROD_IN_COMPOSTO == 0);
        //    return query.ToList();
        //}

        //public List<PRODUTO> GetEstoqueZerado()
        //{
        //    Int32? idAss = SessionMocks.IdAssinante;
        //    IQueryable<PRODUTO> query = Db.PRODUTO.Where(p => p.PROD_IN_ATIVO == 1);
        //    query = query.Where(p => p.PROD_QN_ESTOQUE == 0);
        //    query = query.Where(p => p.ASSI_CD_ID == idAss);
        //    query = query.Where(p => p.PROD_IN_COMPOSTO == 0);
        //    return query.ToList();
        //}

        public List<PRODUTO> ExecuteFilter(Int32? catId, String barcode, String nome, String marca, String codigo, String modelo, String fabricante, Int32? idAss)
        {
            List<PRODUTO> lista = new List<PRODUTO>();
            IQueryable<PRODUTO> query = Db.PRODUTO;
            if (catId != null)
            {
                query = query.Where(p => p.CAPR_CD_ID == catId);
            }
            if (!String.IsNullOrEmpty(nome))
            {
                query = query.Where(p => p.PROD_NM_NOME.Contains(nome));
            }
            if (!String.IsNullOrEmpty(marca))
            {
                query = query.Where(p => p.PROD_NM_MARCA.Contains(marca));
            }
            if (!String.IsNullOrEmpty(codigo))
            {
                query = query.Where(p => p.PROD_NR_BARCODE == codigo);
            }
            if (!String.IsNullOrEmpty(codigo))
            {
                query = query.Where(p => p.PROD_CD_CODIGO.Contains(codigo));
            }
            if (query != null)
            {
                query = query.Where(p => p.PROD_IN_ATIVO == 1);
                query = query.Where(p => p.ASSI_CD_ID == idAss);
                query = query.OrderBy(a => a.PROD_NM_NOME);
                lista = query.ToList<PRODUTO>();
            }
            return lista;
        }

        //public List<PRODUTO_ESTOQUE_FILIAL> ExecuteFilterEstoque(Int32? filial, String nome, String marca, String codigo)
        //{
        //    Int32? idAss = SessionMocks.IdAssinante;
        //    List<PRODUTO_ESTOQUE_FILIAL> lista = new List<PRODUTO_ESTOQUE_FILIAL>();
        //    IQueryable<PRODUTO_ESTOQUE_FILIAL> query = Db.PRODUTO_ESTOQUE_FILIAL;
        //    if (filial != null)
        //    {
        //        query = query.Where(p => p.FILI_CD_ID == filial);
        //    }
        //    if (!String.IsNullOrEmpty(nome))
        //    {
        //        query = query.Where(p => p.PRODUTO.PROD_NM_NOME.Contains(nome));
        //    }
        //    if (!String.IsNullOrEmpty(marca))
        //    {
        //        query = query.Where(p => p.PRODUTO.PROD_NM_MARCA.Contains(marca));
        //    }
        //    if (!String.IsNullOrEmpty(codigo))
        //    {
        //        query = query.Where(p => p.PRODUTO.PROD_NR_BARCODE == codigo);
        //    }
        //    if (query != null)
        //    {
        //        query = query.Where(p => p.PREF_IN_ATIVO == 1);
        //        query = query.Where(p => p.PRODUTO.ASSI_CD_ID == idAss);
        //        query = query.OrderBy(a => a.PRODUTO.PROD_NM_NOME);
        //        lista = query.ToList<PRODUTO_ESTOQUE_FILIAL>();
        //    }
        //    return lista;
        //}

        //public List<PRODUTO_ESTOQUE_FILIAL> RecuperarQuantidadesFiliais(Int32? idFilial)
        //{
        //    Int32? idAss = SessionMocks.IdAssinante;
        //    IQueryable<PRODUTO_ESTOQUE_FILIAL> query = Db.PRODUTO_ESTOQUE_FILIAL.Where(p => p.PREF_IN_ATIVO == 1);
        //    query = query.Where(p => p.PRODUTO.ASSI_CD_ID == idAss);
        //    if (idFilial != null)
        //    {
        //        query = query.Where(p => p.FILI_CD_ID == idFilial);
        //    }
        //    query = query.Include(p => p.PRODUTO);
        //    return query.ToList();
        //}
    }
}
