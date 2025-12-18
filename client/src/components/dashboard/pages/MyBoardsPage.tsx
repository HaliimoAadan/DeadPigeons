const boards = [
    {
        id: 'board-1',
        numbers: [2, 5, 9, 11, 14],
        purchased: '12 Nov 2025',
        price: 20,
        status: 'Repeating',
        nextCharge: 'Week 47'
    },
    {
        id: 'board-2',
        numbers: [1, 4, 7, 10, 13, 16],
        purchased: '05 Nov 2025',
        price: 40,
        status: 'Repeating',
        nextCharge: 'Week 46'
    },
    {
        id: 'board-3',
        numbers: [3, 6, 8, 12, 15, 16, 2],
        purchased: '29 Oct 2025',
        price: 80,
        status: 'Paused',
        nextCharge: 'Paused'
    }
];

export default function MyBoardsPage() {
    return (
        <section className="space-y-6">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Your boards</p>
                <h2 className="text-3xl font-semibold text-slate-900">My Boards</h2>
                <p className="mt-2 max-w-3xl text-slate-600">
                    Track every board you have purchased, including when it was bought, the price level, and whether it
                    continues into the next game.
                </p>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
                {boards.map((board) => (
                    <article key={board.id} className="rounded-3xl bg-white/80 p-5 shadow-lg shadow-orange-100">
                        <div className="flex items-center justify-between text-sm text-slate-500">
                            <span>{board.status}</span>
                            <span>Purchased {board.purchased}</span>
                        </div>
                        <div className="mt-4 flex flex-wrap gap-2">
                            {board.numbers.map((number) => (
                                <span
                                    key={`${board.id}-${number}`}
                                    className="flex h-10 w-10 items-center justify-center rounded-2xl bg-[#f7a166] text-white"
                                >
                                    {number}
                                </span>
                            ))}
                        </div>
                        <div className="mt-4 flex items-center justify-between text-sm text-slate-600">
                            <p>Price: {board.price} kr</p>
                            <p>Next charge: {board.nextCharge}</p>
                        </div>
                        <div className="mt-4 flex gap-3 text-sm font-semibold">
                            <button
                                className="flex-1 rounded-full border border-orange-200 px-4 py-2 text-slate-600 transition hover:bg-orange-50">
                                Pause
                            </button>
                            <button
                                className="flex-1 rounded-full bg-[#f7a166] px-4 py-2 text-white shadow-orange-200 transition hover:shadow-lg">
                                Edit Numbers
                            </button>
                        </div>
                    </article>
                ))}
            </div>
        </section>
    );
}