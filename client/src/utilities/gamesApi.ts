import {baseUrl} from "@core/baseUrl.ts";
import {customFetch} from "@utilities/customFetch.ts";

export type GameDto = {
    gameId: string;
    winningNumbers?: number[] | null;
    drawDate?: string | null;
    expirationDate: string;
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

export const gamesApi = {
    async getAll(): Promise<GameDto[]> {
        const response = await customFetch.fetch(`${baseUrl}/api/Game`, {
            method: "GET",
            headers: jsonHeaders
        });

        if (!response.ok) throw new Error("Failed to fetch games");

        const data: unknown = await response.json();
        return normalizeArray<GameDto>(data);
    },

    async create(expirationDate: string): Promise<GameDto> {
        const response = await customFetch.fetch(`${baseUrl}/api/Game`, {
            method: "POST",
            headers: jsonHeaders,
            body: JSON.stringify({expirationDate})
        });

        if (!response.ok) throw new Error("Failed to create game");
        return await response.json() as GameDto;
    }
};