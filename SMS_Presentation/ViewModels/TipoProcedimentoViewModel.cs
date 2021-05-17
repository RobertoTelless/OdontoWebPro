using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class TipoProcedimentoViewModel
    {
        [Key]
        public int TIPR_CD_ID { get; set; }
        public Nullable<int> ASSI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo UNIDADE obrigatorio")]
        public Nullable<int> FILI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 50 caracteres.")]
        public string TIPR_NM_NOME { get; set; }
        [StringLength(10, ErrorMessage = "A SIGLA deve ter máximo 10 caracteres.")]
        public string TIPR_SG_SIGLA { get; set; }
        [StringLength(5000, ErrorMessage = "A DESCRIÇÃO deve ter no máximo 5000 caracteres.")]
        public string TIPR_DS_DESCRICAO { get; set; }
        public int TIPR_IN_ATIVO { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "O VALOR deve ser um valor numérico positivo")]
        public Nullable<decimal> TIPR_VL_VALOR { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "A DURAÇÂO (HORAS) deve ser um valor numérico positivo")]
        public Nullable<int> TIPR_VL_PRAZO { get; set; }

        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual FILIAL FILIAL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO_ITEM> ORCAMENTO_ITEM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TIPO_PROCEDIMENTO_ANEXO> TIPO_PROCEDIMENTO_ANEXO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SUB_PROCEDIMENTO> SUB_PROCEDIMENTO { get; set; }

    }
}