import { useEffect, useRef, useState } from "react";
import { confirmPasswordReset, getCurrentUser, login, logout, requestPasswordReset, verifyPasswordResetCode } from "./services/auth";
import { LockKeyhole, Mail, ShieldCheck, UsersRound } from "lucide-react";
import pessoasLogin from "./assets/pessoas-login.png";
import "./App.css";
import Workspace from "./workspace/Workspace";
import { FacialTerminal } from "./workspace/Facial";

function App() {
  const [paginaAtual, setPaginaAtual] = useState(window.location.pathname);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [user, setUser] = useState(null);
  const [busy, setBusy] = useState(false);
  const [checking, setChecking] = useState(true);
  const [error, setError] = useState("");
  const [telaAcesso, setTelaAcesso] = useState("login");
  const [codigoRecuperacao, setCodigoRecuperacao] = useState(["", "", "", "", "", ""]);
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [mensagemRecuperacao, setMensagemRecuperacao] = useState("");
  const inputsCodigo = useRef([]);

  function navegar(para, replace = false) {
    const path = `/${para}`;
    window.history[replace ? "replaceState" : "pushState"]({}, "", path);
    setPaginaAtual(path);
  }

  useEffect(() => {
    let active = true;
    async function checkSession() {
      if (window.location.pathname === "/terminal") { if (active) setChecking(false); return; }
      try {
        const current = await getCurrentUser();
        if (!active) return;
        setUser(current);
        if (!current) navegar("login", true);
        else if (["/", "/login"].includes(window.location.pathname)) navegar("inicio", true);
      } catch (err) {
        if (active) { setUser(null); setError(err.message); navegar("login", true); }
      } finally { if (active) setChecking(false); }
    }
    function onHistory() { setPaginaAtual(window.location.pathname); checkSession(); }
    function onVisible() { if (document.visibilityState === "visible") checkSession(); }
    function onExpired() { setUser(null); setPassword(""); navegar("login", true); }
    window.addEventListener("timeid:expired", onExpired);
    checkSession();
    const interval = window.setInterval(checkSession, 60000);
    window.addEventListener("popstate", onHistory);
    window.addEventListener("pageshow", onHistory);
    document.addEventListener("visibilitychange", onVisible);
    return () => {
      active = false;
      window.removeEventListener("timeid:expired", onExpired);
      window.clearInterval(interval);
      window.removeEventListener("popstate", onHistory);
      window.removeEventListener("pageshow", onHistory);
      document.removeEventListener("visibilitychange", onVisible);
    };
  }, []);

  async function handleLogin(event) {
    event.preventDefault(); setBusy(true); setError("");
    try { setUser(await login(email, password)); setPassword(""); navegar("inicio", true); }
    catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }
  async function handleLogout() {
    setBusy(true); setError("");
    try { await logout(); setUser(null); setPassword(""); navegar("login", true); }
    catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }

  function abrirRecuperacao() {
    setError("");
    setMensagemRecuperacao("");
    setCodigoRecuperacao(["", "", "", "", "", ""]);
    setTelaAcesso("codigo");
  }

  function voltarParaLogin() {
    setError("");
    setMensagemRecuperacao("");
    setTelaAcesso("login");
  }

  async function enviarCodigoRecuperacao() {
    if (!email.includes("@")) {
      setError("Informe um e-mail válido para receber o código.");
      return;
    }
    setBusy(true); setError("");
    try {
      await requestPasswordReset(email);
      setMensagemRecuperacao("Se este e-mail estiver cadastrado, o código foi enviado.");
      window.setTimeout(() => inputsCodigo.current[0]?.focus(), 0);
    } catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }

  function alterarCodigo(valor, indice) {
    const digitos = valor.replace(/\D/g, "").slice(0, 6 - indice);
    const proximoCodigo = [...codigoRecuperacao];
    digitos.split("").forEach((digito, posicao) => { proximoCodigo[indice + posicao] = digito; });
    if (!digitos) proximoCodigo[indice] = "";
    setCodigoRecuperacao(proximoCodigo);

    const proximoIndice = Math.min(indice + Math.max(digitos.length, 1), 5);
    if (digitos) window.setTimeout(() => inputsCodigo.current[proximoIndice]?.focus(), 0);
  }

  function colarCodigo(evento) {
    evento.preventDefault();
    const digitos = evento.clipboardData.getData("text").replace(/\D/g, "").slice(0, 6).split("");
    if (!digitos.length) return;
    setCodigoRecuperacao([...digitos, ...Array(6 - digitos.length).fill("")]);
    window.setTimeout(() => inputsCodigo.current[Math.min(digitos.length, 5)]?.focus(), 0);
  }

  async function validarCodigo(evento) {
    evento.preventDefault();
    if (codigoRecuperacao.some((digito) => !digito)) {
      setError("Preencha os seis dígitos do código.");
      return;
    }
    setBusy(true); setError("");
    try {
      await verifyPasswordResetCode(email, codigoRecuperacao.join(""));
      setTelaAcesso("nova-senha");
    } catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }

  async function redefinirSenha(evento) {
    evento.preventDefault();
    if (novaSenha.length < 8) { setError("A nova senha precisa ter pelo menos 8 caracteres."); return; }
    if (novaSenha !== confirmarSenha) { setError("As senhas não coincidem."); return; }
    setBusy(true); setError("");
    try {
      await confirmPasswordReset(email, codigoRecuperacao.join(""), novaSenha);
      setPassword(""); setNovaSenha(""); setConfirmarSenha("");
      setMensagemRecuperacao("Senha redefinida com sucesso. Entre com a nova senha.");
      setTelaAcesso("login");
    } catch (err) { setError(err.message); }
    finally { setBusy(false); }
  }
  if (window.location.pathname === "/terminal") return <FacialTerminal/>;
  if (checking) return <main className="carregando-sessao" role="status">Verificando sessão...</main>;
  if (user) {
    return <>{error && <p className="erro-sessao" role="alert">{error}</p>}<Workspace paginaAtual={paginaAtual} onUpdateProfile={setUser} user={user} aoNavegar={navegar} aoSair={handleLogout} busy={busy} /></>;
  }
  return (
    <main className="pagina-login">
      <section className="lado-esquerdo">
        <div className="marca">
          <span className="icone-logo" aria-hidden="true">◷</span>

          <h1>
            TIME<strong>ID</strong>
          </h1>
        </div>

        <div className="apresentacao">
          <h2>
            Mais controle,
            <br />
            Mais tempo para o
            <br />
            <strong>que importa.</strong>
          </h2>

          <p>
            Sistema de ponto e <strong>gestão de pessoas</strong>
            <br />
            para empresas de <strong>todos os tamanhos.</strong>
          </p>
        </div>

        <div className="ilustracao-login">
          <div className="relogio-destaque" aria-label="Relógio decorativo animado">
            <span className="ponteiro ponteiro-horas" />
            <span className="ponteiro ponteiro-minutos" />
            <span className="miolo-relogio" />
          </div>

          <img
            className="pessoas-login"
            src={pessoasLogin}
            alt="Três colaboradores do TIMEID"
          />
          <span className="faixa-ilustracao" aria-hidden="true" />
        </div>

        <div className="beneficios-login">
          <div className="beneficio-login">
            <ShieldCheck size={30} aria-hidden="true" />
            <p><strong>Segurança e confiabilidade</strong><span>Seus dados protegidos em todas as etapas.</span></p>
          </div>
          <div className="beneficio-login">
            <UsersRound size={30} aria-hidden="true" />
            <p><strong>Mais de 1M+ de usuários</strong><span>Em empresas de todo o Brasil</span></p>
          </div>
        </div>
      </section>

      <section className="lado-direito">
        <div className="card-login" id="form-login">
          <div className="marca-card">
            <span className="icone-logo">◷</span>

            <h1>
              TIME<strong>ID</strong>
            </h1>
          </div>

          {telaAcesso === "login" && <>
            <h2>Bem-vindo!</h2>
            <p>Faça seu login para acessar o sistema.</p>
            {error && <p className="mensagem-erro" role="alert">{error}</p>}
            {mensagemRecuperacao && <p className="mensagem-sucesso" role="status">{mensagemRecuperacao}</p>}
            <form onSubmit={handleLogin}>
                <label className="rotulo-campo" htmlFor="email">E-mail</label>
                <div className="campo-login">
                  <Mail aria-hidden="true" />

                  <input
                    type="email"
                    id="email"
                    name="email"
                    aria-label="E-mail"
                    autoComplete="username"
                    required
                    maxLength={254}
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    disabled={busy}
                    placeholder="E-mail"
                  />
                </div>

                <label className="rotulo-campo" htmlFor="senha">Senha</label>
                <div className="campo-login">
                  <LockKeyhole aria-hidden="true" />

                  <input
                    type="password"
                    id="senha"
                    name="password"
                    aria-label="Senha"
                    autoComplete="current-password"
                    required
                    maxLength={256}
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    disabled={busy}
                    placeholder="Senha"
                  />
                </div>

                <div className="opcoes-login">
                  <label>
                    <input type="checkbox" />
                    Lembrar de mim
                  </label>

                  <button type="button" className="link-senha" onClick={abrirRecuperacao}>
                    Esqueceu a senha?
                  </button>
                </div>

                <button type="submit" disabled={busy}>
                  {busy ? "Aguarde..." : "Entrar  →"}
                </button>
            </form>
          </>}

          {telaAcesso === "codigo" && <section className="recuperar-senha">
            <button type="button" className="voltar-login" onClick={voltarParaLogin}>← Voltar ao login</button>
            <h2>Redefinir senha</h2>
            <p>Digite o código de 6 dígitos enviado para o seu e-mail.</p>
            {error && <p className="mensagem-erro" role="alert">{error}</p>}
            <form onSubmit={validarCodigo}>
              <label className="rotulo-campo" htmlFor="email-recuperacao">E-mail</label>
              <div className="campo-login">
                <Mail aria-hidden="true" />
                <input
                  type="email"
                  id="email-recuperacao"
                  autoComplete="email"
                  required
                  maxLength={254}
                  value={email}
                  onChange={(evento) => setEmail(evento.target.value)}
                  placeholder="Digite seu e-mail"
                />
              </div>
              <button type="button" className="enviar-codigo" onClick={enviarCodigoRecuperacao} disabled={busy}>
                {busy ? "Enviando..." : "Enviar código"}
              </button>
              {mensagemRecuperacao && <p className="mensagem-sucesso" role="status">{mensagemRecuperacao}</p>}
              <div className="codigo-recuperacao" onPaste={colarCodigo} aria-label="Código de recuperação com seis dígitos">
                {codigoRecuperacao.map((digito, indice) => <input
                  key={indice}
                  ref={(elemento) => { inputsCodigo.current[indice] = elemento; }}
                  type="text"
                  inputMode="numeric"
                  autoComplete={indice === 0 ? "one-time-code" : "off"}
                  maxLength={6}
                  aria-label={`Dígito ${indice + 1} do código`}
                  value={digito}
                  onChange={(evento) => alterarCodigo(evento.target.value, indice)}
                  onKeyDown={(evento) => {
                    if (evento.key === "Backspace" && !digito && indice > 0) inputsCodigo.current[indice - 1]?.focus();
                  }}
                />)}
              </div>
              <button type="submit" disabled={busy}>{busy ? "Validando..." : "Confirmar código →"}</button>
            </form>
            <button type="button" className="reenviar-codigo" onClick={enviarCodigoRecuperacao} disabled={busy}>Não recebeu o código? Reenviar</button>
          </section>}

          {telaAcesso === "nova-senha" && <section className="recuperar-senha">
            <button type="button" className="voltar-login" onClick={() => { setError(""); setTelaAcesso("codigo"); }}>← Alterar código</button>
            <h2>Crie uma nova senha</h2>
            <p>Escolha uma senha forte para proteger sua conta.</p>
            {error && <p className="mensagem-erro" role="alert">{error}</p>}
            <form onSubmit={redefinirSenha}>
              <label className="rotulo-campo" htmlFor="nova-senha">Nova senha</label>
              <div className="campo-login">
                <LockKeyhole aria-hidden="true" />
                <input type="password" id="nova-senha" autoComplete="new-password" minLength={8} required value={novaSenha} onChange={(evento) => setNovaSenha(evento.target.value)} placeholder="Digite a nova senha" />
              </div>
              <label className="rotulo-campo" htmlFor="confirmar-senha">Confirme a nova senha</label>
              <div className="campo-login">
                <LockKeyhole aria-hidden="true" />
                <input type="password" id="confirmar-senha" autoComplete="new-password" minLength={8} required value={confirmarSenha} onChange={(evento) => setConfirmarSenha(evento.target.value)} placeholder="Repita a nova senha" />
              </div>
              <button type="submit" disabled={busy}>{busy ? "Redefinindo..." : "Redefinir senha →"}</button>
            </form>
          </section>}

          <div className="acesso-seguro">
            <span />
            <p><ShieldCheck size={18} /> Acesso seguro — Seus dados protegidos.</p>
            <span />
          </div>

        </div>
      </section>
    </main>
  );
}

export default App;
