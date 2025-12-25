import {useEffect, useMemo, useState} from "react";
import toast from "react-hot-toast";
import {boardsApi, type BoardDto, winningBoardsApi, type WinningBoardDto} from "@utilities/boardsApi.ts";
import {gamesApi, type GameDto} from "@utilities/gamesApi.ts";

const statusStyles: Record<string, string> = {
    Active: "bg-yellow-100 text-yellow-800",
    Winning: "bg-amber-100 text-amber-700",
    Losing: "bg-slate-200 text-slate-600",
};

type HistoryBoard = BoardDto & {
    status: keyof typeof statusStyles;
    matched: number;
    winningNumbers: number[];
    drawDate?: string | null;
};

function formatDateTime(value?: string) {
    if (!value) return "";
    const date = new Date(value);
    return date.toLocaleString();
}

export default function PlayerHistoryPage() {

    const [playerId, setPlayerId] = useState<string>(() => localStorage.getItem("playerId") ?? "");
    const [boards, setBoards] = useState<BoardDto[]>([]);
    const [winningBoards, setWinningBoards] = useState<WinningBoardDto[]>([]);
    const [games, setGames] = useState<GameDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [gameFilter, setGameFilter] = useState<string>("all");
    const [activeBoard, setActiveBoard] = useState<HistoryBoard | null>(null);

    useEffect(() => {
        const stored = localStorage.getItem("playerId") ?? "";
        const trimmed = stored.trim();
        if (trimmed && trimmed !== playerId) setPlayerId(trimmed);
    }, []);


    useEffect(() => {
        const loadStatic = async () => {
            try {
                const response = await gamesApi.getAll();
                setGames(Array.isArray(response) ? response : []);
            } catch (error) {
                console.error(error);
                toast.error("Could not load games.");
            }
        };

        loadStatic();
    }, []);

    const refreshBoards = async (id: string) => {
        const trimmedId = id.trim();
        if (!trimmedId) return;
        setLoading(true);
        try {
            const [boardsResponse, winningResponse, gamesResponse] = await Promise.all([
                boardsApi.list(),
                winningBoardsApi.getAll(),
                gamesApi.getAll()
            ]);

            const playerBoards = (Array.isArray(boardsResponse) ? boardsResponse : []).filter((board) => board.playerId === trimmedId);
            setBoards(playerBoards);
            setWinningBoards(Array.isArray(winningResponse) ? winningResponse : []);
            localStorage.setItem("playerId", trimmedId);
            setPlayerId(trimmedId);
        } catch (error) {
            console.error(error);
            toast.error("Could not load your history.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (playerId) {
            void refreshBoards(playerId);
        }
    }, [playerId]);

    const winningBoardIds = useMemo(() => new Set(winningBoards.map((board) => board.boardId)), [winningBoards]);
    const winningBoardLookup = useMemo(() => {
        const map = new Map<string, WinningBoardDto>();
        winningBoards.forEach((wb) => map.set(wb.boardId, wb));
        return map;
    }, [winningBoards]);
    const gameLookup = useMemo(() => {
        const map = new Map<string, GameDto>();
        games.forEach((game) => map.set(game.gameId, game));
        return map;
    }, [games]);

    const history: HistoryBoard[] = useMemo(() => {
        return boards.map((board) => {
            const game = gameLookup.get(board.gameId);
            const winningNumbers = game?.winningNumbers ?? [];
            const hasResults = (winningNumbers?.length ?? 0) >= 3;
            const matched = winningBoardLookup.get(board.boardId)?.winningNumbersMatched ?? 0;

            const isWinner = winningBoardIds.has(board.boardId);
            const status: keyof typeof statusStyles = !hasResults
                ? "Active"
                : isWinner
                    ? "Winning"
                    : "Losing";

            return {
                ...board,
                status,
                matched,
                winningNumbers,
                drawDate: game?.drawDate
            };
        });
    }, [boards, gameLookup, winningBoardIds, winningBoardLookup]);

    useEffect(() => {
        const newlyWinning = history.find((board) => {
            if (board.status !== "Winning") return false;
            const key = `winShown:${board.gameId}:${board.boardId}`;
            return !localStorage.getItem(key);
        });

        if (newlyWinning) {
            const key = `winShown:${newlyWinning.gameId}:${newlyWinning.boardId}`;
            localStorage.setItem(key, "true");
            toast.success("🎉 You won!", {duration: 5000});
            setActiveBoard(newlyWinning);
        }
    }, [history]);

    const sortedHistory = useMemo(
        () => [...history].sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()),
        [history]
    );

    const filteredHistory = useMemo(() => {
        if (gameFilter === "all") return sortedHistory;
        return sortedHistory.filter((board) => board.gameId === gameFilter);
    }, [gameFilter, sortedHistory]);

    const activeGameOptions = useMemo(() => [{
        label: "All games",
        value: "all"
    }, ...games.map((game) => ({
        label: `Ends ${new Date(game.expirationDate).toLocaleDateString()}`,
        value: game.gameId
    }))], [games]);

    return (
        <section className="space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Your results</p>
                    <h2 className="text-3xl font-semibold text-slate-900">History</h2>
                    <p className="mt-2 max-w-3xl text-slate-600">
                        Review every board you have purchased. Results update automatically when admins publish winning
                        numbers.
                    </p>
                </div>
                <button
                    type="button"
                    onClick={() => playerId && refreshBoards(playerId)}
                    className="rounded-full bg-[#f7a166] px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-orange-200"
                >
                    Refresh
                </button>
            </div>
            <div className="rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                <div className="grid gap-4 md:grid-cols-2">
                    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                        Player ID
                        <input
                            value={playerId}
                            onChange={(event) => setPlayerId(event.target.value)}
                            className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="Paste your player ID"
                        />
                        <span className="text-xs text-slate-500">Same ID used for deposits and board purchases.</span>
                    </label>
                    <div className="rounded-2xl bg-orange-50/60 p-4 shadow-inner">
                        <p className="text-xs uppercase tracking-wide text-slate-500">Timeline</p>
                        <p className="mt-1 text-sm text-slate-700">Filter by game date.</p>
                        <select
                            value={gameFilter}
                            onChange={(event) => setGameFilter(event.target.value)}
                            className="mt-2 w-full rounded-2xl border border-orange-100 bg-white px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                        >
                            {activeGameOptions.map((option) => (
                                <option key={option.value} value={option.value}>{option.label}</option>
                            ))}
                        </select>
                    </div>
                </div>
            </div>

            {loading && <p className="text-slate-500">Loading your boards…</p>}
            {!loading && filteredHistory.length === 0 && (
                <div
                    className="flex flex-col items-center gap-3 rounded-3xl border border-dashed border-orange-200 bg-[#fff7ef] p-6 text-center text-slate-600 shadow-inner">
                    <p className="text-lg font-semibold text-slate-700">No board purchases yet</p>
                    <p className="max-w-2xl text-sm">When you buy boards, they will appear here across every game.</p>
                </div>
            )}
            <div className="overflow-x-auto rounded-3xl bg-white/90 p-4 shadow-lg shadow-orange-100">
                <table className="min-w-[1120px] w-full table-auto divide-y divide-orange-100 text-left text-sm">
                    <thead className="bg-[#fef7ef] text-xs uppercase tracking-wide text-slate-500">
                    <tr>
                        <th className="px-6 py-3">Board</th>
                        <th className="px-6 py-3">Game</th>
                        <th className="px-6 py-3">Purchased</th>
                        <th className="px-6 py-3">Price</th>
                        <th className="px-6 py-3">Matches</th>
                        <th className="px-6 py-3">Published</th>
                        <th className="px-6 py-3">Result</th>
                    </tr>
                    </thead>
                    <tbody className="divide-y divide-orange-50">
                    {filteredHistory.map((board) => {
                        const game = gameLookup.get(board.gameId);
                        const winningNumbersLabel = board.winningNumbers?.length ? board.winningNumbers.join(", ") : "Pending";

                        return (
                            <tr
                                key={board.boardId}
                                className="cursor-pointer transition hover:bg-[#fff8f0]"
                                onClick={() => setActiveBoard(board)}
                            >
                                <td className="px-6 py-4 font-semibold text-slate-800">{board.boardId}</td>
                                <td className="px-6 py-4 text-slate-600">{game ? new Date(game.expirationDate).toLocaleDateString() : board.gameId}</td>
                                <td className="px-6 py-4 text-slate-600">{formatDateTime(board.timestamp)}</td>
                                <td className="px-6 py-4 text-slate-800">{board.price} kr</td>
                                <td className="px-6 py-4 text-slate-600">{board.winningNumbers?.length ? `${board.matched}/${board.winningNumbers.length}` : "—"}</td>
                                <td className="px-6 py-4 text-slate-600">{board.drawDate ? formatDateTime(board.drawDate) : "Pending"}</td>
                                <td className="px-6 py-4">
                                    <span
                                        className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${statusStyles[board.status]}`}>
                                        {board.status}
                                    </span>
                                    <p className="text-[11px] text-slate-500">Winning numbers: {winningNumbersLabel}</p>
                                </td>
                            </tr>
                        );
                    })}
                    </tbody>
                </table>
            </div>

            {activeBoard && (
                <div className="fixed inset-0 z-30 flex items-center justify-center bg-black/30 p-4">
                    <div className="max-w-lg space-y-4 rounded-3xl bg-white p-6 shadow-2xl shadow-orange-200">
                        <div className="flex items-start justify-between gap-3">
                            <div>
                                <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Board result</p>
                                <h3 className="text-2xl font-semibold text-slate-900">{activeBoard.boardId}</h3>
                                <p className="text-sm text-slate-600">Purchased {formatDateTime(activeBoard.timestamp)}</p>
                                {activeBoard.drawDate && (
                                    <p className="text-xs text-slate-500">Numbers
                                        published: {formatDateTime(activeBoard.drawDate)}</p>
                                )}
                            </div>
                            <span
                                className={`rounded-full px-3 py-1 text-xs font-semibold ${statusStyles[activeBoard.status]}`}>
                                {activeBoard.status}
                            </span>
                        </div>
                        <div className="grid gap-3 rounded-2xl bg-orange-50 p-4 md:grid-cols-2">
                            <div>
                                <p className="text-xs uppercase tracking-wide text-slate-500">Chosen numbers</p>
                                <div className="mt-2 flex flex-wrap gap-2">
                                    {activeBoard.chosenNumbers.map((num) => {
                                        const isMatch = activeBoard.winningNumbers?.includes(num);
                                        return (
                                            <span
                                                key={`chosen-${num}`}
                                                className={`flex h-10 w-10 items-center justify-center rounded-2xl ${isMatch ? "bg-emerald-100 text-emerald-700 ring-2 ring-emerald-200" : "bg-[#f7a166] text-white"}`}
                                            >
                                                {num}
                                            </span>
                                        );
                                    })}
                                </div>
                            </div>
                            <div>
                                <p className="text-xs uppercase tracking-wide text-slate-500">Winning numbers</p>
                                <div className="mt-2 flex flex-wrap gap-2">
                                    {activeBoard.winningNumbers?.length ? (
                                        activeBoard.winningNumbers.map((num) => (
                                            <span key={`win-${num}`}
                                                  className="flex h-10 w-10 items-center justify-center rounded-2xl bg-emerald-100 text-emerald-700 ring-2 ring-emerald-200">
                                                {num}
                                            </span>
                                        ))
                                    ) : (
                                        <span className="text-sm text-slate-600">Pending publication</span>
                                    )}
                                </div>
                            </div>
                        </div>
                        <div className="rounded-2xl bg-white p-4 shadow-inner">
                            <p className="text-sm font-semibold text-slate-800">Matched: {activeBoard.winningNumbers?.length ? `${activeBoard.matched}/${activeBoard.winningNumbers.length}` : "—"}</p>
                            {(() => {

                                const payoutValue = (winningBoardLookup.get(activeBoard.boardId) as unknown as { payout?: number } | undefined)?.payout;
                                return (
                                    <p className="text-sm text-slate-600">Payout: {payoutValue !== undefined ? `${payoutValue} kr` : "Pending"}</p>
                                );
                            })()}
                        </div>
                        <button
                            type="button"
                            className="w-full rounded-full bg-[#f7a166] px-4 py-2 text-center text-sm font-semibold text-white shadow-lg shadow-orange-200"
                            onClick={() => setActiveBoard(null)}
                        >
                            Close
                        </button>
                    </div>
                </div>
            )}
        </section>
    );
}