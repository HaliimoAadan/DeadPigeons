import {useEffect, useMemo, useState} from "react";
import {useAtom} from "jotai";
import toast from "react-hot-toast";
import NumberGrid from "../NumberGrid";
import {playerSelectionAtom} from "../state/gameAtoms";
import {gamesApi, type GameDto} from "@utilities/gamesApi.ts";
import {boardsApi, type BoardDto,  winningBoardsApi, type WinningBoardDto} from "@utilities/boardsApi.ts";
import {transactionsApi} from "@utilities/transactionsApi.ts";
import { normalizeNumbers } from "@utilities/jsonNormalize";


const pricing: Record<number, number> = {
    5: 20,
    6: 40,
    7: 80,
    8: 160
};

function getPrice(count: number) {
    return pricing[count] ?? 0;
}

function pickActiveGame(games: GameDto[]): GameDto | null {
    if (games.length === 0) return null;

    const now = Date.now();
    const oneDayAhead = now + 24 * 60 * 60 * 1000;

    const normalized = games
        .map((game) => ({
            game,
            exp: new Date(game.expirationDate).getTime(),
        }))
        .filter(({exp}) => !Number.isNaN(exp))
        .sort((a, b) => a.exp - b.exp);

    const soonestUpcoming = normalized.find(
        ({exp, game}) =>
            exp >= now &&
            exp <= oneDayAhead &&
            (game.winningNumbers?.length ?? 0) === 0
    );

    if (soonestUpcoming) return soonestUpcoming.game;

    const nextFuture = normalized.find(
        ({exp, game}) =>
            exp >= now &&
            (game.winningNumbers?.length ?? 0) === 0
    );

    if (nextFuture) return nextFuture.game;

    const last = normalized.length > 0 ? normalized[normalized.length - 1] : undefined;
    return last?.game ?? games[0];
}

function formatDateTime(value?: string) {
    if (!value) return "";
    const date = new Date(value);
    return date.toLocaleString();
}

type BoardStatus = "Active" | "Winning" | "Losing";

type BoardWithMeta = BoardDto & {
    status: BoardStatus;
    matched: number;
    winningNumbers: number[];
    drawDate?: string | null;
};

function normGuid(value: unknown): string {
    return String(value ?? "").trim().toLowerCase();
}


export default function MyBoardsPage() {

    const [selectedNumbers, setSelectedNumbers] = useAtom(playerSelectionAtom);
    const [playerId, setPlayerId] = useState<string>(() => localStorage.getItem("playerId") ?? "");
    const [balance, setBalance] = useState<number | null>(null);
    const [boards, setBoards] = useState<BoardDto[]>([]);
    const [winningBoards, setWinningBoards] = useState<WinningBoardDto[]>([]);
    const [games, setGames] = useState<GameDto[]>([]);
    const [currentGame, setCurrentGame] = useState<GameDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [isPurchasing, setIsPurchasing] = useState(false);
    const [viewMode, setViewMode] = useState<"grid" | "table">("grid");
    const [gameFilter, setGameFilter] = useState<string>("all");
    const [statusFilter, setStatusFilter] = useState<string>("all");
    const [sortBy, setSortBy] = useState<"newest" | "oldest" | "priceHigh" | "priceLow">("newest");
    const [celebratedBoard, setCelebratedBoard] = useState<BoardWithMeta | null>(null);

    useEffect(() => {
        const stored = localStorage.getItem("playerId") ?? "";
        const trimmed = stored.trim();
        if (trimmed && trimmed !== playerId) setPlayerId(trimmed);
    }, []);


    useEffect(() => {
        const loadGames = async () => {
            try {
                const response = await gamesApi.getAll();
                setGames(response);
                setCurrentGame(pickActiveGame(response));
            } catch (error) {
                console.error(error);
                toast.error("Could not load games.");
            }
        };

        loadGames();
    }, []);

    const refreshPlayerData = async (id: string) => {
        const trimmedId = id.trim();
        const playerKey = normGuid(trimmedId);
        if (!trimmedId) return;

        setLoading(true);
        try {
            const [balanceResponse, boardResponse, winningResponse, gamesResponse] =
                await Promise.all([
                    transactionsApi.getBalance(trimmedId),
                    boardsApi.list(),
                    winningBoardsApi.getAll(),
                    gamesApi.getAll(),
                ]);

            const allBoards = boardResponse;
            const allWinning = winningResponse;
            const allGames = gamesResponse;

            setBalance(balanceResponse.balance);

            const playerBoards = allBoards.filter((b) => normGuid(b.playerId) === playerKey);

// If the backend never computed winners for this game yet, trigger it so the DB gets updated
            const staleGameIds = new Set<string>();
            for (const board of playerBoards) {
                const game = allGames.find((g) => normGuid(g.gameId) === normGuid(board.gameId));
                const winningNumbers = normalizeNumbers(game?.winningNumbers);
                const hasResults = (winningNumbers?.length ?? 0) >= 3;
                const matches = hasResults
                    ? board.chosenNumbers.filter((n) => winningNumbers?.includes(n)).length
                    : 0;

                const alreadyRecorded = allWinning.some(
                    (w) => normGuid(w.boardId) === normGuid(board.boardId)
                );

                if (hasResults && matches === (winningNumbers?.length ?? 0) && !alreadyRecorded) {
                    staleGameIds.add(board.gameId);
                }
            }

            if (staleGameIds.size > 0) {
                for (const gameId of staleGameIds) {
                    try {
                        await winningBoardsApi.compute(gameId);
                    } catch (error) {
                        console.error("Could not compute winning boards", error);
                    }
                }

                const recomputed = await winningBoardsApi.getAll();
                setWinningBoards(recomputed);
            } else {
                setWinningBoards(allWinning);
            }

            setBoards(playerBoards);
            setGames(allGames);
            setCurrentGame(pickActiveGame(allGames));
            localStorage.setItem("playerId", trimmedId);

        } catch (error) {
            console.error(error);
            toast.error("Could not load your boards.");
        } finally {
            setLoading(false);
        }

    };

    useEffect(() => {
        if (playerId) {
            void refreshPlayerData(playerId);

        } else {
            setLoading(false);
        }
    }, [playerId]);

    const toggleNumber = (value: number) => {
        setSelectedNumbers((prev) =>
            prev.includes(value)
                ? prev.filter((item) => item !== value)
                : [...prev, value]
        );
    };

    const price = useMemo(() => getPrice(selectedNumbers.length), [selectedNumbers.length]);
    const isReady = selectedNumbers.length >= 5 && selectedNumbers.length <= 8 && !!playerId && !!currentGame;

    const gameLookup = useMemo(() => {
        const map = new Map<string, GameDto>();
        games.forEach((game) => map.set(game.gameId, game));
        return map;
    }, [games]);

    const winningBoardIds = useMemo(() => new Set(winningBoards.map((board) => board.boardId)), [winningBoards]);

    const boardsWithMeta: BoardWithMeta[] = useMemo(() => {
        return boards.map((board) => {
            const game = gameLookup.get(board.gameId);
            const winningNumbers = normalizeNumbers(game?.winningNumbers);
            const hasResults = (winningNumbers?.length ?? 0) >= 3;
            const storedMatch = winningBoards.find((wb) => wb.boardId === board.boardId)?.winningNumbersMatched;
            const computedMatch = hasResults
                ? board.chosenNumbers.filter((num) => winningNumbers?.includes(num)).length
                : 0;
            const matched = storedMatch ?? computedMatch;

//Prefer persisted winners, but avoid mislabeling a full match as losing while recomputing
            const hasFullMatch = hasResults && computedMatch === (winningNumbers?.length ?? 0);
            const isWinner = winningBoardIds.has(board.boardId) || hasFullMatch;
            const status: BoardStatus = !hasResults
                ? "Active"
                : isWinner
                    ? "Winning"
                    : "Losing";

            return {
                ...board,
                matched,
                winningNumbers,
                status
            };
        });
    }, [boards, gameLookup, winningBoardIds, winningBoards]);

    useEffect(() => {
        const newlyWinning = boardsWithMeta.find((board) => {
            if (board.status !== "Winning") return false;
            const key = `winShown:${board.gameId}:${board.boardId}`;
            return !localStorage.getItem(key);
        });

        if (newlyWinning) {
            const key = `winShown:${newlyWinning.gameId}:${newlyWinning.boardId}`;
            localStorage.setItem(key, "true");
            setCelebratedBoard(newlyWinning);
            toast.success("🎉 You won!", {duration: 5000});
        }
    }, [boardsWithMeta]);

    const filteredBoards = useMemo(() => {
        let next = [...boardsWithMeta];

        if (gameFilter !== "all") {
            next = next.filter((board) => board.gameId === gameFilter);
        }

        if (statusFilter !== "all") {
            next = next.filter((board) => {
                if (statusFilter === "Completed") {
                    return board.status === "Winning" || board.status === "Losing";
                }
                return board.status === statusFilter;
            });
        }

        switch (sortBy) {
            case "oldest":
                next.sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
                break;
            case "priceHigh":
                next.sort((a, b) => b.price - a.price);
                break;
            case "priceLow":
                next.sort((a, b) => a.price - b.price);
                break;
            default:
                next.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
        }

        return next;
    }, [boardsWithMeta, gameFilter, sortBy, statusFilter]);

    const StatusBadge = ({status}: { status: BoardStatus }) => {
        const colors: Record<BoardStatus, string> = {
            Active: "bg-amber-100 text-amber-700",
            Winning: "bg-emerald-100 text-emerald-700",
            Losing: "bg-slate-200 text-slate-600",
        };

        return (
            <span className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${colors[status]}`}>
      {status}
    </span>
        );
    };


    const handlePurchase = async () => {
        const trimmedId = playerId.trim();
        if (!trimmedId || !currentGame) {
            toast.error("Player ID and game are required.");
            return;
        }

        setIsPurchasing(true);
        try {
            await boardsApi.purchase(trimmedId, {
                gameId: currentGame.gameId,
                chosenNumbers: selectedNumbers,
                isRepeating: false,
                repeatUntilGameId: null
            });

            toast.success("Board purchased successfully.");
            setSelectedNumbers([]);
            await refreshPlayerData(trimmedId);
        } catch (error) {
            console.error(error);
            const message = error instanceof Error ? error.message : "Could not purchase board.";
            toast.error(message || "Could not purchase board.");
        } finally {
            setIsPurchasing(false);
        }
    };


    return (
        <section className="space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Your boards</p>
                    <h2 className="text-3xl font-semibold text-slate-900">My Boards</h2>
                    <p className="mt-2 max-w-3xl text-slate-600">
                        Use your balance to purchase boards for the current game. Every purchase immediately deducts
                        from
                        your
                        approved deposits.
                    </p>
                </div>
                <button
                    type="button"
                    onClick={() => playerId && refreshPlayerData(playerId)}
                    className="rounded-full bg-[#f7a166] px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-orange-200"
                >
                    Refresh data
                </button>
            </div>
            <div className="rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                <div className="grid gap-4 md:grid-cols-3">
                    <label
                        className="flex flex-col gap-3 rounded-2xl border border-orange-100/80 bg-[#fff7ef] p-4 text-sm font-medium text-slate-700 shadow-inner">
                        <div
                            className="flex items-center justify-between text-xs uppercase tracking-[0.2em] text-slate-500">
                            <span>Player ID</span>
                            <button
                                type="button"
                                className="text-[11px] font-semibold text-[#f1812c] hover:text-[#d96b18]"
                                onClick={() => {
                                    if (playerId.trim()) {
                                        navigator.clipboard?.writeText(playerId.trim());
                                        toast.success("Player ID copied");
                                    }
                                }}
                            >
                                Copy
                            </button>

                        </div>
                        <input
                            value={playerId}
                            onChange={(event) => setPlayerId(event.target.value)}
                            className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="Paste your player ID"
                        />
                        <span
                            className="text-xs font-normal text-slate-500">Stored locally after you enter it once.</span>
                    </label>
                    <div className="rounded-2xl bg-orange-50/60 p-4 shadow-inner">
                        <p className="text-xs uppercase tracking-wide text-slate-500">Balance</p>
                        <p className="mt-1 text-2xl font-semibold text-slate-900">{balance !== null ? `${balance.toFixed(2)} kr` : "—"}</p>
                        <p className="text-xs text-slate-500">Calculated from approved deposits minus boards
                            purchased.</p>
                    </div>
                    <div className="rounded-2xl bg-orange-50/60 p-4 shadow-inner">
                        <p className="text-xs uppercase tracking-wide text-slate-500">Current game</p>
                        <p className="mt-1 text-lg font-semibold text-slate-900">{currentGame ? new Date(currentGame.expirationDate).toDateString() : "No game available"}</p>
                        <p className="text-xs text-slate-500">{currentGame?.winningNumbers?.length ? "Winning numbers published" : "Waiting for winning numbers"}</p>
                    </div>
                </div>


                <div className="mt-6 space-y-4">
                    <NumberGrid selectedNumbers={selectedNumbers} onToggle={toggleNumber} maxSelectable={8}/>
                    <div className="flex flex-col items-center gap-3 text-center">
                        <p className="text-lg font-semibold text-slate-700">
                            {isReady ? `Price: ${price} kr` : "Select between 5 and 8 numbers to continue"}
                        </p>
                        <p className="text-sm text-slate-500">5 picks = 20 kr · 6 = 40 kr · 7 = 80 kr · 8 = 160
                            kr</p>
                        <button
                            type="button"
                            onClick={handlePurchase}
                            disabled={!isReady || isPurchasing}
                            className={`w-48 rounded-full px-6 py-3 text-lg font-semibold transition ${
                                isReady
                                    ? "bg-[#f7a166] text-white shadow-xl shadow-orange-200"
                                    : "bg-slate-200 text-slate-500"
                            }`}
                        >
                            {isPurchasing ? "Purchasing…" : "Buy board"}
                        </button>
                    </div>
                </div>
            </div>

            <div className="space-y-4">
                <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                    <div>
                        <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Boards overview</p>
                        <p className="text-slate-600">Purchased boards and timestamps for this player.</p>
                    </div>
                    <div className="flex flex-wrap gap-2 text-sm text-slate-700">
                        <select
                            value={gameFilter}
                            onChange={(event) => setGameFilter(event.target.value)}
                            className="rounded-2xl border border-orange-100 bg-white px-4 py-2 shadow-inner focus:border-orange-300 focus:outline-none"
                        >
                            <option value="all">All games</option>
                            {games.map((game) => (
                                <option key={game.gameId} value={game.gameId}>
                                    Game ending {new Date(game.expirationDate).toLocaleDateString()}
                                </option>
                            ))}
                        </select>
                        <select
                            value={sortBy}
                            onChange={(event) => setSortBy(event.target.value as typeof sortBy)}
                            className="rounded-2xl border border-orange-100 bg-white px-4 py-2 shadow-inner focus:border-orange-300 focus:outline-none"
                        >
                            <option value="newest">Newest first</option>
                            <option value="oldest">Oldest first</option>
                            <option value="priceHigh">Price: high to low</option>
                            <option value="priceLow">Price: low to high</option>
                        </select>
                        <div
                            className="flex items-center gap-1 rounded-full bg-[#fef7ef] p-1 text-xs font-semibold text-slate-600">
                            {["grid", "table"].map((mode) => (
                                <button
                                    key={mode}
                                    type="button"
                                    onClick={() => setViewMode(mode as "grid" | "table")}
                                    className={`rounded-full px-3 py-1 transition ${
                                        viewMode === mode ? "bg-[#f7a166] text-white shadow" : "hover:bg-orange-50"
                                    }`}
                                >
                                    {mode === "grid" ? "Card view" : "Table view"}
                                </button>
                            ))}
                        </div>
                    </div>
                </div>
                {loading && <p className="text-slate-500">Loading your boards…</p>}

                {!loading && filteredBoards.length === 0 && (
                    <div
                        className="flex flex-col items-center gap-3 rounded-3xl border border-dashed border-orange-200 bg-[#fff7ef] p-6 text-center text-slate-600 shadow-inner">
                        <p className="text-lg font-semibold text-slate-700">No boards yet</p>
                        <p className="max-w-2xl text-sm">Purchase a board above to see it here instantly. Use the
                            refresh button or the "All games" filter to surface every past board tied to your player
                            ID.</p>
                    </div>

                )}
                {viewMode === "grid" && filteredBoards.length > 0 && (
                    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
                        {filteredBoards.map((board) => {
                            const game = gameLookup.get(board.gameId);
                            const gameLabel = game
                                ? `Game ending ${new Date(game.expirationDate).toLocaleString()}`
                                : `Game ${board.gameId}`;
                            const matchLabel = board.winningNumbers?.length
                                ? `Matched: ${board.matched} / ${board.winningNumbers.length}`
                                : "Awaiting results";

                            return (
                                <article key={board.boardId}
                                         className="flex h-full flex-col gap-4 rounded-3xl bg-white/80 p-5 shadow-lg shadow-orange-100">
                                    <div className="flex items-start justify-between gap-3">
                                        <div className="text-sm text-slate-600">
                                            <p className="font-semibold text-slate-900">Purchased {formatDateTime(board.timestamp)}</p>
                                            <p>{gameLabel}</p>
                                            <p className="text-xs uppercase tracking-wide text-slate-500">Board
                                                ID: {board.boardId}</p>
                                        </div>
                                        <div className="space-y-1 text-right">
                                            <p className="text-xs uppercase tracking-wide text-slate-500">Price</p>
                                            <td className="px-6 py-5 text-right tabular-nums text-slate-800">{board.price} kr</td>
                                            <StatusBadge status={board.status}/>
                                        </div>
                                    </div>
                                    <div className="flex items-center justify-between text-xs text-slate-500">
                                        <span>{matchLabel}</span>
                                        {board.winningNumbers?.length > 0 && (
                                            <span
                                                className="text-[11px] text-slate-500">Winning numbers: {board.winningNumbers.join(", ")}</span>
                                        )}
                                    </div>
                                    <p className="text-xs uppercase tracking-wide text-slate-500">Numbers chosen</p>
                                    <div className="flex flex-wrap gap-2">
                                        {board.chosenNumbers.map((number) => {
                                            const isMatch = board.winningNumbers?.includes(number);
                                            return (
                                                <span
                                                    key={`${board.boardId}-${number}`}
                                                    className={`flex h-10 w-10 items-center justify-center rounded-2xl ${
                                                        isMatch ? "bg-emerald-100 text-emerald-700 ring-2 ring-emerald-200" : "bg-[#f7a166] text-white"
                                                    }`}
                                                >
                                                    {number}
                                                </span>
                                            );
                                        })}
                                    </div>
                                </article>
                            );
                        })}
                    </div>
                )}

                {viewMode === "table" && filteredBoards.length > 0 && (
                    <div className="overflow-x-auto rounded-3xl bg-white/90 p-6 shadow-lg shadow-orange-100">
                        <table
                            className="min-w-[1120px] w-full table-auto divide-y divide-orange-100 text-left text-[15px]">
                            <thead
                                className="bg-[#fef7ef] text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                            <tr>
                                <th className="px-6 py-4">Board</th>
                                <th className="px-6 py-4">Game</th>
                                <th className="px-6 py-4">Purchased</th>
                                <th className="px-6 py-4">Price</th>
                                <th className="px-6 py-4">Matches</th>
                                <th className="px-6 py-4">Status</th>
                            </tr>
                            </thead>
                            <tbody className="divide-y divide-orange-50 align middle">
                            {filteredBoards.map((board) => {
                                const game = gameLookup.get(board.gameId);
                                const gameLabel = game
                                    ? new Date(game.expirationDate).toLocaleDateString()
                                    : board.gameId;

                                return (
                                    <tr key={board.boardId} className="transition hover:bg-[#fff8f0]">
                                        <td className="px-6 py-5 font-semibold text-slate-800">
                                            <span className="block max-w-[260px] truncate">{board.boardId}</span>
                                        </td>
                                        <td className="px-6 py-5 text-slate-600">
                                            <span className="block max-w-[180px] truncate">{gameLabel}</span>
                                        </td>
                                        <td className="px-6 py-5 text-slate-600">{formatDateTime(board.timestamp)}</td>
                                        <td className="px-6 py-5 text-slate-800">{board.price} kr</td>
                                        <td className="px-6 py-5 text-slate-600">
                                            {board.winningNumbers?.length ? `${board.matched}/${board.winningNumbers.length}` : "—"}
                                        </td>
                                        <td className="px-6 py-4"><StatusBadge status={board.status}/></td>
                                    </tr>
                                );
                            })}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>

            {celebratedBoard && (
                <div className="fixed inset-0 z-20 flex items-center justify-center bg-black/30 p-4">
                    <div className="max-w-lg space-y-4 rounded-3xl bg-white p-6 shadow-2xl shadow-orange-200">
                        <h3 className="text-2xl font-semibold text-slate-900">🎉 You won!</h3>
                        <p className="text-slate-600">Board {celebratedBoard.boardId} matched all winning
                            numbers.</p>
                        <div className="rounded-2xl bg-orange-50 p-4">
                            <p className="text-xs uppercase tracking-wide text-slate-500">Winning numbers</p>
                            <div className="mt-2 flex flex-wrap gap-2">
                                {celebratedBoard.winningNumbers.map((number) => (
                                    <span key={`win-${celebratedBoard.boardId}-${number}`}
                                          className="flex h-10 w-10 items-center justify-center rounded-2xl bg-emerald-100 text-emerald-700 ring-2 ring-emerald-200">
                                        {number}
                                    </span>
                                ))}
                            </div>
                        </div>
                        <button
                            type="button"
                            className="w-full rounded-full bg-[#f7a166] px-4 py-2 text-center text-sm font-semibold text-white shadow-lg shadow-orange-200"
                            onClick={() => setCelebratedBoard(null)}
                        >
                            Close
                        </button>
                    </div>
                </div>
            )}
        </section>
    );
}