# Persistent local access for this project

This setup addresses the observed Codex CLI 0.164.0 session restrictions: workspace .git is read-only and command networking is disabled. It does not change application permissions, marketplace capabilities or the authorized OpenAI spend budget.

The reviewed template is `config/codex/hedami.config.toml`. It extends the built-in workspace permission profile, enables Git metadata writes in active workspaces and activates a network proxy with selected GitHub/OpenAI/NuGet and local-development destinations. Other protected workspace metadata remains inherited. The local .env is read-only to sandboxed commands, allowing the application to load it without making it writable. The allowlist applies to sandboxed command networking, not every connector or browser surface.

## One-time activation by the owner

Run outside the current sandbox, in your regular terminal:

```sh
mkdir -p "$HOME/.codex"
cp -n config/codex/hedami.config.toml "$HOME/.codex/hedami.config.toml"
```

The no-clobber copy preserves an existing file. If a profile already exists, inspect and merge intentionally. The destination was absent during the initial inspection. This does not overwrite the base `~/.codex/config.toml`.

Exit the current Codex CLI interaction normally, then resume from the project root:

```sh
codex resume --last --profile hedami --ask-for-approval on-request --strict-config
```

The installed CLI help documents `--profile` as a user-level `<name>.config.toml` layer and supports these resume options. The configuration is TOML-validated locally, but effective runtime permissions can only be confirmed after activation. Use `codex --profile hedami` for future new sessions. To make it your standard starting point, use that command in your normal launcher or shell shortcut rather than copying the policy into every repository.

Do not add `--sandbox` or a `sandbox_mode` setting to this profile: current permission profiles and legacy sandbox configuration are alternative systems. A legacy setting in another loaded layer can take precedence. Administrator/launcher-enforced policy can also restrict available profiles; if activation is rejected, preserve the exact error and adjust the higher-level policy through its owner rather than bypassing it.

## Confirm after resuming

Ask the agent to verify effective filesystem/network permissions and inspect GitHub access. Normal checks include network access to GitHub, `gh auth status`, and creating/reading the repository's Git metadata only within the intended task. Revisit any remaining standard-test socket restrictions separately; the local-host allowlist alone is not a claim that every IPC restriction has been removed.

GitHub authentication remains a separate prerequisite. Check `gh auth status` in the regular terminal. Only if it fails there, authenticate through `gh auth login --hostname github.com --git-protocol https --web`. Never paste a token into chat. Local commit identity still requires a confirmed name/email when absent.

The current session's failed `gh auth status` is inconclusive about credential validity because DNS resolution was independently unavailable. Profile activation grants no GitHub account permissions by itself. The repository visibility choice also remains separate from network/filesystem authorization.

## Sources

- Official permission profiles: https://learn.chatgpt.com/docs/permissions
- Official sandbox controls and CLI /permissions: https://learn.chatgpt.com/docs/sandboxing
- Installed `codex --help` and `codex resume --help` inspected locally (0.164.0).

No access policy was installed outside the workspace by the agent, no new repository was created, and no push was attempted as part of preparing this template.
