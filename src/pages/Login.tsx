import {
    type FormEvent,
    useState
} from "react";

import {
    ArrowRight,
    BarChart3,
    Eye,
    EyeOff,
    FileText,
    LockKeyhole,
    Mail,
    ShieldCheck
} from "lucide-react";

import { useNavigate } from "react-router-dom";
import { api, auth } from "../api/client";

import "./Login.css";

export default function Login() {
    const [showPassword, setShowPassword] =
        useState(false);

    const [email, setEmail] =
        useState("");

    const [password, setPassword] =
        useState("");

    const [error, setError] =
        useState("");

    const [busy, setBusy] =
        useState(false);

    const navigate = useNavigate();

    async function submit(e: FormEvent) {
        e.preventDefault();

        setBusy(true);
        setError("");

        try {
            const { data } =
                await api.post(
                    "/auth/login",
                    {
                        email,
                        password
                    }
                );

            localStorage.setItem(
                "access_token",
                data.token
            );

            auth.set(data);

            navigate("/dashboard");
        } catch (x: any) {
            setError(
                x.response?.data?.error ??
                "Unable to sign in."
            );
        } finally {
            setBusy(false);
        }
    }

    return (
        <main className="cms-login-page">

            <div className="cms-login-overlay" />

            <section className="cms-login-shell">

                {/* LEFT BRAND AREA */}
                <div className="cms-brand-panel">

                    <div className="cms-brand">

                        <div className="cms-mark">
                            <span />
                            <span />
                            <span />
                            <span />
                        </div>

                        <div className="cms-name">
                            CMS
                        </div>

                        <div className="cms-brand-divider" />

                        <div className="cms-full-name">
                            COLLATERAL
                            <br />
                            MANAGEMENT
                            <br />
                            SOLUTION
                        </div>

                    </div>

                    <div className="cms-hero-copy">

                        <h1>
                            Securing Assets
                            <span>
                                Powering Opportunities
                            </span>
                        </h1>

                        <p>
                            A centralized platform for
                            collateral lifecycle management,
                            valuation, documentation and
                            monitoring.
                        </p>

                    </div>

                    <div className="cms-benefits">

                        <div className="cms-benefit">
                            <div className="cms-benefit-icon">
                                <ShieldCheck size={27} />
                            </div>

                            <span>
                                Secure
                                <br />
                                Access
                            </span>
                        </div>

                        <div className="cms-benefit-divider" />

                        <div className="cms-benefit">
                            <div className="cms-benefit-icon">
                                <FileText size={27} />
                            </div>

                            <span>
                                Streamlined
                                <br />
                                Process
                            </span>
                        </div>

                        <div className="cms-benefit-divider" />

                        <div className="cms-benefit">
                            <div className="cms-benefit-icon">
                                <BarChart3 size={27} />
                            </div>

                            <span>
                                Better
                                <br />
                                Decisions
                            </span>
                        </div>

                    </div>

                    <div className="cms-values">
                        <span />
                        TRUST
                        <b>|</b>
                        TRANSPARENCY
                        <b>|</b>
                        VALUE
                    </div>

                </div>

                {/* LOGIN AREA */}
                <div className="cms-login-panel">

                    <div className="cms-login-card">

                        <header className="cms-login-header">
                            <h2>
                                Welcome Back
                            </h2>

                            <p>
                                Sign in to your Collateral
                                Management Solution account
                            </p>
                        </header>

                        {error && (
                            <div
                                className="cms-login-error"
                                role="alert"
                            >
                                {error}
                            </div>
                        )}

                        <form
                            onSubmit={submit}
                            className="cms-login-form"
                        >

                            <div className="cms-field">

                                <label htmlFor="email">
                                    Business Email
                                    <span>*</span>
                                </label>

                                <div className="cms-input-wrap">

                                    <Mail
                                        className="cms-input-icon"
                                        size={20}
                                    />

                                    <input
                                        id="email"
                                        value={email}
                                        onChange={e =>
                                            setEmail(
                                                e.target.value
                                            )
                                        }
                                        type="email"
                                        autoComplete="username"
                                        placeholder="name@bank.com"
                                        required
                                    />

                                </div>

                            </div>

                            <div className="cms-field">

                                <label htmlFor="password">
                                    Password
                                    <span>*</span>
                                </label>

                                <div className="cms-input-wrap">

                                    <LockKeyhole
                                        className="cms-input-icon"
                                        size={20}
                                    />

                                    <input
                                        id="password"
                                        value={password}
                                        onChange={e =>
                                            setPassword(
                                                e.target.value
                                            )
                                        }
                                        type={
                                            showPassword
                                                ? "text"
                                                : "password"
                                        }
                                        autoComplete="current-password"
                                        placeholder="Enter your password"
                                        required
                                    />

                                    <button
                                        type="button"
                                        className="cms-password-toggle"
                                        onClick={() =>
                                            setShowPassword(
                                                value => !value
                                            )
                                        }
                                        aria-label={
                                            showPassword
                                                ? "Hide password"
                                                : "Show password"
                                        }
                                    >
                                        {showPassword ? (
                                            <EyeOff size={19} />
                                        ) : (
                                            <Eye size={19} />
                                        )}
                                    </button>

                                </div>

                            </div>

                            <div className="cms-login-meta">

                                <label className="cms-remember">
                                    <input
                                        type="checkbox"
                                    />
                                    <span>
                                        Keep me signed in
                                    </span>
                                </label>

                                <button
                                    type="button"
                                    className="cms-reset-link"
                                >
                                    Reset password
                                </button>

                            </div>

                            <button
                                type="submit"
                                className="cms-signin-button"
                                disabled={busy}
                            >
                                <span>
                                    {busy
                                        ? "Signing in..."
                                        : "Continue"}
                                </span>

                                {!busy && (
                                    <ArrowRight size={19} />
                                )}
                            </button>

                        </form>

                        <div className="cms-secure-divider">
                            <span />
                            <p>
                                SECURE CORPORATE ACCESS
                            </p>
                            <span />
                        </div>

                        <footer className="cms-login-footer">
                            Authorized users only.
                            All activities may be monitored
                            and audited.
                        </footer>

                    </div>

                </div>

            </section>

        </main>
    );
}