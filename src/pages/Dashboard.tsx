import {
    AlertTriangle,
    Archive,
    Building2,
    CheckCircle2,
    Clock3,
    Gavel,
    RefreshCw,
    ShieldCheck
} from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import { Stat } from "../components/UI";

type DashboardSummary = {
    total: number;
    pendingVerification: number;
    pendingValuation: number;
    legalPerfection: number;
    insurance: number;
    onHold: number;
    archived: number;
    released: number;
    withinTat: number;
    beyondTat: number;
};

const initialSummary: DashboardSummary = {
    total: 0,
    pendingVerification: 0,
    pendingValuation: 0,
    legalPerfection: 0,
    insurance: 0,
    onHold: 0,
    archived: 0,
    released: 0,
    withinTat: 0,
    beyondTat: 0
};

export default function Dashboard() {
    const navigate = useNavigate();

    const [summary, setSummary] =
        useState<DashboardSummary>(initialSummary);

    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);
    const [error, setError] = useState("");
    const [lastUpdated, setLastUpdated] =
        useState<Date | null>(null);

    const loadDashboard = useCallback(
        async (isRefresh = false) => {
            try {
                if (isRefresh) {
                    setRefreshing(true);
                } else {
                    setLoading(true);
                }

                setError("");

                const response =
                    await api.get<DashboardSummary>(
                        "/collaterals/summary"
                    );

                setSummary(response.data);
                setLastUpdated(new Date());
            } catch (err) {
                console.error(
                    "Unable to load dashboard summary",
                    err
                );

                setError(
                    "Unable to load dashboard information."
                );
            } finally {
                setLoading(false);
                setRefreshing(false);
            }
        },
        []
    );

    useEffect(() => {
        loadDashboard();
    }, [loadDashboard]);

    const activeTat =
        summary.withinTat + summary.beyondTat;

    const tatCompliance =
        activeTat === 0
            ? 0
            : Math.round(
                (summary.withinTat / activeTat) * 100
            );

    const workflow = [
        {
            label: "Verification",
            value: summary.pendingVerification,
            path: "/collateral"
        },
        {
            label: "Valuation",
            value: summary.pendingValuation,
            path: "/valuation"
        },
        {
            label: "Legal Perfection",
            value: summary.legalPerfection,
            path: "/legal"
        },
        {
            label: "Insurance",
            value: summary.insurance,
            path: "/insurance"
        },
        {
            label: "System Hold",
            value: summary.onHold,
            path: "/archive"
        },
        {
            label: "Archived",
            value: summary.archived,
            path: "/archive?tab=archived"
        }
    ];

    const maxWorkflowCount = Math.max(
        ...workflow.map(x => x.value),
        1
    );

    return (
        <div className="dashboardPage">
            <div className="pageTitle dashboardTitle">
                <div>
                    <h1>Dashboard</h1>

                    <p>
                        Enterprise collateral workflow overview
                    </p>
                </div>

                <div className="dashboardHeaderActions">
                    {lastUpdated && (
                        <span className="dashboardUpdated">
                            Last refreshed{" "}
                            {lastUpdated.toLocaleTimeString(
                                "en-NG",
                                {
                                    hour: "2-digit",
                                    minute: "2-digit"
                                }
                            )}
                        </span>
                    )}

                    <button
                        type="button"
                        className="outline dashboardRefresh"
                        disabled={refreshing}
                        onClick={() =>
                            loadDashboard(true)
                        }
                    >
                        <RefreshCw
                            size={16}
                            className={
                                refreshing
                                    ? "dashboardSpin"
                                    : ""
                            }
                        />

                        {refreshing
                            ? "Refreshing..."
                            : "Refresh"}
                    </button>
                </div>
            </div>

            {error && (
                <div className="dashboardError">
                    <AlertTriangle size={17} />

                    <span>{error}</span>

                    <button
                        type="button"
                        onClick={() =>
                            loadDashboard(true)
                        }
                    >
                        Retry
                    </button>
                </div>
            )}

            <div className="stats dashboardStats">
                <Stat
                    label="Total Collateral"
                    value={
                        loading ? "..." : summary.total
                    }
                    onClick={() =>
                        navigate("/collateral")
                    }
                />

                <Stat
                    label="Pending Verification"
                    value={
                        loading
                            ? "..."
                            : summary.pendingVerification
                    }
                    kind="amber"
                    onClick={() =>
                        navigate("/collateral")
                    }
                />

                <Stat
                    label="Pending Valuation"
                    value={
                        loading
                            ? "..."
                            : summary.pendingValuation
                    }
                    kind="purple"
                    onClick={() =>
                        navigate("/valuation")
                    }
                />

                <Stat
                    label="Legal Perfection"
                    value={
                        loading
                            ? "..."
                            : summary.legalPerfection
                    }
                    kind="blue"
                    onClick={() =>
                        navigate("/legal")
                    }
                />

                <Stat
                    label="Insurance"
                    value={
                        loading
                            ? "..."
                            : summary.insurance
                    }
                    kind="green"
                    onClick={() =>
                        navigate("/insurance")
                    }
                />

                <Stat
                    label="On Hold"
                    value={
                        loading
                            ? "..."
                            : summary.onHold
                    }
                    kind="amber"
                    onClick={() =>
                        navigate("/archive")
                    }
                />

                <Stat
                    label="Archived"
                    value={
                        loading
                            ? "..."
                            : summary.archived
                    }
                    kind="purple"
                    onClick={() =>
                        navigate(
                            "/archive?tab=archived"
                        )
                    }
                />
            </div>

            <div className="dashboardGrid">
                <section className="panel dashboardWorkflowPanel">
                    <div className="panelHead">
                        <div>
                            <h2>
                                Workflow Distribution
                            </h2>

                            <p>
                                Current collateral position
                                across the processing lifecycle
                            </p>
                        </div>

                        <Building2 size={20} />
                    </div>

                    <div className="dashboardWorkflowBody">
                        {workflow.map(item => {
                            const width =
                                item.value === 0
                                    ? 0
                                    : Math.max(
                                        8,
                                        (item.value /
                                            maxWorkflowCount) *
                                        100
                                    );

                            return (
                                <button
                                    type="button"
                                    className="dashboardWorkflowRow"
                                    key={item.label}
                                    onClick={() =>
                                        navigate(item.path)
                                    }
                                >
                                    <div className="dashboardWorkflowLabel">
                                        <span>
                                            {item.label}
                                        </span>

                                        <strong>
                                            {loading
                                                ? "..."
                                                : item.value}
                                        </strong>
                                    </div>

                                    <div className="dashboardWorkflowTrack">
                                        <span
                                            style={{
                                                width: `${width}%`
                                            }}
                                        />
                                    </div>
                                </button>
                            );
                        })}
                    </div>
                </section>

                <section className="panel dashboardTatPanel">
                    <div className="panelHead">
                        <div>
                            <h2>
                                TAT Performance
                            </h2>

                            <p>
                                Active workflow turnaround
                                performance
                            </p>
                        </div>

                        <Clock3 size={20} />
                    </div>

                    <div className="dashboardTatBody">
                        <div className="dashboardTatScore">
                            <strong>
                                {loading
                                    ? "..."
                                    : `${tatCompliance}%`}
                            </strong>

                            <span>
                                TAT Compliance
                            </span>
                        </div>

                        <div className="dashboardTatMetrics">
                            <div className="dashboardTatMetric success">
                                <span>
                                    <CheckCircle2
                                        size={17}
                                    />
                                    Within TAT
                                </span>

                                <strong>
                                    {loading
                                        ? "..."
                                        : summary.withinTat}
                                </strong>
                            </div>

                            <div className="dashboardTatMetric danger">
                                <span>
                                    <AlertTriangle
                                        size={17}
                                    />
                                    Beyond TAT
                                </span>

                                <strong>
                                    {loading
                                        ? "..."
                                        : summary.beyondTat}
                                </strong>
                            </div>
                        </div>

                        <div className="dashboardTatFootnote">
                            TAT applies to Verification,
                            Valuation, Legal Perfection and
                            Insurance stages.
                        </div>
                    </div>
                </section>
            </div>

            <section className="panel dashboardModules">
                <div className="panelHead">
                    <div>
                        <h2>
                            Operational Modules
                        </h2>

                        <p>
                            Access active collateral
                            processing queues
                        </p>
                    </div>
                </div>

                <div className="dashboardModuleGrid">
                    <Module
                        icon={<Building2 size={19} />}
                        title="Collateral Verification"
                        description="Review newly submitted collateral and verification requests."
                        count={summary.pendingVerification}
                        onClick={() =>
                            navigate("/collateral")
                        }
                    />

                    <Module
                        icon={<CheckCircle2 size={19} />}
                        title="Valuation"
                        description="Review valuation reports and collateral coverage."
                        count={summary.pendingValuation}
                        onClick={() =>
                            navigate("/valuation")
                        }
                    />

                    <Module
                        icon={<Gavel size={19} />}
                        title="Legal Perfection"
                        description="Complete legal perfection and registration."
                        count={summary.legalPerfection}
                        onClick={() =>
                            navigate("/legal")
                        }
                    />

                    <Module
                        icon={<ShieldCheck size={19} />}
                        title="Insurance"
                        description="Capture insurance policy information and reports."
                        count={summary.insurance}
                        onClick={() =>
                            navigate("/insurance")
                        }
                    />

                    <Module
                        icon={<Archive size={19} />}
                        title="Release & Archive"
                        description="Review system hold, repayment and archived collateral."
                        count={
                            summary.onHold +
                            summary.archived
                        }
                        onClick={() =>
                            navigate("/archive")
                        }
                    />
                </div>
            </section>
        </div>
    );
}

function Module({
    icon,
    title,
    description,
    count,
    onClick
}: {
    icon: React.ReactNode;
    title: string;
    description: string;
    count: number;
    onClick: () => void;
}) {
    return (
        <button
            type="button"
            className="dashboardModule"
            onClick={onClick}
        >
            <div className="dashboardModuleIcon">
                {icon}
            </div>

            <div className="dashboardModuleText">
                <strong>{title}</strong>
                <span>{description}</span>
            </div>

            <div className="dashboardModuleCount">
                {count}
            </div>
        </button>
    );
}