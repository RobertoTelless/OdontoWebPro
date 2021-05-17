using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class SubProcedimentoViewModel
    {
        [Key]
        public int SUPR_CD_ID { get; set; }
        public Nullable<int> TIPR_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 50 caracteres.")]
        public string SUPR_NM_NOME { get; set; }
        [Required(ErrorMessage = "Campo SIGLA obrigatorio")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "A SIGLA deve ter no minimo 1 caractere e no máximo 10 caracteres.")]
        public string SUPR_SG_SIGLA { get; set; }
        [StringLength(5000, ErrorMessage = "A DESCRIÇÃO deve ter no máximo 5000 caracteres.")]
        public string SUPR_DS_DESCRICAO { get; set; }
        [Required(ErrorMessage = "Campo VALOR obrigatorio")]
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "O VALOR deve ser um valor numérico positivo")]
        public Nullable<decimal> SUPR_VL_VALOR { get; set; }
        [Required(ErrorMessage = "Campo DURAÇÃO (HORAS) obrigatorio")]
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "A DURAÇÃO (HORAS) deve ser um valor numérico positivo")]
        public Nullable<int> SUPR_VL_PRAZO { get; set; }
        public Nullable<int> SUPR_IN_ATIVO { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SUB_PROCEDIMENTO_ANEXO> SUB_PROCEDIMENTO_ANEXO { get; set; }
        public virtual TIPO_PROCEDIMENTO TIPO_PROCEDIMENTO { get; set; }
    }
}