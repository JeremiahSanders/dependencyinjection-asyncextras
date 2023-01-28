#!/usr/bin/env bash
# shellcheck disable=SC2155

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

function ci-regenerate-docs() {
  function reset-directories() {
    if [[ -d "${PROJECT_ROOT}/docs/api" ]]; then
      rm -rf "${PROJECT_ROOT}/docs/api"
    fi
  }

  printf "Beginning document regeneration...\n\n" &&
    reset-directories &&
    mkdir --parents "${PROJECT_ROOT}/docs/api" &&
    ci-compose &&
    cp -R "${BUILD_DOCS}/md/." "${PROJECT_ROOT}/docs/api/" &&
    cp "${PROJECT_ROOT}/README.md" "${PROJECT_ROOT}/src/library/README.md" &&
    printf "Document regeneration complete.\n\n"
}

export -f ci-regenerate-docs
