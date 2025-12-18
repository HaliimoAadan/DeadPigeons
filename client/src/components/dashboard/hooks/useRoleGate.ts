import {useNavigate} from "react-router";
import {currentUserAtom, type DashboardUser} from "../state/gameAtoms";
import {useEffect} from "react";
import {useAtom} from "jotai";

export function useRoleGate(requiredRole: DashboardUser["role"]) {
    const [currentUser = useAtom, setCurrentUser] = useAtom(currentUserAtom);
    const navigate = useNavigate();

    useEffect(() => {
        const token = localStorage.getItem("jwt");
        const storedRole = localStorage.getItem("userRole") as DashboardUser["role"] | null;
        const storedName = localStorage.getItem("userName");

        if (!token || storedRole !== requiredRole) {
            setCurrentUser(null);
            
            navigate("/auth", {replace: true});

            return;
        }

        if (!currentUser || currentUser.role !== storedRole) {
            setCurrentUser({
                name: storedName ?? "Authenticated User",
                role: storedRole,
            });
        }
    }, [currentUser, requiredRole, navigate, setCurrentUser]);

    return currentUser && currentUser.role === requiredRole ? currentUser : null;
}