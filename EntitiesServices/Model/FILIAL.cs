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
    
    public partial class FILIAL
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FILIAL()
        {
            this.BANCO = new HashSet<BANCO>();
            this.CONTA_BANCO = new HashSet<CONTA_BANCO>();
            this.FORNECEDOR = new HashSet<FORNECEDOR>();
            this.MOVIMENTO_ESTOQUE_PRODUTO = new HashSet<MOVIMENTO_ESTOQUE_PRODUTO>();
            this.PACIENTE = new HashSet<PACIENTE>();
            this.PERCENTUAL_REMUNERACAO = new HashSet<PERCENTUAL_REMUNERACAO>();
            this.PRODUTO_ESTOQUE_FILIAL = new HashSet<PRODUTO_ESTOQUE_FILIAL>();
            this.TIPO_PROCEDIMENTO = new HashSet<TIPO_PROCEDIMENTO>();
            this.USUARIO = new HashSet<USUARIO>();
            this.PRODUTO_TABELA_PRECO = new HashSet<PRODUTO_TABELA_PRECO>();
            this.PEDIDO_COMPRA = new HashSet<PEDIDO_COMPRA>();
            this.PRODUTO = new HashSet<PRODUTO>();
        }
    
        public int FILI_CD_ID { get; set; }
        public Nullable<int> ASSI_CD_ID { get; set; }
        public int MATR_CD_ID { get; set; }
        public int TIPE_CD_ID { get; set; }
        public Nullable<int> CRTR_CD_ID { get; set; }
        public string FILI_NM_NOME { get; set; }
        public string FILI_NM_RAZAO { get; set; }
        public string FILI_NR_CNPJ { get; set; }
        public string FILI_NM_EMAIL { get; set; }
        public string FILI_NM_CONTATOS { get; set; }
        public string FILI_NM_TELEFONES { get; set; }
        public string FILI_NM_ENDERECO { get; set; }
        public string FILI_NM_BAIRRO { get; set; }
        public string FILI_NM_CIDADE { get; set; }
        public string FILI_NR_CEP { get; set; }
        public Nullable<int> UF_CD_ID { get; set; }
        public System.DateTime FILI_DT_CADASTRO { get; set; }
        public int FILI_IN_ATIVO { get; set; }
        public string FILI_NR_INSCRICAO_ESTADUAL { get; set; }
        public string FILI_NR_INSCRICAO_MUNICIPAL { get; set; }
        public Nullable<int> FILI_IN_IE_ISENTO { get; set; }
        public string FILI_NR_CNAE { get; set; }
        public string FILI_NM_WEBSITE { get; set; }
        public string FILI_NR_CELULAR { get; set; }
        public string FILI_AQ_LOGOTIPO { get; set; }
        public string FILI_NR_CPF { get; set; }
        public string FILI_NR_RG { get; set; }
    
        public virtual ASSINANTE ASSINANTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BANCO> BANCO { get; set; }
        public virtual CODIGO_REGIME_TRIBUTARIO CODIGO_REGIME_TRIBUTARIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONTA_BANCO> CONTA_BANCO { get; set; }
        public virtual MATRIZ MATRIZ { get; set; }
        public virtual TIPO_PESSOA TIPO_PESSOA { get; set; }
        public virtual UF UF { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FORNECEDOR> FORNECEDOR { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MOVIMENTO_ESTOQUE_PRODUTO> MOVIMENTO_ESTOQUE_PRODUTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE> PACIENTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PERCENTUAL_REMUNERACAO> PERCENTUAL_REMUNERACAO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRODUTO_ESTOQUE_FILIAL> PRODUTO_ESTOQUE_FILIAL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TIPO_PROCEDIMENTO> TIPO_PROCEDIMENTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USUARIO> USUARIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRODUTO_TABELA_PRECO> PRODUTO_TABELA_PRECO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PEDIDO_COMPRA> PEDIDO_COMPRA { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRODUTO> PRODUTO { get; set; }
    }
}
