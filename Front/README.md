# Frontend

Base arquitectónica del frontend de la plataforma de turnos médicos. En esta etapa usa HTML, CSS, JavaScript Vanilla y ES Modules.

## Estructura

```text
src/
├── pages/       # Pantallas organizadas por ruta y rol
│   └── **/_components/ # Componentes privados de una página
├── features/    # Módulos del negocio; no conocen las páginas
├── shared/      # Infraestructura y UI reutilizable entre módulos
└── assets/      # Fuentes, iconos e imágenes estáticas
```

### Reglas de dependencia

- `pages` puede importar desde `features` y `shared`.
- `features` puede importar desde `shared`, pero no desde `pages`.
- `shared` no importa desde `features` ni desde `pages`.
- Un `_components` pertenece a su página. Se mueve a `shared/components` sólo cuando realmente se reutiliza.
- Cada carpeta expone su superficie pública mediante `index.js`; se evitan imports hacia detalles internos de otro módulo.

Esta dirección permite conservar `pages`, `features`, `shared` y `assets` al migrar a React + Vite. En esa etapa los archivos de vista pueden cambiar a `.jsx` sin reubicar los módulos.

## Ejecución local

Servir la carpeta `Front` con cualquier servidor HTTP estático. Los ES Modules no deben abrirse mediante `file://`.

La aplicación actual muestra solamente una pantalla base: no incluye autenticación, API, CRUD, reservas ni estado funcional.
