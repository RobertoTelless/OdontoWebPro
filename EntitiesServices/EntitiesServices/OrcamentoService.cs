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
    public class OrcamentoService : ServiceBase<ORCAMENTO>, IOrcamentoService
    {
        private readonly IOrcamentoRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly ITipoProcedimentoRepository _procRepository;
        private readonly IOrcamentoAnexoRepository _anexoRepository;
        private readonly IOrcamentoAcompanhamentoRepository _acomRepository;
        private readonly IOrcamentoItemRepository _itemRepository;
        private readonly ISubProcedimentoRepository _subRepository;
        private readonly IDenteRegiaoRepository _regRepository;
        protected Odonto_DBEntities Db = new Odonto_DBEntities();

        public OrcamentoService(IOrcamentoRepository baseRepository, ILogRepository logRepository, ITipoProcedimentoRepository procRepository, IOrcamentoAnexoRepository anexoRepository, IOrcamentoAcompanhamentoRepository acomRepository, IOrcamentoItemRepository itemRepository, ISubProcedimentoRepository subRepository, IDenteRegiaoRepository regRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _procRepository = procRepository;
            _anexoRepository = anexoRepository;
            _acomRepository = acomRepository;
            _itemRepository = itemRepository;
            _subRepository = subRepository;
            _regRepository = regRepository;
        }

        public ORCAMENTO CheckExist(ORCAMENTO conta, Int32? idAss)
        {
            ORCAMENTO item = _baseRepository.CheckExist(conta, idAss);
            return item;
        }

        public ORCAMENTO GetItemById(Int32 id)
        {
            ORCAMENTO item = _baseRepository.GetItemById(id);
            return item;
        }

        public List<ORCAMENTO> GetAllItens(Int32? idAss)
        {
            return _baseRepository.GetAllItens(idAss);
        }

        public List<ORCAMENTO> GetAllItensAdm(Int32? idAss)
        {
            return _baseRepository.GetAllItensAdm(idAss);
        }

        public List<TIPO_PROCEDIMENTO> GetAllProcs(Int32 idAss)
        {
            return _procRepository.GetAllItens(idAss);
        }

        public List<SUB_PROCEDIMENTO> GetAllSubs(Int32 idAss)
        {
            return _subRepository.GetAllItens(idAss);
        }

        public List<REGIAO_DENTE> GetAllRegioes(Int32 idAss)
        {
            return _regRepository.GetAllItens(idAss);
        }

        public ORCAMENTO_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public ORCAMENTO_ACOMPANHAMENTO GetAcompanhamentoById(Int32 id)
        {
            return _acomRepository.GetItemById(id);
        }

        public List<ORCAMENTO> ExecuteFilter(Int32? paciId, Int32? status, DateTime data, String nome, Int32? idAss)
        {
            List<ORCAMENTO> lista = _baseRepository.ExecuteFilter(paciId, status, data, nome, idAss);
            return lista;
        }

        public Int32 Create(ORCAMENTO item, LOG log)
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

        public Int32 Create(ORCAMENTO item)
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


        public Int32 Edit(ORCAMENTO item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    ORCAMENTO obj = _baseRepository.GetById(item.ORCA_CD_ID);
                    _baseRepository.Detach(obj);
                    _logRepository.Add(log);
                    _baseRepository.Update(item);
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

        public Int32 Edit(ORCAMENTO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    ORCAMENTO obj = _baseRepository.GetById(item.ORCA_CD_ID);
                    _baseRepository.Detach(obj);
                    _baseRepository.Update(item);
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

        public Int32 Delete(ORCAMENTO item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _logRepository.Add(log);
                    _baseRepository.Remove(item);
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

        public ORCAMENTO_ITEM GetItemOrcamentoById(Int32 id)
        {
            return _itemRepository.GetItemById(id);
        }

        public Int32 EditItemOrcamento(ORCAMENTO_ITEM item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    ORCAMENTO_ITEM obj = _itemRepository.GetById(item.ORIT_CD_ID);
                    _itemRepository.Detach(obj);
                    _itemRepository.Update(item);
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

        public Int32 CreateItemOrcamento(ORCAMENTO_ITEM item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _itemRepository.Add(item);
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
