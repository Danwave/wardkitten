> **ANTES DE CUALQUIER ACCIÓN: lee `AGENTS.md` completo.** Todas las instrucciones del proyecto están ahí.

# Wardkitten

Watchdog SaaS para tareas/procesos periódicos (dead-man's-switch). Stack: .NET 10 (API + worker),
MongoDB, Blazor WASM (web) + .NET MAUI Blazor Hybrid (móvil), Stripe (suscripciones + créditos),
canales Email/Telegram/Push (gratis) y SMS/WhatsApp (de pago, vía wallet de créditos). K8s + ArgoCD.

## Publicar nueva versión (K8S deploy)

| Workflow | Imagen | Carpeta manifiestos |
|---|---|---|
| `Build` (API) | `ghcr.io/avanware/wardkitten:N` | `K8S/{produccion,preproduccion}/wardkitten.yaml` |
| `Build Worker` | `ghcr.io/avanware/wardkitten-worker:N` | `K8S/{produccion,preproduccion}/worker.yaml` |

```bash
gh run list --repo Danwave/wardkitten --workflow "Build" --limit 1 --json number,status,displayTitle
OLD=12; NEW=13
find K8S -name "wardkitten.yaml" | xargs sed -i "s|wardkitten:$OLD|wardkitten:$NEW|g"
git add K8S/ && git commit -m "K8S deploy wardkitten:$NEW" && git push
```

Numeraciones independientes para API y worker. Despliegue por ArgoCD (Synced + Healthy).
