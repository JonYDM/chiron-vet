# 🐾 Chiron — Metodología de Trabajo

> Última actualización: 2026-09-26

---

## Estrategia de ramas: Git Flow simplificado (GitHub Flow)

```
main            → código estable
feature/*       → una rama por historia de usuario
```

### Flujo por historia
1. Desde `main`, crear rama `feature/Hx.x-descripcion-corta`.
2. Escribir el código de la historia.
3. Probar (compila / corre en consola).
4. Commit con Conventional Commits.
5. **Con permiso del usuario**, merge a `main` (idealmente vía Pull Request en GitHub).
6. Siguiente historia.

> Cuando el proyecto crezca o entren más desarrolladores, se puede migrar a Git Flow completo (`develop`, `release/*`, `hotfix/*`).

---

## Nomenclatura: Conventional Commits

Formato:
```
<tipo>(<alcance>): <descripción corta> [Hx.x]

[cuerpo opcional]
```

### Tipos
| Tipo | Uso |
|------|-----|
| `feat` | Nueva funcionalidad (historia de usuario) |
| `fix` | Corrección de bug |
| `refactor` | Reestructurar sin cambiar comportamiento |
| `test` | Agregar/corregir pruebas |
| `docs` | Documentación |
| `chore` | Configuración, build, dependencias |
| `style` | Formato sin cambio de lógica |

### Ejemplos
```
chore(setup): inicializar solucion con Clean Architecture [H1.1]
feat(domain): agregar entidad Cliente con validaciones [H2.1]
feat(application): agregar caso de uso registrar cliente [H2.1]
test(domain): agregar pruebas de validacion de Cliente [H2.1]
docs(backlog): marcar H2.1 como completada
```

---

## Rama por historia — convención de nombres

```
feature/H1.1-init-solucion
feature/H2.1-registrar-cliente
feature/H3.2-vacunas-recordatorio
```

---

## Regla de oro sobre Git

⚠️ **El asistente NO ejecuta `commit`, `push`, `merge` ni operaciones destructivas sin confirmación explícita del usuario.** Siempre avisa qué va a hacer y espera el "sí".

---

## Definición de "Hecho" (Definition of Done)

Una historia está ✅ cuando:
- [ ] El código compila sin errores.
- [ ] Respeta SOLID, DI y es multiplataforma.
- [ ] Se probó (consola o pruebas automatizadas).
- [ ] Está documentada en `BITACORA.md`.
- [ ] Se hizo commit con Conventional Commits referenciando la historia.
