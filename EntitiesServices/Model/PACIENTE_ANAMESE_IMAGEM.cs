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
    
    public partial class PACIENTE_ANAMESE_IMAGEM
    {
        public int PCAI_CD_ID { get; set; }
        public int PACI_CD_ID { get; set; }
        public string PCAI_NM_NOME { get; set; }
        public string PCAI_DS_DESCRICAO { get; set; }
        public System.DateTime PCAI_DT_DATA { get; set; }
        public int TPIM_CD_ID { get; set; }
        public string PCAI_AQ_ARQUIVO { get; set; }
        public int PCAI_IN_ATIVO { get; set; }
        public int PCAI_IN_ASSINATURA { get; set; }
    
        public virtual PACIENTE PACIENTE { get; set; }
        public virtual TIPO_IMAGEM TIPO_IMAGEM { get; set; }
    }
}
