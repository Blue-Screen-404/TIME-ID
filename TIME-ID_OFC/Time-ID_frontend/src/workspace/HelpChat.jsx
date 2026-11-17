import { useEffect, useRef, useState } from "react";
import { Bot, MessageCircle, Send, Sparkles, X } from "lucide-react";
import "./HelpChat.css";

const greeting = { role: "assistant", text: "Oi! Posso ajudar com as telas e funções do TIME-ID. O que você quer saber?" };
const suggestions = ["Como registro meu ponto?", "Como solicito férias?", "O que posso fazer em Configurações?"];

export default function HelpChat() {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState([greeting]);
  const [draft, setDraft] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const scrollRef = useRef(null);
  const inputRef = useRef(null);

  useEffect(() => { if (open) { inputRef.current?.focus(); scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: "smooth" }); } }, [open, messages, busy]);
  useEffect(() => {
    if (!open) return undefined;
    function onKeyDown(event) { if (event.key === "Escape") setOpen(false); }
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open]);

  async function ask(text) {
    const question = text.trim();
    if (!question || busy) return;
    setDraft(""); setError(""); setBusy(true);
    const prior = messages.slice(-6).map(message => ({ role: message.role === "assistant" ? "model" : "user", text: message.text }));
    setMessages(current => [...current, { role: "user", text: question }]);
    try {
      const response = await fetch("/api/help-chat", {
        method: "POST", credentials: "same-origin", headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message: question, history: prior })
      });
      const payload = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(payload.title || "Não consegui enviar sua pergunta. Tente novamente.");
      setMessages(current => [...current, { role: "assistant", text: payload.answer, sources: payload.sources || [] }]);
    } catch (requestError) {
      setError(requestError.message || "Não consegui me conectar agora. Tente novamente.");
    } finally { setBusy(false); }
  }

  return <>
    <button className="help-chat-launcher" type="button" onClick={() => setOpen(value => !value)} aria-label={open ? "Fechar assistente TIME-ID" : "Abrir assistente TIME-ID"} aria-expanded={open}>
      {open ? <X size={21}/> : <MessageCircle size={22}/>}<span>Ajuda TIME-ID</span>
    </button>
    {open && <section className="help-chat-panel" role="dialog" aria-modal="false" aria-labelledby="help-chat-title">
      <header className="help-chat-header"><span className="help-chat-avatar"><Bot size={21}/></span><span className="help-chat-heading"><strong id="help-chat-title">Assistente TIME-ID</strong><small><i/> Online para ajudar</small></span><button type="button" className="help-chat-close" onClick={() => setOpen(false)} aria-label="Fechar chat"><X size={19}/></button></header>
      <div className="help-chat-messages" ref={scrollRef} aria-live="polite">
        <div className="help-chat-intro"><Sparkles size={15}/> Tire dúvidas sobre o funcionamento do sistema</div>
        {messages.map((message, index) => <article className={`help-chat-message ${message.role}`} key={`${index}-${message.text.slice(0, 18)}`}>
          {message.role === "assistant" && <span className="help-chat-message-avatar"><Bot size={15}/></span>}
          <div className="help-chat-bubble"><p>{message.text}</p>{message.sources?.length > 0 && <small>{message.sources.length > 1 ? "Seções" : "Seção"}: {message.sources.join(" · ")}</small>}</div>
        </article>)}
        {messages.length === 1 && <div className="help-chat-suggestions">{suggestions.map(item => <button type="button" key={item} onClick={() => ask(item)} disabled={busy}>{item}</button>)}</div>}
        {busy && <div className="help-chat-message assistant"><span className="help-chat-message-avatar"><Bot size={15}/></span><div className="help-chat-bubble help-chat-typing" aria-label="Escrevendo"><i/><i/><i/></div></div>}
        {error && <p className="help-chat-error" role="alert">{error}</p>}
      </div>
      <form className="help-chat-form" onSubmit={event => { event.preventDefault(); ask(draft); }}>
        <input ref={inputRef} value={draft} onChange={event => setDraft(event.target.value)} maxLength={1000} placeholder="Escreva sua dúvida..." aria-label="Sua pergunta" disabled={busy}/>
        <button type="submit" aria-label="Enviar pergunta" disabled={busy || !draft.trim()}><Send size={18}/></button>
      </form>
      <footer className="help-chat-footer">Pode perguntar sobre qualquer tela ou função do TIME-ID.</footer>
    </section>}
  </>;
}
