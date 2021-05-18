using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class OrcamentoItemViewModel
    {
        [Key]
        public int ORIT_CD_ID { get; set; }
        public Nullable<int> ORCA_CD_ID { get; set; }
        public Nullable<int> REDE_CD_ID { get; set; }
        public Nullable<int> TIPR_CD_ID { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "O VALOR (R$) deve ser um valor numérico positivo")]
        public Nullable<decimal> ORIT_VL_OVERRIDE { get; set; }
        public Nullable<int> ORIT_IN_ATIVO { get; set; }
        public Nullable<int> SUPR_CD_ID { get; set; }

        public virtual ORCAMENTO ORCAMENTO { get; set; }
        public virtual REGIAO_DENTE REGIAO_DENTE { get; set; }
        public virtual TIPO_PROCEDIMENTO TIPO_PROCEDIMENTO { get; set; }
        public virtual SUB_PROCEDIMENTO SUB_PROCEDIMENTO { get; set; }
    }
}