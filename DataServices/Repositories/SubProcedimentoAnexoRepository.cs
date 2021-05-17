using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using ModelServices.Interfaces.Repositories;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class SubProcedimentoAnexoRepository : RepositoryBase<SUB_PROCEDIMENTO_ANEXO>, ISubProcedimentoAnexoRepository
    {
        public List<SUB_PROCEDIMENTO_ANEXO> GetAllItens()
        {
            return Db.SUB_PROCEDIMENTO_ANEXO.ToList();
        }

        public SUB_PROCEDIMENTO_ANEXO GetItemById(Int32 id)
        {
            IQueryable<SUB_PROCEDIMENTO_ANEXO> query = Db.SUB_PROCEDIMENTO_ANEXO.Where(p => p.SPAN_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
 