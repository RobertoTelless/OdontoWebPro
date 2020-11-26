using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class DenteRegiaoViewModel
    {
        [Key]
        public int REDE_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 50.")]
        public string REDE_NM_NOME { get; set; }
        public Nullable<int> REDE_IN_ATIVO { get; set; }
        [StringLength(5000, ErrorMessage = "A DESCRIÇÂO deve ter no máximo 5000 caracteres.")]
        public string REDE_DS_DESCRICAO { get; set; }
        [StringLength(250, ErrorMessage = "O ARQUIVO deve ter no máximo 250 caracteres.")]
        public string REDE_AQ_FOTO { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO_ITEM> ORCAMENTO_ITEM { get; set; }
    }
}