import {type FormEvent, useEffect, useMemo, useState} from "react";
import toast from "react-hot-toast";
import type {PlayerCreateDto, PlayerResponseDto} from "@core/generated-client.ts";
import {playersApi} from "@utilities/playersApi.ts"

const initialFormState: PlayerCreateDto = {
    fullName: "",
    email: "",
    phoneNumber: "",
    isActive: true
};



export default function ManagePlayersPage() {
    const [players, setPlayers] = useState<PlayerResponseDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [formState, setFormState] = useState<PlayerCreateDto>(initialFormState);

    useEffect(() => {
        const fetchPlayers = async () => {
            try {
                const response = await playersApi.getAll(undefined);
                setPlayers(response ?? []);
            } catch (error) {
                console.error(error);
                toast.error("Could not load players.");
            } finally {
                setLoading(false);
            }
        };

        fetchPlayers();
    }, []);

    const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setSubmitting(true);

        try {
            const createdPlayer = await playersApi.create(formState);
            if (createdPlayer) {
                setPlayers((prev) => [createdPlayer, ...prev]);
                setFormState(initialFormState);
                toast.success("Player created successfully.");
            }
        } catch (error) {
            console.error(error);
            toast.error("Could not create player.");
        } finally {
            setSubmitting(false);
        }
    };

    const toggleActive = async (player: PlayerResponseDto) => {
        const nextStatus = !player.isActive;

        try {
            await playersApi.updateStatus(player.playerId, nextStatus);
            setPlayers((prev) =>
                prev.map((existing) => existing.playerId === player.playerId ? {
                    ...existing,
                    isActive: nextStatus
                } : existing)
            );
            toast.success(`${player.fullName} marked as ${nextStatus ? "active" : "inactive"}.`);
        } catch (error) {
            console.error(error);
            toast.error("Could not update status.");
        }

    };
    const isSubmitDisabled = useMemo(() => {
        return submitting || !formState.fullName || !formState.email || !formState.phoneNumber;
    }, [formState.email, formState.fullName, formState.phoneNumber, submitting]);
    

    return (
        <section className="space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Players</p>
                    <h2 className="text-3xl font-semibold text-slate-900">Manage Players</h2>
                </div>
                <button 
                    form="new-player-form" 
                    type="submit"
                    disabled={isSubmitDisabled}
                    className="rounded-full bg-[#f7a166] px-6 py-3 font-semibold text-white shadow-lg shadow-orange-200 disabled:cursor-not-allowed disabled:opacity-70"
                >
                    {submitting ? "Adding..." : "Add Player"}

                </button>
            </div>
            <form
                id="new-player-form"
                onSubmit={handleSubmit}
                className="grid gap-4 rounded-3xl bg-white/90 p-6 shadow-lg shadow-orange-100 md:grid-cols-4"
            >
                <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                    Full Name
                    <input
                        required
                        value={formState.fullName}
                        onChange={(event) => setFormState((prev) => ({...prev, fullName: event.target.value}))}
                        className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                        placeholder="Jane Doe"
                    />
                </label>
                <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                    Email
                    <input
                        required
                        type="email"
                        value={formState.email}
                        onChange={(event) => setFormState((prev) => ({...prev, email: event.target.value}))}
                        className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                        placeholder="player@example.com"
                    />
                </label>
                <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                    Phone
                    <input
                        required
                        value={formState.phoneNumber}
                        onChange={(event) => setFormState((prev) => ({...prev, phoneNumber: event.target.value}))}
                        className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                        placeholder="+45 1234 5678"
                    />
                </label>
                <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
                    Status
                    <select
                        value={formState.isActive ? "active" : "inactive"}
                        onChange={(event) => setFormState((prev) => ({
                            ...prev,
                            isActive: event.target.value === "active"
                        }))}
                        className="rounded-2xl border border-orange-100 px-4 py-3 text-sm shadow-inner focus:border-orange-300 focus:outline-none"
                    >
                        <option value="active">Active</option>
                        <option value="inactive">Inactive</option>
                    </select>
                </label>
            </form>
            
            <div className="overflow-hidden rounded-3xl bg-white/90 shadow-lg shadow-orange-100">
                <table className="min-w-full divide-y divide-orange-100 text-left text-sm">
                    <thead className="bg-[#fef7ef] text-xs uppercase tracking-wide text-slate-500">
                    <tr>
                        <th className="px-6 py-3">Name</th>
                        <th className="px-6 py-3">Email</th>
                        <th className="px-6 py-3">Phone</th>
                        <th className="px-6 py-3">Status</th>
                    </tr>
                    </thead>
                    <tbody className="divide-y divide-orange-50">
                    {loading && (
                        <tr>
                            <td colSpan={4} className="px-6 py-8 text-center text-slate-500">
                                Loading players...
                            </td>
                        </tr>
                    )}
                    {!loading && players.length === 0 && (
                        <tr>
                            <td colSpan={4} className="px-6 py-8 text-center text-slate-500">
                                No players found.
                            </td>
                        </tr>
                    )}
                    {!loading && players.map((player) => (
                        <tr key={player.playerId} className="transition hover:bg-[#fff8f0]">
                            <td className="px-6 py-4 font-medium text-slate-900">{player.fullName}</td>
                            <td className="px-6 py-4 text-slate-600">{player.email}</td>
                            <td className="px-6 py-4 text-slate-600">{player.phoneNumber}</td>
                            <td className="px-6 py-4">
                                <button
                                    type="button"
                                    onClick={() => toggleActive(player)}
                                    className={`rounded-full px-4 py-1 text-xs font-semibold transition ${
                                        player.isActive
                                            ? "bg-emerald-100 text-emerald-700"
                                            : "bg-slate-200 text-slate-500"
                                    }`}
                                >
                                    {player.isActive ? "Active" : "Inactive"}
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