export const locationPreviewKey = employeeId => `timeid:location-preview:${employeeId}`;

export function locationMapUrl(employeeId, preview = false) {
  const query = new URLSearchParams({ mapa: employeeId });
  if (preview) query.set('previa', '1');
  return `/geolocalizacao?${query}`;
}

export async function geoApi(path, options = {}) {
  let response;
  try { response = await fetch(`/api${path}`, { credentials: 'same-origin', ...options, headers: { 'Content-Type': 'application/json', ...options.headers }, body: options.body === undefined ? undefined : JSON.stringify(options.body) }); }
  catch { throw new Error('Não foi possível conectar ao servidor.'); }
  if (response.status === 401) { window.dispatchEvent(new Event('timeid:expired')); throw new Error('Sua sessão expirou. Entre novamente.'); }
  const result = await response.json().catch(() => null);
  if (!response.ok) {
    const error = new Error(result?.errors ? Object.values(result.errors).flat().join(' ') : result?.title || 'Não foi possível concluir a operação.');
    error.status = response.status;
    throw error;
  }
  return result;
}
export function currentPosition() {
  return new Promise((resolve, reject) => {
    if (!navigator.geolocation || !window.isSecureContext) { reject(new Error('A localização exige HTTPS ou acesso por localhost.')); return; }
    const timeoutMs = 15000;
    const desiredAccuracyMeters = 90;
    let best = null, watchId = null, settleTimer = null, timeoutTimer = null, settled = false;
    const messageFor = code => ({ 1: 'Permissão de localização negada. Você pode informar uma justificativa para registrar sem localização.', 2: 'Localização indisponível. Tente novamente ou informe uma justificativa.', 3: 'A captura demorou demais. Tente novamente ou informe uma justificativa.' })[code] || 'Não foi possível obter a localização.';
    function stop() {
      if (watchId !== null) navigator.geolocation.clearWatch(watchId);
      window.clearTimeout(settleTimer);
      window.clearTimeout(timeoutTimer);
    }
    function finish(position) {
      if (settled) return;
      settled = true;
      stop();
      if (!position) { reject(new Error(messageFor(3))); return; }
      resolve({ latitude: position.coords.latitude, longitude: position.coords.longitude, accuracyMeters: position.coords.accuracy, capturedAt: new Date(position.timestamp).toISOString() });
    }
    function failed(error) {
      if (best) finish(best);
      else if (!settled) { settled = true; stop(); reject(new Error(messageFor(error.code))); }
    }
    watchId = navigator.geolocation.watchPosition(position => {
      if (!best || position.coords.accuracy < best.coords.accuracy) {
        best = position;
        window.clearTimeout(settleTimer);
        // Keep watching longer when the reported accuracy is still broad.
        const settleMs = best.coords.accuracy <= desiredAccuracyMeters ? 1500
          : best.coords.accuracy <= 150 ? 3000
          : best.coords.accuracy <= 300 ? 5000
          : 8000;
        settleTimer = window.setTimeout(() => finish(best), settleMs);
      }
    }, failed, { enableHighAccuracy: true, maximumAge: 0, timeout: timeoutMs });
    timeoutTimer = window.setTimeout(() => finish(best), timeoutMs);
  });
}
export const geoStatus = { DentroDaArea: 'Dentro da área', ForaDaArea: 'Fora da área', Inconclusivo: 'Precisão insuficiente', SemRegra: 'Sem local permitido configurado', SemLocalizacao: 'Sem localização', CapturaDesatualizada: 'Captura desatualizada', Pendente: 'Pendente de análise', NaoNecessaria: 'Análise não necessária', Aceita: 'Justificativa aceita', Recusada: 'Justificativa recusada', Expirada: 'Prazo de análise expirado' };
