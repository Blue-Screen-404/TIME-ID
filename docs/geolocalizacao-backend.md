# Geolocalização no backend

## Estado atual

Esta etapa cria somente arquivos de estrutura, sem campos, endpoints ou lógica.
Não altera o registro de ponto, a persistência ou a configuração de serviços.
O escopo é exclusivo do backend.

## Regras acordadas

- Cada empresa gerencia seus próprios locais permitidos.
- Cada local terá nome, latitude, longitude, raio de tolerância e situação ativa.
- Um local pode ser liberado para toda a empresa, departamentos ou colaboradores específicos.
- As permissões são somadas: basta um local aplicável permitir a localização.
- A validação será feita pelo servidor, considerando distância e precisão informada.
- Os dados de empresas diferentes devem permanecer isolados.

## Propostas que ainda precisam ser confirmadas

- Fora dos locais permitidos: exigir justificativa e manter a marcação pendente de análise.
- Localização ausente ou imprecisa: definir exigência de justificativa e tratamento.
- Colaborador sem locais aplicáveis: definir tratamento.
- Definir limites de precisão, idade da captura e tamanho da justificativa.
- Definir responsáveis, permissões e fluxo de análise de justificativas.
- Definir acesso e retenção dos dados de localização.

## Organização dos arquivos

| Arquivo | Responsabilidade futura |
| --- | --- |
| Controllers/WorkLocationsController.cs | Cadastro, consulta, edição e desativação de locais |
| DTOs/Geolocation/CreateWorkLocationRequest.cs | Contrato de criação |
| DTOs/Geolocation/UpdateWorkLocationRequest.cs | Contrato de atualização |
| DTOs/Geolocation/WorkLocationResponse.cs | Contrato de resposta |
| DTOs/Geolocation/PunchLocationInput.cs | Localização e justificativa recebidas com a batida |
| Services/WorkLocationService.cs | Gestão de locais e permissões |
| Services/GeolocationValidationService.cs | Seleção de locais e validação geográfica |
| Services/ReverseGeocodingService.cs | Obtenção de bairro/cidade a partir das coordenadas |

## Integrações futuras nos arquivos existentes

- Operations/Models.cs: modelos de locais, vínculos e resultado da localização; extensão de Punch e PunchInput.
- Operations/IWorkspaceStore.cs e Operations/DataStore.cs: avaliar e adaptar persistência conforme o contrato existente.
- Services/PunchService.cs: integrar a validação à marcação, preservando o horário do servidor.
- Controllers/WorkspaceController.cs: adaptar consultas e respostas quando necessário.
- Program.cs: registrar serviços quando houver implementação.

Antes de implementar, confirmar como a empresa será identificada e isolada no modelo atual.
Não assumir que o identificador de empresa enviado pelo cliente seja uma autorização.
O provedor de geocodificação reversa permanece em aberto.

## Ordem sugerida

1. Fechar regras pendentes e identificação da empresa.
2. Implementar cadastro de locais e permissões.
3. Implementar validação por distância e precisão.
4. Integrar ao registro e à consulta do ponto.
5. Implementar consulta do local aproximado sem impedir a marcação por falha do provedor.
6. Adicionar testes em projeto separado, fora da pasta do projeto web.

## Cenários de teste futuros

- Local permitido pela empresa, pelo departamento e pelo colaborador.
- Soma de permissões e isolamento entre empresas.
- Locais inativos e ausência de locais aplicáveis.
- Posições dentro, fora e próximas ao limite do raio.
- Coordenadas inválidas, precisão insuficiente e captura antiga.
- Ausência de localização, justificativas e falha na geocodificação.
- Preservação do horário do servidor e dos dados originais da marcação.
