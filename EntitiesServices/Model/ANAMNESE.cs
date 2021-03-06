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
    
    public partial class ANAMNESE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ANAMNESE()
        {
            this.ANAMNESE_IMAGEM = new HashSet<ANAMNESE_IMAGEM>();
            this.ANAMNESE_NOVA_PERGUNTA = new HashSet<ANAMNESE_NOVA_PERGUNTA>();
        }
    
        public int ANAM_CD_ID { get; set; }
        public Nullable<int> PACI_CD_ID { get; set; }
        public Nullable<int> ANAM_IN_ALGUMA_DOENCA { get; set; }
        public string ANAM_DS__ALGUMA_DOENCA { get; set; }
        public Nullable<int> ANAM_IN_ALERGICO_MEDICAMENTO { get; set; }
        public string ANAM_DS_ALERGICO_MEDICAMENTO { get; set; }
        public Nullable<int> ANAM_IN_USO_MEDICAMENTO { get; set; }
        public string ANAM_DS_USO_MEDICAMENTO { get; set; }
        public Nullable<int> ANAM_IN_HOSPITALIZADO { get; set; }
        public string ANAM_DS_HOSPITALIZADO { get; set; }
        public Nullable<int> ANAM_IN_FUMA_BEBE { get; set; }
        public string ANAM_DS_FUMA_BEBE { get; set; }
        public Nullable<System.DateTime> ANAM_DT_DATA_CRIACAO { get; set; }
        public Nullable<int> ANAM_IN_ATIVO { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ANAMNESE_IMAGEM> ANAMNESE_IMAGEM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ANAMNESE_NOVA_PERGUNTA> ANAMNESE_NOVA_PERGUNTA { get; set; }
        public virtual PACIENTE PACIENTE { get; set; }
    }
}
