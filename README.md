# TIMEID

Sistema Inteligente de Gestão de Pessoas, Controle de Jornada e Segurança Corporativa.

**Para iniciar o login temporário, siga [Executar e testar o backend](#executar-e-testar-o-backend).**

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

O projeto está em fase inicial. O backend usa C# e .NET, e o frontend será desenvolvido em React. O backend possui uma API de login temporário funcional; o frontend será integrado a partir de outro projeto.

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

# Executar e testar o backend

O backend é uma **API feita em .NET 8**. Ele roda no seu computador e recebe pedidos pelo endereço **http://localhost:5000**. Esse endereço não é um frontend React nem um site publicado. Nesta etapa, você testa o login pelo PowerShell; ainda não há uma tela com campos de e-mail e senha.

Você usará **dois terminais**:
- **Terminal 1:** mantém o backend ligado.
- **Terminal 2:** envia o e-mail e a senha e testa a sessão.

## 1. Abra a pasta certa

No VS Code, abra a pasta principal TIME-ID, que contém este README, global.json e a pasta TIME-ID_OFC. Depois abra **Terminal > Novo Terminal** e escolha **PowerShell**.

## 2. Confira o .NET

```powershell
dotnet --version
```

O global.json seleciona o SDK **8.0.425**, permitindo atualizações de patch da mesma faixa. Se o comando informar que o SDK não foi encontrado, instale essa versão do **SDK .NET**, não apenas o runtime. Para executar somente o backend, você não precisa de Node.js, npm ou React.

## 3. Inicie o backend no Terminal 1

Se ele já estiver rodando em outro terminal, pare a execução anterior com **Ctrl+C** antes de continuar. Se iniciou pelo depurador do VS Code, use **Shift+F5**.

```powershell
dotnet run --project ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj --urls http://localhost:5000
```

Esse comando restaura as dependências, compila e inicia o backend. Não é preciso executar dotnet build separadamente para iniciar.

Aguarde uma mensagem semelhante a:

```text
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

**Deixe esse terminal aberto.** Ele permanece ocupado enquanto o servidor está funcionando; isso é esperado. Não execute novamente o comando em outro terminal.

Opcionalmente, abra http://localhost:5000 no navegador. Você verá informações da API em JSON, não um formulário de login.

## 4. Faça login no Terminal 2

Abra outro terminal PowerShell pelo botão **+** do painel de terminais. Mantenha o Terminal 1 funcionando.

Use o usuário temporário:

| Campo | Valor |
|---|---|
| E-mail | teste@timeid.local |
| Senha | TimeId@123 |

Copie o bloco inteiro abaixo para o Terminal 2:

```powershell
$credenciais = @{
    email = "teste@timeid.local"
    password = "TimeId@123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -ContentType "application/json" -Body $credenciais -SessionVariable sessao
```

Se funcionar, aparecerão os campos **id**, **username** e **email**. O comando guarda o cookie de autenticação na variável **$sessao** para os próximos passos.

A senha diferencia maiúsculas de minúsculas e não remove espaços. O e-mail ignora diferenças de maiúsculas/minúsculas e espaços nas extremidades.

## 5. Confira se está autenticado

No **mesmo Terminal 2**, execute:

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/auth/me" -WebSession $sessao
```

A API deve devolver os dados do usuário. Sem o cookie da sessão, essa rota retorna **401**.

## 6. Faça logout

Ainda no Terminal 2:

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/auth/logout" -Method Post -WebSession $sessao
```

O logout retorna **204**, sem texto no corpo da resposta. Isso é sucesso, não uma falha.

Se repetir o comando do passo 5 após sair, o PowerShell mostrará **401 (Unauthorized)**. Esse resultado é esperado: você já encerrou a sessão. Para entrar novamente, repita o passo 4.

## 7. Pare o backend antes de recompilar ou executar novamente

Volte ao **Terminal 1** e pressione **Ctrl+C**. Aguarde o prompt do PowerShell reaparecer.

Para iniciar de novo, repita o passo 3. Reiniciar o backend invalida os cookies anteriores, então repita também o login do passo 4.

## Solução do erro MSB3021: arquivo em uso

A mensagem "apphost.exe ... TIME-ID_OFC.exe ... being used by another process" significa que uma instância anterior ainda está usando o executável que a compilação tenta substituir.

1. Pare o backend com **Ctrl+C** no terminal onde ele foi iniciado, ou **Shift+F5** se estiver depurando.
2. Aguarde a execução terminar.
3. Execute novamente o comando do passo 3.

Se você perdeu o terminal original e o processo continuou aberto, execute este bloco **na raiz do repositório**. Ele encerra somente processos cujo caminho corresponde ao executável deste backend:

```powershell
$executavel = Join-Path (Get-Location).Path "TIME-ID_OFC/Time-ID_backend/bin/Debug/net8.0/TIME-ID_OFC.exe"
Get-Process -Name TIME-ID_OFC -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -eq $executavel } |
    Stop-Process -Force
```

Depois repita o passo 3. Se aparecer "Acesso negado", encerre pelo terminal/depurador original ou execute esse mesmo bloco em um PowerShell como administrador, entrando antes na pasta do projeto. Encerrar o backend invalida as sessões temporárias.

Não é necessário apagar bin/obj, reinstalar o .NET ou remover as validações de login para resolver esse bloqueio.

## Outros resultados comuns

| Resultado | Significado / ação |
|---|---|
| Conexão recusada | O backend não está ligado. Verifique o Terminal 1 e aguarde a mensagem de inicialização. |
| Porta 5000 em uso | Já existe um servidor nessa porta. Pare a instância anterior; não inicie duas cópias na mesma porta. |
| 400 no login | Verifique campos obrigatórios, formato do e-mail e JSON enviado. |
| 401 no login | E-mail/senha incorretos ou usuário inativo. |
| 401 em /api/auth/me | Faça login e envie $sessao no mesmo terminal; a sessão também pode ter expirado. |
| 404 em /api/employees | A API de cadastro foi removida. Nesta etapa, use /api/auth/login, /me e /logout. |

## Armazenamento e duração da sessão

O usuário de demonstração é recriado em memória a cada execução. Não há banco de dados nem cadastro de usuários nesta etapa. A senha é verificada por hash com PasswordHasher<User>; respostas nunca incluem a senha nem o hash.

A sessão usa cookie HttpOnly, SameSite=Strict, não persistente, com validade de até 30 minutos e sem renovação automática. HTTPS utiliza Secure; HTTP é permitido para o teste local. Os cookies deixam de funcionar ao reiniciar o backend. As credenciais documentadas são públicas e destinadas apenas à demonstração local.

## Teste automatizado (opcional)

Pare o backend antes de executar o teste, pois ele também precisa compilar o projeto. Com a porta 5099 livre e o terminal na raiz:

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/Test-Login.ps1
```

O teste compila, inicia sua própria instância, verifica login, validações, sessão, logout e reinício e encerra essa instância ao terminar. O resultado esperado é **Todos os testes passaram**.

Para rodar o backend, execute na raiz do repositório: `dotnet run --project ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj --urls http://localhost:5000`; para parar, pressione Ctrl+C no mesmo terminal.
