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
    public class SubProcedimentoAppService : AppServiceBase<SUB_PROCEDIMENTO>, ISubProcedimentoAppService
    {
        private readonly ISubProcedimentoService _baseService;

        public SubProcedimentoAppService(ISubProcedimentoService baseService): base(baseService)
        {
            _baseService = baseService;
        }

        public SUB_PROCEDIMENTO CheckExist(SUB_PROCEDIMENTO obj, Int32? idAss)
        {
            SUB_PROCEDIMENTO item = _baseService.CheckExist(obj, idAss);
            return item;
        }

        public List<SUB_PROCEDIMENTO> GetAllItens(Int32 idAss)
        {
            List<SUB_PROCEDIMENTO> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<SUB_PROCEDIMENTO> GetAllItensAdm(Int32 idAss)
        {
            List<SUB_PROCEDIMENTO> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public SUB_PROCEDIMENTO GetItemById(Int32 id)
        {
            SUB_PROCEDIMENTO item = _baseService.GetItemById(id);
            return item;
        }

        public SUB_PROCEDIMENTO_ANEXO GetAnexoById(Int32 id)
        {
            SUB_PROCEDIMENTO_ANEXO lista = _baseService.GetAnexoById(id);
            return lista;
        }

        public Int32 ExecuteFilter(String nome, String descricao, Int32 idAss, out List<SUB_PROCEDIMENTO> objeto)
        {
            try
            {
                objeto = new List<SUB_PROCEDIMENTO>();
                Int32 volta = 0;

                // Processa filtro
                objeto = _baseService.ExecuteFilter(nome, descricao, idAss);
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

        public Int32 ValidateCreate(SUB_PROCEDIMENTO item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia
                if (_baseService.CheckExist(item, usuario.ASSI_CD_ID) != null)
                {
                    return 1;
                }

                // Completa objeto
                item.SUPR_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "AddSUPR",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<SUB_PROCEDIMENTO>(item)
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

        public Int32 ValidateEdit(SUB_PROCEDIMENTO item, SUB_PROCEDIMENTO itemAntes, USUARIO usuario)
        {
            try
            {
                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "EditSUPR",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<SUB_PROCEDIMENTO>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<SUB_PROCEDIMENTO>(itemAntes)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(SUB_PROCEDIMENTO item, SUB_PROCEDIMENTO itemAntes)
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

        public Int32 ValidateDelete(SUB_PROCEDIMENTO item, USUARIO usuario)
        {
            try
            {
                // Acerta campos
                item.SUPR_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DelSUPR",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<SUB_PROCEDIMENTO>(item)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(SUB_PROCEDIMENTO item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.SUPR_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatSUPR",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<SUB_PROCEDIMENTO>(item)
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
