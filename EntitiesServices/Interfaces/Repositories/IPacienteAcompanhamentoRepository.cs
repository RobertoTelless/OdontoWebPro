using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPacienteAcompanhamentoRepository : IRepositoryBase<PACIENTE_ACOMPANHAMENTO>
    {
        List<PACIENTE_ACOMPANHAMENTO> GetAllItens(Int32? idAss);
        PACIENTE_ACOMPANHAMENTO GetItemById(Int32 id);
    }
}
