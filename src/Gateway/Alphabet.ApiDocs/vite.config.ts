import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: "wwwroot",
    emptyOutDir: false
  },
  server: {
    proxy: {
      "/api": "https://localhost:58241",
      "/health": "https://localhost:58241",
      "/openapi": "https://localhost:58441"
    }
  }
});
