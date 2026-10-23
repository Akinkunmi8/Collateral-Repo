import { api } from "../api/client";

export async function downloadCsv(
    url: string,
    fallbackFileName: string
): Promise<void> {
    const response = await api.get<Blob>(url, {
        responseType: "blob"
    });

    /*
     * Axios headers can contain several value types,
     * so explicitly normalize Content-Disposition.
     */
    const rawDisposition =
        response.headers["content-disposition"];

    const disposition =
        typeof rawDisposition === "string"
            ? rawDisposition
            : undefined;

    let fileName: string = fallbackFileName;

    if (disposition) {
        /*
         * Handles:
         * filename="audit-trail.csv"
         * filename=audit-trail.csv
         */
        const match = disposition.match(
            /filename="?([^";]+)"?/i
        );

        if (match?.[1]) {
            fileName = match[1].trim();
        }
    }

    const rawContentType =
        response.headers["content-type"];

    const contentType =
        typeof rawContentType === "string"
            ? rawContentType
            : "text/csv;charset=utf-8";

    const blob = new Blob(
        [response.data],
        {
            type: contentType
        }
    );

    const objectUrl =
        window.URL.createObjectURL(blob);

    const link =
        document.createElement("a");

    link.href = objectUrl;
    link.download = fileName;

    document.body.appendChild(link);

    link.click();

    link.remove();

    window.URL.revokeObjectURL(
        objectUrl
    );
}