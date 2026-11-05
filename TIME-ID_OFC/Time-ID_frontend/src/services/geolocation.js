export async function geoApi(path, options = {}) {
  let response;
  try { response = await fetch(`/api${path}`, { credentials: 'same-origin', ...options, headers: { 'Content-Type': 'application/json', ...options.headers }, body: options.body === undefined ? undefined : JSON.stringify(options.body) }); }
  catch { throw new Error('Não foi possível conectar ao servidor.'); }
  if (response.status === 401) { window.dispatchEvent(new Event('timeid:expired')); throw new Error('Sua sessão expirou. Entre novamente.'); }
  const result = await response.json().catch(() => null);
  if (!response.ok) throw new Error(result?.errors ? Object.values(result.errors).flat().join(' ') : result?.title || 'Não foi possível concluir a operação.');
  return result;
}
export function currentPosition() {
  return new Promise((resolve, reject) => {
    if (!navigator.geolocation || !window.isSecureContext) { reject(new Error('A localização exige HTTPS ou acesso por localhost.')); return; }
    navigator.geolocation.getCurrentPosition(p => resolve({ latitude: p.coords.latitude, longitude: p.coords.longitude, accuracyMeters: p.coords.accuracy, capturedAt: new Date(p.timestamp).toISOString() }), e => reject(new Error(({ 1: 'Permissão de localização negada. Você pode informar uma justificativa para registrar sem localização.', 2: 'Localização indisponível. Tente novamente ou informe uma justificativa.', 3: 'A captura demorou demais. Tente novamente ou informe uma justificativa.' })[e.code] || 'Não foi possível obter a localização.')), { enableHighAccuracy: true, maximumAge: 0, timeout: 12000 });
  });
}
export const geoStatus = { DentroDaArea: 'Dentro da área', ForaDaArea: 'Fora da área', Inconclusivo: 'Precisão insuficiente', SemRegra: 'Sem local permitido configurado', SemLocalizacao: 'Sem localização', CapturaDesatualizada: 'Captura desatualizada', Pendente: 'Pendente de análise', NaoNecessaria: 'Análise não necessária', Aceita: 'Justificativa aceita', Recusada: 'Justificativa recusada', Expirada: 'Prazo de análise expirado' };
