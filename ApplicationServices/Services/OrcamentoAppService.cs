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
    public class OrcamentoAppService : AppServiceBase<ORCAMENTO>, IOrcamentoAppService
    {
        private readonly IOrcamentoService _baseService;

        public OrcamentoAppService(IOrcamentoService baseService) : base(baseService)
        {
            _baseService = baseService;
        }

        public List<ORCAMENTO> GetAllItens(Int32? idAss)
        {
            List<ORCAMENTO> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<ORCAMENTO> GetAllItensAdm(Int32? idAss)
        {
            List<ORCAMENTO> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public ORCAMENTO GetItemById(Int32 id)
        {
            ORCAMENTO item = _baseService.GetItemById(id);
            return item;
        }

        public ORCAMENTO CheckExist(ORCAMENTO conta, Int32? idAss)
        {
            ORCAMENTO item = _baseService.CheckExist(conta, idAss);
            return item;
        }

        public List<TIPO_PROCEDIMENTO> GetAllProcs(Int32 idAss)
        {
            List<TIPO_PROCEDIMENTO> lista = _baseService.GetAllProcs(idAss);
            return lista;
        }

        public List<SUB_PROCEDIMENTO> GetAllSubs(Int32 idAss)
        {
            List<SUB_PROCEDIMENTO> lista = _baseService.GetAllSubs(idAss);
            return lista;
        }

        public List<REGIAO_DENTE> GetAllRegioes(Int32 idAss)
        {
            List<REGIAO_DENTE> lista = _baseService.GetAllRegioes(idAss);
            return lista;
        }

        public ORCAMENTO_ANEXO GetAnexoById(Int32 id)
        {
            ORCAMENTO_ANEXO lista = _baseService.GetAnexoById(id);
            return lista;
        }

        public ORCAMENTO_ACOMPANHAMENTO GetAcompanhamentoById(Int32 id)
        {
            ORCAMENTO_ACOMPANHAMENTO lista = _baseService.GetAcompanhamentoById(id);
            return lista;
        }

        public Int32 ExecuteFilter(Int32? paciId, Int32? status, DateTime data, String nome, Int32? idAss, out List<ORCAMENTO> objeto)
        {
            try
            {
                objeto = new List<ORCAMENTO>();
                Int32 volta = 0;

                // Processa filtro
                objeto = _baseService.ExecuteFilter(paciId, status, data, nome, idAss);
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

        public Int32 ValidateCreate(ORCAMENTO item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia

                // Completa objeto
                item.ORCA_IN_ATIVO = 1;
                item.ASSI_CD_ID = usuario.ASSI_CD_ID;
                item.ORCA_VL_DESCONTO = 0;
                item.ORCA_VL_FINAL = 0;
                item.ORCA_VL_VALOR = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddORCA",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<ORCAMENTO>(item)
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

        public Int32 ValidateEdit(ORCAMENTO item, ORCAMENTO itemAntes, USUARIO usuario)
        {
            try
            {

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "EditORCA",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<ORCAMENTO>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<ORCAMENTO>(itemAntes)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(ORCAMENTO item, ORCAMENTO itemAntes)
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

        public Int32 ValidateDelete(ORCAMENTO item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                if (usuario.PERF_CD_ID != 1)
                {
                    return 1;
                }
                if (item.ORCA_DT_PAGAMENTO != null)
                {
                    return 2;
                }

                // Acerta campos
                item.ORCA_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DelORCA",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<ORCAMENTO>(item)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(ORCAMENTO item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.ORCA_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatORCA",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<ORCAMENTO>(item)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ORCAMENTO_ITEM GetItemOrcamentoById(Int32 id)
        {
            ORCAMENTO_ITEM lista = _baseService.GetItemOrcamentoById(id);
            return lista;
        }

        public Int32 EditItemOrcamento(ORCAMENTO_ITEM item)
        {
            try
            {
                // Persiste
                return _baseService.EditItemOrcamento(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 CreateItemOrcamento(ORCAMENTO_ITEM item)
        {
            try
            {
                // Persiste
                item.ORIT_IN_ATIVO = 1;
                Int32 volta = _baseService.CreateItemOrcamento(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
