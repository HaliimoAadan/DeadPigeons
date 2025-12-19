import {baseUrl} from "@core/baseUrl.ts";
import {customFetch} from "@utilities/customFetch.ts";

export type BoardDto = {
    boardId: string;
    playerId: string;
    gameId: string;
    chosenNumbers: number[];
    price: number;
    timestamp: string;
};

export type WinningBoardDto = {
    winningboardId: string;
    boardId: string;
    gameId: string;
    winningNumbersMatched: number;
    timestamp: string;
};

export type PurchaseBoardRequest = {
    gameId: string;
    chosenNumbers: number[];
    isRepeating?: boolean;
    repeatUntilGameId?: string | null;
};

const jsonHeaders = {
    "Accept": "application/json",
    "Content-Type": "application/json"
};

function normalizeArray<T>(data: unknown): T[] {
    if (Array.isArray(data)) return data as T[];
    if (data && typeof data === "object" && Array.isArray((data as any).$values)) {
        return (data as any).$values as T[];
    }
    return [];
}

export const boardsApi = {
    async list(): Promise<BoardDto[]> {
        const response = await customFetch.fetch(`${baseUrl}/api/Board`, {
            method: "GET",
            headers: {"Accept": "application/json"}
        });

        if (!response.ok) throw new Error("Failed to fetch boards");

        const data: unknown = await response.json();
        return normalizeArray<BoardDto>(data);
    },

    async purchase(playerId: string, payload: PurchaseBoardRequest): Promise<BoardDto> {
        const response = await customFetch.fetch(`${baseUrl}/api/Board/purchase`, {
            method: "POST",
            headers: {
                ...jsonHeaders,
                "X-Player-Id": playerId
            },
            body: JSON.stringify({
                ...payload,
                isRepeating: payload.isRepeating ?? false,
                repeatUntilGameId: payload.repeatUntilGameId ?? null
            })
        });

        if (!response.ok) throw new Error("Failed to purchase board");
        return await response.json() as BoardDto;
    }
};

export const winningBoardsApi = {
    async list(): Promise<WinningBoardDto[]> {
        const response = await customFetch.fetch(`${baseUrl}/api/WinningBoard`, {
            method: "GET",
            headers: {"Accept": "application/json"}
        });

        if (!response.ok) throw new Error("Failed to fetch winning boards");

        const data: unknown = await response.json();
        return normalizeArray<WinningBoardDto>(data);
    }
};