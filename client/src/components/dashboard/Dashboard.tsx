import {Fragment} from "react";
import {NavLink, Outlet, useNavigate} from "react-router";
import {useSetAtom} from "jotai";
import {currentUserAtom} from "./state/gameAtoms";
import {useRoleGate} from "./hooks/useRoleGate";
import jerneLogo from "../../assets/jerne-if-logo.png";

type NavOption = {label: string; to: string} | {label: string; action: () => void};

const navItems: Extract<NavOption, {to: string}>[] = [
    {label: "Play", to: "."},
    {label: "Deposit", to: "deposit"},
    {label: "My Boards", to: "boards"},
    {label: "History", to: "history"}
] as const;

export default function Dashboard() {
    const currentUser = useRoleGate("player");
    const setCurrentUser = useSetAtom(currentUserAtom);
    const navigate = useNavigate();

    const handleLogout = () => {
        setCurrentUser(null);
        localStorage.removeItem("jwt");
        localStorage.removeItem("userRole");
        localStorage.removeItem("userName")
        navigate("/auth");
    };

    if (!currentUser) {
        return (
            <div className="flex min-h-screen items-center justify-center bg-[#f8f1e7] text-slate-500">
                Redirecting to sign in…
            </div>
        );
    }

    const navOptions: NavOption[] = [...navItems, {label: "Log Out", action: handleLogout}];

    return (
        <div className="min-h-screen bg-[#f8f1e7] text-slate-800">
            <header className="sticky top-0 z-10 border-b border-orange-200 bg-[#fefbf7]/95 backdrop-blur">
                <div
                    className="mx-auto flex max-w-5xl flex-col gap-3 px-6 py-5 md:flex-row md:items-center md:justify-between">
                    <div className="flex items-center gap-4">
                        <img
                            src={jerneLogo}
                            alt="Jerne IF crest"
                            className="h-14 w-14 rounded-full border border-orange-100 bg-white object-cover"
                        />
                        <div>
                            <p className="text-xs uppercase tracking-[0.3em] text-slate-400">Jerne IF Player</p>
                            <h1 className="text-2xl font-semibold text-slate-900">Dead Pigeons</h1>
                            <p className="text-sm text-slate-500">{currentUser ? `Signed in as ${currentUser.name}` : "Session ended"}</p>
                        </div>
                    </div>
                    <nav className="flex flex-wrap items-center text-sm font-semibold text-slate-500">
                        {navOptions.map((item, index) => (
                            <Fragment key={item.label}>
                                {"to" in item ? (
                                    <NavLink
                                        to={item.to}
                                        end={item.to === "."}
                                        className={({isActive}) =>
                                            `px-3 py-1 transition ${
                                                isActive
                                                    ? "text-slate-900"
                                                    : "text-slate-500 hover:text-slate-700"
                                            }`
                                        }
                                    >
                                        {item.label}
                                    </NavLink>
                                ) : (
                                    <button
                                        type="button"
                                        onClick={item.action}
                                        className="px-3 py-1 text-slate-500 transition hover:text-slate-700"
                                    >
                                        {item.label}
                                    </button>
                                )}
                                {index < navOptions.length - 1 && <span className="px-1 text-slate-300">|</span>}
                            </Fragment>
                        ))}
                    </nav>
                </div>
            </header>
            <main className="mx-auto min-h-[calc(100vh-5rem)] max-w-5xl px-6 py-8">
                <Outlet/>
            </main>
        </div>
    );
}