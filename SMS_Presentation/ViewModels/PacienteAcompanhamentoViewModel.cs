using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class PacienteAcompanhamentoViewModel
    {
        [Key]
        public int PAAC_CD_ID { get; set; }
        public Nullable<int> PACI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo USUÁRIO obrigatorio")]
        public Nullable<int> USUA_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo DATA obrigatorio")]
        public Nullable<System.DateTime> PAAC_DT_DATA { get; set; }
        [Required(ErrorMessage = "Campo TEXTO obrigatorio")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "O TEXTO deve ter no minimo 1 caractere e no máximo 500 caracteres.")]
        public string PAAC_TX_ACOMPANHENTO { get; set; }
        public Nullable<int> PAAC_IN_ATIVO { get; set; }

        public virtual PACIENTE PACIENTE { get; set; }
        public virtual USUARIO USUARIO { get; set; }
    }
}