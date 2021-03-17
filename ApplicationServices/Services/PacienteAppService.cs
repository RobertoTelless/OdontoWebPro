using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;
using ApplicationServices.Interfaces;
using ModelServices.Interfaces.EntitiesServices;
using CrossCutting;
using System.Text.RegularExpressions;

namespace ApplicationServices.Services
{
    public class PacienteAppService : AppServiceBase<PACIENTE>, IPacienteAppService
    {
        private readonly IPacienteService _baseService;

        public PacienteAppService(IPacienteService baseService) : base(baseService)
        {
            _baseService = baseService;
        }

        public List<PACIENTE> GetAllItens(Int32? idAss)
        {
            List<PACIENTE> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<PACIENTE> GetAllItensAdm(Int32? idAss)
        {
            List<PACIENTE> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public PACIENTE GetItemById(Int32 id)
        {
            PACIENTE item = _baseService.GetItemById(id);
            return item;
        }

        public UF GetUFBySigla(String sigla)
        {
            UF item = _baseService.GetUFBySigla(sigla);
            return item;
        }

        public PACIENTE GetByNome(String nome, Int32? idAss)
        {
            PACIENTE item = _baseService.GetByNome(nome, idAss);
            return item;
        }

        public PACIENTE CheckExist(PACIENTE conta, Int32? idAss)
        {
            PACIENTE item = _baseService.CheckExist(conta, idAss);
            return item;
        }

        public List<FILIAL> GetAllFiliais(Int32 idAss)
        {
            List<FILIAL> lista = _baseService.GetAllFiliais(idAss);
            return lista;
        }

        public List<CATEGORIA_PACIENTE> GetAllTipos(Int32 idAss)
        {
            List<CATEGORIA_PACIENTE> lista = _baseService.GetAllTipos(idAss);
            return lista;
        }

        public List<UF> GetAllUF()
        {
            List<UF> lista = _baseService.GetAllUF();
            return lista;
        }

        public PACIENTE_ANEXO GetAnexoById(Int32 id)
        {
            PACIENTE_ANEXO lista = _baseService.GetAnexoById(id);
            return lista;
        }

        public PACIENTE_ACOMPANHAMENTO GetAcompanhamentoById(Int32 id)
        {
            PACIENTE_ACOMPANHAMENTO lista = _baseService.GetAcompanhamentoById(id);
            return lista;
        }

        public Int32 ExecuteFilter(Int32? catId, Int32? filialId, String nome, String cpf, String telefone, String celular, String cidade, DateTime dataNasc, String email, Int32? idAss, out List<PACIENTE> objeto)
        {
            try
            {
                objeto = new List<PACIENTE>();
                Int32 volta = 0;

                // Processa filtro
                objeto = _baseService.ExecuteFilter(catId, filialId, nome, cpf, telefone, celular, cidade, dataNasc, email, idAss);
                if (objeto.Count == 0)
                {
                    volta = 1;
                }
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateCreate(PACIENTE item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia
                if (_baseService.CheckExist(item, usuario.ASSI_CD_ID) != null)
                {
                    return 1;
                }

                // Completa objeto
                item.PACI_IN_ATIVO = 1;
                item.ASSI_CD_ID = usuario.ASSI_CD_ID;
                item.PACI_DT_CADASTRO = DateTime.Today.Date;
                item.PACI_VL_SALDO_FINANCEIRO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddPACI",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<PACIENTE>(item)
                };

                // Persiste
                Int32 volta = _baseService.Create(item, log);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(PACIENTE item, PACIENTE itemAntes, USUARIO usuario)
        {
            try
            {

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "EditPACI",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<PACIENTE>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<PACIENTE>(itemAntes)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(PACIENTE item, PACIENTE itemAntes)
        {
            try
            {
                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateDelete(PACIENTE item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                if (item.ORCAMENTO.Count > 0)
                {
                    return 1;
                }

                // Acerta campos
                item.PACI_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DelPACI",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<PACIENTE>(item)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(PACIENTE item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.PACI_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatPACI",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<PACIENTE>(item)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public PACIENTE_PRESCRICAO GetPrescricaoById(Int32 id)
        {
            PACIENTE_PRESCRICAO lista = _baseService.GetPrescricaoById(id);
            return lista;
        }

        public Int32 EditPrescricao(PACIENTE_PRESCRICAO item)
        {
            try
            {
                // Persiste
                return _baseService.EditPrescricao(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 CreatePrescricao(PACIENTE_PRESCRICAO item)
        {
            try
            {
                // Persiste
                item.PRES_IN_ATIVO = 1;
                Int32 volta = _baseService.CreatePrescricao(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public PACIENTE_RECOMENDACAO GetRecomendacaoById(Int32 id)
        {
            PACIENTE_RECOMENDACAO lista = _baseService.GetRecomendacaoById(id);
            return lista;
        }

        public Int32 EditRecomendacao(PACIENTE_RECOMENDACAO item)
        {
            try
            {
                // Persiste
                return _baseService.EditRecomendacao(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 CreateRecomendacao(PACIENTE_RECOMENDACAO item)
        {
            try
            {
                // Persiste
                item.RECO_IN_ATIVO = 1;
                Int32 volta = _baseService.CreateRecomendacao(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
