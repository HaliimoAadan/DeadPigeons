import { baseUrl } from "@core/baseUrl";
import { customFetch } from "@utilities/customFetch";

/* ---------- Normalizer ---------- */
export function normalizeArray<T>(x: any): T[] {
    if (Array.isArray(x)) return x;
    if (x && Array.isArray(x.$values)) return x.$values;
    return [];
}

/* ---------- DTOs ---------- */
export type BoardDto = {
    boardId: string;
    playerId: string;
    gameId: string;
    chosenNumbers: number[];
    timestamp: string;
    price: number;
};

export type WinningBoardDto = {
    winningboardId: string;
    boardId: string;
    gameId: string;
    playerId?: string;
    winningNumbersMatched: number;
    timestamp: string;
};

/* ---------- Boards API ---------- */
export const boardsApi = {
    async list(): Promise<BoardDto[]> {
        const res = await customFetch.fetch(`${baseUrl}/api/Board`, {

            method: "GET",
            headers: { Accept: "application/json" },
        });

        if (!res.ok) throw new Error("Failed to fetch boards");

        const raw = await res.json();
        return normalizeArray<any>(raw).map((b) => ({
            boardId: String(b.boardId ?? b.BoardId ?? ""),
            playerId: String(b.playerId ?? b.PlayerId ?? ""),
            gameId: String(b.gameId ?? b.GameId ?? ""),
            chosenNumbers: normalizeArray<number>(b.chosenNumbers ?? b.ChosenNumbers),
            timestamp: String(b.timestamp ?? b.Timestamp ?? ""),
            price: Number(b.price ?? b.Price ?? 0),
        }));
    },

    async purchase(
        playerId: string,
        payload: {
            gameId: string;
            chosenNumbers: number[];
            isRepeating: boolean;
            repeatUntilGameId: string | null;
        }
    ): Promise<void> {
        const res = await customFetch.fetch(`${baseUrl}/api/Board/purchase`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Player-Id": playerId,
            },
            body: JSON.stringify(payload),
        });

        if (!res.ok) {
            const text = await res.text();
            throw new Error(text || "Failed to purchase board");
        }
    },
};

/* ---------- Winning boards ---------- */
export const winningBoardsApi = {
    async getAll(): Promise<WinningBoardDto[]> {
        const endpoints = [`${baseUrl}/api/WinningBoard`, `${baseUrl}/winningboards`];

        const mapResult = (raw: any): WinningBoardDto[] =>
            normalizeArray<any>(raw).map((w) => ({
                winningboardId: String(
                    w.winningboardId ?? w.winningBoardId ?? w.WinningboardId ?? w.WinningBoardId ?? ""
                ),
                boardId: String(w.boardId ?? w.BoardId ?? ""),
                gameId: String(w.gameId ?? w.GameId ?? w.gameID ?? w.GameID ?? ""),
                playerId: String(
                    w.playerId ??
                    w.PlayerId ??
                    w.board?.playerId ??
                    w.Board?.PlayerId ??
                    w.board?.player?.playerId ??
                    w.Board?.Player?.PlayerId ??
                    ""
                ),
                winningNumbersMatched: Number(
                    w.winningNumbersMatched ?? w.winningNumbers ?? w.WinningNumbersMatched ?? w.WinningNumbers ?? 0
                ),
                timestamp: String(w.timestamp ?? w.Timestamp ?? ""),
            }));

        let lastError: unknown = null;

        for (const url of endpoints) {
            try {
                const res = await customFetch.fetch(url, {
                    method: "GET",
                    headers: {Accept: "application/json"},
                });

                if (!res.ok) throw new Error(`Failed to fetch winning boards via ${url}`);

                const raw = await res.json();
                return mapResult(raw);
            } catch (error) {
                lastError = error;
                console.warn("winningBoardsApi.getAll fallback", error);
            }
        }

        throw lastError ?? new Error("Failed to fetch winning boards");
    },


    async compute(gameId: string): Promise<void> {
        const res = await customFetch.fetch(
            `${baseUrl}/api/WinningBoard/${gameId}/compute-winningboards`,
            { method: "POST" }
        );
        if (!res.ok) throw new Error("Failed to compute winning boards");
    },
};