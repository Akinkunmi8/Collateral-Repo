import {
    ChangeEvent,
    useEffect,
    useMemo,
    useState
} from "react";

import {
    useNavigate,
    useParams
} from "react-router-dom";

import {
    completeLegalPerfection,
    downloadLegalReport,
    getCollateralDocuments,
    getLegalWorkItem,
    uploadLegalReport
} from "../api/legal";

import {
    CollateralDocumentType
} from "../types/legal";

import type {
    CollateralDocument,
    LegalWorkItem
} from "../types/legal";

import "./legal-perfection.css";

export default function LegalPerfection() {

    const { id } = useParams<{ id: string }>();

    const navigate = useNavigate();

    const [workItem, setWorkItem] =
        useState<LegalWorkItem | null>(null);

    const [documents, setDocuments] =
        useState<CollateralDocument[]>([]);

    const [registrationReference, setRegistrationReference] =
        useState("");

    const [legalUnit, setLegalUnit] =
        useState("");

    const [collateralValid, setCollateralValid] =
        useState(true);

    const [invalidReason, setInvalidReason] =
        useState("");

    const [selectedFile, setSelectedFile] =
        useState<File | null>(null);

    const [loading, setLoading] =
        useState(true);

    const [uploading, setUploading] =
        useState(false);

    const [submitting, setSubmitting] =
        useState(false);

    const [error, setError] =
        useState<string | null>(null);

    const [success, setSuccess] =
        useState<string | null>(null);

    const [showConfirm, setShowConfirm] =
        useState(false);

    useEffect(() => {

        if (!id) {
            return;
        }

        loadPage();

    }, [id]);

    async function loadPage() {

        if (!id) {
            return;
        }

        try {

            setLoading(true);
            setError(null);

            const [
                workItemResult,
                documentResult
            ] = await Promise.all([
                getLegalWorkItem(id),
                getCollateralDocuments(id)
            ]);

            setWorkItem(workItemResult);
            setDocuments(documentResult);

            /*
             * If Legal Perfection data already exists,
             * populate the form.
             */
            if (workItemResult.perfection) {

                setRegistrationReference(
                    workItemResult.perfection
                        .registrationReference ?? ""
                );

                setLegalUnit(
                    workItemResult.perfection
                        .legalUnit ?? ""
                );

                setCollateralValid(
                    workItemResult.perfection
                        .collateralValid
                );

                setInvalidReason(
                    workItemResult.perfection
                        .invalidReason ?? ""
                );
            }

        }
        catch (err) {

            console.error(err);

            setError(
                "Unable to load the Legal Perfection work item."
            );

        }
        finally {

            setLoading(false);
        }
    }

    const legalReports = useMemo(
        () =>
            documents.filter(
                document =>
                    document.documentType ===
                    CollateralDocumentType.LegalReport
            ),
        [documents]
    );

    const latestLegalReport =
        legalReports.length > 0
            ? legalReports[0]
            : null;

    function handleFileChange(
        event: ChangeEvent<HTMLInputElement>
    ) {

        setError(null);
        setSuccess(null);

        const file =
            event.target.files?.[0];

        if (!file) {
            setSelectedFile(null);
            return;
        }

        if (file.type !== "application/pdf") {

            setSelectedFile(null);

            setError(
                "Legal Perfection reports must be PDF documents."
            );

            event.target.value = "";

            return;
        }

        if (file.size > 10_000_000) {

            setSelectedFile(null);

            setError(
                "The Legal Perfection report cannot exceed 10 MB."
            );

            event.target.value = "";

            return;
        }

        setSelectedFile(file);
    }

    async function handleUpload() {

        if (!id || !selectedFile) {
            return;
        }

        try {

            setUploading(true);
            setError(null);
            setSuccess(null);

            await uploadLegalReport(
                id,
                selectedFile
            );

            const refreshedDocuments =
                await getCollateralDocuments(id);

            setDocuments(refreshedDocuments);
            setSelectedFile(null);

            setSuccess(
                "Legal Perfection report uploaded successfully."
            );

        }
        catch (err) {

            console.error(err);

            setError(
                "Unable to upload the Legal Perfection report."
            );

        }
        finally {

            setUploading(false);
        }
    }

    async function handleViewReport(
        document: CollateralDocument
    ) {

        if (!id) {
            return;
        }

        try {

            setError(null);

            const blob =
                await downloadLegalReport(
                    id,
                    document.id
                );

            const url =
                window.URL.createObjectURL(blob);

            window.open(
                url,
                "_blank",
                "noopener,noreferrer"
            );

            /*
             * Give the browser time to open the
             * object URL before releasing it.
             */
            window.setTimeout(
                () =>
                    window.URL.revokeObjectURL(url),
                60_000
            );

        }
        catch (err) {

            console.error(err);

            setError(
                "Unable to open the Legal Perfection report."
            );
        }
    }

    function validateCompletion(): boolean {

        setError(null);

        if (!registrationReference.trim()) {

            setError(
                "Registration Reference is required."
            );

            return false;
        }

        if (!legalUnit.trim()) {

            setError(
                "Legal Unit is required."
            );

            return false;
        }

        if (
            !collateralValid &&
            !invalidReason.trim()
        ) {

            setError(
                "Please provide the reason the collateral is not valid."
            );

            return false;
        }

        if (!latestLegalReport) {

            setError(
                "Upload the Legal Perfection report before completing perfection."
            );

            return false;
        }

        return true;
    }

    function requestCompletion() {

        if (!validateCompletion()) {
            return;
        }

        setShowConfirm(true);
    }

    async function handleComplete() {

        if (!id) {
            return;
        }

        try {

            setSubmitting(true);
            setError(null);
            setSuccess(null);

            await completeLegalPerfection(
                id,
                {
                    collateralValid,
                    invalidReason:
                        collateralValid
                            ? null
                            : invalidReason.trim(),

                    registrationReference:
                        registrationReference.trim(),

                    legalUnit:
                        legalUnit.trim()
                }
            );

            setShowConfirm(false);

            setSuccess(
                "Legal Perfection completed successfully. The collateral has moved to Insurance."
            );

            /*
             * Give the user feedback before returning
             * to the Legal queue.
             */
            window.setTimeout(
                () => navigate("/legal"),
                1200
            );

        }
        catch (err) {

            console.error(err);

            setShowConfirm(false);

            setError(
                "Unable to complete Legal Perfection."
            );

        }
        finally {

            setSubmitting(false);
        }
    }

    function formatMoney(
        value: number | null | undefined,
        currency = "NGN"
    ) {

        if (
            value === null ||
            value === undefined
        ) {
            return "-";
        }

        return new Intl.NumberFormat(
            "en-NG",
            {
                style: "currency",
                currency,
                maximumFractionDigits: 2
            }
        ).format(value);
    }

    function formatDate(
        value: string | null | undefined
    ) {

        if (!value) {
            return "-";
        }

        return new Date(value)
            .toLocaleDateString();
    }

    if (loading) {

        return (
            <div className="legal-detail-page">
                <div className="legal-loading">
                    Loading Legal Perfection...
                </div>
            </div>
        );
    }

    if (!workItem) {

        return (
            <div className="legal-detail-page">
                <div className="legal-error">
                    Legal Perfection work item
                    could not be found.
                </div>
            </div>
        );
    }

    const collateral =
        workItem.collateralData;

    const valuation =
        workItem.valuation;

    return (
        <div className="legal-detail-page">

            <div className="legal-detail-header">

                <div>

                    <button
                        className="legal-back-button"
                        onClick={() =>
                            navigate("/legal")
                        }
                    >
                        ← Back to Legal Queue
                    </button>

                    <h1>
                        Legal Perfection
                    </h1>

                    <p>
                        {collateral.loanContractId}
                        {" · "}
                        {collateral.customerName}
                    </p>

                </div>

                <div className="legal-stage-badge">
                    {workItem.status}
                </div>

            </div>

            {error && (
                <div className="legal-message error">
                    {error}
                </div>
            )}

            {success && (
                <div className="legal-message success">
                    {success}
                </div>
            )}

            <div className="legal-detail-grid">

                {/* COLLATERAL */}

                <section className="legal-card">

                    <div className="legal-card-heading">
                        <div>
                            <span className="legal-section-number">
                                01
                            </span>

                            <h2>
                                Collateral Information
                            </h2>
                        </div>

                        <span className="legal-readonly">
                            Read Only
                        </span>
                    </div>

                    <div className="legal-info-grid">

                        <Info
                            label="Loan Contract"
                            value={
                                collateral.loanContractId
                            }
                        />

                        <Info
                            label="Account Number"
                            value={
                                collateral.accountNumber
                            }
                        />

                        <Info
                            label="CIF ID"
                            value={
                                collateral.cifId
                            }
                        />

                        <Info
                            label="Customer"
                            value={
                                collateral.customerName
                            }
                        />

                        <Info
                            label="Facility Type"
                            value={
                                collateral.facilityType
                            }
                        />

                        <Info
                            label="Approved Limit"
                            value={formatMoney(
                                collateral.approvedLimit,
                                collateral.currency
                            )}
                        />

                        <Info
                            label="Collateral Type"
                            value={
                                collateral.collateralType
                            }
                        />

                        <Info
                            label="Region"
                            value={
                                collateral.region
                            }
                        />

                        <Info
                            label="Collateral Owner"
                            value={
                                collateral.collateralOwner
                            }
                        />

                        <Info
                            label="Branch"
                            value={
                                collateral.branchName
                            }
                        />

                        <Info
                            label="Relationship Manager"
                            value={
                                collateral.relationshipManagerName
                            }
                        />

                        <Info
                            label="Branch Manager"
                            value={
                                collateral.branchManagerName
                            }
                        />

                    </div>

                    <div className="legal-description">

                        <span>
                            Collateral Description
                        </span>

                        <p>
                            {
                                collateral
                                    .collateralDescription
                            }
                        </p>

                    </div>

                    <div className="legal-description">

                        <span>
                            Collateral Address
                        </span>

                        <p>
                            {
                                collateral
                                    .collateralAddress ||
                                "-"
                            }
                        </p>

                    </div>

                </section>

                {/* VALUATION */}

                <section className="legal-card">

                    <div className="legal-card-heading">

                        <div>
                            <span className="legal-section-number">
                                02
                            </span>

                            <h2>
                                Valuation Summary
                            </h2>
                        </div>

                        <span className="legal-readonly">
                            Read Only
                        </span>

                    </div>

                    {!valuation ? (

                        <div className="legal-empty">
                            No completed valuation
                            information is available.
                        </div>

                    ) : (

                        <>
                            <div className="legal-info-grid">

                                <Info
                                    label="Open Market Value"
                                    value={formatMoney(
                                        valuation.openMarketValue,
                                        collateral.currency
                                    )}
                                />

                                <Info
                                    label="Forced Sale Value"
                                    value={formatMoney(
                                        valuation.forcedSaleValue,
                                        collateral.currency
                                    )}
                                />

                                <Info
                                    label="Valuer"
                                    value={
                                        valuation.valuersName
                                    }
                                />

                                <Info
                                    label="Valuation Date"
                                    value={formatDate(
                                        valuation.valuationDate
                                    )}
                                />

                                <Info
                                    label="Actual Coverage"
                                    value={
                                        `${valuation.actualCoverageRatioPercent}%`
                                    }
                                />

                                <Info
                                    label="Required Coverage"
                                    value={
                                        `${valuation.requiredCoverageRatioPercent}%`
                                    }
                                />

                            </div>

                            <div className="coverage-row">

                                <span>
                                    Coverage Assessment
                                </span>

                                <span
                                    className={
                                        valuation.coverageAdequate
                                            ? "coverage-good"
                                            : "coverage-low"
                                    }
                                >
                                    {
                                        valuation.coverageAdequate
                                            ? "Adequate"
                                            : "Below Required Coverage"
                                    }
                                </span>

                            </div>

                            {valuation.remark && (
                                <div className="legal-description">
                                    <span>
                                        Valuation Remark
                                    </span>

                                    <p>
                                        {valuation.remark}
                                    </p>
                                </div>
                            )}
                        </>

                    )}

                </section>

            </div>

            {/* LEGAL PERFECTION FORM */}

            <section className="legal-card legal-form-card">

                <div className="legal-card-heading">

                    <div>
                        <span className="legal-section-number">
                            03
                        </span>

                        <h2>
                            Legal Perfection Details
                        </h2>
                    </div>

                    <span className="required-badge">
                        Required
                    </span>

                </div>

                <div className="legal-form-grid">

                    <div className="legal-field">

                        <label>
                            Registration Reference
                            <span>*</span>
                        </label>

                        <input
                            type="text"
                            maxLength={100}
                            value={
                                registrationReference
                            }
                            onChange={e =>
                                setRegistrationReference(
                                    e.target.value
                                )
                            }
                            placeholder=
                            "Enter registration reference"
                        />

                    </div>

                    <div className="legal-field">

                        <label>
                            Legal Unit
                            <span>*</span>
                        </label>

                        <input
                            type="text"
                            maxLength={100}
                            value={legalUnit}
                            onChange={e =>
                                setLegalUnit(
                                    e.target.value
                                )
                            }
                            placeholder=
                            "Enter responsible legal unit"
                        />

                    </div>

                </div>

                <div className="legal-validity">

                    <div>
                        <strong>
                            Collateral Record Validity
                        </strong>

                        <p>
                            Confirm that the pledged
                            collateral record exists and
                            is legally valid.
                        </p>
                    </div>

                    <div className="validity-options">

                        <label
                            className={
                                collateralValid
                                    ? "validity-option selected"
                                    : "validity-option"
                            }
                        >
                            <input
                                type="radio"
                                checked={collateralValid}
                                onChange={() => {
                                    setCollateralValid(true);
                                    setInvalidReason("");
                                }}
                            />

                            Valid
                        </label>

                        <label
                            className={
                                !collateralValid
                                    ? "validity-option selected invalid"
                                    : "validity-option"
                            }
                        >
                            <input
                                type="radio"
                                checked={!collateralValid}
                                onChange={() =>
                                    setCollateralValid(false)
                                }
                            />

                            Not Valid
                        </label>

                    </div>

                </div>

                {!collateralValid && (

                    <div className="legal-field full">

                        <label>
                            Reason Collateral Is Not Valid
                            <span>*</span>
                        </label>

                        <textarea
                            rows={4}
                            maxLength={500}
                            value={invalidReason}
                            onChange={e =>
                                setInvalidReason(
                                    e.target.value
                                )
                            }
                            placeholder=
                            "Provide the legal reason..."
                        />

                    </div>

                )}

            </section>

            {/* REPORT */}

            <section className="legal-card legal-report-card">

                <div className="legal-card-heading">

                    <div>
                        <span className="legal-section-number">
                            04
                        </span>

                        <h2>
                            Legal Perfection Report
                        </h2>
                    </div>

                    <span className="required-badge">
                        PDF Required
                    </span>

                </div>

                {latestLegalReport && (

                    <div className="existing-report">

                        <div className="report-icon">
                            PDF
                        </div>

                        <div className="report-details">

                            <strong>
                                {
                                    latestLegalReport
                                        .fileName
                                }
                            </strong>

                            <span>
                                Uploaded by{" "}
                                {
                                    latestLegalReport
                                        .uploadedBy
                                }
                            </span>

                        </div>

                        <button
                            type="button"
                            className="view-report-btn"
                            onClick={() =>
                                handleViewReport(
                                    latestLegalReport
                                )
                            }
                        >
                            View Report
                        </button>

                    </div>

                )}

                <div className="legal-upload">

                    <input
                        id="legal-report"
                        type="file"
                        accept="application/pdf,.pdf"
                        onChange={
                            handleFileChange
                        }
                    />

                    <label htmlFor="legal-report">

                        <strong>
                            {
                                selectedFile
                                    ? selectedFile.name
                                    : latestLegalReport
                                        ? "Upload Replacement Report"
                                        : "Select Legal Perfection Report"
                            }
                        </strong>

                        <span>
                            PDF only · Maximum 10 MB
                        </span>

                    </label>

                    {selectedFile && (

                        <button
                            type="button"
                            className="upload-report-btn"
                            disabled={uploading}
                            onClick={
                                handleUpload
                            }
                        >
                            {
                                uploading
                                    ? "Uploading..."
                                    : "Upload Report"
                            }
                        </button>

                    )}

                </div>

            </section>

            {/* COMPLETE */}

            <div className="legal-action-bar">

                <div>
                    <strong>
                        Complete Legal Perfection
                    </strong>

                    <span>
                        Completion will move this
                        collateral to Insurance.
                    </span>
                </div>

                <button
                    type="button"
                    className="complete-perfection-btn"
                    disabled={
                        submitting ||
                        uploading
                    }
                    onClick={
                        requestCompletion
                    }
                >
                    Complete Legal Perfection
                </button>

            </div>

            {/* CONFIRM MODAL */}

            {showConfirm && (

                <div className="legal-modal-backdrop">

                    <div className="legal-modal">

                        <div className="legal-modal-icon">
                            ✓
                        </div>

                        <h2>
                            Complete Legal Perfection?
                        </h2>

                        <p>
                            You are about to complete
                            Legal Perfection for
                            <strong>
                                {" "}
                                {
                                    collateral
                                        .loanContractId
                                }
                            </strong>.
                        </p>

                        <p className="legal-modal-note">
                            The collateral will move
                            immediately to the Insurance
                            stage.
                        </p>

                        <div className="legal-modal-actions">

                            <button
                                type="button"
                                className="modal-cancel"
                                disabled={submitting}
                                onClick={() =>
                                    setShowConfirm(false)
                                }
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                className="modal-confirm"
                                disabled={submitting}
                                onClick={
                                    handleComplete
                                }
                            >
                                {
                                    submitting
                                        ? "Completing..."
                                        : "Yes, Complete Perfection"
                                }
                            </button>

                        </div>

                    </div>

                </div>

            )}

        </div>
    );
}

function Info({
    label,
    value
}: {
    label: string;
    value: string | null | undefined;
}) {

    return (
        <div className="legal-info-item">

            <span>
                {label}
            </span>

            <strong>
                {value || "-"}
            </strong>

        </div>
    );
}