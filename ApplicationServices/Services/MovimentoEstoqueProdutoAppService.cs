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
    public class MovimentoEstoqueProdutoAppService : AppServiceBase<MOVIMENTO_ESTOQUE_PRODUTO>, IMovimentoEstoqueProdutoAppService
    {
        private readonly IMovimentoEstoqueProdutoService _baseService;

        public MovimentoEstoqueProdutoAppService(IMovimentoEstoqueProdutoService baseService) : base(baseService)
        {
            _baseService = baseService;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItens(Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public MOVIMENTO_ESTOQUE_PRODUTO GetItemById(Int32 id)
        {
            MOVIMENTO_ESTOQUE_PRODUTO item = _baseService.GetItemById(id);
            return item;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensEntrada(Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseService.GetAllItensEntrada(idAss);
            return lista;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensUserDataMes(Int32 idUsu, DateTime data, Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseService.GetAllItensUserDataMes(idUsu, data, idAss);
            return lista;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensUserDataDia(Int32 idUsu, DateTime data, Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseService.GetAllItensUserDataDia(idUsu, data, idAss);
            return lista;
        }

        public List<MOVIMENTO_ESTOQUE_PRODUTO> GetAllItensSaida(Int32? idAss)
        {
            List<MOVIMENTO_ESTOQUE_PRODUTO> lista = _baseService.GetAllItensSaida(idAss);
            return lista;
        }

        public Int32 ExecuteFilter(Int32? catId, String nome, String barcode, Int32? filiId, DateTime? dtMov, Int32? idAss, out List<MOVIMENTO_ESTOQUE_PRODUTO> objeto)
        {
            try
            {
                objeto = new List<MOVIMENTO_ESTOQUE_PRODUTO>();
                Int32 volta = 0;

                // Processa filtro
                objeto = _baseService.ExecuteFilter(catId, nome, barcode, filiId, dtMov, idAss);
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

        public Int32 ValidateCreate(MOVIMENTO_ESTOQUE_PRODUTO mov, USUARIO usuario)
        {
            try
            {
                mov.MOEP_IN_ATIVO = 1;

                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "AddMOEP",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<MOVIMENTO_ESTOQUE_PRODUTO>(mov)
                };

                Int32 volta = _baseService.Create(mov);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateCreateLeve(MOVIMENTO_ESTOQUE_PRODUTO mov, USUARIO usuario)
        {
            try
            {
                mov.MOEP_IN_ATIVO = 1;

                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    LOG_NM_OPERACAO = "AddMOEP",
                    LOG_IN_ATIVO = 1,
                };

                Int32 volta = _baseService.Create(mov);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
