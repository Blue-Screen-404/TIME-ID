import ProfileMenu from "../components/ProfileMenu";
import Sidebar from "../components/Sidebar";
import "./Inicio.css";
import {
  ArrowRight,
  BarChart3,
  Building2,
  Cake,
  CalendarDays,
  ClipboardList,
  Clock3,
  Hand,
  List,
  ShieldCheck,
  UserMinus,
  UserPlus,
  Users,
} from "lucide-react";

function Inicio({ aoNavegar, aoSair, user, busy, onUpdateProfile }) {
  return (
    <main className="dashboard">
      <Sidebar pagina="inicio" aoNavegar={aoNavegar} aoSair={aoSair} busy={busy} />

      <section className="conteudo-dashboard">
        <header className="barra-superior">
          <input
            type="search"
            placeholder="Buscar funcionário, departamento..."
          />

          <ProfileMenu user={user} onUpdate={onUpdateProfile} />
        </header>

        <section className="conteudo-principal">
          <div className="cabecalho-pagina">
            <div className="boas-vindas">
              <div className="icone-boas-vindas">
                <Hand size={26} />
              </div>

              <div>
                <h2>Olá, {user.username}!</h2>

                <p>
                  Aqui está um resumo da sua equipe. Continue assim!
                </p>
              </div>
            </div>

            <div className="data-atual">
              <CalendarDays size={24} />

              <div>
                <strong>{new Date().toLocaleDateString("pt-BR", { dateStyle: "full" })}</strong>
                <span>Bom dia! Tenha um ótimo dia!</span>
              </div>
            </div>
          </div>

          <section className="conteudo-em-colunas">
            <div className="coluna-principal">
              <section className="cards-resumo">
                <article className="card-resumo">
                  <div className="icone-card">
                    <Users size={28} />
                  </div>

                  <strong className="numero-card">0</strong>
                  <span>Funcionários cadastrados</span>

                  <small className="crescimento">
                    ↗ Nenhum este mês
                  </small>
                </article>

                <article className="card-resumo">
                  <div className="icone-card icone-azul">
                    <UserMinus size={28} />
                  </div>

                  <strong className="numero-card">0</strong>
                  <span>Funcionários inativos</span>

                  <small className="neutro">
                    ↓ Nenhum este mês
                  </small>
                </article>

                <article className="card-resumo">
                  <div className="icone-card">
                    <Cake size={28} />
                  </div>

                  <strong className="numero-card">0</strong>
                  <span>Aniversariantes do dia</span>

                  <small className="crescimento">
                    🎉 Nenhum hoje
                  </small>
                </article>

                <article className="card-resumo">
                  <div className="icone-card">
                    <Building2 size={28} />
                  </div>

                  <strong className="numero-card">0</strong>
                  <span>Departamentos</span>

                  <small className="crescimento">
                    → Ver todos
                  </small>
                </article>
              </section>

              <section className="painel">
                <div className="titulo-painel">
                  <div>
                    <Users size={22} />
                    <h3>Últimos funcionários cadastrados</h3>
                  </div>

                  <button type="button">
                    Ver todos <ArrowRight size={17} />
                  </button>
                </div>

                <div className="tabela-responsiva">
                  <table>
                    <thead>
                      <tr>
                        <th>Nome</th>
                        <th>Departamento</th>
                        <th>Data de cadastro</th>
                        <th>Status</th>
                      </tr>
                    </thead>

                    <tbody>
                      <tr>
                        <td colSpan="4" className="tabela-vazia">
                          Nenhum funcionário cadastrado ainda.
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </section>

              <section className="acoes-rapidas">
                <div className="titulo-acoes">
                  <h3>Ações rápidas</h3>
                  <p>
                    Acesse as principais funcionalidades do sistema.
                  </p>
                </div>

                <div className="lista-acoes">
                  <button
                    type="button"
                    className="acao-rapida"
                    onClick={() => aoNavegar("cadastro")}
                  >
                    <UserPlus size={25} />
                    <span>Cadastrar funcionário</span>
                    <ArrowRight size={18} />
                  </button>

                  <button type="button" className="acao-rapida">
                    <List size={25} />
                    <span>Listar funcionários</span>
                    <ArrowRight size={18} />
                  </button>

                  <button type="button" className="acao-rapida">
                    <Building2 size={25} />
                    <span>Departamentos</span>
                    <ArrowRight size={18} />
                  </button>

                  <button type="button" className="acao-rapida">
                    <BarChart3 size={25} />
                    <span>Relatórios</span>
                    <ArrowRight size={18} />
                  </button>
                </div>
              </section>
            </div>

            <aside className="coluna-direita">
              <section className="painel painel-lateral">
                <div className="titulo-painel">
                  <div>
                    <Cake size={22} />
                    <h3>Aniversariantes do dia</h3>
                  </div>

                  <button type="button">
                    Ver todos <ArrowRight size={17} />
                  </button>
                </div>

                <div className="estado-vazio">
                  <Cake size={32} />
                  <p>Nenhum aniversariante hoje.</p>
                </div>
              </section>

              <section className="painel painel-lateral">
                <div className="titulo-painel">
                  <div>
                    <Clock3 size={22} />
                    <h3>Atividades recentes</h3>
                  </div>

                  <button type="button">
                    Ver todas <ArrowRight size={17} />
                  </button>
                </div>

                <div className="estado-vazio">
                  <ClipboardList size={32} />
                  <p>Nenhuma atividade registrada.</p>
                </div>
              </section>

              <section className="card-seguranca">
                <ShieldCheck size={32} />

                <div>
                  <h3>Segurança e praticidade</h3>
                  <p>
                    Seus dados protegidos com a mais alta tecnologia.
                  </p>
                </div>
              </section>
            </aside>
          </section>
        </section>
      </section>
    </main>
  );
}

export default Inicio;