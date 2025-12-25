import { useEffect, useMemo, useState } from "react";
import { useAtom } from "jotai";
import toast from "react-hot-toast";
import NumberGrid from "../NumberGrid";
import { adminSelectionAtom } from "../state/gameAtoms";
import { gamesApi, type GameDto } from "@utilities/gamesApi";
import {adminApi} from "@utilities/adminApi.ts";


function formatDate(value?: string) {
    if (!value) return "—";
    const date = new Date(value);
    return date.toLocaleString();
}

function toDateTimeLocal(iso: string) {
    const date = new Date(iso);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");

    return `${year}-${month}-${day}T${hours}:${minutes}`;
}



export default function AdminWinningNumbersPage() {
    const [selectedNumbers, setSelectedNumbers] = useAtom(adminSelectionAtom);
    const [games, setGames] = useState<GameDto[]>([]);
    const [selectedGameId, setSelectedGameId] = useState<string>("");
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    type UiOverview = {
        gameId: string;
        winnerCount: number;
        totalPlayers: number;
        totalPrizePool: number;
        payoutPerWinner: number;
        profit30Percent: number;
        winnersPool70Percent: number;
        remainder: number;
        winners: any[];
    };

    const [overview, setOverview] = useState<UiOverview | null>(null);
    const [expirationInput, setExpirationInput] = useState<string>("");

    const pickDefaultGameId = (list: GameDto[]) => {
        const openGame = list.find((game) => (game.winningNumbers?.length ?? 0) < 3);
        return openGame?.gameId ?? list[0]?.gameId ?? "";
    };

    const sortedGames = useMemo(
        () => [...games].sort((a, b) => new Date(b.expirationDate).getTime() - new Date(a.expirationDate).getTime()),
        [games]
    );

    const activeGame = useMemo(
        () => sortedGames.find((game) => game.gameId === selectedGameId) ?? null,
        [selectedGameId, sortedGames]
    );

    const canPublish =
        selectedNumbers.length === 3 && !!activeGame && (activeGame.winningNumbers?.length ?? 0) < 3;

    // hared loader using backend payout overview
    // shared loader using backend payout overview
    const loadResults = async (gameId: string) => {
        try {
            const [overviewResponse] = await Promise.all([adminApi.getPayoutOverview(gameId)]);

            setOverview(overviewResponse);
        } catch (error) {
            console.error(error);
            setOverview(null);

        }
    };
    useEffect(() => {
        const loadGames = async () => {
            try {
                const response = await gamesApi.getAll();
                setGames(response);

                const defaultId = pickDefaultGameId(response);
                setSelectedGameId(defaultId);
                if (defaultId) void loadResults(defaultId);
            } catch (error) {
                console.error(error);
                toast.error("Could not load games");
            } finally {
                setLoading(false);
            }
        };

        void loadGames();
    }, []);

    useEffect(() => {
        if (!selectedGameId) return;
        void loadResults(selectedGameId);
    }, [games, selectedGameId]);

    const toggleNumber = (value: number) => {
        setSelectedNumbers((prev) => (prev.includes(value) ? prev.filter((x) => x !== value) : [...prev, value]));
    };

    const handlePublish = async () => {
        if (!activeGame) return;
        setSaving(true);

        try {
            await adminApi.publishWinningNumbers(activeGame.gameId, selectedNumbers);

            toast.success("Winning numbers saved for this game.");
            setSelectedNumbers([]);

            const refreshed = await gamesApi.getAll();
            setGames(refreshed);

            // keep same game selected + reload results for it
            setSelectedGameId(activeGame.gameId);
            await loadResults(activeGame.gameId);
        } catch (error) {
            console.error(error);
            toast.error("Could not publish winning numbers.");
        } finally {
            setSaving(false);
        }
    };

    const handleCreateGame = async () => {
        setSaving(true);
        try {
            const targetIso = (() => {
                if (expirationInput) {
                    const parsed = new Date(expirationInput);
                    if (!Number.isNaN(parsed.getTime())) return parsed.toISOString();
                }
                return new Date(Date.now() + 5 * 60 * 1000).toISOString();
            })();

            const created = await gamesApi.create(targetIso);
            const refreshed = await gamesApi.getAll();
            setGames(refreshed);
            setSelectedGameId(created.gameId);
            setExpirationInput("");
            toast.success("New game created.");
        } catch (error) {
            console.error(error);
            toast.error("Could not create a new game.");
        } finally {
            setSaving(false);
        }
    };

    const winners = overview?.winners ?? [];

    const uniqueWinningPlayers = useMemo(() => {
        const ids = new Set(winners.map((w: any) => w.playerId));
        return ids.size;
    }, [winners]);

    const gameStatusLabel = activeGame
        ? (activeGame.winningNumbers?.length ?? 0) >= 3
            ? "Winning numbers already published"
            : "Awaiting winning numbers"
        : "No game selected";

    return (
        <section className="space-y-8">
            <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Admin only</p>
                    <h2 className="text-3xl font-semibold text-slate-900">Publish Winning Numbers</h2>
                    <p className="mt-2 max-w-2xl text-slate-600">Select the three numbers that were drawn.</p>
                </div>

                <div className="rounded-2xl bg-orange-50/60 p-4 text-sm text-slate-700 shadow-inner">
                    <p className="text-xs uppercase tracking-wide text-slate-500">Quick game (5 min)</p>
                    <p className="text-xs text-slate-500">Sets expiration 5 minutes from now for testing.</p>
                    <button
                        type="button"
                        className="mt-2 w-full rounded-full bg-[#f7a166] px-4 py-2 text-xs font-semibold text-white shadow-orange-200"
                        onClick={() => {
                            const fiveMinutes = new Date(Date.now() + 5 * 60 * 1000).toISOString();
                            setExpirationInput(toDateTimeLocal(fiveMinutes));
                        }}
                    >
                        Fill date picker
                    </button>
                </div>
            </div>

            <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
                <div className="space-y-6 rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                        <div>
                            <p className="text-xs uppercase tracking-wide text-slate-400">Target game</p>
                            <p className="text-lg font-semibold text-slate-800">
                                {activeGame ? new Date(activeGame.expirationDate).toDateString() : "No game selected"}
                            </p>
                            <p className="text-sm text-slate-500">{gameStatusLabel}</p>
                        </div>

                        <div className="flex flex-wrap gap-2">
                            <button
                                type="button"
                                onClick={handleCreateGame}
                                disabled={saving}
                                className="rounded-full bg-[#f7a166] px-4 py-2 text-sm font-semibold text-white shadow-orange-200 transition hover:shadow-lg disabled:cursor-not-allowed disabled:opacity-70"
                            >
                                Create game
                            </button>

                            <label
                                className="flex items-center gap-2 rounded-2xl border border-orange-100 bg-white px-3 py-2 text-xs text-slate-600 shadow-inner">
                                <span>Expiration</span>
                                <input
                                    type="datetime-local"
                                    value={expirationInput}
                                    onChange={(event) => setExpirationInput(event.target.value)}
                                    className="rounded-xl border border-orange-100 px-2 py-1 text-xs focus:border-orange-300 focus:outline-none"
                                />
                            </label>
                        </div>
                    </div>

                    <div className="grid gap-3 md:grid-cols-2">
                        {sortedGames.map((game) => {
                            const numbersPublished = (game.winningNumbers?.length ?? 0) >= 3;
                            const isSelected = game.gameId === activeGame?.gameId;
                            const publishedAt = numbersPublished ? formatDate(game.drawDate ?? undefined) : null;

                            return (
                                <button
                                    type="button"
                                    key={game.gameId}
                                    onClick={() => setSelectedGameId(game.gameId)}
                                    className={`flex flex-col gap-1 rounded-2xl border px-4 py-3 text-left shadow-sm transition ${
                                        isSelected ? "border-[#f7a166] bg-[#fff4ea]" : "border-orange-100 bg-white"
                                    }`}
                                >
                                    <div className="flex items-center justify-between text-sm text-slate-600">
                                        <span>{new Date(game.expirationDate).toLocaleDateString()}</span>
                                        <span
                                            className={`rounded-full px-3 py-1 text-xs font-semibold ${
                                                numbersPublished ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"
                                            }`}
                                        >
                      {numbersPublished ? "Published" : "Awaiting numbers"}
                    </span>
                                    </div>

                                    <p className="text-xs text-slate-500">Game ID: {game.gameId}</p>

                                    {numbersPublished && <p className="text-sm font-medium text-slate-700">Winning
                                        numbers: {game.winningNumbers?.join(", ")}</p>}
                                    {publishedAt &&
                                        <p className="text-[11px] text-slate-500">Published: {publishedAt}</p>}
                                </button>
                            );
                        })}
                    </div>

                    <NumberGrid selectedNumbers={selectedNumbers} onToggle={toggleNumber} maxSelectable={3}/>

                    <div className="mt-4 flex flex-col items-center gap-3 text-center">
                        <p className="text-sm text-slate-500">Exactly three numbers are required.</p>
                        <button
                            type="button"
                            disabled={!canPublish || saving}
                            onClick={handlePublish}
                            className={`w-64 rounded-full px-6 py-3 text-lg font-semibold transition ${
                                canPublish ? "bg-emerald-500 text-white shadow-xl shadow-emerald-200" : "bg-slate-200 text-slate-500"
                            }`}
                        >
                            {saving ? "Saving…" : "Publish Winning Numbers"}
                        </button>
                    </div>

                    <div className="grid gap-4 md:grid-cols-4">
                        {[
                            {label: "Winning boards", value: overview?.winnerCount ?? 0},
                            {label: "Unique winning players", value: uniqueWinningPlayers},
                            {
                                label: "Prize pool (70%)",
                                value: overview ? `${overview.winnersPool70Percent.toFixed(2)} kr` : "—"
                            },
                            {
                                label: "Payout per winner",
                                value: overview ? `${overview.payoutPerWinner.toFixed(2)} kr` : "—"
                            },
                        ].map((stat) => (
                            <article key={stat.label}
                                     className="rounded-3xl bg-white/80 p-4 text-center shadow-lg shadow-orange-100">
                                <p className="text-xs uppercase tracking-wide text-slate-400">{stat.label}</p>
                                <p className="mt-2 text-xl font-semibold text-slate-900">{stat.value}</p>
                            </article>
                        ))}
                    </div>

                    <article className="space-y-4 rounded-3xl bg-white/90 p-4 shadow-inner">
                        {!overview &&
                            <p className="rounded-2xl bg-orange-50 px-4 py-3 text-sm text-slate-600">Select a game
                                to
                                see winners.</p>}
                        {overview && winners.length === 0 &&
                            <p className="rounded-2xl bg-orange-50 px-4 py-3 text-sm text-slate-600">No winners
                                yet.</p>}

                        {overview && winners.length > 0 && (
                            <div className="overflow-x-auto rounded-2xl bg-white/90 p-2 shadow-inner">
                                <table className="min-w-full divide-y divide-orange-100 text-left text-sm">
                                    <thead className="bg-[#fef7ef] text-xs uppercase tracking-wide text-slate-500">
                                    <tr>
                                        <th className="px-4 py-3">Name</th>
                                        <th className="px-4 py-3">Player ID</th>
                                        <th className="px-4 py-3">Board</th>
                                        <th className="px-4 py-3">Matched</th>
                                        <th className="px-4 py-3">Prize</th>
                                        <th className="px-4 py-3">Timestamp</th>
                                    </tr>
                                    </thead>
                                    <tbody className="divide-y divide-orange-50">
                                    {winners.map((winner: any) => (
                                        <tr key={winner.winningboardId} className="transition hover:bg-[#fff8f0]">
                                            <td className="px-4 py-3 font-semibold text-slate-800">{winner.playerName || "—"}</td>
                                            <td className="px-4 py-3 text-slate-600">{winner.playerId}</td>
                                            <td className="px-4 py-3 text-slate-600">{winner.boardId}</td>
                                            <td className="px-4 py-3 text-slate-600">{winner.winningNumbersMatched}</td>
                                            <td className="px-4 py-3 text-slate-800">{winner.payout?.toFixed?.(2) ?? "0.00"} kr</td>
                                            <td className="px-4 py-3 text-slate-500">{formatDate(winner.timestamp)}</td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </article>
                </div>

                <aside className="space-y-4 rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                        Select game
                        <select
                            value={selectedGameId}
                            onChange={(event) => setSelectedGameId(event.target.value)}
                            disabled={loading || games.length === 0}
                            className="rounded-2xl border border-orange-100 bg-white px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none disabled:opacity-60"
                        >
                            <option value="" disabled>
                                {loading ? "Loading games…" : games.length === 0 ? "No games available" : "Select a game"}
                            </option>
                            {sortedGames.map((game) => {
                                const hasNumbers = (game.winningNumbers?.length ?? 0) >= 3;
                                return (
                                    <option key={game.gameId} value={game.gameId}>
                                        Ends {new Date(game.expirationDate).toLocaleDateString()} — {hasNumbers ? "Results" : "Pending"}
                                    </option>
                                );
                            })}
                        </select>
                    </label>

                    <div className="rounded-2xl bg-orange-50/60 p-4 shadow-inner">
                        <p className="text-xs uppercase tracking-wide text-slate-500">Game status</p>
                        <p className="mt-1 text-sm font-semibold text-slate-900">{gameStatusLabel}</p>
                        <p className="mt-1 text-xs text-slate-500">
                            {activeGame?.winningNumbers?.length ? `Winning numbers: ${activeGame.winningNumbers.join(", ")}` : "Winning numbers not yet published"}
                        </p>
                        <p className="mt-2 text-xs text-slate-500">Expiration: {formatDate(activeGame?.expirationDate)}</p>
                    </div>
                </aside>
            </div>
        </section>

    );
}