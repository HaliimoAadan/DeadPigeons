type NumberGridProps = {
    selectedNumbers: number[];
    onToggle: (value: number) => void;
    maxSelectable: number;
};

const numbers = Array.from({length: 16}, (_, index) => index + 1);

export default function NumberGrid({selectedNumbers, onToggle, maxSelectable}: NumberGridProps) {
    return (
        <div className="grid grid-cols-4 gap-3">
            {numbers.map((value) => {
                const isSelected = selectedNumbers.includes(value);
                const limitReached = selectedNumbers.length >= maxSelectable && !isSelected;
                return (
                    <button
                        key={value}
                        type="button"
                        onClick={() => !limitReached && onToggle(value)}
                        className={`h-16 rounded-2xl border-2 text-lg font-semibold transition ${
                            isSelected
                                ? "border-orange-400 bg-[#f7a166] text-white shadow-lg shadow-orange-200"
                                : "border-transparent bg-[#fdf5ec] text-slate-700"
                        } ${limitReached ? "cursor-not-allowed opacity-50" : "hover:border-orange-200"}`}
                    >
                        {value}
                    </button>
                );
            })}
        </div>
    );
}