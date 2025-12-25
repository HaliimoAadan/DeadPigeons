import { useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import { gamesApi, type GameDto } from "@utilities/gamesApi";
import {adminApi} from "@utilities/adminApi.ts";

function formatCurrency(amount?: number | null) {
    if (amount === undefined || amount === null) return "0.00";
    return amount.toLocaleString("da-DK", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatDateShort(dateStr?: string | null) {
    if (!dateStr) return "—";
    const d = new Date(dateStr);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleDateString();
}

type UiWinnerLine = {
    winningboardId: string;
    boardId: string;
    playerId: string;
    playerName: string;
    winningNumbersMatched: number;
    timestamp: string;
    payout: number;
};

export default function AdminGameHistoryPage() {

    const [overview, setOverview] = useState<{
        gameId: string;
        winnerCount: number;
        totalPlayers: number;
        totalPrizePool: number;
        payoutPerWinner: number;
        profit30Percent: number;
        winnersPool70Percent: number;
        remainder: number;
        winners: UiWinnerLine[];
    } | null>(null);
    
    const [games, setGames] = useState<GameDto[]>([]);
    const [selectedGameId, setSelectedGameId] = useState<string>("");
    const [loading, setLoading] = useState(true);

    // one function used by both dropdown + default selection
    const loadResults = async (gameId: string) => {
        try {
            const overview = await adminApi.getPayoutOverview(gameId);
            setOverview(overview);
        } catch (err) {
            console.error(err);
            toast.error("Failed to load payout overview");
            setOverview(null);
        }
    };

    useEffect(() => {
        const load = async () => {
            setLoading(true);
            try {
                const [gamesResponse] = await Promise.all([gamesApi.getAll()]);
                const nextGames = Array.isArray(gamesResponse) ? gamesResponse : [];
                setGames(nextGames);

                const sorted = [...nextGames].sort(
                    (a, b) => new Date(b.expirationDate).getTime() - new Date(a.expirationDate).getTime()
                );

                const latest = sorted[0] ?? null;

                if (latest?.gameId) {
                    setSelectedGameId(latest.gameId);
                    await loadResults(latest.gameId);
                    
                } else {
                    setSelectedGameId("");
                    setOverview(null);
                }
            } catch (error) {
                console.error(error);
                toast.error("Could not load games.");
            } finally {
                setLoading(false);
            }
        };

        void load();
    }, []);
    
    const selectedGame = useMemo(
        () => games.find((g) => g.gameId === selectedGameId) ?? null,
        [games, selectedGameId]
    );
    

    const gameStatus = useMemo(() => {
        if (!selectedGame) return "No game selected";
        const hasNumbers = (selectedGame.winningNumbers?.length ?? 0) >= 3;
        const isExpired = new Date(selectedGame.expirationDate) < new Date();
   
        if (!hasNumbers) return "Waiting for winning numbers";
        return isExpired ? "Game completed" : "Results published";
    }, [selectedGame]);

    const stats = useMemo(
        () => [
            { label: "Total players this round", value: overview?.totalPlayers ?? 0 },
            { label: "Total prize money", value: `${formatCurrency(overview?.totalPrizePool)} kr` },
            { label: "Number of winners", value: overview?.winnerCount ?? 0 },
            { label: "Amount each winner won", value: `${formatCurrency(overview?.payoutPerWinner)} kr` },
            { label: "Profit (30%) from total earned", value: `${formatCurrency(overview?.profit30Percent)} kr` },
        ],
        [overview]
    );

    const winners = overview?.winners ?? [];

    return (
        <section className="space-y-6">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Weekly overview</p>

                <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                    <div>
                        <h2 className="text-3xl font-semibold text-slate-900">Game History</h2>
                        <p className="mt-2 max-w-3xl text-slate-600">
                            Review results, prize splits, and winners for completed games.
                        </p>
                    </div>

                    <div className="flex w-full flex-col gap-2 md:w-80">
                        <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                            Select game
                            <select
                                value={selectedGameId}
                                onChange={(event) => {
                                    const nextId = event.target.value;
                                    setSelectedGameId(nextId);
                                    if (nextId) {
                                        void loadResults(nextId);
                                    }                                }}
                                disabled={loading || games.length === 0}
                                className="rounded-2xl border border-orange-100 bg-white px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none disabled:opacity-60"
                            >
                                <option value="" disabled>
                                    {loading ? "Loading games…" : games.length === 0 ? "No games available" : "Select a game"}
                                </option>

                                {games.map((game) => {
                                    const hasNumbers = (game.winningNumbers?.length ?? 0) >= 3;
                                    return (
                                        <option key={game.gameId} value={game.gameId}>
                                            Ends {formatDateShort(game.expirationDate)} — {hasNumbers ? "Results" : "Pending"}
                                        </option>
                                    );
                                })}
                            </select>
                        </label>

                        <div className="rounded-2xl bg-orange-50/60 p-4 shadow-inner">
                            <p className="text-xs uppercase tracking-wide text-slate-500">Game status</p>
                            <p className="mt-1 text-sm font-semibold text-slate-900">{gameStatus}</p>
                            <p className="mt-1 text-xs text-slate-500">
                                {selectedGame?.winningNumbers?.length
                                    ? `Winning numbers: ${selectedGame.winningNumbers.join(", ")}`
                                    : "Winning numbers not yet published"}
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            <div className="grid gap-4 md:grid-cols-5">
                {stats.map((stat) => (
                    <article key={stat.label} className="rounded-3xl bg-white/80 p-5 text-center shadow-lg shadow-orange-100">
                        <p className="text-xs uppercase tracking-wide text-slate-400">{stat.label}</p>
                        <p className="mt-2 text-2xl font-semibold text-slate-900">{stat.value}</p>
                    </article>
                ))}
            </div>

            <article className="space-y-4 rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Winning players</p>
                    <h3 className="text-2xl font-semibold text-slate-900">Payout overview</h3>
                </div>
                
                {(!overview || !selectedGameId) && (
                    <p className="rounded-2xl bg-orange-50 px-4 py-3 text-sm text-slate-600">Select a game to see results.</p>
                )}

                {overview && winners.length === 0 && (
                    <p className="rounded-2xl bg-orange-50 px-4 py-3 text-sm text-slate-600">No winners this round.</p>
                )}

                {overview && winners.length > 0 && (
                    <div className="overflow-x-auto rounded-2xl bg-white/90 p-2 shadow-inner">
                        <table className="min-w-full divide-y divide-orange-100 text-left text-sm">
                            <thead className="bg-[#fef7ef] text-xs uppercase tracking-wide text-slate-500">
                            <tr>
                                <th className="px-4 py-3">Player</th>
                                <th className="px-4 py-3">Email</th>
                                <th className="px-4 py-3">Amount won</th>
                                <th className="px-4 py-3">Board</th>
                            </tr>
                            </thead>
                            <tbody className="divide-y divide-orange-50">
                            {winners.map((winner: any) => {
                                const name = winner.playerName || "Unknown";
                                const email = "—";

                                return (
                                    <tr key={winner.winningboardId} className="transition hover:bg-[#fff8f0]">
                                        <td className="px-4 py-3 font-semibold text-slate-800">{name}</td>
                                        <td className="px-4 py-3 text-slate-600">{email}</td>
                                        <td className="px-4 py-3 text-slate-800">{formatCurrency(winner.payout)} kr</td>
                                        <td className="px-4 py-3 text-slate-500">{winner.boardId}</td>
                                    </tr>
                                );
                            })}
                            </tbody>
                        </table>
                    </div>
                )}
            </article>
        </section>
    );
}
