# TIMEID

Sistema Inteligente de Gestão de Pessoas, Controle de Jornada e Segurança Corporativa.

**Para iniciar o React e a API C# juntos, siga [Executar o sistema integrado](#executar-o-sistema-integrado).**

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

O projeto está em fase inicial. A tela React está integrada à API de autenticação em C#/.NET. O ASP.NET serve o frontend compilado e a API no mesmo endereço. O usuário de demonstração permanece em memória; ainda não há banco de dados nem cadastro de usuários.

Pastas vazias não são versionadas pelo Git. O backend possui DTOs, serviço de autenticação e controller de login implementados. As pastas geradas `bin/` e `obj/` são ignoradas.

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

# Executar o sistema integrado

Este passo a passo é para quem quer baixar o projeto, abrir a tela de login e testar o sistema no próprio computador. Você usará **um terminal e um navegador**.

**Para executar a versão pronta, basta o .NET. Não é necessário instalar Node.js nem executar comandos npm.** O backend C# entrega a interface React já preparada e atende às solicitações de login no mesmo endereço.

## 1. Confira o que precisa estar instalado

- **SDK .NET 8.0.425**, ou um patch posterior da mesma faixa 8.0.4xx, conforme o arquivo `global.json`. Instale o **SDK**, que permite compilar o código; apenas o runtime não basta para este passo a passo.
- **Um navegador**, como Edge, Chrome ou Firefox.
- **VS Code**, se quiser seguir as instruções de abertura abaixo. Você também pode usar um terminal PowerShell diretamente.

Na primeira execução, mantenha a conexão com a internet disponível para o .NET restaurar as dependências, caso seja necessário.

## 2. Abra a pasta principal do projeto

Se baixou um ZIP, extraia os arquivos antes de começar. Se usa GitHub Desktop, abra a pasta do repositório baixado.

No VS Code:

1. Clique em **Arquivo > Abrir Pasta** (*File > Open Folder*).
2. Selecione a pasta principal do projeto, que contém `README.md`, `global.json` e `TIME-ID_OFC`.
3. Clique em **Terminal > Novo Terminal** (*Terminal > New Terminal*).
4. Se houver opção de terminal, escolha **PowerShell**.

Você deve estar nesta estrutura:

```text
TIME-ID/                   ← execute os comandos nesta pasta
├── README.md
├── global.json
├── scripts/
└── TIME-ID_OFC/
    ├── Time-ID_backend/
    └── Time-ID_frontend/
```

Para conferir a pasta atual, digite no terminal e pressione Enter:

```powershell
Get-Location
Get-ChildItem
```

A listagem deve mostrar `README.md`, `global.json` e `TIME-ID_OFC`. Se esses itens não aparecerem, abra a pasta correta antes de continuar. Não entre na pasta do frontend para executar os comandos abaixo.

## 3. Confira se o .NET está disponível

No mesmo terminal, execute:

```powershell
dotnet --version
```

O resultado deve ser uma versão compatível com o `global.json`, como `8.0.425`.

- Se aparecer **“dotnet não é reconhecido”**, instale o SDK .NET e reabra o VS Code ou o terminal.
- Se aparecer **“SDK não encontrado”** ou **“A compatible .NET SDK was not found”**, confira as versões instaladas com `dotnet --list-sdks` e instale uma versão compatível. Ter apenas o SDK de outra versão principal não atende à configuração deste projeto.

## 4. Inicie o sistema

Copie este comando inteiro para o terminal, na pasta principal, e pressione Enter:

```powershell
dotnet run --project ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

Esse comando faz três coisas: prepara as dependências .NET, compila o código C# e inicia o servidor que entrega a tela e a API. Você não precisa executar `dotnet build` separadamente.

Aguarde uma mensagem semelhante a:

```text
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

**Deixe esse terminal aberto.** Ele fica ocupado enquanto o sistema está funcionando; isso é esperado. Não execute o comando novamente em outro terminal.

A porta `5000` já está definida em `Time-ID_backend/Properties/launchSettings.json`. Não é preciso iniciar um segundo servidor para o frontend.

## 5. Abra a tela e faça login

Abra o navegador e digite este endereço na barra de endereços:

**[http://localhost:5000](http://localhost:5000)**

`localhost` significa o seu próprio computador. O endereço funciona enquanto o servidor iniciado no passo anterior estiver ligado; ele não é um site publicado na internet.

Você deve ver a tela de login do TIMEID. Preencha:

| Campo | O que digitar |
|---|---|
| E-mail | `teste@timeid.local` |
| Senha | `TimeId@123` |

Clique em **Entrar**. A senha deve ser digitada exatamente como está na tabela, respeitando maiúsculas e minúsculas.

Se funcionar, a tela mostrará o nome do usuário, o e-mail, a mensagem **“Login realizado com sucesso.”** e o botão **Sair**. Esta versão demonstra a autenticação; os outros módulos ainda não estão implementados.

Para conferir o fluxo:

1. Recarregue a página: a sessão válida deve manter o usuário conectado.
2. Clique em **Sair**: o formulário de login deve reaparecer.
3. Tente entrar com uma senha incorreta: a tela deve informar que o e-mail ou a senha são inválidos.

O usuário é de demonstração e fica na memória, sem banco de dados. Ao reiniciar o backend, a sessão anterior deixa de valer e você precisa fazer login novamente.

## 6. Encerre ou execute novamente

Para **desligar o sistema**, volte ao terminal onde executou `dotnet run` e pressione **Ctrl+C**. Aguarde o terminal voltar a aceitar comandos. Fechar apenas a aba do navegador não encerra o servidor.

Para **executar outro dia**, abra novamente a pasta principal e repita o comando do passo 4. Depois acesse o endereço do passo 5. Não é necessário reinstalar o SDK a cada execução.

Se alterar o código C#, pare o servidor com Ctrl+C e execute o comando novamente para compilar e usar a alteração.

## Se algo não funcionar

| O que aconteceu | O que fazer |
|---|---|
| “O arquivo de projeto não existe” ou `MSB1009` | Confira se o terminal está na pasta que contém `TIME-ID_OFC` e copie o comando completo do passo 4. |
| “dotnet não é reconhecido” | Instale o SDK .NET e reabra o terminal. |
| “A compatible .NET SDK was not found” | Confira o passo 3 e a versão exigida no `global.json`. |
| Porta 5000 em uso ou “address already in use” | Verifique se o sistema já está aberto em outro terminal. Encerre a execução anterior com Ctrl+C antes de iniciar outra. |
| Arquivo em uso ao compilar, `MSB3021` ou `MSB3027` | Pare a execução anterior com Ctrl+C. Se iniciou pelo depurador do VS Code, use Shift+F5. Depois repita o passo 4. |
| O navegador informa “conexão recusada” | Confira se o terminal continua aberto e se apareceu “Now listening on”. Use `http://localhost:5000`, conforme configurado. |
| Aparece JSON em vez da tela | Abra `/`, ou seja, `http://localhost:5000`. O endereço `/api` mostra informações técnicas da API. |
| A página inicial retorna 404 | Confira se `Time-ID_backend/wwwroot/index.html` e a pasta `wwwroot/assets` vieram junto com o projeto. Eles contêm a interface pronta. |
| E-mail ou senha inválidos | Use as credenciais do passo 5. Não acrescente espaços na senha. |
| A sessão deixou de funcionar | Faça login novamente. A sessão expira em até 30 minutos e também é invalidada ao reiniciar o backend. |

**Os passos acima são suficientes para executar e testar a tela.** As próximas seções explicam a integração e tarefas opcionais de desenvolvimento.

## Como a integração funciona

1. O navegador solicita `/`. `UseDefaultFiles` seleciona `index.html` e `UseStaticFiles` entrega os arquivos compilados de `wwwroot`.
2. Ao abrir a tela, React chama `GET /api/auth/me`. Uma sessão válida recupera o usuário; `401` mostra o formulário.
3. Ao enviar o formulário, `src/services/auth.js` faz `fetch` para `POST /api/auth/login` com JSON contendo `email` e `password`.
4. O ASP.NET valida `LoginRequest`. `AuthController` chama `AuthenticationService`, que compara o e-mail e verifica o hash da senha do objeto `User` em memória.
5. Quando as credenciais são válidas, o backend retorna os dados do usuário e envia o cookie `TimeId.Session`. A senha e o hash não são retornados.
6. O navegador guarda e envia o cookie nas próximas chamadas. O React usa `credentials: "same-origin"`; não guarda senha nem token no localStorage.
7. O botão Sair chama `POST /api/auth/logout`; o backend expira o cookie e o React volta ao formulário.

Frontend e API usam a mesma origem (protocolo, host e porta), portanto esse modo não precisa configurar CORS. O React não faz a validação real da senha; ela permanece no C#.

| Método | Endereço | Resultado |
|---|---|---|
| GET | `/` | Interface React |
| GET | `/api` | Informações da API em JSON |
| POST | `/api/auth/login` | `200` com usuário e cookie; `400` para entrada inválida; `401` para credenciais incorretas |
| GET | `/api/auth/me` | `200` com usuário autenticado ou `401` sem sessão válida |
| POST | `/api/auth/logout` | `204` após sair; exige autenticação |

A senha diferencia maiúsculas, minúsculas e espaços. O e-mail ignora caixa e espaços nas extremidades. A sessão usa cookie HttpOnly, SameSite=Strict, não persistente, com validade de até 30 minutos e sem renovação automática. HTTPS gera cookie Secure; HTTP é permitido na demonstração local. Reiniciar o backend recria o usuário e invalida as sessões anteriores. As credenciais são públicas e destinadas a testes locais.

## Alterar o código-fonte React (somente desenvolvimento do frontend)

O backend continua exclusivamente em C# e a execução normal exige apenas .NET. O código-fonte React foi preservado em `Time-ID_frontend`.

Se um desenvolvedor alterar JSX, CSS ou imagens nessa pasta, precisará atualizar a versão pronta em `wwwroot`. As ferramentas atuais para essa tarefa são Node/npm/Vite:

```powershell
npm.cmd ci --prefix ./TIME-ID_OFC/Time-ID_frontend
npm.cmd run build --prefix ./TIME-ID_OFC/Time-ID_frontend
```

Esses comandos não são necessários para executar o sistema nem para alterar apenas o backend C#. O Vite substitui o conteúdo gerado de `wwwroot`; não coloque arquivos manuais nessa pasta. Inclua o código-fonte alterado e os arquivos gerados no mesmo commit para que quem baixar o projeto execute apenas com .NET.

## Teste automatizado da API (opcional)

Este teste é para quem está desenvolvendo o projeto; não é necessário para abrir a tela de login.

Pare o backend com Ctrl+C e, na pasta principal do projeto, execute:

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/Test-Login.ps1
```

O teste usa a porta `5099`, que deve estar livre. Ele compila o projeto, inicia sua própria instância e verifica entradas inválidas, login, cookies, consulta do usuário, logout e invalidação após reiniciar. Ao terminar, encerra apenas a instância de teste.

O resultado esperado é **“Todos os testes passaram.”** Para abrir a interface depois, execute novamente o comando do passo 4.

## Gerar uma versão de distribuição (opcional)

Esta etapa prepara uma pasta com o sistema pronto para execução. Ela não é necessária para trabalhar no projeto com `dotnet run`.

Na pasta principal, execute:

```powershell
dotnet publish ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj -c Release -o ./artifacts/time-id
```

Depois entre na pasta gerada e inicie o sistema:

```powershell
cd ./artifacts/time-id
dotnet TIME-ID_OFC.dll --urls http://localhost:5000
```

O .NET inclui a interface de `wwwroot` na publicação, sem executar Node/npm. Para executar essa distribuição, o computador precisa do runtime **ASP.NET Core 8**. Mantenha a pasta publicada completa, incluindo `wwwroot`, e execute a DLL de dentro dela.

A porta é informada nesse comando porque `launchSettings.json` é usado no desenvolvimento, não na execução da distribuição. Pare qualquer instância anterior na porta 5000 antes de iniciar. Para encerrar, use Ctrl+C.
