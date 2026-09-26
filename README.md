# TIMEID

Sistema Inteligente de Gestão de Pessoas, Controle de Jornada e Segurança Corporativa.

**Para abrir o sistema, siga [Como executar o projeto](#como-executar-o-projeto). O acesso começa pelo login e leva à tela inicial após validar e-mail e senha.**

---

## Sobre o Projeto

O TIMEID é uma plataforma desenvolvida para centralizar processos de Recursos Humanos, controle de jornada, gestão organizacional e monitoramento operacional.

O projeto foi concebido em duas etapas:

### Fase 1 — Sistema Base

- Gestão de Funcionários
- Controle de Ponto
- Controle de Departamentos
- Hierarquia Organizacional
- Gestão de Férias
- Gestão de Feriados
- Relatórios
- Dashboard Gerencial
- Auditoria de Alterações

### Fase 2 — Inteligência Artificial e Segurança Inteligente

- Reconhecimento Facial
- Geolocalização
- Controle de Visitantes
- Monitoramento por Câmeras
- Identificação de Pessoas Não Cadastradas
- Alertas Inteligentes
- Assistente Inteligente de RH
- Segurança Corporativa Integrada

---

# Objetivo

Fornecer uma solução moderna para gestão de pessoas, controle de frequência e monitoramento corporativo, permitindo escalabilidade futura através da integração de Inteligência Artificial.

---

# Tecnologias Utilizadas

## Backend

- C#
- .NET 8

## Frontend

- React
- CSS e Lucide React (ícones)
- Vite e Node.js/npm para instalar as dependências e preparar a interface

## Controle de Versão

- Git
- GitHub

---

# Estrutura do Projeto

```text
TIME-ID/
├── README.md
├── global.json
├── scripts/
└── TIME-ID_OFC/
    └── Time-ID_backend/
        ├── Controllers/       ← acesso às páginas e autenticação
        ├── wwwroot/           ← interface React preparada
        ├── Dominios/
        ├── Program.cs
        └── TIME-ID_OFC.csproj

TIME-ID_OFC/Time-ID_frontend/
├── src/                      ← telas e componentes React
├── package.json              ← dependências do frontend
└── vite.config.js            ← preparação da interface e conexão com a API
```

O projeto usa React para as telas e C#/ASP.NET Core para servir a interface preparada e validar o login. A página inicial e o formulário demonstrativo de cadastro exigem uma sessão autenticada. O usuário de teste fica em memória; ainda não há banco de dados nem gravação dos cadastros. As opções dos demais módulos no menu ainda são demonstrativas.

---

# Funcionalidades

## Gestão de Funcionários

- Cadastro de colaboradores
- Edição de dados
- Inativação de funcionários
- Histórico de informações
- Associação com departamento
- Associação com cargo
- Associação com líder

---

## Controle de Ponto

- Entrada
- Início do intervalo
- Retorno do intervalo
- Saída

---

## Correções de Ponto

- Inclusão manual
- Alteração de registros
- Justificativas obrigatórias
- Histórico de alterações

---

## Relatórios

### Individual

- Horas trabalhadas
- Horas extras
- Faltas
- Atrasos

### Departamento

- Indicadores da equipe

### Empresa

- Indicadores gerais

---

## Exportação

- PDF
- Excel

---

## Dashboard

Indicadores em tempo real:

- Funcionários ativos
- Funcionários inativos
- Presentes
- Ausentes
- Em férias
- Atrasados

---

# Entidades da Fase 1

## Employee

Representa o colaborador da empresa.

Campos principais:

- Matrícula
- Nome
- CPF
- E-mail
- Telefone
- Data de nascimento
- Data de admissão
- Departamento
- Cargo
- Líder
- Status

## Department

Representa os departamentos da organização.

## Position

Representa cargos.

## User

Representa usuários do sistema.

## Role

Perfis de acesso:

- Usuário
- Líder
- Administrador

## Permission

Controle granular de permissões.

## TimeRecord

Registro de ponto.

## TimeRecordEdit

Correções de ponto.

## Vacation

Controle de férias.

## Holiday

Controle de feriados.

## AuditLog

Registro de auditoria.

---

# Entidades da Fase 2

## FacialProfile

Cadastro facial dos colaboradores.

## Visitor

Controle de visitantes.

## SecurityEvent

Eventos de segurança.

Exemplos:

- Funcionário Reconhecido
- Visitante Reconhecido
- Pessoa Não Identificada
- Acesso Não Autorizado

## GeolocationRecord

Registro de localização.

## AIAlert

Alertas gerados pela Inteligência Artificial.

---
# Git Flow

## Branch Principal

```text
main
```

Código estável.

---

## Branch de Desenvolvimento

```text
build
```

Integração das funcionalidades.

---

## Criando uma Nova Feature

Atualizar develop:

```bash
git checkout develop
git pull origin develop
```

Criar branch:

```bash
git checkout -b feature/nome-da-feature
```

Exemplos:

```bash
git checkout -b feature/employee-registration
```

```bash
git checkout -b feature/time-record
```

---

## Commit

Adicionar alterações:

```bash
git add .
```

Commit:

```bash
git commit -m "feat(employee): create employee entity"
```

---

## Enviar para o GitHub

```bash
git push origin feature/employee-registration
```

---

## Abrir Pull Request

Fluxo:

```text
feature/*
        ↓
develop
        ↓
main
```

---

# Padrão de Commits

## Nova funcionalidade

```text
feat:
```

Exemplo:

```text
feat(employee): create employee entity
```

---

## Correção

```text
fix:
```

Exemplo:

```text
fix(auth): login validation
```

---

## Refatoração

```text
refactor:
```

---

## Documentação

```text
docs:
```

---

## Testes

```text
test:
```

---

## Ajustes internos

```text
chore:
```

---

# Roadmap

## Fase 1

- [ ] Autenticação
- [ ] Funcionários
- [ ] Departamentos
- [ ] Hierarquia
- [ ] Controle de Ponto
- [ ] Auditoria
- [ ] Férias
- [ ] Feriados
- [ ] Relatórios
- [ ] Dashboard

## Fase 2

- [ ] Reconhecimento Facial
- [ ] Geolocalização
- [ ] Controle de Visitantes
- [ ] Segurança Inteligente
- [ ] Alertas Automáticos
- [ ] Assistente de RH com IA

---

# Equipe

Blue-Screen-404

Projeto acadêmico desenvolvido para a disciplina de desenvolvimento de software e evolução para uma plataforma inteligente de gestão de pessoas e segurança corporativa.

# Como executar o projeto

Você precisa de **um terminal e um navegador**. O C# entrega a interface React já preparada e verifica o login. Para executar a cópia preparada incluída no projeto, basta o .NET.

O frontend continua sendo React. Node.js/npm e Vite são ferramentas de preparação: são necessários para instalar dependências ou atualizar essa cópia após alterar o React, mas não precisam ficar rodando junto com o sistema.

## 1. Instale o necessário

Instale o **SDK .NET 8.0.425**, ou uma atualização da mesma faixa **8.0.4xx**, conforme o arquivo `global.json`. O SDK é o conjunto de ferramentas que prepara e executa o projeto; instalar somente o runtime não basta.

Depois da instalação, reabra o terminal e digite:

```powershell
dotnet --version
```

Deve aparecer uma versão compatível, como `8.0.425`. Você também precisará de um navegador, como Edge, Chrome ou Firefox. Na primeira execução, mantenha a internet disponível para baixar as bibliotecas necessárias.

## 2. Abra a pasta do projeto

Se baixou um ZIP, extraia os arquivos. No VS Code, clique em **Arquivo > Abrir Pasta** e selecione a pasta que contém este `README.md`, `global.json` e `TIME-ID_OFC`.

Depois clique em **Terminal > Novo Terminal**. O terminal é o espaço onde você digita o comando para ligar o sistema. Deixe-o na pasta principal do projeto.

## 3. Inicie o sistema

Copie o comando inteiro abaixo no terminal e pressione Enter:

```powershell
dotnet run --project ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

Esse comando prepara o código e liga o servidor — a parte que entrega as telas ao navegador. Aguarde aparecer:

```text
Now listening on: http://localhost:5000
```

**Mantenha esse terminal aberto enquanto usar o sistema.** Não é necessário iniciar outro programa ou executar um segundo comando.

## 4. Abra o login e entre

No navegador, acesse **[http://localhost:5000](http://localhost:5000)**. `localhost` significa o seu próprio computador; esse endereço funciona enquanto o servidor estiver ligado.

Sem uma sessão ativa, o sistema abre a tela de login. Use os dados de demonstração:

| Campo | O que digitar |
|---|---|
| E-mail | `teste@timeid.local` |
| Senha | `TimeId@123` |

Clique em **Entrar**. A senha diferencia letras maiúsculas e minúsculas.

Para testar as permissões de usuário comum, use `usuario@timeid.local` com a mesma senha `TimeId@123`. A conta `teste@timeid.local` é administradora.

No canto superior direito, clique no perfil e escolha **Ver perfil**. O administrador pode editar o próprio nome, e-mail e foto; o usuário comum pode alterar somente a foto. O tipo de acesso não é editável pelo modal. Ao editar, use **Salvar** para confirmar ou **Cancelar** para descartar: clicar fora ou pressionar Esc não fecha o modal. Fotos aceitam PNG, JPEG ou WebP de até 2 MB.

As alterações de perfil ficam em memória e são descartadas ao reiniciar o servidor. Se o administrador alterar seu e-mail, deverá usar o novo endereço no próximo login enquanto essa execução estiver ativa.


- **Dados corretos:** o sistema abre automaticamente a tela inicial, com a sidebar (menu lateral) e o resumo da equipe.
- **Dados incorretos:** você continua no login e recebe uma mensagem para conferir e-mail e senha.
- **Sair:** o botão no menu lateral encerra sua sessão e volta ao login.

A tela inicial e o cadastro não podem ser acessados sem autenticação. Se você recarregar a página com uma sessão válida, continuará conectado. A sessão dura até 30 minutos; ao reiniciar o servidor, será necessário entrar novamente.

O cadastro e os indicadores ainda são demonstrativos: nenhum funcionário é salvo e nenhum convite é enviado. As credenciais acima servem apenas para testar o projeto localmente.

## 5. Encerrar e executar outro dia

Para desligar, volte ao terminal e pressione **Ctrl+C**. Fechar somente o navegador não encerra o servidor.

Para executar novamente, abra a pasta principal, repita o comando do passo 3 e acesse o endereço do passo 4. Não é necessário reinstalar o .NET. Se alterar apenas o C#, pare o servidor e execute o mesmo comando novamente. Se alterar o React, atualize primeiro a interface conforme abaixo.

## Se algo não funcionar

| O que aconteceu | Como resolver |
|---|---|
| “dotnet não é reconhecido” | Instale o SDK .NET e reabra o terminal ou o VS Code. |
| “A compatible .NET SDK was not found” | Confira `dotnet --list-sdks` e instale uma versão da faixa 8.0.4xx indicada em `global.json`. |
| “O arquivo de projeto não existe” ou `MSB1009` | Abra o terminal na pasta que contém `README.md` e `TIME-ID_OFC`; copie o comando completo do passo 3. |
| Falha ao baixar bibliotecas | Confira a conexão com a internet e execute o comando novamente. |
| Porta 5000 em uso | Verifique se o sistema já está ligado em outro terminal. Encerre a execução anterior com Ctrl+C antes de iniciar outra. |
| Arquivo em uso, `MSB3021` ou `MSB3027` | Pare a execução anterior com Ctrl+C (ou Shift+F5 se iniciou pelo depurador do VS Code) e tente novamente. |
| O navegador mostra “conexão recusada” | Confira se o terminal continua aberto e se apareceu “Now listening on”. Use `http://localhost:5000`. |
| E-mail ou senha inválidos | Copie os dados do passo 4, sem acrescentar espaços na senha. |
| O sistema voltou ao login | A sessão pode ter expirado ou o servidor ter sido reiniciado. Entre novamente. |

## Se você alterar as telas React

Instale Node.js 22.12 ou superior. Na pasta principal, execute:

```powershell
npm.cmd ci --prefix ./TIME-ID_OFC/Time-ID_frontend
npm.cmd run build --prefix ./TIME-ID_OFC/Time-ID_frontend
```

O primeiro comando instala as dependências do frontend (necessário na primeira vez ou quando elas mudarem). O segundo prepara as telas e grava os arquivos em `wwwroot`, para o C# entregá-los ao navegador. Depois execute o mesmo `dotnet run` do passo 3. Ao compartilhar alterações das telas, inclua também os arquivos preparados em `wwwroot`.

Você não precisa executar `npm run dev` para usar o projeto integrado. React cuida da interface; C# cuida da autenticação e da API.
