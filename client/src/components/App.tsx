import {createBrowserRouter, Navigate, RouterProvider} from "react-router";
import Home from "@components/Home.tsx";
import {DevTools} from "jotai-devtools";
import 'jotai-devtools/styles.css'
import {Toaster} from "react-hot-toast";
import Auth from "@components/routes/auth/Auth.tsx";
import PlayerDashboard from "@components/dashboard/Dashboard.tsx";
import PlayPage from "@components/dashboard/pages/PlayPage.tsx";
import MyBoardsPage from "@components/dashboard/pages/MyBoardsPage.tsx";
import PlayerHistoryPage from "@components/dashboard/pages/PlayerHistoryPage.tsx";
import AdminDashboard from "@components/dashboard/AdminDashboard.tsx";
import AdminDashboardPage from "@components/dashboard/pages/AdminDashboardPage.tsx";
import ManagePlayersPage from "@components/dashboard/pages/ManagePlayersPage.tsx";
import AdminWinningNumbersPage from "@components/dashboard/pages/AdminWinningNumbersPage.tsx";
import AdminGameHistoryPage from "@components/dashboard/pages/AdminGameHistoryPage.tsx";

function App() {
    return (
        <>
            <RouterProvider router={createBrowserRouter([
                {
                    path: '/',
                    element: <Home/>,
                    children: [
                        {
                            index: true ,
                            element: <Navigate to="/auth" replace/>
                        },
                    //     here you can just make more components: i just have a a single auth page with a quick crud test

                        {
                            path: 'player',
                            element: <PlayerDashboard/>,
                            children: [
                                {
                                    index: true,
                                    element: <PlayPage/>
                                },
                                {
                                    path: 'boards',
                                    element: <MyBoardsPage/>
                                },
                                {
                                    path: 'history',
                                    element: <PlayerHistoryPage/>
                                }
                            ]
                        },
                        {
                            path: 'admin',
                            element: <AdminDashboard/>,
                            children: [
                                {
                                    index: true,
                                    element: <AdminDashboardPage/>
                                },
                                {
                                    path: 'manage-players',
                                    element: <ManagePlayersPage/>
                                },
                                {
                                    path: 'winning-numbers',
                                    element: <AdminWinningNumbersPage/>
                                },
                                {
                                    path: 'game-history',
                                    element: <AdminGameHistoryPage/>
                                }
                            ]
                        },
                        {
                            path: 'auth',
                            element: <Auth/>
                        }
                    ]
                }
            ])}/>
            <DevTools/>
            <Toaster
                position="top-center"
                reverseOrder={false}
            />
        </>
    )
}

export default App
