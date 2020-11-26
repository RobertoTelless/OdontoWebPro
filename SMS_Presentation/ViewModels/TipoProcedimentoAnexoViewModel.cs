using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class TipoProcedimentoAnexoViewModel
    {
        public int TPAN_CD_ID { get; set; }
        public Nullable<int> TIPR_CD_ID { get; set; }
        public string TPAN_NM_TITULO { get; set; }
        public Nullable<int> TPAN_IN_TIPO { get; set; }
        public Nullable<System.DateTime> TPAN_DT_ANEXO { get; set; }
        public string TPAN_AQ_ARQUIVO { get; set; }
        public Nullable<int> TPAN_IN_ATIVO { get; set; }

        public virtual TIPO_PROCEDIMENTO TIPO_PROCEDIMENTO { get; set; }

    }
}