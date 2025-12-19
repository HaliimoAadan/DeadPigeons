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
        const url = new URL(`${baseUrl}/api/player`);
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
            const payload: PlayerCreateDto = {
                fullName: (dto.fullName ?? "").trim(),
                email: (dto.email ?? "").trim(),
                phoneNumber: (dto.phoneNumber ?? "").trim(),
                isActive: dto.isActive ?? true,
            };

            const response = await customFetch.fetch(`${baseUrl}/api/player`, {
                method: "POST",
                headers: jsonHeaders,
                body: JSON.stringify(payload),
            });

            if (!response.ok) {
                const errText = await response.text().catch(() => "");
                console.error("Create player failed:", response.status, errText);
                throw new Error("Could not create player.");
            }

            const data: any = await response.json();

            // If backend returns { playerId, fullName, ... } you're good.
            // If backend returns firstName/lastName, you’ll see it here in console.
            return data as PlayerResponseDto;
    },

    async updateStatus(playerId: string, isActive: boolean): Promise<void> {
        const response = await customFetch.fetch(`${baseUrl}/api/player/${playerId}/status?isActive=${isActive}`, {
            method: "PATCH",
            headers: {
                "Accept": "application/json"
            }
        });

        if (!response.ok) throw new Error("Failed to update player status");
    }
};