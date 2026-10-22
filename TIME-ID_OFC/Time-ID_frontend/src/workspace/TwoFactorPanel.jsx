import { useState } from "react";
import { ShieldCheck } from "lucide-react";
import { Modal, Field } from "./Ui";
import { api } from "../services/workspace";
export default function TwoFactorPanel({ enabled, refresh }) {
 const [open,setOpen]=useState(false),[password,setPassword]=useState(""),[code,setCode]=useState(""),[secret,setSecret]=useState(""),[codes,setCodes]=useState(null),[busy,setBusy]=useState(false),[error,setError]=useState("");
 function start(){setPassword("");setCode("");setSecret("");setCodes(null);setError("");setOpen(true);}
 async function submit(e){e.preventDefault();if(codes){setOpen(false);return;}setBusy(true);setError("");try{
 if(enabled&&!secret){await api("/two-factor/disable",{method:"POST",body:{password,code}});await refresh();setOpen(false);}
 else if(!secret){const result=await api("/two-factor/setup",{method:"POST",body:{password}});setSecret(result.secret);}
 else {const result=await api("/two-factor/enable",{method:"POST",body:{password,code}});setCodes(result.recoveryCodes);await refresh();}
 }catch(err){setError(err.message);}finally{setBusy(false);}}
 function saveCodes(){const url=URL.createObjectURL(new Blob(["TIMEID — códigos de recuperação (uso único)\n"+codes.join("\n")],{type:"text/plain;charset=utf-8"}));const a=document.createElement("a");a.href=url;a.download="timeid-codigos-recuperacao.txt";a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);}
 return <><button className="setting-row" onClick={start}><ShieldCheck/><span><strong>Autenticação em dois fatores</strong><small>{enabled?"Ativa · gerenciar proteção":"Desativada · proteger com um aplicativo autenticador"}</small></span></button>{open&&<Modal title={codes?"Guarde seus códigos de recuperação":enabled&&!secret?"Desativar dois fatores":"Ativar dois fatores"} busy={busy} error={error} onCancel={()=>setOpen(false)} onSubmit={submit} submitLabel={codes?"Concluir":enabled&&!secret?"Desativar":secret?"Ativar":"Continuar"}>
 {codes?<><p>Dois fatores ativados. Cada código abaixo substitui uma vez o código do aplicativo caso você perca o acesso. Eles não serão exibidos novamente.</p><pre className="recovery-codes">{codes.join("\n")}</pre><button className="btn secondary" type="button" onClick={saveCodes}>Baixar códigos</button></>:<><Field label="Senha atual" type="password" autoComplete="current-password" required value={password} onChange={e=>setPassword(e.target.value)}/>{secret&&<><p>Adicione uma conta no seu aplicativo autenticador usando esta chave. Selecione código baseado em tempo (TOTP), 6 dígitos, a cada 30 segundos.</p><code className="two-factor-key">{secret}</code></>}{(secret||enabled)&&<Field label={enabled&&!secret?"Código do aplicativo ou de recuperação":"Código de 6 dígitos"} autoComplete="one-time-code" required maxLength={64} value={code} onChange={e=>setCode(e.target.value)}/>}</>}
 </Modal>}</>;
}
