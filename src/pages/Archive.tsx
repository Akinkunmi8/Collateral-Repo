import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
    Archive as ArchiveIcon,
    CheckCircle2,
    ChevronLeft,
    ChevronRight,
    Clock3,
    History,
    Search,
    ShieldCheck
} from "lucide-react";
import { PageTitle, Stat } from "../components/UI";
import { api } from "../api/client";
import "./archive.css";

type ReleaseRow = {
    id: string;
    loanContractId: string;
    customerName: string;
    type: string;
    currentStage: string;
    status: string;
    stageStartedAtUtc: string;
    createdAtUtc: string;
};

type ArchivedRow = {
    id: string;
    loanContractId: string;
    customerName: string;
    accountNumber: string;
    collateralType: string;
    region: string;
    approvedLimit: number;
    currency: string;
    releaseDate: string;
    releasedBy: string;
    loanFullyRepaid: boolean;
    notes: string | null;
};

type PagedResult<T> = {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
};

type Tab = "pending" | "archived";

export default function Archive() {
    const navigate = useNavigate();

    const [tab, setTab] = useState<Tab>("pending");

    const [pendingRows, setPendingRows] =
        useState<ReleaseRow[]>([]);

    const [archivedRows, setArchivedRows] =
        useState<ArchivedRow[]>([]);

    const [pendingCount, setPendingCount] = useState(0);
    const [archivedCount, setArchivedCount] = useState(0);

    const [searchInput, setSearchInput] = useState("");
    const [search, setSearch] = useState("");

    const [pageNumber, setPageNumber] = useState(1);
    const pageSize = 20;

    const [totalPages, setTotalPages] = useState(1);
    const [hasPreviousPage, setHasPreviousPage] =
        useState(false);
    const [hasNextPage, setHasNextPage] =
        useState(false);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const load = useCallback(async () => {
        try {
            setLoading(true);
            setError("");

            if (tab === "pending") {
                const response =
                    await api.get<PagedResult<ReleaseRow>>(
                        "/dashboards/release/queue",
                        {
                            params: {
                                search: search || undefined,
                                pageNumber,
                                pageSize
                            }
                        }
                    );

                setPendingRows(response.data.items ?? []);
                setPendingCount(response.data.totalCount ?? 0);
                setTotalPages(
                    Math.max(response.data.totalPages ?? 1, 1)
                );
                setHasPreviousPage(
                    response.data.hasPreviousPage ?? false
                );
                setHasNextPage(
                    response.data.hasNextPage ?? false
                );
            } else {
                const response =
                    await api.get<PagedResult<ArchivedRow>>(
                        "/dashboards/release/archive",
                        {
                            params: {
                                search: search || undefined,
                                pageNumber,
                                pageSize
                            }
                        }
                    );

                setArchivedRows(response.data.items ?? []);
                setArchivedCount(response.data.totalCount ?? 0);
                setTotalPages(
                    Math.max(response.data.totalPages ?? 1, 1)
                );
                setHasPreviousPage(
                    response.data.hasPreviousPage ?? false
                );
                setHasNextPage(
                    response.data.hasNextPage ?? false
                );
            }
        } catch (err: any) {
            console.error(err);

            setError(
                err?.response?.data?.detail ||
                err?.response?.data?.message ||
                "Unable to load Archive & Release records."
            );
        } finally {
            setLoading(false);
        }
    }, [tab, search, pageNumber]);

    /*
     * Load current tab.
     */
    useEffect(() => {
        load();
    }, [load]);

    /*
     * Load counts independently so both cards always show
     * their correct values.
     */
    useEffect(() => {
        async function loadCounts() {
            try {
                const [pending, archived] =
                    await Promise.all([
                        api.get<PagedResult<ReleaseRow>>(
                            "/dashboards/release/queue",
                            {
                                params: {
                                    pageNumber: 1,
                                    pageSize: 1
                                }
                            }
                        ),

                        api.get<PagedResult<ArchivedRow>>(
                            "/dashboards/release/archive",
                            {
                                params: {
                                    pageNumber: 1,
                                    pageSize: 1
                                }
                            }
                        )
                    ]);

                setPendingCount(
                    pending.data.totalCount ?? 0
                );

                setArchivedCount(
                    archived.data.totalCount ?? 0
                );
            } catch (err) {
                console.error(
                    "Unable to load archive counts",
                    err
                );
            }
        }

        loadCounts();
    }, []);

    function changeTab(nextTab: Tab) {
        setTab(nextTab);
        setPageNumber(1);
        setSearch("");
        setSearchInput("");
        setError("");
    }

    function submitSearch(e: React.FormEvent) {
        e.preventDefault();

        setPageNumber(1);
        setSearch(searchInput.trim());
    }

    function clearSearch() {
        setSearch("");
        setSearchInput("");
        setPageNumber(1);
    }

    function formatDate(value: string) {
        return new Intl.DateTimeFormat("en-NG", {
            day: "2-digit",
            month: "short",
            year: "numeric"
        }).format(new Date(value));
    }

    function money(
        value: number,
        currency = "NGN"
    ) {
        return new Intl.NumberFormat("en-NG", {
            style: "currency",
            currency,
            maximumFractionDigits: 2
        }).format(value);
    }

    const activeCount =
        tab === "pending"
            ? pendingCount
            : archivedCount;

    return (
        <div className="archive-page">
            <PageTitle
                title="Archive & Release"
                subtitle="Manage collateral awaiting release and review completed collateral release records."
            />

            <div className="stats archive-summary-cards">
                <div
                    className={`archive-summary-card ${tab === "pending"
                            ? "archive-summary-active"
                            : ""
                        }`}
                    onClick={() =>
                        changeTab("pending")
                    }
                >
                    <Clock3 size={20} />

                    <div>
                        <span>Awaiting Release</span>
                        <strong>{pendingCount}</strong>
                    </div>
                </div>

                <div
                    className={`archive-summary-card ${tab === "archived"
                            ? "archive-summary-active"
                            : ""
                        }`}
                    onClick={() =>
                        changeTab("archived")
                    }
                >
                    <ArchiveIcon size={20} />

                    <div>
                        <span>Archived Collateral</span>
                        <strong>{archivedCount}</strong>
                    </div>
                </div>
            </div>

            <section className="archive-panel">
                <div className="archive-tabs">
                    <button
                        className={
                            tab === "pending"
                                ? "active"
                                : ""
                        }
                        onClick={() =>
                            changeTab("pending")
                        }
                    >
                        <Clock3 size={16} />
                        Awaiting Release

                        <span>{pendingCount}</span>
                    </button>

                    <button
                        className={
                            tab === "archived"
                                ? "active"
                                : ""
                        }
                        onClick={() =>
                            changeTab("archived")
                        }
                    >
                        <History size={16} />
                        Archived Collateral

                        <span>{archivedCount}</span>
                    </button>
                </div>

                <div className="archive-panel-heading">
                    <div>
                        <div className="archive-heading-icon">
                            {tab === "pending" ? (
                                <ArchiveIcon size={20} />
                            ) : (
                                <History size={20} />
                            )}
                        </div>

                        <div>
                            <h2>
                                {tab === "pending"
                                    ? "System Hold Queue"
                                    : "Archived Collateral"}
                            </h2>

                            <p>
                                {tab === "pending"
                                    ? "Collateral awaiting confirmation of full loan repayment."
                                    : "Completed collateral releases retained as permanent historical records."}
                            </p>
                        </div>
                    </div>

                    <span className="archive-count">
                        {activeCount}{" "}
                        {activeCount === 1
                            ? "record"
                            : "records"}
                    </span>
                </div>

                <form
                    className="archive-toolbar"
                    onSubmit={submitSearch}
                >
                    <div className="archive-search">
                        <Search size={18} />

                        <input
                            value={searchInput}
                            onChange={(e) =>
                                setSearchInput(
                                    e.target.value
                                )
                            }
                            placeholder={
                                tab === "pending"
                                    ? "Search loan contract or customer..."
                                    : "Search loan, customer or account..."
                            }
                        />
                    </div>

                    <button
                        type="submit"
                        className="archive-search-btn"
                    >
                        Search
                    </button>

                    {(search || searchInput) && (
                        <button
                            type="button"
                            className="archive-clear-btn"
                            onClick={clearSearch}
                        >
                            Clear
                        </button>
                    )}
                </form>

                {error && (
                    <div className="archive-error">
                        {error}
                    </div>
                )}

                {tab === "pending" ? (
                    <PendingTable
                        loading={loading}
                        rows={pendingRows}
                        formatDate={formatDate}
                        onReview={(id) =>
                            navigate(`/archive/${id}`)
                        }
                    />
                ) : (
                    <ArchivedTable
                        loading={loading}
                        rows={archivedRows}
                        formatDate={formatDate}
                        money={money}
                        onView={(id) =>
                            navigate(`/archive/${id}`)
                        }
                    />
                )}

                {!loading && activeCount > 0 && (
                    <div className="archive-pagination">
                        <span>
                            Page{" "}
                            <strong>
                                {pageNumber}
                            </strong>{" "}
                            of{" "}
                            <strong>
                                {totalPages}
                            </strong>
                        </span>

                        <div>
                            <button
                                disabled={
                                    !hasPreviousPage
                                }
                                onClick={() =>
                                    setPageNumber(
                                        current =>
                                            Math.max(
                                                current - 1,
                                                1
                                            )
                                    )
                                }
                            >
                                <ChevronLeft size={17} />
                                Previous
                            </button>

                            <button
                                disabled={!hasNextPage}
                                onClick={() =>
                                    setPageNumber(
                                        current =>
                                            current + 1
                                    )
                                }
                            >
                                Next
                                <ChevronRight size={17} />
                            </button>
                        </div>
                    </div>
                )}
            </section>
        </div>
    );
}

function PendingTable({
    loading,
    rows,
    formatDate,
    onReview
}: {
    loading: boolean;
    rows: ReleaseRow[];
    formatDate: (value: string) => string;
    onReview: (id: string) => void;
}) {
    return (
        <div className="archive-table-wrap">
            <table className="archive-table">
                <thead>
                    <tr>
                        <th>Loan Contract</th>
                        <th>Customer</th>
                        <th>Collateral Type</th>
                        <th>Stage</th>
                        <th>Status</th>
                        <th>On Hold Since</th>
                        <th></th>
                    </tr>
                </thead>

                <tbody>
                    {loading ? (
                        <tr>
                            <td colSpan={7}>
                                <div className="archive-empty">
                                    Loading release queue...
                                </div>
                            </td>
                        </tr>
                    ) : rows.length === 0 ? (
                        <tr>
                            <td colSpan={7}>
                                <div className="archive-empty">
                                    <ShieldCheck size={30} />

                                    <strong>
                                        No collateral awaiting release
                                    </strong>

                                    <span>
                                        Collateral placed on System
                                        Hold after insurance completion
                                        will appear here.
                                    </span>
                                </div>
                            </td>
                        </tr>
                    ) : (
                        rows.map(row => (
                            <tr key={row.id}>
                                <td>
                                    <strong className="archive-loan">
                                        {row.loanContractId}
                                    </strong>
                                </td>

                                <td>
                                    {row.customerName}
                                </td>

                                <td>{row.type}</td>

                                <td>
                                    <span className="archive-stage">
                                        System Hold
                                    </span>
                                </td>

                                <td>
                                    <span className="archive-status">
                                        <Clock3 size={14} />
                                        On Hold
                                    </span>
                                </td>

                                <td>
                                    {formatDate(
                                        row.stageStartedAtUtc
                                    )}
                                </td>

                                <td className="archive-action-cell">
                                    <button
                                        className="archive-review-btn"
                                        onClick={() =>
                                            onReview(row.id)
                                        }
                                    >
                                        Review
                                    </button>
                                </td>
                            </tr>
                        ))
                    )}
                </tbody>
            </table>
        </div>
    );
}

function ArchivedTable({
    loading,
    rows,
    formatDate,
    money,
    onView
}: {
    loading: boolean;
    rows: ArchivedRow[];
    formatDate: (value: string) => string;
    money: (
        value: number,
        currency?: string
    ) => string;
    onView: (id: string) => void;
}) {
    return (
        <div className="archive-table-wrap">
            <table className="archive-table archive-history-table">
                <thead>
                    <tr>
                        <th>Loan Contract</th>
                        <th>Customer</th>
                        <th>Collateral</th>
                        <th>Approved Limit</th>
                        <th>Released By</th>
                        <th>Release Date</th>
                        <th>Repayment</th>
                        <th></th>
                    </tr>
                </thead>

                <tbody>
                    {loading ? (
                        <tr>
                            <td colSpan={8}>
                                <div className="archive-empty">
                                    Loading archived collateral...
                                </div>
                            </td>
                        </tr>
                    ) : rows.length === 0 ? (
                        <tr>
                            <td colSpan={8}>
                                <div className="archive-empty">
                                    <ArchiveIcon size={30} />

                                    <strong>
                                        No archived collateral
                                    </strong>

                                    <span>
                                        Successfully released
                                        collateral will appear here.
                                    </span>
                                </div>
                            </td>
                        </tr>
                    ) : (
                        rows.map(row => (
                            <tr key={row.id}>
                                <td>
                                    <strong className="archive-loan">
                                        {row.loanContractId}
                                    </strong>
                                </td>

                                <td>
                                    <div>
                                        {row.customerName}
                                    </div>

                                    <small className="archive-account">
                                        {row.accountNumber}
                                    </small>
                                </td>

                                <td>
                                    {row.collateralType}
                                </td>

                                <td>
                                    {money(
                                        row.approvedLimit,
                                        row.currency
                                    )}
                                </td>

                                <td>
                                    {row.releasedBy}
                                </td>

                                <td>
                                    {formatDate(
                                        row.releaseDate
                                    )}
                                </td>

                                <td>
                                    <span className="archive-repaid">
                                        <CheckCircle2 size={14} />
                                        Fully Repaid
                                    </span>
                                </td>

                                <td className="archive-action-cell">
                                    <button
                                        className="archive-view-btn"
                                        onClick={() =>
                                            onView(row.id)
                                        }
                                    >
                                        View Record
                                    </button>
                                </td>
                            </tr>
                        ))
                    )}
                </tbody>
            </table>
        </div>
    );
}