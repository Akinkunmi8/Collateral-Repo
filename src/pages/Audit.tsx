import {
    AlertCircle,
    Download,
    FileClock,
    Loader2,
    RefreshCw
} from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { api, auth } from "../api/client";
import { PageTitle } from "../components/UI";
import { downloadCsv } from "../utils/downloadCsv";

type AuditLog = {
    collateralId: string;
    action: string;
    performedBy: string;
    performedAtUtc: string;
    details?: string | null;
};

export default function Audit() {
    const [rows, setRows] = useState<AuditLog[]>([]);
    const [loading, setLoading] = useState(true);
    const [exporting, setExporting] = useState(false);
    const [error, setError] = useState("");

    const isAuditor = auth.isAuditor();

    const loadAuditLogs = useCallback(async () => {
        try {
            setLoading(true);
            setError("");

            const response =
                await api.get<AuditLog[]>(
                    "/dashboards/audit"
                );

            setRows(response.data);
        } catch (err) {
            console.error(
                "Unable to load audit logs",
                err
            );

            setError(
                "Unable to load audit trail."
            );
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        loadAuditLogs();
    }, [loadAuditLogs]);

    async function handleExport() {
        if (!isAuditor || exporting) {
            return;
        }

        try {
            setExporting(true);
            setError("");

            await downloadCsv(
                "/dashboards/audit/export",
                "audit-trail.csv"
            );
        } catch (err) {
            console.error(
                "Audit CSV export failed",
                err
            );

            setError(
                "Unable to export audit trail."
            );
        } finally {
            setExporting(false);
        }
    }

    function formatDate(value: string) {
        if (!value) {
            return "—";
        }

        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return value;
        }

        return date.toLocaleString("en-NG", {
            year: "numeric",
            month: "short",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        });
    }

    function formatAction(value: string) {
        if (!value) {
            return "—";
        }

        return value
            .replace(/([a-z])([A-Z])/g, "$1 $2")
            .replace(/_/g, " ");
    }

    return (
        <div className="auditPage">
            <PageTitle
                title="Audit Trail"
                subtitle="Workflow activity and accountability records."
                action={
                    isAuditor ? (
                        <button
                            type="button"
                            className="primary small"
                            disabled={exporting}
                            onClick={handleExport}
                        >
                            {exporting ? (
                                <Loader2
                                    size={17}
                                    className="auditSpin"
                                />
                            ) : (
                                <Download size={17} />
                            )}

                            {exporting
                                ? "Exporting..."
                                : "Export CSV"}
                        </button>
                    ) : undefined
                }
            />

            {error && (
                <div className="auditNotice auditNoticeError">
                    <AlertCircle size={17} />

                    <span>{error}</span>

                    <button
                        type="button"
                        onClick={loadAuditLogs}
                    >
                        Retry
                    </button>
                </div>
            )}

            <div className="panel auditPanel">
                <div className="auditPanelHeader">
                    <div>
                        <div className="auditPanelTitle">
                            <FileClock size={18} />

                            <h2>
                                Activity History
                            </h2>
                        </div>

                        <p>
                            Most recent workflow activities
                            recorded by the platform.
                        </p>
                    </div>

                    <div className="auditPanelActions">
                        {!loading && (
                            <span className="auditRecordCount">
                                {rows.length}{" "}
                                {rows.length === 1
                                    ? "record"
                                    : "records"}
                            </span>
                        )}

                        <button
                            type="button"
                            className="auditRefreshButton"
                            disabled={loading}
                            onClick={loadAuditLogs}
                            title="Refresh audit trail"
                        >
                            <RefreshCw
                                size={16}
                                className={
                                    loading
                                        ? "auditSpin"
                                        : ""
                                }
                            />

                            Refresh
                        </button>
                    </div>
                </div>

                {loading ? (
                    <div className="auditState">
                        <Loader2
                            size={25}
                            className="auditSpin"
                        />

                        <span>
                            Loading audit trail...
                        </span>
                    </div>
                ) : rows.length === 0 ? (
                    <div className="auditState">
                        <FileClock size={30} />

                        <strong>
                            No audit records
                        </strong>

                        <span>
                            Workflow activity will appear
                            here when actions are recorded.
                        </span>
                    </div>
                ) : (
                    <div className="tableWrap">
                        <table className="auditTable">
                            <thead>
                                <tr>
                                    <th>Collateral</th>
                                    <th>Action</th>
                                    <th>Performed By</th>
                                    <th>Date / Time</th>
                                    <th>Details</th>
                                </tr>
                            </thead>

                            <tbody>
                                {rows.map((row, index) => (
                                    <tr
                                        key={`${row.collateralId}-${row.performedAtUtc}-${index}`}
                                    >
                                        <td>
                                            <span className="auditCollateralId">
                                                {row.collateralId}
                                            </span>
                                        </td>

                                        <td>
                                            <span className="auditActionBadge">
                                                {formatAction(
                                                    row.action
                                                )}
                                            </span>
                                        </td>

                                        <td>
                                            <div className="auditActor">
                                                <span className="auditActorAvatar">
                                                    {(
                                                        row.performedBy ||
                                                        "S"
                                                    )
                                                        .charAt(0)
                                                        .toUpperCase()}
                                                </span>

                                                <span>
                                                    {row.performedBy ||
                                                        "system"}
                                                </span>
                                            </div>
                                        </td>

                                        <td>
                                            <span className="auditDate">
                                                {formatDate(
                                                    row.performedAtUtc
                                                )}
                                            </span>
                                        </td>

                                        <td>
                                            <div className="auditDetails">
                                                {row.details ||
                                                    "—"}
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
}