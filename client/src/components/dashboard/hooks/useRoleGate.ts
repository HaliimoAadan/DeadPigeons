import {useNavigate} from "react-router";
import {currentUserAtom, type DashboardUser} from "../state/gameAtoms";
import {useEffect} from "react";
import {useAtomValue} from "jotai";

export function useRoleGate(requiredRole: DashboardUser["role"]) {
    const currentUser = useAtomValue(currentUserAtom);
    const navigate = useNavigate();

    useEffect(() => {
        if (!currentUser || currentUser.role !== requiredRole) {
            navigate("/auth", {replace: true});
        }
    }, [currentUser, requiredRole, navigate]);

    return currentUser && currentUser.role === requiredRole ? currentUser : null;
}