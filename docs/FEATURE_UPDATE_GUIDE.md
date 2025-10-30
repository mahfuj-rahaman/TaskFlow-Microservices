# 🔄 Feature Update Guide: Solving the Paradox

## 🤔 The Update Paradox

**The Problem:**
You generate a feature, add custom business logic, then need to update it later. How do you add new properties without losing your custom code?

```bash
# Generate feature
./scripts/generate-from-spec.sh Product Catalog
# ✅ 26+ files created

# You add custom code:
# - Custom validation in ProductEntity
# - Custom business rules
# - Custom API endpoints
# - Custom tests

# Later, you need to add "Category" property
./scripts/ai-scaffold.sh Product Catalog  # Update spec
./scripts/generate-from-spec.sh Product Catalog  # Regenerate

# ❌ PROBLEM: All custom code is OVERWRITTEN!
```

---

## ✅ Solution: 3 Approaches

### **Approach 1: Marker-Based Protection** (Recommended)

Mark custom code with `[CUSTOM]` comments:

```csharp
public sealed class ProductEntity : AggregateRoot<Guid>
{
    // Generated properties
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    // [CUSTOM] - Custom property added by developer
    public string InternalCode { get; private set; } = string.Empty;

    public static ProductEntity Create(string name, decimal price)
    {
        // Generated validation
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name required");

        // [CUSTOM] - Custom business rule
        if (price > 10000)
            throw new DomainException("Price exceeds maximum allowed");

        // Generated code continues...
    }

    // [CUSTOM] - Custom method added by developer
    public void ApplyDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new DomainException("Invalid discount");

        Price = Price * (1 - percentage / 100);
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Update command:**
```bash
./scripts/update-feature.sh Product Catalog --interactive
```

**What happens:**
- ✅ Files with `[CUSTOM]` markers are **skipped**
- ✅ You see diffs for other files
- ✅ Choose which files to update
- ✅ Backup created automatically

---

### **Approach 2: Smart Update Script** (Best for Teams)

Use the smart update system:

```bash
# Update with interactive mode (default)
./scripts/update-feature.sh Product Catalog

# The script will:
# 1. Detect existing feature
# 2. Create backup
# 3. Generate new files to temp directory
# 4. Show diffs for each file
# 5. Ask what to do:
#    [y] Update file
#    [n] Keep current
#    [d] Show full diff
#    [e] Open merge editor

# Force mode (overwrites everything)
./scripts/update-feature.sh Product Catalog --force
```

**Update Strategy:**

| File Type | Strategy | Reason |
|-----------|----------|--------|
| DTOs | ✅ Safe to regenerate | Data contracts rarely have custom code |
| Repository Interface | ✅ Safe to regenerate | Interface rarely customized |
| Query Handlers | ✅ Safe to regenerate | Read-only, rarely customized |
| EF Configuration | ✅ Safe to regenerate | Can be fully regenerated |
| Command Handlers | ⚠️ Merge required | Often have custom business logic |
| Validators | ⚠️ Merge required | Often have custom rules |
| Domain Entity | ⚠️ Merge required | Core business logic |
| Controllers | ⚠️ Merge required | May have custom endpoints |
| Tests | ⚠️ Merge required | May have custom test cases |

---

### **Approach 3: Partial Generation** (Manual Control)

Generate only specific layers:

```bash
# Only regenerate DTOs
./scripts/generators/generate-application.sh Product Catalog --dto-only

# Only regenerate repository
./scripts/generators/generate-infrastructure.sh Product Catalog --repo-only

# Only regenerate queries (safe)
./scripts/generators/generate-application.sh Product Catalog --queries-only

# Only add new command
./scripts/generators/add-command.sh ActivateProduct Product Catalog
```

---

## 🎯 Best Practices

### 1. **Use [CUSTOM] Markers**

Always mark your custom code:

```csharp
// [CUSTOM] - Start of custom code
public void MyCustomMethod()
{
    // Your logic
}
// [CUSTOM] - End of custom code
```

### 2. **Separate Custom Logic**

Create separate files for custom logic:

```
Domain/
├── Entities/
│   ├── ProductEntity.cs              ← Generated
│   └── ProductEntity.Custom.cs       ← Your custom code (partial class)
```

```csharp
// ProductEntity.Custom.cs
namespace TaskFlow.Catalog.Domain.Entities;

public sealed partial class ProductEntity
{
    // All your custom methods here
    public void ApplyDiscount(decimal percentage)
    {
        // Custom logic
    }

    public bool IsLowStock() => StockQuantity < 10;
}
```

### 3. **Version Control Strategy**

```bash
# Before regenerating
git add -A
git commit -m "feat: Before updating Product feature"

# Run update
./scripts/update-feature.sh Product Catalog

# Review changes
git diff

# If happy, commit
git add -A
git commit -m "feat: Updated Product with new properties"

# If not happy, rollback
git reset --hard HEAD
```

### 4. **Backup Strategy**

The update script automatically creates backups:

```
.backups/
└── Product_20251030_143022/
    ├── ProductEntity.cs
    ├── Commands/
    └── Queries/
```

Restore if needed:
```bash
cp -r .backups/Product_20251030_143022/* src/Services/Catalog/
```

---

## 🔧 Update Scenarios

### Scenario 1: Add New Property

**Before:**
```json
{
  "properties": [
    {"name": "Name", "type": "string", "required": true},
    {"name": "Price", "type": "decimal", "required": true}
  ]
}
```

**After:**
```json
{
  "properties": [
    {"name": "Name", "type": "string", "required": true},
    {"name": "Price", "type": "decimal", "required": true},
    {"name": "Category", "type": "string", "required": true}  ← New
  ]
}
```

**Update:**
```bash
./scripts/update-feature.sh Product Catalog --interactive
```

**Result:**
- ✅ DTO updated with new property
- ✅ Create/Update commands updated
- ✅ Entity updated (if no custom code)
- ⚠️ You review and merge changes

---

### Scenario 2: Add New Business Rule

**Update specification:**
```markdown
## Business Rules
- Price must be positive
- Stock cannot be negative
- Category must be from predefined list  ← New
```

**Implementation:**
1. Update `Product_data.json`
2. Run update script
3. The validator will be regenerated
4. Merge new validation with existing custom rules

---

### Scenario 3: Add New Operation

**Before:**
```json
{
  "additionalOperations": [
    "UpdateStock"
  ]
}
```

**After:**
```json
{
  "additionalOperations": [
    "UpdateStock",
    "ActivateProduct",    ← New
    "DeactivateProduct"   ← New
  ]
}
```

**Update:**
```bash
./scripts/update-feature.sh Product Catalog
```

**Result:**
- ✅ New commands generated
- ✅ Controller endpoints added
- ✅ Tests generated
- ✅ Existing code untouched

---

## 🛡️ Safety Mechanisms

### 1. **Automatic Backups**

Every update creates a timestamped backup:
```
.backups/Product_20251030_143022/
```

### 2. **Diff Preview**

See changes before applying:
```diff
- public decimal Price { get; private set; }
+ public decimal Price { get; private set; }
+ public string Category { get; private set; }
```

### 3. **Selective Updates**

Choose which files to update:
```
⚠ Changes detected in: ProductDto
Diff: ...

Options:
  [y] Accept changes
  [n] Keep current
  [d] Show full diff
  [e] Edit merge
Choice: y
```

### 4. **Rollback Support**

```bash
# Via Git
git reset --hard HEAD

# Via Backup
cp -r .backups/Product_TIMESTAMP/* src/Services/
```

---

## 📋 Update Checklist

Before updating:
- [ ] Commit current changes to git
- [ ] Review what you're updating
- [ ] Mark custom code with [CUSTOM]
- [ ] Ensure tests pass

During update:
- [ ] Use interactive mode first time
- [ ] Review each diff carefully
- [ ] Test after each merge
- [ ] Keep backup location noted

After update:
- [ ] Run all tests: `dotnet test`
- [ ] Build solution: `dotnet build`
- [ ] Review generated code
- [ ] Update documentation if needed
- [ ] Commit changes with clear message

---

## 🎓 Advanced: Custom Generators

Create your own update-safe generators:

```bash
# scripts/generators/my-custom-generator.sh
#!/bin/bash

generate_custom_feature() {
    local FEATURE=$1
    local SERVICE=$2

    # Check for [CUSTOM] markers
    if grep -q "\[CUSTOM\]" "$existing_file"; then
        echo "Skipping (custom code detected)"
        return
    fi

    # Generate new content
    cat > "$output_file" << EOF
// Auto-generated - safe to regenerate
// To add custom code, use [CUSTOM] markers

public class ${FEATURE}CustomLogic
{
    // [CUSTOM] - Add your custom methods below this line
}
EOF
}
```

---

## 🚨 What NOT to Do

❌ **Don't regenerate without backup:**
```bash
./scripts/generate-from-spec.sh Product Catalog --force
# ❌ Loses all custom code!
```

❌ **Don't modify generated files without markers:**
```csharp
// ❌ No marker - will be lost on regeneration
public void MyMethod() { }

// ✅ Marked - will be preserved
// [CUSTOM]
public void MyMethod() { }
```

❌ **Don't mix generated and custom code:**
```csharp
// ❌ Bad - custom code mixed with generated
public void Update(string name)  // Generated
{
    Name = name;
    UpdatedAt = DateTime.UtcNow;
    SendNotification();  // ❌ Custom code here!
}

// ✅ Good - separate custom method
public void Update(string name)  // Generated
{
    Name = name;
    UpdatedAt = DateTime.UtcNow;
}

// [CUSTOM]
public void UpdateWithNotification(string name)  // Custom
{
    Update(name);
    SendNotification();
}
```

---

## 🎯 Recommended Workflow

### For New Features
```bash
# 1. Generate
./scripts/ai-scaffold.sh Product Catalog
./scripts/generate-from-spec.sh Product Catalog

# 2. Commit generated code
git add -A
git commit -m "feat: Generate Product feature"

# 3. Add custom code with markers
# [CUSTOM] everywhere

# 4. Commit custom code
git add -A
git commit -m "feat: Add custom business logic to Product"
```

### For Updates
```bash
# 1. Commit current state
git add -A
git commit -m "feat: Product feature before update"

# 2. Update specification
./scripts/ai-scaffold.sh Product Catalog

# 3. Run smart update
./scripts/update-feature.sh Product Catalog --interactive

# 4. Review changes
git diff

# 5. Run tests
dotnet test

# 6. Commit or rollback
git add -A
git commit -m "feat: Add Category to Product"
# OR
git reset --hard HEAD  # If not satisfied
```

---

## 📚 Summary

### The Paradox
Generated code vs. Custom code - how to update without losing work?

### The Solution
1. **Mark custom code** with `[CUSTOM]`
2. **Use smart update script** for safe updates
3. **Separate custom logic** with partial classes
4. **Version control** everything
5. **Always backup** before updating

### The Result
✅ Generate features rapidly
✅ Add custom logic safely
✅ Update features without fear
✅ Preserve custom code always

**No more paradox!** 🎉

---

## 🔗 Related Documentation

- **Smart Update Script**: `scripts/update-feature.sh`
- **Code Generation**: `docs/CODE_GENERATION_SYSTEM.md`
- **Best Practices**: `docs/AI_SCAFFOLDING_GUIDE.md`

---

**Remember**: Generated code is a **starting point**, not a prison. The system is designed to help you, not limit you!
