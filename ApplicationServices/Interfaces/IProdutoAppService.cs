using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IProdutoAppService : IAppServiceBase<PRODUTO>
    {
        Int32 ValidateCreate(PRODUTO perfil, USUARIO usuario);
        Int32 ValidateCreateLeve(PRODUTO item, USUARIO usuario);
        Int32 ValidateEdit(PRODUTO perfil, PRODUTO perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(PRODUTO item, PRODUTO itemAntes);
        Int32 ValidateDelete(PRODUTO perfil, USUARIO usuario);
        Int32 ValidateReativar(PRODUTO perfil, USUARIO usuario);
        //Int32 ValidateAcertaEstoque(PRODUTO perfil, PRODUTO perfilAntes, USUARIO usuario);

        List<PRODUTO> GetAllItens(Int32 idAss);
        List<PRODUTO> GetAllItensAdm(Int32 idAss);
        PRODUTO GetItemById(Int32 id);
        PRODUTO GetByNome(String nome, Int32 idAss);
        PRODUTO CheckExist(PRODUTO conta, Int32 idAss);
        PRODUTO CheckExist(String barcode, String codigo, Int32 idAss);
        List<CATEGORIA_PRODUTO> GetAllTipos(Int32 idAss);
        List<SUBCATEGORIA_PRODUTO> GetAllSubs(Int32 idAss);
        List<UNIDADE> GetAllUnidades(Int32 idAss);
        PRODUTO_ANEXO GetAnexoById(Int32 id);
        Int32 ExecuteFilter(Int32? catId, Int32? subId, String barcode, String nome, String marca, String codigo, String modelo, String fabricante, Int32? idAss, out List<PRODUTO> objeto);
        PRODUTO_FORNECEDOR GetFornecedorById(Int32 id);
        Int32 ValidateEditFornecedor(PRODUTO_FORNECEDOR item);
        Int32 ValidateCreateFornecedor(PRODUTO_FORNECEDOR item);
        List<PRODUTO> GetPontoPedido();
        List<PRODUTO> GetEstoqueZerado();
        List<PRODUTO_ORIGEM> GetAllOrigens();
        PRODUTO_TABELA_PRECO CheckExist(PRODUTO_TABELA_PRECO item, Int32 idAss);

        Int32 IncluirTabelaPreco(PRODUTO item, USUARIO usuario);
        Int32 ValidateEditTabelaPreco(PRODUTO_TABELA_PRECO item);

        //List<PRODUTO> CalcularPontoPedido();
        //List<PRODUTO> CalcularEstoqueZerado();
        //List<PRODUTO> CalcularEstoqueNegativo();
        //List<PRODUTO> CalcularMaisVendidos();
        List<PRODUTO_ESTOQUE_FILIAL> RecuperarQuantidadesFiliais(Int32? idFilial);
        Int32 ExecuteFilterEstoque(Int32? filial, String nome, String marca, String codigo, Int32 idAss, out List<PRODUTO_ESTOQUE_FILIAL> objeto);

    }
}
