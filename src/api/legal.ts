import { api } from "./client";

import type {
    CollateralDocument,
    LegalQueueResponse,
    LegalWorkItem,
    SubmitPerfectionRequest
} from "../types/legal";

export interface GetLegalQueueParams {
    search?: string;
    type?: string;
    tat?: string;
    pageNumber?: number;
    pageSize?: number;
}

export async function getLegalQueue(
    params: GetLegalQueueParams
): Promise<LegalQueueResponse> {

    const response = await api.get<LegalQueueResponse>(
        "/dashboards/legal/queue",
        { params }
    );

    return response.data;
}

export async function getLegalWorkItem(
    collateralId: string
): Promise<LegalWorkItem> {

    const response = await api.get<LegalWorkItem>(
        `/dashboards/legal/${collateralId}`
    );

    return response.data;
}

export async function getCollateralDocuments(
    collateralId: string
): Promise<CollateralDocument[]> {
    const response = await api.get<CollateralDocument[]>(
        `/collaterals/${collateralId}/documents`
    );

    return response.data;
}

export async function uploadLegalReport(
    collateralId: string,
    file: File
): Promise<void> {
    const formData = new FormData();

    // LegalReport = 2
    formData.append("documentType", "2");
    formData.append("file", file);

    // Do not manually set multipart Content-Type.
    // Axios/browser will add the correct boundary.
    await api.post(
        `/collaterals/${collateralId}/documents`,
        formData
    );
}

export async function downloadLegalReport(
    collateralId: string,
    documentId: string
): Promise<Blob> {
    const response = await api.get(
        `/collaterals/${collateralId}/documents/${documentId}/download`,
        {
            responseType: "blob"
        }
    );

    return response.data;
}

export async function completeLegalPerfection(
    collateralId: string,
    request: SubmitPerfectionRequest
): Promise<void> {
    await api.post(
        `/collaterals/${collateralId}/perfection`,
        request
    );
}