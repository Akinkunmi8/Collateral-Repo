import {
    AlertCircle,
    AlertTriangle,
    ArrowLeft,
    Building2,
    CalendarDays,
    Check,
    CheckCircle2,
    Clock3,
    FileCheck2,
    Info,
    Loader2,
    RefreshCw,
    Send,
    ShieldCheck,
    X,
} from 'lucide-react';
import {
    useCallback,
    useEffect,
    useMemo,
    useState,
} from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { api } from '../api/client';

interface CollateralDetail {
    id: string;

    loanContractId: string;
    customerId: string;
    customerName: string;
    accountNumber: string;
    approvedLimit: number;

    type: string;
    region: string;
    description: string;
    facilityType?: string | null;
    currency: string;
    collateralAddress?: string | null;
    collateralOwner?: string | null;
    branchName?: string | null;
    relationshipManagerName?: string | null;
    branchManagerName?: string | null;

    currentStage: string;
    status: string;

    stageStartedAtUtc: string;
    createdAtUtc: string;
    lastModifiedAtUtc?: string | null;

    tatStatus?: string | null;
    tatElapsedHours?: number | null;
    tatAllowedHours?: number | null;
    tatDueAtUtc?: string | null;

    additionalInformationRequest?: string | null;
    additionalInformationRequestedAtUtc?: string | null;
}

type VerificationDecision = 1 | 2;
type ModalAction = VerificationDecision | 'resubmit' | null;

function humanize(value?: string | null) {
    if (!value) {
        return '—';
    }

    const result = value
        .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
        .replace(/_/g, ' ')
        .trim();

    return result
        .replace(/\bTat\b/gi, 'TAT')
        .replace(/\bNgn\b/g, 'NGN');
}

function displayValue(value?: string | null) {
    if (!value || !value.trim()) {
        return '—';
    }

    return value;
}

function formatMoney(
    value: number,
    currency = 'NGN',
) {
    try {
        return new Intl.NumberFormat('en-NG', {
            style: 'currency',
            currency,
            maximumFractionDigits: 2,
        }).format(value);
    } catch {
        return `${currency} ${value.toLocaleString()}`;
    }
}

function formatDate(value?: string | null) {
    if (!value) {
        return '—';
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return new Intl.DateTimeFormat('en-GB', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
    }).format(date);
}

function cleanAdditionalRequest(value?: string | null) {
    if (!value) {
        return '';
    }

    return value
        .replace(
            /^Additional collateral\/information requested\.\s*Remark:\s*/i,
            '',
        )
        .trim();
}

function DetailItem({
    label,
    value,
    wide = false,
}: {
    label: string;
    value: React.ReactNode;
    wide?: boolean;
}) {
    return (
        <div
            className={
                wide
                    ? 'verificationDetailItem verificationDetailWide'
                    : 'verificationDetailItem'
            }
        >
            <span className="verificationDetailLabel">
                {label}
            </span>

            <div className="verificationDetailValue">
                {value}
            </div>
        </div>
    );
}

export default function CollateralVerification() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [detail, setDetail] =
        useState<CollateralDetail | null>(null);

    const [remark, setRemark] = useState('');
    const [response, setResponse] = useState('');

    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);

    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const [modalAction, setModalAction] =
        useState<ModalAction>(null);

    const loadCollateral = useCallback(async () => {
        if (!id) {
            setError('Collateral identifier was not supplied.');
            setLoading(false);
            return;
        }

        try {
            setLoading(true);
            setError('');

            const result =
                await api.get<CollateralDetail>(
                    `/collaterals/${id}`,
                );

            setDetail(result.data);
        } catch (err: any) {
            setError(
                err?.response?.data?.detail ??
                err?.response?.data?.message ??
                'Unable to load collateral information.',
            );
        } finally {
            setLoading(false);
        }
    }, [id]);

    useEffect(() => {
        void loadCollateral();
    }, [loadCollateral]);

    useEffect(() => {
        if (!success) {
            return;
        }

        const timer = window.setTimeout(() => {
            setSuccess('');
        }, 5000);

        return () => window.clearTimeout(timer);
    }, [success]);

    useEffect(() => {
        if (modalAction === null) {
            return;
        }

        const handleKeyDown = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && !submitting) {
                setModalAction(null);
            }
        };

        document.addEventListener(
            'keydown',
            handleKeyDown,
        );

        return () =>
            document.removeEventListener(
                'keydown',
                handleKeyDown,
            );
    }, [modalAction, submitting]);

    const canVerify =
        detail?.currentStage === 'CollateralData' &&
        detail?.status === 'PendingApproval';

    const needsAdditionalInformation =
        detail?.currentStage === 'CollateralData' &&
        detail?.status ===
        'AdditionalCollateralRequested';

    const verificationCompleted =
        detail?.currentStage === 'Valuation' &&
        detail?.status === 'InValuation';

    const previousRequest = useMemo(
        () =>
            cleanAdditionalRequest(
                detail?.additionalInformationRequest,
            ),
        [detail?.additionalInformationRequest],
    );

    const exceededHours = useMemo(() => {
        if (
            detail?.tatElapsedHours == null ||
            detail?.tatAllowedHours == null
        ) {
            return 0;
        }

        return Math.max(
            0,
            detail.tatElapsedHours -
            detail.tatAllowedHours,
        );
    }, [
        detail?.tatElapsedHours,
        detail?.tatAllowedHours,
    ]);

    function openDecision(
        decision: VerificationDecision,
    ) {
        setError('');
        setSuccess('');

        if (
            decision === 2 &&
            !remark.trim()
        ) {
            setError(
                'Enter the additional information required before continuing.',
            );
            return;
        }

        setModalAction(decision);
    }

    function openResubmit() {
        setError('');
        setSuccess('');

        if (!response.trim()) {
            setError(
                'Enter a response explaining what was provided or corrected before resubmitting.',
            );
            return;
        }

        setModalAction('resubmit');
    }

    async function confirmAction() {
        if (!id || modalAction === null) {
            return;
        }

        try {
            setSubmitting(true);
            setError('');
            setSuccess('');

            if (modalAction === 'resubmit') {
                await api.post(
                    `/collaterals/${id}/resubmit`,
                    {
                        response: response.trim(),
                    },
                );

                setModalAction(null);
                setResponse('');

                setSuccess(
                    'Collateral resubmitted successfully and returned for verification.',
                );
            } else {
                await api.post(
                    `/collaterals/${id}/verify`,
                    {
                        decision: modalAction,
                        remark:
                            remark.trim() || null,
                    },
                );

                const wasApproved =
                    modalAction === 1;

                setModalAction(null);
                setRemark('');

                setSuccess(
                    wasApproved
                        ? 'Collateral verified successfully and sent to Valuation.'
                        : 'Additional information has been requested successfully.',
                );
            }

            await loadCollateral();
        } catch (err: any) {
            setModalAction(null);

            setError(
                err?.response?.data?.detail ??
                err?.response?.data?.message ??
                err?.response?.data?.title ??
                'The operation could not be completed.',
            );
        } finally {
            setSubmitting(false);
        }
    }

    if (loading) {
        return (
            <div className="verificationPage">
                <div className="verificationLoading">
                    <Loader2
                        size={30}
                        className="verificationSpinner"
                    />
                    <span>
                        Loading collateral information...
                    </span>
                </div>
            </div>
        );
    }

    if (!detail) {
        return (
            <div className="verificationPage">
                <div className="verificationErrorState">
                    <AlertCircle size={34} />

                    <h2>
                        Unable to open collateral
                    </h2>

                    <p>
                        {error ||
                            'The collateral record could not be loaded.'}
                    </p>

                    <button
                        type="button"
                        className="verificationSecondaryButton"
                        onClick={() =>
                            navigate('/collateral')
                        }
                    >
                        <ArrowLeft size={16} />
                        Back to Register
                    </button>
                </div>
            </div>
        );
    }

    const tatBeyond =
        detail.tatStatus === 'BeyondTat';

    return (
        <div className="verificationPage">
            {/* Header */}

            <div className="verificationBreadcrumb">
                <button
                    type="button"
                    onClick={() =>
                        navigate('/collateral')
                    }
                >
                    Collateral
                </button>

                <span>/</span>
                <span>Verification</span>
                <span>/</span>

                <strong>
                    {detail.loanContractId}
                </strong>
            </div>

            <div className="verificationHeader">
                <div>
                    <h1>
                        Collateral Verification
                    </h1>

                    <p>
                        Review collateral information,
                        verify completeness and progress
                        the record to the next stage.
                    </p>
                </div>

                <button
                    type="button"
                    className="verificationBackButton"
                    onClick={() =>
                        navigate('/collateral')
                    }
                >
                    <ArrowLeft size={16} />
                    Back to Register
                </button>
            </div>

            {/* Notifications */}

            {success && (
                <div
                    className="verificationNotice verificationNoticeSuccess"
                    role="status"
                >
                    <CheckCircle2 size={19} />

                    <div>
                        <strong>
                            Operation successful
                        </strong>
                        <span>{success}</span>
                    </div>

                    <button
                        type="button"
                        aria-label="Close notification"
                        onClick={() =>
                            setSuccess('')
                        }
                    >
                        <X size={17} />
                    </button>
                </div>
            )}

            {error && (
                <div
                    className="verificationNotice verificationNoticeError"
                    role="alert"
                >
                    <AlertCircle size={19} />

                    <div>
                        <strong>
                            Action required
                        </strong>
                        <span>{error}</span>
                    </div>

                    <button
                        type="button"
                        aria-label="Close notification"
                        onClick={() =>
                            setError('')
                        }
                    >
                        <X size={17} />
                    </button>
                </div>
            )}

            {/* Workflow & TAT */}

            <section className="verificationCard">
                <div className="verificationCardHeader">
                    <div className="verificationSectionIcon">
                        <ShieldCheck size={19} />
                    </div>

                    <div>
                        <h2>
                            Workflow &amp; TAT
                        </h2>
                        <p>
                            Current processing position
                            and turnaround-time status.
                        </p>
                    </div>
                </div>

                <div className="workflowMetrics">
                    <div className="workflowMetric">
                        <span>Current Stage</span>

                        <strong>
                            <span className="statusBadge statusBadgeBlue">
                                {humanize(
                                    detail.currentStage,
                                )}
                            </span>
                        </strong>
                    </div>

                    <div className="workflowMetric">
                        <span>Status</span>

                        <strong>
                            <span
                                className={`statusBadge ${detail.status ===
                                        'PendingApproval'
                                        ? 'statusBadgeAmber'
                                        : detail.status ===
                                            'AdditionalCollateralRequested'
                                            ? 'statusBadgePurple'
                                            : detail.status ===
                                                'InValuation'
                                                ? 'statusBadgeGreen'
                                                : 'statusBadgeNeutral'
                                    }`}
                            >
                                {humanize(
                                    detail.status,
                                )}
                            </span>
                        </strong>
                    </div>

                    <div className="workflowMetric">
                        <span>TAT Status</span>

                        <strong>
                            <span
                                className={`statusBadge ${tatBeyond
                                        ? 'statusBadgeRed'
                                        : 'statusBadgeGreen'
                                    }`}
                            >
                                {humanize(
                                    detail.tatStatus,
                                )}
                            </span>
                        </strong>
                    </div>

                    <div className="workflowMetric">
                        <span>Elapsed Time</span>
                        <strong>
                            {detail.tatElapsedHours !=
                                null
                                ? `${detail.tatElapsedHours.toFixed(
                                    2,
                                )} hours`
                                : '—'}
                        </strong>
                    </div>

                    <div className="workflowMetric">
                        <span>Allowed TAT</span>
                        <strong>
                            {detail.tatAllowedHours !=
                                null
                                ? `${detail.tatAllowedHours} hours`
                                : '—'}
                        </strong>
                    </div>

                    <div className="workflowMetric">
                        <span>TAT Due</span>
                        <strong>
                            {formatDate(
                                detail.tatDueAtUtc,
                            )}
                        </strong>
                    </div>
                </div>

                {tatBeyond && (
                    <div className="tatExceededBanner">
                        <AlertTriangle size={18} />

                        <span>
                            This collateral has exceeded
                            its current-stage TAT by{' '}
                            <strong>
                                {exceededHours.toFixed(2)}
                                {' '}hours
                            </strong>
                            .
                        </span>
                    </div>
                )}
            </section>

            {/* Customer / Facility */}

            <section className="verificationCard">
                <div className="verificationCardHeader">
                    <div className="verificationSectionIcon">
                        <Building2 size={19} />
                    </div>

                    <div>
                        <h2>
                            Customer &amp; Facility Information
                        </h2>
                        <p>
                            Core customer and credit
                            facility information.
                        </p>
                    </div>
                </div>

                <div className="verificationDetailGrid">
                    <DetailItem
                        label="Customer Name"
                        value={
                            detail.customerName
                        }
                    />

                    <DetailItem
                        label="Customer ID"
                        value={
                            detail.customerId
                        }
                    />

                    <DetailItem
                        label="Account Number"
                        value={
                            detail.accountNumber
                        }
                    />

                    <DetailItem
                        label="Loan Contract"
                        value={
                            detail.loanContractId
                        }
                    />

                    <DetailItem
                        label="Facility Type"
                        value={displayValue(
                            detail.facilityType,
                        )}
                    />

                    <DetailItem
                        label="Approved Limit"
                        value={formatMoney(
                            detail.approvedLimit,
                            detail.currency,
                        )}
                    />

                    <DetailItem
                        label="Branch"
                        value={displayValue(
                            detail.branchName,
                        )}
                    />

                    <DetailItem
                        label="Region"
                        value={humanize(
                            detail.region,
                        )}
                    />

                    <DetailItem
                        label="Relationship Manager"
                        value={displayValue(
                            detail.relationshipManagerName,
                        )}
                    />

                    <DetailItem
                        label="Branch Manager"
                        value={displayValue(
                            detail.branchManagerName,
                        )}
                    />
                </div>
            </section>

            {/* Collateral */}

            <section className="verificationCard">
                <div className="verificationCardHeader">
                    <div className="verificationSectionIcon">
                        <FileCheck2 size={19} />
                    </div>

                    <div>
                        <h2>
                            Collateral Information
                        </h2>
                        <p>
                            Details of the security
                            supporting this facility.
                        </p>
                    </div>
                </div>

                <div className="verificationDetailGrid">
                    <DetailItem
                        label="Collateral Type"
                        value={humanize(
                            detail.type,
                        )}
                    />

                    <DetailItem
                        label="Owner"
                        value={displayValue(
                            detail.collateralOwner,
                        )}
                    />

                    <DetailItem
                        label="Currency"
                        value={
                            detail.currency
                        }
                    />

                    <DetailItem
                        label="Created"
                        value={formatDate(
                            detail.createdAtUtc,
                        )}
                    />

                    <DetailItem
                        label="Collateral Address"
                        value={displayValue(
                            detail.collateralAddress,
                        )}
                        wide
                    />

                    <DetailItem
                        label="Description"
                        value={
                            detail.description
                        }
                        wide
                    />
                </div>
            </section>

            {/* Previous request history */}

            {previousRequest &&
                !needsAdditionalInformation && (
                    <section className="previousRequestCard">
                        <div className="previousRequestIcon">
                            <RefreshCw size={18} />
                        </div>

                        <div className="previousRequestContent">
                            <div className="previousRequestHeading">
                                <div>
                                    <strong>
                                        Previous additional
                                        information request
                                    </strong>

                                    <span>
                                        This collateral was
                                        previously returned
                                        for clarification.
                                    </span>
                                </div>

                                {detail.additionalInformationRequestedAtUtc && (
                                    <div className="previousRequestDate">
                                        <CalendarDays
                                            size={15}
                                        />

                                        {formatDate(
                                            detail.additionalInformationRequestedAtUtc,
                                        )}
                                    </div>
                                )}
                            </div>

                            <p>
                                {previousRequest}
                            </p>
                        </div>
                    </section>
                )}

            {/* =================================================
                STATE 1: PENDING APPROVAL
               ================================================= */}

            {canVerify && (
                <section className="verificationDecisionCard">
                    <div className="verificationDecisionHeader">
                        <div>
                            <h2>
                                Verification Decision
                            </h2>

                            <p>
                                Confirm the collateral
                                information or return it
                                for additional information.
                            </p>
                        </div>

                        <span className="verificationRequiredBadge">
                            Review Required
                        </span>
                    </div>

                    <div className="verificationFormGroup">
                        <label htmlFor="verificationRemark">
                            Verification Remark
                            <span>
                                Optional for approval
                            </span>
                        </label>

                        <textarea
                            id="verificationRemark"
                            value={remark}
                            maxLength={1000}
                            rows={4}
                            placeholder="Enter a verification remark or describe the additional information required..."
                            onChange={(e) =>
                                setRemark(
                                    e.target.value,
                                )
                            }
                        />

                        <div className="verificationCharacterCount">
                            {remark.length}/1000
                        </div>
                    </div>

                    <div className="verificationDecisionFooter">
                        <div className="verificationDecisionHint">
                            <Info size={16} />

                            <span>
                                Request Additional requires
                                a remark. Approval moves the
                                collateral directly to
                                Valuation.
                            </span>
                        </div>

                        <div className="verificationActionButtons">
                            <button
                                type="button"
                                className="verificationRequestButton"
                                disabled={submitting}
                                onClick={() =>
                                    openDecision(2)
                                }
                            >
                                Request Additional
                            </button>

                            <button
                                type="button"
                                className="verificationApproveButton"
                                disabled={submitting}
                                onClick={() =>
                                    openDecision(1)
                                }
                            >
                                <Check size={17} />
                                Approve
                            </button>
                        </div>
                    </div>
                </section>
            )}

            {/* =================================================
                STATE 2: ADDITIONAL INFORMATION REQUESTED
               ================================================= */}

            {needsAdditionalInformation && (
                <section className="additionalInfoCard">
                    <div className="additionalInfoHeader">
                        <div className="additionalInfoIcon">
                            <AlertTriangle size={20} />
                        </div>

                        <div>
                            <h2>
                                Additional Information Required
                            </h2>

                            <p>
                                This collateral was returned
                                for additional information
                                before verification can
                                continue.
                            </p>
                        </div>
                    </div>

                    <div className="requestSummary">
                        <div className="requestSummaryTop">
                            <span>
                                Verification Request
                            </span>

                            {detail.additionalInformationRequestedAtUtc && (
                                <span className="requestSummaryDate">
                                    <Clock3 size={14} />

                                    {formatDate(
                                        detail.additionalInformationRequestedAtUtc,
                                    )}
                                </span>
                            )}
                        </div>

                        <p>
                            {previousRequest ||
                                'Additional collateral information has been requested.'}
                        </p>
                    </div>

                    <div className="verificationFormGroup">
                        <label htmlFor="additionalResponse">
                            Response / Information Provided
                            <span>Required</span>
                        </label>

                        <textarea
                            id="additionalResponse"
                            value={response}
                            maxLength={1000}
                            rows={5}
                            placeholder="Explain what has been corrected, clarified or provided..."
                            onChange={(e) =>
                                setResponse(
                                    e.target.value,
                                )
                            }
                        />

                        <div className="verificationCharacterCount">
                            {response.length}/1000
                        </div>
                    </div>

                    <div className="resubmitFooter">
                        <div className="verificationDecisionHint">
                            <Info size={16} />

                            <span>
                                Resubmitting returns this
                                collateral to Pending
                                Approval for another
                                verification review.
                            </span>
                        </div>

                        <button
                            type="button"
                            className="verificationApproveButton"
                            disabled={submitting}
                            onClick={openResubmit}
                        >
                            <Send size={16} />
                            Resubmit
                        </button>
                    </div>
                </section>
            )}

            {/* =================================================
                STATE 3: MOVED TO VALUATION
               ================================================= */}

            {verificationCompleted && (
                <section className="verificationCompleteCard">
                    <div className="verificationCompleteIcon">
                        <CheckCircle2 size={25} />
                    </div>

                    <div className="verificationCompleteBody">
                        <h2>
                            Verification Completed
                        </h2>

                        <p>
                            This collateral has been
                            successfully verified and sent
                            to the Valuation stage.
                        </p>

                        <div className="verificationCompleteMeta">
                            <div>
                                <span>
                                    Current Stage
                                </span>

                                <strong>
                                    {humanize(
                                        detail.currentStage,
                                    )}
                                </strong>
                            </div>

                            <div>
                                <span>
                                    Status
                                </span>

                                <strong>
                                    {humanize(
                                        detail.status,
                                    )}
                                </strong>
                            </div>

                            <div>
                                <span>
                                    Stage Started
                                </span>

                                <strong>
                                    {formatDate(
                                        detail.stageStartedAtUtc,
                                    )}
                                </strong>
                            </div>
                        </div>
                    </div>
                </section>
            )}

            {/* Other lifecycle states */}

            {!canVerify &&
                !needsAdditionalInformation &&
                !verificationCompleted && (
                    <section className="verificationReadOnlyCard">
                        <Info size={20} />

                        <div>
                            <strong>
                                No verification action
                                required
                            </strong>

                            <span>
                                This collateral is currently
                                at the{' '}
                                {humanize(
                                    detail.currentStage,
                                )}{' '}
                                stage with status{' '}
                                {humanize(
                                    detail.status,
                                )}
                                .
                            </span>
                        </div>
                    </section>
                )}

            {/* =================================================
                CUSTOM CONFIRMATION MODAL
               ================================================= */}

            {modalAction !== null && (
                <div
                    className="verificationModalOverlay"
                    role="presentation"
                    onMouseDown={(e) => {
                        if (
                            e.target ===
                            e.currentTarget &&
                            !submitting
                        ) {
                            setModalAction(null);
                        }
                    }}
                >
                    <div
                        className="verificationModal"
                        role="dialog"
                        aria-modal="true"
                        aria-labelledby="verificationModalTitle"
                    >
                        <button
                            type="button"
                            className="verificationModalClose"
                            aria-label="Close"
                            disabled={submitting}
                            onClick={() =>
                                setModalAction(null)
                            }
                        >
                            <X size={18} />
                        </button>

                        <div
                            className={`verificationModalIcon ${modalAction === 1
                                    ? 'verificationModalIconSuccess'
                                    : modalAction === 2
                                        ? 'verificationModalIconWarning'
                                        : 'verificationModalIconBlue'
                                }`}
                        >
                            {modalAction === 1 ? (
                                <CheckCircle2
                                    size={24}
                                />
                            ) : modalAction === 2 ? (
                                <AlertTriangle
                                    size={24}
                                />
                            ) : (
                                <Send size={22} />
                            )}
                        </div>

                        <h2 id="verificationModalTitle">
                            {modalAction === 1
                                ? 'Approve Collateral?'
                                : modalAction === 2
                                    ? 'Request Additional Information?'
                                    : 'Resubmit for Verification?'}
                        </h2>

                        <p className="verificationModalDescription">
                            {modalAction === 1
                                ? 'This collateral will be verified and moved to the Valuation stage.'
                                : modalAction === 2
                                    ? 'The collateral will be returned for additional information before verification can continue.'
                                    : 'This collateral will return to Pending Approval for another verification review.'}
                        </p>

                        {(modalAction === 2 ||
                            modalAction ===
                            'resubmit') && (
                                <div className="verificationModalRemark">
                                    <span>
                                        {modalAction === 2
                                            ? 'Request'
                                            : 'Response'}
                                    </span>

                                    <p>
                                        {modalAction === 2
                                            ? remark
                                            : response}
                                    </p>
                                </div>
                            )}

                        {modalAction === 1 &&
                            remark.trim() && (
                                <div className="verificationModalRemark">
                                    <span>
                                        Remark
                                    </span>
                                    <p>{remark}</p>
                                </div>
                            )}

                        <div className="verificationModalFooter">
                            <button
                                type="button"
                                className="verificationModalCancel"
                                disabled={submitting}
                                onClick={() =>
                                    setModalAction(null)
                                }
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                className={
                                    modalAction === 2
                                        ? 'verificationModalConfirm verificationModalConfirmDanger'
                                        : 'verificationModalConfirm'
                                }
                                disabled={submitting}
                                onClick={() =>
                                    void confirmAction()
                                }
                            >
                                {submitting ? (
                                    <>
                                        <Loader2
                                            size={16}
                                            className="verificationSpinner"
                                        />
                                        Processing...
                                    </>
                                ) : modalAction ===
                                    1 ? (
                                    <>
                                        <Check
                                            size={16}
                                        />
                                        Confirm Approval
                                    </>
                                ) : modalAction ===
                                    2 ? (
                                    'Confirm Request'
                                ) : (
                                    <>
                                        <Send
                                            size={16}
                                        />
                                        Confirm Resubmit
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