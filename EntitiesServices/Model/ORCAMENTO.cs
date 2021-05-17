//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiesServices.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class ORCAMENTO
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ORCAMENTO()
        {
            this.ORCAMENTO_ANEXO = new HashSet<ORCAMENTO_ANEXO>();
            this.ORCAMENTO_ITEM = new HashSet<ORCAMENTO_ITEM>();
            this.ORCAMENTO_ACOMPANHAMENTO = new HashSet<ORCAMENTO_ACOMPANHAMENTO>();
        }
    
        public int ORCA_CD_ID { get; set; }
        public Nullable<int> PACI_CD_ID { get; set; }
        public Nullable<System.DateTime> ORCA_DT_DATA { get; set; }
        public Nullable<int> ORCA_IN_STATUS { get; set; }
        public Nullable<decimal> ORCA_VL_VALOR { get; set; }
        public Nullable<decimal> ORCA_VL_DESCONTO { get; set; }
        public Nullable<decimal> ORCA_VL_FINAL { get; set; }
        public Nullable<int> ORCA_IN_ATIVO { get; set; }
        public string ORCA_DS_OBSERVACOES { get; set; }
        public Nullable<System.DateTime> ORCA_DT_PAGAMENTO { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO_ANEXO> ORCAMENTO_ANEXO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO_ITEM> ORCAMENTO_ITEM { get; set; }
        public virtual PACIENTE PACIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO_ACOMPANHAMENTO> ORCAMENTO_ACOMPANHAMENTO { get; set; }
    }
}
