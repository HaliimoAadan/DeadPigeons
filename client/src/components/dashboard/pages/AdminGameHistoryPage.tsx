const history = [
    {week: 47, year: 2025, winning: [2, 7, 11], digitalBoards: 118, winners: 4},
    {week: 46, year: 2025, winning: [1, 4, 16], digitalBoards: 105, winners: 3},
    {week: 45, year: 2025, winning: [3, 5, 9], digitalBoards: 99, winners: 2}
];

export default function AdminGameHistoryPage() {
    return (
        <section className="space-y-6">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Weekly overview</p>
                <h2 className="text-3xl font-semibold text-slate-900">Game History</h2>
                <p className="mt-2 max-w-2xl text-slate-600">
                    Keep the offline and digital games in sync. Each entry lists the winning combination and the number of
                    digital boards submitted to help the admins split prizes.
                </p>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
                {history.map((entry) => (
                    <article
                        key={`${entry.week}-${entry.year}`}
                        className="rounded-3xl bg-white/80 p-5 shadow-lg shadow-orange-100"
                    >
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-xs uppercase tracking-wide text-slate-400">Week</p>
                                <p className="text-2xl font-semibold text-slate-900">{entry.week}</p>
                            </div>
                            <span className="rounded-full bg-[#fcd8b4] px-4 py-1 text-xs font-semibold text-slate-700">
                                {entry.year}
                            </span>
                        </div>
                        <p className="mt-4 text-sm uppercase tracking-wide text-slate-500">Winning numbers</p>
                        <div className="mt-2 flex gap-2">
                            {entry.winning.map((number) => (
                                <span
                                    key={number}
                                    className="flex h-10 w-10 items-center justify-center rounded-2xl bg-[#f7a166] text-white"
                                >
                                    {number}
                                </span>
                            ))}
                        </div>
                        <div className="mt-6 flex items-center justify-between text-sm text-slate-600">
                            <p>{entry.digitalBoards} digital boards</p>
                            <p>{entry.winners} winning boards</p>
                        </div>
                    </article>
                ))}
            </div>
        </section>
    );
}