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
    public class SubProcedimentoService : ServiceBase<SUB_PROCEDIMENTO>, ISubProcedimentoService
    {
        private readonly ISubProcedimentoRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly ISubProcedimentoAnexoRepository _anexoRepository;
        protected Odonto_DBEntities Db = new Odonto_DBEntities();

        public SubProcedimentoService(ISubProcedimentoRepository baseRepository, ILogRepository logRepository, ISubProcedimentoAnexoRepository anexoRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _anexoRepository = anexoRepository;
        }

        public SUB_PROCEDIMENTO CheckExist(SUB_PROCEDIMENTO obj, Int32? idAss)
        {
            SUB_PROCEDIMENTO item = _baseRepository.CheckExist(obj, idAss);
            return item;
        }

        public SUB_PROCEDIMENTO GetItemById(Int32 id)
        {
            SUB_PROCEDIMENTO item = _baseRepository.GetItemById(id);
            return item;
        }

        public List<SUB_PROCEDIMENTO> GetAllItens(Int32 idAss)
        {
            return _baseRepository.GetAllItens(idAss);
        }

        public List<SUB_PROCEDIMENTO> GetAllItensAdm(Int32 idAss)
        {
            return _baseRepository.GetAllItensAdm(idAss);
        }

        public SUB_PROCEDIMENTO_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public List<SUB_PROCEDIMENTO> ExecuteFilter(String nome, String descricao, Int32 idAss)
        {
            return _baseRepository.ExecuteFilter(nome, descricao, idAss);

        }

        public Int32 Create(SUB_PROCEDIMENTO item, LOG log)
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

        public Int32 Create(SUB_PROCEDIMENTO item)
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


        public Int32 Edit(SUB_PROCEDIMENTO item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    SUB_PROCEDIMENTO obj = _baseRepository.GetById(item.SUPR_CD_ID);
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

        public Int32 Edit(SUB_PROCEDIMENTO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    SUB_PROCEDIMENTO obj = _baseRepository.GetById(item.SUPR_CD_ID);
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

        public Int32 Delete(SUB_PROCEDIMENTO item, LOG log)
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
