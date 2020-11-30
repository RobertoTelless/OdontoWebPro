using System;
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
    public class TipoProcedimentoAppService : AppServiceBase<TIPO_PROCEDIMENTO>, ITipoProcedimentoAppService
    {
        private readonly ITipoProcedimentoService _baseService;

        public TipoProcedimentoAppService(ITipoProcedimentoService baseService): base(baseService)
        {
            _baseService = baseService;
        }

        public TIPO_PROCEDIMENTO CheckExist(TIPO_PROCEDIMENTO obj, Int32? idAss)
        {
            TIPO_PROCEDIMENTO item = _baseService.CheckExist(obj, idAss);
            return item;
        }

        public List<TIPO_PROCEDIMENTO> GetAllItens(Int32 idAss)
        {
            List<TIPO_PROCEDIMENTO> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<TIPO_PROCEDIMENTO> GetAllItensAdm(Int32 idAss)
        {
            List<TIPO_PROCEDIMENTO> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public TIPO_PROCEDIMENTO GetItemById(Int32 id)
        {
            TIPO_PROCEDIMENTO item = _baseService.GetItemById(id);
            return item;
        }

        public TIPO_PROCEDIMENTO_ANEXO GetAnexoById(Int32 id)
        {
            TIPO_PROCEDIMENTO_ANEXO lista = _baseService.GetAnexoById(id);
            return lista;
        }

        public Int32 ExecuteFilter(String nome, String descricao, Int32? idFilial, Int32 idAss, out List<TIPO_PROCEDIMENTO> objeto)
        {
            try
            {
                objeto = new List<TIPO_PROCEDIMENTO>();
                Int32 volta = 0;

                // Processa filtro
                objeto = _baseService.ExecuteFilter(nome, descricao, idFilial, idAss);
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

        public Int32 ValidateCreate(TIPO_PROCEDIMENTO item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia
                if (_baseService.CheckExist(item, usuario.ASSI_CD_ID) != null)
                {
                    return 1;
                }

                // Completa objeto
                item.TIPR_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "AddTIPR",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_PROCEDIMENTO>(item)
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

        public Int32 ValidateEdit(TIPO_PROCEDIMENTO item, TIPO_PROCEDIMENTO itemAntes, USUARIO usuario)
        {
            try
            {
                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "EditTIPR",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_PROCEDIMENTO>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<TIPO_PROCEDIMENTO>(itemAntes)
                };

                // Persiste
                item.ASSINANTE = null;
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(TIPO_PROCEDIMENTO item, TIPO_PROCEDIMENTO itemAntes)
        {
            try
            {

                // Persiste
                item.ASSINANTE = null;
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateDelete(TIPO_PROCEDIMENTO item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                if (item.ORCAMENTO_ITEM.Count > 0)
                {
                    return 1;
                }

                // Acerta campos
                item.TIPR_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DelTIPR",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_PROCEDIMENTO>(item)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(TIPO_PROCEDIMENTO item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.TIPR_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatTIPR",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_PROCEDIMENTO>(item)
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
