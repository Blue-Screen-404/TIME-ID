# Análise e decisões de modelagem

## Decisão 1 — Preservar os registros de persistência enquanto se integra comportamento

**Alternativa A:** trocar imediatamente o modelo persistido `Staff` pela classe `Employee` de `Dominios/Employee/`.

**Alternativa B (direção escolhida):** manter `Staff` como formato de persistência/API e mover responsabilidades de domínio em passos pequenos, adaptando o limite de persistência sem alterar nomes de colunas, JSON, DTOs ou contratos HTTP.

**Justificativa:** `TimeIdDbContext` mapeia `Staff` diretamente para a tabela `Employees`; serviços, controladores, migrações e respostas dependem dos nomes e tipos atuais. Uma substituição ampla aumentaria o risco de quebrar persistência e contratos. O código já constrói `Employee` em `EmployeeService`, mas o converte de volta para `Staff`, então essa ponte é um ponto concreto para evolução gradual. A escolha não afirma que o desenho atual seja o ideal; ela reduz o tamanho de cada mudança e exige preservar os contratos existentes.

## Decisão 2 — Preservar o histórico de ponto e férias

**Alternativa A:** permitir exclusão em cascata do funcionário e remover seu histórico associado.

**Alternativa B (implementada):** bloquear exclusão quando existem marcações ou férias e orientar a inativação.

**Justificativa:** ponto e férias constituem histórico de negócio. A regra de exclusão em `EmployeeService.Delete` preserva esses registros e torna explícita a diferença entre encerrar o vínculo ativo e apagar dados históricos. A associação é por identificadores e coordenada pela aplicação; não há composição EF declarada.

## Avaliação de propriedades de POO

- **Coesão:** `EmployeeService` executa autenticação/autorização, valida vários dados e sincroniza conta, liderança, perfil facial, endereço, funcionário e auditoria. Há responsabilidades candidatas a extração.
- **Encapsulamento:** métodos como `AssignLeader` e `UpdateContact` protegem algumas operações de `Employee`, mas vários setters públicos permitem modificar estado sem passar por esses métodos.
- **Acoplamento:** `EmployeeService` depende diretamente de `Database` e das coleções concretas; a persistência e as verificações entre funcionários são coordenadas no mesmo serviço.
- **Polimorfismo:** não foi identificado contrato comum com implementações distintas utilizado por esses fluxos.
- **Invariantes entre registros:** unicidade e ciclo de liderança dependem da consulta ao conjunto; portanto não podem ser garantidos por uma instância isolada de `Employee`.

## Parecer desta revisão

Aceitar a evolução gradual como estratégia segura de integração. Não considerar a modelagem atual concluída: o objeto de domínio ainda não é a identidade persistida, estados de férias e marcações não são protegidos por comportamento próprio e faltam testes automatizados de regra no nível dos objetos.
