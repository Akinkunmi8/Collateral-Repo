import {
    FormEvent,
    useCallback,
    useEffect,
    useState
} from 'react';
import { Link } from 'react-router-dom';
import { Plus, Search } from 'lucide-react';

import {
    PageTitle,
    Stat
} from '../components/UI';

import { api } from '../api/client';
import { FileText } from "lucide-react";
import { useNavigate } from "react-router-dom";

interface CollateralRegisterItem {
    id: string;
    loanContractId: string;
    customerName: string;
    type: string;
    currentStage: string;
    status: string;
    createdAtUtc: string;
}

interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

interface CollateralSummary {
    total: number;
    pending: number;
    archived: number;
    released: number;
    withinTat: number;
    beyondTat: number;
}

type KpiFilter =
    | 'all'
    | 'pending'
    | 'archived'
    | 'released'
    | 'withinTat'
    | 'beyondTat';

interface CollateralForm {
    loanContractId: string;
    type: number;
    region: number;
    description: string;
    facilityType: string;
    currency: string;
    collateralAddress: string;
    collateralOwner: string;
    relationshipManagerName: string;
    branchManagerName: string;
}

const initialForm: CollateralForm = {
    loanContractId: '',
    type: 1,
    region: 1,
    description: '',
    facilityType: '',
    currency: 'NGN',
    collateralAddress: '',
    collateralOwner: '',
    relationshipManagerName: '',
    branchManagerName: ''
};

export default function Collateral() {

    // --------------------------------------------------
    // Register
    // --------------------------------------------------

    const [rows, setRows] =
        useState<CollateralRegisterItem[]>([]);

    // --------------------------------------------------
    // Manual collateral form
    // --------------------------------------------------

    const [showForm, setShowForm] =
        useState(false);

    const [form, setForm] =
        useState<CollateralForm>(initialForm);

    // --------------------------------------------------
    // Register filters
    // --------------------------------------------------

    const [search, setSearch] =
        useState('');

    const [stageFilter, setStageFilter] =
        useState('');

    const [typeFilter, setTypeFilter] =
        useState('');

    const [statusFilter, setStatusFilter] =
        useState('');

    const [tatFilter, setTatFilter] =
        useState('');

    // Which KPI card is currently selected.
    const [activeKpi, setActiveKpi] =
        useState<KpiFilter>('all');

    // --------------------------------------------------
    // Dashboard summary
    // --------------------------------------------------

    const [summary, setSummary] =
        useState<CollateralSummary>({
            total: 0,
            pending: 0,
            archived: 0,
            released: 0,
            withinTat: 0,
            beyondTat: 0
        });

    const [summaryLoading, setSummaryLoading] =
        useState(false);

    // --------------------------------------------------
    // Pagination
    // --------------------------------------------------

    const [pageNumber, setPageNumber] =
        useState(1);

    const [pageSize, setPageSize] =
        useState(10);

    const [totalCount, setTotalCount] =
        useState(0);

    const [totalPages, setTotalPages] =
        useState(0);

    const [hasPreviousPage, setHasPreviousPage] =
        useState(false);

    const [hasNextPage, setHasNextPage] =
        useState(false);

    // --------------------------------------------------
    // General UI state
    // --------------------------------------------------

    const [loading, setLoading] =
        useState(false);

    const [error, setError] =
        useState('');

    // ==================================================
    // LOAD COLLATERAL REGISTER
    // ==================================================
    const navigate = useNavigate();
    const load = useCallback(async () => {

        setLoading(true);
        setError('');

        try {

            const response =
                await api.get<
                    PagedResult<CollateralRegisterItem>
                >(
                    '/collaterals',
                    {
                        params: {
                            search:
                                search.trim() ||
                                undefined,

                            stageFilter:
                                stageFilter ||
                                undefined,

                            typeFilter:
                                typeFilter ||
                                undefined,

                            statusFilter:
                                statusFilter ||
                                undefined,

                            tatFilter:
                                tatFilter ||
                                undefined,

                            pageNumber,
                            pageSize
                        }
                    }
                );

            const data = response.data;

            setRows(data.items ?? []);

            setTotalCount(
                data.totalCount ?? 0
            );

            setTotalPages(
                data.totalPages ?? 0
            );

            setHasPreviousPage(
                data.hasPreviousPage ?? false
            );

            setHasNextPage(
                data.hasNextPage ?? false
            );
        }
        catch (err: any) {

            console.error(
                'Failed to load collateral register:',
                err
            );

            setRows([]);
            setTotalCount(0);
            setTotalPages(0);
            setHasPreviousPage(false);
            setHasNextPage(false);

            setError(
                err.response?.data?.error ??
                'Unable to load collateral register.'
            );
        }
        finally {
            setLoading(false);
        }

    }, [
        search,
        stageFilter,
        typeFilter,
        statusFilter,
        tatFilter,
        pageNumber,
        pageSize
    ]);

    // Automatic reload for filters/pagination.
    //
    // Search is deliberately excluded because we only
    // want search to execute when Search is submitted.
    useEffect(() => {
        load();
    }, [
        pageNumber,
        pageSize,
        stageFilter,
        typeFilter,
        statusFilter,
        tatFilter
    ]);

    // ==================================================
    // LOAD DASHBOARD SUMMARY
    // ==================================================

    const loadSummary =
        useCallback(async () => {

            setSummaryLoading(true);

            try {

                const response =
                    await api.get<CollateralSummary>(
                        '/collaterals/summary'
                    );

                setSummary(response.data);
            }
            catch (err: any) {
                console.error(
                    'Failed to load collateral summary:',
                    err
                );

                setError(
                    err.response?.data?.error ??
                    err.response?.data?.message ??
                    'Unable to load collateral dashboard summary.'
                );
            }
            finally {
                setSummaryLoading(false);
            }

        }, []);

    useEffect(() => {
        loadSummary();
    }, [loadSummary]);

    // ==================================================
    // KPI FILTERING
    // ==================================================

    function applyKpiFilter(
        filter: KpiFilter
    ) {

        setActiveKpi(filter);

        /*
         * A KPI represents a new high-level filter.
         *
         * Therefore remove Status/TAT filters left
         * behind by another KPI before applying the
         * newly selected KPI.
         */

        setStatusFilter('');
        setTatFilter('');
        setPageNumber(1);

        switch (filter) {

            case 'pending':

                setStatusFilter(
                    'PendingApproval'
                );

                break;

            case 'archived':

                setStatusFilter(
                    'Archived'
                );

                break;

            case 'released':

                setStatusFilter(
                    'Released'
                );

                break;

            case 'withinTat':

                setTatFilter(
                    'WithinTat'
                );

                break;

            case 'beyondTat':

                setTatFilter(
                    'BeyondTat'
                );

                break;

            case 'all':
            default:
                break;
        }
    }

    // ==================================================
    // MANUAL COLLATERAL CREATION
    // ==================================================

    async function create(
        e: FormEvent
    ) {

        e.preventDefault();

        setError('');

        try {

            await api.post(
                '/collaterals',
                {
                    ...form,

                    type:
                        Number(form.type),

                    region:
                        Number(form.region)
                }
            );

            setShowForm(false);

            setForm(initialForm);

            /*
             * Refresh the dashboard totals immediately.
             * This means Total/Pending/Within TAT will
             * reflect the newly created collateral.
             */
            await loadSummary();

            /*
             * Newly created collateral normally appears
             * at the top of page 1.
             */
            if (pageNumber !== 1) {

                setPageNumber(1);
            }
            else {

                await load();
            }
        }
        catch (err: any) {

            setError(
                err.response?.data?.error ??
                'Unable to create collateral.'
            );
        }
    }

    // ==================================================
    // SEARCH
    // ==================================================

    function submitSearch(
        e: FormEvent
    ) {

        e.preventDefault();

        /*
         * Manual searching means we are no longer
         * representing only a KPI selection.
         */
        setActiveKpi('all');

        if (pageNumber !== 1) {

            setPageNumber(1);
        }
        else {

            load();
        }
    }

    // ==================================================
    // CLEAR FILTERS
    // ==================================================

    function clearFilters() {

        setSearch('');

        setStageFilter('');

        setTypeFilter('');

        setStatusFilter('');

        setTatFilter('');

        setActiveKpi('all');

        setPageNumber(1);
    }

    // ==================================================
    // PAGINATION
    // ==================================================

    function previousPage() {

        if (
            hasPreviousPage &&
            pageNumber > 1
        ) {

            setPageNumber(
                current =>
                    current - 1
            );
        }
    }

    function nextPage() {

        if (
            hasNextPage &&
            pageNumber < totalPages
        ) {

            setPageNumber(
                current =>
                    current + 1
            );
        }
    }

    // ==================================================
    // RENDER
    // ==================================================

    return (
        <>

            <PageTitle
                title="Collateral Management"
                subtitle="Register, verify and monitor pledged assets."
                action={
                    <button
                        type="button"
                        className="primary small"
                        onClick={() =>
                            setShowForm(
                                current => !current
                            )
                        }
                    >
                        <Plus size={17} />
                        Manual Collateral
                    </button>
                }
            />

            {/* =========================================
                OPERATIONAL KPI CARDS
               ========================================= */}

            <div className="stats collateralStats">

                <Stat
                    label="Total Collateral"
                    value={
                        summaryLoading
                            ? '...'
                            : summary.total
                    }
                    kind="blue"
                    active={
                        activeKpi === 'all'
                    }
                    onClick={() =>
                        applyKpiFilter('all')
                    }
                />

                <Stat
                    label="Pending"
                    value={
                        summaryLoading
                            ? '...'
                            : summary.pending
                    }
                    kind="purple"
                    active={
                        activeKpi ===
                        'pending'
                    }
                    onClick={() =>
                        applyKpiFilter(
                            'pending'
                        )
                    }
                />

                <Stat
                    label="Archived"
                    value={
                        summaryLoading
                            ? '...'
                            : summary.archived
                    }
                    kind="green"
                    active={
                        activeKpi ===
                        'archived'
                    }
                    onClick={() =>
                        applyKpiFilter(
                            'archived'
                        )
                    }
                />

                <Stat
                    label="Released"
                    value={
                        summaryLoading
                            ? '...'
                            : summary.released
                    }
                    kind="green"
                    active={
                        activeKpi ===
                        'released'
                    }
                    onClick={() =>
                        applyKpiFilter(
                            'released'
                        )
                    }
                />

                <Stat
                    label="Within TAT"
                    value={
                        summaryLoading
                            ? '...'
                            : summary.withinTat
                    }
                    kind="green"
                    active={
                        activeKpi ===
                        'withinTat'
                    }
                    onClick={() =>
                        applyKpiFilter(
                            'withinTat'
                        )
                    }
                />

                <Stat
                    label="Beyond TAT"
                    value={
                        summaryLoading
                            ? '...'
                            : summary.beyondTat
                    }
                    kind="red"
                    active={
                        activeKpi ===
                        'beyondTat'
                    }
                    onClick={() =>
                        applyKpiFilter(
                            'beyondTat'
                        )
                    }
                />

            </div>

            {/* =========================================
                ERROR
               ========================================= */}

            {error && (
                <div className="alert">
                    {error}
                </div>
            )}

            {/* =========================================
                MANUAL COLLATERAL FORM
               ========================================= */}

            {showForm && (

                <form
                    className="panel formPanel"
                    onSubmit={create}
                >

                    <h2>
                        Manual Collateral Input
                    </h2>

                    <div className="formGrid">

                        <label>
                            Loan Contract ID

                            <input
                                required
                                value={
                                    form.loanContractId
                                }
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        loanContractId:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label>
                            Collateral Type

                            <select
                                value={form.type}
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        type:
                                            Number(
                                                e.target.value
                                            )
                                    })
                                }
                            >
                                <option value={1}>
                                    Real Estate
                                </option>

                                <option value={2}>
                                    Cash
                                </option>

                                <option value={3}>
                                    Blue Chip Shares
                                </option>

                                <option value={4}>
                                    Movable Asset
                                </option>

                                <option value={5}>
                                    Securities
                                </option>
                            </select>
                        </label>

                        <label>
                            Region

                            <select
                                value={form.region}
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        region:
                                            Number(
                                                e.target.value
                                            )
                                    })
                                }
                            >
                                <option value={1}>
                                    Lagos
                                </option>

                                <option value={2}>
                                    Abuja
                                </option>

                                <option value={3}>
                                    Other Region
                                </option>
                            </select>
                        </label>

                        <label>
                            Facility Type

                            <input
                                value={
                                    form.facilityType
                                }
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        facilityType:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label>
                            Currency

                            <input
                                value={
                                    form.currency
                                }
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        currency:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label>
                            Collateral Owner

                            <input
                                value={
                                    form.collateralOwner
                                }
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        collateralOwner:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label>
                            Address

                            <input
                                value={
                                    form.collateralAddress
                                }
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        collateralAddress:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label>
                            Relationship Manager

                            <input
                                value={
                                    form.relationshipManagerName
                                }
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        relationshipManagerName:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label>
                            Branch Manager

                            <input
                                value={
                                    form.branchManagerName
                                }
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        branchManagerName:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label className="wide">
                            Description

                            <textarea
                                required
                                value={
                                    form.description
                                }
                                onChange={e =>
                                    setForm({
                                        ...form,

                                        description:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                    </div>

                    <button
                        className="primary small"
                        type="submit"
                    >
                        Submit Collateral
                    </button>

                </form>
            )}

            {/* =========================================
                COLLATERAL REGISTER
               ========================================= */}

            <div className="panel">

                <div className="panelHead">

                    <div>

                        <h2>
                            Collateral Register
                        </h2>

                        <p>
                            Live collateral records from
                            the ASP.NET Core API.
                        </p>

                    </div>

                </div>

                {/* =====================================
                    SEARCH / FILTERS
                   ===================================== */}

                <form
                    className="searchBox"
                    onSubmit={submitSearch}
                >

                    <input
                        placeholder="Search contract, customer or account..."
                        value={search}
                        onChange={e =>
                            setSearch(
                                e.target.value
                            )
                        }
                    />

                    {/* STAGE */}

                    <select
                        value={stageFilter}
                        onChange={e => {

                            setStageFilter(
                                e.target.value
                            );

                            /*
                             * Manual filtering means
                             * there is no single KPI
                             * controlling the table.
                             */
                            setActiveKpi('all');

                            setPageNumber(1);
                        }}
                    >
                        <option value="">
                            All Stages
                        </option>

                        <option value="CollateralData">
                            Collateral Data
                        </option>

                        <option value="Valuation">
                            Valuation
                        </option>

                        <option value="Perfection">
                            Legal Perfection
                        </option>

                        <option value="Insurance">
                            Insurance
                        </option>

                        <option value="SystemHold">
                            System Hold
                        </option>

                        <option value="Release">
                            Release
                        </option>

                        <option value="Archived">
                            Archived
                        </option>
                    </select>

                    {/* TYPE */}

                    <select
                        value={typeFilter}
                        onChange={e => {

                            setTypeFilter(
                                e.target.value
                            );

                            setActiveKpi('all');

                            setPageNumber(1);
                        }}
                    >
                        <option value="">
                            All Types
                        </option>

                        <option value="RealEstate">
                            Real Estate
                        </option>

                        <option value="Cash">
                            Cash
                        </option>

                        <option value="BlueChipShares">
                            Blue Chip Shares
                        </option>

                        <option value="MovableAsset">
                            Movable Asset
                        </option>

                        <option value="Securities">
                            Securities
                        </option>
                    </select>

                    {/* STATUS */}

                    <select
                        value={statusFilter}
                        onChange={e => {

                            setStatusFilter(
                                e.target.value
                            );

                            setActiveKpi('all');

                            setPageNumber(1);
                        }}
                    >
                        <option value="">
                            All Statuses
                        </option>

                        <option value="PendingApproval">
                            Pending Approval
                        </option>

                        <option value="Approved">
                            Approved
                        </option>

                        <option value="Rejected">
                            Rejected
                        </option>

                        <option value="Released">
                            Released
                        </option>

                        <option value="Archived">
                            Archived
                        </option>
                    </select>

                    {/* TAT */}

                    <select
                        value={tatFilter}
                        onChange={e => {

                            setTatFilter(
                                e.target.value
                            );

                            setActiveKpi('all');

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

                    <button
                        className="primary small"
                        type="submit"
                    >
                        <Search size={16} />
                        Search
                    </button>

                    <button
                        type="button"
                        className="linkBtn"
                        onClick={
                            clearFilters
                        }
                    >
                        Clear
                    </button>

                </form>

                {/* =====================================
                    REGISTER TABLE
                   ===================================== */}

                <div className="tableWrap">

                    <table>

                        <thead>
                            <tr>
                                <th>
                                    Loan Contract
                                </th>

                                <th>
                                    Customer
                                </th>

                                <th>
                                    Type
                                </th>

                                <th>
                                    Stage
                                </th>

                                <th>
                                    Status
                                </th>

                                <th>
                                    Created
                                </th>
                            </tr>
                        </thead>

                        <tbody>

                            {loading ? (

                                <tr>
                                    <td colSpan={6}>
                                        Loading collateral
                                        register...
                                    </td>
                                </tr>

                            ) : rows.length === 0 ? (

                                <tr>
                                    <td colSpan={6}>
                                        No collateral records
                                        found.
                                    </td>
                                </tr>

                            ) : (

                                rows.map(row => (

                                    <tr key={row.id}>

                                        <td>
                                            <Link
                                                to={`/collateral/${row.id}`}
                                                className="contractLink"
                                                title={`View ${row.loanContractId}`}
                                            >
                                                {row.loanContractId}
                                            </Link>
                                        </td>

                                        <td>
                                            {
                                                row.customerName
                                            }
                                        </td>

                                        <td>
                                            {row.type}
                                        </td>

                                        <td>
                                            {
                                                row.currentStage
                                            }
                                        </td>

                                        <td>
                                            {row.status}
                                        </td>

                                        <td>
                                            {
                                                new Date(
                                                    row.createdAtUtc
                                                )
                                                    .toLocaleDateString()
                                            }
                                        </td>
                                        <td>
                                            <button
                                                type="button"
                                                className="secondary small"
                                                onClick={() =>
                                                    navigate(
                                                        `/collateral/${row.id}/report`
                                                    )
                                                }
                                            >
                                                <FileText size={15} />
                                                Report
                                            </button>
                                        </td>

                                    </tr>
                                ))
                            )}

                        </tbody>

                    </table>

                </div>

                {/* =====================================
                    PAGINATION
                   ===================================== */}

                <div className="pagination">

                    <div>
                        Showing{' '}
                        <b>
                            {rows.length}
                        </b>
                        {' '}of{' '}
                        <b>
                            {totalCount}
                        </b>
                        {' '}records
                    </div>

                    <div>

                        <button
                            type="button"
                            className="linkBtn"
                            disabled={
                                !hasPreviousPage ||
                                loading
                            }
                            onClick={
                                previousPage
                            }
                        >
                            Previous
                        </button>

                        <span>
                            Page{' '}
                            <b>
                                {
                                    totalPages === 0
                                        ? 0
                                        : pageNumber
                                }
                            </b>
                            {' '}of{' '}
                            <b>
                                {totalPages}
                            </b>
                        </span>

                        <button
                            type="button"
                            className="linkBtn"
                            disabled={
                                !hasNextPage ||
                                loading
                            }
                            onClick={
                                nextPage
                            }
                        >
                            Next
                        </button>

                        <select
                            value={pageSize}
                            onChange={e => {

                                setPageSize(
                                    Number(
                                        e.target.value
                                    )
                                );

                                setPageNumber(1);
                            }}
                        >
                            <option value={5}>
                                5 per page
                            </option>

                            <option value={10}>
                                10 per page
                            </option>

                            <option value={20}>
                                20 per page
                            </option>

                            <option value={50}>
                                50 per page
                            </option>
                        </select>

                    </div>

                </div>

            </div>

        </>
    );
}