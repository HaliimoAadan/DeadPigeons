import {baseUrl} from "@core/baseUrl.ts";
import {customFetch} from "@utilities/customFetch.ts";

export type AdminWinnerLine = {
    winningboardId: string;
    boardId: string;
    playerId: string;
    playerName: string;
    winningNumbersMatched: number;
    timestamp: string;
    payout: number;
};

export type AdminPayoutOverview = {
    gameId: string;
    totalPlayers: number;
    totalPrizePool: number;
    winnerCount: number;
    profit30Percent: number;
    winnersPool70Percent: number;
    payoutPerWinner: number;
    remainder: number;
    winners: AdminWinnerLine[];
};

const jsonHeaders = {
    "Accept": "application/json",
    "Content-Type": "application/json"
};

export const adminApi = {
    async publishWinningNumbers(gameId: string, numbers: number[]): Promise<{ gameId: string; winningNumbers: number[] }> {
        const response = await customFetch.fetch(`${baseUrl}/api/Admin/${gameId}/publish-winning-numbers`, {
            method: "POST",
            headers: jsonHeaders,
            body: JSON.stringify({numbers})
        });

        if (!response.ok) throw new Error("Failed to publish winning numbers");

        return await response.json() as { gameId: string; winningNumbers: number[] };
    },

    async getPayoutOverview(gameId: string): Promise<AdminPayoutOverview> {
        const response = await customFetch.fetch(`${baseUrl}/api/Admin/games/${gameId}/payout-overview`, {
            method: "GET",
            headers: {"Accept": "application/json"}
        });

        if (!response.ok) throw new Error("Failed to fetch payout overview");

        return await response.json() as AdminPayoutOverview;
    }
};