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
    public class TipoImagemAppService : AppServiceBase<TIPO_IMAGEM>, ITipoImagemAppService
    {
        private readonly ITipoImagemService _baseService;

        public TipoImagemAppService(ITipoImagemService baseService) : base(baseService)
        {
            _baseService = baseService;
        }

        public List<TIPO_IMAGEM> GetAllItens(Int32 idAss)
        {
            List<TIPO_IMAGEM> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<TIPO_IMAGEM> GetAllItensAdm(Int32 idAss)
        {
            List<TIPO_IMAGEM> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public TIPO_IMAGEM GetItemById(Int32 id)
        {
            TIPO_IMAGEM item = _baseService.GetItemById(id);
            return item;
        }

        public Int32 ValidateCreate(TIPO_IMAGEM item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia

                // Completa objeto

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddTIIM",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_IMAGEM>(item)
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

        public Int32 ValidateEdit(TIPO_IMAGEM item, TIPO_IMAGEM itemAntes, USUARIO usuario)
        {
            try
            {
                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "EditTIIM",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<TIPO_IMAGEM>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<TIPO_IMAGEM>(itemAntes)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(TIPO_IMAGEM item, TIPO_IMAGEM itemAntes)
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

        public Int32 ValidateDelete(TIPO_IMAGEM item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                if (item.PACIENTE_ANAMESE_IMAGEM.Count > 0)
                {
                    return 1;
                }

                // Acerta campos

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DeleTIM",
                    LOG_TX_REGISTRO = "Tipo de Imagem: " + item.TPIM_NM_NOME
                };

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(TIPO_IMAGEM item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatTIIM",
                    LOG_TX_REGISTRO = "Tipo de Imagem: " + item.TPIM_NM_NOME
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
