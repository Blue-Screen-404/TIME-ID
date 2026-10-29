# Arquitetura do TIME-ID

Este documento descreve a organização atual do sistema e o caminho dos dados entre a interface, a API e o armazenamento local. O TIME-ID é uma aplicação web local: o servidor ASP.NET Core entrega os arquivos estáticos do React e atende às chamadas da API na mesma origem.

## Visão geral

```text
Navegador
  ├── Interface React (arquivos compilados em Time-ID_backend/wwwroot)
  └── chamadas HTTP /api/* com cookie de sessão
          ↓
ASP.NET Core 8
  ├── Controllers: autenticação, área de trabalho e recursos faciais
  ├── Services: autenticação, perfil e serviços auxiliares
  ├── Dominios e DTOs: entidades e contratos da API
  └── Operations/DataStore: validação, estado e persistência JSON
          ↓
App_Data/timeid.json
```

O frontend não possui um servidor de produção separado. Depois de preparado, seus arquivos são servidos pelo mesmo processo C# que implementa a API. Por isso, a execução documentada usa apenas `dotnet run`.

## Estrutura do repositório

```text
TIME-ID/
├── Arch.md                         # arquitetura e instruções para desenvolvimento
├── README.md                       # uso do sistema
├── global.json                     # faixa do SDK .NET usada pelo projeto
├── scripts/                        # testes de integração em PowerShell
├── artifacts/                      # dados temporários usados pelos testes
└── TIME-ID_OFC/
    ├── Time-ID_backend/
    │   ├── Controllers/            # endpoints HTTP da API
    │   ├── DTOs/                   # contratos de entrada e saída
    │   ├── Dominios/               # entidades agrupadas por área funcional
    │   ├── Operations/             # DataStore, modelos e autenticação em duas etapas
    │   ├── Repositories/           # componentes de acesso a dados
    │   ├── Services/               # autenticação e serviços auxiliares
    │   ├── Views/                  # recursos de interface do backend
    │   ├── App_Data/               # dados e chaves locais gerados em execução
    │   ├── wwwroot/                # frontend compilado e modelos faciais
    │   ├── Program.cs              # configuração e inicialização da aplicação
    │   └── TIME-ID_OFC.csproj
    └── Time-ID_frontend/
        ├── src/pages/              # telas de entrada e cadastro
        ├── src/workspace/          # telas funcionais e componentes da área logada
        ├── src/components/         # componentes compartilhados
        ├── src/services/           # chamadas HTTP para a API
        ├── scripts/                # preparação dos modelos faciais no build
        └── package.json            # dependências e scripts Vite
```

## Interface e comunicação

O frontend é escrito em React, JavaScript e CSS. O Vite gera o pacote estático, que é publicado em `Time-ID_backend/wwwroot`. As funções em `src/services/auth.js`, `workspace.js` e `facial.js` centralizam as chamadas para `/api/auth`, `/api/workspace` e `/api/facial`.

As chamadas usam a mesma origem e enviam o cookie de sessão do navegador. O backend também serve `index.html` nas rotas da interface, como `/inicio`, `/ponto` e `/relatorios`; essas rotas protegidas exigem autenticação. Rotas de API desconhecidas não são redirecionadas para a interface e continuam respondendo como API.

O reconhecimento facial é executado no navegador com os modelos distribuídos em `wwwroot/models`. O servidor oferece endpoints para os recursos faciais e controla o acesso conforme o perfil. As telas e o fluxo de câmera estão em `src/workspace/Facial.jsx` e `Facial.css`.

## Backend e responsabilidades

- `Program.cs` registra os serviços, configura cookies, autorização, arquivos estáticos e rotas da interface e da API.
- `Controllers/AuthController.cs` trata login, sessão, perfil, senha e autenticação em duas etapas.
- `Controllers/WorkspaceController.cs` reúne operações da área de trabalho, como cadastros, ponto, férias, feriados, relatórios e backup.
- `Controllers/FacialController.cs` expõe operações do fluxo de reconhecimento facial.
- `Services/AuthenticationService.cs` implementa validações de identidade e segurança da sessão. `PasswordResetEmailService.cs` contém o serviço auxiliar de redefinição de senha.
- `Operations/Models.cs` define o modelo serializado do estado local. `Operations/DataStore.cs` lê, valida e persiste esse estado.
- `Operations/TwoFactor.cs` implementa funcionalidades de autenticação em duas etapas; `DTOs/` define os objetos usados nos limites HTTP.
- `Dominios/` agrupa entidades de funcionários, departamentos, ponto, férias, feriados, auditoria e painel.

Os controllers e serviços aplicam as permissões no servidor. A visibilidade de botões e menus na interface não é o mecanismo de autorização.

## Autenticação e segurança

O login emite um cookie HTTP-only chamado `TimeId.Session`, com validade de 30 minutos e política SameSite Strict. A API responde `401` para chamadas sem sessão válida e `403` para operações sem permissão. A validade é conferida com identificador do usuário e marcadores de sessão e segurança; alterações relevantes invalidam sessões anteriores.

As senhas são armazenadas como hashes usando `PasswordHasher`. O ASP.NET Core Data Protection guarda suas chaves locais em `App_Data/keys`; no Windows, as chaves são protegidas com DPAPI. Respostas da API recebem `Cache-Control: no-store`. Segredos locais e dados gerados em `App_Data` não devem ser incluídos no Git.

## Persistência atual

Na execução atual, o `DataStore` mantém o estado em memória e o grava em `Time-ID_backend/App_Data/timeid.json`. O caminho pode ser substituído pela configuração `TIMEID_DATA_PATH`. Quando o arquivo ainda não existe, ele é inicializado com contas de demonstração; os dados existentes não são recriados automaticamente.

As operações de escrita são serializadas por um lock. Cada mutação é aplicada a uma cópia do estado e salva primeiro em arquivo temporário, que então substitui o arquivo principal. Isso reduz o risco de deixar o JSON parcialmente gravado, mas não transforma o armazenamento em banco transacional compartilhado. O desenho pressupõe uma única instância do servidor; várias instâncias não devem gravar no mesmo arquivo.

`TimeIdDbContext` e o registro de SQLite no `Program.cs` são preparação para uma migração futura. Atualmente, o contexto não é usado para ler ou gravar os dados, nenhuma migração é executada na inicialização e o arquivo SQLite não é a persistência ativa. O armazenamento efetivo permanece sendo JSON.

## Build e execução de desenvolvimento

### Alterações no backend C#

Na raiz do repositório, compile ou execute o servidor:

```powershell
dotnet build ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
dotnet run --project ./TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj
```

O segundo comando inicia o backend e serve a interface em `http://localhost:5000`.

### Alterações no frontend React

O frontend requer Node.js 22.12 ou superior para o processo de build. Na raiz do repositório:

```powershell
npm.cmd ci --prefix ./TIME-ID_OFC/Time-ID_frontend
npm.cmd run build --prefix ./TIME-ID_OFC/Time-ID_frontend
```

O build prepara os modelos faciais e gera os arquivos de produção em `Time-ID_backend/wwwroot`. Inclua esses arquivos compilados ao compartilhar alterações da interface. Para usá-las, reinicie o processo `dotnet run` se ele já estiver em execução.

## Testes de integração

Os testes automatizados disponíveis são scripts PowerShell localizados em `scripts/`. Execute-os na raiz:

```powershell
./scripts/Test-Login.ps1
./scripts/Test-Crud.ps1
```

Eles compilam o backend, iniciam servidores temporários nas portas 5099 e 5101 e usam arquivos de dados isolados em `artifacts`. Cobrem login e sessão, perfil, operações CRUD, permissões, validações, relatórios, persistência após reinício, senha e autenticação em duas etapas. Os dados da execução normal não são utilizados pelos testes.
