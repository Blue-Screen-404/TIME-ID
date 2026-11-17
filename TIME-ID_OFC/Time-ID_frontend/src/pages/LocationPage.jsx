import { useEffect, useRef, useState } from "react";
import { divIcon, map as createMap, marker as createMarker, tileLayer } from "leaflet";
import { Clock3, ExternalLink, MapPin } from "lucide-react";
import "leaflet/dist/leaflet.css";
import { geoApi, locationPreviewKey } from "../services/geolocation";
import logoTimeId from "../assets/logo-timeid-esmeralda.png";
import "./LocationPage.css";

const coordinates = new Intl.NumberFormat("pt-BR", {
  minimumFractionDigits: 6,
  maximumFractionDigits: 6,
  useGrouping: false,
});
const updatedTime = new Intl.DateTimeFormat("pt-BR", {
  dateStyle: "short",
  timeStyle: "short",
  timeZone: "America/Sao_Paulo",
});

function LocationMap({ latitude, longitude, name }) {
  const container = useRef(null);
  const view = useRef(null);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    const map = createMap(container.current);
    const tiles = tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
    });
    let hasTile = false;
    tiles.on("loading", () => { hasTile = false; });
    tiles.on("tileload", () => { hasTile = true; });
    tiles.on("load", () => setFailed(!hasTile));
    tiles.addTo(map);
    view.current = { map, marker: null };
    return () => {
      map.remove();
      view.current = null;
    };
  }, []);

  useEffect(() => {
    if (!view.current) return;
    const { map, marker } = view.current;
    const point = [latitude, longitude];
    if (marker) {
      marker.setLatLng(point);
      map.panTo(point);
      return;
    }
    const icon = divIcon({
      className: "location-map-pin",
      html: "<span></span>",
      iconSize: [34, 40],
      iconAnchor: [17, 39],
    });
    map.setView(point, 16);
    view.current.marker = createMarker(point, { icon, title: `Localização de ${name}` }).addTo(map);
  }, [latitude, longitude, name]);

  return (
    <section className="location-map-panel" aria-label={`Mapa da localização de ${name}`}>
      <div className="location-map" ref={container} />
      {failed && <p role="status">O mapa base está indisponível. Use o link abaixo dos dados da localização para abrir no OpenStreetMap.</p>}
    </section>
  );
}

export default function LocationPage({ employeeId }) {
  const [location, setLocation] = useState(null);
  const [error, setError] = useState("");
  const [expired, setExpired] = useState(false);
  const name = location?.name;
  const preview = new URLSearchParams(window.location.search).get("previa") === "1";

  useEffect(() => {
    document.title = name ? `Localização de ${name} — TIMEID` : "Localização — TIMEID";
  }, [name]);

  useEffect(() => {
    const controller = new AbortController();
    let pending = false;
    let sessionExpired = false;
    function onExpired() {
      sessionExpired = true;
      setExpired(true);
    }
    async function load() {
      if (pending || sessionExpired) return;
      pending = true;
      try {
        const id = encodeURIComponent(employeeId);
        const options = { signal: controller.signal };
        if (preview) {
          const stored = sessionStorage.getItem(locationPreviewKey(employeeId));
          if (!stored) throw new Error("Abra o mapa pela aba Validar e bater ponto para visualizar a localização capturada.");
          const { name: capturedName, ...capture } = JSON.parse(stored);
          if (capturedName) setLocation({ ...capture, name: capturedName, updatedAt: capture.capturedAt });
          options.method = "POST";
          options.body = capture;
        }
        const result = await geoApi(`/geolocation/${preview ? "preview" : "live-location"}/${id}`, options);
        if (controller.signal.aborted) return;
        setLocation(result);
        setError("");
      } catch (err) {
        if (controller.signal.aborted) return;
        if (!preview || sessionExpired || err.status === 403) setLocation(null);
        setError(err.message);
      } finally {
        pending = false;
      }
    }
    window.addEventListener("timeid:expired", onExpired);
    if (!preview) window.addEventListener("focus", load);
    const timer = preview ? null : window.setInterval(load, 60000);
    load();
    return () => {
      controller.abort();
      window.clearInterval(timer);
      window.removeEventListener("timeid:expired", onExpired);
      window.removeEventListener("focus", load);
    };
  }, [employeeId, preview]);

  return (
    <div className="location-page">
      <header className="location-page-header">
        <div className="location-page-width location-page-header-inner">
          <img src={logoTimeId} alt="TIMEID — sua jornada, nosso controle" />
          {!preview && <span className="location-live-status"><i aria-hidden="true" />Atualiza automaticamente</span>}
        </div>
      </header>
      <main className="location-page-width location-page-content">
        {location ? (
          <>
            <nav className="location-breadcrumb" aria-label="Navegação estrutural">
              <a href="/geolocalizacao">Geolocalização</a><span aria-hidden="true">›</span><span>Localização do colaborador</span>
            </nav>
            <div className="location-title-row">
              <div>
                <h1>{name ? `Localização de ${name}` : "Localização do colaborador"}</h1>
                <p className="location-updated"><i aria-hidden="true" />{preview ? "Prévia da localização capturada" : "Última posição atualizada"} · {updatedTime.format(new Date(location.updatedAt))} (Brasília)</p>
              </div>
            </div>
            <div className="location-page-grid">
              <LocationMap latitude={location.latitude} longitude={location.longitude} name={name} />
              <section className="location-details" aria-labelledby="location-details-title">
                <div className="location-details-heading"><MapPin size={19} aria-hidden="true"/><h2 id="location-details-title">Dados da localização</h2></div>
                <div className="location-person">
                  <span className="location-avatar" aria-hidden="true">
                    {name.trim().split(/\s+/).slice(0, 2).map(part => part[0]).join("").toUpperCase()}
                  </span>
                  <div><span>Colaborador</span><strong>{name}</strong></div>
                </div>
                <dl>
                  <div className="location-address">
                    <dt>Endereço aproximado</dt>
                    <dd>{location.address?.name || "Endereço indisponível"}</dd>
                    {location.address?.attribution && <dd className="location-attribution">{location.address.attribution}</dd>}
                  </div>
                  <div className="location-coordinates">
                    <dt>Latitude</dt><dd>{coordinates.format(location.latitude)}</dd>
                    <dt>Longitude</dt><dd>{coordinates.format(location.longitude)}</dd>
                    <dt>Precisão</dt><dd>±{Math.round(location.accuracyMeters)} m</dd>
                  </div>
                  <div className="location-time-row">
                    <dt><Clock3 size={15} aria-hidden="true"/>{preview ? "Capturada em (Brasília)" : "Última atualização (Brasília)"}</dt>
                    <dd><time dateTime={location.updatedAt}>{updatedTime.format(new Date(location.updatedAt))}</time></dd>
                  </div>
                </dl>
                {location.accuracyMeters >= 100 && <p className="location-accuracy-hint" role="status">A margem de precisão veio ampla do dispositivo. No Brave, permita a localização para este site e confira se a localização do Windows está ativada.</p>}
                <a className="location-login" target="_blank" rel="noopener noreferrer"
                  href={`https://www.openstreetmap.org/?mlat=${location.latitude}&mlon=${location.longitude}#map=16/${location.latitude}/${location.longitude}`}>
                  <ExternalLink size={16} aria-hidden="true"/>Abrir no OpenStreetMap
                </a>
              </section>
            </div>
          </>
        ) : (
          <div className="location-details location-empty">
            <h1>{preview ? "Prévia da localização" : "Localização do colaborador"}</h1>
            {error ? <p role="alert">{error}</p> : <p role="status">Carregando localização…</p>}
            {expired && <a className="location-login" href="/login">Entrar no TIMEID</a>}
          </div>
        )}
      </main>
    </div>
  );
}
