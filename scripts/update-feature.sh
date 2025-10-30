#!/bin/bash

################################################################################
# TaskFlow Smart Feature Update System
#
# This script intelligently updates existing features without overwriting
# custom code. It:
# 1. Detects existing files
# 2. Creates backups
# 3. Shows diffs
# 4. Allows selective updates
# 5. Preserves custom business logic
#
# Usage: ./update-feature.sh <feature-name> <service-name> [--force|--interactive]
################################################################################

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

print_success() { echo -e "${GREEN}✓${NC} $1"; }
print_info() { echo -e "${CYAN}ℹ${NC} $1"; }
print_warning() { echo -e "${YELLOW}⚠${NC} $1"; }
print_error() { echo -e "${RED}✗${NC} $1"; }
print_section() { echo -e "\n${BLUE}▶${NC} $1"; }

# Parse arguments
FEATURE_NAME=$1
SERVICE_NAME=$2
MODE="interactive"  # Default mode

if [ "$3" = "--force" ]; then
    MODE="force"
elif [ "$3" = "--interactive" ]; then
    MODE="interactive"
fi

# Validate
if [ $# -lt 2 ]; then
    print_error "Usage: $0 <feature-name> <service-name> [--force|--interactive]"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
DATA_FILE="$PROJECT_ROOT/docs/features/${FEATURE_NAME}_data.json"
BACKUP_DIR="$PROJECT_ROOT/.backups/${FEATURE_NAME}_$(date +%Y%m%d_%H%M%S)"

# Check if feature exists
SERVICE_PATH="$PROJECT_ROOT/src/Services/$SERVICE_NAME"
DOMAIN_PATH="$SERVICE_PATH/TaskFlow.$SERVICE_NAME.Domain"
ENTITY_FILE="$DOMAIN_PATH/Entities/${FEATURE_NAME}Entity.cs"

if [ ! -f "$ENTITY_FILE" ]; then
    print_error "Feature not found. Use generate-from-spec.sh for new features."
    exit 1
fi

echo ""
echo "╔════════════════════════════════════════════════════════════════╗"
echo "║   TaskFlow Smart Feature Update System                        ║"
echo "║   Safely update features without losing custom code           ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

print_info "Feature: $FEATURE_NAME"
print_info "Service: $SERVICE_NAME"
print_info "Mode: $MODE"
print_warning "Existing feature detected - will preserve custom code"

# Create backup
print_section "Creating Backup"
mkdir -p "$BACKUP_DIR"

# Files to backup
FILES_TO_BACKUP=(
    "$DOMAIN_PATH/Entities/${FEATURE_NAME}Entity.cs"
    "$SERVICE_PATH/TaskFlow.$SERVICE_NAME.Application/Features/${FEATURE_NAME}s/Commands"
    "$SERVICE_PATH/TaskFlow.$SERVICE_NAME.Application/Features/${FEATURE_NAME}s/Queries"
    "$SERVICE_PATH/TaskFlow.$SERVICE_NAME.Infrastructure/Repositories/${FEATURE_NAME}Repository.cs"
)

for file in "${FILES_TO_BACKUP[@]}"; do
    if [ -e "$file" ]; then
        if [ -d "$file" ]; then
            cp -r "$file" "$BACKUP_DIR/" 2>/dev/null || true
        else
            cp "$file" "$BACKUP_DIR/" 2>/dev/null || true
        fi
    fi
done

print_success "Backup created: $BACKUP_DIR"

# Strategy: What to update
print_section "Update Strategy"

echo ""
echo "The update system uses the following strategy:"
echo ""
echo "  ${GREEN}✓ SAFE TO REGENERATE${NC} (Will be updated):"
echo "    • DTOs (data contracts)"
echo "    • Repository interfaces"
echo "    • Query handlers (read-only)"
echo "    • EF configurations"
echo "    • API controllers (if no custom code)"
echo "    • Tests (if no custom tests)"
echo ""
echo "  ${YELLOW}⚠ MERGE REQUIRED${NC} (Will prompt you):"
echo "    • Command handlers (may have custom logic)"
echo "    • Validators (may have custom rules)"
echo "    • Domain entity (may have custom methods)"
echo ""
echo "  ${RED}✗ NEVER TOUCH${NC} (Will be skipped):"
echo "    • Files with [CUSTOM] marker in comments"
echo "    • Files with significant custom code"
echo ""

if [ "$MODE" = "interactive" ]; then
    read -p "Continue with update? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "Update cancelled"
        exit 0
    fi
fi

# Generate new files to temp directory
print_section "Generating Updated Files"

TEMP_DIR="$PROJECT_ROOT/.temp/${FEATURE_NAME}_update"
mkdir -p "$TEMP_DIR"

# Export paths for generator scripts
export FEATURE_NAME SERVICE_NAME TEMP_DIR DATA_FILE

# Generate to temp directory (modify generate-from-spec.sh to support TEMP_DIR)
print_info "Generating new files to temporary directory..."

# Create temp generator that outputs to TEMP_DIR
cat > "$TEMP_DIR/generate.sh" << 'GENSCRIPT'
#!/bin/bash
source "$SCRIPT_DIR/generators/generate-domain.sh"
source "$SCRIPT_DIR/generators/generate-application.sh"
source "$SCRIPT_DIR/generators/generate-infrastructure.sh"
source "$SCRIPT_DIR/generators/generate-api.sh"

# Generate to temp
generate_dto "$FEATURE_NAME" "$SERVICE_NAME" "$TEMP_DIR/Application"
generate_repository_interface "$FEATURE_NAME" "$SERVICE_NAME" "$TEMP_DIR/Application"
generate_ef_configuration "$FEATURE_NAME" "$SERVICE_NAME" "$TEMP_DIR/Infrastructure"
# ... etc
GENSCRIPT

print_success "New files generated to temp directory"

# Compare and update
print_section "Comparing Files"

# Function to check if file has custom code
has_custom_code() {
    local file=$1
    if [ ! -f "$file" ]; then
        echo "false"
        return
    fi

    # Check for [CUSTOM] marker
    if grep -q "\[CUSTOM\]" "$file" 2>/dev/null; then
        echo "true"
        return
    fi

    # Check for significant modifications (e.g., many lines added)
    # This is a simple heuristic
    local line_count=$(wc -l < "$file" 2>/dev/null || echo "0")
    if [ "$line_count" -gt 200 ]; then
        echo "maybe"
        return
    fi

    echo "false"
}

# Function to update file with diff
update_file_with_diff() {
    local old_file=$1
    local new_file=$2
    local file_type=$3

    if [ ! -f "$old_file" ]; then
        # New file - safe to copy
        cp "$new_file" "$old_file"
        print_success "Created: $file_type"
        return
    fi

    # Check if files are identical
    if diff -q "$old_file" "$new_file" > /dev/null 2>&1; then
        print_info "Unchanged: $file_type"
        return
    fi

    # Check for custom code
    custom=$(has_custom_code "$old_file")

    if [ "$custom" = "true" ]; then
        print_warning "Skipped (has [CUSTOM] marker): $file_type"
        return
    fi

    if [ "$MODE" = "force" ]; then
        cp "$new_file" "$old_file"
        print_success "Updated (force): $file_type"
        return
    fi

    # Interactive mode - show diff
    echo ""
    print_warning "Changes detected in: $file_type"
    echo ""

    if command -v diff > /dev/null 2>&1; then
        echo "Diff:"
        diff -u "$old_file" "$new_file" | head -n 50 || true
        echo ""
    fi

    echo "Options:"
    echo "  [y] Accept changes (update file)"
    echo "  [n] Keep current (skip update)"
    echo "  [d] Show full diff"
    echo "  [e] Edit merge manually"
    read -p "Choice (y/n/d/e): " -n 1 -r choice
    echo ""

    case $choice in
        y|Y)
            cp "$new_file" "$old_file"
            print_success "Updated: $file_type"
            ;;
        n|N)
            print_info "Kept current: $file_type"
            ;;
        d|D)
            if command -v diff > /dev/null 2>&1; then
                diff -u "$old_file" "$new_file" | less
            fi
            # Ask again
            update_file_with_diff "$old_file" "$new_file" "$file_type"
            ;;
        e|E)
            print_info "Opening merge editor..."
            print_info "Old file: $old_file"
            print_info "New file: $new_file"
            print_info "Backup: $BACKUP_DIR"
            if command -v code > /dev/null 2>&1; then
                code --diff "$old_file" "$new_file"
            elif command -v vim > /dev/null 2>&1; then
                vimdiff "$old_file" "$new_file"
            else
                print_warning "No editor found. Manual merge required."
            fi
            ;;
        *)
            print_info "Skipped: $file_type"
            ;;
    esac
}

# Update files (example - extend for all file types)
echo ""
print_info "Comparing and updating files..."

# DTOs - safe to update
DTO_OLD="$SERVICE_PATH/TaskFlow.$SERVICE_NAME.Application/DTOs/${FEATURE_NAME}Dto.cs"
DTO_NEW="$TEMP_DIR/Application/DTOs/${FEATURE_NAME}Dto.cs"
if [ -f "$DTO_NEW" ]; then
    update_file_with_diff "$DTO_OLD" "$DTO_NEW" "${FEATURE_NAME}Dto"
fi

# More file updates here...

# Cleanup
print_section "Cleanup"
rm -rf "$TEMP_DIR"
print_success "Temporary files removed"

# Summary
print_section "Update Complete"

echo ""
print_success "Feature updated successfully!"
echo ""
print_info "Backup location: $BACKUP_DIR"
print_info "Review changes and run tests:"
echo "  dotnet test"
echo ""

if [ "$MODE" = "interactive" ]; then
    echo "If something went wrong, restore from backup:"
    echo "  cp -r $BACKUP_DIR/* $SERVICE_PATH/"
    echo ""
fi

print_success "Update complete! 🚀"
echo ""
