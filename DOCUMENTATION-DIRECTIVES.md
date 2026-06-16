# Directivas de documentación — Wardkitten

Versión condensada (adaptada de IntegraSystem). La documentación se organiza en capas; la mínima
obligatoria es el **Feature Registry**.

## Principios

- Documenta el **porqué** (negocio) y el **cómo** (técnico), no repitas el código.
- No asumas la intención de negocio a partir del código: pregunta al desarrollador.
- Cada feature tiene un **código** `FXX.YY` estable y una ficha en `docs/features/`.

## Estructura

```
docs/
  features/_registry.md        # tabla maestra de features con su código FXX.YY
  features/FXX-nombre/overview.md
  architecture/c4-context.md   # diagrama de contexto (C4 nivel 1)
  architecture/c4-containers.md
  devlog/YYYYMMDD-titulo.md     # post-mortems de incidentes runtime/infra
  guides/ reference/ explanation/   # Diátaxis (narrativa, opcional)
```

## Sistema de numeración de features

`F` + módulo (2 dígitos) `.` + feature (2 dígitos). Módulos previstos:

| Módulo | Área |
|--------|------|
| F01 | Cuentas y autenticación (registro, login, OTP, verificación) |
| F02 | Watches (creación, schedule, tolerancias, channel bindings) |
| F03 | Check-ins / ping (HTTP, manual, start/fail) |
| F04 | Motor de evaluación e incidentes (alertas, escalado, idempotencia) |
| F05 | Canales de notificación (Email, Telegram, Push, SMS, WhatsApp) |
| F06 | Wallet de créditos y canales metered |
| F07 | Billing / suscripciones (Stripe, planes y límites) |
| F08 | Web (dashboard, status pages) |
| F09 | Móvil (push, acciones rápidas) |

## Ficha de feature (`docs/features/FXX-nombre/overview.md`)

```markdown
# FXX.YY — Nombre de la feature
## Metadata
- Estado: (borrador | implementada | deprecada)
- Módulo: FXX
## Descripción            (qué hace y para quién)
## Elementos UI           (componentes Razor implicados)
## Endpoints              (rutas de la API)
## Modelo de datos        (colecciones/campos Mongo)
## Reglas de negocio
## Dependencias / Sub-features
```

## Reglas para agentes

- Al implementar una feature, crea/actualiza su ficha y añade la fila en `docs/features/_registry.md`.
- Marca en el código `// Feature: FXX.YY` en la cabecera del archivo principal.
- Registra incidentes de runtime/infra difíciles de diagnosticar en `docs/devlog/`.
