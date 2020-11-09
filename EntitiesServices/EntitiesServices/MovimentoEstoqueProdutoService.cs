using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;
using ModelServices.Interfaces.Repositories;
using ModelServices.Interfaces.EntitiesServices;
using CrossCutting;
using System.Data.Entity;
using System.Data;

namespace ModelServices.EntitiesServices
{
    public class MovimentoEstoqueProdutoService : ServiceBase<MOVIMENTO_ESTOQUE_PRODUTO>, IMovimentoEstoqueProdutoService
    {
        private readonly IMovimentoEstoqueProdutoRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly IFilialRepository _filialRepository;
        protected Odonto_DBEntities Db = new Odonto_DBEntities();

        public MovimentoEstoqueProdutoService(IMovimentoEstoqueProdutoRepository baseRepository, ILogRepository logRepository, IFilialRepository filialRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _filialRepository = filialRepository;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItens(Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseRepository.GetAllItens(idAss);
            return lista;
        }

        public MOVIMENTO_ESTOQUE_PRODUTO GetItemById(Int32 id)
        {
            MOVIMENTO_ESTOQUE_PRODUTO item = _baseRepository.GetItemById(id);
            return item;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensEntrada(Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseRepository.GetAllItensEntrada(idAss);
            return lista;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensSaida(Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseRepository.GetAllItensSaida(idAss);
            return lista;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensUserDataMes(Int32 idUsu, DateTime data,  Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseRepository.GetAllItensUserDataMes(idUsu, data, idAss);
            return lista;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensUserDataDia(Int32 idUsu, DateTime data, Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseRepository.GetAllItensUserDataDia(idUsu, data, idAss);
            return lista;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> ExecuteFilter(Int32? catId, String nome, String barcode, Int32? filiId, DateTime? dtMov, Int32? idAss)
        {
            return _baseRepository.ExecuteFilter(catId, nome, barcode, filiId, dtMov, idAss);
        }

        public Int32 Create(MOVIMENTO_ESTOQUE_PRODUTO item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _logRepository.Add(log);
                    _baseRepository.Add(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Int32 Create(MOVIMENTO_ESTOQUE_PRODUTO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _baseRepository.Add(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }
    }
}
