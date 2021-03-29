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
    public class ProdutoEstoqueFilialAppService : AppServiceBase<PRODUTO_ESTOQUE_FILIAL>, IProdutoEstoqueFilialAppService
    {
        private readonly IProdutoEstoqueFilialService _baseService;

        public ProdutoEstoqueFilialAppService(IProdutoEstoqueFilialService baseService): base(baseService)
        {
            _baseService = baseService;
        }

        public PRODUTO_ESTOQUE_FILIAL CheckExist(PRODUTO_ESTOQUE_FILIAL conta, Int32 idAss)
        {
            PRODUTO_ESTOQUE_FILIAL item = _baseService.CheckExist(conta, idAss);
            return item;
        }

        public PRODUTO_ESTOQUE_FILIAL GetItemById(Int32 id)
        {
            PRODUTO_ESTOQUE_FILIAL item = _baseService.GetItemById(id);
            return item;
        }

        public PRODUTO_ESTOQUE_FILIAL GetItemById(PRODUTO item)
        {
            PRODUTO_ESTOQUE_FILIAL obj = _baseService.GetItemById(item);
            return obj;
        }

        public PRODUTO_ESTOQUE_FILIAL GetByProdFilial(Int32 prod, Int32 fili)
        {
            PRODUTO_ESTOQUE_FILIAL item = _baseService.GetByProdFilial(prod, fili);
            return item;
        }

        public Int32 ValidateCreate(PRODUTO_ESTOQUE_FILIAL item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia
                if (_baseService.CheckExist(item, usuario.ASSI_CD_ID) != null)
                {
                    return 1;
                }

                // Completa objeto
                item.PREF_IN_ATIVO = 1;

                //Persiste
                Int32 volta = _baseService.Create(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(PRODUTO_ESTOQUE_FILIAL item, PRODUTO_ESTOQUE_FILIAL itemAntes, USUARIO usuario)
        {
            try
            {
                if (item.FILIAL != null)
                {
                    item.FILIAL = null;
                }

                if (item.PRODUTO != null)
                {
                    item.PRODUTO = null;
                }

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
