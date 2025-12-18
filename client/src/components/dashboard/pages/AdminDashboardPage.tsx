const stats = [
    {label: 'Total players', value: 164},
    {label: 'Active players', value: 127},
    {label: 'Pending deposits', value: 6},
    {label: 'Current week', value: 'Week 47'}
];

const notices = [
    {
        title: 'Balance approvals',
        detail: '3 MobilePay transfers are waiting for approval before Saturday.',
        cta: 'Review deposits'
    },
    {
        title: 'Repeating boards expiring',
        detail: '12 players will stop auto-play next week. Remind them to top up.',
        cta: 'Send reminder'
    }
];

export default function AdminDashboardPage() {
    return (
        <section className="space-y-6">
            <div>
                <p className="text-sm uppercase tracking-[0.3em] text-slate-500">Overview</p>
                <h2 className="text-3xl font-semibold text-slate-900">Admin's Dashboard</h2>
                <p className="mt-2 max-w-3xl text-slate-600">
                    Keep tabs on Jerne IF's Dead Pigeons game before entering the weekly results. These widgets summarize the
                    operations you need to take care of before the Saturday deadline.
                </p>
            </div>
            <div className="grid gap-4 md:grid-cols-4">
                {stats.map((stat) => (
                    <article key={stat.label} className="rounded-3xl bg-white/80 p-5 text-center shadow-lg shadow-orange-100">
                        <p className="text-xs uppercase tracking-wide text-slate-400">{stat.label}</p>
                        <p className="mt-2 text-3xl font-semibold text-slate-900">{stat.value}</p>
                    </article>
                ))}
            </div>
            <div className="grid gap-4 md:grid-cols-2">
                {notices.map((notice) => (
                    <article key={notice.title} className="rounded-3xl bg-white/80 p-6 shadow-lg shadow-orange-100">
                        <p className="text-sm uppercase tracking-wide text-slate-400">{notice.title}</p>
                        <p className="mt-2 text-lg font-semibold text-slate-900">{notice.detail}</p>
                        <button className="mt-4 rounded-full bg-[#f7a166] px-4 py-2 text-sm font-semibold text-white shadow-orange-200 transition hover:shadow-lg">
                            {notice.cta}
                        </button>
                    </article>
                ))}
            </div>
        </section>
    );
}