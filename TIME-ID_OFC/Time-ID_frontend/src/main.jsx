import { lazy, StrictMode, Suspense } from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";

const LocationPage = lazy(() => import("./pages/LocationPage.jsx"));

const { pathname, search } = window.location;
const employeeId = pathname === "/geolocalizacao"
  ? new URLSearchParams(search).get("mapa")
  : pathname.match(/^\/geolocalizacao\/mapa\/([^/]+)\/?$/)?.[1];

createRoot(document.getElementById("root")).render(
  <StrictMode>
    {employeeId ? (
      <Suspense fallback={<main className="carregando-sessao" role="status">Carregando mapa…</main>}>
        <LocationPage employeeId={employeeId} />
      </Suspense>
    ) : <App />}
  </StrictMode>
);
