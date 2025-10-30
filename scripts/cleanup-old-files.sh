#!/bin/bash

################################################################################
# Cleanup Script - Remove Old PowerShell Scripts and Outdated Documentation
################################################################################

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_success() { echo -e "${GREEN}✓${NC} $1"; }
print_info() { echo -e "${YELLOW}ℹ${NC} $1"; }
print_section() { echo -e "\n${GREEN}▶${NC} $1"; }

echo ""
echo "╔════════════════════════════════════════════════════════════════╗"
echo "║   Cleanup Old Files                                            ║"
echo "║   Removing broken PowerShell scripts and outdated docs        ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

# Files to remove
OLD_SCRIPTS=(
    "scripts/scaffold.ps1"
    "scripts/test-script.ps1"
    "scripts/test-heredoc.ps1"
    "scripts/check-quotes.ps1"
    "scripts/count-herestrings.ps1"
    "scripts/find-herestrings.ps1"
    "scripts/find-smart-quotes.ps1"
)

OLD_DOCS=(
    "SCAFFOLDING.md"
    "SCAFFOLDING-EXAMPLE.md"
)

print_section "Removing Old PowerShell Scripts"

for file in "${OLD_SCRIPTS[@]}"; do
    if [ -f "$file" ]; then
        rm "$file"
        print_success "Removed: $file"
    else
        print_info "Not found (already removed): $file"
    fi
done

print_section "Removing Outdated Documentation"

for file in "${OLD_DOCS[@]}"; do
    if [ -f "$file" ]; then
        rm "$file"
        print_success "Removed: $file"
    else
        print_info "Not found (already removed): $file"
    fi
done

print_section "Cleanup Complete!"

echo ""
print_success "Old files removed successfully!"
echo ""
print_info "Remaining scripts:"
ls -1 scripts/*.sh 2>/dev/null | grep -v cleanup || true
echo ""
print_info "Current documentation:"
ls -1 *.md 2>/dev/null || true
echo ""

echo "✨ Your project is now clean and ready!"
echo ""
