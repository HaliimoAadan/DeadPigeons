const isProduction = import.meta.env.PROD;

const envUrl = import.meta.env.VITE_BACKEND_URL || import.meta.env.BACKEND_URL;
 const prod = "https://projectsolutionserver.fly.dev";
 const dev = "http://localhost:5284";
export const baseUrl = envUrl ?? (isProduction ? prod : dev); //Lets us manually set which backend the frontend talks to