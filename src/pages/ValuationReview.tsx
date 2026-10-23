import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
    getValuation,
    submitValuation,
} from "../api/valuation";

import type {
    SubmitValuationRequest,
    ValuationReport,
} from "../types/valuation";

import "./valuationReview.css";

type DecisionType = "approve" | "request-info" | null;

export default function ValuationReview() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [valuation, setValuation] =
        useState<ValuationReport | null>(null);

    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);

    const [error, setError] = useState("");
    const [successMessage, setSuccessMessage] = useState("");

    const [remark, setRemark] = useState("");
    const [decision, setDecision] = useState<DecisionType>(null);

    useEffect(() => {
        const loadValuation = async () => {
            if (!id) {
                setError("Collateral reference was not supplied.");
                setLoading(false);
                return;
            }

            try {
                setLoading(true);
                setError("");

                const result = await getValuation(id);

                setValuation(result);
            } catch (err) {
                console.error(err);

                setError(
                    "Unable to load the valuation report. Please try again."
                );
            } finally {
                setLoading(false);
            }
        };

        void loadValuation();
    }, [id]);

    const formatCurrency = (
        value?: number | null,
        currency = "NGN"
    ) => {
        if (value === null || value === undefined) {
            return "—";
        }

        return new Intl.NumberFormat("en-NG", {
            style: "currency",
            currency,
            maximumFractionDigits: 0,
        }).format(value);
    };

    const formatDate = (value?: string | null) => {
        if (!value) return "—";

        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return "—";
        }

        return new Intl.DateTimeFormat("en-NG", {
            day: "2-digit",
            month: "short",
            year: "numeric",
        }).format(date);
    };

    const coverageDifference = useMemo(() => {
        if (!valuation) return 0;

        return (
            valuation.actualCoverageRatioPercent -
            valuation.requiredCoverageRatioPercent
        );
    }, [valuation]);

    const openConfirmation = (
        selectedDecision: Exclude<DecisionType, null>
    ) => {
        setError("");
        setSuccessMessage("");

        if (!remark.trim()) {
            setError(
                selectedDecision === "approve"
                    ? "Please enter an approval remark before continuing."
                    : "Please state the additional valuation information required."
            );

            return;
        }

        setDecision(selectedDecision);
    };

    const closeConfirmation = () => {
        if (submitting) return;

        setDecision(null);
    };

    const confirmDecision = async () => {
        if (!id || !valuation || !decision) return;

        try {
            setSubmitting(true);
            setError("");

            const request: SubmitValuationRequest = {
                manualEntry: false,
                manualOpenMarketValue: null,
                manualForcedSaleValue: null,
                manualValuersName: null,
                manualValuationDate: null,
                reportBase64: null,
                reportContentType: null,
                decision: decision === "approve" ? 1 : 2,
                remark: remark.trim(),
            };

            await submitValuation(id, request);

            if (decision === "approve") {
                setSuccessMessage(
                    "Valuation approved successfully. The collateral has moved to Perfection."
                );
            } else {
                setSuccessMessage(
                    "Additional valuation information has been requested successfully."
                );
            }

            setDecision(null);

            window.setTimeout(() => {
                navigate("/valuation");
            }, 1200);
        } catch (err) {
            console.error(err);

            setDecision(null);

            setError(
                "The valuation decision could not be completed. Please review the information and try again."
            );
        } finally {
            setSubmitting(false);
        }
    };

    const openReport = () => {
        if (
            !valuation?.valuationReportBase64 ||
            !valuation.contentType
        ) {
            setError("No valuation report is available.");
            return;
        }

        try {
            const base64 = valuation.valuationReportBase64.replace(
                /^data:.*;base64,/,
                ""
            );

            const binary = window.atob(base64);

            const bytes = new Uint8Array(binary.length);

            for (let i = 0; i < binary.length; i++) {
                bytes[i] = binary.charCodeAt(i);
            }

            const blob = new Blob([bytes], {
                type: valuation.contentType,
            });

            const url = URL.createObjectURL(blob);

            window.open(url, "_blank", "noopener,noreferrer");

            window.setTimeout(() => {
                URL.revokeObjectURL(url);
            }, 60000);
        } catch (err) {
            console.error(err);

            setError(
                "The valuation report could not be opened."
            );
        }
    };

    if (loading) {
        return (
            <div className="valuation-review-page">
                <div className="valuation-review-loading">
                    Loading valuation report...
                </div>
            </div>
        );
    }

    if (!valuation) {
        return (
            <div className="valuation-review-page">
                <button
                    type="button"
                    className="back-button"
                    onClick={() => navigate("/valuation")}
                >
                    <span aria-hidden="true">&larr;</span>
                    <span>Back to Valuation Queue</span>
                </button>

                <div className="valuation-review-error">
                    {error || "Valuation report was not found."}
                </div>
            </div>
        );
    }

    return (
        <div className="valuation-review-page">
            <div className="valuation-review-topbar">
                <button
                    type="button"
                    className="back-button"
                    onClick={() => navigate("/valuation")}
                >
                    ← Back to Valuation Queue
                </button>

                <div className="valuation-reference">
                    <span>Loan Contract</span>
                    <strong>{valuation.loanContractId}</strong>
                </div>
            </div>

            <div className="valuation-review-header">
                <div>
                    <p className="valuation-eyebrow">
                        Collateral Management / Valuation
                    </p>

                    <h1>Valuation Review</h1>

                    <p>
                        Review the valuation information received from
                        the Valuation Management Portal before making
                        a decision.
                    </p>
                </div>

                <span className="valuation-status">
                    Awaiting Valuation Decision
                </span>
            </div>

            {error && (
                <div className="valuation-message valuation-message-error">
                    {error}
                </div>
            )}

            {successMessage && (
                <div className="valuation-message valuation-message-success">
                    {successMessage}
                </div>
            )}

            <div className="valuation-review-grid">
                <main className="valuation-review-main">
                    <section className="review-section">
                        <div className="review-section-heading">
                            <div>
                                <span className="section-number">01</span>
                                <div>
                                    <h2>Customer & Facility</h2>
                                    <p>
                                        Facility information associated
                                        with this collateral.
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className="detail-grid">
                            <DetailItem
                                label="Customer Name"
                                value={valuation.customerName}
                            />

                            <DetailItem
                                label="Account Number"
                                value={valuation.accountNumber}
                            />

                            <DetailItem
                                label="Loan Contract"
                                value={valuation.loanContractId}
                            />

                            <DetailItem
                                label="Facility Reference"
                                value={valuation.facilityRef}
                            />

                            <DetailItem
                                label="Approved Limit"
                                value={formatCurrency(
                                    valuation.approvedLimit,
                                    valuation.currency
                                )}
                                highlight
                            />

                            <DetailItem
                                label="Valuation Facility Amount"
                                value={formatCurrency(
                                    valuation.facilityAmount,
                                    valuation.currency
                                )}
                            />

                            <DetailItem
                                label="Operative Account"
                                value={valuation.operativeAcc}
                            />

                            <DetailItem
                                label="Report Status"
                                value={valuation.status}
                            />
                        </div>
                    </section>

                    <section className="review-section">
                        <div className="review-section-heading">
                            <div>
                                <span className="section-number">02</span>

                                <div>
                                    <h2>Property Information</h2>
                                    <p>
                                        Details of the asset supplied by
                                        the valuation portal.
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className="detail-grid">
                            <DetailItem
                                label="State"
                                value={valuation.state}
                            />

                            <DetailItem
                                label="Local Government"
                                value={valuation.localGov}
                            />

                            <DetailItem
                                label="Land Size"
                                value={valuation.landsize}
                            />

                            <DetailItem
                                label="Sales Location"
                                value={valuation.salesLocation}
                            />

                            <DetailItem
                                label="Sales Date"
                                value={formatDate(
                                    valuation.salesDate
                                )}
                            />

                            <DetailItem
                                label="Property Description"
                                value={
                                    valuation.propertyDescription
                                }
                                wide
                            />
                        </div>
                    </section>

                    <section className="review-section">
                        <div className="review-section-heading">
                            <div>
                                <span className="section-number">03</span>

                                <div>
                                    <h2>Valuation Details</h2>
                                    <p>
                                        Values returned by the external
                                        Valuation Management Portal.
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className="valuation-values">
                            <div className="valuation-value-card">
                                <span>Open Market Value</span>
                                <strong>
                                    {formatCurrency(
                                        valuation.openMarketValue,
                                        valuation.currency
                                    )}
                                </strong>
                            </div>

                            <div className="valuation-value-card">
                                <span>Forced Sale Value</span>
                                <strong>
                                    {formatCurrency(
                                        valuation.forceSalesValue,
                                        valuation.currency
                                    )}
                                </strong>
                            </div>

                            <div className="valuation-value-card">
                                <span>Insurance Value</span>
                                <strong>
                                    {formatCurrency(
                                        valuation.insuranceValue,
                                        valuation.currency
                                    )}
                                </strong>
                            </div>
                        </div>

                        <div className="detail-grid valuation-meta">
                            <DetailItem
                                label="Valuer"
                                value={valuation.valuersName}
                            />

                            <DetailItem
                                label="Date Assigned"
                                value={formatDate(
                                    valuation.dateAssigned
                                )}
                            />

                            <DetailItem
                                label="Valuation Date"
                                value={formatDate(
                                    valuation.valuationDate
                                )}
                            />

                            <DetailItem
                                label="Report Name"
                                value={
                                    valuation.valuationReportName
                                }
                            />
                        </div>
                    </section>

                    <section className="review-section">
                        <div className="review-section-heading report-heading">
                            <div>
                                <span className="section-number">04</span>

                                <div>
                                    <h2>Valuation Report</h2>
                                    <p>
                                        Supporting report received from
                                        the valuation system.
                                    </p>
                                </div>
                            </div>

                            <button
                                type="button"
                                className="report-button"
                                disabled={
                                    !valuation.valuationReportBase64
                                }
                                onClick={openReport}
                            >
                                View Report
                            </button>
                        </div>

                        <div className="report-file">
                            <div className="report-icon">PDF</div>

                            <div>
                                <strong>
                                    {valuation.valuationReportName ||
                                        "Valuation Report"}
                                </strong>

                                <span>
                                    {valuation.contentType ||
                                        "No report attached"}
                                </span>
                            </div>
                        </div>
                    </section>
                </main>

                <aside className="valuation-review-sidebar">
                    <section className="coverage-card">
                        <div className="coverage-heading">
                            <div>
                                <span>Coverage Analysis</span>
                                <h2>
                                    {
                                        valuation.actualCoverageRatioPercent
                                    }
                                    %
                                </h2>
                            </div>

                            <span
                                className={
                                    valuation.coverageAdequate
                                        ? "coverage-indicator adequate"
                                        : "coverage-indicator inadequate"
                                }
                            >
                                {valuation.coverageAdequate
                                    ? "Adequate"
                                    : "Below Requirement"}
                            </span>
                        </div>

                        <div className="coverage-progress">
                            <div
                                className="coverage-progress-value"
                                style={{
                                    width: `${Math.min(
                                        Math.max(
                                            (valuation.actualCoverageRatioPercent /
                                                Math.max(
                                                    valuation.requiredCoverageRatioPercent,
                                                    1
                                                )) *
                                            100,
                                            0
                                        ),
                                        100
                                    )}%`,
                                }}
                            />
                        </div>

                        <div className="coverage-comparison">
                            <div>
                                <span>Actual Coverage</span>
                                <strong>
                                    {
                                        valuation.actualCoverageRatioPercent
                                    }
                                    %
                                </strong>
                            </div>

                            <div>
                                <span>Required Coverage</span>
                                <strong>
                                    {
                                        valuation.requiredCoverageRatioPercent
                                    }
                                    %
                                </strong>
                            </div>
                        </div>

                        <div
                            className={
                                coverageDifference >= 0
                                    ? "coverage-difference positive"
                                    : "coverage-difference negative"
                            }
                        >
                            {coverageDifference >= 0 ? "+" : ""}
                            {coverageDifference.toFixed(1)} percentage
                            points against requirement
                        </div>

                        <p className="coverage-note">
                            Coverage is an assessment indicator. The
                            final valuation decision remains with the
                            reviewing officer.
                        </p>
                    </section>

                    <section className="decision-card">
                        <div className="decision-card-heading">
                            <h2>Valuation Decision</h2>
                            <p>
                                Enter your review remark before making
                                a decision.
                            </p>
                        </div>

                        <label
                            className="remark-label"
                            htmlFor="valuation-remark"
                        >
                            Review Remark
                        </label>

                        <textarea
                            id="valuation-remark"
                            value={remark}
                            onChange={(e) =>
                                setRemark(e.target.value)
                            }
                            rows={6}
                            maxLength={1000}
                            placeholder="Enter your valuation review remark..."
                        />

                        <div className="remark-counter">
                            {remark.length}/1000
                        </div>

                        <div className="decision-actions">
                            <button
                                type="button"
                                className="request-info-button"
                                disabled={submitting}
                                onClick={() =>
                                    openConfirmation(
                                        "request-info"
                                    )
                                }
                            >
                                Request Additional Information
                            </button>

                            <button
                                type="button"
                                className="approve-button"
                                disabled={submitting}
                                onClick={() =>
                                    openConfirmation("approve")
                                }
                            >
                                Approve Valuation
                            </button>
                        </div>
                    </section>
                </aside>
            </div>

            {decision && (
                <div
                    className="valuation-modal-backdrop"
                    role="presentation"
                    onMouseDown={(e) => {
                        if (e.target === e.currentTarget) {
                            closeConfirmation();
                        }
                    }}
                >
                    <div
                        className="valuation-modal"
                        role="dialog"
                        aria-modal="true"
                        aria-labelledby="valuation-confirm-title"
                    >
                        <div
                            className={
                                decision === "approve"
                                    ? "modal-icon modal-icon-approve"
                                    : "modal-icon modal-icon-request"
                            }
                        >
                            {decision === "approve" ? "✓" : "!"}
                        </div>

                        <h2 id="valuation-confirm-title">
                            {decision === "approve"
                                ? "Approve Valuation?"
                                : "Request Additional Information?"}
                        </h2>

                        <p>
                            {decision === "approve"
                                ? "The approved valuation will be stored and this collateral will move to the Perfection stage."
                                : "This collateral will remain in the Valuation stage until the requested information is updated and resubmitted."}
                        </p>

                        <div className="confirmation-remark">
                            <span>Remark</span>
                            <p>{remark}</p>
                        </div>

                        <div className="modal-actions">
                            <button
                                type="button"
                                className="modal-cancel"
                                disabled={submitting}
                                onClick={closeConfirmation}
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                disabled={submitting}
                                className={
                                    decision === "approve"
                                        ? "modal-confirm approve"
                                        : "modal-confirm request"
                                }
                                onClick={confirmDecision}
                            >
                                {submitting
                                    ? "Processing..."
                                    : decision === "approve"
                                        ? "Confirm Approval"
                                        : "Confirm Request"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

interface DetailItemProps {
    label: string;
    value?: string | number | null;
    highlight?: boolean;
    wide?: boolean;
}

function DetailItem({
    label,
    value,
    highlight = false,
    wide = false,
}: DetailItemProps) {
    const displayValue =
        value === null ||
            value === undefined ||
            value === ""
            ? "—"
            : value;

    return (
        <div
            className={`detail-item ${wide ? "detail-item-wide" : ""
                }`}
        >
            <span>{label}</span>

            <strong
                className={
                    highlight ? "detail-highlight" : ""
                }
            >
                {displayValue}
            </strong>
        </div>
    );
}