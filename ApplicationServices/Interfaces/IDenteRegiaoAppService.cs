using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IDenteRegiaoAppService : IAppServiceBase<REGIAO_DENTE>
    {
        Int32 ValidateCreate(REGIAO_DENTE item, USUARIO usuario);
        Int32 ValidateEdit(REGIAO_DENTE item, REGIAO_DENTE itemAntes, USUARIO usuario);
        Int32 ValidateEdit(REGIAO_DENTE item, REGIAO_DENTE itemAntes);
        Int32 ValidateDelete(REGIAO_DENTE item, USUARIO usuario);
        Int32 ValidateReativar(REGIAO_DENTE item, USUARIO usuario);

        List<REGIAO_DENTE> GetAllItens(Int32 idAss);
        List<REGIAO_DENTE> GetAllItensAdm(Int32 idAss);
        REGIAO_DENTE GetItemById(Int32 id);
    }
}
