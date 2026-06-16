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

## Pendientes bloqueados por terceros / herramientas (no realizables en código)

Estos pendientes no dependen de escribir código en el repo, sino de herramientas, cuentas externas o
secretos. Quedan documentados para retomarlos en el entorno adecuado.

| Tema | Descripción | Bloqueo |
|------|-------------|---------|
| MAUI build firmado | El scaffold `Wardkitten.Mobile` reutiliza `Shared.UI` y vive en `wardkitten.mobile.slnx`. Para compilar/publicar: `dotnet workload install maui` + Android SDK/JDK, y **macOS + Xcode** para iOS, más assets de tienda y firma. | Workload + SDKs + cuentas de tienda (no instalables en este entorno) |
| Push FCM (móvil) | `FcmTokenRegistrar` ya registra el token en la API (`POST /api/auth/push-tokens`), pero obtener el token del dispositivo requiere el SDK de Firebase por plataforma (p.ej. Plugin.Firebase). | Requiere proyecto Firebase + integración nativa |
| Plantillas WhatsApp | Las plantillas de mensaje de WhatsApp deben aprobarse en **Meta Business** antes de usarse en prod. | Aprobación externa de Meta |
| Secretos de producción | Los `K8S/**` usan placeholders (`REPLACE_ME`); cargar los secretos reales por canal seguro (sealed-secrets/ArgoCD), nunca en git. | Operativo (no es código) |

> Nota: status pages, equipos/on-call e integraciones salientes (Webhook/Slack/Discord/Microsoft Teams)
> ya están **implementadas** (antes figuraban como esbozo en esta tabla).
