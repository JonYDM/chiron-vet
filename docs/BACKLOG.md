# 🐾 Chiron — Product Backlog

> Estado de cada historia: ⬜ Por hacer · 🟡 En progreso · ✅ Hecho
> Última actualización: 2026-09-26

Cada historia = 1 rama `feature/*` = 1 commit (Conventional Commits, referenciando `[Hx.x]`).

---

## ÉPICA 1 — Fundación técnica
Base del proyecto con Clean Architecture y DI.

| ID | Historia | Estado |
|----|----------|--------|
| H1.1 | Inicializar solución .NET con capas (Domain, Application, Infrastructure, ConsoleApp) + `.gitignore` | ✅ |
| H1.2 | Configurar inyección de dependencias en la app de consola | ✅ |
| H1.3 | Crear contrato de repositorio genérico + repositorio base en memoria (intercambiable) | ✅ |

## ÉPICA 1.5 — Fundación SaaS Multi-Tenant (GIRO DEL PRODUCTO)
Núcleo que permite rentar el software a múltiples veterinarias con aislamiento de datos.

| ID | Historia | Estado |
|----|----------|--------|
| H1.5.1 | Entidad Veterinaria (Tenant) — el inquilino que renta | ✅ |
| H1.5.2 | Entidad Usuario + enum Rol (Administrador, Veterinario, Recepcionista), asociada a Veterinaria | ✅ |
| H1.5.3 | Agregar `VeterinariaId` a entidades para aislamiento multi-tenant | ✅ (Cliente y Mascota) |

## ÉPICA 2 — Gestión de clientes y mascotas (el corazón)
| ID | Historia | Estado |
|----|----------|--------|
| H2.1 | Registrar cliente (dueño) con validaciones | ✅ |
| H2.2 | Registrar mascota asociada a un cliente (relación 1 Cliente → N Mascotas) | ✅ |
| H2.2b | Caso de uso "Registro rápido": alta de cliente + su primera mascota en una operación | ✅ |
| H2.3 | Listar y buscar clientes / mascotas | ✅ |

## ÉPICA 3 — Expediente médico
| ID | Historia | Estado |
|----|----------|--------|
| H3.1 | Registrar consulta médica en el historial de una mascota | ✅ |
| H3.2 | Registrar vacunas/desparasitaciones con fecha de próxima aplicación | ✅ |
| H3.3 | Ver expediente completo de una mascota | ✅ |

## ÉPICA 4 — Citas
| ID | Historia | Estado |
|----|----------|--------|
| H4.1 | Agendar cita para una mascota | ✅ |
| H4.2 | Ver agenda del día / próximas citas | ✅ |

## ÉPICA 5 — Recordatorios (diferenciador)
| ID | Historia | Estado |
|----|----------|--------|
| H5.1 | Detectar vacunas/citas próximas a vencer | ✅ |
| H5.2 | Generar mensajes de recordatorio (base para WhatsApp) | ✅ |

## ÉPICA 6 — Punto de venta
| ID | Historia | Estado |
|----|----------|--------|
| H6.1 | Catálogo de productos (alimento, medicina) | ✅ |
| H6.2 | Registrar venta y cobro | ✅ |

## ÉPICA 7 — Persistencia real
| ID | Historia | Estado |
|----|----------|--------|
| H7.1 | Integrar base de datos PostgreSQL (EF Core) | ✅ |
| H7.2 | Dockerizar (Dockerfile) y preparar despliegue en Railway | ✅ (archivos listos; despliegue pendiente) |

## ÉPICA 8 — API y Frontend
| ID | Historia | Estado |
|----|----------|--------|
| H8.1 | Exponer API REST (ASP.NET Core Web API) | ✅ |
| H8.2 | Interfaz web (celular + computadora) — opciones: React/Vue (aprovecha Node) o Blazor (solo C#). Móvil futuro: .NET MAUI | ⬜ |

## ÉPICA 9 — Seguridad (Autenticación y Autorización) — CRÍTICA
Ver detalle en `ROADMAP-SEGURIDAD-NOTIFICACIONES.md`.
| ID | Historia | Estado |
|----|----------|--------|
| H9.1 | Login + JWT (tokens firmados) y hash seguro de contraseñas | ✅ |
| H9.2 | Roles predefinidos en backend + autorización por endpoint (`[Authorize]`) | ✅ |
| H9.3 | Rol SuperAdmin + endpoints de alta/baja de veterinarias (proteger POST /api/veterinarias) | ✅ |
| H9.4 | Control de suscripción: bloquear acceso a veterinarias desactivadas | ✅ (en Login) |

## ÉPICA 10 — Rol Dueño de mascota + Notificaciones
| ID | Historia | Estado |
|----|----------|--------|
| H10.1 | Rol Dueño de mascota (acceso limitado a sus mascotas e historial) | ✅ |
| H10.2 | Recordatorios in-app (reusa lógica de detección de Épica 5) | ✅ |
| H10.3 | Notificaciones push web (gratis, sin fricción de Meta) | ⬜ (depende del frontend) |

> **Nota de autenticación (cambio en H9.1)**: se usa **identificador + PIN de 6 dígitos** (sin correo):
> staff → nombre de usuario; dueño de mascota → su teléfono. PIN hasheado con BCrypt + bloqueo tras 5 intentos fallidos.

## FUTURO / Bajo demanda
| ID | Historia | Estado |
|----|----------|--------|
| F1 | Integración real de WhatsApp (movido desde H8.3; solo si un cliente lo pide) | 🔵 Futuro |
| F2 | Atención por WhatsApp: webhook + envío de PDF del expediente (ventana 24h) | 🔵 Futuro |

---

## Plan de Sprints

| Sprint | Épicas | Objetivo |
|--------|--------|----------|
| Sprint 1 | Épica 1 | Fundación técnica sólida |
| Sprint 2 | Épica 2 | Clientes y mascotas en consola |
| Sprint 3 | Épica 3 | Expediente médico |
| Sprint 4 | Épica 4 + 5 | Citas y recordatorios |
| Sprint 5 | Épica 6 | Punto de venta |
| Sprint 6+ | Épica 7, 8 | Persistencia, API, frontend, WhatsApp |
