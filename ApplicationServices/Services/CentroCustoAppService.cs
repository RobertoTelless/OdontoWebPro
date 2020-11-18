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
    public class CentroCustoAppService : AppServiceBase<CENTRO_CUSTO>, ICentroCustoAppService
    {
        private readonly ICentroCustoService _baseService;

        public CentroCustoAppService(ICentroCustoService baseService) : base(baseService)
        {
            _baseService = baseService;
        }

        public List<CENTRO_CUSTO> GetAllItens(Int32? idAss)
        {
            List<CENTRO_CUSTO> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<CENTRO_CUSTO> GetAllDespesas(Int32? idAss)
        {
            List<CENTRO_CUSTO> lista = _baseService.GetAllDespesas(idAss);
            return lista;
        }

        public List<CENTRO_CUSTO> GetAllReceitas(Int32? idAss)
        {
            List<CENTRO_CUSTO> lista = _baseService.GetAllReceitas(idAss);
            return lista;
        }

        public List<CENTRO_CUSTO> GetAllItensAdm(Int32? idAss)
        {
            List<CENTRO_CUSTO> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public CENTRO_CUSTO GetItemById(Int32 id)
        {
            CENTRO_CUSTO item = _baseService.GetItemById(id);
            return item;
        }

        public CENTRO_CUSTO CheckExist(CENTRO_CUSTO obj, Int32? idAss)
        {
            CENTRO_CUSTO item = _baseService.CheckExist(obj, idAss);
            return item;
        }

        public Int32 ExecuteFilter(Int32? grupoId, Int32? subGrupoId, Int32? tipo, Int32? movimento, String numero, String nome, Int32? idAss, out List<CENTRO_CUSTO> objeto)
        {
            try
            {
                objeto = new List<CENTRO_CUSTO>();
                Int32 volta = 0;

                // Processa filtro
                objeto = _baseService.ExecuteFilter(grupoId, subGrupoId, tipo, movimento, numero, nome, idAss);
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

        public Int32 ValidateCreate(CENTRO_CUSTO item, USUARIO usuario)
        {
            try
            {
                // Completa objeto
                item.CECU_IN_ATIVO = 1;
                item.ASSI_CD_ID = usuario.ASSI_CD_ID;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddCECU",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CENTRO_CUSTO>(item)
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

        public Int32 ValidateEdit(CENTRO_CUSTO item, CENTRO_CUSTO itemAntes, USUARIO usuario)
        {
            try
            {
                // Monta Log
                item.ASSINANTE = null;
                itemAntes.ASSINANTE = null;
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "EditCECU",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CENTRO_CUSTO>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<CENTRO_CUSTO>(itemAntes)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateDelete(CENTRO_CUSTO item, USUARIO usuario)
        {
            try
            {
                // Checa integridade

                // Acerta campos
                item.CECU_IN_ATIVO = 0;
                item.ASSI_CD_ID = usuario.ASSI_CD_ID;
                item.ASSINANTE = null;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DelCECU",
                    LOG_TX_REGISTRO = "Desativa��o Centro de Custos - " + item.CECU_NM_EXIBE
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(CENTRO_CUSTO item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.CECU_IN_ATIVO = 1;
                item.ASSI_CD_ID = usuario.ASSI_CD_ID;

                // Monta Log
                item.ASSINANTE = null;
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatCECU",
                    LOG_TX_REGISTRO = "Reativa��o Centro de Custos - " + item.CECU_NM_EXIBE
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
