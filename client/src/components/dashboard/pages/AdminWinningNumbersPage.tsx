import {useEffect, useMemo, useState} from "react";
import {useAtom} from "jotai";
import toast from "react-hot-toast";
import NumberGrid from "../NumberGrid";
import {adminSelectionAtom} from "../state/gameAtoms";
import {gamesApi, type GameDto} from "@utilities/gamesApi.ts";
import {adminApi} from "@utilities/adminApi.ts";

export default function AdminWinningNumbersPage() {
    const [selectedNumbers, setSelectedNumbers] = useAtom(adminSelectionAtom);
    const [games, setGames] = useState<GameDto[]>([]);
    const [selectedGameId, setSelectedGameId] = useState<string>("");
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    const pickDefaultGameId = (list: GameDto[]) => {
        const openGame = list.find((game) => (game.winningNumbers?.length ?? 0) < 3);
        return openGame?.gameId ?? list[0]?.gameId ?? "";
    };

    const toggleNumber = (value: number) => {
        setSelectedNumbers((prev) =>
            prev.includes(value)
                ? prev.filter((item) => item !== value)
                : [...prev, value]
        );
    };

    useEffect(() => {
        const loadGames = async () => {
            try {
                const response = await gamesApi.getAll();
                setGames(response);

                setSelectedGameId(pickDefaultGameId(response));
            } catch (error) {
                console.error(error);
                toast.error("Could not load games");
            } finally {
                setLoading(false);
            }
        };

        loadGames();
    }, []);

    const sortedGames = useMemo(
        () => [...games].sort((a, b) => new Date(b.expirationDate).getTime() - new Date(a.expirationDate).getTime()),
        [games]
    );

    const activeGame = useMemo(
        () => sortedGames.find((game) => game.gameId === selectedGameId) ?? null,
        [selectedGameId, sortedGames]
    );

    const canPublish = selectedNumbers.length === 3 && !!activeGame && (activeGame.winningNumbers?.length ?? 0) < 3;

    const handlePublish = async () => {
        if (!activeGame) return;
        setSaving(true);

        try {
            await adminApi.publishWinningNumbers(activeGame.gameId, selectedNumbers);
            toast.success("Winning numbers saved for this game.");
            setSelectedNumbers([]);

            const refreshed = await gamesApi.getAll();
            setGames(refreshed);
            setSelectedGameId(pickDefaultGameId(refreshed));
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
            const expiresInFiveMinutes = (() => {
                const now = new Date();
                const fiveMinutesFromNow = new Date(now.getTime() + 5 * 60 * 1000);
                return fiveMinutesFromNow.toISOString();
            })();

            const created = await gamesApi.create(expiresInFiveMinutes);
            const refreshed = await gamesApi.getAll();
            setGames(refreshed);
            setSelectedGameId(created.gameId);
            toast.success("New game created for this week.");
        } catch (error) {
            console.error(error);
            toast.error("Could not create a new game.");
        } finally {
            setSaving(false);
        }
    };

    const gameStatusLabel = activeGame
        ? (activeGame.winningNumbers?.length ?? 0) >= 3
            ? "Winning numbers already published"
            : "Awaiting winning numbers"
        : "No game selected";
    
    
    return (
        <section className="space-y-8">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Admin only</p>
                <h2 className="text-3xl font-semibold text-slate-900">Publish Winning Numbers</h2>
                <p className="mt-2 max-w-2xl text-slate-600">
                    Select the three numbers that were drawn from the hat this Saturday.
                </p>
            </div>
            <div className="space-y-4 rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                    <div>
                        <p className="text-xs uppercase tracking-wide text-slate-400">Target game</p>
                        <p className="text-lg font-semibold text-slate-800">{activeGame ? new Date(activeGame.expirationDate).toDateString() : "No game selected"}</p>
                        <p className="text-sm text-slate-500">{gameStatusLabel}</p>
                    </div>
                    <div className="flex flex-wrap gap-2">
                        <button
                            type="button"
                            onClick={handleCreateGame}
                            disabled={saving}
                            className="rounded-full bg-[#f7a166] px-4 py-2 text-sm font-semibold text-white shadow-orange-200 transition hover:shadow-lg disabled:cursor-not-allowed disabled:opacity-70"
                        >
                            Start next game
                        </button>
                    </div>
                </div>
                <div className="grid gap-3 md:grid-cols-2">
                    {sortedGames.map((game) => {
                        const numbersPublished = (game.winningNumbers?.length ?? 0) >= 3;
                        const isSelected = game.gameId === activeGame?.gameId;

                        return (
                            <button
                                type="button"
                                key={game.gameId}
                                onClick={() => setSelectedGameId(game.gameId)}
                                className={`flex flex-col gap-1 rounded-2xl border px-4 py-3 text-left shadow-sm transition ${
                                    isSelected ? "border-[#f7a166] bg-[#fff4ea]" : "border-orange-100 bg-white"}`}
                            >
                                <div className="flex items-center justify-between text-sm text-slate-600">
                                    <span>{new Date(game.expirationDate).toLocaleDateString()}</span>
                                    <span className={`rounded-full px-3 py-1 text-xs font-semibold ${numbersPublished ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"}`}>
                                        {numbersPublished ? "Published" : "Awaiting numbers"}
                                    </span>
                                </div>
                                <p className="text-xs text-slate-500">Game ID: {game.gameId}</p>
                                {numbersPublished && (
                                    <p className="text-sm font-medium text-slate-700">Winning numbers: {game.winningNumbers?.join(", ")}</p>
                                )}
                            </button>
                        );
                    })}
                    {sortedGames.length === 0 && (
                        <p className="rounded-2xl border border-dashed border-orange-200 bg-orange-50 px-4 py-3 text-sm text-slate-600">Create a game to publish numbers.</p>
                    )}
                </div>
                
                <NumberGrid selectedNumbers={selectedNumbers} onToggle={toggleNumber} maxSelectable={3}/>
                <div className="mt-6 flex flex-col items-center gap-4">
                    <p className="text-sm text-slate-500">Exactly three numbers are required.</p>
                    <button
                        type="button"
                        disabled={!canPublish || saving}
                        onClick={handlePublish}
                        className={`w-64 rounded-full px-6 py-3 text-lg font-semibold transition ${
                            canPublish
                                ? "bg-emerald-500 text-white shadow-xl shadow-emerald-200"
                                : "bg-slate-200 text-slate-500"
                        }`}
                    >
                        {saving ? "Saving…" : "Publish Winning Numbers"}
                    </button>
                </div>
            </div>
        </section>
    );
}