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
    
    public partial class SUB_PROCEDIMENTO_ANEXO
    {
        public int SPAN_CD_ID { get; set; }
        public int SUPR_CD_ID { get; set; }
        public string SPAN_NM_TITULO { get; set; }
        public Nullable<int> SPAN_IN_TIPO { get; set; }
        public Nullable<System.DateTime> SPAN_DT_ANEXO { get; set; }
        public string SPAN_AQ_ARQUIVO { get; set; }
        public Nullable<int> SPAN_IN_ATIVO { get; set; }
    
        public virtual SUB_PROCEDIMENTO SUB_PROCEDIMENTO { get; set; }
    }
}
