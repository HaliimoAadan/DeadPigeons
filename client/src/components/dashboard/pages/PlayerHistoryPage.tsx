import {useEffect, useMemo, useState} from "react";
import toast from "react-hot-toast";
import {boardsApi, type BoardDto, winningBoardsApi, type WinningBoardDto} from "@utilities/boardsApi.ts";
import {gamesApi, type GameDto} from "@utilities/gamesApi.ts";

const statusStyles: Record<string, string> = {
    Active: "bg-yellow-100 text-yellow-800",
    Winning: "bg-emerald-100 text-emerald-700",
    Losing: "bg-slate-200 text-slate-600"
};

export default function PlayerHistoryPage() {

    const [playerId, setPlayerId] = useState<string>(() => localStorage.getItem("playerId") ?? "");
    const [boards, setBoards] = useState<BoardDto[]>([]);
    const [winningBoards, setWinningBoards] = useState<WinningBoardDto[]>([]);
    const [games, setGames] = useState<GameDto[]>([]);
    const [loading, setLoading] = useState(false);

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
            const [boardsResponse, winningResponse] = await Promise.all([
                boardsApi.list(),
                winningBoardsApi.list()
            ]);

            const playerBoards = (Array.isArray(boardsResponse) ? boardsResponse : []).filter((board) => board.playerId === trimmedId);
            setBoards(playerBoards);
            setWinningBoards(Array.isArray(winningResponse) ? winningResponse : []);
            localStorage.setItem("playerId", trimmedId);
            setPlayerId(trimmedId)
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
    const gameLookup = useMemo(() => {
        const map = new Map<string, GameDto>();
        games.forEach((game) => map.set(game.gameId, game));
        return map;
    }, [games]);

    const history = boards.map((board) => {
        const game = gameLookup.get(board.gameId);
        let status: keyof typeof statusStyles = "Active";

        if (winningBoardIds.has(board.boardId)) {
            status = "Winning";
        } else if (game && (game.winningNumbers?.length ?? 0) >= 3) {
            status = "Losing";
        }

        return {
            ...board,
            status
        };
    });

    const sortedHistory = useMemo(
        () => [...history].sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()),
        [history]
    );
    
    
    return (
        <section className="space-y-6">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Your results</p>
                <h2 className="text-3xl font-semibold text-slate-900">History</h2>
                <p className="mt-2 max-w-3xl text-slate-600">
                    Review every board you have purchased.
                </p>
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
                        <p className="text-xs uppercase tracking-wide text-slate-500">Tip</p>
                        <p className="text-sm text-slate-700">Statuses update as soon as admins publish winning numbers.</p>
                    </div>
                </div>
            </div>

            {loading && <p className="text-slate-500">Loading your boards…</p>}
            {!loading && sortedHistory.length === 0 && (
                <p className="rounded-2xl bg-orange-50 px-4 py-3 text-sm text-slate-600">No board purchases yet for this ID.</p>
            )}
            <div className="space-y-4">

                {sortedHistory.map((board) => (
                    <article key={board.boardId} className="rounded-3xl bg-white/80 p-5 shadow-lg shadow-orange-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-xs uppercase tracking-wide text-slate-400">Board</p>
                                <p className="text-lg font-semibold text-slate-900">{board.boardId}</p>
                                <p className="text-sm text-slate-500">Purchased {new Date(board.timestamp).toLocaleString()}</p>
                            </div>
                            <span className={`rounded-full px-4 py-1 text-xs font-semibold ${statusStyles[board.status]}`}>
                                {board.status}
                            </span>
                        </div>
                        <p className="mt-4 text-sm uppercase tracking-wide text-slate-500">Numbers</p>
                        <div className="mt-2 flex flex-wrap gap-2">
                            {(() => {
                                const raw = board.chosenNumbers as unknown;

                                const numbers: number[] = Array.isArray(raw)
                                    ? raw
                                    : typeof raw === "string"
                                        ? (() => {
                                            try {
                                                const parsed = JSON.parse(raw);
                                                if (Array.isArray(parsed)) return parsed.map(Number);
                                            } catch {}

                                            return raw
                                                .split(/[\s,]+/)
                                                .map((v) => Number(v))
                                                .filter(Number.isFinite);
                                        })()
                                        : [];

                                return numbers.map((number) => (
                                    <span
                                        key={`${board.boardId}-${number}`}
                                        className="flex h-10 w-10 items-center justify-center rounded-2xl bg-[#f7a166] text-white"
                                    >
            {number}
        </span>
                                ));
                            })()}

                        </div>
                        <p className="mt-4 text-sm text-slate-600">Price: {board.price} kr</p>
                    </article>
                ))}
            </div>
        </section>
    );
}