import {useState} from "react";
import {type LoginRequestDto} from "@core/generated-client.ts";
import {authApi} from "@utilities/authApi.ts";
import toast from "react-hot-toast";
import {useNavigate} from "react-router";
import {useSetAtom} from "jotai";
import {currentUserAtom} from "@components/dashboard/state/gameAtoms";
import {ApiException} from "@core/generated-client.ts";

type LoginRole = "player" | "admin";

async function extractToken(response: { data: Blob; headers?: Record<string, string> } | null) {
    if (!response) return null;

    const headerToken = response.headers?.authorization || response.headers?.Authorization;
    if (typeof headerToken === "string" && headerToken.trim().length > 0) {
        return headerToken.replace(/^Bearer\s+/i, "").trim();
    }

    if (!response.data) return null;

    try {
        const text = await response.data.text();
        const parsed = JSON.parse(text) as { token?: string; Token?: string };
        const token = parsed.token ?? parsed.Token;

        if (token && token.trim().length > 0) {
            return token.trim();
        }

        const raw = text.trim();
        return raw.length > 0 ? raw.replace(/^"|"$/g, "") : null;
    } catch (error) {
        console.error("Unable to parse login response", error);
        return null;
    }

}


export default function Auth() {

    const navigate = useNavigate();
    const setCurrentUser = useSetAtom(currentUserAtom);

    const [credentials, setCredentials] = useState<LoginRequestDto>({
        email: "",
        password: "",
    });
    const [role, setRole] = useState<LoginRole>("player");
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleLogin = async () => {
        setIsSubmitting(true);
        try {
            const response =
                role === "player"
                    ? await authApi.playerLogin(credentials)
                    : await authApi.adminLogin(credentials);

            const token = await extractToken(response);

            if (!token) {
                toast.error("Login failed: no token returned");
                return;
            }

            const formattedToken = token.startsWith("Bearer ") ? token : `Bearer ${token}`;

            localStorage.setItem("jwt", formattedToken);
            localStorage.setItem("userRole", role);
            localStorage.setItem("userName", credentials.email);

            // if player login, store playerId returned from backend
            const bodyText = response?.data ? await response.data.text() : null;
            const body = bodyText ? JSON.parse(bodyText) : null;

            if (role === "player") {
                const bodyText = response?.data ? await response.data.text() : null;
                const body = bodyText ? JSON.parse(bodyText) : null;

                const bodyPlayerId = body?.playerId ?? body?.PlayerId;

                if (bodyPlayerId) {
                    localStorage.setItem("playerId", String(bodyPlayerId).trim());
                } else {
                    // if backend doesn't send playerId, DO NOT guess from JWT (it caused your mismatch)
                    localStorage.removeItem("playerId");
                }
            } else {
                //admin shouldnt keep a player id obvi
                localStorage.removeItem("playerId");
            }
            
            setCurrentUser({
                name: credentials.email,
                role,
            });

            toast.success("Signed in successfully");
            navigate(role === "player" ? "/player" : "/admin");
        } catch (error) {
            console.error(error);

            if (error instanceof ApiException) {
                toast.error(
                    error.response ||
                    "Invalid credentials. Double-check the email/password and role (Admin vs Player)."
                );
            } else {
                toast.error("Invalid credentials. Double-check the email/password and role (Admin vs Player).");
            }
            localStorage.removeItem("jwt");
        } finally {
            setIsSubmitting(false);
        }
    };

    const canSubmit = credentials.email.trim() !== "" && credentials.password.trim() !== "";
    
    return (

        <div
            className="flex min-h-screen items-center justify-center bg-gradient-to-br from-[#fef3ec] via-[#fffaf6] to-[#f3f4ff] px-4 py-12 text-slate-800">
            <div className="w-full max-w-xl space-y-8 rounded-3xl bg-white/90 p-12 shadow-2xl shadow-orange-100">
                <div className="flex flex-col gap-2 text-center">
                    <span
                        className="mx-auto inline-flex items-center gap-2 rounded-full bg-orange-50 px-4 py-2 text-xs font-semibold uppercase tracking-[0.28em] text-[#f1812c]">
                        Dead Pigeons
                    </span>
                    <h1 className="text-3xl font-semibold text-slate-700">Sign in</h1>
                </div>

                <div className="grid grid-cols-2 gap-2 rounded-2xl bg-[#fefbf7] p-2">
                    {(["player", "admin"] as LoginRole[]).map((option) => (
                        <button
                            key={option}
                            type="button"
                            onClick={() => setRole(option)}
                            className={`rounded-xl px-4 py-3 text-sm font-semibold transition ${
                                role === option
                                    ? "bg-[#f1812c] text-white shadow"
                                    : "text-slate-600 hover:bg-orange-50"
                            }`}
                        >
                            
                            {option === "player" ? "Player" : "Admin"}
                        </button>
                    ))}
                </div>
                <div className="space-y-4">
                    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                        Email
                        <input
                            className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="you@example.com"
                            value={credentials.email}
                            onChange={(e) => setCredentials({...credentials, email: e.target.value})}
                            type="email"
                            autoComplete="email"
                            required
                        />
                    </label>
                    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                        Password
                        <input
                            className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="••••••••"
                            value={credentials.password}
                            onChange={(e) => setCredentials({...credentials, password: e.target.value})}
                            type="password"
                            autoComplete="current-password"
                            required
                        />
                    </label>
                </div>

                <button
                    className="w-full rounded-full bg-[#f7a166] px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-orange-200 disabled:cursor-not-allowed disabled:opacity-70"
                    disabled={!canSubmit || isSubmitting}
                    onClick={handleLogin}
                >
                    {isSubmitting ? "Signing in…" : `Sign in as ${role === "player" ? "Player" : "Admin"}`}
                </button>
                </div>
            </div>
    );
}