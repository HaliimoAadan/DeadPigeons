import {useState} from "react";
import {type LoginRequestDto, type RegisterRequestDto} from "@core/generated-client.ts";
import {authApi} from "@utilities/authApi.ts";
import toast from "react-hot-toast";
//import {libraryApi} from "@utilities/libraryApi.ts";
//import {SieveQueryBuilder} from "ts-sieve-query-builder";

import { useNavigate } from "react-router";
import { useSetAtom } from "jotai";
import { currentUserAtom } from "@components/dashboard/state/gameAtoms";

// login options -- admin or admin
const newUsers = [
    {
        role: "player" as const,
        name: "Guest Player",
        heading: "Player",
        description:
            "Pick your numbers, review your boards, and track your wins.",
        destination: "/player",
    },
    {
        role: "admin" as const,
        name: "Guest Admin",
        heading: "Admin",
        description: "Manage players, publish results, and maintain history.",
        destination: "/admin",
    },
];
export default function Auth() {

    const navigate = useNavigate();
    const setCurrentUser = useSetAtom(currentUserAtom);
    
    const [registerForm, setRegisterForm] = useState<RegisterRequestDto>({
        email: '',
        password: ''
    })
    //const [books, setBooks] = useState<Book[]>([])

// Switch between Admin / Player mock login
    const handleSelectUser = (role: "player" | "admin") => {
        const user = newUsers.find((u) => u.role === role);
        if (!user) return;

        setCurrentUser({
            name: user.name,
            role: user.role,
        });

        navigate(user.destination);
    };

    
    return (

        <div className="min-h-screen bg-[#f8f1e7] px-6 py-12 text-slate-800">

            {/* Mock login selector */}
            <div
                className="mx-auto mb-10 flex max-w-4xl flex-col gap-8 rounded-3xl bg-white/80 p-8 text-center shadow-lg shadow-orange-100">
                <div>
                    <p className="text-xs uppercase tracking-[0.3em] text-slate-400">
                        Login
                    </p>
                    <h1 className="mt-2 text-3xl font-semibold text-slate-900">
                        Choose User Type
                    </h1>
                    <p className="mt-3 text-slate-600">
                        Use this to preview Player or Admin interfaces while login is
                        under development.
                    </p>
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                    {newUsers.map((user) => (
                        <button
                            key={user.role}
                            onClick={() => handleSelectUser(user.role)}
                            className="rounded-3xl border border-orange-100 bg-[#fefbf7] p-6 text-left transition hover:border-orange-300 hover:shadow-lg"
                        >
                            <p className="text-sm uppercase tracking-[0.3em] text-slate-400">
                                {user.heading}
                            </p>
                            <p className="mt-2 text-2xl font-semibold text-slate-900">
                                {user.name}
                            </p>
                            <p className="mt-3 text-sm text-slate-600">
                                {user.description}
                            </p>
                            <p className="mt-6 text-sm font-semibold text-[#f1812c]">
                                Enter {user.heading} View →
                            </p>
                        </button>
                    ))}
                </div>
            </div>
    
            <div className="mx-auto max-w-lg space-y-6 rounded-xl bg-white/70 p-6 shadow">
                <h2 className="text-xl font-semibold">Register (for later)</h2>

                <input
                    className="input input-bordered w-full"
                    placeholder="Email"
                    value={registerForm.email}
                    onChange={(e) =>
                        setRegisterForm({ ...registerForm, email: e.target.value })
                    }
                />

                <input
                    className="input input-bordered w-full"
                    type="password"
                    placeholder="Password (min 8 chars)"
                    value={registerForm.password}
                    onChange={(e) =>
                        setRegisterForm({ ...registerForm, password: e.target.value })
                    }
                />

                <button
                    className="btn btn-primary w-full"
                    disabled={registerForm.password.length < 8}
                    onClick={() => {
                        authApi.register(registerForm).then((r) => {
                            localStorage.setItem("jwt", r.token);
                            toast.success("Welcome!");
                        });
                    }}
                >
                    Register me
                </button>
            </div>
        </div>
    );
}