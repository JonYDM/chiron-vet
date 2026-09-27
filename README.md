# 🐾 Chiron

**Software de gestión para veterinarias** (SaaS multi-tenant) — mercado latinoamericano, iniciando en Morelos, México.

> El nombre viene de *Quirón (Chiron)*, el centauro sabio de la mitología griega, maestro de la medicina.

🌐 **Backend en producción:** https://chiron-vet-production.up.railway.app

---

## ¿Qué es?

Chiron es un SaaS (software por suscripción) para veterinarias que buscan digitalizar y mejorar su operación. No inventamos una categoría nueva: **mejoramos lo existente** con un producto más simple, accesible y con un diferenciador claro.

### Diferenciador clave
🔔 **Recordatorios automáticos** de citas, vacunas y desparasitaciones mediante **notificaciones push / portal del dueño de la mascota** (sin costo por mensaje ni fricción de crear cuentas externas). Que el cliente regrese = la veterinaria gana más.

> WhatsApp quedó como opción futura bajo demanda (ver `docs/INTEGRACION-WHATSAPP.md`).

---

## Estado del proyecto

### ✅ Backend (completo y desplegado en producción)
- Gestión de **clientes** (dueños) y **mascotas** + registro rápido
- **Expediente médico** (consultas, vacunas, desparasitaciones con próxima aplicación)
- **Agenda de citas** (con estados)
- **Recordatorios** (detección de vacunas/citas próximas)
- **Punto de venta** (catálogo + ventas con control de stock)
- **Multi-tenant**: cada veterinaria con datos aislados
- **Seguridad**: login con usuario/teléfono + PIN, JWT, roles, SuperAdmin, control de suscripción
- **API REST** + **PostgreSQL** + **Docker**, desplegado en **Railway**

### 🚧 En construcción
- **Frontend** (React + Vite, PWA) — repo aparte `chiron-web`. Ver `docs/FRONTEND.md`
- Notificaciones push web
- (Futuro) Integración real de WhatsApp

---

## Arquitectura

**Monolito modular con Clean Architecture** (backend) + **frontend desacoplado** (SPA que consume la API REST).

```
Backend (repo chiron-vet)              Frontend (repo chiron-web)
├── Chiron.Domain         (entidades)  └── React + Vite (PWA)
├── Chiron.Application    (casos uso)      consume la API REST
├── Chiron.Infrastructure (datos/EF)       desplegado en Vercel/Netlify
├── Chiron.Api            (REST + JWT)
└── Chiron.ConsoleApp     (pruebas)
        ↓ desplegado en Railway + PostgreSQL
```

### Stack técnico
| Componente | Tecnología |
|---|---|
| Backend | C# / .NET 8 (LTS), Clean Architecture |
| Base de datos | PostgreSQL (EF Core) |
| Autenticación | JWT + BCrypt (usuario/teléfono + PIN) |
| Empaquetado | Docker |
| Hosting backend | Railway |
| Frontend (en construcción) | React + Vite (PWA) |
| Hosting frontend (planeado) | Vercel o Netlify |

### Principios de ingeniería
- SOLID e inyección de dependencias
- Código mantenible y multiplataforma
- Seguridad validada en el backend (roles por endpoint)
- Eficiencia de cómputo

---

## Roles del sistema

- 🔑 **SuperAdmin** (dueño de Chiron): gestiona veterinarias y suscripciones.
- **Administrador** (de cada veterinaria): gestiona su staff y operación.
- **Veterinario**: expedientes clínicos.
- **Recepcionista**: agenda, registro, ventas.
- **Dueño de mascota**: portal limitado con sus mascotas, historial y recordatorios.

---

## Documentación (`docs/`)

- [`CONTEXTO.md`](./docs/CONTEXTO.md) — Documento maestro del proyecto
- [`BACKLOG.md`](./docs/BACKLOG.md) — Épicas, historias de usuario y sprints
- [`METODOLOGIA.md`](./docs/METODOLOGIA.md) — Git Flow, Conventional Commits
- [`BITACORA.md`](./docs/BITACORA.md) — Diario del proyecto
- [`DESPLIEGUE.md`](./docs/DESPLIEGUE.md) — Hosting (Railway) y Docker
- [`ROADMAP-SEGURIDAD-NOTIFICACIONES.md`](./docs/ROADMAP-SEGURIDAD-NOTIFICACIONES.md) — Seguridad, roles, bootstrap del SuperAdmin
- [`INTEGRACION-WHATSAPP.md`](./docs/INTEGRACION-WHATSAPP.md) — WhatsApp (feature futura)
- [`FRONTEND.md`](./docs/FRONTEND.md) — Plan del frontend (React + Vite)

---

## Metodología

- Kanban/Scrum con backlog priorizado
- Git Flow simplificado (`main` + `feature/*`), `main` protegida con ruleset
- Commit por historia de usuario con [Conventional Commits](https://www.conventionalcommits.org/)
- Documentación viva en `docs/`
