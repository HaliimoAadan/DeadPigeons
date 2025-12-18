import type {ProblemDetails} from "@core/problemdetails.ts";
import toast from "react-hot-toast";

/**
 * This fetch http client attaches JWT from localstorage
 * and toasts if http requests fail.
 * Note: circular reference resolution is handled at the API client level,
 * not here, because JSON.stringify() cannot preserve circular references.
 */
export const customFetch = {
    fetch(url: RequestInfo, init?: RequestInit): Promise<Response> {
        const token = localStorage.getItem('jwt');
        const headers = new Headers(init?.headers);

        if (token) {
            const authHeader = token.startsWith("Bearer ") ? token : `Bearer ${token}`;
            headers.set('Authorization', authHeader);        
        }

        return fetch(url, {
            ...init,
            headers
        }).then(async (response) => {
            // Handle errors by reading from one clone
            if (!response.ok) {
                try {
                    const errorClone = response.clone();
                    const problemDetails = (await errorClone.json()) as ProblemDetails;
                    console.log(problemDetails);
                    toast(problemDetails.title ?? "Request failed");
                } catch {
                    toast("Request failed");
                }
            }

            return response;
        });
    }
};