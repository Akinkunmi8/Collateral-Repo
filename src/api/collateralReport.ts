import { api } from "./client";
import type {
    CollateralAssessmentReport
} from "../types/collateralReport";

export async function getCollateralAssessmentReport(
    collateralId: string
): Promise<CollateralAssessmentReport> {
    const response =
        await api.get<CollateralAssessmentReport>(
            `/collaterals/${collateralId}/report`
        );

    return response.data;
}