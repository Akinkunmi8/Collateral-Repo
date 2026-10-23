import {
    useCallback,
    useEffect,
    useState,
} from "react";

import { Link } from "react-router-dom";

import {
    getValuationQueue,
    resubmitValuation,
} from "../api/valuation";

import type {
    PagedResult,
    ValuationQueueItem,
} from "../types/valuation";

import "./valuation.css";

const PAGE_SIZE = 20;

type ValuationTab = "pending" | "additional";

function formatEnumLabel(value?: string | null) {
    if (!value) return "—";

    return value
        .replace(/([a-z])([A-Z])/g, "$1 $2")
        .replace(/([A-Z])([A-Z][a-z])/g, "$1 $2")
        .trim();
}

function formatCurrency(
    value: number,
    currency: string
) {
    return new Intl.NumberFormat("en-NG", {
        style: "currency",
        currency: currency || "NGN",
        maximumFractionDigits: 0,
    }).format(value);
}

function formatDate(value?: string | null) {
    if (!value) return "—";

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return "—";
    }

    return new Intl.DateTimeFormat("en-NG", {
        day: "2-digit",
        month: "short",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
    }).format(date);
}

export default function Valuation() {
    const [data, setData] =
        useState<PagedResult<ValuationQueueItem> | null>(null);

    const [activeTab, setActiveTab] =
        useState<ValuationTab>("pending");

    const [searchInput, setSearchInput] =
        useState("");

    const [search, setSearch] =
        useState("");

    const [pageNumber, setPageNumber] =
        useState(1);

    const [loading, setLoading] =
        useState(true);

    const [error, setError] =
        useState("");

    const [successMessage, setSuccessMessage] =
        useState("");

    /*
     * Resubmit modal
     */
    const [selectedCollateral, setSelectedCollateral] =
        useState<ValuationQueueItem | null>(null);

    const [resubmitResponse, setResubmitResponse] =
        useState("");

    const [resubmitting, setResubmitting] =
        useState(false);

    const [resubmitError, setResubmitError] =
        useState("");

    /*
     * Used to force queue refresh after a successful
     * resubmission without duplicating loading logic.
     */
    const [refreshKey, setRefreshKey] =
        useState(0);

    const loadQueue = useCallback(async () => {
        try {
            setLoading(true);
            setError("");

            const status =
                activeTab === "pending"
                    ? "InValuation"
                    : "AdditionalValuationInformationRequested";

            const result = await getValuationQueue({
                search: search || undefined,
                status,
                pageNumber,
                pageSize: PAGE_SIZE,
            });

            setData(result);
        } catch (err) {
            console.error(err);

            setError(
                activeTab === "pending"
                    ? "Unable to load the valuation work queue."
                    : "Unable to load valuation records awaiting additional information."
            );
        } finally {
            setLoading(false);
        }
    }, [
        activeTab,
        search,
        pageNumber,
    ]);

    useEffect(() => {
        void loadQueue();
    }, [
        loadQueue,
        refreshKey,
    ]);

    const handleSearch = (
        event: React.FormEvent
    ) => {
        event.preventDefault();

        setPageNumber(1);
        setSearch(searchInput.trim());
    };

    const handleClearSearch = () => {
        setSearchInput("");
        setSearch("");
        setPageNumber(1);
    };

    const handleTabChange = (
        tab: ValuationTab
    ) => {
        if (tab === activeTab) {
            return;
        }

        setActiveTab(tab);

        setSearchInput("");
        setSearch("");

        setPageNumber(1);

        setError("");
        setSuccessMessage("");
    };

    const handleOpenResubmit = (
        item: ValuationQueueItem
    ) => {
        setSelectedCollateral(item);

        setResubmitResponse("");
        setResubmitError("");
        setSuccessMessage("");
    };

    const handleCloseResubmit = () => {
        if (resubmitting) {
            return;
        }

        setSelectedCollateral(null);
        setResubmitResponse("");
        setResubmitError("");
    };

    const handleResubmit = async () => {
        if (!selectedCollateral) {
            return;
        }

        const response =
            resubmitResponse.trim();

        if (!response) {
            setResubmitError(
                "Please provide a response before resubmitting the valuation."
            );

            return;
        }

        try {
            setResubmitting(true);
            setResubmitError("");

            await resubmitValuation(
                selectedCollateral.id,
                {
                    response,
                }
            );

            const loanContractId =
                selectedCollateral.loanContractId;

            setSelectedCollateral(null);
            setResubmitResponse("");

            /*
             * After successful resubmission the backend
             * changes the record back to InValuation.
             */
            setSuccessMessage(
                `${loanContractId} has been resubmitted for valuation review.`
            );

            /*
             * Stay on the Additional Information tab.
             * The record will disappear from this queue
             * because its status is now InValuation.
             */
            setPageNumber(1);
            setRefreshKey((value) => value + 1);
        } catch (err) {
            console.error(err);

            setResubmitError(
                "Unable to resubmit the valuation. Please try again."
            );
        } finally {
            setResubmitting(false);
        }
    };

    const isAdditionalTab =
        activeTab === "additional";

    const queueTitle =
        isAdditionalTab
            ? "Additional Information Requested"
            : "Pending Valuation";

    const emptyTitle =
        isAdditionalTab
            ? "No valuation awaiting resubmission"
            : "No collateral awaiting valuation";

    const emptyDescription =
        isAdditionalTab
            ? "There are currently no valuation records waiting for additional information to be resubmitted."
            : "There are currently no collateral records requiring valuation review.";

    return (
        <div className="valuation-page">
            <div className="valuation-page-header">
                <div>
                    <p className="page-eyebrow">
                        Collateral Management
                    </p>

                    <h1>
                        Valuation Work Queue
                    </h1>

                    <p className="page-description">
                        Review collateral valuation,
                        manage information requests and
                        resubmit updated valuation reports
                        for approval.
                    </p>
                </div>

                <div className="queue-count">
                    <span>{queueTitle}</span>

                    <strong>
                        {data?.totalCount ?? 0}
                    </strong>
                </div>
            </div>

            <div className="valuation-card">

                {/* Queue tabs */}

                <div className="valuation-tabs">
                    <button
                        type="button"
                        className={
                            activeTab === "pending"
                                ? "valuation-tab active"
                                : "valuation-tab"
                        }
                        onClick={() =>
                            handleTabChange("pending")
                        }
                    >
                        Pending Review
                    </button>

                    <button
                        type="button"
                        className={
                            activeTab === "additional"
                                ? "valuation-tab active"
                                : "valuation-tab"
                        }
                        onClick={() =>
                            handleTabChange("additional")
                        }
                    >
                        Additional Information Requested
                    </button>
                </div>

                {/* Search */}

                <div className="queue-toolbar">
                    <form
                        className="queue-search"
                        onSubmit={handleSearch}
                    >
                        <input
                            value={searchInput}
                            onChange={(event) =>
                                setSearchInput(
                                    event.target.value
                                )
                            }
                            placeholder="Search loan, customer, CIF or account number"
                        />

                        <button type="submit">
                            Search
                        </button>
                    </form>

                    {search && (
                        <button
                            type="button"
                            className="clear-filter"
                            onClick={handleClearSearch}
                        >
                            Clear search
                        </button>
                    )}
                </div>

                {/* Messages */}

                {successMessage && (
                    <div className="valuation-success">
                        {successMessage}
                    </div>
                )}

                {error && (
                    <div className="valuation-error">
                        {error}
                    </div>
                )}

                {/* Queue */}

                {loading ? (
                    <div className="valuation-loading">
                        Loading valuation queue...
                    </div>
                ) : !data ||
                    data.items.length === 0 ? (
                    <div className="valuation-empty">
                        <h3>
                            {emptyTitle}
                        </h3>

                        <p>
                            {emptyDescription}
                        </p>
                    </div>
                ) : (
                    <>
                        <div className="table-responsive">
                            <table className="valuation-table">
                                <thead>
                                    <tr>
                                        <th>
                                            Loan / Customer
                                        </th>

                                        <th>
                                            Collateral
                                        </th>

                                        <th>
                                            Approved Limit
                                        </th>

                                        <th>
                                            Location
                                        </th>

                                        <th>
                                            TAT
                                        </th>

                                        <th>
                                            Status
                                        </th>

                                        <th>
                                            Action
                                        </th>
                                    </tr>
                                </thead>

                                <tbody>
                                    {data.items.map(
                                        (item) => (
                                            <tr key={item.id}>
                                                <td>
                                                    <strong>
                                                        {
                                                            item.loanContractId
                                                        }
                                                    </strong>

                                                    <span>
                                                        {
                                                            item.customerName
                                                        }
                                                    </span>

                                                    <small>
                                                        {
                                                            item.accountNumber
                                                        }
                                                    </small>
                                                </td>

                                                <td>
                                                    <strong>
                                                        {formatEnumLabel(
                                                            item.type
                                                        )}
                                                    </strong>

                                                    <span>
                                                        {
                                                            item.description
                                                        }
                                                    </span>
                                                </td>

                                                <td>
                                                    <strong>
                                                        {formatCurrency(
                                                            item.approvedLimit,
                                                            item.currency
                                                        )}
                                                    </strong>

                                                    <span>
                                                        {item.facilityType ||
                                                            "—"}
                                                    </span>
                                                </td>

                                                <td>
                                                    <strong>
                                                        {formatEnumLabel(
                                                            item.region
                                                        )}
                                                    </strong>

                                                    <span>
                                                        {item.collateralAddress ||
                                                            "—"}
                                                    </span>
                                                </td>

                                                <td>
                                                    {item.tatStatus ? (
                                                        <>
                                                            <span
                                                                className={
                                                                    item.tatStatus ===
                                                                        "BeyondTat"
                                                                        ? "tat-badge tat-beyond"
                                                                        : "tat-badge tat-within"
                                                                }
                                                            >
                                                                {item.tatStatus ===
                                                                    "BeyondTat"
                                                                    ? "Beyond TAT"
                                                                    : "Within TAT"}
                                                            </span>

                                                            <small>
                                                                {item.tatElapsedHours?.toFixed(
                                                                    1
                                                                ) ??
                                                                    "0"}{" "}
                                                                /{" "}
                                                                {item.tatAllowedHours ??
                                                                    "—"}{" "}
                                                                hrs
                                                            </small>

                                                            <small>
                                                                Due{" "}
                                                                {formatDate(
                                                                    item.tatDueAtUtc
                                                                )}
                                                            </small>
                                                        </>
                                                    ) : (
                                                        <span>
                                                            —
                                                        </span>
                                                    )}
                                                </td>

                                                <td>
                                                    <span
                                                        className={
                                                            isAdditionalTab
                                                                ? "status-badge status-information-requested"
                                                                : "status-badge"
                                                        }
                                                    >
                                                        {isAdditionalTab
                                                            ? "Information Requested"
                                                            : "In Valuation"}
                                                    </span>
                                                </td>

                                                <td>
                                                    {isAdditionalTab ? (
                                                        <button
                                                            type="button"
                                                            className="resubmit-valuation-button"
                                                            onClick={() =>
                                                                handleOpenResubmit(
                                                                    item
                                                                )
                                                            }
                                                        >
                                                            Resubmit
                                                        </button>
                                                    ) : (
                                                        <Link
                                                            className="view-valuation-button"
                                                            to={`/valuation/${item.id}`}
                                                        >
                                                            View Valuation
                                                        </Link>
                                                    )}
                                                </td>
                                            </tr>
                                        )
                                    )}
                                </tbody>
                            </table>
                        </div>

                        <div className="queue-pagination">
                            <span>
                                Page {data.pageNumber} of{" "}
                                {Math.max(
                                    data.totalPages,
                                    1
                                )}
                            </span>

                            <div>
                                <button
                                    type="button"
                                    disabled={
                                        !data.hasPreviousPage
                                    }
                                    onClick={() =>
                                        setPageNumber(
                                            (page) =>
                                                Math.max(
                                                    page - 1,
                                                    1
                                                )
                                        )
                                    }
                                >
                                    Previous
                                </button>

                                <button
                                    type="button"
                                    disabled={
                                        !data.hasNextPage
                                    }
                                    onClick={() =>
                                        setPageNumber(
                                            (page) =>
                                                page + 1
                                        )
                                    }
                                >
                                    Next
                                </button>
                            </div>
                        </div>
                    </>
                )}
            </div>

            {/* Resubmit modal */}

            {selectedCollateral && (
                <div
                    className="valuation-modal-backdrop"
                    onMouseDown={(event) => {
                        if (
                            event.target ===
                            event.currentTarget
                        ) {
                            handleCloseResubmit();
                        }
                    }}
                >
                    <div
                        className="valuation-modal"
                        role="dialog"
                        aria-modal="true"
                        aria-labelledby="resubmit-valuation-title"
                    >
                        <div className="valuation-modal-header">
                            <div>
                                <span className="valuation-modal-eyebrow">
                                    Valuation Review
                                </span>

                                <h2 id="resubmit-valuation-title">
                                    Resubmit Valuation
                                </h2>
                            </div>

                            <button
                                type="button"
                                className="valuation-modal-close"
                                onClick={
                                    handleCloseResubmit
                                }
                                disabled={resubmitting}
                                aria-label="Close"
                            >
                                ×
                            </button>
                        </div>

                        <div className="valuation-modal-body">
                            <div className="resubmit-record">
                                <div>
                                    <span>
                                        Loan Contract
                                    </span>

                                    <strong>
                                        {
                                            selectedCollateral.loanContractId
                                        }
                                    </strong>
                                </div>

                                <div>
                                    <span>
                                        Customer
                                    </span>

                                    <strong>
                                        {
                                            selectedCollateral.customerName
                                        }
                                    </strong>
                                </div>

                                <div>
                                    <span>
                                        Account
                                    </span>

                                    <strong>
                                        {
                                            selectedCollateral.accountNumber
                                        }
                                    </strong>
                                </div>
                            </div>

                            <p className="resubmit-description">
                                Confirm that the requested
                                valuation information has
                                been updated in the valuation
                                system before resubmitting
                                this collateral for review.
                            </p>

                            <label
                                className="resubmit-field"
                                htmlFor="valuation-response"
                            >
                                <span>
                                    Response
                                </span>

                                <textarea
                                    id="valuation-response"
                                    rows={5}
                                    value={
                                        resubmitResponse
                                    }
                                    onChange={(event) => {
                                        setResubmitResponse(
                                            event.target.value
                                        );

                                        if (
                                            resubmitError
                                        ) {
                                            setResubmitError(
                                                ""
                                            );
                                        }
                                    }}
                                    placeholder="Example: Updated valuation report and supporting information have been provided."
                                    disabled={
                                        resubmitting
                                    }
                                    maxLength={1000}
                                />

                                <small>
                                    {
                                        resubmitResponse.length
                                    }
                                    /1000
                                </small>
                            </label>

                            {resubmitError && (
                                <div className="valuation-modal-error">
                                    {resubmitError}
                                </div>
                            )}
                        </div>

                        <div className="valuation-modal-footer">
                            <button
                                type="button"
                                className="modal-cancel-button"
                                onClick={
                                    handleCloseResubmit
                                }
                                disabled={resubmitting}
                            >
                                Cancel
                            </button>

                            <button
                                type="button"
                                className="modal-submit-button"
                                onClick={() =>
                                    void handleResubmit()
                                }
                                disabled={
                                    resubmitting ||
                                    !resubmitResponse.trim()
                                }
                            >
                                {resubmitting
                                    ? "Resubmitting..."
                                    : "Resubmit Valuation"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}