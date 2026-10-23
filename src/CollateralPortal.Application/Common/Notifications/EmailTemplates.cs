using System.Globalization;
using System.Text;

namespace CollateralPortal.Application.Common.Notifications;

/// <summary>
/// Email subject/body templates transcribed from the BRD. All notifications
/// are event-driven — each is built at the moment its triggering event
/// happens and handed to IMessagePublisher, never polled/batched.
/// </summary>
public static class EmailTemplates
{
    private static string Fmt(decimal? value) => value.HasValue ? value.Value.ToString("N2", CultureInfo.InvariantCulture) : "N/A";
    private static string Fmt(DateTime? value) => value.HasValue ? value.Value.ToString("dd MMM yyyy") : "N/A";
    private static string Or(string? value) => string.IsNullOrWhiteSpace(value) ? "N/A" : value;

    /// <summary>UC-001 — sent when a collateral record is created or updated, to RM/BM/AM/RH/SG Head.</summary>
    public static EmailContent CollateralQueueNotification(string customerName, CollateralQueueNotificationData d)
    {
        var subject = $"Notification: Collateral Queue Update ({customerName})";

        var sb = new StringBuilder();
        sb.AppendLine($"This is to notify you of the current collateral record for {customerName}.");
        sb.AppendLine();
        AppendRow(sb, "Facility type", d.FacilityType);
        AppendRow(sb, "Collateral type", d.CollateralType);
        AppendRow(sb, "Collateral description", d.CollateralDescription);
        AppendRow(sb, "Collateral currency", d.Currency);
        AppendRow(sb, "Total collateral value", Fmt(d.TotalCollateralValue));
        AppendRow(sb, "Collateral address", Or(d.CollateralAddress));
        AppendRow(sb, "Collateral location state", d.CollateralLocationState);
        AppendRow(sb, "Collateral information account number", d.AccountNumber);
        AppendRow(sb, "Contract ID", d.ContractId);
        AppendRow(sb, "Loan start date", Fmt(d.LoanStartDate));
        AppendRow(sb, "Loan maturity date", Fmt(d.LoanMaturityDate));
        AppendRow(sb, "Principal amount", Fmt(d.PrincipalAmount));
        AppendRow(sb, "Exposure LCY", Fmt(d.ExposureLcy));
        AppendRow(sb, "Valuer name", Or(d.ValuerName));
        AppendRow(sb, "Valuation start date", Fmt(d.ValuationStartDate));
        AppendRow(sb, "Valuation end date", Fmt(d.ValuationEndDate));
        AppendRow(sb, "Legal status", d.LegalStatus);
        AppendRow(sb, "SOL, region", d.Region);
        AppendRow(sb, "Insurance name", Or(d.InsuranceName));
        AppendRow(sb, "Premium amount", Fmt(d.PremiumAmount));
        AppendRow(sb, "Sum insured amount", Fmt(d.SumInsuredAmount));
        AppendRow(sb, "Insurance start date", Fmt(d.InsuranceStartDate));
        AppendRow(sb, "Insurance end date", Fmt(d.InsuranceEndDate));
        AppendRow(sb, "Asset type", d.AssetType);
        AppendRow(sb, "Valuation", d.ValuationStatus);
        AppendRow(sb, "Ownership", Or(d.CollateralOwner));
        AppendRow(sb, "RM", Or(d.RelationshipManagerName));
        AppendRow(sb, "BM", Or(d.BranchManagerName));
        AppendRow(sb, "Next repayment date", Fmt(d.NextRepaymentDate));
        AppendRow(sb, "Loan exposure (total outstanding)", Fmt(d.LoanExposure));
        sb.AppendLine();
        sb.AppendLine("Customer data:");
        AppendRow(sb, "Borrower name", d.BorrowerName);
        AppendRow(sb, "Account number", d.AccountNumber);
        AppendRow(sb, "Branch name", d.BranchName);
        AppendRow(sb, "CIF ID", d.CifId);
        AppendRow(sb, "Historical credit performance (risk rating)", Or(d.RiskRating));

        return new EmailContent(subject, sb.ToString());
    }

    /// <summary>UC-002 (Submit path) — sent once a valuation report has been uploaded.</summary>
    public static EmailContent ValuationReportUploaded(
        string customerName, string accountNumber, string? collateralOwner, string? address,
        decimal approvedLimit, decimal? forcedSaleValue, decimal? openMarketValue)
    {
        var subject = $"Notification: Collateral Valuation Report Uploaded {customerName}";
        var body = new StringBuilder()
            .AppendLine("Dear Recipient,")
            .AppendLine()
            .AppendLine($"This is to inform you that a collateral valuation report for {customerName} has been successfully uploaded to the Collateral Valuation Portal.")
            .AppendLine($"Please log in to the portal to review — the valuation report for the property is ready. The FSV is {Fmt(forcedSaleValue)} while the OMV is {Fmt(openMarketValue)}.")
            .AppendLine()
            .AppendLine($"Customer Name: {customerName}")
            .AppendLine($"Account Number: {accountNumber}")
            .AppendLine($"Collateral Owner: {Or(collateralOwner)}")
            .AppendLine($"Address: {Or(address)}")
            .AppendLine($"Facility Amount: {Fmt(approvedLimit)}")
            .ToString();

        return new EmailContent(subject, body);
    }

    /// <summary>UC-002 (Request additional collateral path).</summary>
    public static EmailContent AdditionalCollateralRequested(string customerName, string accountNumber, decimal actualCoverage, decimal requiredCoverage, string remark)
    {
        var subject = $"Notification: Additional Collateral Requested ({customerName})";
        var body = $"Collateral coverage for {customerName} (account {accountNumber}) is {actualCoverage:N2}%, below the required {requiredCoverage:N2}%. " +
                   $"Additional collateral has been requested. Remark: {remark}";
        return new EmailContent(subject, body);
    }

    /// <summary>UC-003 (Legal queue) — collateral record missing/invalid for the pledged property.</summary>
    public static EmailContent CollateralNotFoundOrInvalid(string customerName, string collateralDescription, string collateralAddress, string collateralLocation, string facilityType)
    {
        var subject = $"Notification: Collateral Not Found & Not valid ({customerName})";
        var body = $"To inform you that no collateral record exists, or the collateral is not valid, for the pledged property for {customerName} " +
                   $"— collateral description: {collateralDescription}, collateral address: {collateralAddress}, collateral location: {collateralLocation}, facility type: {facilityType}.";
        return new EmailContent(subject, body);
    }

    /// <summary>UC-003 (Legal queue) — normal perfection status update.</summary>
    public static EmailContent CollateralPerfectionUpdate(string customerName, string collateralAddress, string collateralLocation, string collateralDescription)
    {
        var subject = $"Notification: Collateral perfection update ({customerName})";
        var body = $"To inform you that the current collateral perfection status for the pledged property for {customerName} " +
                   $"— collateral address: {collateralAddress}, collateral location: {collateralLocation}, collateral description: {collateralDescription} — has been updated.";
        return new EmailContent(subject, body);
    }

    /// <summary>UC-004 (Insurance queue) — one month before renewal date.</summary>
    public static EmailContent InsuranceRenewalNotice(string customerName, int daysLeft)
    {
        var subject = $"Notification: Insurance renewal notice ({customerName})";
        var body = $"Let it come to your notice that there are {daysLeft} days left for the expiry of the insurance policy for this pledged collateral " +
                   $"({customerName}). Please inform the customer immediately and kindly initiate the process of renewal immediately, then update your records on the portal.";
        return new EmailContent(subject, body);
    }

    /// <summary>UC-004 (Insurance queue) — renewal date has elapsed without renewal.</summary>
    public static EmailContent InsuranceRenewalOverdue(string customerName, DateTime originalPolicyDate, DateTime expiryDate)
    {
        var subject = $"Notification: renewal of expired insurance ({customerName})";
        var body = $"Be informed that the insurance cover for the subject {customerName} pledged property, which was done ({Fmt(originalPolicyDate)}), " +
                   $"has now expired as of ({Fmt(expiryDate)}), and it requires insurance renewal. Additionally, insurance cover for pledged property " +
                   "should be renewed annually in accordance with the Bank's policy.";
        return new EmailContent(subject, body);
    }

    /// <summary>Generic status-change notice — used for System Hold / Release / Archive and any other status change not covered by a specific template above.</summary>
    public static EmailContent StatusChange(string customerName, string accountNumber, string newStatus, string? details = null)
    {
        var subject = $"Notification: Collateral status change ({customerName})";
        var body = $"The collateral status for {customerName} (account {accountNumber}) has changed to {newStatus}." +
                   (string.IsNullOrWhiteSpace(details) ? string.Empty : $" {details}");
        return new EmailContent(subject, body);
    }

    private static void AppendRow(StringBuilder sb, string label, string value) => sb.AppendLine($"{label}: {value}");
}
