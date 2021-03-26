using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;
using EntitiesServices.Attributes;

namespace OdontoWeb.ViewModels
{
    public class ProdutoViewModel
    {
        [Key]
        public int PROD_CD_ID { get; set; }
        public Nullable<int> ASSI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo CATEGORIA obrigatorio")]
        public int CAPR_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo UNIDADE obrigatorio")]
        public Nullable<int> UNID_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo CÓDIGO obrigatorio")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "O CÓDIGO deve conter no minimo 1 e no máximo 15 caracteres.")]
        public string PROD_CD_CODIGO { get; set; }
        public string PROD_NR_BARCODE { get; set; }
        public string PROD_AQ_FOTO { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "O NOME deve conter no minimo 1 e no máximo 100 caracteres.")]
        public string PROD_NM_NOME { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "A MARCA deve conter no máximo 50 caracteres.")]
        public string PROD_NM_MARCA { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O MODELO deve conter no máximo 50 caracteres.")]
        public string PROD_NM_MODELO { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O FABRICANTE deve conter no máximo 50 caracteres.")]
        public string PROD_NM_FABRICANTE { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "A REFERENCIA deve conter no máximo 50 caracteres.")]
        public string PROD_NM_REFERENCIA { get; set; }
        [Required(ErrorMessage = "Campo DESCRIÇÃO obrigatorio")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "A DESCRIÇÃO deve conter no minimo 1 e no máximo 500 caracteres.")]
        public string PROD_DS_DESCRICAO { get; set; }
        [Required(ErrorMessage = "Campo QUANTIDADE MÍNIMA obrigatorio")]
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_QN_QUANTIDADE_MINIMA { get; set; }
        [Required(ErrorMessage = "Campo QUANTIDADE MÁXIMA obrigatorio")]
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_QN_QUANTIDADE_MAXIMA { get; set; }
        public Nullable<int> PROD_IN_AVISA_MINIMO { get; set; }
        public Nullable<int> PROD_IN_ATIVO { get; set; }
        public Nullable<System.DateTime> PROD_DT_CADASTRO { get; set; }
        [Required(ErrorMessage = "Campo SUBCATEGORIA obrigatorio")]
        public Nullable<int> SUPR_CD_ID { get; set; }
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "AS INFORMAÇÕES deve conter no máximo 1000 caracteres.")]
        public string PROD_DS_INFORMACOES { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_QN_RESERVA_ESTOQUE { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "A LOCALIZAÇÃO deve conter no máximo 50 caracteres.")]
        public string PROD_NM_LOCALIZACAO_ESTOQUE { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<decimal> PROD_QN_PESO_BRUTO { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<decimal> PROD_QN_PESO_LIQUIDO { get; set; }
        public Nullable<int> PROD_IN_TIPO_EMBALAGEM { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_NR_LARGURA { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_NR_COMPRIMENTO { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_NR_ALTURA { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_NR_DIAMETRO { get; set; }
        public string PROD_TX_OBSERVACOES { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "A REFERENCIA deve conter no máximo 50 caracteres.")]
        public string PROD_NM_REFERENCIA_FABRICANTE { get; set; }
        public string PROD_QR_QRCODE { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_QN_CONTAGEM { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> PROD_QN_NOVA_CONTAGEM { get; set; }

        public bool AvisaMinima
        {
            get
            {
                if (PROD_IN_AVISA_MINIMO == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PROD_IN_AVISA_MINIMO = (value == true) ? 1 : 0;
            }
        }

        [RegularExpression(@"^[0-9]+([,][0-9]+)?$", ErrorMessage = "Deve ser um valor numérico positivo")]
        public Nullable<int> QuantidadeMaxima
        {
            get
            {
                return PROD_QN_QUANTIDADE_MAXIMA;
            }
            set
            {
                PROD_QN_QUANTIDADE_MAXIMA = value;
            }
        }

        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual CATEGORIA_PRODUTO CATEGORIA_PRODUTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MOVIMENTO_ESTOQUE_PRODUTO> MOVIMENTO_ESTOQUE_PRODUTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRODUTO_ANEXO> PRODUTO_ANEXO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRODUTO_ESTOQUE_FILIAL> PRODUTO_ESTOQUE_FILIAL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRODUTO_FORNECEDOR> PRODUTO_FORNECEDOR { get; set; }
        public virtual UNIDADE UNIDADE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRODUTO_TABELA_PRECO> PRODUTO_TABELA_PRECO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ITEM_PEDIDO_COMPRA> ITEM_PEDIDO_COMPRA { get; set; }
        public virtual SUBCATEGORIA_PRODUTO SUBCATEGORIA_PRODUTO { get; set; }

    }
}