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
    public class TipoContribuinteService : ServiceBase<TIPO_CONTRIBUINTE>, ITipoContribuinteService
    {
        private readonly ITipoContribuinteRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        protected Odonto_DBEntities Db = new Odonto_DBEntities();

        public TipoContribuinteService(ITipoContribuinteRepository baseRepository, ILogRepository logRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
        }

        public TIPO_CONTRIBUINTE GetItemById(Int32 id)
        {
            TIPO_CONTRIBUINTE item = _baseRepository.GetItemById(id);
            return item;
        }

        public List<TIPO_CONTRIBUINTE> GetAllItens(Int32 idAss)
        {
            return _baseRepository.GetAllItens(idAss);
        }

        public List<TIPO_CONTRIBUINTE> GetAllItensAdm(Int32 idAss)
        {
            return _baseRepository.GetAllItensAdm(idAss);
        }
    
        public Int32 Create(TIPO_CONTRIBUINTE item, LOG log)
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

        public Int32 Create(TIPO_CONTRIBUINTE item)
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


        public Int32 Edit(TIPO_CONTRIBUINTE item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    TIPO_CONTRIBUINTE obj = _baseRepository.GetById(item.TICO_CD_ID);
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

        public Int32 Edit(TIPO_CONTRIBUINTE item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    TIPO_CONTRIBUINTE obj = _baseRepository.GetById(item.TICO_CD_ID);
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

        public Int32 Delete(TIPO_CONTRIBUINTE item, LOG log)
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

    }
}
