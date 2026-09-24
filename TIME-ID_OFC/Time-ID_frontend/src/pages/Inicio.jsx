import "./Inicio.css";

function Inicio() {
  return (
    <main className="dashboard">
      <aside className="menu-lateral">
        <h1>
          TIME<span>ID</span>
        </h1>

        <nav>
          <a className="menu-ativo" href="/inicio">
            Início
          </a>

          <a href="/">Ponto</a>
          <a href="/">Funcionários</a>
          <a href="/">Cadastrar</a>
          <a href="/">Listar</a>
          <a href="/">Departamentos</a>
          <a href="/">Relatórios</a>
          <a href="/">Férias</a>
          <a href="/">Feriados</a>
          <a href="/">Configurações</a>
        </nav>

        <footer>TIMEID v1.0.0</footer>
      </aside>

      <section className="conteudo-dashboard">
        <header className="barra-superior">
          <input
            type="search"
            placeholder="Buscar funcionário, departamento..."
          />

          <div className="perfil">
            <div className="foto-perfil">GS</div>

            <div>
              <strong>Gabriel Silva</strong>
              <span>Administrador</span>
            </div>
          </div>
        </header>

        <section className="conteudo-principal">
          <h2>Olá, Gabriel!</h2>

          <p>
            Aqui está um resumo da sua equipe. Continue assim!
          </p>
        </section>
      </section>
    </main>
  );
}

export default Inicio;