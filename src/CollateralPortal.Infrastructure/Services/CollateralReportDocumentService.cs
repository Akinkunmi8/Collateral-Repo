using CollateralPortal.Application.Collaterals
    .Queries.GetCollateralAssessmentReport;
using CollateralPortal.Application.Common.Interfaces;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using System.Globalization;

using PdfDocument =
    QuestPDF.Fluent.Document;

using WordDocument =
    DocumentFormat.OpenXml.Wordprocessing.Document;

using WordColor =
    DocumentFormat.OpenXml.Wordprocessing.Color;

using WordPageSize =
    DocumentFormat.OpenXml.Wordprocessing.PageSize;

namespace CollateralPortal.Infrastructure.Services;

public sealed class CollateralReportDocumentService: ICollateralReportDocumentService
{
    private const string PrimaryColor = "#123B69";
    private const string SecondaryColor = "#315F9F";
    private const string BorderColor = "#D8E0E9";
    private const string LightBackground = "#F5F7FA";
    private const string MutedText = "#667085";
    private const string SuccessColor = "#067647";
    private const string DangerColor = "#B42318";

    public CollateralReportDocumentService()
    {
        
    }

    // ============================================================
    // PDF
    // ============================================================

    public byte[] GeneratePdf(
        CollateralAssessmentReportVm report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return PdfDocument.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);

                page.MarginVertical(25);
                page.MarginHorizontal(30);

                page.DefaultTextStyle(
                    x => x
                        .FontSize(9)
                        .FontColor("#263238"));

                page.Header()
                    .Element(container =>
                        ComposePdfHeader(
                            container,
                            report));

                page.Content()
                    .PaddingVertical(15)
                    .Column(column =>
                    {
                        column.Spacing(14);

                        column.Item()
                            .Element(container =>
                                ComposeIdentity(
                                    container,
                                    report));

                        column.Item()
                            .Element(container =>
                                ComposeAssessmentOverview(
                                    container,
                                    report));

                        column.Item()
                            .Element(container =>
                                ComposeLifecycle(
                                    container,
                                    report));

                        column.Item()
                            .Element(container =>
                                ComposeFacilityAndCollateral(
                                    container,
                                    report));

                        column.Item()
                            .Element(container =>
                                ComposeValuation(
                                    container,
                                    report));

                        column.Item()
                            .Element(container =>
                                ComposeControlPosition(
                                    container,
                                    report));

                        column.Item()
                            .Element(container =>
                                ComposeDocuments(
                                    container,
                                    report));

                        column.Item()
                            .Element(container =>
                                ComposeWorkflowHistory(
                                    container,
                                    report));
                    });

                page.Footer()
                    .Element(container =>
                        ComposePdfFooter(
                            container,
                            report));
            });
        }).GeneratePdf();
    }

    // ============================================================
    // PDF HEADER
    // ============================================================

    private static void ComposePdfHeader(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container
            .BorderBottom(1)
            .BorderColor(BorderColor)
            .PaddingBottom(10)
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column.Item()
                            .Text(
                                "COLLATERAL MANAGEMENT SOLUTION")
                            .Bold()
                            .FontSize(13)
                            .FontColor(PrimaryColor);

                        column.Item()
                            .PaddingTop(2)
                            .Text(
                                "Individual Collateral Assessment Report")
                            .FontSize(9)
                            .FontColor(MutedText);
                    });

                row.ConstantItem(210)
                    .AlignRight()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text("REPORT REFERENCE")
                            .FontSize(7)
                            .FontColor(MutedText);

                        column.Item()
                            .AlignRight()
                            .Text(report.ReportReference)
                            .Bold()
                            .FontSize(8);

                        column.Item()
                            .PaddingTop(4)
                            .AlignRight()
                            .Text(
                                $"Generated {FormatDateTime(report.GeneratedAtUtc)}")
                            .FontSize(7)
                            .FontColor(MutedText);
                    });
            });
    }

    // ============================================================
    // CUSTOMER IDENTITY
    // ============================================================

    private static void ComposeIdentity(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container
            .Background(LightBackground)
            .Border(1)
            .BorderColor(BorderColor)
            .Padding(12)
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("CUSTOMER / COLLATERAL")
                            .FontSize(7)
                            .FontColor(MutedText);

                        column.Item()
                            .PaddingTop(3)
                            .Text(report.Header.CustomerName)
                            .Bold()
                            .FontSize(14)
                            .FontColor(PrimaryColor);

                        column.Item()
                            .PaddingTop(4)
                            .Text(
                                $"{report.Header.LoanContractId}  |  " +
                                $"{FormatEnum(report.Header.CollateralType)}  |  " +
                                $"{FormatEnum(report.Header.Region)}")
                            .FontSize(8)
                            .FontColor(MutedText);
                    });

                row.ConstantItem(85)
                    .AlignMiddle()
                    .AlignRight()
                    .Element(status =>
                    {
                        status
                            .Background("#E8EDF3")
                            .PaddingVertical(5)
                            .PaddingHorizontal(10)
                            .Text(
                                FormatEnum(
                                    report.Header.Status))
                            .Bold()
                            .FontSize(8)
                            .FontColor(PrimaryColor);
                    });
            });
    }

    // ============================================================
    // ASSESSMENT OVERVIEW
    // ============================================================

    private static void ComposeAssessmentOverview(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container.Column(column =>
        {
            SectionTitle(
                column,
                "Assessment Overview",
                "Current financial and workflow position.");

            column.Item()
                .PaddingTop(8)
                .Row(row =>
                {
                    row.Spacing(8);

                    row.RelativeItem()
                        .Element(x =>
                            MetricCard(
                                x,
                                "Approved Facility",
                                Money(
                                    report.Overview.ApprovedLimit,
                                    report.Overview.Currency),
                                report.Facility.FacilityType
                                ?? "Facility"));

                    row.RelativeItem()
                        .Element(x =>
                            MetricCard(
                                x,
                                "Open Market Value",
                                Money(
                                    report.Overview.OpenMarketValue,
                                    report.Overview.Currency),
                                "Latest approved valuation"));

                    row.RelativeItem()
                        .Element(x =>
                            MetricCard(
                                x,
                                "Forced Sale Value",
                                Money(
                                    report.Overview.ForcedSaleValue,
                                    report.Overview.Currency),
                                "Realisation value"));

                    row.RelativeItem()
                        .Element(x =>
                            MetricCard(
                                x,
                                "Coverage",
                                Percent(
                                    report.Overview.ActualCoveragePercent),
                                report.Overview.RequiredCoveragePercent
                                    is null
                                    ? "Not assessed"
                                    : $"Required {Percent(report.Overview.RequiredCoveragePercent)}"));
                });

            column.Item()
                .PaddingTop(8)
                .Background(LightBackground)
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(9)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("Current Position")
                        .FontSize(8)
                        .FontColor(MutedText);

                    row.RelativeItem()
                        .Text(
                            FormatEnum(
                                report.Overview.CurrentPosition))
                        .Bold()
                        .FontSize(8);

                    row.RelativeItem()
                        .AlignRight()
                        .Text(
                            report.Overview.CoveragePosition)
                        .Bold()
                        .FontSize(8)
                        .FontColor(
                            CoverageColor(
                                report.Overview.CoveragePosition));
                });
        });
    }

    private static void MetricCard(
        IContainer container,
        string label,
        string value,
        string note)
    {
        container
            .Border(1)
            .BorderColor(BorderColor)
            .Padding(9)
            .MinHeight(72)
            .Column(column =>
            {
                column.Item()
                    .Text(label)
                    .FontSize(7)
                    .FontColor(MutedText);

                column.Item()
                    .PaddingTop(4)
                    .Text(value)
                    .Bold()
                    .FontSize(11)
                    .FontColor(PrimaryColor);

                column.Item()
                    .PaddingTop(3)
                    .Text(note)
                    .FontSize(6.5f)
                    .FontColor(MutedText);
            });
    }

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private static void ComposeLifecycle(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container.Column(column =>
        {
            SectionTitle(
                column,
                "Collateral Lifecycle",
                "Progress through the collateral control process.");

            var lifecycle =
                BuildLifecycle(
                    report.Header.CurrentStage);

            column.Item()
                .PaddingTop(10)
                .Row(row =>
                {
                    row.Spacing(5);

                    foreach (var stage in lifecycle)
                    {
                        row.RelativeItem()
                            .Element(x =>
                            {
                                x.Border(1)
                                    .BorderColor(
                                        stage.Completed
                                            ? "#A6D8C3"
                                            : BorderColor)
                                    .Background(
                                        stage.Completed
                                            ? "#F0FAF5"
                                            : "#FFFFFF")
                                    .Padding(7)
                                    .Column(c =>
                                    {
                                        c.Item()
                                            .AlignCenter()
                                            .Text(
                                                stage.Completed
                                                    ? "Completed"
                                                    : stage.Current
                                                        ? "Current"
                                                        : "Pending")
                                            .Bold()
                                            .FontSize(6.5f)
                                            .FontColor(
                                                stage.Completed
                                                    ? SuccessColor
                                                    : SecondaryColor);

                                        c.Item()
                                            .PaddingTop(3)
                                            .AlignCenter()
                                            .Text(stage.Label)
                                            .Bold()
                                            .FontSize(7);
                                    });
                            });
                    }
                });
        });
    }

    // ============================================================
    // FACILITY & COLLATERAL
    // ============================================================

    private static void ComposeFacilityAndCollateral(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container.Column(column =>
        {
            SectionTitle(
                column,
                "Facility & Collateral",
                "Facility exposure and security information.");

            column.Item()
                .PaddingTop(8)
                .Row(row =>
                {
                    row.Spacing(10);

                    row.RelativeItem()
                        .Element(x =>
                            InfoCard(
                                x,
                                "Facility Information",
                                new[]
                                {
                                    Pair(
                                        "Loan Contract",
                                        report.Facility.LoanContractId),

                                    Pair(
                                        "Customer ID",
                                        report.Facility.CustomerId),

                                    Pair(
                                        "Account Number",
                                        report.Facility.AccountNumber),

                                    Pair(
                                        "Facility Type",
                                        report.Facility.FacilityType),

                                    Pair(
                                        "Approved Limit",
                                        Money(
                                            report.Facility.ApprovedLimit,
                                            report.Facility.Currency)),

                                    Pair(
                                        "Branch",
                                        report.Facility.BranchName),

                                    Pair(
                                        "Relationship Manager",
                                        report.Facility.RelationshipManagerName),

                                    Pair(
                                        "Branch Manager",
                                        report.Facility.BranchManagerName)
                                }));

                    row.RelativeItem()
                        .Element(x =>
                            InfoCard(
                                x,
                                "Collateral Information",
                                new[]
                                {
                                    Pair(
                                        "Type",
                                        FormatEnum(
                                            report.Collateral.Type)),

                                    Pair(
                                        "Owner",
                                        report.Collateral.Owner),

                                    Pair(
                                        "Region",
                                        FormatEnum(
                                            report.Collateral.Region)),

                                    Pair(
                                        "Location",
                                        report.Collateral.Address)
                                }));
                });

            column.Item()
                .PaddingTop(8)
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(9)
                .Column(c =>
                {
                    c.Item()
                        .Text("COLLATERAL DESCRIPTION")
                        .FontSize(7)
                        .FontColor(MutedText);

                    c.Item()
                        .PaddingTop(4)
                        .Text(
                            Display(
                                report.Collateral.Description))
                        .FontSize(8);
                });
        });
    }

    // ============================================================
    // VALUATION
    // ============================================================

    private static void ComposeValuation(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container.Column(column =>
        {
            SectionTitle(
                column,
                "Valuation Assessment",
                "Approved valuation and collateral coverage position.");

            column.Item()
                .PaddingTop(8)
                .Element(x =>
                    InfoCard(
                        x,
                        "Valuation Details",
                        new[]
                        {
                            Pair(
                                "Open Market Value",
                                Money(
                                    report.Valuation.OpenMarketValue,
                                    report.Overview.Currency)),

                            Pair(
                                "Forced Sale Value",
                                Money(
                                    report.Valuation.ForcedSaleValue,
                                    report.Overview.Currency)),

                            Pair(
                                "Insurance Value",
                                Money(
                                    report.Valuation.InsuranceValue,
                                    report.Overview.Currency)),

                            Pair(
                                "Valuation Date",
                                FormatDate(
                                    report.Valuation.ValuationDate)),

                            Pair(
                                "Valuer",
                                report.Valuation.ValuerName),

                            Pair(
                                "Decision",
                                FormatEnum(
                                    report.Valuation.Decision)),

                            Pair(
                                "Required Coverage",
                                Percent(
                                    report.Valuation.RequiredCoveragePercent)),

                            Pair(
                                "Actual Coverage",
                                Percent(
                                    report.Valuation.ActualCoveragePercent))
                        }));

            column.Item()
                .PaddingTop(8)
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(9)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("Coverage Assessment")
                        .FontSize(8)
                        .FontColor(MutedText);

                    row.RelativeItem()
                        .AlignRight()
                        .Text(
                            report.Valuation.CoveragePosition)
                        .Bold()
                        .FontSize(8)
                        .FontColor(
                            CoverageColor(
                                report.Valuation.CoveragePosition));
                });

            if (!string.IsNullOrWhiteSpace(
                    report.Valuation.Remark))
            {
                column.Item()
                    .PaddingTop(8)
                    .Background(LightBackground)
                    .Padding(9)
                    .Column(c =>
                    {
                        c.Item()
                            .Text("VALUATION REMARK")
                            .FontSize(7)
                            .FontColor(MutedText);

                        c.Item()
                            .PaddingTop(4)
                            .Text(
                                report.Valuation.Remark!)
                            .FontSize(8);
                    });
            }
        });
    }

    // ============================================================
    // CONTROL POSITION
    // ============================================================

    private static void ComposeControlPosition(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container.Column(column =>
        {
            SectionTitle(
                column,
                "Collateral Control Position",
                "Legal, insurance and release controls.");

            column.Item()
                .PaddingTop(8)
                .Row(row =>
                {
                    row.Spacing(8);

                    row.RelativeItem()
                        .Element(x =>
                            ControlCard(
                                x,
                                "Legal Perfection",
                                report.ControlPosition.Legal.Status,
                                new[]
                                {
                                    Pair(
                                        "Registration Ref.",
                                        report.ControlPosition
                                            .Legal
                                            .RegistrationReference),

                                    Pair(
                                        "Legal Unit",
                                        report.ControlPosition
                                            .Legal
                                            .LegalUnit),

                                    Pair(
                                        "Collateral Valid",
                                        BoolText(
                                            report.ControlPosition
                                                .Legal
                                                .CollateralValid))
                                }));

                    row.RelativeItem()
                        .Element(x =>
                            ControlCard(
                                x,
                                "Insurance",
                                report.ControlPosition
                                    .Insurance
                                    .Status,
                                new[]
                                {
                                    Pair(
                                        "Insurer",
                                        report.ControlPosition
                                            .Insurance
                                            .Insurer),

                                    Pair(
                                        "Policy Number",
                                        report.ControlPosition
                                            .Insurance
                                            .PolicyNumber),

                                    Pair(
                                        "Coverage",
                                        Money(
                                            report.ControlPosition
                                                .Insurance
                                                .CoverageAmount,
                                            report.Overview.Currency)),

                                    Pair(
                                        "Expiry",
                                        FormatDate(
                                            report.ControlPosition
                                                .Insurance
                                                .PolicyExpiryDate))
                                }));

                    row.RelativeItem()
                        .Element(x =>
                            ControlCard(
                                x,
                                "Release",
                                report.ControlPosition
                                    .Release
                                    .Status,
                                new[]
                                {
                                    Pair(
                                        "Repayment",
                                        report.ControlPosition
                                            .Release
                                            .RepaymentStatus),

                                    Pair(
                                        "Release Date",
                                        FormatDate(
                                            report.ControlPosition
                                                .Release
                                                .ReleaseDate))
                                }));
                });
        });
    }

    // ============================================================
    // DOCUMENTS
    // ============================================================

    private static void ComposeDocuments(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container.Column(column =>
        {
            SectionTitle(
                column,
                "Supporting Documents",
                "Documents retained against this collateral.");

            column.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(2.3f);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(1.2f);
                    });

                    TableHeader(
                        table,
                        "Document",
                        "File Name",
                        "Uploaded By",
                        "Uploaded");

                    if (report.Documents.Count == 0)
                    {
                        table.Cell()
                            .ColumnSpan(4)
                            .BorderBottom(1)
                            .BorderColor(BorderColor)
                            .Padding(8)
                            .Text(
                                "No supporting documents recorded.")
                            .FontSize(8)
                            .FontColor(MutedText);

                        return;
                    }

                    foreach (var document
                             in report.Documents)
                    {
                        TableCell(
                            table,
                            FormatEnum(
                                document.DocumentType));

                        TableCell(
                            table,
                            document.FileName);

                        TableCell(
                            table,
                            document.UploadedBy);

                        TableCell(
                            table,
                            FormatDate(
                                document.UploadedAtUtc));
                    }
                });
        });
    }

    // ============================================================
    // WORKFLOW HISTORY
    // ============================================================

    private static void ComposeWorkflowHistory(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container.Column(column =>
        {
            SectionTitle(
                column,
                "Workflow History",
                "Recorded activities for this collateral.");

            if (report.WorkflowHistory.Count == 0)
            {
                column.Item()
                    .PaddingTop(8)
                    .Text(
                        "No workflow history recorded.")
                    .FontColor(MutedText);

                return;
            }

            foreach (var item
                     in report.WorkflowHistory
                         .OrderBy(x => x.PerformedAtUtc))
            {
                column.Item()
                    .PaddingTop(7)
                    .BorderLeft(2)
                    .BorderColor(SecondaryColor)
                    .PaddingLeft(8)
                    .Column(c =>
                    {
                        c.Item()
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Text(
                                        FormatEnum(
                                            item.Action))
                                    .Bold()
                                    .FontSize(8);

                                row.ConstantItem(125)
                                    .AlignRight()
                                    .Text(
                                        FormatDateTime(
                                            item.PerformedAtUtc))
                                    .FontSize(7)
                                    .FontColor(MutedText);
                            });

                        c.Item()
                            .PaddingTop(2)
                            .Text(
                                $"Performed by {Display(item.PerformedBy)}")
                            .FontSize(7)
                            .FontColor(MutedText);

                        if (!string.IsNullOrWhiteSpace(
                                item.Details))
                        {
                            c.Item()
                                .PaddingTop(3)
                                .Text(item.Details!)
                                .FontSize(7.5f);
                        }
                    });
            }
        });
    }

    // ============================================================
    // PDF FOOTER
    // ============================================================

    private static void ComposePdfFooter(
        IContainer container,
        CollateralAssessmentReportVm report)
    {
        container
            .BorderTop(1)
            .BorderColor(BorderColor)
            .PaddingTop(7)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text(report.ReportReference)
                    .FontSize(7)
                    .FontColor(MutedText);

                row.RelativeItem()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ")
                            .FontSize(7);

                        text.CurrentPageNumber()
                            .FontSize(7);

                        text.Span(" of ")
                            .FontSize(7);

                        text.TotalPages()
                            .FontSize(7);
                    });

                row.RelativeItem()
                    .AlignRight()
                    .Text(
                        FormatDateTime(
                            report.GeneratedAtUtc))
                    .FontSize(7)
                    .FontColor(MutedText);
            });
    }

    // ============================================================
    // PDF REUSABLE COMPONENTS
    // ============================================================

    private static void SectionTitle(
        ColumnDescriptor column,
        string title,
        string subtitle)
    {
        column.Item()
            .Column(c =>
            {
                c.Item()
                    .Text(title)
                    .Bold()
                    .FontSize(10)
                    .FontColor(PrimaryColor);

                c.Item()
                    .PaddingTop(1)
                    .Text(subtitle)
                    .FontSize(7)
                    .FontColor(MutedText);
            });
    }

    private static void InfoCard(
        IContainer container,
        string title,
        IReadOnlyCollection<KeyValuePair<string, string>> values)
    {
        container
            .Border(1)
            .BorderColor(BorderColor)
            .Column(column =>
            {
                column.Item()
                    .Background(LightBackground)
                    .Padding(7)
                    .Text(title)
                    .Bold()
                    .FontSize(8)
                    .FontColor(PrimaryColor);

                foreach (var value in values)
                {
                    column.Item()
                        .PaddingHorizontal(7)
                        .PaddingVertical(5)
                        .BorderBottom(0.5f)
                        .BorderColor(BorderColor)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(value.Key)
                                .FontSize(7)
                                .FontColor(MutedText);

                            row.RelativeItem()
                                .AlignRight()
                                .Text(value.Value)
                                .Bold()
                                .FontSize(7);
                        });
                }
            });
    }

    private static void ControlCard(
        IContainer container,
        string title,
        string status,
        IReadOnlyCollection<KeyValuePair<string, string>> values)
    {
        container
            .Border(1)
            .BorderColor(BorderColor)
            .Column(column =>
            {
                column.Item()
                    .Background(LightBackground)
                    .Padding(7)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text(title)
                            .Bold()
                            .FontSize(8)
                            .FontColor(PrimaryColor);

                        row.RelativeItem()
                            .AlignRight()
                            .Text(
                                Display(status))
                            .Bold()
                            .FontSize(7)
                            .FontColor(
                                status is "Completed" or "Released"
                                    ? SuccessColor
                                    : SecondaryColor);
                    });

                foreach (var value in values)
                {
                    column.Item()
                        .Padding(7)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(value.Key)
                                .FontSize(7)
                                .FontColor(MutedText);

                            row.RelativeItem()
                                .AlignRight()
                                .Text(value.Value)
                                .Bold()
                                .FontSize(7);
                        });
                }
            });
    }

    private static void TableHeader(
        TableDescriptor table,
        params string[] headers)
    {
        foreach (var header in headers)
        {
            table.Cell()
                .Background(PrimaryColor)
                .Padding(6)
                .Text(header)
                .Bold()
                .FontSize(7)
                .FontColor(Colors.White);
        }
    }

    private static void TableCell(
        TableDescriptor table,
        string? value)
    {
        table.Cell()
            .BorderBottom(1)
            .BorderColor(BorderColor)
            .Padding(6)
            .Text(Display(value))
            .FontSize(7);
    }

    // ============================================================
    // WORD
    // ============================================================

    public byte[] GenerateWord(
        CollateralAssessmentReportVm report)
    {
        ArgumentNullException.ThrowIfNull(report);

        using var stream =
            new MemoryStream();

        using (
            var document =
                WordprocessingDocument.Create(
                    stream,
                    WordprocessingDocumentType.Document,
                    true))
        {
            var mainPart =
                document.AddMainDocumentPart();

            mainPart.Document =
                new WordDocument();

            var body =
                mainPart.Document.AppendChild(
                    new Body());

            ConfigureWordPage(body);

            AddWordHeader(
                body,
                report);

            AddWordIdentity(
                body,
                report);

            AddWordSectionTitle(
                body,
                "Assessment Overview");

            AddWordOverview(
                body,
                report);

            AddWordSectionTitle(
                body,
                "Collateral Lifecycle");

            AddWordLifecycle(
                body,
                report);

            AddWordSectionTitle(
                body,
                "Facility Information");

            AddWordInfoTable(
                body,
                new[]
                {
                    Pair(
                        "Loan Contract",
                        report.Facility.LoanContractId),

                    Pair(
                        "Customer ID",
                        report.Facility.CustomerId),

                    Pair(
                        "Customer Name",
                        report.Facility.CustomerName),

                    Pair(
                        "Account Number",
                        report.Facility.AccountNumber),

                    Pair(
                        "Facility Type",
                        report.Facility.FacilityType),

                    Pair(
                        "Approved Limit",
                        Money(
                            report.Facility.ApprovedLimit,
                            report.Facility.Currency)),

                    Pair(
                        "Branch",
                        report.Facility.BranchName),

                    Pair(
                        "Relationship Manager",
                        report.Facility.RelationshipManagerName),

                    Pair(
                        "Branch Manager",
                        report.Facility.BranchManagerName)
                });

            AddWordSectionTitle(
                body,
                "Collateral Information");

            AddWordInfoTable(
                body,
                new[]
                {
                    Pair(
                        "Type",
                        FormatEnum(
                            report.Collateral.Type)),

                    Pair(
                        "Region",
                        FormatEnum(
                            report.Collateral.Region)),

                    Pair(
                        "Owner",
                        report.Collateral.Owner),

                    Pair(
                        "Location",
                        report.Collateral.Address),

                    Pair(
                        "Description",
                        report.Collateral.Description)
                });

            AddWordSectionTitle(
                body,
                "Valuation Assessment");

            AddWordInfoTable(
                body,
                new[]
                {
                    Pair(
                        "Open Market Value",
                        Money(
                            report.Valuation.OpenMarketValue,
                            report.Overview.Currency)),

                    Pair(
                        "Forced Sale Value",
                        Money(
                            report.Valuation.ForcedSaleValue,
                            report.Overview.Currency)),

                    Pair(
                        "Insurance Value",
                        Money(
                            report.Valuation.InsuranceValue,
                            report.Overview.Currency)),

                    Pair(
                        "Valuation Date",
                        FormatDate(
                            report.Valuation.ValuationDate)),

                    Pair(
                        "Valuer",
                        report.Valuation.ValuerName),

                    Pair(
                        "Required Coverage",
                        Percent(
                            report.Valuation.RequiredCoveragePercent)),

                    Pair(
                        "Actual Coverage",
                        Percent(
                            report.Valuation.ActualCoveragePercent)),

                    Pair(
                        "Coverage Position",
                        report.Valuation.CoveragePosition),

                    Pair(
                        "Decision",
                        FormatEnum(
                            report.Valuation.Decision)),

                    Pair(
                        "Remark",
                        report.Valuation.Remark)
                });

            AddWordSectionTitle(
                body,
                "Collateral Control Position");

            AddWordControlPosition(
                body,
                report);

            AddWordSectionTitle(
                body,
                "Supporting Documents");

            AddWordDocuments(
                body,
                report);

            AddWordSectionTitle(
                body,
                "Workflow History");

            AddWordWorkflowHistory(
                body,
                report);

            AddWordFooterText(
                body,
                report);

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    // ============================================================
    // WORD HELPERS
    // ============================================================

    private static void ConfigureWordPage(
        Body body)
    {
        var sectionProperties =
            new SectionProperties(
                new WordPageSize
                {
                    Width = 11906,
                    Height = 16838
                },
                new PageMargin
                {
                    Top = 900,
                    Right = 900,
                    Bottom = 900,
                    Left = 900,
                    Header = 450,
                    Footer = 450
                });

        body.Append(sectionProperties);
    }

    private static void AddWordHeader(
        Body body,
        CollateralAssessmentReportVm report)
    {
        body.Append(
            WordParagraph(
                "COLLATERAL MANAGEMENT SOLUTION",
                28,
                true,
                PrimaryColor));

        body.Append(
            WordParagraph(
                "Individual Collateral Assessment Report",
                20,
                false,
                MutedText));

        body.Append(
            WordParagraph(
                $"Report Reference: {report.ReportReference}",
                17,
                true));

        body.Append(
            WordParagraph(
                $"Generated: {FormatDateTime(report.GeneratedAtUtc)}",
                16,
                false,
                MutedText));

        body.Append(
            WordDivider());
    }

    private static void AddWordIdentity(
        Body body,
        CollateralAssessmentReportVm report)
    {
        body.Append(
            WordParagraph(
                report.Header.CustomerName,
                26,
                true,
                PrimaryColor));

        body.Append(
            WordParagraph(
                $"{report.Header.LoanContractId} | " +
                $"{FormatEnum(report.Header.CollateralType)} | " +
                $"{FormatEnum(report.Header.Region)}",
                17,
                false,
                MutedText));

        body.Append(
            WordParagraph(
                $"Status: {FormatEnum(report.Header.Status)}",
                17,
                true));
    }

    private static void AddWordSectionTitle(
        Body body,
        string title)
    {
        var paragraph =
            WordParagraph(
                title,
                21,
                true,
                PrimaryColor);

        paragraph.ParagraphProperties ??=
            new ParagraphProperties();

        paragraph.ParagraphProperties.Append(
            new SpacingBetweenLines
            {
                Before = "260",
                After = "100"
            });

        body.Append(paragraph);
    }

    private static void AddWordOverview(
        Body body,
        CollateralAssessmentReportVm report)
    {
        AddWordInfoTable(
            body,
            new[]
            {
                Pair(
                    "Approved Facility",
                    Money(
                        report.Overview.ApprovedLimit,
                        report.Overview.Currency)),

                Pair(
                    "Open Market Value",
                    Money(
                        report.Overview.OpenMarketValue,
                        report.Overview.Currency)),

                Pair(
                    "Forced Sale Value",
                    Money(
                        report.Overview.ForcedSaleValue,
                        report.Overview.Currency)),

                Pair(
                    "Actual Coverage",
                    Percent(
                        report.Overview.ActualCoveragePercent)),

                Pair(
                    "Required Coverage",
                    Percent(
                        report.Overview.RequiredCoveragePercent)),

                Pair(
                    "Coverage Position",
                    report.Overview.CoveragePosition),

                Pair(
                    "Current Position",
                    FormatEnum(
                        report.Overview.CurrentPosition))
            });
    }

    private static void AddWordLifecycle(
        Body body,
        CollateralAssessmentReportVm report)
    {
        var stages =
            BuildLifecycle(
                report.Header.CurrentStage);

        var table =
            CreateWordTable();

        var row =
            new TableRow();

        foreach (var stage in stages)
        {
            row.Append(
                CreateWordCell(
                    $"{stage.Label}\n" +
                    (
                        stage.Completed
                            ? "Completed"
                            : stage.Current
                                ? "Current"
                                : "Pending"
                    ),
                    true));
        }

        table.Append(row);

        body.Append(table);
    }

    private static void AddWordControlPosition(
        Body body,
        CollateralAssessmentReportVm report)
    {
        AddWordInfoTable(
            body,
            new[]
            {
                Pair(
                    "Legal Status",
                    report.ControlPosition
                        .Legal.Status),

                Pair(
                    "Registration Reference",
                    report.ControlPosition
                        .Legal
                        .RegistrationReference),

                Pair(
                    "Legal Unit",
                    report.ControlPosition
                        .Legal
                        .LegalUnit),

                Pair(
                    "Collateral Valid",
                    BoolText(
                        report.ControlPosition
                            .Legal
                            .CollateralValid)),

                Pair(
                    "Insurance Status",
                    report.ControlPosition
                        .Insurance
                        .Status),

                Pair(
                    "Insurer",
                    report.ControlPosition
                        .Insurance
                        .Insurer),

                Pair(
                    "Policy Number",
                    report.ControlPosition
                        .Insurance
                        .PolicyNumber),

                Pair(
                    "Insurance Coverage",
                    Money(
                        report.ControlPosition
                            .Insurance
                            .CoverageAmount,
                        report.Overview.Currency)),

                Pair(
                    "Policy Expiry",
                    FormatDate(
                        report.ControlPosition
                            .Insurance
                            .PolicyExpiryDate)),

                Pair(
                    "Release Status",
                    report.ControlPosition
                        .Release
                        .Status),

                Pair(
                    "Repayment Status",
                    report.ControlPosition
                        .Release
                        .RepaymentStatus),

                Pair(
                    "Release Date",
                    FormatDate(
                        report.ControlPosition
                            .Release
                            .ReleaseDate))
            });
    }

    private static void AddWordDocuments(
        Body body,
        CollateralAssessmentReportVm report)
    {
        if (report.Documents.Count == 0)
        {
            body.Append(
                WordParagraph(
                    "No supporting documents recorded.",
                    17,
                    false,
                    MutedText));

            return;
        }

        var table =
            CreateWordTable();

        table.Append(
            CreateWordHeaderRow(
                "Document",
                "File Name",
                "Uploaded By",
                "Uploaded"));

        foreach (var document
                 in report.Documents)
        {
            var row =
                new TableRow();

            row.Append(
                CreateWordCell(
                    FormatEnum(
                        document.DocumentType)));

            row.Append(
                CreateWordCell(
                    Display(
                        document.FileName)));

            row.Append(
                CreateWordCell(
                    Display(
                        document.UploadedBy)));

            row.Append(
                CreateWordCell(
                    FormatDate(
                        document.UploadedAtUtc)));

            table.Append(row);
        }

        body.Append(table);
    }

    private static void AddWordWorkflowHistory(
        Body body,
        CollateralAssessmentReportVm report)
    {
        if (report.WorkflowHistory.Count == 0)
        {
            body.Append(
                WordParagraph(
                    "No workflow history recorded.",
                    17,
                    false,
                    MutedText));

            return;
        }

        var table =
            CreateWordTable();

        table.Append(
            CreateWordHeaderRow(
                "Activity",
                "Performed By",
                "Date / Time",
                "Details"));

        foreach (var item
                 in report.WorkflowHistory
                     .OrderBy(x => x.PerformedAtUtc))
        {
            var row =
                new TableRow();

            row.Append(
                CreateWordCell(
                    FormatEnum(
                        item.Action)));

            row.Append(
                CreateWordCell(
                    Display(
                        item.PerformedBy)));

            row.Append(
                CreateWordCell(
                    FormatDateTime(
                        item.PerformedAtUtc)));

            row.Append(
                CreateWordCell(
                    Display(
                        item.Details)));

            table.Append(row);
        }

        body.Append(table);
    }

    private static void AddWordFooterText(
        Body body,
        CollateralAssessmentReportVm report)
    {
        body.Append(
            WordDivider());

        body.Append(
            WordParagraph(
                $"{report.ReportReference} | " +
                $"Generated from the Collateral Management Solution | " +
                $"{FormatDateTime(report.GeneratedAtUtc)}",
                14,
                false,
                MutedText));
    }

    private static void AddWordInfoTable(
        Body body,
        IEnumerable<KeyValuePair<string, string>> values)
    {
        var table =
            CreateWordTable();

        foreach (var item in values)
        {
            var row =
                new TableRow();

            row.Append(
                CreateWordCell(
                    item.Key,
                    true,
                    LightBackground));

            row.Append(
                CreateWordCell(
                    item.Value));

            table.Append(row);
        }

        body.Append(table);
    }

    private static Table CreateWordTable()
    {
        var table =
            new Table();

        table.AppendChild(
            new TableProperties(
                new TableWidth
                {
                    Width = "5000",
                    Type = TableWidthUnitValues.Pct
                },
                new TableBorders(
                    new TopBorder
                    {
                        Val =
                            BorderValues.Single,
                        Size = 4,
                        Color =
                            BorderColor.Replace("#", "")
                    },
                    new BottomBorder
                    {
                        Val =
                            BorderValues.Single,
                        Size = 4,
                        Color =
                            BorderColor.Replace("#", "")
                    },
                    new LeftBorder
                    {
                        Val =
                            BorderValues.Single,
                        Size = 4,
                        Color =
                            BorderColor.Replace("#", "")
                    },
                    new RightBorder
                    {
                        Val =
                            BorderValues.Single,
                        Size = 4,
                        Color =
                            BorderColor.Replace("#", "")
                    },
                    new InsideHorizontalBorder
                    {
                        Val =
                            BorderValues.Single,
                        Size = 3,
                        Color =
                            BorderColor.Replace("#", "")
                    },
                    new InsideVerticalBorder
                    {
                        Val =
                            BorderValues.Single,
                        Size = 3,
                        Color =
                            BorderColor.Replace("#", "")
                    })));

        return table;
    }

    private static TableRow CreateWordHeaderRow(
        params string[] values)
    {
        var row =
            new TableRow();

        foreach (var value in values)
        {
            row.Append(
                CreateWordCell(
                    value,
                    true,
                    PrimaryColor,
                    "FFFFFF"));
        }

        return row;
    }

    private static TableCell CreateWordCell(
        string value,
        bool bold = false,
        string? background = null,
        string textColor = "263238")
    {
        var cell =
            new TableCell();

        var properties =
            new TableCellProperties(
                new TableCellWidth
                {
                    Type =
                        TableWidthUnitValues.Auto
                },
                new TableCellMargin(
                    new TopMargin
                    {
                        Width = "80",
                        Type = TableWidthUnitValues.Dxa
                    },
                    new BottomMargin
                    {
                        Width = "80",
                        Type = TableWidthUnitValues.Dxa
                    },
                    new LeftMargin
                    {
                        Width = "100",
                        Type = TableWidthUnitValues.Dxa
                    },
                    new RightMargin
                    {
                        Width = "100",
                        Type = TableWidthUnitValues.Dxa
                    }));

        if (!string.IsNullOrWhiteSpace(
                background))
        {
            properties.Append(
                new Shading
                {
                    Fill =
                        background.Replace(
                            "#",
                            "")
                });
        }

        cell.Append(properties);

        cell.Append(
            WordParagraph(
                Display(value),
                16,
                bold,
                textColor));

        return cell;
    }

    private static Paragraph WordParagraph(
        string text,
        int fontSize,
        bool bold = false,
        string color = "263238")
    {
        var runProperties =
            new RunProperties(
                new RunFonts
                {
                    Ascii = "Arial",
                    HighAnsi = "Arial"
                },
                new FontSize
                {
                    Val =
                        fontSize.ToString()
                },
                new WordColor
                {
                    Val =
                        color.Replace("#", "")
                });

        if (bold)
        {
            runProperties.Append(
                new Bold());
        }

        var run =
            new Run();

        run.Append(
            runProperties);

        var lines =
            text.Split('\n');

        for (var i = 0;
             i < lines.Length;
             i++)
        {
            if (i > 0)
            {
                run.Append(
                    new Break());
            }

            run.Append(
                new Text(lines[i])
                {
                    Space =
                        SpaceProcessingModeValues
                            .Preserve
                });
        }

        return new Paragraph(
            new ParagraphProperties(
                new SpacingBetweenLines
                {
                    After = "60"
                }),
            run);
    }

    private static Paragraph WordDivider()
    {
        return new Paragraph(
            new ParagraphProperties(
                new ParagraphBorders(
                    new BottomBorder
                    {
                        Val =
                            BorderValues.Single,
                        Size = 6,
                        Color =
                            BorderColor.Replace(
                                "#",
                                "")
                    }),
                new SpacingBetweenLines
                {
                    After = "100"
                }));
    }

    // ============================================================
    // SHARED FORMATTERS
    // ============================================================

    private static KeyValuePair<string, string>
        Pair(
            string label,
            object? value)
    {
        return new KeyValuePair<string, string>(
            label,
            Display(value));
    }

    private static string Display(
        object? value)
    {
        if (value is null)
        {
            return "N/A";
        }

        var text =
            Convert.ToString(
                value,
                CultureInfo.InvariantCulture);

        return string.IsNullOrWhiteSpace(text)
            ? "N/A"
            : text;
    }

    private static string Money(
        decimal? value,
        string? currency)
    {
        if (value is null)
        {
            return "N/A";
        }

        return
            $"{Display(currency)} " +
            $"{value.Value:N0}";
    }

    private static string Percent(
        decimal? value)
    {
        return value is null
            ? "N/A"
            : $"{value.Value:N2}%";
    }

    private static string FormatDate(
        DateTime? value)
    {
        return value is null
            ? "N/A"
            : value.Value.ToString(
                "dd MMM yyyy",
                CultureInfo.InvariantCulture);
    }

    private static string FormatDate(
        DateTime value)
    {
        return value.ToString(
            "dd MMM yyyy",
            CultureInfo.InvariantCulture);
    }

    private static string FormatDateTime(
        DateTime value)
    {
        return value.ToString(
            "dd MMM yyyy HH:mm 'UTC'",
            CultureInfo.InvariantCulture);
    }

    private static string FormatEnum(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "N/A";
        }

        var result =
            System.Text.RegularExpressions.Regex
                .Replace(
                    value,
                    "([a-z])([A-Z])",
                    "$1 $2");

        return result.Replace(
            "_",
            " ");
    }

    private static string BoolText(
        bool? value)
    {
        return value switch
        {
            true => "Yes",
            false => "No",
            _ => "N/A"
        };
    }

    private static string CoverageColor(
        string? position)
    {
        return position?.Equals(
                   "Adequate",
                   StringComparison.OrdinalIgnoreCase)
               == true
            ? SuccessColor
            : position?.Equals(
                  "Below Requirement",
                  StringComparison.OrdinalIgnoreCase)
              == true
                ? DangerColor
                : SecondaryColor;
    }

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private static IReadOnlyList<LifecycleItem>
        BuildLifecycle(
            string currentStage)
    {
        var stages =
            new[]
            {
                new
                {
                    Key = "CollateralData",
                    Label = "Verification"
                },
                new
                {
                    Key = "Valuation",
                    Label = "Valuation"
                },
                new
                {
                    Key = "Perfection",
                    Label = "Legal Perfection"
                },
                new
                {
                    Key = "Insurance",
                    Label = "Insurance"
                },
                new
                {
                    Key = "Release",
                    Label = "Release"
                }
            };

        if (currentStage.Equals(
                "Archived",
                StringComparison.OrdinalIgnoreCase))
        {
            return stages
                .Select(x =>
                    new LifecycleItem(
                        x.Label,
                        true,
                        false))
                .ToList();
        }

        if (currentStage.Equals(
                "SystemHold",
                StringComparison.OrdinalIgnoreCase))
        {
            return stages
                .Select(x =>
                    new LifecycleItem(
                        x.Label,
                        x.Key != "Release",
                        false))
                .ToList();
        }

        var index =
            Array.FindIndex(
                stages,
                x => x.Key.Equals(
                    currentStage,
                    StringComparison.OrdinalIgnoreCase));

        return stages
            .Select(
                (stage, stageIndex) =>
                    new LifecycleItem(
                        stage.Label,
                        index >= 0 &&
                        stageIndex < index,
                        index >= 0 &&
                        stageIndex == index))
            .ToList();
    }

    private sealed record LifecycleItem(
        string Label,
        bool Completed,
        bool Current);
}