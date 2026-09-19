# TIMEID

Sistema Inteligente de Gestão de Pessoas, Controle de Jornada e Segurança Corporativa.

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

## Controle de Versão

- Git
- GitHub

---

# Estrutura do Projeto

```text
TIME-ID/
├── .gitignore
├── README.md
├── TIME-ID_OFC/
│   ├── Time-ID_backend/
│   │   ├── Dominios/
│   │   ├── Program.cs
│   │   └── TIME-ID_OFC.csproj
│   └── Time-ID_frontend/
│       ├── package.json
│       └── src/
│           ├── App.jsx
│           └── main.jsx
├── TIMEID-Plano_dev.docx
├── TIMEID.docx
└── anotacoes.txt
```

O projeto está em fase inicial. O backend usa C# e .NET, e o frontend será desenvolvido em React. A inicialização das aplicações ainda está em implementação.

As pastas locais ainda vazias, como `API_controller`, `DTOs`, `Services`, `components`, `pages` e `services`, não são versionadas pelo Git até receberem arquivos. As pastas geradas `bin/` e `obj/` são ignoradas.

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

# Instalação

## Pré-requisitos

Instalar:

- .NET SDK 8.0
- Node.js e npm (para o frontend React)
- Git
- Visual Studio 2022 ou VS Code

Verificar instalação:

```bash
dotnet --version
node --version
npm --version
```

---

## Clonar o Repositório

```bash
git clone https://github.com/Blue-Screen-404/TIME-ID.git
```

```bash
cd TIME-ID
```

---

## Restaurar Dependências

Na raiz do repositório, informe o caminho do projeto backend:

```bash
dotnet restore TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

---

## Compilar

```bash
dotnet build TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

A compilação do executável depende da implementação do ponto de entrada em `Program.cs`, que ainda está vazio.

---

## Executar

Após implementar a inicialização do backend:

```bash
dotnet run --project TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

O frontend ainda contém arquivos iniciais vazios. Os comandos de instalação e execução serão documentados após configurar as dependências e os scripts no `package.json`.

---

# Banco de Dados

A configuração de persistência será documentada quando for definida e implementada.

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

# fazer o sistema funcionar APP

TIMEID — Frontend

Interface web do sistema TIMEID, desenvolvida com React e Vite.

Pré-requisitos

Antes de iniciar, instale:

Node.js 22 LTS

GitHub Desktop ou Git

O Node.js já instala o npm automaticamente. Após a instalação, confirme no terminal:

node --version
npm --version

Use Node.js 20.19 ou superior.

Baixar o projeto

Pelo GitHub Desktop

Abra o GitHub Desktop e faça login.

Clique em File → Clone repository.

Selecione o repositório TIME-ID.

Escolha a pasta onde o projeto será salvo e clique em Clone.

No GitHub Desktop, clique em Repository → Open in Terminal.

Pelo terminal

git clone https://github.com/Blue-Screen-404/TIME-ID.git
cd TIME-ID

Instalar as dependências

Entre na pasta do frontend:

cd TIME-ID_OFC/Time-ID_frontend

Instale as bibliotecas do projeto, incluindo React e Vite:

npm install

Esse comando é necessário apenas na primeira execução ou quando as dependências forem atualizadas.
E caso ele não funcione ou gere um erro, selecione no terminal do vscode a seta para baixo e exeute o command prompt e ali tente exeutar o comando novamente.

Iniciar o frontend

Ainda na pasta Time-ID_frontend, execute:

npm run dev

O Vite mostrará um endereço parecido com este:

http://localhost:5173/

Abra o endereço informado no navegador. Mantenha o terminal aberto enquanto estiver usando o projeto.

Para encerrar o servidor, pressione Ctrl + C no terminal.
