using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Attributes;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class OrcamentoViewModel
    {
        [Key]
        public int ORCA_CD_ID { get; set; }
        public Nullable<int> PACI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo DATA obrigatorio")]
        [DataType(DataType.Date, ErrorMessage = "DATA DO ORÇAMENTO Deve ser uma data válida")]
        public Nullable<System.DateTime> ORCA_DT_DATA { get; set; }
        public Nullable<int> ORCA_IN_STATUS { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "O VALOR (R$) deve ser um valor numérico positivo")]
        public Nullable<decimal> ORCA_VL_VALOR { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "O DESCONTO (R$) deve ser um valor numérico positivo")]
        public Nullable<decimal> ORCA_VL_DESCONTO { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "O VALOR FINAL (R$) deve ser um valor numérico positivo")]
        public Nullable<decimal> ORCA_VL_FINAL { get; set; }
        public Nullable<int> ORCA_IN_ATIVO { get; set; }
        public string ORCA_DS_OBSERVACOES { get; set; }
        [DataType(DataType.Date, ErrorMessage = "DATA DO PAGAMENTO Deve ser uma data válida")]
        public Nullable<System.DateTime> ORCA_DT_PAGAMENTO { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "O VALOR PAGO (R$) deve ser um valor numérico positivo")]
        public Nullable<decimal> ORCA_VL_VALOR_PAGO { get; set; }
        public Nullable<int> ASSI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(250, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 250 caracteres.")]
        public string ORCA_NM_NOME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO_ANEXO> ORCAMENTO_ANEXO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO_ITEM> ORCAMENTO_ITEM { get; set; }
        public virtual PACIENTE PACIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO_ACOMPANHAMENTO> ORCAMENTO_ACOMPANHAMENTO { get; set; }
        public virtual ASSINANTE ASSINANTE { get; set; }
    }
}