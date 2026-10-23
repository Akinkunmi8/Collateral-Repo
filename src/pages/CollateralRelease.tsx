import {
    ArrowLeft,
    Archive,
    Building2,
    CheckCircle2,
    Download,
    FileText,
    Gavel,
    Landmark,
    Loader2,
    LockKeyhole,
    ShieldCheck,
    TriangleAlert,
    X
} from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { api, auth } from "../api/client";
import "./collateral-release.css";

type CollateralData = {
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
};

type Valuation = {
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
};

type Perfection = {
    registrationReference: string;
    legalUnit: string;
    stage: string;
    queryNotes: string | null;
    collateralValid: boolean;
    invalidReason: string | null;
};

type Insurance = {
    policyNumber: string;
    insurer: string;
    coverageAmount: number;
    premiumAmount: number;
    policyStartDate: string;
    policyExpiryDate: string;
    renewalCadence: string;
    renewalReminderSent: boolean;
    overdueNoticeSent: boolean;
};

type Release = {
    loanFullyRepaid: boolean;
    releaseDate: string;
    releasedBy: string;
    notes: string | null;
};

type WorkItem = {
    id: string;
    currentStage: string;
    status: string;
    collateralData: CollateralData;
    valuation: Valuation | null;
    perfection: Perfection | null;
    insurance: Insurance | null;
    release: Release | null;
};

type RepaymentStatus = {
    collateralId: string;
    loanContractId: string;
    outstandingBalance: number;
    nextRepaymentDate: string;
    fullyRepaid: boolean;
    branch: string;
    region: string;
};

type DocumentItem = {
    id: string;
    documentType: string;
    fileName: string;
    contentType?: string;
};

export default function CollateralRelease() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [workItem, setWorkItem] = useState<WorkItem | null>(null);
    const [repayment, setRepayment] =
        useState<RepaymentStatus | null>(null);
    const [documents, setDocuments] = useState<DocumentItem[]>([]);

    const [releasedBy, setReleasedBy] = useState("");
    const [notes, setNotes] = useState("");

    const [loading, setLoading] = useState(true);
    const [checkingRepayment, setCheckingRepayment] = useState(false);
    const [submitting, setSubmitting] = useState(false);

    const [error, setError] = useState("");
    const [showConfirm, setShowConfirm] = useState(false);

    useEffect(() => {
        const user = auth.get();

        if (user?.fullName) {
            setReleasedBy(user.fullName);
        }
    }, []);

    useEffect(() => {
        if (!id) return;

        loadPage();
    }, [id]);

    async function loadPage() {
        if (!id) return;

        try {
            setLoading(true);
            setError("");

            const workResponse =
                await api.get<WorkItem>(
                    `/dashboards/release/${id}`
                );

            const work = workResponse.data;

            setWorkItem(work);

            const documentResponse =
                await api.get<DocumentItem[]>(
                    `/collaterals/${id}/documents`
                );

            setDocuments(
                documentResponse.data ?? []
            );

            /*
             * Live Core Banking repayment lookup is only
             * required while collateral is on System Hold.
             *
             * Archived records use the persisted Release
             * information instead.
             */
            if (
                work.currentStage === "SystemHold" &&
                work.status === "OnHold"
            ) {
                const repaymentResponse =
                    await api.get<RepaymentStatus>(
                        `/collaterals/${id}/release/repayment-status`
                    );

                setRepayment(
                    repaymentResponse.data
                );
            } else {
                setRepayment(null);
            }
        } catch (err: any) {
            console.error(err);

            setError(
                err?.response?.data?.detail ||
                err?.response?.data?.message ||
                "Unable to load collateral release information."
            );
        } finally {
            setLoading(false);
        }
    }

    async function refreshRepaymentStatus() {
        if (!id) return;

        try {
            setCheckingRepayment(true);
            setError("");

            const response =
                await api.get<RepaymentStatus>(
                    `/collaterals/${id}/release/repayment-status`
                );

            setRepayment(response.data);
        } catch (err: any) {
            setError(
                err?.response?.data?.detail ||
                err?.response?.data?.message ||
                "Unable to check loan repayment status."
            );
        } finally {
            setCheckingRepayment(false);
        }
    }

    async function downloadDocument(document: DocumentItem) {
        if (!id) return;

        try {
            const response = await api.get(
                `/collaterals/${id}/documents/${document.id}/download`,
                {
                    responseType: "blob"
                }
            );

            const url =
                window.URL.createObjectURL(
                    new Blob([response.data])
                );

            const anchor =
                window.document.createElement("a");

            anchor.href = url;
            anchor.download = document.fileName;

            window.document.body.appendChild(anchor);

            anchor.click();
            anchor.remove();

            window.URL.revokeObjectURL(url);
        } catch (err) {
            console.error(err);
            setError("Unable to download the selected report.");
        }
    }

    async function releaseCollateral() {
        if (!id || !repayment?.fullyRepaid) return;

        try {
            setSubmitting(true);
            setError("");

            await api.post(
                `/collaterals/${id}/release`,
                {
                    releasedBy: releasedBy.trim(),
                    notes: notes.trim() || null
                }
            );

            setShowConfirm(false);

            navigate("/archive", {
                replace: true
            });
        } catch (err: any) {
            console.error(err);

            setShowConfirm(false);

            setError(
                err?.response?.data?.detail ||
                err?.response?.data?.message ||
                "Unable to release the collateral."
            );

            /*
             * The backend re-checks Core Banking during POST.
             * Refresh the displayed status in case repayment
             * position changed after this page was opened.
             */
            await refreshRepaymentStatus();
        } finally {
            setSubmitting(false);
        }
    }

    function money(
        value: number | null | undefined,
        currency = "NGN"
    ) {
        if (value === null || value === undefined) {
            return "—";
        }

        return new Intl.NumberFormat("en-NG", {
            style: "currency",
            currency,
            maximumFractionDigits: 2
        }).format(value);
    }

    function date(value?: string | null) {
        if (!value) return "—";

        return new Intl.DateTimeFormat("en-NG", {
            day: "2-digit",
            month: "short",
            year: "numeric"
        }).format(new Date(value));
    }

    const documentsByType = useMemo(() => {
        const find = (type: string) =>
            documents.find(
                x =>
                    String(x.documentType).toLowerCase() ===
                    type.toLowerCase()
            );

        return {
            valuation: find("ValuationReport"),
            legal: find("LegalReport"),
            insurance: find("InsuranceReport")
        };
    }, [documents]);

    if (loading) {
        return (
            <div className="release-loading">
                <Loader2 className="release-spin" size={28} />
                Loading collateral release...
            </div>
        );
    }

    if (!workItem) {
        return (
            <div className="release-error-page">
                <TriangleAlert size={30} />
                <h2>Unable to open collateral</h2>
                <p>
                    {error ||
                        "The collateral release record could not be loaded."}
                </p>

                <button onClick={() => navigate("/archive")}>
                    Return to Archive & Release
                </button>
            </div>
        );
    }

    const collateral = workItem.collateralData;
    const valuation = workItem.valuation;
    const perfection = workItem.perfection;
    const insurance = workItem.insurance;
    const release = workItem.release;

    const isArchived =
        workItem.currentStage === "Archived" &&
        workItem.status === "Archived";

    const canRelease =
        repayment?.fullyRepaid === true &&
        releasedBy.trim().length > 0 &&
        !submitting;

    return (
        <div className="release-page">
            <button
                className="release-back"
                onClick={() => navigate("/archive")}
            >
                <ArrowLeft size={17} />
                Archive & Release
            </button>

            <div className="release-page-header">
                <div>
                    <h1>
                        {isArchived
                            ? "Archived Collateral Record"
                            : "Collateral Release"}
                    </h1>

                    <p>
                        {collateral.loanContractId}
                        <span>•</span>
                        {collateral.customerName}
                    </p>
                </div>

                <div className="release-current-status">
                    <span>Current Stage</span>

                    <strong>
                        {isArchived
                            ? "ARCHIVED"
                            : workItem.currentStage === "SystemHold"
                                ? "SYSTEM HOLD"
                                : workItem.currentStage}
                    </strong>
                </div>
            </div>

            {error && (
                <div className="release-alert-error">
                    <TriangleAlert size={18} />
                    <span>{error}</span>
                </div>
            )}

            <Section
                number="01"
                icon={<Building2 size={19} />}
                title="Collateral Details"
                subtitle="Facility and collateral information"
            >
                <InfoGrid>
                    <Info
                        label="Loan Contract"
                        value={collateral.loanContractId}
                    />

                    <Info
                        label="Account Number"
                        value={collateral.accountNumber}
                    />

                    <Info
                        label="CIF"
                        value={collateral.cifId}
                    />

                    <Info
                        label="Customer"
                        value={collateral.customerName}
                    />

                    <Info
                        label="Branch"
                        value={collateral.branchName}
                    />

                    <Info
                        label="Facility Type"
                        value={collateral.facilityType}
                    />

                    <Info
                        label="Approved Limit"
                        value={money(
                            collateral.approvedLimit,
                            collateral.currency
                        )}
                    />

                    <Info
                        label="Collateral Type"
                        value={collateral.collateralType}
                    />

                    <Info
                        label="Region"
                        value={collateral.region}
                    />

                    <Info
                        label="Collateral Owner"
                        value={collateral.collateralOwner}
                    />

                    <Info
                        label="Relationship Manager"
                        value={collateral.relationshipManagerName}
                    />

                    <Info
                        label="Branch Manager"
                        value={collateral.branchManagerName}
                    />
                </InfoGrid>

                <InfoWide
                    label="Collateral Description"
                    value={collateral.collateralDescription}
                />

                <InfoWide
                    label="Collateral Address"
                    value={collateral.collateralAddress}
                />
            </Section>

            <Section
                number="02"
                icon={<Landmark size={19} />}
                title="Valuation"
                subtitle="Approved collateral valuation"
            >
                {valuation ? (
                    <>
                        <InfoGrid>
                            <Info
                                label="Open Market Value"
                                value={money(
                                    valuation.openMarketValue,
                                    collateral.currency
                                )}
                            />

                            <Info
                                label="Forced Sale Value"
                                value={money(
                                    valuation.forcedSaleValue,
                                    collateral.currency
                                )}
                            />

                            <Info
                                label="Valuer"
                                value={valuation.valuersName}
                            />

                            <Info
                                label="Valuation Date"
                                value={date(
                                    valuation.valuationDate
                                )}
                            />

                            <Info
                                label="Actual Coverage"
                                value={`${valuation.actualCoverageRatioPercent.toFixed(
                                    2
                                )}%`}
                            />

                            <Info
                                label="Required Coverage"
                                value={`${valuation.requiredCoverageRatioPercent.toFixed(
                                    2
                                )}%`}
                            />
                        </InfoGrid>

                        <div className="release-inline-row">
                            <span
                                className={
                                    valuation.coverageAdequate
                                        ? "release-good-badge"
                                        : "release-warning-badge"
                                }
                            >
                                {valuation.coverageAdequate
                                    ? "Coverage Adequate"
                                    : "Coverage Below Requirement"}
                            </span>

                            {documentsByType.valuation && (
                                <DocumentButton
                                    document={
                                        documentsByType.valuation
                                    }
                                    onClick={downloadDocument}
                                />
                            )}
                        </div>
                    </>
                ) : (
                    <EmptyStage text="No valuation information available." />
                )}
            </Section>

            <Section
                number="03"
                icon={<Gavel size={19} />}
                title="Legal Perfection"
                subtitle="Legal registration and collateral validation"
            >
                {perfection ? (
                    <>
                        <InfoGrid>
                            <Info
                                label="Registration Reference"
                                value={
                                    perfection.registrationReference
                                }
                            />

                            <Info
                                label="Legal Unit"
                                value={perfection.legalUnit}
                            />

                            <Info
                                label="Perfection Status"
                                value={perfection.stage}
                            />

                            <Info
                                label="Collateral Valid"
                                value={
                                    perfection.collateralValid
                                        ? "Yes"
                                        : "No"
                                }
                            />
                        </InfoGrid>

                        {perfection.invalidReason && (
                            <InfoWide
                                label="Invalid Reason"
                                value={perfection.invalidReason}
                            />
                        )}

                        {documentsByType.legal && (
                            <div className="release-document-row">
                                <DocumentButton
                                    document={documentsByType.legal}
                                    onClick={downloadDocument}
                                />
                            </div>
                        )}
                    </>
                ) : (
                    <EmptyStage text="No legal perfection information available." />
                )}
            </Section>

            <Section
                number="04"
                icon={<ShieldCheck size={19} />}
                title="Insurance"
                subtitle="Collateral insurance protection"
            >
                {insurance ? (
                    <>
                        <InfoGrid>
                            <Info
                                label="Policy Number"
                                value={insurance.policyNumber}
                            />

                            <Info
                                label="Insurer"
                                value={insurance.insurer}
                            />

                            <Info
                                label="Coverage Amount"
                                value={money(
                                    insurance.coverageAmount,
                                    collateral.currency
                                )}
                            />

                            <Info
                                label="Premium Amount"
                                value={money(
                                    insurance.premiumAmount,
                                    collateral.currency
                                )}
                            />

                            <Info
                                label="Policy Start"
                                value={date(
                                    insurance.policyStartDate
                                )}
                            />

                            <Info
                                label="Policy Expiry"
                                value={date(
                                    insurance.policyExpiryDate
                                )}
                            />

                            <Info
                                label="Renewal Cadence"
                                value={insurance.renewalCadence}
                            />
                        </InfoGrid>

                        {documentsByType.insurance && (
                            <div className="release-document-row">
                                <DocumentButton
                                    document={
                                        documentsByType.insurance
                                    }
                                    onClick={downloadDocument}
                                />
                            </div>
                        )}
                    </>
                ) : (
                    <EmptyStage text="No insurance information available." />
                )}
            </Section>

            <Section
                number="05"
                icon={
                    isArchived
                        ? <Archive size={19} />
                        : <LockKeyhole size={19} />
                }
                title={
                    isArchived
                        ? "Release Information"
                        : "Loan Repayment & Release"
                }
                subtitle={
                    isArchived
                        ? "Final collateral release and archival record"
                        : "Final repayment verification before collateral release"
                }
            >
                {isArchived ? (
                    release ? (
                        <>
                            <div className="release-archived-banner">
                                <CheckCircle2 size={24} />

                                <div>
                                    <strong>
                                        Collateral Released & Archived
                                    </strong>

                                    <span>
                                        This collateral completed the release
                                        workflow and is retained as a read-only
                                        historical record.
                                    </span>
                                </div>
                            </div>

                            <InfoGrid>
                                <Info
                                    label="Loan Fully Repaid"
                                    value={
                                        release.loanFullyRepaid
                                            ? "Yes"
                                            : "No"
                                    }
                                />

                                <Info
                                    label="Released By"
                                    value={release.releasedBy}
                                />

                                <Info
                                    label="Release Date"
                                    value={date(release.releaseDate)}
                                />

                                <Info
                                    label="Final Status"
                                    value="Archived"
                                />
                            </InfoGrid>

                            <InfoWide
                                label="Release Notes"
                                value={release.notes}
                            />
                        </>
                    ) : (
                        <EmptyStage text="Release information is not available for this archived collateral." />
                    )
                ) : (
                    <>
                        {repayment ? (
                            <div
                                className={`repayment-card ${repayment.fullyRepaid
                                        ? "repayment-card-success"
                                        : "repayment-card-pending"
                                    }`}
                            >
                                <div className="repayment-card-heading">
                                    {repayment.fullyRepaid ? (
                                        <CheckCircle2 size={24} />
                                    ) : (
                                        <TriangleAlert size={24} />
                                    )}

                                    <div>
                                        <strong>
                                            {repayment.fullyRepaid
                                                ? "Full Loan Repayment Confirmed"
                                                : "Loan Not Fully Repaid"}
                                        </strong>

                                        <span>
                                            Live status received from Core Banking
                                        </span>
                                    </div>

                                    <button
                                        type="button"
                                        onClick={refreshRepaymentStatus}
                                        disabled={checkingRepayment}
                                    >
                                        {checkingRepayment
                                            ? "Checking..."
                                            : "Refresh Status"}
                                    </button>
                                </div>

                                <div className="repayment-values">
                                    <Info
                                        label="Outstanding Balance"
                                        value={money(
                                            repayment.outstandingBalance,
                                            collateral.currency
                                        )}
                                    />

                                    <Info
                                        label="Loan Contract"
                                        value={repayment.loanContractId}
                                    />

                                    <Info
                                        label="Branch"
                                        value={repayment.branch}
                                    />

                                    <Info
                                        label="Region"
                                        value={repayment.region}
                                    />

                                    <Info
                                        label="Next Repayment Date"
                                        value={date(
                                            repayment.nextRepaymentDate
                                        )}
                                    />

                                    <Info
                                        label="Repayment Status"
                                        value={
                                            repayment.fullyRepaid
                                                ? "Fully Repaid"
                                                : "Outstanding"
                                        }
                                    />
                                </div>
                            </div>
                        ) : (
                            <div className="release-repayment-unavailable">
                                Unable to obtain repayment status from
                                Core Banking.
                            </div>
                        )}

                        <div className="release-form">
                            <div className="release-field">
                                <label>Released By</label>

                                <input
                                    value={releasedBy}
                                    onChange={(e) =>
                                        setReleasedBy(e.target.value)
                                    }
                                    placeholder="Name of releasing officer"
                                />

                                <small>
                                    Currently populated from the
                                    authenticated user.
                                </small>
                            </div>

                            <div className="release-field">
                                <label>Release Notes</label>

                                <textarea
                                    value={notes}
                                    onChange={(e) =>
                                        setNotes(e.target.value)
                                    }
                                    maxLength={1000}
                                    rows={4}
                                    placeholder="Enter optional release notes..."
                                />

                                <small>
                                    {notes.length}/1000 characters
                                </small>
                            </div>
                        </div>
                    </>
                )}
            </Section>

            <div className="release-sticky-action">
                <div>
                    <strong>Final Collateral Release</strong>

                    <span>
                        {repayment?.fullyRepaid
                            ? "Core Banking confirms that the facility is fully repaid."
                            : "Release is unavailable until the facility is fully repaid."}
                    </span>
                </div>

                <button
                    type="button"
                    disabled={!canRelease}
                    onClick={() => setShowConfirm(true)}
                >
                    <Archive size={18} />
                    Release & Archive
                </button>
            </div>

            {!isArchived && showConfirm && (
                <div
                    className="release-modal-backdrop"
                    onMouseDown={() => {
                        if (!submitting) {
                            setShowConfirm(false);
                        }
                    }}
                >
                    <div
                        className="release-modal"
                        onMouseDown={(e) =>
                            e.stopPropagation()
                        }
                    >
                        <button
                            className="release-modal-close"
                            disabled={submitting}
                            onClick={() =>
                                setShowConfirm(false)
                            }
                        >
                            <X size={18} />
                        </button>

                        <div className="release-modal-icon">
                            <Archive size={25} />
                        </div>

                        <h2>Release & Archive Collateral?</h2>

                        <p>
                            You are about to release collateral for{" "}
                            <strong>
                                {collateral.customerName}
                            </strong>{" "}
                            under loan{" "}
                            <strong>
                                {collateral.loanContractId}
                            </strong>
                            .
                        </p>

                        <div className="release-modal-confirmation">
                            <CheckCircle2 size={18} />

                            <span>
                                Core Banking currently confirms a
                                zero outstanding balance and full
                                repayment.
                            </span>
                        </div>

                        <p className="release-modal-warning">
                            After confirmation, this collateral will
                            be marked Released and immediately moved
                            to the Archive.
                        </p>

                        <div className="release-modal-actions">
                            <button
                                type="button"
                                className="release-modal-cancel"
                                disabled={submitting}
                                onClick={() =>
                                    setShowConfirm(false)
                                }
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                className="release-modal-confirm"
                                disabled={submitting}
                                onClick={releaseCollateral}
                            >
                                {submitting ? (
                                    <>
                                        <Loader2
                                            size={17}
                                            className="release-spin"
                                        />
                                        Releasing...
                                    </>
                                ) : (
                                    <>
                                        <Archive size={17} />
                                        Confirm Release
                                    </>
                                )}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

function Section({
    number,
    icon,
    title,
    subtitle,
    children
}: {
    number: string;
    icon: React.ReactNode;
    title: string;
    subtitle: string;
    children: React.ReactNode;
}) {
    return (
        <section className="release-section">
            <div className="release-section-header">
                <span className="release-section-number">
                    {number}
                </span>

                <span className="release-section-icon">
                    {icon}
                </span>

                <div>
                    <h2>{title}</h2>
                    <p>{subtitle}</p>
                </div>
            </div>

            <div className="release-section-body">
                {children}
            </div>
        </section>
    );
}

function InfoGrid({
    children
}: {
    children: React.ReactNode;
}) {
    return (
        <div className="release-info-grid">
            {children}
        </div>
    );
}

function Info({
    label,
    value
}: {
    label: string;
    value?: React.ReactNode | null;
}) {
    return (
        <div className="release-info">
            <span>{label}</span>
            <strong>{value || "—"}</strong>
        </div>
    );
}

function InfoWide({
    label,
    value
}: {
    label: string;
    value?: React.ReactNode | null;
}) {
    return (
        <div className="release-info-wide">
            <span>{label}</span>
            <strong>{value || "—"}</strong>
        </div>
    );
}

function EmptyStage({
    text
}: {
    text: string;
}) {
    return (
        <div className="release-empty-stage">
            {text}
        </div>
    );
}

function DocumentButton({
    document,
    onClick
}: {
    document: DocumentItem;
    onClick: (document: DocumentItem) => void;
}) {
    return (
        <button
            type="button"
            className="release-document-btn"
            onClick={() => onClick(document)}
        >
            <FileText size={17} />

            <span>
                <strong>{document.fileName}</strong>
                <small>{document.documentType}</small>
            </span>

            <Download size={16} />
        </button>
    );
}