using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class CargoViewModel
    {
        [Key]
        public int CARG_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo CARGO obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O CARGO deve ter no minimo 1 caractere e no máximo 50.")]
        public string CARG_NM_NOME { get; set; }
        public int CARG_IN_ATIVO { get; set; }
        public Nullable<int> CARG_IN_COMISSAO { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USUARIO> USUARIO { get; set; }
        public virtual ASSINANTE ASSINANTE { get; set; }
    }
}