#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#   This workflow performs all steps required to create the project's output.
#
#   This script expects a CICEE CI library environment (which is provided when using 'cicee lib exec').
#   For CI library environment details, see: https://github.com/JeremiahSanders/cicee/blob/main/docs/use/ci-library.md
#
#   Workflow:
#   - Publish an executable from src/library using 'Release' configuration.
#   - Pack a NuGet package from src/library using 'Release' configuration.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

function ci-compose() {
  printf "Composing build artifacts...\n\n" &&
    dotnet publish "${PROJECT_ROOT}/src/library" \
      --configuration Release \
      --output "${BUILD_UNPACKAGED_DIST}" \
      -p:Version="${PROJECT_VERSION_DIST}" \
      -p:GenerateDocumentationFile=true &&
    printf "\nCompiled assembly published to %s\n\n" "${BUILD_UNPACKAGED_DIST}" &&
    dotnet pack "${PROJECT_ROOT}/src/library" \
      --configuration Release \
      --output "${BUILD_PACKAGED_DIST}/nuget/" \
      -p:PackageVersion="${PROJECT_VERSION_DIST}" \
      -p:Version="${PROJECT_VERSION_DIST}" \
      -p:GenerateDocumentationFile=true &&
    printf "\nNuGet package output to %s\n\n" "${BUILD_PACKAGED_DIST}/nuget/" &&
    printf "Composition complete.\n"
}

export -f ci-compose
