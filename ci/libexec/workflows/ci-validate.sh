#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Validate the project's source, e.g. run tests, linting.
#
#   This script expects a CICEE CI library environment (which is provided when using 'cicee lib exec').
#   For CI library environment details, see: https://github.com/JeremiahSanders/cicee/blob/main/docs/use/ci-library.md
#
#   Workflow:
#   - Restore dependencies.
#   - Build all solution projects using 'Debug' configuration.
#   - Test all solution test projects.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

function ci-validate() {
  printf "Beginning validation...\n\n" &&
    dotnet restore "${PROJECT_ROOT}" &&
    dotnet build "${PROJECT_ROOT}" \
      --configuration Debug \
      -p:Version="${PROJECT_VERSION_DIST}" &&
    dotnet test "${PROJECT_ROOT}" &&
    printf "Validation complete!\n\n"
}

export -f ci-validate
