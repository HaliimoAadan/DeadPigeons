import {baseUrl} from "@core/baseUrl.ts";
import {customFetch} from "@utilities/customFetch.ts";

export type PlayerCreateDto = {
    fullName: string;
    phoneNumber: string;
    email: string;
    isActive?: boolean;
};

export type PlayerResponseDto = {
    playerId: string;
    fullName: string;
    phoneNumber: string;
    email: string;
    isActive: boolean;
};

const jsonHeaders = {
    "Accept": "application/json",
    "Content-Type": "application/json"
};

export const playersApi = {
    async getAll(onlyActive?: boolean): Promise<PlayerResponseDto[]> {
        const url = new URL(`${baseUrl}/api/Players`);
        if (onlyActive) url.searchParams.set("onlyActive", "true");

        const response = await customFetch.fetch(url.toString(), {
            method: "GET",
            headers: jsonHeaders
        });

        if (!response.ok) throw new Error("Failed to fetch players");

        const data: any = await response.json();

        // Handles both normal arrays and EF "$values" shape
        if (Array.isArray(data)) return data as PlayerResponseDto[];
        if (data && Array.isArray(data.$values)) return data.$values as PlayerResponseDto[];

        return[];
    },

    async create(dto: PlayerCreateDto): Promise<PlayerResponseDto> {
        const response = await customFetch.fetch(`${baseUrl}/api/Players`, {
            method: "POST",
            headers: jsonHeaders,
            body: JSON.stringify(dto)
        });

        if (!response.ok) throw new Error("Failed to create player");
        return await response.json() as PlayerResponseDto;
    },

    async updateStatus(playerId: string, isActive: boolean): Promise<void> {
        const response = await customFetch.fetch(`${baseUrl}/api/Players/${playerId}/status?isActive=${isActive}`, {
            method: "PATCH",
            headers: {
                "Accept": "application/json"
            }
        });

        if (!response.ok) throw new Error("Failed to update player status");
    }
};