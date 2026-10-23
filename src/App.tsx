import {
    Navigate,
    Route,
    Routes
} from "react-router-dom";

import Layout from "./components/Layout";

import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";

import Collateral from "./pages/Collateral";
import CollateralVerification from "./pages/CollateralVerification";
import CollateralReport from "./pages/CollateralReport";

import Valuation from "./pages/Valuation";
import ValuationReview from "./pages/ValuationReview";

import Legal from "./pages/Legal";
import LegalPerfection from "./pages/LegalPerfection";

import Insurance from "./pages/Insurance";
import InsuranceProcessing from "./pages/InsuranceProcessing";

import Archive from "./pages/Archive";
import CollateralRelease from "./pages/CollateralRelease";

import Admin from "./pages/Admin";
import Audit from "./pages/Audit";

export default function App() {
    return (
        <Routes>
            {/* Public */}
            <Route
                path="/login"
                element={<Login />}
            />

            {/* Authenticated Application */}
            <Route element={<Layout />}>

                {/* Dashboard */}
                <Route
                    path="/dashboard"
                    element={<Dashboard />}
                />

                {/* Collateral Register */}
                <Route
                    path="/collateral"
                    element={<Collateral />}
                />

                {/* Individual Collateral Assessment Report */}
                <Route
                    path="/collateral/:id/report"
                    element={<CollateralReport />}
                />

                {/* Collateral Detail / Verification */}
                <Route
                    path="/collateral/:id"
                    element={<CollateralVerification />}
                />

                {/* Valuation Work Queue */}
                <Route
                    path="/valuation"
                    element={<Valuation />}
                />

                {/* Valuation Review */}
                <Route
                    path="/valuation/:id"
                    element={<ValuationReview />}
                />

                {/* Legal Perfection Work Queue */}
                <Route
                    path="/legal"
                    element={<Legal />}
                />

                {/* Legal Perfection Detail */}
                <Route
                    path="/legal/:id"
                    element={<LegalPerfection />}
                />

                {/* Insurance Work Queue */}
                <Route
                    path="/insurance"
                    element={<Insurance />}
                />

                {/* Insurance Processing */}
                <Route
                    path="/insurance/:id"
                    element={<InsuranceProcessing />}
                />

                {/* Archive & Release */}
                <Route
                    path="/archive"
                    element={<Archive />}
                />

                {/* Release Detail */}
                <Route
                    path="/archive/:id"
                    element={<CollateralRelease />}
                />

                {/* Administration */}
                <Route
                    path="/admin"
                    element={<Admin />}
                />

                {/* Audit Trail */}
                <Route
                    path="/audit"
                    element={<Audit />}
                />

            </Route>

            {/* Unknown Routes */}
            <Route
                path="*"
                element={
                    <Navigate
                        to="/login"
                        replace
                    />
                }
            />
        </Routes>
    );
}