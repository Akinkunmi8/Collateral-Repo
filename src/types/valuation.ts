export interface ValuationQueueItem {
    id: string;
    loanContractId: string;
    customerId: string;
    customerName: string;
    accountNumber: string;
    type: string;
    region: string;
    description: string;
    approvedLimit: number;
    currency: string;
    facilityType?: string | null;
    collateralAddress?: string | null;
    collateralOwner?: string | null;
    relationshipManagerName?: string | null;
    branchManagerName?: string | null;
    currentStage: string;
    status: string;
    stageStartedAtUtc: string;
    createdAtUtc: string;
    tatStatus?: string | null;
    tatElapsedHours?: number | null;
    tatAllowedHours?: number | null;
    tatDueAtUtc?: string | null;
}

export interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export interface ValuationReport {
    collateralId: string;
    loanContractId: string;
    accountNumber: string;
    customerName: string;
    approvedLimit: number;
    currency: string;

    facilityRef?: string | null;
    facilityAmount: number;
    status?: string | null;
    operativeAcc?: string | null;

    state?: string | null;
    localGov?: string | null;
    propertyDescription?: string | null;
    landsize?: string | null;

    valuationDate?: string | null;
    openMarketValue?: number | null;
    forceSalesValue?: number | null;
    insuranceValue?: number | null;

    salesLocation?: string | null;
    salesDate?: string | null;

    valuationReportName?: string | null;
    valuersName?: string | null;
    dateAssigned?: string | null;

    valuationReportBase64?: string | null;
    contentType?: string | null;

    actualCoverageRatioPercent: number;
    requiredCoverageRatioPercent: number;
    coverageAdequate: boolean;
}

export interface SubmitValuationRequest {
    manualEntry: boolean;
    manualOpenMarketValue: number | null;
    manualForcedSaleValue: number | null;
    manualValuersName: string | null;
    manualValuationDate: string | null;
    reportBase64: string | null;
    reportContentType: string | null;
    decision: 1 | 2;
    remark: string;
}

export interface SubmitValuationResult {
    coverageAdequate: boolean;
    actualCoverageRatioPercent: number;
    requiredCoverageRatioPercent: number;
    decision: number;
}

export interface ResubmitValuationRequest {
    response: string;
}
