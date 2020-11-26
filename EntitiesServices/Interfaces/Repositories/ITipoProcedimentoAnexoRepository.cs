using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ITipoProcedimentoAnexoRepository : IRepositoryBase<TIPO_PROCEDIMENTO_ANEXO>
    {
        List<TIPO_PROCEDIMENTO_ANEXO> GetAllItens();
        TIPO_PROCEDIMENTO_ANEXO GetItemById(Int32 id);
    }
}
