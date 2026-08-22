import { defineConfig } from "vite";

export default defineConfig({
  base: "./",
  build: {
    outDir: "../../Assets/StreamingAssets/LuaPad",
    emptyOutDir: true,
  },
});
