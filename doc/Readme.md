# Documentación

> Introduzca sus datos (de todo el equipo) en la siguiente tabla:

**Nombre** | **Grupo** | **Github**
--|--|--
Nombre1 Apellido1 Apellido2 | C4xx | [@github_user](https://github.com/<user>)
Nombre2 Apellido1 Apellido2 | C4xx | [@github_user](https://github.com/<user>)
Nombre3 Apellido1 Apellido2 | C4xx | [@github_user](https://github.com/<user>)

## Readme

Modifique el contenido documento para documentar de forma clara y concisa los siguientes aspectos:

- Cómo ejecutar (y compilar si es necesario) su compilador.
- Requisitos adicionales, dependencias, configuración, etc.
- Opciones adicionales que tenga su compilador.

## Reporte escrito

En esta carpeta ponga además su reporte escrito. Ya sea hecho en LaTeX, Markdown o Word, **además** genere un PDF y póngale nombre `report.pdf`.

El reporte debe resumir de manera organizada y comprensible la arquitectura e implementación de su compilador.
El documento **NO** debe exceder las 5 cuartillas.
En él explicará en más detalle su solución a los problemas que, durante la implementación de cada una de las fases del proceso de compilación, hayan requerido de Ud. especial atención.

El informe debe incluir además una dirección a un repositorio git público con el código fuente de su compilador. Para la evaluación del proyecto, se clonará el repositorio y se procederá a su revisión. El proyecto debe contener un fichero `README.md` con las indicaciones para ejecutar su compilador, y los mecanismos pertinentes para garantizar su correcto funcionamiento en la máquina del revisor (instalación de dependencias, etc.).

### Estructura del reporte

Usted es libre de estructurar su reporte escrito como más conveniente le parezca. A continuación le sugerimos algunas secciones que no deberían faltar, aunque puede mezclar, renombrar y organizarlas de la manera que mejor le parezca:

- **Uso del compilador**: detalles sobre las opciones de líneas de comando, si tiene opciones adicionales (e.j., `--ast` genera un AST en JSON, etc.). Básicamente lo mismo que pondrá en este Readme.
- **Arquitectura del compilador**: una explicación general de la arquitectura, en cuántos módulos se divide el proyecto, cuantas fases tiene, qué tipo de gramática se utiliza, y en general, como se organiza el proyecto. Una buena imagen siempre ayuda.
- **Problemas técnicos**: detalles sobre cualquier problema teórico o técnico interesante que haya necesitado resolver de forma particular.
