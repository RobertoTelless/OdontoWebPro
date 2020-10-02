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
    public class TipoContribuinteAppService : AppServiceBase<TIPO_CONTRIBUINTE>, ITipoContribuinteAppService
    {
        private readonly ITipoContribuinteService _baseService;

        public TipoContribuinteAppService(ITipoContribuinteService baseService): base(baseService)
        {
            _baseService = baseService;
        }

        public List<TIPO_CONTRIBUINTE> GetAllItens()
        {
            List<TIPO_CONTRIBUINTE> lista = _baseService.GetAllItens();
            return lista;
        }

        public List<TIPO_CONTRIBUINTE> GetAllItensAdm()
        {
            List<TIPO_CONTRIBUINTE> lista = _baseService.GetAllItensAdm();
            return lista;
        }

        public TIPO_CONTRIBUINTE GetItemById(Int32 id)
        {
            TIPO_CONTRIBUINTE item = _baseService.GetItemById(id);
            return item;
        }

        public Int32 ValidateCreate(TIPO_CONTRIBUINTE item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia

                // Completa objeto
                item.TICO_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddTICO",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_CONTRIBUINTE>(item)
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

        public Int32 ValidateEdit(TIPO_CONTRIBUINTE item, TIPO_CONTRIBUINTE itemAntes, USUARIO usuario)
        {
            try
            {
                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "EditTICO",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_CONTRIBUINTE>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<TIPO_CONTRIBUINTE>(itemAntes)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(TIPO_CONTRIBUINTE item, TIPO_CONTRIBUINTE itemAntes)
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

        public Int32 ValidateDelete(TIPO_CONTRIBUINTE item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                //if (item.CLIENTE.Count > 0)
                //{
                //    return 1;
                //}

                // Acerta campos
                item.TICO_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatTICO",
                    LOG_TX_REGISTRO = "Tipo: " + item.TICO_NM_NOME
                };

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(TIPO_CONTRIBUINTE item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.TICO_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatTICO",
                    LOG_TX_REGISTRO = "Tipo: " + item.TICO_NM_NOME
                };

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
