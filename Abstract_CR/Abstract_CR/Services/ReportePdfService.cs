using Abstract_CR.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Abstract_CR.Services
{
    public class ReportePdfService : IReportePdfService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ReportePdfService> _logger;

        public ReportePdfService(IWebHostEnvironment environment, ILogger<ReportePdfService> logger)
        {
            _environment = environment;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerarReporteAsync(ReportesDashboardViewModel model)
        {
            try
            {
                var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo-abstract.png");
                byte[]? logoBytes = null;

                if (File.Exists(logoPath))
                {
                    logoBytes = await File.ReadAllBytesAsync(logoPath);
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Element(ComposeHeader);

                        page.Content()
                            .Element(container => ComposeContent(container, model));

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                                x.Span(" de ");
                                x.TotalPages();
                                x.Span(" | Generado el ");
                                x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                                x.Span(" | Abstract - Sistema de Gestión");
                            });
                    });
                });

                void ComposeHeader(IContainer container)
                {
                    container.Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Reporte Administrativo Abstract")
                                .Style(TextStyle.Default.FontSize(20).Bold().FontColor("#2C3E50"));

                            column.Item().Text($"Período: {model.FechaInicio:dd/MM/yyyy} - {model.FechaFin:dd/MM/yyyy}")
                                .Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken1));
                        });

                        if (logoBytes != null)
                        {
                            row.ConstantItem(60).Image(logoBytes).FitArea();
                        }
                        else
                        {
                            row.ConstantItem(60).AlignCenter().AlignMiddle().Text("ABSTRACT")
                                .Style(TextStyle.Default.FontSize(16).Bold().FontColor("#18BC9C"));
                        }
                    });
                }

                void ComposeContent(IContainer container, ReportesDashboardViewModel model)
                {
                    container
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Portada
                            column.Item().PageBreak();
                            column.Item().Element(ComposePortada);

                            // Mtricas
                            column.Item().PageBreak();
                            column.Item().Element(c => ComposeMetricas(c, model.Resumen));

                            // Resumen de suscripciones
                            column.Item().PageBreak();
                            column.Item().Element(c => ComposeSuscripcionesPorEstado(c, model.SuscripcionesPorEstado));

                            // Suscripciones por vencer
                            column.Item().PageBreak();
                            column.Item().Element(c => ComposeSuscripcionesPorVencer(c, model.SuscripcionesPorVencer));

                            // Mensajes pendientes
                            column.Item().PageBreak();
                            column.Item().Element(c => ComposeMensajesPendientes(c, model.MensajesPendientes));
                        });
                }

                void ComposePortada(IContainer container)
                {
                    container
                        .Padding(40)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().PaddingBottom(30).Text("");

                            if (logoBytes != null)
                            {
                                column.Item().AlignCenter().Width(150).Image(logoBytes).FitArea();
                            }
                            else
                            {
                                column.Item().AlignCenter().Text("ABSTRACT")
                                    .Style(TextStyle.Default.FontSize(32).Bold().FontColor("#18BC9C"));
                            }

                            column.Item().AlignCenter().PaddingTop(40).Text("Reporte Administrativo")
                                .Style(TextStyle.Default.FontSize(28).Bold().FontColor("#2C3E50"));

                            column.Item().AlignCenter().PaddingTop(20).Text("Resumen de Métricas y Análisis")
                                .Style(TextStyle.Default.FontSize(16).FontColor(Colors.Grey.Darken1));

                            column.Item().AlignCenter().PaddingTop(60).Text($"Período de análisis:")
                                .Style(TextStyle.Default.FontSize(14).FontColor(Colors.Grey.Darken2));

                            column.Item().AlignCenter().PaddingTop(5).Text($"{model.FechaInicio:dd 'de' MMMM 'de' yyyy} - {model.FechaFin:dd 'de' MMMM 'de' yyyy}")
                                .Style(TextStyle.Default.FontSize(16).Bold().FontColor("#2C3E50"));

                            column.Item().AlignCenter().PaddingTop(80).Text($"Generado el {DateTime.Now:dd 'de' MMMM 'de' yyyy 'a las' HH:mm}")
                                .Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken1));
                        });
                }

                void ComposeMetricas(IContainer container, ResumenMetricasViewModel resumen)
                {
                    container
                        .Column(column =>
                        {
                            column.Item().PaddingBottom(10).Text("Resumen de Métricas")
                                .Style(TextStyle.Default.FontSize(18).Bold().FontColor("#2C3E50"));

                            // Usuarios
                            column.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Total Usuarios").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.TotalUsuarios.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor("#2C3E50"));
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Usuarios Activos").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.UsuariosActivos.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor(Colors.Green.Darken2));
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Usuarios Inactivos").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.UsuariosInactivos.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor(Colors.Red.Darken2));
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Nuevas Altas").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.NuevasAltas.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor("#18BC9C"));
                                });
                            });

                            // Suscripciones
                            column.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Total Suscripciones").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.TotalSuscripciones.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor("#2C3E50"));
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Activas").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.SuscripcionesActivas.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor(Colors.Green.Darken2));
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Pausadas").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.SuscripcionesPausadas.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor(Colors.Orange.Darken2));
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Canceladas").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.SuscripcionesCanceladas.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor(Colors.Red.Darken2));
                                });
                            });

                            // Recetas y Mensajes
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Recetas Publicadas").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.TotalRecetas.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor("#18BC9C"));
                                });

                                row.ConstantItem(10);

                                row.RelativeItem().Background("#F8F9FA").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Mensajes Pendientes").Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken2));
                                    col.Item().Text(resumen.MensajesPendientes.ToString()).Style(TextStyle.Default.FontSize(20).Bold().FontColor(Colors.Orange.Darken2));
                                });
                            });
                        });
                }

                void ComposeSuscripcionesPorEstado(IContainer container, IEnumerable<CategoriaValor> suscripciones)
                {
                    container
                        .Column(column =>
                        {
                            column.Item().PaddingBottom(10).Text("Suscripciones por Estado")
                                .Style(TextStyle.Default.FontSize(18).Bold().FontColor("#2C3E50"));

                            if (!suscripciones.Any())
                            {
                                column.Item().Text("No hay datos de suscripciones disponibles.")
                                    .Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken1).Italic());
                                return;
                            }

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Estado").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).AlignRight().Text("Cantidad").Style(TextStyle.Default.Bold());
                                });

                                foreach (var item in suscripciones)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Categoria);
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.Valor.ToString());
                                }
                            });
                        });
                }

                void ComposeSuscripcionesPorVencer(IContainer container, IEnumerable<SuscripcionVencimientoViewModel> suscripciones)
                {
                    container
                        .Column(column =>
                        {
                            column.Item().PaddingBottom(10).Text("Suscripciones Próximas a Vencer")
                                .Style(TextStyle.Default.FontSize(18).Bold().FontColor("#2C3E50"));

                            if (!suscripciones.Any())
                            {
                                column.Item().Text("No hay suscripciones próximas a vencer en los próximos 14 días.")
                                    .Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken1).Italic());
                                return;
                            }

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Usuario").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).Text("Correo").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).Text("Próxima Facturación").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).Text("Estado").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).AlignRight().Text("Días Restantes").Style(TextStyle.Default.Bold());
                                });

                                foreach (var item in suscripciones)
                                {
                                    table.Cell().Element(CellStyle).Text(item.NombreUsuario);
                                    table.Cell().Element(CellStyle).Text(item.Correo).Style(TextStyle.Default.FontSize(9));
                                    table.Cell().Element(CellStyle).Text(item.ProximaFacturacion?.ToString("dd/MM/yyyy") ?? "N/A");
                                    table.Cell().Element(CellStyle).Text(item.Estado);
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.DiasRestantes?.ToString() ?? "N/A");
                                }
                            });
                        });
                }

                void ComposeMensajesPendientes(IContainer container, IEnumerable<InteraccionPendienteViewModel> mensajes)
                {
                    container
                        .Column(column =>
                        {
                            column.Item().PaddingBottom(10).Text("Mensajes Pendientes de Respuesta")
                                .Style(TextStyle.Default.FontSize(18).Bold().FontColor("#2C3E50"));

                            if (!mensajes.Any())
                            {
                                column.Item().Text("No hay mensajes pendientes de respuesta.")
                                    .Style(TextStyle.Default.FontSize(12).FontColor(Colors.Grey.Darken1).Italic());
                                return;
                            }

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Usuario").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).Text("Correo").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).Text("Fecha Último Mensaje").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).Text("Vista Previa").Style(TextStyle.Default.Bold());
                                    header.Cell().Element(CellStyle).AlignRight().Text("Total").Style(TextStyle.Default.Bold());
                                });

                                foreach (var item in mensajes)
                                {
                                    table.Cell().Element(CellStyle).Text(item.NombreUsuario);
                                    table.Cell().Element(CellStyle).Text(item.Correo).Style(TextStyle.Default.FontSize(9));
                                    table.Cell().Element(CellStyle).Text(item.FechaUltimoMensaje.ToString("dd/MM/yyyy HH:mm"));
                                    table.Cell().Element(CellStyle).Text(item.ContenidoUltimoMensaje.Length > 50 
                                        ? item.ContenidoUltimoMensaje.Substring(0, 50) + "..." 
                                        : item.ContenidoUltimoMensaje).Style(TextStyle.Default.FontSize(9));
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.TotalPendientes.ToString());
                                }
                            });
                        });
                }

                IContainer CellStyle(IContainer container)
                {
                    return container
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingVertical(5)
                        .PaddingHorizontal(5);
                }

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el PDF del reporte");
                throw;
            }
        }

        /// <inheritdoc />
        public Task<byte[]> GenerarCocinaPdfAsync(IReadOnlyList<CocinaClienteFilaViewModel> filas)
        {
            try
            {
                var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo-abstract.png");
                byte[]? logoBytes = File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;

                static string Truncar(string? texto, int max)
                {
                    if (string.IsNullOrEmpty(texto)) return "—";
                    texto = texto.Trim();
                    return texto.Length <= max ? texto : texto[..max] + "…";
                }

                static string RestriccionesTexto(CocinaClienteFilaViewModel f)
                {
                    if (f.Restricciones == null || f.Restricciones.Count == 0)
                        return "Sin restricciones registradas";
                    return string.Join("; ", f.Restricciones);
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.2f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(header =>
                        {
                            header.Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Cocina · Clientes con suscripción activa")
                                        .Style(TextStyle.Default.FontSize(16).Bold().FontColor("#2C3E50"));
                                    col.Item().Text("Dirección de entrega y restricciones alimentarias")
                                        .Style(TextStyle.Default.FontSize(10).FontColor(Colors.Grey.Darken1));
                                });
                                if (logoBytes != null)
                                    row.ConstantItem(50).Image(logoBytes).FitArea();
                                else
                                    row.ConstantItem(50).AlignMiddle().Text("ABSTRACT")
                                        .Style(TextStyle.Default.FontSize(12).Bold().FontColor("#18BC9C"));
                            });
                        });

                        page.Content().PaddingTop(8).Element(content =>
                        {
                            if (filas == null || filas.Count == 0)
                            {
                                content.Text("No hay registros para mostrar.")
                                    .Style(TextStyle.Default.FontSize(11).Italic().FontColor(Colors.Grey.Darken2));
                                return;
                            }

                            content.Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(1.4f);
                                    cols.RelativeColumn(1.8f);
                                    cols.RelativeColumn(2f);
                                    cols.RelativeColumn(0.9f);
                                    cols.RelativeColumn(0.9f);
                                    cols.RelativeColumn(2f);
                                });

                                static IContainer Cell(IContainer c) =>
                                    c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(4);

                                table.Header(h =>
                                {
                                    h.Cell().Element(Cell).Text("Cliente").Style(TextStyle.Default.Bold());
                                    h.Cell().Element(Cell).Text("Correo").Style(TextStyle.Default.Bold());
                                    h.Cell().Element(Cell).Text("Dirección entrega").Style(TextStyle.Default.Bold());
                                    h.Cell().Element(Cell).Text("Suscripción").Style(TextStyle.Default.Bold());
                                    h.Cell().Element(Cell).Text("Vence").Style(TextStyle.Default.Bold());
                                    h.Cell().Element(Cell).Text("Restricciones").Style(TextStyle.Default.Bold());
                                });

                                foreach (var f in filas)
                                {
                                    table.Cell().Element(Cell).Text(Truncar(f.NombreCompleto, 80));
                                    table.Cell().Element(Cell).Text(Truncar(f.CorreoElectronico, 60)).FontSize(7);
                                    table.Cell().Element(Cell).Text(Truncar(f.DireccionEntrega, 120)).FontSize(7);
                                    table.Cell().Element(Cell).Text(f.EstadoSuscripcion);
                                    table.Cell().Element(Cell).Text(
                                        f.FechaFinSuscripcion.HasValue
                                            ? f.FechaFinSuscripcion.Value.ToString("dd/MM/yyyy")
                                            : "Sin fecha fin");
                                    table.Cell().Element(Cell).Text(Truncar(RestriccionesTexto(f), 400)).FontSize(7);
                                }
                            });
                        });

                        page.Footer().AlignCenter().Text(t =>
                        {
                            t.Span("Página ");
                            t.CurrentPageNumber();
                            t.Span(" de ");
                            t.TotalPages();
                            t.Span(" | Generado ");
                            t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                            t.Span(" | Abstract");
                        });
                    });
                });

                return Task.FromResult(document.GeneratePdf());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el PDF de Cocina");
                throw;
            }
        }
    }
}
