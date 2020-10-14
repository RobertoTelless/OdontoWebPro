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
    public class TipoTagAppService : AppServiceBase<TIPO_TAG>, ITipoTagAppService
    {
        private readonly ITipoTagService _baseService;

        public TipoTagAppService(ITipoTagService baseService): base(baseService)
        {
            _baseService = baseService;
        }

        public List<TIPO_TAG> GetAllItens(Int32 idAss)
        {
            List<TIPO_TAG> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<TIPO_TAG> GetAllItensAdm(Int32 idAss)
        {
            List<TIPO_TAG> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public TIPO_TAG GetItemById(Int32 id)
        {
            TIPO_TAG item = _baseService.GetItemById(id);
            return item;
        }

        public Int32 ValidateCreate(TIPO_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia

                // Completa objeto
                item.TITA_IN_ATIVO = 1;
                item.TITA_IN_TIPO = 1;
                item.ASSI_CD_ID = usuario.ASSI_CD_ID;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "AddTITA",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_TAG>(item)
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

        public Int32 ValidateEdit(TIPO_TAG item, TIPO_TAG itemAntes, USUARIO usuario)
        {
            try
            {
                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "EditTITA",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_TAG>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<TIPO_TAG>(itemAntes)
                };

                // Persiste
                item.TITA_IN_TIPO = 1;
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(TIPO_TAG item, TIPO_TAG itemAntes)
        {
            try
            {
                // Persiste
                item.TITA_IN_TIPO = 1;
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateDelete(TIPO_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                if (item.NOTICIA_TAG.Count > 0)
                {
                    return 1;
                }

                // Acerta campos
                item.TITA_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DeleTITA",
                    LOG_TX_REGISTRO = "Tipo: " + item.TITA_NM_NOME
                };

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(TIPO_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.TITA_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatTITA",
                    LOG_TX_REGISTRO = "Tipo: " + item.TITA_NM_NOME
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
