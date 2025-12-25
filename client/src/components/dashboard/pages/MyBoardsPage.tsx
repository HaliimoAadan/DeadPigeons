import {useEffect, useMemo, useState} from "react";
import {useAtom} from "jotai";
import toast from "react-hot-toast";
import NumberGrid from "../NumberGrid";
import {playerSelectionAtom} from "../state/gameAtoms";
import {gamesApi, type GameDto} from "@utilities/gamesApi.ts";
import {boardsApi, type BoardDto} from "@utilities/boardsApi.ts";
import {transactionsApi} from "@utilities/transactionsApi.ts";

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
    const today = new Date();
    const upcoming = [...games]
        .filter((game) => new Date(game.expirationDate) >= today)
        .sort((a, b) => new Date(a.expirationDate).getTime() - new Date(b.expirationDate).getTime());

    if (upcoming.length > 0) return upcoming[0];
    return games[0] ?? null;
}

export default function MyBoardsPage() {

    const [selectedNumbers, setSelectedNumbers] = useAtom(playerSelectionAtom);
    const [playerId, setPlayerId] = useState<string>(() => localStorage.getItem("playerId") ?? "");
    const [balance, setBalance] = useState<number | null>(null);
    const [boards, setBoards] = useState<BoardDto[]>([]);
    const [games, setGames] = useState<GameDto[]>([]);
    const [currentGame, setCurrentGame] = useState<GameDto | null>(null);
    const [loading, setLoading] = useState(false);
    const [isPurchasing, setIsPurchasing] = useState(false);

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
        if (!trimmedId) return;
        setLoading(true);
        try {
            const [balanceResponse, boardResponse] = await Promise.all([
                transactionsApi.getBalance(trimmedId),
                boardsApi.list()
            ]);

            console.log("boardsApi.list()", boardResponse);
            console.log("filtered", boardResponse.filter(b => b.playerId === trimmedId));

            setBalance(balanceResponse.balance);
            const playerBoards = boardResponse.filter((board) => board.playerId === trimmedId);
            setBoards(playerBoards);
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

    const sortedBoards = useMemo(
        () => [...boards].sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()),
        [boards]
    );

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
            toast.error("Could not purchase board.");
        } finally {
            setIsPurchasing(false);
        }
};
    
    return (
        <section className="space-y-6">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Your boards</p>
                <h2 className="text-3xl font-semibold text-slate-900">My Boards</h2>
                <p className="mt-2 max-w-3xl text-slate-600">
                    Use your balance to purchase boards for the current game. Every purchase immediately deducts from your
                    approved deposits.
                </p>
            </div>
            <div className="rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                <div className="grid gap-4 md:grid-cols-3">
                    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                        Player ID
                        <input
                            value={playerId}
                            onChange={(event) => setPlayerId(event.target.value)}
                            className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="Paste your player ID"
                        />
                        <span className="text-xs text-slate-500">Stored locally after you enter it once.</span>
                    </label>
                    <div className="rounded-2xl bg-orange-50/60 p-4 shadow-inner">
                        <p className="text-xs uppercase tracking-wide text-slate-500">Balance</p>
                        <p className="mt-1 text-2xl font-semibold text-slate-900">{balance !== null ? `${balance.toFixed(2)} kr` : "—"}</p>
                        <p className="text-xs text-slate-500">Calculated from approved deposits minus boards purchased.</p>
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
                        <p className="text-sm text-slate-500">5 picks = 20 kr · 6 = 40 kr · 7 = 80 kr · 8 = 160 kr</p>
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
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Boards owned</p>
                    <p className="text-slate-600">Purchased boards and timestamps for this player.</p>
                </div>
                {loading && <p className="text-slate-500">Loading your boards…</p>}
                {!loading && boards.length === 0 && (
                    <p className="rounded-2xl bg-orange-50 px-4 py-3 text-sm text-slate-600">No boards purchased yet.</p>
                )}
                <div className="grid gap-4 md:grid-cols-2">
                    {sortedBoards.map((board) => {
                        const game = gameLookup.get(board.gameId);
                        const gameLabel = game
                            ? `Game ending ${new Date(game.expirationDate).toLocaleString()}`
                            : `Game ${board.gameId}`;
                        const gameStatus = game
                            ? (game.winningNumbers?.length ?? 0) >= 3
                                ? "Winning numbers published"
                                : "Waiting for winning numbers"
                            : "Game info unavailable";

                        return (
                            <article key={board.boardId} className="rounded-3xl bg-white/80 p-5 shadow-lg shadow-orange-100">
                                <div className="flex items-start justify-between gap-3">
                                    <div className="text-sm text-slate-600">
                                        <p className="font-semibold text-slate-900">Purchased {new Date(board.timestamp).toLocaleString()}</p>
                                        <p>{gameLabel}</p>
                                        <p className="text-xs uppercase tracking-wide text-slate-500">{gameStatus}</p>
                                    </div>
                                    <div className="text-right">
                                        <p className="text-xs uppercase tracking-wide text-slate-500">Price</p>
                                        <p className="text-2xl font-semibold text-slate-900">{board.price} kr</p>
                                    </div>
                                </div>

                                <p className="mt-4 text-xs uppercase tracking-wide text-slate-500">Numbers chosen</p>
                                <div className="mt-2 flex flex-wrap gap-2">
                                    {board.chosenNumbers.map((number) => (
                                        <span
                                            key={`${board.boardId}-${number}`}
                                            className="flex h-10 w-10 items-center justify-center rounded-2xl bg-[#f7a166] text-white"
                                        >
                                            {number}
                                        </span>
                                    ))}
                                </div>

                                <p className="mt-4 text-xs uppercase tracking-wide text-slate-500">Game</p>
                                <p className="text-sm font-semibold text-slate-700">{board.gameId}</p>
                            </article>
                        );
                    })}
                </div>
            </div>
        </section>
    );
}