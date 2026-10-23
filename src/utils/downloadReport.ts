import { api } from "../api/client";

export async function downloadReport(
    url: string,
    fallbackFileName: string
): Promise<void> {
    const response = await api.get<Blob>(
        url,
        {
            responseType: "blob"
        }
    );

    const rawDisposition =
        response.headers["content-disposition"];

    const disposition =
        typeof rawDisposition === "string"
            ? rawDisposition
            : undefined;

    let fileName = fallbackFileName;

    if (disposition) {
        const utf8Match =
            disposition.match(
                /filename\*=UTF-8''([^;]+)/i
            );

        const normalMatch =
            disposition.match(
                /filename="?([^";]+)"?/i
            );

        if (utf8Match?.[1]) {
            fileName =
                decodeURIComponent(
                    utf8Match[1]
                );
        } else if (normalMatch?.[1]) {
            fileName =
                normalMatch[1].trim();
        }
    }

    const contentType =
        typeof response.headers["content-type"] === "string"
            ? response.headers["content-type"]
            : "application/octet-stream";

    const blob =
        new Blob(
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

    window.URL.revokeObjectURL(objectUrl);
}