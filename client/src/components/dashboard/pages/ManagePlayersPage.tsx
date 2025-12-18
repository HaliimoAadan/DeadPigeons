import {useState} from "react";

type Player = {
    id: number;
    name: string;
    email: string;
    phone: string;
    active: boolean;
};

const seedPlayers: Player[] = [
    {id: 1, name: "Silas Madsen", email: "silas@jerneif.dk", phone: "+45 1111 2222", active: true},
    {id: 2, name: "Freja Sørensen", email: "freja@jerneif.dk", phone: "+45 2222 3333", active: true},
    {id: 3, name: "Rune Larsen", email: "rune@jerneif.dk", phone: "+45 3333 4444", active: false},
    {id: 4, name: "Asta Hvid", email: "asta@jerneif.dk", phone: "+45 4444 5555", active: true}
];

export default function ManagePlayersPage() {
    const [players, setPlayers] = useState<Player[]>(seedPlayers);

    const toggleActive = (id: number) => {
        setPlayers((prev) =>
            prev.map((player) =>
                player.id === id ? {...player, active: !player.active} : player
            )
        );
    };

    return (
        <section className="space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                    <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Players</p>
                    <h2 className="text-3xl font-semibold text-slate-900">Manage Players</h2>
                </div>
                <button className="rounded-full bg-[#f7a166] px-6 py-3 font-semibold text-white shadow-lg shadow-orange-200">
                    Add Player
                </button>
            </div>
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
                    {players.map((player) => (
                        <tr key={player.id} className="transition hover:bg-[#fff8f0]">
                            <td className="px-6 py-4 font-medium text-slate-900">{player.name}</td>
                            <td className="px-6 py-4 text-slate-600">{player.email}</td>
                            <td className="px-6 py-4 text-slate-600">{player.phone}</td>
                            <td className="px-6 py-4">
                                <button
                                    type="button"
                                    onClick={() => toggleActive(player.id)}
                                    className={`rounded-full px-4 py-1 text-xs font-semibold transition ${
                                        player.active
                                            ? "bg-emerald-100 text-emerald-700"
                                            : "bg-slate-200 text-slate-500"
                                    }`}
                                >
                                    {player.active ? "Active" : "Inactive"}
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