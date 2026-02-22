# Agent Notes for Zafiro.Avalonia

## GitVersion Pull Request Workflow
When working on features in repositories that use GitVersion (like this one), the default workflow is:

1. Create a branch for the feature.
2. Implement the feature.
3. Push changes.
4. Push to master (one or more commits, depending on the flow).
5. Create a PR with an explanatory message excluding boilerplate, focusing on the global idea and important future details.
6. Wait for CI to pass.
7. Squash merge the PR using GitVersion semver: The squash merge commit message MUST end with the suffix `+semver:[major|minor|fix]` so GitVersion correctly bumps the version.
