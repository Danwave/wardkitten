# Tech debt — Wardkitten

Registro de elementos obsoletos/deprecados y deuda técnica conocida.

**Regla:** al deprecar algo (`[Obsolete]`, `@deprecated`), añade una fila con fecha de deprecación,
fecha límite de retirada (deprecación **+ 2 meses**) y sustituto. Las entradas se retiran como máximo a
los 2 meses si no quedan consumidores activos.

## Deprecaciones

| Elemento | Tipo | Deprecado | Fecha límite | Sustituto | Notas |
|----------|------|-----------|--------------|-----------|-------|
| — | — | — | — | — | — |

## Advisories de dependencias aceptados

| Advisory | Paquete | Estado | Justificación | Revisión |
|----------|---------|--------|---------------|----------|
| GHSA-6c8g-7p36-r338 | SharpCompress (transitiva de MongoDB.Driver) | Suprimido por ID en `Directory.Build.props` | Zip-slip en `WriteToDirectory`; sin fix upstream (afecta a todas las versiones). **No explotable**: Wardkitten no extrae archivos a disco con SharpCompress. | Reevaluar al actualizar `MongoDB.Driver` o cuando haya versión parcheada. |

## Deuda conocida / pendientes de la v1 (scaffold a iterar)

| Tema | Descripción | Prioridad |
|------|-------------|-----------|
| MAUI build firmado | Falta workload MAUI + Android SDK/JDK y build firmado para tiendas iOS/Android. Proyecto compila como scaffold. | Media |
| WhatsApp templates | Plantillas de mensaje de WhatsApp deben aprobarse en Meta Business antes de uso en prod. | Media |
| Secretos de producción | Los `K8S/**` usan placeholders; cargar secretos reales por canal seguro. | Alta |
| Pantallas secundarias web | Status pages públicas, equipos/on-call, integraciones (Slack/Discord) quedan esbozadas. | Baja |
