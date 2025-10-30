# 🔄 Migration Guide: Old → New System

## 📢 Breaking Change Notice

The **old PowerShell scaffolding system has been removed** and replaced with a modern, AI-powered shell script system.

---

## ❌ What Was Removed

### Old Files (Removed)
- ❌ `scripts/scaffold.ps1` - Broken PowerShell script
- ❌ `scripts/test-*.ps1` - Temporary test scripts
- ❌ `scripts/check-*.ps1` - Debug scripts
- ❌ `SCAFFOLDING.md` - Outdated documentation
- ❌ `SCAFFOLDING-EXAMPLE.md` - Old examples

**Reason**: PowerShell script had encoding issues, wasn't cross-platform, and couldn't handle updates.

---

## ✅ What Replaced It

### New System (3 Scripts)

1. **`scripts/ai-scaffold.sh`** - AI-powered specification creator
2. **`scripts/generate-from-spec.sh`** - Complete code generator
3. **`scripts/update-feature.sh`** - Smart update system (NEW!)

**Benefits**:
- ✅ Cross-platform (works on Windows, Mac, Linux)
- ✅ No encoding issues
- ✅ AI-powered requirements gathering
- ✅ **Solves the update paradox** (preserve custom code)
- ✅ Interactive and safe
- ✅ Automatic backups
- ✅ Better documentation

---

## 🔄 Migration Steps

### If You Were Using `scaffold.ps1`

**Old Command:**
```powershell
.\scripts\scaffold.ps1 -EntityPath "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs"
```

**New Commands:**
```bash
# Step 1: Create specification (new!)
./scripts/ai-scaffold.sh User User

# Answer questions about your feature
# Properties, business rules, operations, etc.

# Step 2: Generate code
./scripts/generate-from-spec.sh User User
```

**Key Differences:**
- ❌ Old: You had to create the entity manually first
- ✅ New: AI helps you design it with questions
- ✅ New: Better file organization
- ✅ New: More complete generation (includes tests!)

---

### If You Have Existing Generated Code

**Scenario**: You already generated code with the old script and added custom logic.

**What to do:**

1. **Mark your custom code** with `[CUSTOM]` markers:
   ```csharp
   // [CUSTOM]
   public void MyCustomMethod()
   {
       // Your custom logic
   }
   // [CUSTOM]
   ```

2. **Create specification for existing feature**:
   ```bash
   ./scripts/ai-scaffold.sh User User
   # Describe your existing feature
   ```

3. **Use update script** (not generate):
   ```bash
   ./scripts/update-feature.sh User User --interactive
   ```

4. **Review changes**:
   - The script will preserve your `[CUSTOM]` marked code
   - You'll see diffs for other changes
   - Accept or reject each change

---

## 📊 Feature Comparison

| Feature | Old System | New System |
|---------|------------|------------|
| **Cross-platform** | ❌ Windows only | ✅ All platforms |
| **Encoding issues** | ❌ Yes | ✅ None |
| **Requirements gathering** | ❌ No | ✅ Interactive AI |
| **Documentation** | ❌ Minimal | ✅ Complete |
| **Update support** | ❌ No (overwrites!) | ✅ Smart updates |
| **Custom code protection** | ❌ Lost on regenerate | ✅ Preserved |
| **Backups** | ❌ Manual | ✅ Automatic |
| **Tests generation** | ⚠️ Basic | ✅ Complete |
| **Diffs preview** | ❌ No | ✅ Yes |

---

## 🎯 Quick Migration Examples

### Example 1: Fresh Start

If you haven't generated anything yet:

```bash
# Just use the new system!
./scripts/ai-scaffold.sh Product Catalog
./scripts/generate-from-spec.sh Product Catalog
```

### Example 2: Existing Feature Without Custom Code

If you generated code but didn't add custom logic:

```bash
# Delete old generated files
rm -rf src/Services/User/TaskFlow.User.Application/Features/Users/

# Regenerate with new system
./scripts/ai-scaffold.sh User User
./scripts/generate-from-spec.sh User User
```

### Example 3: Existing Feature With Custom Code

If you have custom business logic:

```bash
# 1. Mark custom code
# Add [CUSTOM] markers around your custom methods

# 2. Create specification
./scripts/ai-scaffold.sh User User
# Describe what you have

# 3. Use update (not generate)
./scripts/update-feature.sh User User --interactive

# 4. Review each change
# The script will show diffs and ask permission
```

---

## 🛡️ Safety First

### Before Migration

```bash
# 1. Commit everything
git add -A
git commit -m "Before migration to new scaffolding system"

# 2. Create a backup branch
git branch backup-before-migration

# 3. Tag the current state
git tag before-migration
```

### During Migration

```bash
# Use interactive mode for safety
./scripts/update-feature.sh User User --interactive

# Review every change
# Don't use --force mode!
```

### After Migration

```bash
# 1. Run tests
dotnet test

# 2. Review changes
git diff

# 3. If happy
git add -A
git commit -m "Migrated to new AI scaffolding system"

# 4. If not happy
git reset --hard HEAD
# Restore from backup-before-migration branch
```

---

## 📚 New Documentation Structure

### Old Documentation (Removed)
- ❌ `SCAFFOLDING.md`
- ❌ `SCAFFOLDING-EXAMPLE.md`

### New Documentation (Current)

**Start Here:**
1. ✅ `QUICKSTART_CODE_GENERATION.md` - 2-minute quick start
2. ✅ `COMPLETE_SYSTEM_SUMMARY.md` - Complete overview

**Deep Dives:**
3. ✅ `docs/CODE_GENERATION_SYSTEM.md` - Full system documentation
4. ✅ `docs/AI_SCAFFOLDING_GUIDE.md` - AI scaffolding guide
5. ✅ `docs/FEATURE_UPDATE_GUIDE.md` - Update guide
6. ✅ `docs/UPDATE_PARADOX_SOLVED.md` - Update paradox solution

**Reference:**
7. ✅ `SCAFFOLDING_SYSTEM.md` - System overview
8. ✅ `docs/features/Identity_feature_example.md` - Real example

---

## 🔧 Troubleshooting

### "I can't find scaffold.ps1"

**Answer**: It was removed. Use the new system:
```bash
./scripts/ai-scaffold.sh <Feature> <Service>
./scripts/generate-from-spec.sh <Feature> <Service>
```

### "My custom code was overwritten!"

**Answer**:
1. Restore from git: `git reset --hard HEAD`
2. Mark custom code with `[CUSTOM]`
3. Use update script: `./scripts/update-feature.sh Feature Service --interactive`

### "The old script worked for me"

**Answer**: The old script had critical issues:
- Encoding problems (couldn't run on many systems)
- No update support (lost custom code on regenerate)
- Windows-only
- Poor error handling

The new system solves all these problems.

### "Can I keep using the old system?"

**Answer**: No, it's been removed because:
1. It was broken (PowerShell encoding issues)
2. It couldn't handle updates (the paradox)
3. The new system is better in every way

---

## 💡 Tips for Smooth Migration

### 1. Start with New Features

Don't migrate everything at once. Use the new system for new features first:

```bash
./scripts/ai-scaffold.sh NewFeature Service
./scripts/generate-from-spec.sh NewFeature Service
```

### 2. Mark Custom Code Everywhere

Before updating existing features:

```csharp
// In ALL files with custom code
// [CUSTOM]
// Your custom code here
// [CUSTOM]
```

### 3. Use Git Branches

```bash
git checkout -b migration-user-feature
./scripts/update-feature.sh User User --interactive
# Test thoroughly
git checkout main
git merge migration-user-feature
```

### 4. One Feature at a Time

Don't migrate all features at once. Do them one by one:

```bash
# Day 1
./scripts/update-feature.sh User User

# Day 2
./scripts/update-feature.sh Product Catalog

# Day 3
./scripts/update-feature.sh Order Order
```

---

## 🎓 Learning the New System

### Quick Start (5 minutes)

```bash
# 1. Read the quick start
cat QUICKSTART_CODE_GENERATION.md

# 2. Generate a test feature
./scripts/ai-scaffold.sh TestProduct Catalog
./scripts/generate-from-spec.sh TestProduct Catalog

# 3. Explore generated files
ls -R src/Services/Catalog/

# 4. Try updating
./scripts/update-feature.sh TestProduct Catalog --interactive

# 5. Delete test feature
rm -rf src/Services/Catalog/TaskFlow.Catalog.*/Features/TestProducts/
```

### Deep Dive (30 minutes)

1. Read `COMPLETE_SYSTEM_SUMMARY.md`
2. Read `docs/UPDATE_PARADOX_SOLVED.md`
3. Review `docs/features/Identity_feature_example.md`
4. Generate a real feature
5. Add custom code with markers
6. Practice updating

---

## 🚀 Benefits of Migrating

### Immediate Benefits

✅ **No more encoding issues** - Works everywhere
✅ **Better error messages** - Clear, actionable
✅ **Automatic backups** - Never lose work
✅ **Interactive updates** - See changes before applying
✅ **Custom code preserved** - [CUSTOM] markers

### Long-term Benefits

✅ **Faster development** - AI-powered specifications
✅ **Consistent quality** - Better templates
✅ **Easier maintenance** - Modular generators
✅ **Team collaboration** - Cross-platform scripts
✅ **Fearless updates** - Update paradox solved

---

## 📞 Need Help?

### Quick References

- **"How do I generate a feature?"**
  → `QUICKSTART_CODE_GENERATION.md`

- **"How do I update without losing custom code?"**
  → `docs/UPDATE_PARADOX_SOLVED.md`

- **"What's the complete system?"**
  → `COMPLETE_SYSTEM_SUMMARY.md`

- **"Show me a real example"**
  → `docs/features/Identity_feature_example.md`

### Still Confused?

Read the docs in this order:
1. `QUICKSTART_CODE_GENERATION.md`
2. `COMPLETE_SYSTEM_SUMMARY.md`
3. `docs/UPDATE_PARADOX_SOLVED.md`

---

## ✅ Migration Checklist

Before you start:
- [ ] Commit all current work
- [ ] Create backup branch
- [ ] Read quick start guide

For each feature:
- [ ] Mark custom code with `[CUSTOM]`
- [ ] Create specification with ai-scaffold.sh
- [ ] Use update-feature.sh (not generate-from-spec.sh)
- [ ] Review diffs carefully
- [ ] Run tests
- [ ] Commit changes

After migration:
- [ ] All features updated
- [ ] All tests passing
- [ ] Documentation reviewed
- [ ] Team trained on new system

---

## 🎉 Welcome to the New System!

**You now have:**
- ✅ AI-powered feature generation
- ✅ Safe updates without losing custom code
- ✅ Cross-platform scripts
- ✅ Complete documentation
- ✅ Automatic backups
- ✅ Interactive mode

**The old system is gone. The new system is better in every way!**

Start generating: `./scripts/ai-scaffold.sh YourFeature YourService` 🚀
