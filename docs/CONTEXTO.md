# 🐾 Chiron — Contexto del Proyecto

> **Este es el documento maestro.** Se lee al inicio de cada sesión para retomar el hilo rápidamente.
> Última actualización: 2026-09-26

---

## ¿Qué es Chiron?

**Chiron** es un producto de software (SaaS) de gestión para **veterinarias** del mercado latinoamericano, iniciando en **Morelos, México** (Temixco, Cuernavaca, Jiutepec).

El nombre viene de *Quirón (Chiron)*, el centauro sabio de la mitología griega, maestro de la medicina.

### Modelo de negocio
- **SaaS por suscripción mensual** (software que se "renta", no se vende una sola vez).
- Precio objetivo accesible (~149-299 MXN/mes) frente a competidores caros (500-1500 MXN/mes).

### Propuesta de valor ("hacerlo mejor")
No inventamos una categoría nueva; mejoramos lo existente:

| Problema de la competencia | Mejora de Chiron |
|---|---|
| Caro | Precio accesible |
| Complicado, sobrecargado | Simple, directo, en español mexicano |
| Sin recordatorios (o cobran extra) | **Recordatorios automáticos** vía notificaciones push / portal del dueño (diferenciador estrella) |
| Soporte lento / en inglés | Soporte local y cercano |
| Solo escritorio | Multiplataforma (web, celular) |

### Diferenciador clave (ACTUALIZADO 2026-09-26)
**Recordatorios automáticos** de citas, vacunas y desparasitaciones mediante **notificaciones push / portal del dueño de la mascota** dentro de la propia app web. Esto hace que el cliente regrese → la veterinaria gana más → justifica pagar la renta mensual.

> **GIRO DE ESTRATEGIA**: los recordatorios pasan de WhatsApp a **push notifications in-app** (costo cero, sin fricción de crear cuentas de Meta ni pagar por mensaje — barrera de venta en LATAM). Se agrega un **rol Dueño de mascota** para que el cliente final vea sus mascotas y recordatorios. Detalle y roadmap en `ROADMAP-SEGURIDAD-NOTIFICACIONES.md`.
> **WhatsApp** queda como **feature futura bajo demanda** (diseño preservado en `INTEGRACION-WHATSAPP.md`). La lógica de detección de recordatorios (Épica 5) se reaprovecha; solo cambia el canal de entrega gracias a la abstracción `IServicioMensajeria`.

---

## Decisiones técnicas tomadas

| Decisión | Elección | Razón |
|---|---|---|
| Lenguaje | **C# / .NET 8 (LTS)** | SOLID y DI nativos, alto rendimiento, tipado fuerte, multiplataforma |
| Arquitectura | **Clean Architecture** (Domain, Application, Infrastructure, ConsoleApp) | Separación de capas, inversión de dependencias |
| Estilo de despliegue | **Monolito modular** (no microservicios al inicio) | Rápido, barato y manejable para un solo dev; se puede evolucionar a servicios cuando el negocio lo pida |
| Base de datos (futuro) | **PostgreSQL** | Gratis, potente, corre excelente en Linux |
| Pruebas iniciales | **Repositorio en memoria** | No requiere instalar BD todavía |
| Despliegue (futuro) | **Railway (Docker)** + PostgreSQL | Aloja API .NET y BD juntas, capa gratuita para empezar; Docker evita lock-in. Ver `DESPLIEGUE.md` |
| Primer entregable | **Backend probado en consola** | Validar lógica antes de API/frontend |

### Principios de código
- **SOLID** (especialmente inversión de dependencias).
- **Inyección de dependencias**.
- **Código mantenible y legible**.
- **Eficiencia de cómputo**: evitar lógica y comparaciones redundantes (ej: para enteros, `> 1` en vez de `>= 2`).
- **Multiplataforma desde el día 1**: sin APIs ni rutas exclusivas de Windows.

### Decisión arquitectónica: Monolito modular
- Se inicia con un **monolito modular**, NO con microservicios/SOA.
- Razón: para un solo desarrollador y un producto en validación, microservicios agregan complejidad y costo sin beneficio. Regla "MonolithFirst" (Martin Fowler).
- El código se organiza por **módulos de negocio** (Clientes, Mascotas, Expedientes, Citas, Recordatorios, PuntoVenta) con bajo acoplamiento y alta cohesión.
- Gracias a Clean Architecture + SOLID + DI, si un módulo necesitara escalar de forma independiente en el futuro, puede **extraerse a un microservicio** sin reescribir el resto.
- Señales para migrar a servicios: múltiples clientes con necesidades de escalado independiente, equipos grandes, despliegue independiente de módulos.

### 🔑 GIRO IMPORTANTE DEL PRODUCTO — SaaS Multi-Tenant (2026-09-26)
Para que Chiron sea un producto **rentable a múltiples veterinarias**, se adopta un diseño **multi-tenant** desde el núcleo:

- **Tenant = Veterinaria**: cada veterinaria que renta el software es un "inquilino".
- **Aislamiento de datos**: los datos de una veterinaria NUNCA se mezclan ni son visibles para otra. Casi todas las entidades llevan `VeterinariaId`.
- **Usuarios y Roles**: dentro de cada veterinaria hay usuarios que operan el sistema, con roles:
  - `Administrador` — dueño/gerente, ve todo incluidos reportes de dinero.
  - `Veterinario` — atiende, ve/edita expedientes clínicos.
  - `Recepcionista` — agenda citas, registra clientes.
- **Nota sobre autenticación**: la entidad Usuario se modela ahora (datos + rol), pero el login/contraseñas seguras (autenticación real) es un tema aparte que va con la API/frontend (Épica 8). Por ahora NO hay login funcional.

Este giro es la columna vertebral del modelo de negocio (rentar a muchas veterinarias con una sola instancia).

---

## Metodología de trabajo

- **Kanban/Scrum** con backlog priorizado y sprints.
- **Git Flow simplificado (GitHub Flow)**: `main` + ramas `feature/*` por historia.
- **Commit por historia de usuario** (atómico).
- **Conventional Commits** como nomenclatura.
- **Documentación viva en `.md`** para mantener contexto entre sesiones.
- ⚠️ **Regla de oro**: no se hace `commit`, `push` ni `merge` sin confirmación explícita del usuario.

Ver detalles en `METODOLOGIA.md`.

---

## Repositorio

- **Remoto**: https://github.com/JonYDM/chiron-vet.git
- **Carpeta local**: `C:\Users\jonyo\chiron`

---

## Estado actual

- **Fase**: Backend COMPLETO y desplegado en producción (Railway + PostgreSQL). Siguiente: **frontend**.
- **Backend en producción**: https://chiron-vet-production.up.railway.app
- **Épicas completadas**: 1 (fundación), 1.5 (multi-tenant), 2 (clientes/mascotas), 3 (expediente), 4 (citas), 5 (recordatorios), 6 (punto de venta), 7 (PostgreSQL/Docker), 8.1 (API REST), 9 (seguridad: JWT/roles/SuperAdmin), 9.5 (gestión de usuarios), 10.1-10.2 (rol dueño + recordatorios in-app).
- **Siguiente**: Frontend en repo separado `chiron-web` (React + Vite + PWA). Ver `FRONTEND.md`.
- **Pendientes backend**: CORS para el frontend, H10.3 (push web, requiere frontend), issue menor de Swagger UI, DataProtection persistente (mejora).
- Ver progreso detallado en `BACKLOG.md` y `BITACORA.md`.

---

## Datos del contexto de negocio

- El usuario (JonYDM / Jonathan Ocampo) es de **Temixco, Morelos**.
- Ya validó interés con **al menos un dueño de veterinaria**.
- Observación clave del mercado LATAM: **los clientes compran cuando VEN el sistema funcionando** → prioridad en demos funcionales.
- **Precio objetivo**: ~$199-349 MXN/mes por veterinaria (rentable desde el primer cliente; infraestructura ~$100 MXN/mes para todas juntas).
- **Autenticación**: usuario/teléfono + PIN de 6 dígitos (sin correo — decisión por fricción en LATAM).
