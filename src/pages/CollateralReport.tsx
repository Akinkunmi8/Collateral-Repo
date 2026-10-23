import {
    ArrowLeft,
    CheckCircle2,
    Circle,
    Download,
    Eye,
    FileText,
    Loader2,
    ShieldCheck
} from "lucide-react";

import {
    useEffect,
    useState
} from "react";

import {
    useNavigate,
    useParams
} from "react-router-dom";

import { api } from "../api/client";

import {
    getCollateralAssessmentReport
} from "../api/collateralReport";

import type {
    CollateralAssessmentReport
} from "../types/collateralReport";

import {
    downloadReport
} from "../utils/downloadReport";

import "./collateral-report.css";

export default function CollateralReport() {
    const { id } = useParams();
    const navigate = useNavigate();

    const [report, setReport] =
        useState<CollateralAssessmentReport | null>(null);

    const [loading, setLoading] =
        useState(true);

    const [error, setError] =
        useState("");

    const [downloading, setDownloading] =
        useState<"pdf" | "word" | null>(null);

    useEffect(() => {
        if (!id) {
            setError(
                "Collateral ID was not supplied."
            );

            setLoading(false);
            return;
        }

        void loadReport(id);
    }, [id]);

    async function loadReport(
        collateralId: string
    ) {
        try {
            setLoading(true);
            setError("");

            const data =
                await getCollateralAssessmentReport(
                    collateralId
                );

            setReport(data);
        } catch (err) {
            console.error(
                "Unable to load collateral report",
                err
            );

            setError(
                "Unable to load the collateral assessment report."
            );
        } finally {
            setLoading(false);
        }
    }

    async function handleDownload(
        type: "pdf" | "word"
    ) {
        if (!report || downloading) {
            return;
        }

        try {
            setDownloading(type);

            const extension =
                type === "pdf"
                    ? "pdf"
                    : "docx";

            await downloadReport(
                `/collaterals/${report.collateralId}/report/${type}`,
                `Collateral-Assessment-${report.header.loanContractId}.${extension}`
            );
        } catch (err) {
            console.error(
                `Unable to download ${type} report`,
                err
            );
        } finally {
            setDownloading(null);
        }
    }

    async function handleViewDocument(
        documentId: string
    ) {
        if (!report) {
            return;
        }

        try {
            const response =
                await api.get<Blob>(
                    `/collaterals/${report.collateralId}/documents/${documentId}/download`,
                    {
                        responseType: "blob"
                    }
                );

            const rawContentType =
                response.headers[
                "content-type"
                ];

            const contentType =
                typeof rawContentType === "string"
                    ? rawContentType
                    : "application/octet-stream";

            const blob = new Blob(
                [response.data],
                {
                    type: contentType
                }
            );

            const objectUrl =
                window.URL.createObjectURL(
                    blob
                );

            window.open(
                objectUrl,
                "_blank",
                "noopener,noreferrer"
            );

            window.setTimeout(
                () =>
                    window.URL.revokeObjectURL(
                        objectUrl
                    ),
                60_000
            );
        } catch (err) {
            console.error(
                "Unable to open supporting document",
                err
            );
        }
    }

    if (loading) {
        return (
            <div className="carState">
                <Loader2
                    size={28}
                    className="carSpin"
                />
                <span>
                    Preparing collateral assessment...
                </span>
            </div>
        );
    }

    if (error || !report) {
        return (
            <div className="carState">
                <strong>
                    Unable to display report
                </strong>

                <span>
                    {error || "Report not found."}
                </span>

                <button
                    className="secondary"
                    onClick={() => navigate(-1)}
                >
                    Go Back
                </button>
            </div>
        );
    }

    const {
        header,
        overview,
        facility,
        collateral,
        valuation,
        controlPosition,
        documents,
        workflowHistory
    } = report;

    const lifecycle = getLifecycle(
        header.currentStage
    );

    return (
        <div className="carPage">

            {/* Screen toolbar */}

            <div className="carToolbar">
                <div>
                    <button
                        type="button"
                        className="carBack"
                        onClick={() => navigate(-1)}
                    >
                        <ArrowLeft size={16} />
                        Back
                    </button>

                    <h1>
                        Individual Collateral
                        Assessment Report
                    </h1>

                    <p>
                        Complete assessment and control
                        position of the selected collateral.
                    </p>
                </div>

                <div className="carToolbarActions">
                    <button
                        type="button"
                        className="secondary"
                        disabled={downloading !== null}
                        onClick={() =>
                            handleDownload("word")
                        }
                    >
                        {downloading === "word" ? (
                            <Loader2
                                size={16}
                                className="carSpin"
                            />
                        ) : (
                            <FileText size={16} />
                        )}

                        {downloading === "word"
                            ? "Preparing Word..."
                            : "Download Word"}
                    </button>

                    <button
                        type="button"
                        className="primary"
                        disabled={downloading !== null}
                        onClick={() =>
                            handleDownload("pdf")
                        }
                    >
                        {downloading === "pdf" ? (
                            <Loader2
                                size={16}
                                className="carSpin"
                            />
                        ) : (
                            <Download size={16} />
                        )}

                        {downloading === "pdf"
                            ? "Preparing PDF..."
                            : "Download PDF"}
                    </button>
                </div>
            </div>

            {/* Actual report */}

            <article className="carDocument">

                <header className="carReportHeader">
                    <div className="carBrand">
                        <div className="carBrandMark">
                            <ShieldCheck size={25} />
                        </div>

                        <div>
                            <strong>
                                COLLATERAL MANAGEMENT
                                SOLUTION
                            </strong>

                            <span>
                                Individual Collateral
                                Assessment Report
                            </span>
                        </div>
                    </div>

                    <div className="carReportMeta">
                        <div>
                            <span>Report Reference</span>
                            <strong>
                                {report.reportReference}
                            </strong>
                        </div>

                        <div>
                            <span>Generated</span>
                            <strong>
                                {formatDateTime(
                                    report.generatedAtUtc
                                )}
                            </strong>
                        </div>
                    </div>
                </header>

                {/* Customer identity */}

                <section className="carIdentity">
                    <div>
                        <span className="carEyebrow">
                            CUSTOMER / COLLATERAL
                        </span>

                        <h2>
                            {header.customerName}
                        </h2>

                        <p>
                            {header.loanContractId}
                            <span className="carIdentityDot" />
                            {formatEnum(header.collateralType)}
                            <span className="carIdentityDot" />
                            {formatEnum(header.region)}
                        </p>
                    </div>

                    <StatusPill
                        value={formatEnum(
                            header.status
                        )}
                        kind={
                            header.status === "Archived"
                                ? "neutral"
                                : "blue"
                        }
                    />
                </section>

                {/* Overview */}

                <ReportSection
                    title="Assessment Overview"
                    subtitle="Current financial and workflow position."
                >
                    <div className="carOverviewGrid">

                        <Metric
                            label="Approved Facility"
                            value={money(
                                overview.approvedLimit,
                                overview.currency
                            )}
                            note={facility.facilityType ?? "Facility"}
                        />

                        <Metric
                            label="Open Market Value"
                            value={money(
                                overview.openMarketValue,
                                overview.currency
                            )}
                            note="Latest approved valuation"
                        />

                        <Metric
                            label="Forced Sale Value"
                            value={money(
                                overview.forcedSaleValue,
                                overview.currency
                            )}
                            note="Realisation value"
                        />

                        <Metric
                            label="Coverage"
                            value={
                                overview.actualCoveragePercent != null
                                    ? `${overview.actualCoveragePercent.toFixed(2)}%`
                                    : "—"
                            }
                            note={
                                overview.requiredCoveragePercent != null
                                    ? `Required ${overview.requiredCoveragePercent}%`
                                    : "Not assessed"
                            }
                            state={
                                overview.coveragePosition ===
                                    "Adequate"
                                    ? "good"
                                    : overview.coveragePosition ===
                                        "Below Requirement"
                                        ? "bad"
                                        : undefined
                            }
                        />
                    </div>

                    <div className="carPositionBar">
                        <span>Current Position</span>

                        <strong>
                            {formatEnum(
                                overview.currentPosition
                            )}
                        </strong>

                        <StatusPill
                            value={
                                overview.coveragePosition
                            }
                            kind={
                                overview.coveragePosition ===
                                    "Adequate"
                                    ? "green"
                                    : overview.coveragePosition ===
                                        "Below Requirement"
                                        ? "red"
                                        : "neutral"
                            }
                        />
                    </div>
                </ReportSection>

                {/* Lifecycle */}

                <ReportSection
                    title="Collateral Lifecycle"
                    subtitle="Progress through the collateral control process."
                >
                    <div className="carLifecycle">
                        {lifecycle.map(
                            (item, index) => (
                                <div
                                    className={`carLifeItem ${item.state}`}
                                    key={item.label}
                                >
                                    <div className="carLifeTop">
                                        <div className="carLifeIcon">
                                            {item.state ===
                                                "completed" ? (
                                                <CheckCircle2
                                                    size={18}
                                                />
                                            ) : (
                                                <Circle
                                                    size={18}
                                                />
                                            )}
                                        </div>

                                        {index <
                                            lifecycle.length -
                                            1 && (
                                                <div className="carLifeLine" />
                                            )}
                                    </div>

                                    <strong>
                                        {item.label}
                                    </strong>

                                    <span>
                                        {item.state ===
                                            "completed"
                                            ? "Completed"
                                            : item.state ===
                                                "current"
                                                ? "Current"
                                                : "Pending"}
                                    </span>
                                </div>
                            )
                        )}
                    </div>
                </ReportSection>

                {/* Facility / collateral */}

                <ReportSection
                    title="Facility & Collateral"
                    subtitle="Facility exposure and security information."
                >
                    <div className="carTwoColumns">
                        <InfoCard title="Facility Information">
                            <Info
                                label="Loan Contract"
                                value={
                                    facility.loanContractId
                                }
                            />
                            <Info
                                label="Customer ID"
                                value={
                                    facility.customerId
                                }
                            />
                            <Info
                                label="Account Number"
                                value={
                                    facility.accountNumber
                                }
                            />
                            <Info
                                label="Facility Type"
                                value={
                                    facility.facilityType
                                }
                            />
                            <Info
                                label="Approved Limit"
                                value={money(
                                    facility.approvedLimit,
                                    facility.currency
                                )}
                            />
                            <Info
                                label="Branch"
                                value={
                                    facility.branchName
                                }
                            />
                            <Info
                                label="Relationship Manager"
                                value={
                                    facility.relationshipManagerName
                                }
                            />
                            <Info
                                label="Branch Manager"
                                value={
                                    facility.branchManagerName
                                }
                            />
                        </InfoCard>

                        <InfoCard title="Collateral Information">
                            <Info
                                label="Type"
                                value={formatEnum(
                                    collateral.type
                                )}
                            />
                            <Info
                                label="Owner"
                                value={collateral.owner}
                            />
                            <Info
                                label="Region"
                                value={formatEnum(
                                    collateral.region
                                )}
                            />
                            <Info
                                label="Location"
                                value={collateral.address}
                            />
                        </InfoCard>
                    </div>

                    <div className="carDescription">
                        <span>
                            COLLATERAL DESCRIPTION
                        </span>
                        <p>
                            {collateral.description ||
                                "No description provided."}
                        </p>
                    </div>
                </ReportSection>

                {/* Valuation */}

                <ReportSection
                    title="Valuation Assessment"
                    subtitle="Approved valuation and collateral coverage position."
                >
                    <div className="carValuationGrid">
                        <Info
                            label="Open Market Value"
                            value={money(
                                valuation.openMarketValue,
                                overview.currency
                            )}
                        />

                        <Info
                            label="Forced Sale Value"
                            value={money(
                                valuation.forcedSaleValue,
                                overview.currency
                            )}
                        />

                        <Info
                            label="Insurance Value"
                            value={money(
                                valuation.insuranceValue,
                                overview.currency
                            )}
                        />

                        <Info
                            label="Valuation Date"
                            value={formatDate(
                                valuation.valuationDate
                            )}
                        />

                        <Info
                            label="Valuer"
                            value={valuation.valuerName}
                        />

                        <Info
                            label="Decision"
                            value={formatEnum(
                                valuation.decision
                            )}
                        />

                        <Info
                            label="Required Coverage"
                            value={
                                valuation.requiredCoveragePercent !=
                                    null
                                    ? `${valuation.requiredCoveragePercent}%`
                                    : "—"
                            }
                        />

                        <Info
                            label="Actual Coverage"
                            value={
                                valuation.actualCoveragePercent !=
                                    null
                                    ? `${valuation.actualCoveragePercent.toFixed(
                                        2
                                    )}%`
                                    : "—"
                            }
                        />
                    </div>

                    <div className="carAssessmentResult">
                        <span>Coverage Assessment</span>

                        <StatusPill
                            value={
                                valuation.coveragePosition
                            }
                            kind={
                                valuation.coveragePosition ===
                                    "Adequate"
                                    ? "green"
                                    : "red"
                            }
                        />
                    </div>

                    {valuation.remark && (
                        <div className="carRemark">
                            <span>
                                VALUATION REMARK
                            </span>
                            <p>{valuation.remark}</p>
                        </div>
                    )}
                </ReportSection>

                {/* Controls */}

                <ReportSection
                    title="Collateral Control Position"
                    subtitle="Legal, insurance and release controls."
                >
                    <div className="carControlGrid">

                        <ControlCard
                            title="Legal Perfection"
                            status={
                                controlPosition.legal.status
                            }
                        >
                            <Info
                                label="Registration Ref."
                                value={
                                    controlPosition.legal
                                        .registrationReference
                                }
                            />

                            <Info
                                label="Legal Unit"
                                value={
                                    controlPosition.legal
                                        .legalUnit
                                }
                            />

                            <Info
                                label="Collateral Valid"
                                value={
                                    controlPosition.legal
                                        .collateralValid == null
                                        ? "—"
                                        : controlPosition.legal
                                            .collateralValid
                                            ? "Yes"
                                            : "No"
                                }
                            />
                        </ControlCard>

                        <ControlCard
                            title="Insurance"
                            status={
                                controlPosition.insurance
                                    .status
                            }
                        >
                            <Info
                                label="Insurer"
                                value={
                                    controlPosition.insurance
                                        .insurer
                                }
                            />

                            <Info
                                label="Policy Number"
                                value={
                                    controlPosition.insurance
                                        .policyNumber
                                }
                            />

                            <Info
                                label="Coverage"
                                value={money(
                                    controlPosition.insurance
                                        .coverageAmount,
                                    overview.currency
                                )}
                            />

                            <Info
                                label="Expiry"
                                value={formatDate(
                                    controlPosition.insurance
                                        .policyExpiryDate
                                )}
                            />
                        </ControlCard>

                        <ControlCard
                            title="Release"
                            status={
                                controlPosition.release.status
                            }
                        >
                            <Info
                                label="Repayment"
                                value={
                                    controlPosition.release
                                        .repaymentStatus
                                }
                            />

                            <Info
                                label="Release Date"
                                value={formatDate(
                                    controlPosition.release
                                        .releaseDate
                                )}
                            />
                        </ControlCard>
                    </div>
                </ReportSection>

                {/* Documents */}

                <ReportSection
                    title="Supporting Documents"
                    subtitle="Documents retained against this collateral."
                >
                    {documents.length === 0 ? (
                        <EmptyText text="No supporting documents recorded." />
                    ) : (
                            <div className="carTableWrap">
                                <table className="carTable">
                                    <thead>
                                        <tr>
                                            <th>Document</th>
                                            <th>File Name</th>
                                            <th>Uploaded By</th>
                                            <th>Uploaded</th>
                                            <th>Action</th>
                                        </tr>
                                    </thead>

                                    <tbody>
                                        {documents.map(document => (
                                            <tr key={document.id}>
                                                <td>
                                                    {formatEnum(
                                                        document.documentType
                                                    )}
                                                </td>

                                                <td>
                                                    {document.fileName}
                                                </td>

                                                <td>
                                                    {document.uploadedBy ??
                                                        "N/A"}
                                                </td>

                                                <td>
                                                    {formatDate(
                                                        document.uploadedAtUtc
                                                    )}
                                                </td>

                                                <td>
                                                    <button
                                                        type="button"
                                                        className="carDocumentView"
                                                        onClick={() =>
                                                            handleViewDocument(
                                                                document.id
                                                            )
                                                        }
                                                    >
                                                        <Eye size={14} />
                                                        View
                                                    </button>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                    )}
                </ReportSection>

                {/* History */}

                <ReportSection
                    title="Workflow History"
                    subtitle="Recorded activities for this collateral."
                >
                    {workflowHistory.length === 0 ? (
                        <EmptyText text="No workflow history recorded." />
                    ) : (
                        <div className="carTimeline">
                            {workflowHistory.map(
                                (item, index) => (
                                    <div
                                        className="carTimelineItem"
                                        key={`${item.performedAtUtc}-${index}`}
                                    >
                                        <div className="carTimelineRail">
                                            <span />
                                        </div>

                                        <div className="carTimelineContent">
                                            <div className="carTimelineHead">
                                                <strong>
                                                    {formatEnum(
                                                        item.action
                                                    )}
                                                </strong>

                                                <time>
                                                    {formatDateTime(
                                                        item.performedAtUtc
                                                    )}
                                                </time>
                                            </div>

                                            <span className="carPerformedBy">
                                                Performed by{" "}
                                                {item.performedBy}
                                            </span>

                                            {item.details && (
                                                <p>
                                                    {
                                                        item.details
                                                    }
                                                </p>
                                            )}
                                        </div>
                                    </div>
                                )
                            )}
                        </div>
                    )}
                </ReportSection>

                <footer className="carFooter">
                    <div>
                        <strong>
                            {report.reportReference}
                        </strong>
                        <span>
                            Generated from the Collateral
                            Management Solution
                        </span>
                    </div>

                    <div>
                        <span>
                            Report generated
                        </span>
                        <strong>
                            {formatDateTime(
                                report.generatedAtUtc
                            )}
                        </strong>
                    </div>
                </footer>
            </article>
        </div>
    );
}


/* ============================================================
   SMALL COMPONENTS
   ============================================================ */

function ReportSection({
    title,
    subtitle,
    children
}: {
    title: string;
    subtitle?: string;
    children: React.ReactNode;
}) {
    return (
        <section className="carSection">
            <div className="carSectionHead">
                <h3>{title}</h3>
                {subtitle && <p>{subtitle}</p>}
            </div>

            {children}
        </section>
    );
}

function Metric({
    label,
    value,
    note,
    state
}: {
    label: string;
    value: string;
    note?: string;
    state?: "good" | "bad";
}) {
    return (
        <div
            className={`carMetric ${state ? `carMetric-${state}` : ""
                }`}
        >
            <span>{label}</span>
            <strong>{value}</strong>
            {note && <small>{note}</small>}
        </div>
    );
}

function InfoCard({
    title,
    children
}: {
    title: string;
    children: React.ReactNode;
}) {
    return (
        <div className="carInfoCard">
            <h4>{title}</h4>
            <div className="carInfoRows">
                {children}
            </div>
        </div>
    );
}

function Info({
    label,
    value
}: {
    label: string;
    value?: string | number | null;
}) {
    const displayValue =
        value === null ||
            value === undefined ||
            value === ""
            ? "N/A"
            : value;

    return (
        <div className="carInfo">
            <span>{label}</span>
            <strong>{displayValue}</strong>
        </div>
    );
}

function ControlCard({
    title,
    status,
    children
}: {
    title: string;
    status: string;
    children: React.ReactNode;
}) {
    return (
        <div className="carControlCard">
            <div className="carControlHead">
                <h4>{title}</h4>
                <StatusPill
                    value={status}
                    kind={
                        status === "Completed" ||
                            status === "Released"
                            ? "green"
                            : status === "In Progress" ||
                                status === "Awaiting Release"
                                ? "blue"
                                : "neutral"
                    }
                />
            </div>

            <div className="carControlBody">
                {children}
            </div>
        </div>
    );
}

function StatusPill({
    value,
    kind = "neutral"
}: {
    value: string;
    kind?:
    | "green"
    | "red"
    | "blue"
    | "neutral";
}) {
    return (
        <span
            className={`carStatus carStatus-${kind}`}
        >
            {value}
        </span>
    );
}

function EmptyText({
    text
}: {
    text: string;
}) {
    return (
        <div className="carEmpty">
            {text}
        </div>
    );
}

/* ============================================================
   FORMATTERS
   ============================================================ */

function money(
    value: number | null | undefined,
    currency = "NGN"
) {
    if (value == null) return "N/A";

    try {
        return new Intl.NumberFormat("en-NG", {
            style: "currency",
            currency,
            maximumFractionDigits: 0
        }).format(value);
    } catch {
        return `${currency} ${value.toLocaleString()}`;
    }
}

function formatDate(
    value: string | null | undefined
) {
    if (!value) return "N/A";

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return date.toLocaleDateString("en-NG", {
        day: "2-digit",
        month: "short",
        year: "numeric"
    });
}

function formatDateTime(
    value: string | null | undefined
) {
    if (!value) return "N/A";

    const date = new Date(value);

    if (Number.isNaN(date.getTime()))
        return value;

    return date.toLocaleString(
        "en-NG",
        {
            day: "2-digit",
            month: "short",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        }
    );
}

function formatEnum(
    value: string | null | undefined
) {
    if (!value) return "N/A";

    return value
        .replace(/([a-z])([A-Z])/g, "$1 $2")
        .replace(/_/g, " ");
}

function getLifecycle(currentStage: string) {
    const stages = [
        {
            key: "CollateralData",
            label: "Verification"
        },
        {
            key: "Valuation",
            label: "Valuation"
        },
        {
            key: "Perfection",
            label: "Legal Perfection"
        },
        {
            key: "Insurance",
            label: "Insurance"
        },
        {
            key: "Release",
            label: "Release"
        }
    ];

    if (currentStage === "Archived") {
        return stages.map(stage => ({
            ...stage,
            state: "completed" as const
        }));
    }

    if (currentStage === "SystemHold") {
        return stages.map(stage => ({
            ...stage,
            state:
                stage.key === "Release"
                    ? ("pending" as const)
                    : ("completed" as const)
        }));
    }

    const currentIndex =
        stages.findIndex(
            stage => stage.key === currentStage
        );

    return stages.map((stage, index) => ({
        ...stage,
        state:
            index < currentIndex
                ? ("completed" as const)
                : index === currentIndex
                    ? ("current" as const)
                    : ("pending" as const)
    }));
}