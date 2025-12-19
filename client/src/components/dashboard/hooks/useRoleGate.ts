import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAtom } from "jotai";
import { currentUserAtom, type DashboardUser } from "../state/gameAtoms";

export function useRoleGate(requiredRole: DashboardUser["role"]) {
    const [currentUser, setCurrentUser] = useAtom(currentUserAtom);
    const navigate = useNavigate();

    useEffect(() => {
        const token = localStorage.getItem("jwt");
        const storedRole = localStorage.getItem("userRole") as DashboardUser["role"] | null;
        const storedName = localStorage.getItem("userName");

        if (!token || storedRole !== requiredRole) {
            setCurrentUser(null);
            navigate("/auth", { replace: true });
            return;
        }

        // Ensure atom is populated (or consistent) after refresh
        if (!currentUser || currentUser.role !== storedRole) {
            setCurrentUser({
                name: storedName ?? "Authenticated User",
                role: storedRole,
            });
        }
    }, [currentUser, requiredRole, navigate, setCurrentUser]);

    return currentUser?.role === requiredRole ? currentUser : null;
}
