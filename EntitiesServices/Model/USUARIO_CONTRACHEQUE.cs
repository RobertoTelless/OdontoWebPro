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
    
    public partial class USUARIO_CONTRACHEQUE
    {
        public int USCC_CD_ID { get; set; }
        public int USUA_CD_ID { get; set; }
        public Nullable<System.DateTime> USCC_DT_CADASTRO { get; set; }
        public string USCC_AQ_ARQUIVO { get; set; }
        public Nullable<int> USCC_IN_VISUALIZACOES { get; set; }
        public Nullable<int> USCC_IN_DOWNLOADS { get; set; }
        public Nullable<int> USCC_IN_ATIVO { get; set; }
    
        public virtual USUARIO USUARIO { get; set; }
    }
}
