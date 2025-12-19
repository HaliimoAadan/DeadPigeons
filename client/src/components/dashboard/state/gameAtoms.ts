import {atom} from "jotai";

export type DashboardUser = {
    name: string;
    role: "admin" | "player";
};

export const currentUserAtom = atom<DashboardUser | null>(null);

export const playerSelectionAtom = atom<number[]>([]);
export const adminSelectionAtom = atom<number[]>([]);