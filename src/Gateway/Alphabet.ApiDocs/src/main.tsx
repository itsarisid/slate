import React from "react";
import { createRoot } from "react-dom/client";
import "../wwwroot/styles.css";

function App() {
  return (
    <main className="panel">
      <p className="eyebrow">Alphabet API Docs</p>
      <h1>Vite React source lives here.</h1>
      <p>
        The production portal is prebuilt in <code>wwwroot</code> so the .NET docs gateway works immediately. Use this
        Vite entrypoint for future componentized development.
      </p>
    </main>
  );
}

createRoot(document.getElementById("root")!).render(<App />);
