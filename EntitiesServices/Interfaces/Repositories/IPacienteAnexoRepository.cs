using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPacienteAnexoRepository : IRepositoryBase<PACIENTE_ANEXO>
    {
        List<PACIENTE_ANEXO> GetAllItens(Int32? idAss);
        PACIENTE_ANEXO GetItemById(Int32 id);
    }
}
