import { useCallback, useEffect, useRef, useState } from "react";
import { AlertTriangle, Camera, CircleStop, Plus, Radio, Trash2, Video } from "lucide-react";
import { Heading } from "./Ui";
import { addCameraSource, cameraMonitorData, monitorFace, removeCameraSource, saveCameraAlertPhoto } from "../services/facial";
import "./CameraMonitor.css";

let faceapi;
let modelsReady;
function loadModels() {
  if (!modelsReady) modelsReady = (async () => {
    faceapi = await import("@vladmandic/face-api");
    await Promise.all([
      faceapi.nets.tinyFaceDetector.loadFromUri("/models"),
      faceapi.nets.faceLandmark68TinyNet.loadFromUri("/models"),
      faceapi.nets.faceRecognitionNet.loadFromUri("/models"),
    ]);
  })().catch(error => { modelsReady = null; throw error; });
  return modelsReady;
}

const timestamp = value => new Date(value).toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" });

export default function CameraMonitor() {
  const videoRef = useRef(null), streamRef = useRef(null), activeRef = useRef(false), scanTimerRef = useRef(null), lastAnalysisRef = useRef(0);
  const [monitoring, setMonitoring] = useState(false), [status, setStatus] = useState("Monitoramento desligado."), [error, setError] = useState("");
  const [cameraData, setCameraData] = useState({ sources: [], alerts: [] }), [detections, setDetections] = useState([]);
  const [draft, setDraft] = useState({ name: "", protocol: "RTSP", endpoint: "", username: "", password: "" });
  const [saving, setSaving] = useState(false);

  const refresh = useCallback(async () => {
    try { setCameraData(await cameraMonitorData()); }
    catch (requestError) { setError(requestError.message); }
  }, []);

  useEffect(() => {
    refresh();
    const interval = window.setInterval(refresh, 15000);
    return () => window.clearInterval(interval);
  }, [refresh]);

  const stop = useCallback(() => {
    activeRef.current = false;
    window.clearTimeout(scanTimerRef.current);
    streamRef.current?.getTracks().forEach(track => track.stop());
    streamRef.current = null;
    setMonitoring(false);
    setStatus("Monitoramento desligado.");
  }, []);

  useEffect(() => () => {
    activeRef.current = false;
    window.clearTimeout(scanTimerRef.current);
    streamRef.current?.getTracks().forEach(track => track.stop());
  }, []);

  async function start() {
    setError(""); setStatus("Preparando a webcam e os modelos faciais...");
    try {
      await loadModels();
      const stream = await navigator.mediaDevices.getUserMedia({ video: { width: { ideal: 960 }, height: { ideal: 540 } }, audio: false });
      streamRef.current = stream;
      videoRef.current.srcObject = stream;
      await videoRef.current.play();
      activeRef.current = true;
      setMonitoring(true);
      setStatus("Câmera ativa. Aguardando rostos...");

      const scan = async () => {
        if (!activeRef.current) return;
        try {
          if (Date.now() - lastAnalysisRef.current >= 2500 && videoRef.current?.readyState >= 2) {
            lastAnalysisRef.current = Date.now();
            const faces = await faceapi.detectAllFaces(videoRef.current, new faceapi.TinyFaceDetectorOptions({ inputSize: 320, scoreThreshold: 0.62 }))
              .withFaceLandmarks(true).withFaceDescriptors();
            if (!activeRef.current) return;
            if (faces.length === 0) {
              setDetections([]);
              setStatus("Câmera ativa. Nenhum rosto no enquadramento.");
            } else {
              const results = [];
              for (const face of faces.slice(0, 4)) {
                if (!activeRef.current) return;
                const result = await monitorFace(Array.from(face.descriptor));
                results.push(result);
                if (!result.matched && result.alertId && !result.hasPhoto) {
                  const video = videoRef.current;
                  const canvas = document.createElement("canvas");
                  const scale = Math.min(1, 640 / video.videoWidth, 360 / video.videoHeight);
                  canvas.width = Math.max(1, Math.round(video.videoWidth * scale));
                  canvas.height = Math.max(1, Math.round(video.videoHeight * scale));
                  canvas.getContext("2d").drawImage(video, 0, 0, canvas.width, canvas.height);
                  await saveCameraAlertPhoto(result.alertId, canvas.toDataURL("image/jpeg", 0.72));
                  result.hasPhoto = true;
                }
              }
              setDetections(results);
              const known = results.filter(result => result.matched).length;
              const unknown = results.length - known;
              setStatus(unknown > 0
                ? `${unknown} rosto(s) sem correspondência. Alerta registrado ou agrupado na lista abaixo.`
                : `${known} funcionário(s) reconhecido(s).`);
              if (results.some(result => result.alertCreated)) await refresh();
            }
          }
        } catch (scanError) {
          setError(scanError.message || "Não foi possível analisar a imagem da webcam.");
          setStatus("A câmera continua ligada, mas a análise falhou.");
        }
        if (activeRef.current) scanTimerRef.current = window.setTimeout(scan, 500);
      };
      scan();
    } catch (startError) {
      streamRef.current?.getTracks().forEach(track => track.stop());
      streamRef.current = null;
      setMonitoring(false);
      setStatus("Não foi possível iniciar a webcam.");
      setError(startError.message || "Permita o acesso à câmera e tente novamente.");
    }
  }

  async function saveSource(event) {
    event.preventDefault(); setSaving(true); setError("");
    try {
      await addCameraSource(draft);
      setDraft({ name: "", protocol: "RTSP", endpoint: "", username: "", password: "" });
      await refresh();
    } catch (saveError) { setError(saveError.message); }
    finally { setSaving(false); }
  }

  async function deleteSource(source) {
    if (!window.confirm(`Remover o cadastro da câmera “${source.name}”?`)) return;
    setError("");
    try { await removeCameraSource(source.id); await refresh(); }
    catch (deleteError) { setError(deleteError.message); }
  }

  return <main className="camera-monitor-page">
    <Heading title="Câmeras" subtitle="Veja a webcam local, confira correspondências e acompanhe alertas." />
    {error && <p className="form-error camera-monitor-error" role="alert">{error}</p>}
    <section className="camera-monitor-grid">
      <article className="work-card camera-live-card">
        <div className="camera-card-heading"><div><h2><Camera size={19}/> Webcam local</h2><p className="muted-text">A análise usa os padrões faciais cadastrados no TIME-ID.</p></div><span className={`camera-live-pill ${monitoring ? "active" : ""}`}><i/>{monitoring ? "Ao vivo" : "Desligada"}</span></div>
        <div className="camera-preview"><video ref={videoRef} autoPlay muted playsInline aria-label="Visualização da webcam local"/>{!monitoring && <div className="camera-preview-placeholder"><Video size={38}/><span>A webcam só será ativada quando você iniciar o monitoramento.</span></div>}</div>
        <div className="camera-monitor-controls"><span role="status">{status}</span>{monitoring
          ? <button className="btn danger" type="button" onClick={stop}><CircleStop size={17}/> Desligar câmera</button>
          : <button className="btn primary" type="button" onClick={start}><Radio size={17}/> Iniciar monitoramento</button>}</div>
        {detections.length > 0 && <div className="camera-detections" aria-live="polite">{detections.map((item, index) => <span className={item.matched ? "recognized" : "unmatched"} key={`${index}-${item.employee ?? "unknown"}`}>{item.matched ? item.employee : "Sem correspondência"}</span>)}</div>}
        <p className="muted-text camera-privacy-note">A imagem é analisada no navegador. Quando não há correspondência, uma foto reduzida é salva cifrada junto ao alerta para conferência do administrador.</p>
      </article>

      <article className="work-card camera-sources-card">
        <div className="camera-card-heading"><div><h2><Video size={19}/> Câmeras reais para teste futuro</h2><p className="muted-text">Cadastre RTSP ou ONVIF para preparar a integração.</p></div></div>
        <p className="camera-future-note"><AlertTriangle size={17}/> Estes cadastros ainda não reproduzem o vídeo. A conexão ao fluxo real será ativada quando o equipamento e o método de transmissão forem definidos.</p>
        <form className="camera-source-form" onSubmit={saveSource}>
          <label className="field"><span>Nome da câmera *</span><input required minLength={2} maxLength={80} value={draft.name} onChange={event => setDraft(current => ({ ...current, name: event.target.value }))} placeholder="Ex.: Entrada principal"/></label>
          <label className="field"><span>Tipo de conexão</span><select value={draft.protocol} onChange={event => setDraft(current => ({ ...current, protocol: event.target.value }))}><option value="RTSP">RTSP</option><option value="ONVIF">ONVIF</option></select></label>
          <label className="field camera-endpoint-field"><span>{draft.protocol === "RTSP" ? "Endereço RTSP *" : "Endereço ONVIF (HTTP/HTTPS) *"}</span><input required maxLength={500} value={draft.endpoint} onChange={event => setDraft(current => ({ ...current, endpoint: event.target.value }))} placeholder={draft.protocol === "RTSP" ? "rtsp://192.168.1.20:554/stream1" : "http://192.168.1.20/onvif/device_service"}/></label>
          <div className="camera-credentials"><label className="field"><span>Usuário (opcional)</span><input autoComplete="off" maxLength={100} value={draft.username} onChange={event => setDraft(current => ({ ...current, username: event.target.value }))}/></label><label className="field"><span>Senha (opcional)</span><input type="password" autoComplete="new-password" maxLength={200} value={draft.password} onChange={event => setDraft(current => ({ ...current, password: event.target.value }))}/></label></div>
          <button className="btn primary" type="submit" disabled={saving}><Plus size={17}/> {saving ? "Salvando…" : "Salvar câmera para teste futuro"}</button>
        </form>
        {cameraData.sources.length > 0 && <ul className="camera-source-list">{cameraData.sources.map(source => <li key={source.id}><span><strong>{source.name}</strong><small>{source.protocol} · Reprodução ainda não ativada</small></span><button className="icon-button danger" type="button" aria-label={`Remover ${source.name}`} onClick={() => deleteSource(source)}><Trash2 size={17}/></button></li>)}</ul>}
      </article>
    </section>

    <section className="work-card camera-alerts-card">
      <div className="camera-card-heading"><div><h2><AlertTriangle size={19}/> Pessoas sem correspondência</h2><p className="muted-text">Alertas repetidos da webcam são agrupados por um minuto.</p></div><span className="camera-alert-count">{cameraData.alerts.length}</span></div>
      {cameraData.alerts.length > 0 && <div className="camera-unknown-banner"><AlertTriangle size={20}/><span><strong>PESSOA NÃO CADASTRADA</strong><small>O rosto não encontrou um cadastro facial correspondente. Confira a foto: iluminação ou ângulo podem causar falhas no reconhecimento.</small></span></div>}
      {cameraData.alerts.length === 0 ? <p className="muted-text">Nenhuma ocorrência para exibir.</p> : <div className="camera-alert-table-wrap"><table className="camera-alert-table"><thead><tr><th>Foto</th><th>Situação</th><th>Câmera</th><th>Data e hora</th></tr></thead><tbody>{cameraData.alerts.map(alert => <tr key={alert.id}><td>{alert.hasPhoto ? <img className="camera-alert-photo" src={`/api/facial/alerts/${alert.id}/photo`} alt="Foto registrada no alerta" loading="lazy"/> : <span className="camera-alert-photo-placeholder">Foto pendente</span>}</td><td><span className="camera-unknown-badge">PESSOA NÃO CADASTRADA</span></td><td>{alert.cameraName}</td><td><time dateTime={alert.at}>{timestamp(alert.at)}</time></td></tr>)}</tbody></table></div>}
    </section>
  </main>;
}
