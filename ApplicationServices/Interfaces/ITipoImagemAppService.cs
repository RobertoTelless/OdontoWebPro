using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface ITipoImagemAppService : IAppServiceBase<TIPO_IMAGEM>
    {
        Int32 ValidateCreate(TIPO_IMAGEM item, USUARIO usuario);
        Int32 ValidateEdit(TIPO_IMAGEM item, TIPO_IMAGEM itemAntes, USUARIO usuario);
        Int32 ValidateEdit(TIPO_IMAGEM item, TIPO_IMAGEM itemAntes);
        Int32 ValidateDelete(TIPO_IMAGEM item, USUARIO usuario);
        Int32 ValidateReativar(TIPO_IMAGEM item, USUARIO usuario);

        List<TIPO_IMAGEM> GetAllItens(Int32 idAss);
        List<TIPO_IMAGEM> GetAllItensAdm(Int32 idAss);
        TIPO_IMAGEM GetItemById(Int32 id);
    }
}
