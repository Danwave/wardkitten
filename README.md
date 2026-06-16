<div align="center">

# 🐾 Wardkitten

**Tu gatito vigilante.** Un *watchdog* SaaS para tareas y procesos periódicos.

</div>

Wardkitten vigila que las cosas que deben pasar **de forma periódica** —facturar, regar las plantas,
hacer backups de servidores— realmente pasen. Funciona como un *dead-man's-switch* (perro guardián
electrónico): cada tarea espera una **señal de confirmación** (check-in) antes de su deadline. Si la
señal no llega dentro de la tolerancia configurada, Wardkitten **te avisa**.

## Cómo funciona

- **Procesos automáticos** → hacen un *ping* HTTP a una URL única al terminar (estilo healthcheck).
- **Tareas manuales** → confirmas desde la app, un botón en el email/SMS, o un comando de Telegram.
- **Tolerancias**: margen de retraso (*grace*) y nº de fallos consecutivos permitidos (*skip*).
- **Si todo va en plazo, silencio.** Si una tarea incumple, alerta por los canales que elijas
  **por tarea** (apilables): Email, Telegram, Push, SMS o WhatsApp.

## Planes y créditos

- Planes **Free / Pro / Team** (Stripe).
- SMS y WhatsApp tienen coste real → se pagan con una **wallet de créditos prepago**, independiente del
  plan (incluido el gratuito). Sin saldo, esos canales se desactivan; el resto siguen.

## Arquitectura

| Componente | Tecnología |
|---|---|
| API | ASP.NET Core (.NET 10) + SignalR |
| Worker | BackgroundService (.NET 10) — motor de evaluación |
| BBDD | MongoDB |
| Web | Blazor WebAssembly |
| Móvil | .NET MAUI Blazor Hybrid (iOS/Android) |
| Pagos | Stripe |
| Canales | MailKit (Email), Telegram Bot, FCM (Push), Twilio (SMS/WhatsApp) |
| Despliegue | Docker + Kubernetes + ArgoCD |

## Desarrollo

```bash
docker compose up -d          # levanta MongoDB local
dotnet build wardkitten.slnx
dotnet test
dotnet run --project src/Wardkitten.Api
```

Ver `AGENTS.md` (convenciones), `SECURITY.md` (seguridad) y `docs/` (features y arquitectura).
