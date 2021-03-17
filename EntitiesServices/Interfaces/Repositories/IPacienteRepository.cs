using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPacienteRepository : IRepositoryBase<PACIENTE>
    {
        PACIENTE CheckExist(PACIENTE item, Int32? idAss);
        PACIENTE GetByNome(String nome, Int32? idAss);
        PACIENTE GetItemById(Int32 id);
        List<PACIENTE> GetAllItens(Int32? idAss);
        List<PACIENTE> GetAllItensAdm(Int32? idAss);
        List<PACIENTE> ExecuteFilter(Int32? catId, Int32? filialId, String nome, String cpf, String telefone, String celular, String cidade, DateTime dataNasc, String email, Int32? idAss);
    }
}
