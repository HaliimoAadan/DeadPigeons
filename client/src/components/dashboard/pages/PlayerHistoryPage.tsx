const games = [
    {week: 45, year: 2025, numbers: [2, 5, 9, 11, 14], result: 'Pending'},
    {week: 44, year: 2025, numbers: [1, 4, 7, 10, 13, 16], result: 'No match'},
    {week: 43, year: 2025, numbers: [3, 6, 8, 12, 15, 16, 2], result: 'Winner'}
];

const resultStyles: Record<string, string> = {
    Pending: 'bg-yellow-100 text-yellow-800',
    'No match': 'bg-slate-200 text-slate-600',
    Winner: 'bg-emerald-100 text-emerald-700'
};

export default function PlayerHistoryPage() {
    return (
        <section className="space-y-6">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Your results</p>
                <h2 className="text-3xl font-semibold text-slate-900">History</h2>
                <p className="mt-2 max-w-3xl text-slate-600">
                    Every board you submitted shows up here with the official winning status once the admin publishes the
                    numbers.
                </p>
            </div>
            <div className="space-y-4">
                {games.map((game) => (
                    <article key={`${game.week}-${game.year}`} className="rounded-3xl bg-white/80 p-5 shadow-lg shadow-orange-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-xs uppercase tracking-wide text-slate-400">Week</p>
                                <p className="text-2xl font-semibold text-slate-900">{game.week}</p>
                                <p className="text-sm text-slate-500">{game.year}</p>
                            </div>
                            <span className={`rounded-full px-4 py-1 text-xs font-semibold ${resultStyles[game.result]}`}>
                                {game.result}
                            </span>
                        </div>
                        <p className="mt-4 text-sm uppercase tracking-wide text-slate-500">Numbers you played</p>
                        <div className="mt-2 flex flex-wrap gap-2">
                            {game.numbers.map((number) => (
                                <span key={`${game.week}-${number}`} className="flex h-10 w-10 items-center justify-center rounded-2xl bg-[#f7a166] text-white">
                                    {number}
                                </span>
                            ))}
                        </div>
                    </article>
                ))}
            </div>
        </section>
    );
}