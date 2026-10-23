import { useEffect, useState } from "react";
import { PageTitle, Stat } from "../components/UI";
import { api } from "../api/client";

const cfg: Record<string, { key: string }> = {
    "Valuation": { key: "valuation" },
    "Legal Perfection": { key: "legal" },
    "Insurance": { key: "insurance" },
};

export default function Workflow({
    title,
    upload = false,
}: {
    title: string;
    upload?: boolean;
}) {
    const key = cfg[title]?.key;

    const [rows, setRows] = useState<any[]>([]);
    const [selected, setSelected] = useState<any>(null);
    const [file, setFile] = useState<File | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const load = async () => {
        if (!key) {
            setRows([]);
            setError(`Workflow configuration was not found for ${title}.`);
            return;
        }

        try {
            setLoading(true);
            setError("");

            const response = await api.get(
                `/dashboards/${key}/queue`
            );

            const responseData = response.data;

            // Support both the old array response and
            // the new paginated response.
            if (Array.isArray(responseData)) {
                setRows(responseData);
            } else if (Array.isArray(responseData?.items)) {
                setRows(responseData.items);
            } else {
                console.error(
                    "Unexpected workflow queue response:",
                    responseData
                );

                setRows([]);
            }
        } catch (err) {
            console.error(
                `Unable to load ${title} queue:`,
                err
            );

            setRows([]);
            setError(`Unable to load the ${title} queue.`);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        void load();
    }, [key]);

    async function view(id: string) {
        try {
            setError("");

            const response = await api.get(
                `/dashboards/${key}/${id}`
            );

            setSelected(response.data);
        } catch (err) {
            console.error(err);
            setError("Unable to load collateral details.");
        }
    }

    async function uploadReport() {
        if (!file || !selected) return;

        try {
            const formData = new FormData();

            formData.append("file", file);

            formData.append(
                "documentType",
                key === "legal" ? "2" : "3"
            );

            await api.post(
                `/collaterals/${selected.id}/documents`,
                formData
            );

            alert("Report uploaded successfully.");

            setFile(null);
        } catch (err) {
            console.error(err);
            setError("Unable to upload the report.");
        }
    }

    return (
        <>
            <PageTitle
                title={title}
                subtitle={`Review and process ${title.toLowerCase()} work items.`}
            />

            {error && (
                <div className="error-message">
                    {error}
                </div>
            )}

            <div className="stats">
                <Stat
                    label={`Pending ${title}`}
                    value={rows.length}
                />
            </div>

            <div className="panel">
                <div className="panelHead">
                    <div>
                        <h2>{title} Queue</h2>

                        <p>
                            Select a work item to review its
                            complete collateral details.
                        </p>
                    </div>
                </div>

                {loading ? (
                    <div style={{ padding: "24px" }}>
                        Loading {title.toLowerCase()} queue...
                    </div>
                ) : (
                    <div className="tableWrap">
                        <table>
                            <thead>
                                <tr>
                                    <th>Loan Contract</th>
                                    <th>Customer</th>
                                    <th>Type</th>
                                    <th>Status</th>
                                    <th>Created</th>
                                    <th>Action</th>
                                </tr>
                            </thead>

                            <tbody>
                                {rows.length === 0 ? (
                                    <tr>
                                        <td
                                            colSpan={6}
                                            style={{
                                                textAlign: "center",
                                                padding: "30px",
                                            }}
                                        >
                                            No pending {title.toLowerCase()} work items.
                                        </td>
                                    </tr>
                                ) : (
                                    rows.map((row) => (
                                        <tr key={row.id}>
                                            <td>
                                                {row.loanContractId}
                                            </td>

                                            <td>
                                                {row.customerName}
                                            </td>

                                            <td>
                                                {row.type}
                                            </td>

                                            <td>
                                                {row.status}
                                            </td>

                                            <td>
                                                {row.createdAtUtc
                                                    ? new Date(
                                                        row.createdAtUtc
                                                    ).toLocaleDateString()
                                                    : "—"}
                                            </td>

                                            <td>
                                                <button
                                                    className="linkBtn"
                                                    onClick={() =>
                                                        view(row.id)
                                                    }
                                                >
                                                    View / Process
                                                </button>
                                            </td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>

            {selected && (
                <div className="panel formPanel">
                    <h2>
                        {selected.collateralData?.customerName ??
                            selected.customerName}{" "}
                        —{" "}
                        {selected.collateralData?.loanContractId ??
                            selected.loanContractId}
                    </h2>

                    <p>
                        {selected.collateralData
                            ?.collateralDescription ??
                            selected.description}{" "}
                        ·{" "}
                        {selected.collateralData?.collateralType ??
                            selected.type}{" "}
                        · {selected.currentStage}
                    </p>

                    {upload && (
                        <div>
                            <label>
                                Required supporting report

                                <input
                                    type="file"
                                    accept=".pdf,.png,.jpg,.jpeg"
                                    onChange={(e) =>
                                        setFile(
                                            e.target.files?.[0] ??
                                            null
                                        )
                                    }
                                />
                            </label>

                            <button
                                className="primary small"
                                disabled={!file}
                                onClick={uploadReport}
                            >
                                Upload Report
                            </button>
                        </div>
                    )}

                    <button
                        className="linkBtn"
                        onClick={() => setSelected(null)}
                    >
                        Close
                    </button>
                </div>
            )}
        </>
    );
}