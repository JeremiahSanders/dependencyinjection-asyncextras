#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Publish the project's artifact composition.
#   This workflow assumes that, prior to execution, an artifact composition is created. E.g., ci-compose was executed.
#
#   This script expects a CICEE CI library environment (which is provided when using 'cicee lib exec').
#   For CI library environment details, see: https://github.com/JeremiahSanders/cicee/blob/main/docs/use/ci-library.md
#
#   Workflow:
#   - Push all NuGet packages.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

function ci-publish() {
  printf "Publishing composed artifacts...\n\n"

  local nugetOutput="${BUILD_PACKAGED_DIST}/nuget"

  # dotnet nuget push supports wildcard package names. See: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push
  printf "\n  Pushing NuGet packages to '%s' using 'dotnet nuget push'.\n" "${NUGET_SOURCE}" &&
    cd "${nugetOutput}" &&
    dotnet nuget push "*.nupkg" --api-key "${NUGET_API_KEY}" --source "${NUGET_SOURCE}" &&
    printf "\n  Pushed NuGet packages to '%s'.\n" "${NUGET_SOURCE}" &&
    printf "Publishing complete.\n\n"
}

export -f ci-publish
