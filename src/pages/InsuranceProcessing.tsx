import {
    ChangeEvent,
    FormEvent,
    useEffect,
    useMemo,
    useState
} from "react";

import {
    useNavigate,
    useParams
} from "react-router-dom";

import {
    completeInsurance,
    downloadInsuranceReport,
    getInsuranceDocuments,
    getInsuranceWorkItem,
    uploadInsuranceReport
} from "../api/insurance";

import {
    CollateralDocumentType,
    InsuranceRenewalCadence
} from "../types/insurance";

import type {
    CollateralDocument,
    InsuranceWorkItem,
    SubmitInsuranceRequest
} from "../types/insurance";

import "./insurance-processing.css";
export default function InsuranceProcessing() {

    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [workItem, setWorkItem] =
        useState<InsuranceWorkItem | null>(null);

    const [documents, setDocuments] =
        useState<CollateralDocument[]>([]);

    const [loading, setLoading] =
        useState(true);

    const [submitting, setSubmitting] =
        useState(false);

    const [uploading, setUploading] =
        useState(false);

    const [error, setError] =
        useState<string | null>(null);

    const [success, setSuccess] =
        useState<string | null>(null);

    const [showConfirmModal, setShowConfirmModal] =
        useState(false);

    const [selectedFile, setSelectedFile] =
        useState<File | null>(null);

    const [policyNumber, setPolicyNumber] =
        useState("");

    const [insurer, setInsurer] =
        useState("");

    const [coverageAmount, setCoverageAmount] =
        useState("");

    const [premiumAmount, setPremiumAmount] =
        useState("");

    const [policyStartDate, setPolicyStartDate] =
        useState("");

    const [renewalCadence, setRenewalCadence] =
        useState<InsuranceRenewalCadence>(
            InsuranceRenewalCadence.Annual
        );

    useEffect(() => {

        if (!id) {
            setError("Collateral ID was not provided.");
            setLoading(false);
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
                item,
                documentList
            ] = await Promise.all([
                getInsuranceWorkItem(id),
                getInsuranceDocuments(id)
            ]);

            setWorkItem(item);
            setDocuments(documentList);

            /*
             * Normally Insurance will be null because this
             * collateral has just arrived from Legal.
             *
             * This also supports loading an existing Insurance
             * record should that become necessary later.
             */
            if (item.insurance) {

                setPolicyNumber(
                    item.insurance.policyNumber ?? ""
                );

                setInsurer(
                    item.insurance.insurer ?? ""
                );

                setCoverageAmount(
                    item.insurance.coverageAmount?.toString()
                    ?? ""
                );

                setPremiumAmount(
                    item.insurance.premiumAmount?.toString()
                    ?? ""
                );

                if (item.insurance.policyStartDate) {
                    setPolicyStartDate(
                        item.insurance.policyStartDate
                            .split("T")[0]
                    );
                }

                /*
                 * Backend work-item currently exposes
                 * RenewalCadence as a string.
                 */
                if (
                    item.insurance.renewalCadence ===
                    "ThreeYearly"
                ) {
                    setRenewalCadence(
                        InsuranceRenewalCadence.ThreeYearly
                    );
                }
                else {
                    setRenewalCadence(
                        InsuranceRenewalCadence.Annual
                    );
                }
            }

        }
        catch (err) {

            console.error(err);

            setError(
                getApiErrorMessage(
                    err,
                    "Unable to load the Insurance work item."
                )
            );

        }
        finally {

            setLoading(false);
        }
    }

    const insuranceReports = useMemo(
        () =>
            documents.filter(
                x =>
                    x.documentType ===
                    CollateralDocumentType.InsuranceReport
            ),
        [documents]
    );

    const calculatedExpiryDate = useMemo(
        () =>
            calculateExpiryDate(
                policyStartDate,
                renewalCadence
            ),
        [
            policyStartDate,
            renewalCadence
        ]
    );

    function handleFileChange(
        event: ChangeEvent<HTMLInputElement>
    ) {

        setError(null);
        setSuccess(null);

        const file =
            event.target.files?.[0] ?? null;

        if (!file) {
            setSelectedFile(null);
            return;
        }

        if (
            file.type !== "application/pdf" &&
            !file.name.toLowerCase().endsWith(".pdf")
        ) {
            setSelectedFile(null);

            setError(
                "Only PDF files are allowed for the Insurance report."
            );

            event.target.value = "";

            return;
        }

        /*
         * Backend request limit is currently 10 MB.
         */
        if (file.size > 10_000_000) {

            setSelectedFile(null);

            setError(
                "The Insurance report cannot exceed 10 MB."
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

            await uploadInsuranceReport(
                id,
                selectedFile
            );

            const documentList =
                await getInsuranceDocuments(id);

            setDocuments(documentList);

            setSelectedFile(null);

            setSuccess(
                "Insurance report uploaded successfully."
            );

        }
        catch (err) {

            console.error(err);

            setError(
                getApiErrorMessage(
                    err,
                    "Unable to upload the Insurance report."
                )
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
                await downloadInsuranceReport(
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
             * Give the new browser tab enough time
             * to obtain the object URL.
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
                getApiErrorMessage(
                    err,
                    "Unable to open the Insurance report."
                )
            );
        }
    }

    function handleSubmit(
        event: FormEvent
    ) {

        event.preventDefault();

        setError(null);
        setSuccess(null);

        if (!policyNumber.trim()) {
            setError(
                "Policy number is required."
            );
            return;
        }

        if (!insurer.trim()) {
            setError(
                "Insurer is required."
            );
            return;
        }

        const coverage =
            Number(coverageAmount);

        if (
            !coverageAmount ||
            !Number.isFinite(coverage) ||
            coverage <= 0
        ) {
            setError(
                "Enter a valid coverage amount greater than zero."
            );
            return;
        }

        const premium =
            Number(premiumAmount);

        if (
            premiumAmount === "" ||
            !Number.isFinite(premium) ||
            premium < 0
        ) {
            setError(
                "Enter a valid premium amount."
            );
            return;
        }

        if (!policyStartDate) {
            setError(
                "Policy start date is required."
            );
            return;
        }

        /*
         * The backend also validates this.
         * Keeping the check here improves UX.
         */
        if (
            insuranceReports.length === 0 &&
            !selectedFile
        ) {
            setError(
                "Upload an Insurance report or certificate before completing Insurance."
            );
            return;
        }

        /*
         * If a file has only been selected but has not
         * yet been uploaded, upload it before displaying
         * the final completion modal.
         */
        if (
            insuranceReports.length === 0 &&
            selectedFile
        ) {
            setError(
                "The selected Insurance report has not been uploaded yet. Click Upload Report first."
            );
            return;
        }

        setShowConfirmModal(true);
    }

    async function confirmCompletion() {

        if (!id) {
            return;
        }

        try {

            setSubmitting(true);
            setError(null);

            const request:
                SubmitInsuranceRequest = {

                policyNumber:
                    policyNumber.trim(),

                insurer:
                    insurer.trim(),

                coverageAmount:
                    Number(coverageAmount),

                premiumAmount:
                    Number(premiumAmount),

                /*
                 * Send a valid DateTime value to ASP.NET.
                 * Backend will normalize using .Date.
                 */
                policyStartDate:
                    `${policyStartDate}T00:00:00`,

                renewalCadence
            };

            await completeInsurance(
                id,
                request
            );

            setShowConfirmModal(false);

            /*
             * Once complete, this collateral no longer
             * belongs in the Insurance queue because the
             * backend moves it to SystemHold / OnHold.
             */
            navigate(
                "/insurance",
                {
                    replace: true,
                    state: {
                        message:
                            "Insurance completed successfully. Collateral has been placed on System Hold."
                    }
                }
            );

        }
        catch (err) {

            console.error(err);

            setShowConfirmModal(false);

            setError(
                getApiErrorMessage(
                    err,
                    "Unable to complete Insurance."
                )
            );

        }
        finally {

            setSubmitting(false);
        }
    }

    if (loading) {
        return (
            <div className="insurance-processing-page">
                <div className="insurance-loading">
                    Loading Insurance work item...
                </div>
            </div>
        );
    }

    if (!workItem) {
        return (
            <div className="insurance-processing-page">

                <button
                    className="insurance-back-link"
                    onClick={() =>
                        navigate("/insurance")
                    }
                >
                    ← Back to Insurance Queue
                </button>

                <div className="insurance-page-error">
                    {error ??
                        "Insurance work item was not found."}
                </div>

            </div>
        );
    }

    const collateral =
        workItem.collateralData;

    const valuation =
        workItem.valuation;

    const perfection =
        workItem.perfection;

    return (
        <div className="insurance-processing-page">

            <button
                type="button"
                className="insurance-back-link"
                onClick={() =>
                    navigate("/insurance")
                }
            >
                ← Back to Insurance Queue
            </button>

            <div className="insurance-processing-header">

                <div>
                    <h1>
                        Insurance Processing
                    </h1>

                    <p>
                        {collateral.loanContractId}
                        {" • "}
                        {collateral.customerName}
                    </p>
                </div>

                <div className="insurance-header-badges">

                    <span className="insurance-stage-badge">
                        {workItem.currentStage}
                    </span>

                    <span className="insurance-status-badge">
                        {workItem.status}
                    </span>

                </div>

            </div>

            {error && (
                <div className="insurance-page-error">
                    {error}
                </div>
            )}

            {success && (
                <div className="insurance-page-success">
                    {success}
                </div>
            )}

            <form onSubmit={handleSubmit}>

                {/* -----------------------------------
                    01. Collateral Information
                   ----------------------------------- */}

                <section className="insurance-section">

                    <SectionHeader
                        number="01"
                        title="Collateral Information"
                        badge="Read Only"
                    />

                    <div className="insurance-detail-grid">

                        <ReadOnlyField
                            label="Loan Contract"
                            value={
                                collateral.loanContractId
                            }
                        />

                        <ReadOnlyField
                            label="Account Number"
                            value={
                                collateral.accountNumber
                            }
                        />

                        <ReadOnlyField
                            label="Customer"
                            value={
                                collateral.customerName
                            }
                        />

                        <ReadOnlyField
                            label="CIF ID"
                            value={
                                collateral.cifId
                            }
                        />

                        <ReadOnlyField
                            label="Branch"
                            value={
                                collateral.branchName
                            }
                        />

                        <ReadOnlyField
                            label="Facility Type"
                            value={
                                collateral.facilityType
                            }
                        />

                        <ReadOnlyField
                            label="Approved Limit"
                            value={
                                formatMoney(
                                    collateral.approvedLimit,
                                    collateral.currency
                                )
                            }
                        />

                        <ReadOnlyField
                            label="Collateral Type"
                            value={
                                collateral.collateralType
                            }
                        />

                        <ReadOnlyField
                            label="Region"
                            value={
                                collateral.region
                            }
                        />

                        <ReadOnlyField
                            label="Collateral Owner"
                            value={
                                collateral.collateralOwner
                            }
                        />

                        <ReadOnlyField
                            label="Relationship Manager"
                            value={
                                collateral.relationshipManagerName
                            }
                        />

                        <ReadOnlyField
                            label="Branch Manager"
                            value={
                                collateral.branchManagerName
                            }
                        />

                        <div className="insurance-grid-full">

                            <ReadOnlyField
                                label="Collateral Description"
                                value={
                                    collateral.collateralDescription
                                }
                            />

                        </div>

                        <div className="insurance-grid-full">

                            <ReadOnlyField
                                label="Collateral Address"
                                value={
                                    collateral.collateralAddress
                                }
                            />

                        </div>

                    </div>

                </section>

                {/* -----------------------------------
                    02. Valuation Summary
                   ----------------------------------- */}

                <section className="insurance-section">

                    <SectionHeader
                        number="02"
                        title="Valuation Summary"
                        badge={
                            valuation
                                ? "Completed"
                                : "Not Available"
                        }
                    />

                    {valuation ? (

                        <div className="insurance-detail-grid">

                            <ReadOnlyField
                                label="Open Market Value"
                                value={
                                    formatMoney(
                                        valuation.openMarketValue,
                                        collateral.currency
                                    )
                                }
                            />

                            <ReadOnlyField
                                label="Forced Sale Value"
                                value={
                                    formatMoney(
                                        valuation.forcedSaleValue,
                                        collateral.currency
                                    )
                                }
                            />

                            <ReadOnlyField
                                label="Valuer"
                                value={
                                    valuation.valuersName
                                }
                            />

                            <ReadOnlyField
                                label="Valuation Date"
                                value={
                                    formatDate(
                                        valuation.valuationDate
                                    )
                                }
                            />

                            <ReadOnlyField
                                label="Actual Coverage"
                                value={
                                    `${valuation.actualCoverageRatioPercent.toFixed(2)}%`
                                }
                            />

                            <ReadOnlyField
                                label="Required Coverage"
                                value={
                                    `${valuation.requiredCoverageRatioPercent.toFixed(2)}%`
                                }
                            />

                            <ReadOnlyField
                                label="Coverage Assessment"
                                value={
                                    valuation.coverageAdequate
                                        ? "Adequate"
                                        : "Below Required Coverage"
                                }
                                status={
                                    valuation.coverageAdequate
                                        ? "success"
                                        : "warning"
                                }
                            />

                            <ReadOnlyField
                                label="Decision"
                                value={
                                    valuation.decision
                                }
                            />

                            <div className="insurance-grid-full">

                                <ReadOnlyField
                                    label="Valuation Remark"
                                    value={
                                        valuation.remark
                                    }
                                />

                            </div>

                        </div>

                    ) : (

                        <div className="insurance-empty-section">
                            No valuation information is available.
                        </div>

                    )}

                </section>

                {/* -----------------------------------
                    03. Legal Perfection
                   ----------------------------------- */}

                <section className="insurance-section">

                    <SectionHeader
                        number="03"
                        title="Legal Perfection"
                        badge={
                            perfection
                                ? "Completed"
                                : "Not Available"
                        }
                    />

                    {perfection ? (

                        <div className="insurance-detail-grid">

                            <ReadOnlyField
                                label="Registration Reference"
                                value={
                                    perfection.registrationReference
                                }
                            />

                            <ReadOnlyField
                                label="Legal Unit"
                                value={
                                    perfection.legalUnit
                                }
                            />

                            <ReadOnlyField
                                label="Perfection Status"
                                value={
                                    perfection.stage
                                }
                                status="success"
                            />

                            <ReadOnlyField
                                label="Collateral Valid"
                                value={
                                    perfection.collateralValid
                                        ? "Yes"
                                        : "No"
                                }
                                status={
                                    perfection.collateralValid
                                        ? "success"
                                        : "warning"
                                }
                            />

                            {perfection.invalidReason && (

                                <div className="insurance-grid-full">

                                    <ReadOnlyField
                                        label="Invalid Reason"
                                        value={
                                            perfection.invalidReason
                                        }
                                    />

                                </div>

                            )}

                        </div>

                    ) : (

                        <div className="insurance-empty-section">
                            No Legal Perfection information is available.
                        </div>

                    )}

                </section>

                {/* -----------------------------------
                    04. Insurance Details
                   ----------------------------------- */}

                <section className="insurance-section">

                    <SectionHeader
                        number="04"
                        title="Insurance Details"
                        badge="Required"
                    />

                    <div className="insurance-form-grid">

                        <div className="insurance-form-field">

                            <label htmlFor="policyNumber">
                                Policy Number
                                <span>*</span>
                            </label>

                            <input
                                id="policyNumber"
                                type="text"
                                maxLength={100}
                                value={policyNumber}
                                onChange={e =>
                                    setPolicyNumber(
                                        e.target.value
                                    )
                                }
                                placeholder=
                                "Enter policy number"
                            />

                        </div>

                        <div className="insurance-form-field">

                            <label htmlFor="insurer">
                                Insurer
                                <span>*</span>
                            </label>

                            <input
                                id="insurer"
                                type="text"
                                maxLength={150}
                                value={insurer}
                                onChange={e =>
                                    setInsurer(
                                        e.target.value
                                    )
                                }
                                placeholder=
                                "Enter insurance company"
                            />

                        </div>

                        <div className="insurance-form-field">

                            <label htmlFor="coverageAmount">
                                Coverage Amount
                                <span>*</span>
                            </label>

                            <div className="insurance-money-input">

                                <span>
                                    {collateral.currency}
                                </span>

                                <input
                                    id="coverageAmount"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={coverageAmount}
                                    onChange={e =>
                                        setCoverageAmount(
                                            e.target.value
                                        )
                                    }
                                    placeholder="0.00"
                                />

                            </div>

                        </div>

                        <div className="insurance-form-field">

                            <label htmlFor="premiumAmount">
                                Premium Amount
                                <span>*</span>
                            </label>

                            <div className="insurance-money-input">

                                <span>
                                    {collateral.currency}
                                </span>

                                <input
                                    id="premiumAmount"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={premiumAmount}
                                    onChange={e =>
                                        setPremiumAmount(
                                            e.target.value
                                        )
                                    }
                                    placeholder="0.00"
                                />

                            </div>

                        </div>

                        <div className="insurance-form-field">

                            <label htmlFor="policyStartDate">
                                Policy Start Date
                                <span>*</span>
                            </label>

                            <input
                                id="policyStartDate"
                                type="date"
                                value={policyStartDate}
                                onChange={e =>
                                    setPolicyStartDate(
                                        e.target.value
                                    )
                                }
                            />

                        </div>

                        <div className="insurance-form-field">

                            <label htmlFor="renewalCadence">
                                Renewal Cadence
                                <span>*</span>
                            </label>

                            <select
                                id="renewalCadence"
                                value={renewalCadence}
                                onChange={e =>
                                    setRenewalCadence(
                                        Number(
                                            e.target.value
                                        ) as
                                        InsuranceRenewalCadence
                                    )
                                }
                            >

                                <option
                                    value={
                                        InsuranceRenewalCadence.Annual
                                    }
                                >
                                    Annual
                                </option>

                                <option
                                    value={
                                        InsuranceRenewalCadence.ThreeYearly
                                    }
                                >
                                    Every 3 Years
                                </option>

                            </select>

                        </div>

                        <div className="insurance-form-field">

                            <label>
                                Calculated Expiry Date
                            </label>

                            <div className="insurance-calculated-value">
                                {
                                    calculatedExpiryDate
                                        ? formatDateOnly(
                                            calculatedExpiryDate
                                        )
                                        : "Select policy start date"
                                }
                            </div>

                            <small>
                                Automatically calculated from
                                the renewal cadence.
                            </small>

                        </div>

                    </div>

                </section>

                {/* -----------------------------------
                    05. Insurance Report
                   ----------------------------------- */}

                <section className="insurance-section">

                    <SectionHeader
                        number="05"
                        title="Insurance Report / Certificate"
                        badge="Required"
                    />

                    <div className="insurance-upload-area">

                        <div className="insurance-upload-icon">
                            ↑
                        </div>

                        <div>
                            <strong>
                                Upload Insurance Report
                            </strong>

                            <p>
                                PDF only. Maximum file size
                                10 MB.
                            </p>
                        </div>

                        <input
                            id="insurance-report"
                            className="insurance-file-input"
                            type="file"
                            accept=".pdf,application/pdf"
                            onChange={handleFileChange}
                        />

                        <label
                            htmlFor="insurance-report"
                            className="insurance-choose-file"
                        >
                            Choose PDF
                        </label>

                    </div>

                    {selectedFile && (

                        <div className="insurance-selected-file">

                            <div>
                                <strong>
                                    {selectedFile.name}
                                </strong>

                                <span>
                                    {formatFileSize(
                                        selectedFile.size
                                    )}
                                </span>
                            </div>

                            <button
                                type="button"
                                onClick={handleUpload}
                                disabled={uploading}
                            >
                                {
                                    uploading
                                        ? "Uploading..."
                                        : "Upload Report"
                                }
                            </button>

                        </div>

                    )}

                    {insuranceReports.length > 0 && (

                        <div className="insurance-document-list">

                            <div className="insurance-document-list-title">
                                Uploaded Insurance Report
                            </div>

                            {insuranceReports.map(
                                document => (

                                    <div
                                        className="insurance-document-row"
                                        key={document.id}
                                    >

                                        <div className="insurance-document-icon">
                                            PDF
                                        </div>

                                        <div className="insurance-document-info">

                                            <strong>
                                                {document.fileName}
                                            </strong>

                                            <span>
                                                Uploaded by{" "}
                                                {
                                                    document.uploadedBy ||
                                                    "Unknown"
                                                }
                                            </span>

                                        </div>

                                        <button
                                            type="button"
                                            onClick={() =>
                                                handleViewReport(
                                                    document
                                                )
                                            }
                                        >
                                            View Report
                                        </button>

                                    </div>

                                )
                            )}

                        </div>

                    )}

                </section>

                {/* -----------------------------------
                    Final action
                   ----------------------------------- */}

                <div className="insurance-action-bar">

                    <div>

                        <strong>
                            Ready to complete Insurance?
                        </strong>

                        <p>
                            Completion will move this
                            collateral to System Hold.
                        </p>

                    </div>

                    <button
                        type="submit"
                        className="insurance-complete-btn"
                        disabled={
                            submitting ||
                            uploading
                        }
                    >
                        Complete Insurance
                    </button>

                </div>

            </form>

            {/* ---------------------------------------
                Confirmation Modal
               --------------------------------------- */}

            {showConfirmModal && (

                <div
                    className="insurance-modal-backdrop"
                    role="presentation"
                    onMouseDown={e => {

                        if (
                            e.target ===
                            e.currentTarget &&
                            !submitting
                        ) {
                            setShowConfirmModal(false);
                        }
                    }}
                >

                    <div
                        className="insurance-confirm-modal"
                        role="dialog"
                        aria-modal="true"
                        aria-labelledby=
                        "insurance-confirm-title"
                    >

                        <div className="insurance-confirm-icon">
                            ✓
                        </div>

                        <h2 id="insurance-confirm-title">
                            Complete Insurance?
                        </h2>

                        <p>
                            You are about to complete
                            Insurance processing for
                            <strong>
                                {" "}
                                {collateral.loanContractId}
                            </strong>.
                        </p>

                        <div className="insurance-confirm-summary">

                            <div>
                                <span>
                                    Policy Number
                                </span>

                                <strong>
                                    {policyNumber}
                                </strong>
                            </div>

                            <div>
                                <span>
                                    Insurer
                                </span>

                                <strong>
                                    {insurer}
                                </strong>
                            </div>

                            <div>
                                <span>
                                    Policy Expiry
                                </span>

                                <strong>
                                    {
                                        formatDateOnly(
                                            calculatedExpiryDate
                                        )
                                    }
                                </strong>
                            </div>

                        </div>

                        <div className="insurance-modal-notice">
                            After completion, the collateral
                            will move to{" "}
                            <strong>
                                System Hold / On Hold
                            </strong>.
                        </div>

                        <div className="insurance-modal-actions">

                            <button
                                type="button"
                                className="insurance-modal-cancel"
                                disabled={submitting}
                                onClick={() =>
                                    setShowConfirmModal(
                                        false
                                    )
                                }
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                className="insurance-modal-confirm"
                                disabled={submitting}
                                onClick={
                                    confirmCompletion
                                }
                            >
                                {
                                    submitting
                                        ? "Completing..."
                                        : "Yes, Complete Insurance"
                                }
                            </button>

                        </div>

                    </div>

                </div>

            )}

        </div>
    );
}

/* =====================================================
   Reusable display components
   ===================================================== */

interface SectionHeaderProps {
    number: string;
    title: string;
    badge: string;
}

function SectionHeader({
    number,
    title,
    badge
}: SectionHeaderProps) {

    return (
        <div className="insurance-section-header">

            <div className="insurance-section-title">

                <span className="insurance-section-number">
                    {number}
                </span>

                <h2>
                    {title}
                </h2>

            </div>

            <span className="insurance-section-badge">
                {badge}
            </span>

        </div>
    );
}

interface ReadOnlyFieldProps {
    label: string;
    value:
    | string
    | number
    | null
    | undefined;
    status?:
    | "success"
    | "warning";
}

function ReadOnlyField({
    label,
    value,
    status
}: ReadOnlyFieldProps) {

    const displayValue =
        value === null ||
            value === undefined ||
            value === ""
            ? "-"
            : String(value);

    return (
        <div className="insurance-readonly-field">

            <span>
                {label}
            </span>

            <strong
                className={
                    status
                        ? `insurance-value-${status}`
                        : undefined
                }
            >
                {displayValue}
            </strong>

        </div>
    );
}

/* =====================================================
   Helpers
   ===================================================== */

function calculateExpiryDate(
    startDate: string,
    cadence: InsuranceRenewalCadence
): string {

    if (!startDate) {
        return "";
    }

    /*
     * Parse manually rather than depending on UTC
     * conversion for a date-only value.
     */
    const [
        year,
        month,
        day
    ] = startDate
        .split("-")
        .map(Number);

    const expiry =
        new Date(
            year,
            month - 1,
            day
        );

    if (
        cadence ===
        InsuranceRenewalCadence.ThreeYearly
    ) {
        expiry.setFullYear(
            expiry.getFullYear() + 3
        );
    }
    else {
        expiry.setFullYear(
            expiry.getFullYear() + 1
        );
    }

    const expiryYear =
        expiry.getFullYear();

    const expiryMonth =
        String(
            expiry.getMonth() + 1
        ).padStart(2, "0");

    const expiryDay =
        String(
            expiry.getDate()
        ).padStart(2, "0");

    return `${expiryYear}-${expiryMonth}-${expiryDay}`;
}

function formatMoney(
    amount: number | null,
    currency: string
): string {

    if (
        amount === null ||
        amount === undefined
    ) {
        return "-";
    }

    try {

        return new Intl.NumberFormat(
            "en-NG",
            {
                style: "currency",
                currency,
                maximumFractionDigits: 2
            }
        ).format(amount);

    }
    catch {

        return `${currency} ${amount.toLocaleString()}`;
    }
}

function formatDate(
    value: string | null
): string {

    if (!value) {
        return "-";
    }

    return new Date(value)
        .toLocaleDateString(
            "en-NG",
            {
                day: "2-digit",
                month: "short",
                year: "numeric"
            }
        );
}

function formatDateOnly(
    value: string
): string {

    if (!value) {
        return "-";
    }

    const [
        year,
        month,
        day
    ] = value
        .split("-")
        .map(Number);

    return new Date(
        year,
        month - 1,
        day
    ).toLocaleDateString(
        "en-NG",
        {
            day: "2-digit",
            month: "short",
            year: "numeric"
        }
    );
}

function formatFileSize(
    bytes: number
): string {

    if (bytes < 1024) {
        return `${bytes} B`;
    }

    if (bytes < 1024 * 1024) {
        return `${(
            bytes / 1024
        ).toFixed(1)} KB`;
    }

    return `${(
        bytes /
        (1024 * 1024)
    ).toFixed(1)} MB`;
}

/*
 * Works with Axios-shaped errors without forcing
 * the page to depend on AxiosError.
 */
function getApiErrorMessage(
    error: unknown,
    fallback: string
): string {

    if (
        typeof error === "object" &&
        error !== null &&
        "response" in error
    ) {

        const response =
            (
                error as {
                    response?: {
                        data?: unknown;
                    };
                }
            ).response;

        const data = response?.data;

        if (
            typeof data === "string" &&
            data.trim()
        ) {
            return data;
        }

        if (
            typeof data === "object" &&
            data !== null
        ) {

            const object =
                data as Record<
                    string,
                    unknown
                >;

            const message =
                object.message ??
                object.detail ??
                object.title;

            if (
                typeof message === "string" &&
                message.trim()
            ) {
                return message;
            }
        }
    }

    return fallback;
}