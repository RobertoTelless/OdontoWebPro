using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Attributes;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class FornecedorContatoViewModel
    {
        [Key]
        public int FOCO_CD_ID { get; set; }
        public int FORN_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 50 caracteres.")]
        public string FOCO_NM_NOME { get; set; }
        [StringLength(50, ErrorMessage = "O CARGO deve ter máximo 50 caracteres.")]
        public string FOCO_NM_CARGO { get; set; }
        [StringLength(100, ErrorMessage = "O E-MAIL deve ter no máximo 100 caracteres.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Deve ser um e-mail válido")]
        public string FOCO_NM_EMAIL { get; set; }
        [StringLength(50, ErrorMessage = "O TELEFONE deve ter máximo 50 caracteres.")]
        public string FOCO_NR_TELEFONES { get; set; }
        public int FOCO_IN_ATIVO { get; set; }

        public virtual FORNECEDOR FORNECEDOR { get; set; }
    }
}