import {baseUrl} from "@core/baseUrl.ts";
import {customFetch} from "@utilities/customFetch.ts";
import { resolvePreservedArray } from "@utilities/preserveJson.ts";

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
        const url = new URL(`${baseUrl}/api/Player`);
        if (onlyActive) url.searchParams.set("onlyActive", "true");

        const response = await customFetch.fetch(url.toString(), {
            method: "GET",
            headers: jsonHeaders
        });

        if (!response.ok) throw new Error("Failed to fetch players");

        const raw = await response.json();
        return resolvePreservedArray<PlayerResponseDto>(raw);
    },

    async create(dto: PlayerCreateDto): Promise<PlayerResponseDto> {
        const payload: PlayerCreateDto = {
            fullName: (dto.fullName ?? "").trim(),
            email: (dto.email ?? "").trim(),
            phoneNumber: (dto.phoneNumber ?? "").trim(),
            isActive: dto.isActive ?? true,
        };

            const response = await customFetch.fetch(`${baseUrl}/api/Player`, {
                method: "POST",
                headers: jsonHeaders,
                body: JSON.stringify(payload),
            });

            if (!response.ok) throw new Error("Could not create player.");
            
        return (await response.json()) as PlayerResponseDto;
    },

    async updateStatus(playerId: string, isActive: boolean): Promise<void> {
        const response = await customFetch.fetch(`${baseUrl}/api/Player/${playerId}/status?isActive=${isActive}`, {
            method: "PATCH",
            headers: {
                "Accept": "application/json"
            }
        });

        if (!response.ok) throw new Error("Failed to update player status");
    }
};