using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class OrcamentoAcompanhamentoViewModel
    {
        [Key]
        public int ORAC_CD_ID { get; set; }
        public int ORCA_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo USUÁRIO obrigatorio")]
        public int USUA_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo DATA obrigatorio")]
        [DataType(DataType.Date, ErrorMessage = "DATA DO ORÇAMENTO Deve ser uma data válida")]
        public System.DateTime ORAC_DT_DATA { get; set; }
        [Required(ErrorMessage = "Campo TEXTO obrigatorio")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "O TEXTO deve ter no minimo 1 caractere e no máximo 500 caracteres.")]
        public string ORAC_TX_ACOMPANHAMENTO { get; set; }
        public int ORAC_IN_ATIVO { get; set; }

        public virtual ORCAMENTO ORCAMENTO { get; set; }
        public virtual USUARIO USUARIO { get; set; }
    }
}