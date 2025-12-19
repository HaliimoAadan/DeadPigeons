import {useEffect, useMemo, useState} from "react";
import type {FormEvent} from "react";
import toast from "react-hot-toast";
import {transactionsApi, type AdminTransactionListItem} from "@utilities/transactionsApi.ts";
import {playersApi} from "@utilities/playersApi.ts";

const statusOptions = ["Pending", "Approved", "Rejected"] as const;

export default function AdminReviewDepositsPage() {
    const [transactions, setTransactions] = useState<AdminTransactionListItem[]>([]);
    const [statusFilter, setStatusFilter] = useState<string>("Pending");
    const [search, setSearch] = useState<string>("");
    const [loading, setLoading] = useState(true);
    const [isUpdating, setIsUpdating] = useState(false);

    const [playerLookup, setPlayerLookup] = useState<Map<string, {fullName: string; email: string}>>(new Map());

    const fetchTransactions = async () => {
        setLoading(true);
        try {
            const response = await transactionsApi.list(statusFilter || undefined, search || undefined);
            setTransactions(response ?? []);
        } catch (error) {
            console.error(error);
            toast.error("Could not load deposits.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        void fetchTransactions();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    useEffect(() => {
        (async () => {
            try {
                const players = await playersApi.getAll();
                const map = new Map<string, {fullName: string; email: string}>();
                players.forEach((p) => map.set(p.playerId, {fullName: p.fullName, email: p.email}));
                setPlayerLookup(map);
            } catch (e) {
                console.error(e);
            }
        })();
    }, []);

    const onSubmitFilters = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        await fetchTransactions();
    };

    const updateStatus = async (transaction: AdminTransactionListItem, status: AdminTransactionListItem["status"]) => {
        setIsUpdating(true);
        try {
            await transactionsApi.updateStatus(transaction.transactionId, status);

            if (status === "Approved") {
                await playersApi.updateStatus(transaction.playerId, true);
            }

            toast.success(`Marked as ${status}`);
            await fetchTransactions();
        } catch (error) {
            console.error(error);
            toast.error("Could not update status.");
        } finally {
            setIsUpdating(false);
        }
    };

    const rows = useMemo(() => {
        return transactions.map((t) => {
            const fromLookup = playerLookup.get(t.playerId);

            const fallbackName = `${t.playerFirstName ?? ""} ${t.playerLastName ?? ""}`.trim();
            const fullName = (fromLookup?.fullName?.trim() || fallbackName || t.playerId);

            const email = (fromLookup?.email || t.playerEmail || "").trim();

            return {t, fullName, email};
        });
    }, [transactions, playerLookup]);

    return (
        <section className="space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Deposits</p>
                    <h2 className="text-3xl font-semibold text-slate-900">Review MobilePay Transactions</h2>
                    <p className="mt-1 max-w-2xl text-sm text-slate-600">
                        Approve or reject deposits. Approving activates the player.
                    </p>
                </div>
            </div>

            <form
                onSubmit={onSubmitFilters}
                className="grid gap-4 rounded-3xl bg-white/90 p-4 shadow-lg shadow-orange-100 md:grid-cols-3"
            >
                <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                    Status
                    <select
                        value={statusFilter}
                        onChange={(event) => setStatusFilter(event.target.value)}
                        className="rounded-2xl border border-orange-100 px-3 py-2 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                    >
                        <option value="">All</option>
                        {statusOptions.map((s) => (
                            <option key={s} value={s}>{s}</option>
                        ))}
                    </select>
                </label>

                <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 md:col-span-2">
                    Search by player or transaction number
                    <div className="flex gap-3">
                        <input
                            value={search}
                            onChange={(event) => setSearch(event.target.value)}
                            className="flex-1 rounded-2xl border border-orange-100 px-3 py-2 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                            placeholder="Transaction id, player name, email…"
                        />
                        <button
                            type="submit"
                            className="rounded-full bg-[#f7a166] px-5 py-2 text-sm font-semibold text-white shadow-orange-200 transition hover:shadow-lg"
                        >
                            Apply
                        </button>
                    </div>
                </label>
            </form>

            <div className="overflow-hidden rounded-3xl bg-white/90 shadow-lg shadow-orange-100">
                <table className="min-w-full divide-y divide-orange-100 text-left text-sm">
                    <thead className="bg-[#fef7ef] text-xs uppercase tracking-wide text-slate-500">
                    <tr>
                        <th className="px-6 py-3">Transaction #</th>
                        <th className="px-6 py-3">Player</th>
                        <th className="px-6 py-3">Amount</th>
                        <th className="px-6 py-3">Status</th>
                        <th className="px-6 py-3">Actions</th>
                    </tr>
                    </thead>

                    <tbody className="divide-y divide-orange-50">
                    {loading && (
                        <tr>
                            <td colSpan={5} className="px-6 py-8 text-center text-slate-500">Loading deposits…</td>
                        </tr>
                    )}

                    {!loading && rows.length === 0 && (
                        <tr>
                            <td colSpan={5} className="px-6 py-8 text-center text-slate-500">No deposits match the current filters.</td>
                        </tr>
                    )}

                    {!loading && rows.map(({t, fullName, email}) => (
                        <tr key={t.transactionId} className="transition hover:bg-[#fff8f0]">
                            <td className="px-6 py-4 font-semibold text-slate-900">{t.mobilePayReqId}</td>

                            <td className="px-6 py-4 text-slate-700">
                                <div className="font-medium text-slate-900">{fullName}</div>
                                <div className="text-xs text-slate-500">{email || "—"}</div>
                            </td>

                            <td className="px-6 py-4 text-slate-700">{t.amount} DKK</td>

                            <td className="px-6 py-4">
                  <span
                      className={`rounded-full px-3 py-1 text-xs font-semibold ${
                          t.status === "Approved"
                              ? "bg-emerald-100 text-emerald-700"
                              : t.status === "Rejected"
                                  ? "bg-rose-100 text-rose-700"
                                  : "bg-amber-100 text-amber-700"
                      }`}
                  >
                    {t.status}
                  </span>
                            </td>

                            <td className="px-6 py-4 space-x-2">
                                <button
                                    type="button"
                                    onClick={() => updateStatus(t, "Approved")}
                                    disabled={isUpdating || t.status === "Approved"}
                                    className="rounded-full bg-emerald-100 px-3 py-1 text-xs font-semibold text-emerald-700 shadow-inner disabled:cursor-not-allowed disabled:opacity-60"
                                >
                                    Approve
                                </button>

                                <button
                                    type="button"
                                    onClick={() => updateStatus(t, "Rejected")}
                                    disabled={isUpdating || t.status === "Rejected"}
                                    className="rounded-full bg-rose-100 px-3 py-1 text-xs font-semibold text-rose-700 shadow-inner disabled:cursor-not-allowed disabled:opacity-60"
                                >
                                    Reject
                                </button>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
        </section>
    );
}
