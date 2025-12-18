import {baseUrl} from "@core/baseUrl.ts";
import {customFetch} from "@utilities/customFetch.ts";

export type AdminTransactionListItem = {
    transactionId: string;
    mobilePayReqId: string;
    playerId: string;
    playerFirstName: string;
    playerLastName: string;
    playerEmail: string;
    amount: number;
    status: "Pending" | "Approved" | "Rejected" | string;
    timestamp: string;
};

export type SubmitTransactionDto = {
    mobilePayReqId: string;
    amount: number;
};

export type TransactionResponseDto = {
    transactionId: string;
    playerId: string;
    mobilePayReqId: string;
    amount: number;
    status: string;
    timestamp: string;
};

export type UpdateTransactionStatusDto = {
    status: "Pending" | "Approved" | "Rejected" | string;
};


const jsonHeaders = {
    "Accept": "application/json"
};

export const transactionsApi = {
    async list(status?: string, search?: string): Promise<AdminTransactionListItem[]> {
        const params = new URLSearchParams();

        if (status) params.set("status", status);
        if (search) params.set("search", search);

        const query = params.toString();
        const response = await customFetch.fetch(
            `${baseUrl}/api/Transactions${query ? `?${query}` : ""}`,
            {
                method: "GET",
                headers: jsonHeaders,
            }
        );

        if (!response.ok) throw new Error("Failed to fetch transactions");

        const data: any = await response.json();

        // Accept both: [ ... ]  OR  { $values: [ ... ] }
        if (Array.isArray(data)) return data as AdminTransactionListItem[];
        if (data && Array.isArray(data.$values)) return data.$values as AdminTransactionListItem[];

        return [];
    },

    async updateStatus(transactionId: string, status: UpdateTransactionStatusDto["status"]): Promise<void> {
        const response = await customFetch.fetch(
            `${baseUrl}/api/Transactions/${transactionId}/status`,
            {
                method: "PATCH",
                headers: {
                    ...jsonHeaders,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({status})
            }
        );

        if (!response.ok) throw new Error("Failed to update transaction status");
    },

    async submit(playerId: string, dto: SubmitTransactionDto): Promise<TransactionResponseDto> {
        const response = await customFetch.fetch(
            `${baseUrl}/api/Transactions/submit`,
            {
                method: "POST",
                headers: {
                    ...jsonHeaders,
                    "Content-Type": "application/json",
                    "X-Player-Id": playerId
                },
                body: JSON.stringify(dto)
            }
        );

        if (!response.ok) throw new Error("Failed to submit transaction");
        return await response.json() as TransactionResponseDto;
    }
};