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
    public class FilialAppService : AppServiceBase<FILIAL>, IFilialAppService
    {
        private readonly IFilialService _baseService;

        public FilialAppService(IFilialService baseService): base(baseService)
        {
            _baseService = baseService;
        }

        public List<FILIAL> GetAllItens(Int32 idMatriz)
        {
            List<FILIAL> lista = _baseService.GetAllItens(idMatriz);
            return lista;
        }

        public List<FILIAL> GetAllItensAdm(Int32 idMatriz)
        {
            List<FILIAL> lista = _baseService.GetAllItensAdm(idMatriz);
            return lista;
        }

        public List<UF> GetAllUF()
        {
            return _baseService.GetAllUF();
        }

        public UF GetUFBySigla(String sigla)
        {
            return _baseService.GetUFBySigla(sigla);
        }

        public FILIAL GetItemById(Int32 id)
        {
            FILIAL item = _baseService.GetItemById(id);
            return item;
        }

        public FILIAL CheckExist(FILIAL filial)
        {
            FILIAL item = _baseService.CheckExist(filial);
            return item;
        }

        public Int32 ValidateCreate(FILIAL item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia
                if (_baseService.CheckExist(item) != null)
                {
                    return 1;
                }

                // Completa objeto
                item.FILI_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "AddFILI",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<FILIAL>(item)
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

        public Int32 ValidateEdit(FILIAL item, FILIAL itemAntes, USUARIO usuario)
        {
            try
            {
                //// Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "EditFILI",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<FILIAL>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<FILIAL>(itemAntes)
                };

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateDelete(FILIAL item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                if (item.USUARIO.Count > 0)
                {
                    return 1;
                }
                //if (item.COLABORADOR.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.CONTA_RECEBER.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.EQUIPAMENTO.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.FORNECEDOR.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.MATERIA_PRIMA.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.MOVIMENTO_ESTOQUE_MATERIA_PRIMA.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.MOVIMENTO_ESTOQUE_PRODUTO.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.PATRIMONIO.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.PEDIDO_COMPRA.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.PEDIDO_VENDA.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.PRECO_PRODUTO.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.PRODUTO.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.SERVICO.Count > 0)
                //{
                //    return 1;
                //}

                // Acerta campos
                item.FILI_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DelFILI",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<FILIAL>(item)
                };

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(FILIAL item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.FILI_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatFILI",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<FILIAL>(item)
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
