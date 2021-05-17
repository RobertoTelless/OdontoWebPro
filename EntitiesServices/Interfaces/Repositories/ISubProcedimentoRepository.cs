using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ISubProcedimentoRepository : IRepositoryBase<SUB_PROCEDIMENTO>
    {
        SUB_PROCEDIMENTO CheckExist(SUB_PROCEDIMENTO item, Int32? idAss);
        List<SUB_PROCEDIMENTO> GetAllItens(Int32 idAss);
        List<SUB_PROCEDIMENTO> GetAllItensAdm(Int32 idAss);
        SUB_PROCEDIMENTO GetItemById(Int32 id);
        List<SUB_PROCEDIMENTO> ExecuteFilter(String nome, String descricao, Int32 idAss);
    }
}
