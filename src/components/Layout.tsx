import {
    NavLink,
    Navigate,
    Outlet,
    useNavigate
} from "react-router-dom";

import {
    Archive,
    Building2,
    ChevronDown,
    ClipboardCheck,
    FileCheck2,
    Gavel,
    LayoutDashboard,
    LogOut,
    ShieldCheck,
    Users
} from "lucide-react";

import {
    useEffect,
    useRef,
    useState
} from "react";

import {
    auth,
    logout
} from "../api/client";

const all = [
    ["/dashboard", "Dashboard", LayoutDashboard],
    ["/collateral", "Collateral", Building2],
    ["/valuation", "Valuation", ClipboardCheck],
    ["/legal", "Legal Perfection", Gavel],
    ["/insurance", "Insurance", ShieldCheck],
    ["/archive", "Archive & Release", Archive],
    ["/admin", "Administration", Users],
    ["/audit", "Audit Trail", FileCheck2]
] as const;

const visible = (
    role: string,
    path: string
) =>
    role === "Admin" ||
    (role === "Auditor" && path !== "/admin") ||
    (role === "LegalTeam" && path === "/legal") ||
    (role === "InsuranceTeam" && path === "/insurance") ||
    (
        [
            "CreditTeam",
            "ValuationTeam",
            "CollateralReleaseTeam"
        ].includes(role) &&
        !["/admin", "/audit"].includes(path)
    ) ||
    path === "/dashboard";

export default function Layout() {

    const u = auth.get();
    const nav = useNavigate();

    const [profileOpen, setProfileOpen] =
        useState(false);

    const profileRef =
        useRef<HTMLDivElement>(null);

    useEffect(() => {

        function handleOutsideClick(
            event: MouseEvent
        ) {
            if (
                profileRef.current &&
                !profileRef.current.contains(
                    event.target as Node
                )
            ) {
                setProfileOpen(false);
            }
        }

        document.addEventListener(
            "mousedown",
            handleOutsideClick
        );

        return () => {
            document.removeEventListener(
                "mousedown",
                handleOutsideClick
            );
        };

    }, []);

    if (!u) {
        return (
            <Navigate
                to="/login"
                replace
            />
        );
    }


    const initials =
        getInitials(u.fullName || "User");

    async function handleLogout() {

        setProfileOpen(false);

        try {

            await logout();

        }
        catch (error) {

            console.error(
                "Backend logout failed:",
                error
            );

        }
        finally {

            auth.clear();

            nav("/login", {
                replace: true
            });
        }
    }

    return (
        <div className="shell">

            {/* ================================
                SIDEBAR
               ================================ */}

            <aside className="sidebar">

                <div className="brand">

                    <div className="brandIcon">
                        ▦
                    </div>

                    <div>
                        <b>
                            Collateral
                        </b>

                        <span>
                            MANAGEMENT
                        </span>
                    </div>

                </div>

                <nav>

                    {all
                        .filter(
                            ([path]) =>
                                visible(
                                    u.role,
                                    path
                                )
                        )
                        .map(
                            ([
                                to,
                                label,
                                Icon
                            ]) => (

                                <NavLink
                                    key={to}
                                    to={to}
                                    className={
                                        ({
                                            isActive
                                        }) =>
                                            isActive
                                                ? "active"
                                                : ""
                                    }
                                >

                                    <Icon size={18} />

                                    {label}

                                </NavLink>

                            )
                        )}

                </nav>

                <button
                    type="button"
                    className="logout"
                    onClick={handleLogout}
                >

                    <LogOut size={18} />

                    Sign out

                </button>

            </aside>

            {/* ================================
                MAIN APPLICATION
               ================================ */}

            <main>

                {/* ============================
                    GLOBAL HEADER
                   ============================ */}

                <header>

                    <div>

                        <b>
                            Collateral Management Solution
                        </b>

                        <span>
                            Enterprise Asset Verification
                            & Control
                        </span>

                    </div>

                    {/* ========================
                        GLOBAL USER PROFILE
                       ======================== */}

                    <div
                        className="profileWrapper"
                        ref={profileRef}
                    >

                        <button
                            type="button"
                            className="profile"
                            onClick={() =>
                                setProfileOpen(
                                    current => !current
                                )
                            }
                            aria-expanded={
                                profileOpen
                            }
                            aria-haspopup="menu"
                        >

                            <span className="profileAvatar">
                                {initials}
                            </span>

                            <div>

                                <b>
                                    {u.fullName}
                                </b>

                                <small>
                                    {u.role}
                                </small>

                            </div>

                            <ChevronDown
                                size={17}
                                className={
                                    profileOpen
                                        ? "profileChevron open"
                                        : "profileChevron"
                                }
                            />

                        </button>

                        {/* ====================
                            PROFILE DROPDOWN
                           ==================== */}

                        {profileOpen && (

                            <div
                                className="profileDropdown"
                                role="menu"
                            >

                                <div className="profileDropdownHeader">

                                    <span className="profileDropdownAvatar">
                                        {initials}
                                    </span>

                                    <div>

                                        <b>
                                            {u.fullName}
                                        </b>

                                        <small>
                                            {u.role}
                                        </small>

                                    </div>

                                </div>

                                <div className="profileDivider" />

                                <button
                                    type="button"
                                    role="menuitem"
                                    className="profileLogout"
                                    onClick={
                                        handleLogout
                                    }
                                >

                                    <LogOut
                                        size={17}
                                    />

                                    <span>
                                        Sign out
                                    </span>

                                </button>

                            </div>

                        )}

                    </div>

                </header>

                {/* ============================
                    PAGE CONTENT
                   ============================ */}

                <section className="content">
                    <Outlet />
                </section>

            </main>

        </div>
    );
}

function getInitials(
    fullName: string
): string {

    const names =
        fullName
            .trim()
            .split(/\s+/)
            .filter(Boolean);

    if (names.length === 0) {
        return "U";
    }

    if (names.length === 1) {
        return names[0]
            .substring(0, 2)
            .toUpperCase();
    }

    return (
        names[0][0] +
        names[names.length - 1][0]
    ).toUpperCase();
}