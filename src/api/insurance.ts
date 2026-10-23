import { api } from "./client";

import type {
    CollateralDocument,
    InsuranceQueueResponse,
    InsuranceWorkItem,
    SubmitInsuranceRequest
} from "../types/insurance";

export interface GetInsuranceQueueParams {
    search?: string;
    type?: string;
    tat?: string;
    pageNumber?: number;
    pageSize?: number;
}

export async function getInsuranceQueue(
    params: GetInsuranceQueueParams
): Promise<InsuranceQueueResponse> {

    const response = await api.get<InsuranceQueueResponse>(
        "/dashboards/insurance/queue",
        { params }
    );

    return response.data;
}

export async function getInsuranceWorkItem(
    collateralId: string
): Promise<InsuranceWorkItem> {

    const response = await api.get<InsuranceWorkItem>(
        `/dashboards/insurance/${collateralId}`
    );

    return response.data;
}

export async function getInsuranceDocuments(
    collateralId: string
): Promise<CollateralDocument[]> {

    const response = await api.get<CollateralDocument[]>(
        `/collaterals/${collateralId}/documents`
    );

    return response.data;
}

export async function uploadInsuranceReport(
    collateralId: string,
    file: File
): Promise<void> {

    const formData = new FormData();

    // InsuranceReport = 3
    formData.append("documentType", "3");
    formData.append("file", file);

    await api.post(
        `/collaterals/${collateralId}/documents`,
        formData
    );
}

export async function downloadInsuranceReport(
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

export async function completeInsurance(
    collateralId: string,
    request: SubmitInsuranceRequest
): Promise<void> {

    await api.post(
        `/collaterals/${collateralId}/insurance`,
        request
    );
}