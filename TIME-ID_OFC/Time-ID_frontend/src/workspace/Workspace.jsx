import { useCallback, useEffect, useState } from "react";
import { Bell, Search, X, House } from "lucide-react";
import Sidebar from "../components/Sidebar";
import ProfileMenu from "../components/ProfileMenu";
import Dashboard from "./Dashboard";
import Employees from "./Employees";
import EmployeeForm from "./EmployeeForm";
import { Departments, Vacations, Holidays } from "./Records";
import Attendance from "./Attendance";
import Reports from "./Reports";
import Settings from "./Settings";
import { Field, Modal } from "./Ui";
import { api } from "../services/workspace";
import "../pages/Inicio.css";
import "./Workspace.css";
const departmentExamples = {
 "Recursos Humanos": "Recrutamento, admissão, benefícios e apoio aos colaboradores.",
 "Financeiro": "Contas a pagar e receber, orçamento e acompanhamento financeiro.",
 "Tecnologia da Informação": "Desenvolvimento de sistemas, suporte técnico e infraestrutura.",
 "Marketing": "Campanhas, comunicação, conteúdo e divulgação da empresa.",
 "Comercial": "Vendas, relacionamento com clientes e acompanhamento de propostas.",
 "Administrativo": "Organização de documentos e apoio às rotinas da empresa.",
 "Logística": "Estoque, transporte, distribuição e acompanhamento de entregas.",
 "Produção": "Planejamento e execução da produção e controle de qualidade.",
 "Atendimento ao Cliente": "Atendimento de solicitações, dúvidas e suporte aos clientes."
};
const labels={inicio:"Início",ponto:"Ponto",funcionarios:"Funcionários",cadastro:"Cadastrar",listar:"Listar",departamentos:"Departamentos",ferias:"Férias",feriados:"Feriados",relatorios:"Relatórios",configuracoes:"Configurações"};
export default function Workspace({ user, onUpdateProfile, paginaAtual, aoNavegar, aoSair, busy:authBusy }) {
 const page=paginaAtual.replace(/^\//,"")||"inicio", admin=user.role==="Administrador";
 const [data,setData]=useState(null),[loadError,setLoadError]=useState(""),[busy,setBusy]=useState(false),[query,setQuery]=useState(""),[employee,setEmployee]=useState(null),[editor,setEditor]=useState(null),[error,setError]=useState(""),[notice,setNotice]=useState(null),[notifications,setNotifications]=useState(false);
 const refresh=useCallback(async()=>{const next=await api();setData(next);setLoadError("");},[]);
 useEffect(()=>{let active=true;api().then(next=>{if(active)setData(next);}).catch(err=>{if(active)setLoadError(err.message);});return()=>{active=false;};},[user.id]);
 useEffect(()=>{document.title=`${labels[page]??"Início"} — TIMEID`;},[page]);
 useEffect(()=>{if(data)document.documentElement.dataset.theme=data.preferences.theme;return()=>{delete document.documentElement.dataset.theme;};},[data?.preferences.theme]);
 function message(text,isError=false){setNotice({text,isError});}
 function navigate(target){setError("");setNotifications(false);if(target==="cadastro")setEmployee(null);aoNavegar(target);}
 async function mutate(path,method,body){setBusy(true);try{const result=await api(path,{method,body});await refresh();return result;}finally{setBusy(false);}}
 function editStaff(value){setEmployee(value);setError("");aoNavegar("cadastro");}
 async function saveStaff(payload){setError("");try{await mutate(`/employees${employee?`/${employee.id}`:""}`,employee?"PUT":"POST",payload);setEmployee(null);navigate("listar");message("Funcionário salvo com sucesso.");}catch(err){setError(err.message);}}
 function edit(type,value){setError("");const defaults={departments:{name:"",description:"",leaderId:""},vacations:{employeeId:data.employeeId??"",start:data.today,end:data.today,status:"Solicitada",notes:""},holidays:{name:"",date:data.today,scope:"Nacional"},settings:data.settings,password:{currentPassword:"",newPassword:"",confirm:""}};
 let draft={...(defaults[type]??{}),...value};
 if(type==="punches")draft={at:new Date(new Date(value.at).getTime()-3*3600000).toISOString().slice(0,16),reason:""};
 setEditor({type,id:value?.id,draft});}
 function remove(type,id,name){setError("");setEditor({type,id,deleting:true,name,draft:{reason:""}});}
 function change(key,value){setEditor(current=>({...current,draft:{...current.draft,[key]:value}}));}
 async function saveEditor(event){event.preventDefault();setError("");const {type,id,deleting,draft}=editor;
 try{
 if(deleting){await mutate(`/${type}/${id}${type==="punches"?`?reason=${encodeURIComponent(draft.reason)}`:""}`,"DELETE");}
 else{
 let body={...draft}; delete body.id;
 if(type==="departments")body.leaderId=body.leaderId||null;
 if(type==="punches")body.at=new Date(`${draft.at}:00-03:00`).toISOString();
 if(type==="password"){if(body.newPassword!==body.confirm)throw new Error("As senhas não coincidem.");delete body.confirm;setBusy(true);await api("/password",{method:"PUT",body});setEditor(null);window.dispatchEvent(new Event("timeid:expired"));return;}
 await mutate(`/${type}${id?`/${id}`:""}`,id||type==="settings"?"PUT":"POST",body);
 }
 setEditor(null);message(deleting?"Registro excluído.":"Alterações salvas.");
 }catch(err){setError(err.message);}finally{setBusy(false);}}
 async function punch(employeeId){try{await mutate("/punches","POST",{employeeId});message("Ponto registrado com o horário do servidor.");}catch(err){message(err.message,true);}}
 async function preferences(value){try{await mutate("/preferences","PUT",value);}catch(err){message(err.message,true);}}
 if(!data)return <main className="loading-panel"><h2>TIMEID</h2><p role={loadError?"alert":"status"}>{loadError||"Carregando seus dados..."}</p>{loadError&&<button className="btn primary" onClick={()=>refresh().catch(err=>setLoadError(err.message))}>Tentar novamente</button>}</main>;
 const restricted=!admin&&["funcionarios","cadastro","listar","departamentos","relatorios"].includes(page);
 const pending=data.vacations.filter(v=>v.status==="Solicitada");
 const field=(label,key,props={})=><Field label={label} value={editor.draft[key]??""} onChange={e=>change(key,e.target.value)} {...props}/>;
 const selectedEmployee=editor?.draft.employeeId;
 const leaveStart=editor?.type === "vacations" ? editor.draft.start : "";
 const leaveEnd=editor?.type === "vacations" ? editor.draft.end : "";
 const leaveDays=leaveStart && leaveEnd ? Math.round((Date.parse(leaveEnd)-Date.parse(leaveStart))/86400000)+1 : 0;
 const leaveMax=leaveStart && Number.isFinite(Date.parse(leaveStart)) ? new Date(Date.parse(leaveStart)+13*86400000).toISOString().slice(0,10) : undefined;
 const employeeSelect=<Field label="Funcionário *"><select required value={selectedEmployee??""} onChange={e=>change("employeeId",e.target.value)}><option value="">Selecione</option>{data.employees.filter(e=>e.active||e.id===selectedEmployee).map(e=><option key={e.id} value={e.id}>{e.name}</option>)}</select></Field>;
 return <main className="dashboard work-app"><Sidebar pagina={page} aoNavegar={navigate} aoSair={aoSair} busy={authBusy} admin={admin}/><section className="workspace-main"><header className="workspace-header"><strong className="page-label"><House size={18}/>{labels[page]??"Início"}</strong>{admin&&<form className="global-search" onSubmit={e=>{e.preventDefault();navigate("funcionarios");}}><Search size={17}/><input aria-label="Busca geral" placeholder="Buscar funcionário, departamento..." value={query} onChange={e=>setQuery(e.target.value)}/></form>}<div className="header-actions">{data.preferences.notifications&&<div className="notifications"><button className="icon-button" aria-label="Notificações" aria-expanded={notifications} onClick={()=>setNotifications(v=>!v)}><Bell size={20}/>{pending.length>0&&<i/>}</button>{notifications&&<div className="notification-panel"><strong>Notificações</strong><button onClick={()=>navigate("ferias")}>{pending.length} solicitações de férias pendentes</button><p>{data.stats.birthdays} aniversariantes hoje</p></div>}</div>}<ProfileMenu user={user} onUpdate={updated=>{onUpdateProfile(updated);refresh().catch(err=>message(err.message,true));}}/></div></header>
 <div className="workspace-content">{notice&&<div className={`notice ${notice.isError?"error":""}`} role={notice.isError?"alert":"status"}>{notice.text}<button aria-label="Fechar mensagem" onClick={()=>setNotice(null)}><X size={16}/></button></div>}
 {restricted?<section className="work-card"><h2>Acesso restrito</h2><p>Esta área é exclusiva de administradores.</p></section>:<>
 {page==="inicio"&&<Dashboard data={data} user={user} navigate={navigate}/>}
 {["funcionarios","listar"].includes(page)&&<Employees data={data} query={query} setQuery={setQuery} navigate={navigate} edit={editStaff} remove={remove} list={page==="listar"}/>}
 {page==="cadastro"&&<EmployeeForm key={employee?.id??"new"} data={data} employee={employee} save={saveStaff} createDepartment={()=>edit("departments")} cancel={()=>navigate("listar")} busy={busy} error={error}/>}
 {page==="departamentos"&&<Departments data={data} edit={edit} remove={remove}/>}
 {page==="ponto"&&<Attendance data={data} admin={admin} edit={edit} remove={remove} punch={punch} busy={busy}/>}
 {page==="ferias"&&<Vacations data={data} admin={admin} edit={edit} remove={remove}/>}
 {page==="feriados"&&<Holidays data={data} admin={admin} edit={edit} remove={remove}/>}
 {page==="relatorios"&&<Reports data={data}/>}
 {page==="configuracoes"&&<Settings refresh={refresh} data={data} admin={admin} edit={edit} preferences={preferences} message={message}/>}
 {!labels[page]&&<p>Página não encontrada.</p>}
 </>}
 </div></section>
 {editor&&<Modal title={editor.deleting?"Confirmar exclusão":({departments:editor.id?"Editar departamento":"Novo departamento",vacations:editor.id?"Editar férias":"Solicitar férias",holidays:editor.id?"Editar feriado":"Novo feriado",punches:"Corrigir marcação",settings:"Dados da empresa",password:"Alterar senha"})[editor.type]} busy={busy} error={error} onCancel={()=>setEditor(null)} onSubmit={saveEditor} submitLabel={editor.deleting?"Excluir":"Salvar"}>
 {editor.deleting?<><p>Deseja excluir <strong>{editor.name}</strong>?</p><p className="muted-text">Esta ação remove o registro. Funcionários com histórico devem ser inativados.</p>{editor.type==="punches"&&field("Justificativa *","reason",{required:true,minLength:5,maxLength:500})}</>:<>
 {editor.type==="departments"&&<><Field label="Exemplos de departamentos"><select value="" onChange={e=>{const name=e.target.value;if(name)setEditor(current=>({...current,draft:{...current.draft,name,description:departmentExamples[name]}}));}}><option value="">Escolha um exemplo (opcional)</option>{Object.keys(departmentExamples).map(name=><option key={name} value={name}>{name}</option>)}</select></Field><p className="muted-text">Escolha um exemplo para preencher nome e descrição, ou escreva seus próprios dados abaixo. O departamento será gravado ao salvar.</p>{field("Nome *","name",{required:true,minLength:2,maxLength:100})}{field("Descrição","description",{maxLength:400})}<Field label="Líder"><select value={editor.draft.leaderId??""} onChange={e=>change("leaderId",e.target.value)}><option value="">Não definido</option>{data.employees.filter(e=>e.active&&e.departmentId===editor.id).map(e=><option key={e.id} value={e.id}>{e.name}</option>)}</select></Field></>}
 {editor.type==="vacations"&&<>{employeeSelect}<div className="form-grid">{field("Início *","start",{type:"date",required:true})}{field("Fim *","end",{type:"date",required:true,min:editor.draft.start,max:leaveMax})}</div><p className="muted-text">Limite demonstrativo: 14 dias corridos por solicitação, contando o primeiro e o último dia.{leaveDays > 0 ? ` Período selecionado: ${leaveDays} dia(s).` : ""}</p>{leaveDays > 14 && <p className="form-error" role="alert">Reduza o período para até 14 dias.</p>}{admin&&<Field label="Status"><select value={editor.draft.status} onChange={e=>change("status",e.target.value)}>{["Solicitada","Aprovada","Recusada"].map(s=><option key={s}>{s}</option>)}</select></Field>}{field("Observações","notes",{maxLength:500})}</>}
 {editor.type==="holidays"&&<>{field("Nome *","name",{required:true,maxLength:120})}{field("Data *","date",{type:"date",required:true})}<Field label="Abrangência"><select value={editor.draft.scope} onChange={e=>change("scope",e.target.value)}>{["Nacional","Estadual","Municipal","Empresa"].map(s=><option key={s}>{s}</option>)}</select></Field></>}
 {editor.type==="punches"&&<>{field("Horário corrigido (Brasília) *","at",{type:"datetime-local",required:true})}{field("Justificativa *","reason",{required:true,minLength:5,maxLength:500})}<p className="muted-text">A data original e a ordem das marcações devem ser preservadas.</p></>}
 {editor.type==="settings"&&<>{field("Nome da empresa *","name",{required:true,maxLength:120})}{field("CNPJ","cnpj",{maxLength:18})}{field("E-mail *","email",{type:"email",required:true,maxLength:254})}{field("Telefone","phone",{maxLength:30})}{field("Endereço","address",{maxLength:300})}</>}
 {editor.type==="password"&&<>{field("Senha atual *","currentPassword",{type:"password",required:true,autoComplete:"current-password"})}{field("Nova senha *","newPassword",{type:"password",required:true,minLength:8,maxLength:128,autoComplete:"new-password"})}{field("Confirme a nova senha *","confirm",{type:"password",required:true,minLength:8,maxLength:128,autoComplete:"new-password"})}<p className="muted-text">Depois de salvar, entre novamente com a nova senha.</p></>}
 </>}
 </Modal>}
 </main>;
}
