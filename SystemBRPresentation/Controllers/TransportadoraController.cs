using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SystemBRPresentation.App_Start;
using EntitiesServices.Work_Classes;
using AutoMapper;
using SystemBRPresentation.ViewModels;
using System.IO;
using ExternalServices;
using Canducci.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SystemBRPresentation.Controllers
{
    public class TransportadoraController : Controller
    {
        private readonly IMatrizAppService matrizApp;
        private readonly ITransportadoraAppService tranApp;
        private String msg;
        private Exception exception;
        private String extensao;
        TRANSPORTADORA objetoTran = new TRANSPORTADORA();
        TRANSPORTADORA objetoTranAntes = new TRANSPORTADORA();
        List<TRANSPORTADORA> listaMasterTran = new List<TRANSPORTADORA>();

        public TransportadoraController(IMatrizAppService matrizApps, ITransportadoraAppService tranApps)
        {
            matrizApp = matrizApps;
            tranApp = tranApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            TRANSPORTADORA item = new TRANSPORTADORA();
            TransportadoraViewModel vm = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(item);
            return View(vm);
        }

        public ActionResult Voltar()
        {
           if (SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA == "ADM")
            {
                return RedirectToAction("CarregarBase", "BaseAdmin");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

               [HttpGet]
        public ActionResult MontarTelaTransportadora()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if (SessionMocks.UserCredentials != null)
            {
                usuario = SessionMocks.UserCredentials;

            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }

            // Carrega listas
            if (SessionMocks.listaTransportadora == null)
            {
                listaMasterTran = tranApp.GetAllItens();
                SessionMocks.listaTransportadora = listaMasterTran;
            }
            ViewBag.Listas = SessionMocks.listaTransportadora;
            ViewBag.Title = "Transportadorasxxx";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Filiais = new SelectList(tranApp.GetAllFilial(), "FILI_CD_ID", "FILI_NM_NOME");

            // Indicadores
            ViewBag.Transportadoras = SessionMocks.listaTransportadora.Count;
            
            // Abre view
            objetoTran = new TRANSPORTADORA();
            if (SessionMocks.filtroTransportadora != null)
            {
                objetoTran = SessionMocks.filtroTransportadora;
            }
            SessionMocks.voltaTransportadora = 1;
            return View(objetoTran);
        }

        public ActionResult RetirarFiltroTransportadora()
        {
            SessionMocks.listaTransportadora = null;
            SessionMocks.filtroTransportadora = null;
            return RedirectToAction("MontarTelaTransportadora");
        }

        public ActionResult MostrarTudoTransportadora()
        {
            listaMasterTran = tranApp.GetAllItensAdm();
            SessionMocks.filtroTransportadora = null;
            SessionMocks.listaTransportadora = listaMasterTran;
            return RedirectToAction("MontarTelaTransportadora");
        }

        [HttpPost]
        public ActionResult FiltrarTransportadora(TRANSPORTADORA item)
        {
            try
            {
                // Executa a operação
                List<TRANSPORTADORA> listaObj = new List<TRANSPORTADORA>();
                SessionMocks.filtroTransportadora = item;
                Int32 volta = tranApp.ExecuteFilter(item.TRAN_NM_NOME, item.TRAN_NR_CNPJ, item.TRAN_NM_EMAIL, item.TRAN_NM_CIDADE, item.TRAN_SG_UF, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    ViewBag.Message = SystemBR_Resource.ResourceManager.GetString("M0010", CultureInfo.CurrentCulture);
                }

                // Sucesso
                listaMasterTran = listaObj;
                SessionMocks.listaTransportadora = listaObj;
                return RedirectToAction("MontarTelaTransportadora");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaTransportadora");
            }
        }

        public ActionResult VoltarBaseTransportadora()
        {
            return RedirectToAction("MontarTelaTransportadora");
        }

        [HttpGet]
        public ActionResult IncluirTransportadora()
        {
            // Prepara listas
            ViewBag.Filiais = new SelectList(tranApp.GetAllFilial(), "FILI_CD_ID", "FILI_NM_NOME");

            // Prepara view
            USUARIO usuario = SessionMocks.UserCredentials;
            TRANSPORTADORA item = new TRANSPORTADORA();
            TransportadoraViewModel vm = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(item);
            vm.ASSI_CD_ID = SessionMocks.IdAssinante;
            vm.TRAN_DT_CADASTRO = DateTime.Today;
            vm.TRAN_IN_ATIVO = 1;
            vm.MATR_CD_ID = SessionMocks.Matriz.MATR_CD_ID;
            vm.FILI_CD_ID = SessionMocks.idFilial;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirTransportadora(TransportadoraViewModel vm)
        {
            ViewBag.Filiais = new SelectList(tranApp.GetAllFilial(), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    TRANSPORTADORA item = Mapper.Map<TransportadoraViewModel, TRANSPORTADORA>(vm);
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    Int32 volta = tranApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0028", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Carrega logo e processa alteracao
                    item.TRAN_AQ_LOGO = "~/Imagens/Base/FotoBase.jpg";
                    volta = tranApp.ValidateEdit(item, item, usuarioLogado);
                    
                    // Cria pastas
                    String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Transportadoras/" + item.TRAN_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Transportadoras/" + item.TRAN_CD_ID.ToString() + "/Logos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMasterTran = new List<TRANSPORTADORA>();
                    SessionMocks.listaTransportadora = null;

                    SessionMocks.idVolta = item.TRAN_CD_ID;
                    return Json(item.TRAN_CD_ID);  //RedirectToAction("EditarTransportadora", new { id = item.TRAN_CD_ID });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult EditarTransportadora(Int32 id)
        {
            // Prepara view
            ViewBag.Filiais = new SelectList(tranApp.GetAllFilial(), "FILI_CD_ID", "FILI_NM_NOME");
            TRANSPORTADORA item = tranApp.GetItemById(id);
            objetoTranAntes = item;
            SessionMocks.transportadora = item;
            SessionMocks.idVolta = id;
            TransportadoraViewModel vm = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditarTransportadora(TransportadoraViewModel vm)
        {
            ViewBag.Filiais = new SelectList(tranApp.GetAllFilial(), "FILI_CD_ID", "FILI_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = SessionMocks.UserCredentials;
                    TRANSPORTADORA item = Mapper.Map<TransportadoraViewModel, TRANSPORTADORA>(vm);
                    Int32 volta = tranApp.ValidateEdit(item, objetoTranAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterTran = new List<TRANSPORTADORA>();
                    SessionMocks.listaTransportadora = null;
                    return RedirectToAction("MontarTelaTransportadora");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ExcluirTransportadora(Int32 id)
        {
            // Prepara view
            TRANSPORTADORA item = tranApp.GetItemById(id);
            TransportadoraViewModel vm = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirTransportadora(TransportadoraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                TRANSPORTADORA item = Mapper.Map<TransportadoraViewModel, TRANSPORTADORA>(vm);
                Int32 volta = tranApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0029", CultureInfo.CurrentCulture));
                    return View(vm);
                }

                // Sucesso
                listaMasterTran = new List<TRANSPORTADORA>();
                SessionMocks.listaTransportadora = null;
                return RedirectToAction("MontarTelaTransportadora");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ReativarTransportadora(Int32 id)
        {
            // Prepara view
            TRANSPORTADORA item = tranApp.GetItemById(id);
            TransportadoraViewModel vm = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarTransportadora(TransportadoraViewModel vm)
        {
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                TRANSPORTADORA item = Mapper.Map<TransportadoraViewModel, TRANSPORTADORA>(vm);
                Int32 volta = tranApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMasterTran = new List<TRANSPORTADORA>();
                SessionMocks.listaTransportadora = null;
                return RedirectToAction("MontarTelaTransportadora");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult VerCardsTransportadora()
        {
            // Carrega listas
            if (SessionMocks.listaTransportadora == null)
            {
                listaMasterTran = tranApp.GetAllItens();
                SessionMocks.listaTransportadora = listaMasterTran;
            }
            ViewBag.Listas = SessionMocks.listaTransportadora;
            ViewBag.Title = "Transportadoras";
            SessionMocks.Matriz = matrizApp.GetAllItens().FirstOrDefault();
            ViewBag.Filiais = new SelectList(tranApp.GetAllFilial(), "FILI_CD_ID", "FILI_NM_NOME");

            // Indicadores
            ViewBag.Transportadoras = tranApp.GetAllItens().Count;

            // Abre view
            objetoTran = new TRANSPORTADORA();
            SessionMocks.voltaTransportadora = 2;
            if (SessionMocks.filtroTransportadora != null)
            {
                objetoTran = SessionMocks.filtroTransportadora;
            }
            return View(objetoTran);
        }

        [HttpGet]
        public ActionResult VerAnexoTransportadora(Int32 id)
        {
            // Prepara view
            TRANSPORTADORA_ANEXO item = tranApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoTransportadora()
        {
            return RedirectToAction("EditarTransportadora", new { id = SessionMocks.idVolta });
        }

        public FileResult DownloadTransportadora(Int32 id)
        {
            TRANSPORTADORA_ANEXO item = tranApp.GetAnexoById(id);
            String arquivo = item.TRAX_AQ_ARQUIVO;
            Int32 pos = arquivo.LastIndexOf("/") + 1;
            String nomeDownload = arquivo.Substring(pos);
            String contentType = string.Empty;
            if (arquivo.Contains(".pdf"))
            {
                contentType = "application/pdf";
            }
            else if (arquivo.Contains(".jpg"))
            {
                contentType = "image/jpg";
            }
            else if (arquivo.Contains(".png"))
            {
                contentType = "image/png";
            }
            return File(arquivo, contentType, nomeDownload);
        }

        [HttpPost]
        public ActionResult UploadFileTransportadora(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoTransportadora");
            }

            TRANSPORTADORA item = tranApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0041", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoTransportadora");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Transportadoras/" + item.TRAN_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            TRANSPORTADORA_ANEXO foto = new TRANSPORTADORA_ANEXO();
            foto.TRAX_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.TRAX_DT_ANEXO = DateTime.Today;
            foto.TRAX_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            foto.TRAX_IN_TIPO = tipo;
            foto.TRAX_NM_TITULO = fileName;
            foto.TRAN_CD_ID = item.TRAN_CD_ID;

            item.TRANSPORTADORA_ANEXO.Add(foto);
            objetoTranAntes = item;
            Int32 volta = tranApp.ValidateEdit(item, objetoTranAntes);
            return RedirectToAction("VoltarAnexoTransportadora");
        }

        [HttpPost]
        public JsonResult UploadFileTransportadora_Inclusao(IEnumerable<HttpPostedFileBase> files, Int32 perfil)
        {
            var count = 0;

            if (perfil == 0)
            {
                count++;
            }

            foreach (var file in files)
            {
                if (count == 0)
                {
                    UploadFotoTransportadora(file);

                    count++;
                }
                else
                {
                    UploadFileTransportadora(file);
                }
            }

            return Json("1"); // VoltarAnexoCliente
        }

        [HttpPost]
        public ActionResult UploadFotoTransportadora(HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0076", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoTransportadora");
            }

            TRANSPORTADORA item = tranApp.GetById(SessionMocks.idVolta);
            USUARIO usu = SessionMocks.UserCredentials;
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0041", CultureInfo.CurrentCulture));
                return RedirectToAction("VoltarAnexoTransportadora");
            }
            String caminho = "/Imagens/" + SessionMocks.IdAssinante.ToString() + "/Transportadoras/" + item.TRAN_CD_ID.ToString() + "/Logos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.TRAN_AQ_LOGO = "~" + caminho + fileName;
            objetoTranAntes = item;
            Int32 volta = tranApp.ValidateEdit(item, objetoTranAntes);
            return RedirectToAction("VoltarAnexoTransportadora");
        }

        [HttpGet]
        public ActionResult SlideShowTransportadora()
        {
            // Prepara view
            TRANSPORTADORA item = tranApp.GetItemById(SessionMocks.idVolta);
            objetoTranAntes = item;
            TransportadoraViewModel vm = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult BuscarCEPTransportadora()
        {
            // Prepara view
            if (SessionMocks.voltaCEP == 1)
            {
                TRANSPORTADORA item = SessionMocks.transportadora;
                TransportadoraViewModel vm = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(item);
                vm.TRAN_NR_CEP_BUSCA = String.Empty;
                return View(vm);
            }
            else
            {
                TransportadoraViewModel vm = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(SessionMocks.transportadora);
                vm.TRAN_NR_CEP_BUSCA = String.Empty;
                return View(vm);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult BuscarCEPTransportadora(TransportadoraViewModel vm)
        {
            try
            {
                // Atualiza cliente
                TRANSPORTADORA item = SessionMocks.transportadora;
                TRANSPORTADORA cli = new TRANSPORTADORA();
                cli.ASSI_CD_ID = item.ASSI_CD_ID;
                cli.ASSI_CD_ID = item.ASSI_CD_ID;
                cli.MATR_CD_ID = item.MATR_CD_ID;
                cli.FILI_CD_ID = item.FILI_CD_ID;
                cli.TRAN_AQ_LOGO = item.TRAN_AQ_LOGO;
                cli.TRAN_CD_ID = item.TRAN_CD_ID;
                cli.TRAN_DS_INFORMACOES_GERAIS = item.TRAN_DS_INFORMACOES_GERAIS;
                cli.TRAN_DT_CADASTRO = item.TRAN_DT_CADASTRO;
                cli.TRAN_IN_ATIVO = item.TRAN_IN_ATIVO;
                cli.TRAN_NM_BAIRRO = item.TRAN_NM_BAIRRO;
                cli.TRAN_NM_CIDADE = item.TRAN_NM_CIDADE;
                cli.TRAN_NM_CONTATOS = item.TRAN_NM_CONTATOS;
                cli.TRAN_NM_EMAIL = item.TRAN_NM_EMAIL;
                cli.TRAN_NM_ENDERECO = item.TRAN_NM_ENDERECO;
                cli.TRAN_NM_NOME = item.TRAN_NM_NOME;
                cli.TRAN_NM_RAZAO = item.TRAN_NM_RAZAO;
                cli.TRAN_NM_WEBSITE = item.TRAN_NM_WEBSITE;
                cli.TRAN_NR_CEP = item.TRAN_NR_CEP;
                cli.TRAN_NR_CNPJ = item.TRAN_NR_CNPJ;
                cli.TRAN_NR_INSCRICAO_ESTADUAL = item.TRAN_NR_INSCRICAO_ESTADUAL;
                cli.TRAN_NR_INSCRICAO_MUNICIPAL = item.TRAN_NR_INSCRICAO_MUNICIPAL;
                cli.TRAN_NR_TELEFONES = item.TRAN_NR_TELEFONES;
                cli.TRAN_SG_UF = item.TRAN_SG_UF;
                cli.TRAN_TX_OBSERVACOES = item.TRAN_TX_OBSERVACOES;

                // Executa a operação
                USUARIO usuarioLogado = SessionMocks.UserCredentials;
                Int32 volta = tranApp.ValidateEdit(cli, cli);

                // Verifica retorno

                // Sucesso
                listaMasterTran = new List<TRANSPORTADORA>();
                SessionMocks.listaTransportadora = null;
                return RedirectToAction("EditarTransportadora", new { id = SessionMocks.idVolta });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(vm);
            }
        }

        public ActionResult PesquisaCEP(TransportadoraViewModel itemVolta)
        {
            // Chama servico ECT
            TRANSPORTADORA cli = tranApp.GetItemById(SessionMocks.idVolta);
            TransportadoraViewModel item = Mapper.Map<TRANSPORTADORA, TransportadoraViewModel>(cli);

            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            String cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(itemVolta.TRAN_NR_CEP_BUSCA);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza            
            item.TRAN_NM_ENDERECO = end.Address + "/" + end.Complement;
            item.TRAN_NM_BAIRRO = end.District;
            item.TRAN_NM_CIDADE = end.City;
            item.TRAN_SG_UF = end.Uf;
            item.TRAN_NR_CEP = itemVolta.TRAN_NR_CEP_BUSCA;

            // Retorna
            SessionMocks.voltaCEP = 2;
            SessionMocks.transportadora = Mapper.Map<TransportadoraViewModel, TRANSPORTADORA>(item);
            return RedirectToAction("BuscarCEPTransportadora");
        }

        public ActionResult GerarRelatorioLista()
        {
            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "TransportadoraLista" + "_" + data + ".pdf";
            List<TRANSPORTADORA> lista = SessionMocks.listaTransportadora;
            TRANSPORTADORA filtro = SessionMocks.filtroTransportadora;
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // Linha horizontal
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line);

            // Cabeçalho
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Images/5.png"));
            image.ScaleAbsolute(50, 50);
            cell.AddElement(image);
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Transportadoras - Listagem", meuFont2))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cell.Border = 0;
            cell.Colspan = 4;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);
            line1 = new Paragraph("  ");
            pdfDoc.Add(line1);

            // Grid
            table = new PdfPTable(new float[] { 120f, 60f, 120f, 120f, 80f, 50f});
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Transportadoras selecionadas pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.Colspan = 6;
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Nome", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("CNPJ", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("E-Mail", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Contatos", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Cidade", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("UF", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            foreach (TRANSPORTADORA item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.TRAN_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.TRAN_NR_CNPJ, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.TRAN_NM_EMAIL, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.TRAN_NM_CONTATOS, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.TRAN_NM_CIDADE, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.TRAN_SG_UF, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line2);

            // Rodapé
            Chunk chunk1 = new Chunk("Parâmetros de filtro: ", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            String parametros = String.Empty;
            Int32 ja = 0;
            if (filtro != null)
            {
                if (filtro.TRAN_NM_NOME != null)
                {
                    parametros += "Nome: " + filtro.TRAN_NM_NOME;
                    ja = 1;
                }
                if (filtro.TRAN_NR_CNPJ != null)
                {
                    if (ja == 0)
                    {
                        parametros += "CNPJ: " + filtro.TRAN_NR_CNPJ;
                        ja = 1;
                    }
                    else
                    {
                        parametros +=  " e CNPJ: " + filtro.TRAN_NR_CNPJ;
                    }
                }
                if (filtro.TRAN_NM_EMAIL != null)
                {
                    if (ja == 0)
                    {
                        parametros += "E-Mail: " + filtro.TRAN_NM_EMAIL;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e E-Mail: " + filtro.TRAN_NM_EMAIL;
                    }
                }
                if (filtro.TRAN_NM_CONTATOS != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Contatos: " + filtro.TRAN_NM_CONTATOS;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Contatos: " + filtro.TRAN_NM_CONTATOS;
                    }
                }
                if (filtro.TRAN_NM_CIDADE != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Cidade: " + filtro.TRAN_NM_CIDADE;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Cidade: " + filtro.TRAN_NM_CIDADE;
                    }
                }
                if (filtro.TRAN_SG_UF != null)
                {
                    if (ja == 0)
                    {
                        parametros += "UF: " + filtro.TRAN_SG_UF;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e UF: " + filtro.TRAN_SG_UF;
                    }
                }
                if (ja == 0)
                {
                    parametros = "Nenhum filtro definido.";
                }
            }
            else
            {
                parametros = "Nenhum filtro definido.";
            }
            Chunk chunk = new Chunk(parametros, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);

            // Linha Horizontal
            Paragraph line3 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line3);

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

            return RedirectToAction("MontarTelaTransportadora");
        }

    }
}