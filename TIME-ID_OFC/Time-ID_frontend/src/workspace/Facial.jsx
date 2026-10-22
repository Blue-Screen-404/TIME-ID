import { useCallback, useEffect, useRef, useState } from "react";
import { Camera, Check, CircleAlert, ScanFace, ShieldCheck, Trash2 } from "lucide-react";
import { Heading, Table } from "./Ui";
import { dateLabel, dayKey, timeLabel } from "../services/workspace";
import { enrollFace, facialProfiles, removeFace } from "../services/facial";
import "./Facial.css";

let faceapi, modelsReady;
function loadFacialModels() {
  if (!modelsReady) modelsReady = (async () => {
    faceapi = await import("@vladmandic/face-api");
    await Promise.all([
      faceapi.nets.tinyFaceDetector.loadFromUri("/models"),
      faceapi.nets.faceLandmark68TinyNet.loadFromUri("/models"),
      faceapi.nets.faceRecognitionNet.loadFromUri("/models"),
    ]);
  })();
  return modelsReady;
}

function CameraCapture({ onDescriptor, busy }) {
  const video = useRef(null), stream = useRef(null);
  const [error, setError] = useState(""), [loading, setLoading] = useState(true);
  useEffect(() => {
    let active = true;
    (async () => {
      try {
        await loadFacialModels();
        const media = await navigator.mediaDevices.getUserMedia({ video: { facingMode: "user", width: { ideal: 640 }, height: { ideal: 480 } }, audio: false });
        if (!active) { media.getTracks().forEach(track => track.stop()); return; }
        stream.current = media; video.current.srcObject = media; setLoading(false);
      } catch { if (active) { setError("Não foi possível abrir a câmera ou carregar o modelo local. Confira a permissão e tente novamente."); setLoading(false); } }
    })();
    return () => { active = false; stream.current?.getTracks().forEach(track => track.stop()); };
  }, []);
  async function capture() {
    setError("");
    try {
      const result = await faceapi.detectSingleFace(video.current, new faceapi.TinyFaceDetectorOptions({ inputSize: 320, scoreThreshold: 0.55 })).withFaceLandmarks(true).withFaceDescriptor();
      if (!result) throw new Error("Não encontramos um rosto nítido. Ajuste a iluminação e olhe para a câmera.");
      onDescriptor(Array.from(result.descriptor));
    } catch (e) { setError(e.message || "Falha na captura facial."); }
  }
  return <div className="face-camera"><video ref={video} autoPlay muted playsInline/><div className="face-camera-note"><Camera size={16}/>{loading ? "Preparando câmera e modelo local…" : "Centralize o rosto e mantenha a iluminação uniforme."}</div>{error && <p className="form-error" role="alert">{error}</p>}<button className="btn primary" disabled={loading || busy} onClick={capture}><ScanFace size={17}/> Capturar padrão facial</button></div>;
}

export function FacialAdmin({ data, message, canManage }) {
  const [profiles, setProfiles] = useState([]), [weeklyPunches, setWeeklyPunches] = useState([]), [selected, setSelected] = useState(""), [verified, setVerified] = useState(false), [descriptor, setDescriptor] = useState(null), [busy, setBusy] = useState(false), [error, setError] = useState("");
  const refresh = useCallback(() => facialProfiles().then(result=>{setProfiles(result.employees);setWeeklyPunches(result.weeklyPunches);}).catch(e => setError(e.message)), []);
  useEffect(() => { refresh(); }, [refresh]);
  const missing = canManage ? profiles.filter(p => !p.enrolled) : [], current = profiles.find(p => p.id === selected);
  async function save() {
    if (!descriptor || !current || !verified) return;
    setBusy(true); setError("");
    try { await enrollFace({ employeeId: current.id, descriptor, identityVerified: true }); setDescriptor(null); setVerified(false); setSelected(""); await refresh(); message("Padrão facial cadastrado com proteção local."); }
    catch (e) { setError(e.message); }
    finally { setBusy(false); }
  }
  async function remove(id, name) {
    if (!window.confirm(`Remover o padrão facial de ${name}?`)) return;
    setBusy(true); setError(""); try { await removeFace(id); await refresh(); message("Padrão facial removido."); } catch (e) { setError(e.message); } finally { setBusy(false); }
  }
  const byDay = new Map();
  for (const punch of weeklyPunches) {
    const key = `${punch.employeeId}:${dayKey(punch.at)}`;
    if (!byDay.has(key)) byDay.set(key, { employeeId: punch.employeeId, date: dayKey(punch.at), marks: {} });
    byDay.get(key).marks[punch.kind] = punch.at;
  }
  const dailyRows = [...byDay.values()].sort((a,b)=>b.date.localeCompare(a.date));
  return <><Heading title="Reconhecimento facial" subtitle="Cadastros biométricos e marcações feitas no terminal local da empresa."/>
    <div className="face-stats"><section className="work-card"><strong>{profiles.filter(p => p.enrolled).length}</strong><span>cadastros faciais ativos</span></section><section className="work-card"><strong>{weeklyPunches.length}</strong><span>marcações faciais nesta semana</span></section>{canManage&&<section className="work-card"><strong>{missing.length}</strong><span>colaboradores sem cadastro facial</span></section>}<a className="work-card terminal-link" href="/terminal" target="_blank" rel="noreferrer"><ScanFace size={24}/><strong>Abrir terminal local</strong><span>Captura e batida presencial</span></a></div>
    {canManage&&<section className="work-card"><div className="card-heading"><h3><ShieldCheck size={19}/>Cadastro de colaborador presencial ou híbrido</h3></div><p className="muted-text">Confirme a identidade com os dados do cadastro da empresa. O sistema processa a câmera localmente, envia somente o descritor matemático e não guarda fotografias.</p>
      {error && <p className="form-error" role="alert">{error}</p>}
      {missing.length ? <div className="form-grid"><label className="field"><span>Colaborador</span><select value={selected} onChange={e => { setSelected(e.target.value); setDescriptor(null); setVerified(false); }}><option value="">Selecione</option>{missing.map(p => <option key={p.id} value={p.id}>{p.name} · matrícula {p.registration}</option>)}</select></label>{current && <div className="face-identity"><strong>Confira com o cadastro oficial</strong><span>Nome: {current.name}</span><span>Idade: {current.age} anos</span><span>Matrícula: {current.registration}</span></div>}</div> : <p className="muted-text">Todos os colaboradores ativos já têm cadastro facial.</p>}
      {current && <><label className="face-check"><input type="checkbox" checked={verified} onChange={e => setVerified(e.target.checked)}/> Confirmo que conferi nome, idade e matrícula com os dados da empresa e que o colaborador foi informado sobre o cadastro biométrico.</label><CameraCapture onDescriptor={setDescriptor} busy={busy}/>{descriptor && <button className="btn primary" disabled={!verified || busy} onClick={save}><Check size={17}/> Salvar cadastro facial</button>}</>}
    </section>}
    <section className="work-card"><div className="card-heading"><h3>Relatório semanal · segunda a hoje</h3></div><Table columns={["Colaborador","Idade","Matrícula","Data","Entrada","Início intervalo","Retorno","Saída"]} rows={dailyRows} render={row=>{const person=profiles.find(e=>e.id===row.employeeId);return <tr key={`${row.employeeId}-${row.date}`}><td>{person?.name??"—"}</td><td>{person?.age??"—"}</td><td>{person?.registration??"—"}</td><td>{dateLabel(row.date)}</td>{["Entrada","Início do intervalo","Retorno do intervalo","Saída"].map(kind=><td key={kind}>{row.marks[kind]?timeLabel(row.marks[kind]):"—"}</td>)}</tr>}}/></section>
    <section className="work-card"><div className="card-heading"><h3>Cadastros faciais</h3></div><Table columns={["Colaborador","Idade","Matrícula","Cadastro",...(canManage?["Ações"]:[])]} rows={profiles.filter(p=>p.enrolled)} render={p=><tr key={p.id}><td>{p.name}</td><td>{p.age}</td><td>{p.registration}</td><td>{p.enrolledAt?dateLabel(p.enrolledAt.slice(0,10)):"—"}</td>{canManage&&<td><button className="icon-button danger" aria-label={`Remover cadastro facial de ${p.name}`} disabled={busy} onClick={()=>remove(p.id,p.name)}><Trash2 size={17}/></button></td>}</tr>}/></section>
    <p className="muted-text face-privacy"><CircleAlert size={16}/>Biometria é dado pessoal sensível. A empresa deve definir finalidade, prazo de retenção e alternativa para o colaborador antes de ativar a operação.</p>
  </>;
}

export function FacialTerminal() {
  const video = useRef(null), stream = useRef(null), running = useRef(false), lastPunch = useRef(0);
  const [status, setStatus] = useState("Preparando câmera local…"), [error, setError] = useState(""), [result, setResult] = useState(null), [busy, setBusy] = useState(true);
  useEffect(() => {
    let active = true, timer;
    (async () => {
      try {
        await loadFacialModels();
        const media = await navigator.mediaDevices.getUserMedia({ video: { facingMode: "user", width: { ideal: 640 }, height: { ideal: 480 } }, audio: false });
        if (!active) { media.getTracks().forEach(track=>track.stop()); return; }
        stream.current = media; video.current.srcObject = media; await video.current.play(); setBusy(false); setStatus("Aproxime-se e olhe para a câmera."); running.current = true;
        const scan = async () => {
          if (!active || !running.current || Date.now() - lastPunch.current < 25000) return;
          try {
            const found = await faceapi.detectSingleFace(video.current, new faceapi.TinyFaceDetectorOptions({ inputSize: 320, scoreThreshold: 0.65 })).withFaceLandmarks(true).withFaceDescriptor();
            if (found) {
              running.current = false; setStatus("Verificando identidade localmente…");
              const punch = await (await fetch("/api/facial/terminal/punch", { method: "POST", credentials: "same-origin", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ descriptor: Array.from(found.descriptor) }) })).json();
              if (!punch.employee) throw new Error(punch.title || "Rosto não reconhecido.");
              setResult(punch); setError(""); lastPunch.current = Date.now(); setStatus("Ponto registrado.");
              window.setTimeout(()=>{ if (active) { setResult(null); const wait = Math.max(0, 30000 - (Date.now() - lastPunch.current)); setStatus(wait ? "Registro concluído. Aguarde antes de uma nova marcação." : "Aproxime-se e olhe para a câmera."); window.setTimeout(()=>{if(active){setStatus("Aproxime-se e olhe para a câmera.");running.current=true;scan();}}, wait); } }, 5000);
            }
          } catch (e) { setError(e.message || "Não foi possível registrar o ponto."); setStatus("Não foi possível concluir. Tente novamente ou registre pelo app."); running.current = false; if ((e.message||"").includes("jornada de hoje já foi encerrada")) return; window.setTimeout(()=>{ if(active){running.current=true;scan();} }, 4500); }
          if (active) timer = window.setTimeout(scan, 900);
        };
        scan();
      } catch { if (active) { setError("Câmera ou modelo facial indisponível. Use o registro pelo app e avise o gestor."); setStatus("Terminal facial indisponível."); setBusy(false); } }
    })();
    return () => { active = false; running.current = false; clearTimeout(timer); stream.current?.getTracks().forEach(track=>track.stop()); };
  }, []);
  return <main className="facial-terminal"><header><span>◷ TIME<strong>ID</strong></span><a href="/login">Acesso do gestor</a></header><section className="terminal-card"><h1>Registro de ponto presencial</h1><p>{status}</p><div className="terminal-camera"><video ref={video} autoPlay muted playsInline aria-label="Câmera de reconhecimento facial"/>{!busy&&<div className="face-guide"/>}</div>{result&&<div className="terminal-success"><Check size={22}/><strong>{result.employee}</strong><span>{result.kind} · {timeLabel(result.at)}</span></div>}{error&&<p role="alert" className="form-error">{error}</p>}<small>Seu rosto é comparado neste computador. Nenhuma imagem é salva. Se a câmera não funcionar, registre pelo app ou peça ajuda ao gestor.</small></section></main>;
}
