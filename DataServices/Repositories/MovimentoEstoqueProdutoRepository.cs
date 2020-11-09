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
    public class MovimentoEstoqueProdutoRepository : RepositoryBase<MOVIMENTO_ESTOQUE_PRODUTO>, IMovimentoEstoqueProdutoRepository
    {
        public MOVIMENTO_ESTOQUE_PRODUTO GetItemById(Int32? id)
        {
            IQueryable<MOVIMENTO_ESTOQUE_PRODUTO> query = Db.MOVIMENTO_ESTOQUE_PRODUTO;
            query = query.Where(p => p.MOEP_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItens(Int32? idAss)
        {
            IQueryable<MOVIMENTO_ESTOQUE_PRODUTO> query = Db.MOVIMENTO_ESTOQUE_PRODUTO;
            //query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Where(p => p.MOEP_IN_ATIVO == 1);
            return query.ToList();
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensEntrada(Int32? idAss)
        {
            IQueryable<MOVIMENTO_ESTOQUE_PRODUTO> query = Db.MOVIMENTO_ESTOQUE_PRODUTO;
            //query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Where(p => p.MOEP_IN_ATIVO == 1);
            query = query.Where(p => p.MOEP_IN_TIPO_MOVIMENTO == 1);
            return query.ToList();
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensUserDataMes(Int32 idUsu, DateTime data, Int32? idAss)
        {
            IQueryable<MOVIMENTO_ESTOQUE_PRODUTO> query = Db.MOVIMENTO_ESTOQUE_PRODUTO;
            //query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Where(p => p.MOEP_IN_ATIVO == 1);
            query = query.Where(p => p.USUA_CD_ID == idUsu);
            query = query.Where(p => DbFunctions.TruncateTime(p.MOEP_DT_MOVIMENTO).Value.Month == DbFunctions.TruncateTime(data).Value.Month);
            return query.ToList();
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensUserDataDia(Int32 idUsu, DateTime data, Int32? idAss)
        {
            IQueryable<MOVIMENTO_ESTOQUE_PRODUTO> query = Db.MOVIMENTO_ESTOQUE_PRODUTO;
            //query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Where(p => p.MOEP_IN_ATIVO == 1);
            query = query.Where(p => p.USUA_CD_ID == idUsu);
            query = query.Where(p => p.MOEP_DT_MOVIMENTO == data);
            return query.ToList();
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensSaida(Int32? idAss)
        {
            IQueryable<MOVIMENTO_ESTOQUE_PRODUTO> query = Db.MOVIMENTO_ESTOQUE_PRODUTO;
            //query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Where(p => p.MOEP_IN_ATIVO == 1);
            query = query.Where(p => p.MOEP_IN_TIPO_MOVIMENTO == 2);
            return query.ToList();
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> ExecuteFilter(Int32? catId, String nome, String barcode, Int32? filiId, DateTime? dtMov, Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = new List<MOVIMENTO_ESTOQUE_PRODUTO>();
            IQueryable<MOVIMENTO_ESTOQUE_PRODUTO> query = Db.MOVIMENTO_ESTOQUE_PRODUTO;
            if (catId != null)
            {
                query = query.Where(p => p.PRODUTO.CAPR_CD_ID == catId);
            }
            if (!String.IsNullOrEmpty(nome))
            {
                query = query.Where(p => p.PRODUTO.PROD_NM_NOME == nome);
            }
            if (!String.IsNullOrEmpty(barcode))
            {
                query = query.Where(p => p.PRODUTO.PROD_NR_BARCODE == barcode);
            }
            if (filiId != null)
            {
                query = query.Where(p => p.FILI_CD_ID == filiId);
            }
            if (dtMov != new DateTime(0001, 01, 01, 00, 00, 00))
            {
                query = query.Where(p => p.MOEP_DT_MOVIMENTO == dtMov);
            }

            if (query != null)
            {
                //query = query.Where(p => p.ASSI_CD_ID == idAss);
                query = query.OrderBy(p => p.PRODUTO.PROD_NM_NOME);
                lista = query.ToList<MOVIMENTO_ESTOQUE_PRODUTO>();
            }

            return lista;
        }
    }
}
