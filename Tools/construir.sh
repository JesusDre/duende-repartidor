#!/usr/bin/env bash
# Regenera estructura y escenas del juego en modo batch (con el Editor cerrado).
# Uso: Tools/construir.sh
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p Logs
unity run . -- -executeMethod DuendeBuilder.ConstruirTodo -logFile Logs/construir.log
status=$?
grep -E "error CS|\[DuendeBuilder\]|Exception" Logs/construir.log || true
exit $status
