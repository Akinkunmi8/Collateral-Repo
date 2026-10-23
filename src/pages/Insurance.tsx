import {
    useEffect,
    useState
} from "react";

import {
    useNavigate
} from "react-router-dom";

import {
    getInsuranceQueue
} from "../api/insurance";

import type {
    InsuranceQueueItem
} from "../types/insurance";

import "./insurance.css";

export default function Insurance() {

    const navigate = useNavigate();

    const [items, setItems] =
        useState<InsuranceQueueItem[]>([]);

    const [search, setSearch] =
        useState("");

    const [tat, setTat] =
        useState("");

    const [pageNumber, setPageNumber] =
        useState(1);

    const [totalPages, setTotalPages] =
        useState(0);

    const [totalCount, setTotalCount] =
        useState(0);

    const [loading, setLoading] =
        useState(true);

    const [error, setError] =
        useState<string | null>(null);

    const pageSize = 20;

    useEffect(() => {

        const timer = window.setTimeout(
            () => loadQueue(),
            300
        );

        return () =>
            window.clearTimeout(timer);

    }, [search, tat, pageNumber]);

    async function loadQueue() {

        try {

            setLoading(true);
            setError(null);

            const result =
                await getInsuranceQueue({
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
        catch (err) {

            console.error(err);

            setError(
                "Unable to load the Insurance queue."
            );

        }
        finally {

            setLoading(false);
        }
    }

    function formatHours(
        hours: number | null
    ) {
        return hours === null
            ? "-"
            : `${hours.toFixed(1)} hrs`;
    }

    function formatDate(
        date: string | null
    ) {
        if (!date)
            return "-";

        return new Date(date)
            .toLocaleString();
    }

    return (
        <div className="insurance-page">

            <div className="insurance-header">

                <div>
                    <h1>
                        Insurance Management
                    </h1>

                    <p>
                        Review and complete collateral
                        insurance requirements.
                    </p>
                </div>

                <div className="insurance-count">
                    {totalCount} Pending
                </div>

            </div>

            <div className="insurance-toolbar">

                <input
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
                <div className="insurance-error">
                    {error}
                </div>
            )}

            <div className="insurance-table-card">

                <table className="insurance-table">

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
                                    Loading Insurance queue...
                                </td>
                            </tr>

                        ) : items.length === 0 ? (

                            <tr>
                                <td colSpan={7}>
                                    No collateral is currently
                                    awaiting Insurance.
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
                                            "insurance-status">
                                            {item.status}
                                        </span>
                                    </td>

                                    <td>
                                        <span
                                            className={
                                                item.tatStatus ===
                                                    "BeyondTat"
                                                    ? "insurance-tat-beyond"
                                                    : "insurance-tat-within"
                                            }
                                        >
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
                                    </td>

                                    <td>
                                        {formatDate(
                                            item.tatDueAtUtc
                                        )}
                                    </td>

                                    <td>
                                        <button
                                            className=
                                            "insurance-review-btn"
                                            onClick={() =>
                                                navigate(
                                                    `/insurance/${item.id}`
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

            <div className="insurance-pagination">

                <span>
                    Page {pageNumber} of{" "}
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
                                x => x - 1
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
                                x => x + 1
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