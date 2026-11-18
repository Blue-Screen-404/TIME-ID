# Especificação verificável

Esta especificação registra três fluxos presentes no backend. Datas e horários são os tratados pelo servidor; os critérios não pressupõem funcionalidades futuras.

## Fluxos

### F1 — Manter funcionário

Um administrador cria, edita ou inativa um funcionário. O backend valida os dados, verifica o departamento e as relações de liderança, sincroniza a conta vinculada quando aplicável e registra a alteração na auditoria.

**Aceitação:** um cadastro válido é salvo e aparece na consulta; CPF inválido, matrícula/CPF/e-mail duplicados, departamento inexistente ou ciclo de liderança são recusados sem persistir a alteração. Funcionário com histórico de ponto ou férias não pode ser excluído; deve ser inativado.

### F2 — Solicitar e analisar férias

Um usuário vinculado solicita férias para si; um administrador pode criar ou analisar solicitações. O backend aplica limites de data, duração e sobreposição e mantém o estado da solicitação.

**Aceitação:** período de 1 a 14 dias corridos, após a admissão, sem sobreposição não recusada é aceito; período invertido, acima do limite, anterior à admissão ou sobreposto é recusado. Usuário comum não aprova nem altera uma solicitação já analisada.

### F3 — Registrar ponto

O funcionário ativo registra pelo app ou pelo terminal facial. O backend determina o próximo tipo de marcação, impede marcações durante férias aprovadas, limita o dia a quatro marcações e aplica intervalo mínimo entre registros.

**Aceitação:** a sequência Entrada → Início do intervalo → Retorno do intervalo → Saída é registrada; uma quinta marcação, marcação durante férias aprovadas ou tentativa antes de 30 segundos é recusada. A marcação aceita permanece no histórico.

## Regras de negócio rastreáveis

| ID | Regra | Implementação observada |
|---|---|---|
| RN01 | CPF deve passar pela validação dos dígitos verificadores; CPF, matrícula e e-mail não podem duplicar outro funcionário. | `Services/EmployeeService.cs`, `ValidCpf` e `Save` |
| RN02 | Liderança não pode apontar para si próprio nem formar ciclo; o líder selecionado deve estar ativo. | `Services/EmployeeService.cs`, `WouldCreateLeadershipCycle` e `Save` |
| RN03 | Férias devem ocorrer após a admissão, durar até 14 dias corridos e não sobrepor outro pedido não recusado. | `Controllers/WorkspaceController.cs`, gravação de férias |
| RN04 | Ponto não pode ser lançado em férias aprovadas; há no máximo quatro marcações diárias e intervalo mínimo de 30 segundos. | `Controllers/WorkspaceController.cs` e `Controllers/FacialController.cs` |
| RN05 | Funcionário com histórico de ponto ou férias não pode ser excluído. | `Services/EmployeeService.cs`, `Delete` |

Os critérios acima descrevem comportamento inspecionado no código. Os cenários de aceitação ainda precisam estar cobertos por testes automatizados no nível de domínio e integração conforme a matriz de auditoria.
