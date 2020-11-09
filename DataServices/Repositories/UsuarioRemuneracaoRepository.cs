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
    public class UsuarioRemuneracaoRepository : RepositoryBase<USUARIO_REMUNERACAO>, IUsuarioRemuneracaoRepository
    {
        public USUARIO_REMUNERACAO GetItemById(Int32 id)
        {
            IQueryable<USUARIO_REMUNERACAO> query = Db.USUARIO_REMUNERACAO;
            query = query.Where(p => p.USRE_CD_ID == id);
            return query.FirstOrDefault();
        }

        public USUARIO_REMUNERACAO GetItemByUser(Int32 id, DateTime data)
        {
            IQueryable<USUARIO_REMUNERACAO> query = Db.USUARIO_REMUNERACAO;
            query = query.Where(p => p.USUA_CD_ID == id);
            query = query.Where(p => p.USRE_DT_REFERENCIA == data);
            return query.FirstOrDefault();
        }
    }
}
 