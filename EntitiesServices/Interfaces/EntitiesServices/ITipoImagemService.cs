using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface ITipoImagemService : IServiceBase<TIPO_IMAGEM>
    {
        Int32 Create(TIPO_IMAGEM item, LOG log);
        Int32 Create(TIPO_IMAGEM item);
        Int32 Edit(TIPO_IMAGEM item, LOG log);
        Int32 Edit(TIPO_IMAGEM item);
        Int32 Delete(TIPO_IMAGEM item, LOG log);

        TIPO_IMAGEM GetItemById(Int32 id);
        List<TIPO_IMAGEM> GetAllItens(Int32 idAss);
        List<TIPO_IMAGEM> GetAllItensAdm(Int32 idAss);
    }
}
