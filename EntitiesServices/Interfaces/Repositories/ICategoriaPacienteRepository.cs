using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ICategoriaPacienteRepository : IRepositoryBase<CATEGORIA_PACIENTE>
    {
        List<CATEGORIA_PACIENTE> GetAllItens(Int32 idAss);
        CATEGORIA_PACIENTE GetItemById(Int32 id);
        List<CATEGORIA_PACIENTE> GetAllItensAdm(Int32 idAss);
    }
}
