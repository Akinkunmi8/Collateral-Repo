export interface InsuranceQueueItem {
    id: string;
    loanContractId: string;
    customerName: string;
    type: string;
    currentStage: string;
    status: string;

    stageStartedAtUtc: string;
    createdAtUtc: string;

    tatStatus: string | null;
    tatElapsedHours: number | null;
    tatAllowedHours: number | null;
    tatDueAtUtc: string | null;
}

export interface InsuranceQueueResponse {
    items: InsuranceQueueItem[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export interface CollateralData {
    loanContractId: string;
    accountNumber: string;
    cifId: string;
    customerName: string;
    branchName: string | null;
    facilityType: string | null;
    currency: string;
    collateralType: string;
    collateralDescription: string;
    region: string;
    approvedLimit: number;
    collateralAddress: string | null;
    collateralOwner: string | null;
    relationshipManagerName: string | null;
    branchManagerName: string | null;
}

export interface ValuationData {
    manualEntry: boolean;
    openMarketValue: number | null;
    forcedSaleValue: number | null;
    valuersName: string | null;
    valuationDate: string | null;
    actualCoverageRatioPercent: number;
    requiredCoverageRatioPercent: number;
    coverageAdequate: boolean;
    decision: string;
    remark: string;
}

export interface PerfectionData {
    registrationReference: string;
    legalUnit: string;
    stage: string;
    queryNotes: string | null;
    collateralValid: boolean;
    invalidReason: string | null;
}

export interface InsuranceData {
    policyNumber: string;
    insurer: string;
    coverageAmount: number;
    premiumAmount: number;
    policyStartDate: string;
    policyExpiryDate: string;
    renewalCadence: string;
    renewalReminderSent: boolean;
    overdueNoticeSent: boolean;
}

export interface InsuranceWorkItem {
    id: string;
    currentStage: string;
    status: string;
    collateralData: CollateralData;
    valuation: ValuationData | null;
    perfection: PerfectionData | null;
    insurance: InsuranceData | null;
    release: unknown | null;
}

export interface CollateralDocument {
    id: string;
    fileName: string;
    documentType: number;
    contentType: string;
    uploadedBy: string;
    createdAtUtc: string;
}

export enum CollateralDocumentType {
    ValuationReport = 1,
    LegalReport = 2,
    InsuranceReport = 3,
    Other = 4
}

export enum InsuranceRenewalCadence {
    ThreeYearly = 1,
    Annual = 2
}

export interface SubmitInsuranceRequest {
    policyNumber: string;
    insurer: string;
    coverageAmount: number;
    premiumAmount: number;
    policyStartDate: string;
    renewalCadence: InsuranceRenewalCadence;
}