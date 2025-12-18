import {useAtom} from "jotai";
import NumberGrid from "../NumberGrid";
import {adminSelectionAtom} from "../state/gameAtoms";

export default function AdminWinningNumbersPage() {
    const [selectedNumbers, setSelectedNumbers] = useAtom(adminSelectionAtom);

    const toggleNumber = (value: number) => {
        setSelectedNumbers((prev) =>
            prev.includes(value)
                ? prev.filter((item) => item !== value)
                : [...prev, value]
        );
    };

    const canPublish = selectedNumbers.length === 3;

    return (
        <section className="space-y-8">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Admin only</p>
                <h2 className="text-3xl font-semibold text-slate-900">Publish Winning Numbers</h2>
                <p className="mt-2 max-w-2xl text-slate-600">
                    Select the three numbers that were drawn from the hat this Saturday. Once published, the system will lock
                    further board purchases for the round.
                </p>
            </div>
            <div className="rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                <NumberGrid selectedNumbers={selectedNumbers} onToggle={toggleNumber} maxSelectable={3}/>
                <div className="mt-6 flex flex-col items-center gap-4">
                    <p className="text-sm text-slate-500">Exactly three numbers are required.</p>
                    <button
                        type="button"
                        disabled={!canPublish}
                        className={`w-64 rounded-full px-6 py-3 text-lg font-semibold transition ${
                            canPublish
                                ? "bg-emerald-500 text-white shadow-xl shadow-emerald-200"
                                : "bg-slate-200 text-slate-500"
                        }`}
                    >
                        Publish Winning Numbers
                    </button>
                </div>
            </div>
        </section>
    );
}