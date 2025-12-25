import {baseUrl} from "@core/baseUrl.ts";
import {customFetch} from "@utilities/customFetch.ts";
import {asArray, asNumberArray} from "@utilities/jsonNormalize.ts";

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

export const gamesApi = {
    async getAll(): Promise<GameDto[]> {
        const response = await customFetch.fetch(`${baseUrl}/api/Game`, {
            method: "GET",
            headers: jsonHeaders
        });
        
        if (!response.ok) throw new Error("Failed to fetch games");
        const raw = await response.json();

        const games = asArray<GameDto>(raw).map((g) => ({
            gameId: g.gameId,
            expirationDate: g.expirationDate,
            drawDate: g.drawDate ?? null,
            winningNumbers: asNumberArray(g.winningNumbers)
        }));

        return games.filter(g => g.gameId && g.gameId !== "00000000-0000-0000-0000-000000000000");
        
        },

    async create(expirationDate: string): Promise<GameDto> {
        const response = await customFetch.fetch(`${baseUrl}/api/Game`, {
            method: "POST",
            headers: jsonHeaders,
            body: JSON.stringify({expirationDate: expirationDate }),
        });

        const game = await response.json();
        return {
            ...game,
            winningNumbers: asNumberArray(game.winningNumbers)
        };
    }
};