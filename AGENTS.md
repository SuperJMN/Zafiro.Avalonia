# Agent Notes for Zafiro.Avalonia

## GitVersion Pull Request Workflow
When working on features in repositories that use GitVersion (like this one), the default workflow is:

1. Crear rama para feature
2. Implementar
3. Subir cambios
4. Push a master (uno o varios commits, según sea el flujo)
5. Crear PR con mensaje explicativo (sin mucha bullshit), la idea global y detalles realmente importantes que necesitemos tener en cuenta en el futuro
6. Esperar a que pase el CI
7. Squash merge del la PR usando semver de GitVersion: El mensaje del commit de squash merge llevará al final la coletilla `+semver:[major|minor|fix]` para que GitVersion aumente la versión como corresponda.
