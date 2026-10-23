export interface CollateralAssessmentReport {
    collateralId: string;
    reportReference: string;
    generatedAtUtc: string;
    header: ReportHeader;
    overview: AssessmentOverview;
    facility: FacilityReport;
    collateral: CollateralReport;
    valuation: ValuationAssessment;
    controlPosition: CollateralControlPosition;
    documents: ReportDocument[];
    workflowHistory: WorkflowHistory[];
}

export interface ReportHeader {
    loanContractId: string;
    customerId: string;
    customerName: string;
    accountNumber: string;
    collateralType: string;
    region: string;
    currentStage: string;
    status: string;
}

export interface AssessmentOverview {
    approvedLimit: number;
    currency: string;
    openMarketValue: number | null;
    forcedSaleValue: number | null;
    requiredCoveragePercent: number | null;
    actualCoveragePercent: number | null;
    coveragePosition: string;
    currentPosition: string;
}

export interface FacilityReport {
    loanContractId: string;
    customerId: string;
    customerName: string;
    accountNumber: string;
    facilityType: string | null;
    approvedLimit: number;
    currency: string;
    relationshipManagerName: string | null;
    branchManagerName: string | null;
    branchName: string | null;
}

export interface CollateralReport {
    type: string;
    region: string;
    description: string;
    owner: string | null;
    address: string | null;
}

export interface ValuationAssessment {
    completed: boolean;
    status: string;
    valuationDate: string | null;
    valuerName: string | null;
    openMarketValue: number | null;
    forcedSaleValue: number | null;
    insuranceValue: number | null;
    requiredCoveragePercent: number | null;
    actualCoveragePercent: number | null;
    coveragePosition: string;
    decision: string | null;
    remark: string | null;
}

export interface CollateralControlPosition {
    legal: LegalAssessment;
    insurance: InsuranceAssessment;
    release: ReleaseAssessment;
}

export interface LegalAssessment {
    completed: boolean;
    status: string;
    registrationReference: string | null;
    legalUnit: string | null;
    collateralValid: boolean | null;
    invalidReason: string | null;
}

export interface InsuranceAssessment {
    completed: boolean;
    status: string;
    policyNumber: string | null;
    insurer: string | null;
    coverageAmount: number | null;
    premiumAmount: number | null;
    policyStartDate: string | null;
    policyExpiryDate: string | null;
    renewalCadence: string | null;
    daysToExpiry: number | null;
}

export interface ReleaseAssessment {
    completed: boolean;
    status: string;
    repaymentStatus: string | null;
    releaseDate: string | null;
}

export interface ReportDocument {
    id: string;
    documentType: string;
    fileName: string;
    uploadedBy: string | null;
    uploadedAtUtc: string;
    fileSize: number | null;
}

export interface WorkflowHistory {
    action: string;
    performedBy: string;
    performedAtUtc: string;
    details: string | null;
}