﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
using AutoMapper;
using OdontoWeb.ViewModels;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Odonto.Controllers
{
    public class CentroCustoController : Controller
    {
        private readonly ICentroCustoAppService ccApp;
        private readonly IGrupoAppService gruApp;
        private readonly ISubgrupoAppService subApp;

        private String msg;
        private Exception exception;
        CENTRO_CUSTO objetoCC = new CENTRO_CUSTO();
        CENTRO_CUSTO objetoCCAntes = new CENTRO_CUSTO();
        List<CENTRO_CUSTO> listaMasterCC = new List<CENTRO_CUSTO>();
        String extensao;

        public CentroCustoController(ICentroCustoAppService ccApps, IGrupoAppService gruApps, ISubgrupoAppService subApps)
        {
            ccApp = ccApps;
            gruApp = gruApps;
            subApp = subApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return View();
        }

        public ActionResult Voltar()
        {
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarBase()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaCC"] = ccApp.GetAllItens(idAss);
           return RedirectToAction("MontarTelaCC");
        }

        [HttpGet]
        public ActionResult MontarTelaCC()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCC"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if (Session["LIstaCC"] == null)
            {
                listaMasterCC = ccApp.GetAllItens(idAss);
                Session["LIstaCC"] = listaMasterCC;
            }
            ViewBag.Listas = (List<CENTRO_CUSTO>)Session["ListaCC"];
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            ViewBag.Title = "Centros de Custos";
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            ViewBag.Subs = new SelectList(subApp.GetAllItens(idAss), "SUBG_CD_ID", "SUBG_NM_EXIBE");
            List<SelectListItem> tipoCC = new List<SelectListItem>();
            tipoCC.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoCC.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.Tipos = new SelectList(tipoCC, "Value", "Text");
            List<SelectListItem> tipoMov = new List<SelectListItem>();
            tipoMov.Add(new SelectListItem() { Text = "Todas", Value = "1" });
            tipoMov.Add(new SelectListItem() { Text = "Compras", Value = "2" });
            tipoMov.Add(new SelectListItem() { Text = "Vendas", Value = "3" });
            ViewBag.TipoMov = new SelectList(tipoMov, "Value", "Text");


            // Indicadores
            ViewBag.Cats = ((List<CENTRO_CUSTO>)Session["ListaCC"]).Count;

            // Mensagem
            if ((Int32)Session["MensCC"] == 1)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCC"] == 3)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0036", CultureInfo.CurrentCulture));
            }
            if ((Int32)Session["MensCC"] == 4)
            {
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0037", CultureInfo.CurrentCulture));
            }

            // Abre view
            objetoCC = new CENTRO_CUSTO();
            return View(objetoCC);
        }

        public ActionResult RetirarFiltroCC()
        {
            Session["ListsCC"] = null;
            Session["FiltroCC"] = null;
            return RedirectToAction("MontarTelaCC");
        }

        public ActionResult MostrarTudoCC()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterCC = ccApp.GetAllItensAdm(idAss);
            Session["ListaCC"] = listaMasterCC;
            return RedirectToAction("MontarTelaCC");
        }

        [HttpPost]
        public ActionResult FiltrarCC(CENTRO_CUSTO item)
        {
            try
            {
                // Executa a operação
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<CENTRO_CUSTO> listaObj = new List<CENTRO_CUSTO>();
                Session["FiltroCC"] = item;
                Int32 volta = ccApp.ExecuteFilter(item.GRUP_CD_ID, item.SUBG_CD_ID, item.CECU_IN_TIPO, item.CECU_IN_MOVTO, item.CECU_NR_NUMERO, item.CECU_NM_NOME, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensCC"] = 1;
                    ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0016", CultureInfo.CurrentCulture));
                }

                // Sucesso
                listaMasterCC = listaObj;
                Session["ListaCC"] = listaObj;
                return RedirectToAction("MontarTelaCC");

            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaCC");
            }
        }

        // Filtro em cascata de subgrupo
        [HttpPost]
        public JsonResult FiltroSubGrupoCC(Int32? id)
        {
            var listaSubFiltrada = new List<SUBGRUPO>();
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Filtro para caso o placeholder seja selecionado
            if (id == null)
            {
                listaSubFiltrada = subApp.GetAllItens(idAss);
            }
            else
            {
                listaSubFiltrada = subApp.GetAllItens(idAss).Where(x => x.SUBG_CD_ID == id).ToList();
            }

            return Json(listaSubFiltrada.Select(x => new { x.SUBG_CD_ID, x.SUBG_NM_NOME }));
        }

        public ActionResult VoltarBaseCC()
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaCC"] = ccApp.GetAllItens(idAss);
            return RedirectToAction("MontarTelaCC");
        }

        [HttpGet]
        public ActionResult IncluirCC()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCC"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            ViewBag.Subs = new SelectList(subApp.GetAllItens(idAss), "SUBG_CD_ID", "SUBG_NM_EXIBE");
            List<SelectListItem> tipoCC = new List<SelectListItem>();
            tipoCC.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoCC.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.TiposLancamento = new SelectList(tipoCC, "Value", "Text");
            List<SelectListItem> tipoMov = new List<SelectListItem>();
            tipoMov.Add(new SelectListItem() { Text = "Todas", Value = "1" });
            tipoMov.Add(new SelectListItem() { Text = "Compras", Value = "2" });
            tipoMov.Add(new SelectListItem() { Text = "Vendas", Value = "3" });
            ViewBag.TipoMov = new SelectList(tipoMov, "Value", "Text");

            // Prepara view
            CENTRO_CUSTO item = new CENTRO_CUSTO();
            CentroCustoViewModel vm = Mapper.Map<CENTRO_CUSTO, CentroCustoViewModel>(item);
            vm.CECU_IN_ATIVO = 1;
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirCC(CentroCustoViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            ViewBag.Subs = new SelectList(subApp.GetAllItens(idAss), "SUBG_CD_ID", "SUBG_NM_EXIBE");
            List<SelectListItem> tipoCC = new List<SelectListItem>();
            tipoCC.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoCC.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.TiposLancamento = new SelectList(tipoCC, "Value", "Text");
            List<SelectListItem> tipoMov = new List<SelectListItem>();
            tipoMov.Add(new SelectListItem() { Text = "Todas", Value = "1" });
            tipoMov.Add(new SelectListItem() { Text = "Compras", Value = "2" });
            tipoMov.Add(new SelectListItem() { Text = "Vendas", Value = "3" });
            ViewBag.TipoMov = new SelectList(tipoMov, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    CENTRO_CUSTO item = Mapper.Map<CentroCustoViewModel, CENTRO_CUSTO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = ccApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensCC"] = 3;
                        ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0036", CultureInfo.CurrentCulture));
                        return View(vm);
                    }

                    // Sucesso
                    listaMasterCC = new List<CENTRO_CUSTO>();
                    Session["ListaCC"] = null;
                    return RedirectToAction("MontarTelaCC");
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
        public ActionResult EditarCC(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCC"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            ViewBag.Subs = new SelectList(subApp.GetAllItens(idAss), "SUBG_CD_ID", "SUBG_NM_EXIBE");
            List<SelectListItem> tipoCC = new List<SelectListItem>();
            tipoCC.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoCC.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.TiposLancamento = new SelectList(tipoCC, "Value", "Text");
            List<SelectListItem> tipoMov = new List<SelectListItem>();
            tipoMov.Add(new SelectListItem() { Text = "Todas", Value = "1" });
            tipoMov.Add(new SelectListItem() { Text = "Compras", Value = "2" });
            tipoMov.Add(new SelectListItem() { Text = "Vendas", Value = "3" });
            ViewBag.TipoMov = new SelectList(tipoMov, "Value", "Text");

            // Prepara view
            CENTRO_CUSTO item = ccApp.GetItemById(id);
            objetoCCAntes = item;
            Session["CCusto"] = item;
            Session["idVolta"] = id;
            CentroCustoViewModel vm = Mapper.Map<CENTRO_CUSTO, CentroCustoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EditarCC(CentroCustoViewModel vm)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Grupos = new SelectList(gruApp.GetAllItens(idAss), "GRUP_CD_ID", "GR_NM_EXIBE");
            ViewBag.Subs = new SelectList(subApp.GetAllItens(idAss), "SUBG_CD_ID", "SUBG_NM_EXIBE");
            List<SelectListItem> tipoCC = new List<SelectListItem>();
            tipoCC.Add(new SelectListItem() { Text = "Crédito", Value = "1" });
            tipoCC.Add(new SelectListItem() { Text = "Débito", Value = "2" });
            ViewBag.TiposLancamento = new SelectList(tipoCC, "Value", "Text");
            List<SelectListItem> tipoMov = new List<SelectListItem>();
            tipoMov.Add(new SelectListItem() { Text = "Todas", Value = "1" });
            tipoMov.Add(new SelectListItem() { Text = "Compras", Value = "2" });
            tipoMov.Add(new SelectListItem() { Text = "Vendas", Value = "3" });
            ViewBag.TipoMov = new SelectList(tipoMov, "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    CENTRO_CUSTO item = Mapper.Map<CentroCustoViewModel, CENTRO_CUSTO>(vm);
                    Int32 volta = ccApp.ValidateEdit(item, objetoCCAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterCC = new List<CENTRO_CUSTO>();
                    Session["ListaCC"] = null;
                    return RedirectToAction("MontarTelaCC");
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
        public ActionResult ExcluirCC(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCC"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            CENTRO_CUSTO item = ccApp.GetItemById(id);
            objetoCCAntes = (CENTRO_CUSTO)Session["CCusto"];
            item.CECU_IN_ATIVO = 0;
            item.GRUPO = null;
            item.SUBGRUPO = null;
            item.ASSINANTE = null;
            Int32 volta = ccApp.ValidateDelete(item, usuario);
            if (volta == 1)
            {
                Session["MensCC"] = 4;
                ModelState.AddModelError("", OdontoWeb_Resources.ResourceManager.GetString("M0037", CultureInfo.CurrentCulture));
                return RedirectToAction("MontarTelaCC");
            }
            listaMasterCC = new List<CENTRO_CUSTO>();
            Session["ListaCC"] = null;
            return RedirectToAction("MontarTelaCC");
        }

        [HttpGet]
        public ActionResult ReativarCC(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA != "ADM")
                {
                    Session["MensCC"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Executar
            CENTRO_CUSTO item = ccApp.GetItemById(id);
            objetoCCAntes = (CENTRO_CUSTO)Session["CCusto"];
            item.CECU_IN_ATIVO = 1;
            item.GRUPO = null;
            item.SUBGRUPO = null;
            item.ASSINANTE = null;
            Int32 volta = ccApp.ValidateReativar(item, usuario);
            listaMasterCC = new List<CENTRO_CUSTO>();
            Session["ListaCC"] = null;
            return RedirectToAction("MontarTelaCC");
        }

        public ActionResult GerarRelatorioLista()
        {
            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "CCLista" + "_" + data + ".pdf";
            List<CENTRO_CUSTO> lista = (List<CENTRO_CUSTO>)Session["ListsCC"];
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);
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

            cell = new PdfPCell(new Paragraph("Centros de Custo - Listagem", meuFont2))
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
            table = new PdfPTable(new float[] { 70f, 150f, 70f, 70f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Número", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Nome", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Tipo", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Movimento", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            foreach (CENTRO_CUSTO item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.CECU_NR_NUMERO, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.CECU_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.CECU_IN_TIPO == 1)
                {
                    cell = new PdfPCell(new Paragraph("Crédito", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("Débito", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                if (item.CECU_IN_MOVTO == 1)
                {
                    cell = new PdfPCell(new Paragraph("Todos", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else if (item.CECU_IN_MOVTO == 2)
                {
                    cell = new PdfPCell(new Paragraph("Compra", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("Venda", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line2);

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

            return RedirectToAction("MontarTelaCC");
        }
    }
}