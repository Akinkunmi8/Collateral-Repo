import {
    useEffect,
    useState
} from "react";

import {
    useNavigate
} from "react-router-dom";

import {
    getLegalQueue
} from "../api/legal";

import type {
    LegalQueueItem,
    LegalQueueResponse
} from "../types/legal";

import "./legal.css";

export default function Legal() {

    const navigate = useNavigate();

    const [items, setItems] =
        useState<LegalQueueItem[]>([]);

    const [search, setSearch] =
        useState("");

    const [tat, setTat] =
        useState("");

    const [pageNumber, setPageNumber] =
        useState(1);

    const [pageSize] =
        useState(20);

    const [totalPages, setTotalPages] =
        useState(0);

    const [totalCount, setTotalCount] =
        useState(0);

    const [loading, setLoading] =
        useState(true);

    const [error, setError] =
        useState<string | null>(null);

    useEffect(() => {

        const timer =
            window.setTimeout(() => {
                loadQueue();
            }, 300);

        return () =>
            window.clearTimeout(timer);

    }, [
        search,
        tat,
        pageNumber
    ]);

    async function loadQueue() {

        try {

            setLoading(true);
            setError(null);

            const result: LegalQueueResponse =
                await getLegalQueue({
                    search:
                        search.trim() || undefined,

                    tat:
                        tat || undefined,

                    pageNumber,
                    pageSize
                });

            setItems(result.items);
            setTotalPages(result.totalPages);
            setTotalCount(result.totalCount);

        }
        catch {

            setError(
                "Unable to load the Legal Perfection queue."
            );

        }
        finally {

            setLoading(false);
        }
    }

    function formatDate(value: string) {

        return new Date(value)
            .toLocaleString();
    }

    function formatHours(
        value: number | null
    ) {

        if (value === null)
            return "-";

        return `${value.toFixed(1)} hrs`;
    }

    return (
        <div className="legal-page">

            <div className="legal-header">

                <div>
                    <h1>
                        Legal Perfection
                    </h1>

                    <p>
                        Review and complete collateral
                        legal perfection requirements.
                    </p>
                </div>

                <div className="legal-count">
                    {totalCount} Pending
                </div>

            </div>

            <div className="legal-toolbar">

                <input
                    className="legal-search"
                    type="text"
                    placeholder=
                    "Search loan, customer or account..."
                    value={search}
                    onChange={e => {
                        setSearch(e.target.value);
                        setPageNumber(1);
                    }}
                />

                <select
                    value={tat}
                    onChange={e => {
                        setTat(e.target.value);
                        setPageNumber(1);
                    }}
                >
                    <option value="">
                        All TAT
                    </option>

                    <option value="WithinTat">
                        Within TAT
                    </option>

                    <option value="BeyondTat">
                        Beyond TAT
                    </option>
                </select>

            </div>

            {error && (
                <div className="legal-error">
                    {error}
                </div>
            )}

            <div className="legal-table-card">

                <table className="legal-table">

                    <thead>
                        <tr>
                            <th>Loan Contract</th>
                            <th>Customer</th>
                            <th>Collateral</th>
                            <th>Status</th>
                            <th>TAT</th>
                            <th>Due Date</th>
                            <th></th>
                        </tr>
                    </thead>

                    <tbody>

                        {loading ? (

                            <tr>
                                <td colSpan={7}>
                                    Loading Legal Perfection queue...
                                </td>
                            </tr>

                        ) : items.length === 0 ? (

                            <tr>
                                <td colSpan={7}>
                                    No collateral is currently
                                    awaiting Legal Perfection.
                                </td>
                            </tr>

                        ) : (

                            items.map(item => (

                                <tr key={item.id}>

                                    <td>
                                        <strong>
                                            {item.loanContractId}
                                        </strong>
                                    </td>

                                    <td>
                                        {item.customerName}
                                    </td>

                                    <td>
                                        {item.type}
                                    </td>

                                    <td>
                                        <span className=
                                            "legal-status">
                                            {item.status}
                                        </span>
                                    </td>

                                    <td>
                                        <div>
                                            <span className={
                                                item.tatStatus ===
                                                    "BeyondTat"
                                                    ? "tat-beyond"
                                                    : "tat-within"
                                            }>
                                                {
                                                    item.tatStatus ===
                                                        "BeyondTat"
                                                        ? "Beyond TAT"
                                                        : "Within TAT"
                                                }
                                            </span>

                                            <small>
                                                {formatHours(
                                                    item.tatElapsedHours
                                                )}
                                                {" / "}
                                                {
                                                    item.tatAllowedHours ??
                                                    "-"
                                                }
                                                {" hrs"}
                                            </small>
                                        </div>
                                    </td>

                                    <td>
                                        {
                                            item.tatDueAtUtc
                                                ? formatDate(
                                                    item.tatDueAtUtc
                                                )
                                                : "-"
                                        }
                                    </td>

                                    <td>
                                        <button
                                            className="legal-review-btn"
                                            onClick={() =>
                                                navigate(
                                                    `/legal/${item.id}`
                                                )
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

            <div className="legal-pagination">

                <span>
                    Page {pageNumber}
                    {" of "}
                    {Math.max(totalPages, 1)}
                </span>

                <div>

                    <button
                        disabled={
                            pageNumber <= 1 ||
                            loading
                        }
                        onClick={() =>
                            setPageNumber(
                                value => value - 1
                            )
                        }
                    >
                        Previous
                    </button>

                    <button
                        disabled={
                            pageNumber >= totalPages ||
                            totalPages === 0 ||
                            loading
                        }
                        onClick={() =>
                            setPageNumber(
                                value => value + 1
                            )
                        }
                    >
                        Next
                    </button>

                </div>

            </div>

        </div>
    );
}