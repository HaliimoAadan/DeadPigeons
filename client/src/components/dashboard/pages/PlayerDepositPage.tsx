import {useEffect, useState} from "react";
import type {FormEvent} from "react";
import toast from "react-hot-toast";
import {transactionsApi, type SubmitTransactionDto, type TransactionResponseDto} from "@utilities/transactionsApi.ts";


const defaultDto: SubmitTransactionDto = {
    mobilePayReqId: "",
    amount: 0
};

export default function PlayerDepositPage() {
    const [playerId, setPlayerId] = useState<string>("");
    const [form, setForm] = useState<SubmitTransactionDto>(defaultDto);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [lastResult, setLastResult] = useState<TransactionResponseDto | null>(null);

    useEffect(() => {
        const storedPlayerId = localStorage.getItem("playerId");
        if (storedPlayerId) {
            setPlayerId(storedPlayerId);
        }
    }, []);

    const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setIsSubmitting(true);
        setLastResult(null);

        try {
            if (!playerId) {
                toast.error("Player ID is required");
                return;
            }

            const dto: SubmitTransactionDto = {
                mobilePayReqId: form.mobilePayReqId.trim(),
                amount: Number(form.amount)
            };

            const response = await transactionsApi.submit(playerId, dto);
            setLastResult(response);
            localStorage.setItem("playerId", playerId);
            toast.success("Deposit submitted for review.");
            setForm(defaultDto);
        } catch (error) {
            console.error(error);
            toast.error("Could not submit deposit.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <section className="space-y-6">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Deposit</p>
                <h2 className="text-3xl font-semibold text-slate-900">Submit your MobilePay transaction</h2>
                <p className="mt-2 max-w-2xl text-slate-600">Enter the transaction number and amount you sent. The club will review and approve it before your boards activate.</p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4 rounded-3xl bg-white/90 p-6 shadow-lg shadow-orange-100">
                <div className="grid gap-4 md:grid-cols-2">
                    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                        Player ID
                        <input
                            required
                            value={playerId}
                            onChange={(event) => setPlayerId(event.target.value)}
                            className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="Paste your player ID"
                        />
                        <span className="text-xs text-slate-500">Find this in your welcome email or ask the admin.</span>
                    </label>
                    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                        Transaction number
                        <input
                            required
                            value={form.mobilePayReqId}
                            onChange={(event) => setForm((prev) => ({...prev, mobilePayReqId: event.target.value}))}
                            className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="e.g. 12345678"
                        />
                    </label>
                    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                        Amount (DKK)
                        <input
                            required
                            type="number"
                            min={1}
                            step={1}
                            value={form.amount}
                            onChange={(event) => setForm((prev) => ({...prev, amount: Number(event.target.value)}))}
                            className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="Enter the total you sent"
                        />
                    </label>
                </div>
                <button
                    type="submit"
                    disabled={isSubmitting}
                    className="rounded-full bg-[#f7a166] px-6 py-3 font-semibold text-white shadow-lg shadow-orange-200 disabled:cursor-not-allowed disabled:opacity-70"
                >
                    {isSubmitting ? "Submitting…" : "Submit deposit"}
                </button>
            </form>

            {lastResult && (
                <div className="rounded-3xl bg-white/90 p-6 shadow-lg shadow-orange-100">
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Status</p>
                    <h3 className="mt-2 text-2xl font-semibold text-slate-900">{lastResult.status}</h3>
                    <p className="mt-2 text-slate-700">Transaction {lastResult.mobilePayReqId} for {lastResult.amount} DKK has been logged.</p>
                </div>
            )}
        </section>
    );
}