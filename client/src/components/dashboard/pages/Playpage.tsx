import {useAtom} from "jotai";
import NumberGrid from "../NumberGrid";
import {playerSelectionAtom} from "../state/gameAtoms";

const pricing: Record<number, number> = {
    5: 20,
    6: 40,
    7: 80,
    8: 160
};

const getWeekNumber = (date: Date) => {
    const startOfYear = new Date(Date.UTC(date.getUTCFullYear(), 0, 1));
    const pastDays = Math.floor((date.getTime() - startOfYear.getTime()) / 86400000);
    return Math.ceil((pastDays + startOfYear.getUTCDay() + 1) / 7);
};

export default function PlayPage() {
    const [selectedNumbers, setSelectedNumbers] = useAtom(playerSelectionAtom);
    const today = new Date();
    const currentWeek = getWeekNumber(today);

    const toggleNumber = (value: number) => {
        setSelectedNumbers((prev) =>
            prev.includes(value)
                ? prev.filter((item) => item !== value)
                : [...prev, value]
        );
    };

    const price = pricing[selectedNumbers.length];
    const isReady = selectedNumbers.length >= 5 && selectedNumbers.length <= 8;

    return (
        <section className="space-y-8">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Current Game</p>
                <h2 className="text-3xl font-semibold text-slate-900">Week {currentWeek} {today.getFullYear()}</h2>
                <p className="mt-2 max-w-2xl text-slate-600">
                    Pick between <strong className="font-semibold text-slate-800">5 and 8 numbers</strong>. Selected
                    squares glow orange and count toward the price ladder shown below.
                </p>
            </div>
            <div className="rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                <NumberGrid selectedNumbers={selectedNumbers} onToggle={toggleNumber} maxSelectable={8}/>
                <div className="mt-6 flex flex-col items-center gap-4 text-center">
                    <p className="text-lg font-semibold text-slate-700">
                        {isReady ? `Price: ${price} kr` : "Select between 5 and 8 numbers to continue"}
                    </p>
                    <p className="text-sm text-slate-500">5 picks = 20 kr · 6 = 40 kr · 7 = 80 kr · 8 = 160 kr</p>
                    <button
                        type="button"
                        disabled={!isReady}
                        className={`w-48 rounded-full px-6 py-3 text-lg font-semibold transition ${
                            isReady
                                ? "bg-[#f7a166] text-white shadow-xl shadow-orange-200"
                                : "bg-slate-200 text-slate-500"
                        }`}
                    >
                        Next!
                    </button>
                </div>
            </div>
        </section>
    );
}