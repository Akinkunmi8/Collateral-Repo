import axios from "axios";

export const api = axios.create({
    baseURL:
        import.meta.env.VITE_API_BASE_URL ??
        "http://localhost:5080/api"
});

api.interceptors.request.use(config => {

    const token =
        localStorage.getItem("access_token");

    if (token) {
        config.headers.Authorization =
            `Bearer ${token}`;
    }

    return config;
});

api.interceptors.response.use(
    response => response,

    error => {

        if (error.response?.status === 401) {

            localStorage.clear();

            if (location.pathname !== "/login") {
                location.href = "/login";
            }
        }

        return Promise.reject(error);
    }
);

export const auth = {

    get: () => {
        try {
            return JSON.parse(
                localStorage.getItem("auth_user") ||
                "null"
            );
        }
        catch {
            return null;
        }
    },

    set: (value: any) =>
        localStorage.setItem(
            "auth_user",
            JSON.stringify(value)
        ),

    clear: () =>
        localStorage.clear(),

    isInRole: (role: string) => {
        const user = auth.get();

        if (!user) {
            return false;
        }

        const currentRole =
            user.role ??
            user.Role ??
            "";

        return currentRole
            .toString()
            .toLowerCase() ===
            role.toLowerCase();
    },

    isAuditor: () => {
        return auth.isInRole("Auditor");
    }
};

export async function logout(): Promise<void> {
    await api.post("/auth/logout");
}