import { House, Clock3, Users, Plus, List, Building2, BarChart3, Umbrella, CalendarDays, Settings, LogOut, ScanFace } from "lucide-react";
import logoTimeId from "../assets/logo-timeid-esmeralda.png";
const items = [
  ["Início", House, "inicio"], ["Ponto", Clock3, "ponto"],
  ["Funcionários", Users, "funcionarios"], ["Cadastrar", Plus, "cadastro"],
  ["Listar", List, "listar"], ["Departamentos", Building2, "departamentos"],
  ["Reconhecimento facial", ScanFace, "facial"], ["Relatórios", BarChart3, "relatorios"], ["Férias", Umbrella, "ferias"],
  ["Feriados", CalendarDays, "feriados"], ["Configurações", Settings, "configuracoes"],
];
export default function Sidebar({ pagina, aoNavegar, aoSair, busy, admin = true, facialAccess = false }) {
  return <aside className="menu-lateral">
    <h1 className="sidebar-logo"><img src={logoTimeId} alt="TIMEID — sua jornada, nosso controle" /></h1>
    <nav aria-label="Menu principal">{items.filter(([, , target]) => admin || target === "facial" && facialAccess || ["inicio", "ponto", "ferias", "feriados", "configuracoes"].includes(target)).map(([label, Icon, target]) => {
      const active = pagina === target;
      return <a key={label} href={`/${target}`} className={active ? "menu-ativo" : undefined} aria-current={active ? "page" : undefined} onClick={event => { event.preventDefault(); aoNavegar(target); }}><Icon className="icone-menu" size={22} aria-hidden="true" /><span>{label}</span></a>;
    })}</nav>
    <footer>TIMEID v1.0.0<button className="botao-sair" type="button" onClick={aoSair} disabled={busy}><LogOut size={18} aria-hidden="true" />{busy ? "Aguarde..." : "Sair"}</button></footer>
  </aside>;
}
