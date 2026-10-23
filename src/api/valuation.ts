import { api } from "./client";

import type {
    PagedResult,
    ResubmitValuationRequest,
    SubmitValuationRequest,
    SubmitValuationResult,
    ValuationQueueItem,
    ValuationReport,
} from "../types/valuation";

export interface GetValuationQueueParams {
    search?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
}

export async function getValuationQueue(
    params: GetValuationQueueParams
): Promise<PagedResult<ValuationQueueItem>> {
    const response = await api.get<PagedResult<ValuationQueueItem>>(
        "/collaterals/valuation",
        {
            params,
        }
    );

    return response.data;
}

export async function getValuation(
    collateralId: string
): Promise<ValuationReport> {
    const response = await api.get<ValuationReport>(
        `/collaterals/${collateralId}/valuation`
    );

    return response.data;
}

export async function submitValuation(
    collateralId: string,
    request: SubmitValuationRequest
): Promise<SubmitValuationResult> {
    const response = await api.post<SubmitValuationResult>(
        `/collaterals/${collateralId}/valuation`,
        request
    );

    return response.data;
}

export async function resubmitValuation(
    collateralId: string,
    request: ResubmitValuationRequest
): Promise<void> {
    await api.post(
        `/collaterals/${collateralId}/valuation/resubmit`,
        request
    );
}