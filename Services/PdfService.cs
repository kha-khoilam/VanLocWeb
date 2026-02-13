using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using VanLocWeb.Models;

namespace VanLocWeb.Services
{
    public class PdfService
    {
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public PdfService(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _env = env;
            QuestPDF.Settings.License = LicenseType.Community;

            // Tải font từ thư mục wwwroot/fonts nếu có (giúp chạy tốt trên Linux/Vercel)
            var fontPath = Path.Combine(_env.WebRootPath, "fonts", "font.ttf");
            if (File.Exists(fontPath))
            {
                using var stream = File.OpenRead(fontPath);
                FontManager.RegisterFont(stream);
            }
        }

        public byte[] GenerateFinanceReport(List<FinanceTransaction> transactions, string title)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial")); // Hoặc tên font bạn vừa đăng ký

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("HỘI ĐỒNG HƯƠNG VẠN LỘC").FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Ban Chấp hành - Ban Tài chính").FontSize(10).Italic();
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(10);
                            col.Item().Text("BÁO CÁO TÀI CHÍNH").FontSize(14).Bold();
                        });
                    });

                    page.Content().PaddingVertical(10).Column(x =>
                    {
                        x.Item().Text(title).FontSize(14).SemiBold().AlignCenter();
                        x.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("STT");
                                header.Cell().Element(CellStyle).Text("Ngày");
                                header.Cell().Element(CellStyle).Text("Nội dung");
                                header.Cell().Element(CellStyle).Text("Loại");
                                header.Cell().Element(CellStyle).Text("Số tiền");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            int index = 1;
                            foreach (var t in transactions)
                            {
                                table.Cell().Element(CellStyle).Text(index++.ToString());
                                table.Cell().Element(CellStyle).Text(t.Date.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(t.Description);
                                table.Cell().Element(CellStyle).Text(t.Type == "Income" ? "Thu" : "Chi");
                                table.Cell().Element(CellStyle).AlignRight().Text(t.Amount.ToString("N0") + "đ");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5);
                                }
                            }
                        });

                        var income = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
                        var expense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

                        x.Item().PaddingTop(20).AlignRight().Column(col =>
                        {
                            col.Item().Text($"Tổng thu: {income:N0}đ").SemiBold().FontColor(Colors.Green.Medium);
                            col.Item().Text($"Tổng chi: {expense:N0}đ").SemiBold().FontColor(Colors.Red.Medium);
                            col.Item().Text($"Số dư: {(income - expense):N0}đ").Bold().FontSize(12).FontColor(Colors.Blue.Medium);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateParticipantList(List<EventRegistration> registrations, string eventTitle)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                    page.Header().Text(text =>
                    {
                        text.Line("DANH SÁCH ĐĂNG KÝ THAM GIA").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        text.Line(eventTitle).FontSize(14);
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.ConstantColumn(100);
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#");
                            header.Cell().Element(CellStyle).Text("Họ và tên");
                            header.Cell().Element(CellStyle).Text("Số điện thoại");
                            header.Cell().Element(CellStyle).Text("Số người");
                            header.Cell().Element(CellStyle).Text("Ghi chú");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        foreach (var reg in registrations)
                        {
                            table.Cell().Element(ValueStyle).Text(reg.Id.ToString());
                            table.Cell().Element(ValueStyle).Text(reg.FullName);
                            table.Cell().Element(ValueStyle).Text(reg.Phone);
                            table.Cell().Element(ValueStyle).Text(reg.NumberOfAttendees.ToString());
                            table.Cell().Element(ValueStyle).Text(reg.Note ?? "");

                            static IContainer ValueStyle(IContainer container)
                            {
                                return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                    });
                });
            });

            using (var stream = new MemoryStream())
            {
                document.GeneratePdf(stream);
                return stream.ToArray();
            }
        }
    }
}
