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
    public class DenteRegiaoAppService : AppServiceBase<REGIAO_DENTE>, IDenteRegiaoAppService
    {
        private readonly IDenteRegiaoService _baseService;

        public DenteRegiaoAppService(IDenteRegiaoService baseService): base(baseService)
        {
            _baseService = baseService;
        }

        public List<REGIAO_DENTE> GetAllItens(Int32 idAss)
        {
            List<REGIAO_DENTE> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public List<REGIAO_DENTE> GetAllItensAdm(Int32 idAss)
        {
            List<REGIAO_DENTE> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public REGIAO_DENTE GetItemById(Int32 id)
        {
            REGIAO_DENTE item = _baseService.GetItemById(id);
            return item;
        }

        public Int32 ValidateCreate(REGIAO_DENTE item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia

                // Completa objeto
                item.REDE_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddREDE",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<REGIAO_DENTE>(item)
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

        public Int32 ValidateEdit(REGIAO_DENTE item, REGIAO_DENTE itemAntes, USUARIO usuario)
        {
            try
            {
                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "EditREDE",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<REGIAO_DENTE>(item),
                    LOG_TX_REGISTRO_ANTES = Serialization.SerializeJSON<REGIAO_DENTE>(itemAntes)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(REGIAO_DENTE item, REGIAO_DENTE itemAntes)
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

        public Int32 ValidateDelete(REGIAO_DENTE item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                if (item.ORCAMENTO_ITEM.Count > 0)
                {
                    return 1;
                }

                // Acerta campos
                item.REDE_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DeleREDE",
                    LOG_TX_REGISTRO = "Dente/Região: " + item.REDE_NM_NOME
                };

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(REGIAO_DENTE item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.REDE_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatREDE",
                    LOG_TX_REGISTRO = "Região/Dente: " + item.REDE_NM_NOME
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
