using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface ITelefoneAppService : IAppServiceBase<TELEFONE>
    {
        Int32 ValidateCreate(TELEFONE item, USUARIO usuario);
        Int32 ValidateEdit(TELEFONE item, TELEFONE itemAntes, USUARIO usuario);
        Int32 ValidateEdit(TELEFONE item, TELEFONE itemAntes);
        Int32 ValidateDelete(TELEFONE item, USUARIO usuario);
        Int32 ValidateReativar(TELEFONE item, USUARIO usuario);

        TELEFONE GetItemById(Int32 id);
        List<TELEFONE> GetAllItens(Int32 idAss);
        List<TELEFONE> GetAllItensAdm(Int32 idAss);
        Int32 ExecuteFilter(Int32? tipo, String nome, String telefone, String celular, Int32 idAss, out List<TELEFONE> objeto);
        List<CATEGORIA_TELEFONE> GetAllCategorias(Int32 idAss);
    }
}
