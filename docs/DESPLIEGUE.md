# 🐾 Chiron — Despliegue y Hosting

> Decisiones de infraestructura para publicar Chiron.
> Última actualización: 2026-09-26

---

## Decisión: Railway + Docker

- **Hosting elegido: Railway** — aloja la **API .NET** y la **base de datos PostgreSQL** en un mismo lugar, con panel simple y capa gratuita para empezar.
- **Empaquetado: Docker** — Chiron se empaqueta en un contenedor, lo que permite desplegarlo en Railway hoy y moverlo a otro proveedor (Render, Fly.io, Cloud Run, Ubuntu propio) sin reescribir nada. Cero lock-in.

### Estrategia gratis → pago
- **Gratis**: desarrollo, demos y primeros 1-2 clientes (Railway ofrece crédito mensual gratis).
- **Pago**: cuando crezca el número de veterinarias (más recursos, sin "dormir" el servicio). ~5-20 USD/mes al inicio.

---

## Contexto: qué hospeda cada cosa

| Componente | Dónde | Estado |
|---|---|---|
| API .NET (ASP.NET Core) | Railway (contenedor Docker) | Pendiente (Épica 8) |
| Base de datos PostgreSQL | Railway (o Supabase/Neon si se separa después) | Pendiente (Épica 7) |

### Aclaración sobre servicios considerados
- **Supabase** = PostgreSQL gestionado (BD), NO ejecuta .NET. Opción válida para la BD si algún día se separa de Railway.
- **Cloudflare Workers** = ejecuta JS/WASM, NO .NET tradicional. Descartado para la API.
- Alternativas de hosting de la API (equivalentes, por si se migra): Render, Fly.io, Google Cloud Run, Azure App Service.

---

## Pasos de despliegue (cuando lleguemos a Épica 7/8)

1. Crear `Dockerfile` multi-stage para la API .NET (build + runtime ligero).
2. (Opcional) `docker-compose.yml` para desarrollo local (API + PostgreSQL).
3. Migrar persistencia de "en memoria" a PostgreSQL (EF Core) — Épica 7 (H7.1).
4. Conectar el repo de GitHub a Railway (deploy automático en cada push a main).
5. Configurar variables de entorno (cadena de conexión, secretos) en Railway — NO en el código.

---

## ⚠️ Consideraciones para producción (datos de salud)

Chiron maneja expedientes médicos (datos sensibles). Antes de datos reales de pacientes:
- Verificar cumplimiento de protección de datos (México: LFPDPPP).
- Usar planes que garanticen backups, cifrado y control de ubicación de datos.
- Nunca poner secretos/credenciales en el código ni en el repo (usar variables de entorno de Railway).
