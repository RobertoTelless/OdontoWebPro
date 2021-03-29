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
    public class PacienteService : ServiceBase<PACIENTE>, IPacienteService
    {
        private readonly IPacienteRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly IFilialRepository _filialRepository;
        private readonly IPacienteAnexoRepository _anexoRepository;
        private readonly IUFRepository _ufRepository;
        private readonly IPacienteAcompanhamentoRepository _paRepository;
        private readonly IPacientePrescricaoRepository _ppRepository;
        private readonly IPacienteRecomendacaoRepository _prRepository;
        private readonly ICategoriaPacienteRepository _catRepository;
        private readonly IPacientePerguntaRepository _pgRepository;
        private readonly IPacienteAnamneseImagemRepository _imRepository;
        private readonly ITipoImagemRepository _tiRepository;
        protected Odonto_DBEntities Db = new Odonto_DBEntities();

        public PacienteService(IPacienteRepository baseRepository, ILogRepository logRepository, IFilialRepository filialRepository, IPacienteAnexoRepository anexoRepository, IUFRepository ufRepository, IPacienteAcompanhamentoRepository paRepository, IPacientePrescricaoRepository ppRepository, IPacienteRecomendacaoRepository prRepository, ICategoriaPacienteRepository catRepository, ITipoImagemRepository tiRepository, IPacientePerguntaRepository pgRepository, IPacienteAnamneseImagemRepository imRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _filialRepository = filialRepository;
            _anexoRepository = anexoRepository;
            _ufRepository = ufRepository;
            _paRepository = paRepository;
            _ppRepository = ppRepository;
            _prRepository = prRepository;
            _catRepository = catRepository;
            _tiRepository = tiRepository;
            _pgRepository = pgRepository;
            _imRepository = imRepository;
        }

        public PACIENTE CheckExist(PACIENTE conta, Int32? idAss)
        {
            PACIENTE item = _baseRepository.CheckExist(conta, idAss);
            return item;
        }

        public PACIENTE GetItemById(Int32 id)
        {
            PACIENTE item = _baseRepository.GetItemById(id);
            return item;
        }

        public PACIENTE GetByNome(String nome, Int32? idAss)
        {
            PACIENTE item = _baseRepository.GetByNome(nome, idAss);
            return item;
        }

        public UF GetUFBySigla(String sigla)
        {
            UF item = _ufRepository.GetItemBySigla(sigla);
            return item;
        }

        public List<PACIENTE> GetAllItens(Int32? idAss)
        {
            return _baseRepository.GetAllItens(idAss);
        }

        public List<PACIENTE> GetAllItensAdm(Int32? idAss)
        {
            return _baseRepository.GetAllItensAdm(idAss);
        }

        public List<FILIAL> GetAllFiliais(Int32 idAss)
        {
            return _filialRepository.GetAllItens(idAss);
        }

        public List<CATEGORIA_PACIENTE> GetAllTipos(Int32 idAss)
        {
            return _catRepository.GetAllItens(idAss);
        }

        public List<TIPO_IMAGEM> GetAllTipoImagem(Int32 idAss)
        {
            return _tiRepository.GetAllItens(idAss);
        }

        public List<UF> GetAllUF()
        {
            return _ufRepository.GetAllItens();
        }

        public PACIENTE_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public PACIENTE_ACOMPANHAMENTO GetAcompanhamentoById(Int32 id)
        {
            return _paRepository.GetItemById(id);
        }

        public List<PACIENTE> ExecuteFilter(Int32? catId, Int32? filialId, String nome, String cpf, String telefone, String celular, String cidade, DateTime dataNasc, String email, Int32? idAss)
        {
            List<PACIENTE> lista = _baseRepository.ExecuteFilter(catId, filialId, nome, cpf, telefone, celular, cidade, dataNasc, email, idAss);
            return lista;
        }

        public Int32 Create(PACIENTE item, LOG log)
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

        public Int32 Create(PACIENTE item)
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


        public Int32 Edit(PACIENTE item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PACIENTE obj = _baseRepository.GetById(item.PACI_CD_ID);
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

        public Int32 Edit(PACIENTE item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PACIENTE obj = _baseRepository.GetById(item.PACI_CD_ID);
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

        public Int32 Delete(PACIENTE item, LOG log)
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

        public PACIENTE_PRESCRICAO GetPrescricaoById(Int32 id)
        {
            return _ppRepository.GetItemById(id);
        }

        public Int32 EditPrescricao(PACIENTE_PRESCRICAO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PACIENTE_PRESCRICAO obj = _ppRepository.GetById(item.PRES_CD_ID);
                    _ppRepository.Detach(obj);
                    _ppRepository.Update(item);
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

        public Int32 CreatePrescricao(PACIENTE_PRESCRICAO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _ppRepository.Add(item);
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

        public PACIENTE_RECOMENDACAO GetRecomendacaoById(Int32 id)
        {
            return _prRepository.GetItemById(id);
        }

        public Int32 EditRecomendacao(PACIENTE_RECOMENDACAO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PACIENTE_RECOMENDACAO obj = _prRepository.GetById(item.RECO_CD_ID);
                    _prRepository.Detach(obj);
                    _prRepository.Update(item);
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

        public Int32 CreateRecomendacao(PACIENTE_RECOMENDACAO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _prRepository.Add(item);
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

        public PACIENTE_ANAMNESE_PERGUNTA GetPerguntaById(Int32 id)
        {
            return _pgRepository.GetItemById(id);
        }

        public Int32 EditPergunta(PACIENTE_ANAMNESE_PERGUNTA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PACIENTE_ANAMNESE_PERGUNTA obj = _pgRepository.GetById(item.PCAN_CD_ID);
                    _pgRepository.Detach(obj);
                    _pgRepository.Update(item);
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

        public Int32 CreatePergunta(PACIENTE_ANAMNESE_PERGUNTA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _pgRepository.Add(item);
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

        public PACIENTE_ANAMESE_IMAGEM GetAnamneseImagemById(Int32 id)
        {
            return _imRepository.GetItemById(id);
        }

        public Int32 EditAnamneseImagem(PACIENTE_ANAMESE_IMAGEM item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PACIENTE_ANAMESE_IMAGEM obj = _imRepository.GetById(item.PCAI_CD_ID);
                    _imRepository.Detach(obj);
                    _imRepository.Update(item);
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

        public Int32 CreateAnamneseImagem(PACIENTE_ANAMESE_IMAGEM item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _imRepository.Add(item);
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
