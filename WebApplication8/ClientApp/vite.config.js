import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Builds the React app into wwwroot/dist with stable filenames so the
// Razor view can reference ~/dist/react-demo.js directly.
export default defineConfig({
    plugins: [react()],
    // Dev server used for Hot Module Replacement / React Fast Refresh.
    // The Razor view loads modules from this origin when Vite:UseDevServer is on.
    server: {
        port: 5173,
        strictPort: true,
        cors: true,
        origin: "http://localhost:5173",
    },



    build: {
        outDir: "../wwwroot/dist",
        emptyOutDir: true,
        rollupOptions: {
            // Separate React apps, each built to its own file.
            input: {
                "react-demo": "src/main.jsx",
                "crud-demo": "src/crud.jsx",
                "employees-demo": "src/employees.jsx",
                "departments-demo": "src/departments.jsx",
                "categories-demo": "src/categories.jsx",
                "salaries-demo": "src/salaries.jsx"
            },
            output: {
                entryFileNames: "[name].js",       // -> react-demo.js, crud-demo.js
                chunkFileNames: "chunk-[name].js", // shared React code
                assetFileNames: "[name][extname]",
            },
        },
    },
});
