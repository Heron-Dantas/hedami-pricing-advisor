#!/usr/bin/env bash
# Run in the owner's normal terminal. Never enable shell tracing for credential operations.
set -euo pipefail
cd "$(dirname "$0")/.."
project_root="$(pwd -P)"
visibility="${1:-}"
case "$visibility" in
  private|public) ;;
  *) printf '%s\n' 'Usage: bash scripts/publish-new-github.sh private|public'; exit 2 ;;
esac
command -v gh >/dev/null
command -v git >/dev/null
command -v python3 >/dev/null
if ! gh auth status --hostname github.com >/dev/null 2>&1; then
  printf '%s\n' 'Authenticate through your browser. Do not paste tokens into chat.'
  gh auth login --hostname github.com --git-protocol https --web
fi
github_owner="$(gh api user --jq .login)"
if [[ ! "$github_owner" =~ ^[A-Za-z0-9-]+$ ]]; then
  printf '%s\n' 'Could not determine the authenticated GitHub account.' >&2; exit 1
fi
repo_name="$github_owner/hedami-pricing-advisor"
printf 'Destination: https://github.com/%s (%s)\n' "$repo_name" "$visibility"
if gh repo view "$repo_name" --json nameWithOwner >/dev/null 2>&1; then
  printf '%s\n' 'That repository already exists. Stopped without changing it; inspect its history before publishing.' >&2; exit 1
fi
if git rev-parse --show-toplevel >/dev/null 2>&1; then
  actual_root="$(git rev-parse --show-toplevel)"
  if [[ "$actual_root" != "$project_root" ]]; then
    printf '%s\n' 'The project is inside another Git repository. Stopped.' >&2; exit 1
  fi
  if git rev-parse --verify HEAD >/dev/null 2>&1; then
    printf '%s\n' 'Local commits already exist. Stopped to preserve existing history; use a reviewed existing-repository workflow.' >&2; exit 1
  fi
  if git remote get-url origin >/dev/null 2>&1; then
    printf '%s\n' 'An origin remote already exists. Stopped without replacing it.' >&2; exit 1
  fi
else
  git init --initial-branch=main
fi
commit_name="$(git config user.name || true)"
commit_email="$(git config user.email || true)"
if [[ -z "$commit_name" ]]; then
  read -r -p 'Commit author name [Heron Dantas]: ' commit_name
  commit_name="${commit_name:-Heron Dantas}"
  git config --local user.name "$commit_name"
fi
if [[ -z "$commit_email" ]]; then
  read -r -p 'Confirmed commit email (you may use your GitHub no-reply address): ' commit_email
  if [[ ! "$commit_email" =~ ^[^[:space:]@]+@[^[:space:]@]+$ ]]; then
    printf '%s\n' 'A valid confirmed email is required. No remote repository was created.' >&2; exit 1
  fi
  git config --local user.email "$commit_email"
fi
# Inspect actual Git candidates, including accidental tracked secrets, before committing.
python3 scripts/check-publication.py
if ! git check-ignore --quiet .env; then
  printf '%s\n' '.env is not safely ignored. Stopped before staging or publication.' >&2; exit 1
fi
git add --all -- .
python3 scripts/check-publication.py
git commit -m 'Initial HEDAMI Pricing Advisor console prototype and evidence'
# Create an empty repository: no remote README/license/history is overwritten.
gh repo create "$repo_name" "--$visibility" --description 'Inspectable .NET pricing advisor with deterministic calculations, bounded agent loops and evaluation evidence.'
git remote add origin "https://github.com/$repo_name.git"
# Use gh as the credential helper for this invocation only; no global helper rewrite.
git -c credential.helper= -c 'credential.helper=!gh auth git-credential' push --set-upstream origin HEAD:main
printf '\nPublished repository: https://github.com/%s\n' "$repo_name"
gh repo view "$repo_name" --json url,isPrivate,defaultBranchRef
