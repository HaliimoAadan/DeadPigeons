import {useEffect, useMemo, useState} from "react";
import {useNavigate} from "react-router-dom";
import toast from "react-hot-toast";
import {playersApi, type PlayerResponseDto} from "@utilities/playersApi.ts";
import {transactionsApi, type AdminTransactionListItem} from "@utilities/transactionsApi.ts";
import {gamesApi, type GameDto} from "@utilities/gamesApi.ts";

type AdminStats = {
    totalPlayers: number;
    activePlayers: number;
    pendingDeposits: number;
    gameState: string;
};

const notices = [
    {
        title: 'Balance approvals',
        detail: 'Review incoming MobilePay deposits before publishing weekly results.',
        cta: 'Review deposits'
    },
    {
        title: 'Inactive players',
        detail: 'Send a quick reminder to players who have not been active yet.',
        cta: 'Send reminder'
    }
];

export default function AdminDashboardPage() {
    
    const [players, setPlayers] = useState<PlayerResponseDto[]>([]);
    const [transactions, setTransactions] = useState<AdminTransactionListItem[]>([]);
    const [games, setGames] = useState<GameDto[]>([]);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    useEffect(() => {
        const load = async () => {
            try {
                const [playersResponse, transactionResponse, gamesResponse] = await Promise.all([
                    playersApi.getAll(),
                    transactionsApi.list(),
                    gamesApi.getAll()
                ]);

                const nextPlayers = Array.isArray(playersResponse) ? playersResponse : [];
                const nextTransactions = Array.isArray(transactionResponse) ? transactionResponse : [];
                const nextGames = Array.isArray(gamesResponse) ? gamesResponse : [];

                if (!Array.isArray(playersResponse) || !Array.isArray(transactionResponse)  || !Array.isArray(gamesResponse)) {
                    toast.error("Unexpected dashboard data format.");
                }

                setPlayers(nextPlayers);
                setTransactions(nextTransactions);
                setGames(nextGames);
                
            } catch (error) {
                console.error(error);
                toast.error("Could not load dashboard metrics.");
            } finally {
                setLoading(false);
            }
        };

        load();
    }, []);

    const stats: AdminStats = useMemo(() => {
        const latestGame = [...games]
            .sort((a, b) => new Date(b.expirationDate).getTime() - new Date(a.expirationDate).getTime())[0];

        const hasNumbers = (latestGame?.winningNumbers?.length ?? 0) >= 3;
        const isExpired = latestGame ? new Date(latestGame.expirationDate) < new Date() : false;

        const gameState = !latestGame
            ? "Waiting for winning numbers"
            : hasNumbers && isExpired
                ? "Game completed"
                : hasNumbers
                    ? "Game active"
                    : "Waiting for winning numbers";

        return {
            totalPlayers: players.length,
            activePlayers: players.filter((player) => player.isActive).length,
            pendingDeposits: transactions.filter((tx) => tx.status === "Pending").length,
            gameState
        };
    }, [games, players, transactions]);

    const handleSendReminder = () => {
        const inactive = players.filter((player) => !player.isActive);
        if (inactive.length === 0) {
            toast.success("No inactive players to remind right now.");
            return;
        }

        const names = inactive
            .map((player) => player.fullName?.trim())
            .filter(Boolean)
            .join(", ");

        toast.success(`Reminders sent to ${inactive.length} inactive player${inactive.length === 1 ? "" : "s"}${names ? ` (${names})` : ""}.`);
    };

    const handleReviewDeposits = () => {
        if (stats.pendingDeposits === 0) {
            toast.success("No pending deposits to review.");
            return;
        }

        navigate("/admin/review-deposits");
    };
    
    return (
        <section className="space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Overview</p>
                    <h2 className="text-3xl font-semibold text-slate-900">Admin's Dashboard</h2>
                    <p className="mt-2 max-w-3xl text-slate-600">
                        Keep tabs on Jerne IF's Dead Pigeons game before entering the weekly results. These widgets summarize the
                        operations you need to take care of before the Saturday deadline.
                    </p>
                </div>
                <button
                    type="button"
                    onClick={() => navigate("/admin/game-history")}
                    className="rounded-full bg-[#f7a166] px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-orange-200"
                >
                    View game history
                </button>
            </div>
            <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-4">
                {[{
                    label: "Total players",
                    value: stats.totalPlayers
                }, {
                    label: "Active players",
                    value: stats.activePlayers
                }, {
                    label: "Pending deposits",
                    value: stats.pendingDeposits
                }, {
                    label: "Current week",
                    value: stats.gameState,
                    helper: stats.gameState === "Running (ends Saturday)" ? "Game ends Saturday" : undefined
                }].map((stat) => (

                    <article
                        key={stat.label}
                        className="flex min-h-[96px] flex-col items-center justify-center gap-1 rounded-3xl bg-white/90 px-4 py-3 text-center shadow-lg shadow-orange-100"
                    >
                        <p className="text-xs uppercase tracking-wide text-slate-400">{stat.label}</p>
                        <p
                            className={`font-semibold leading-tight text-slate-900 ${typeof stat.value === "number" ? "text-2xl" : "text-base"}`}
                        >                           
                            {loading ? "…" : stat.value}
                        </p>
                        {stat.helper && !loading && (
                            <p className="text-xs text-slate-500">{stat.helper}</p>
                        )}
                    </article>
                ))}
            </div>
            <div className="grid gap-4 md:grid-cols-2">
                {notices.map((notice) => {
                    const isReminder = notice.cta === "Send reminder";
                    const onClick = isReminder ? handleSendReminder : handleReviewDeposits;

                    return (
                        <article key={notice.title} className="rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                            <p className="text-sm uppercase tracking-wide text-slate-400">{notice.title}</p>
                            <p className="mt-2 text-lg font-semibold text-slate-900">{notice.detail}</p>

                            <button
                                type="button"
                                onClick={onClick}
                                className="mt-4 rounded-full bg-[#f7a166] px-4 py-2 text-sm font-semibold text-white shadow-orange-200 transition hover:shadow-lg"
                            >
                                {notice.cta}
                            </button>
                        </article>
                    );
                })}
            </div>
        </section>
    );
}