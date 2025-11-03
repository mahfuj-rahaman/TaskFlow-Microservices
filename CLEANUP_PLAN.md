# Project Cleanup Plan

**Date**: 2025-11-03
**Reason**: Many files are outdated, redundant, or no longer relevant after major refactoring

---

## Files to KEEP ✅

### Root Documentation (Essential)
1. **README.md** - Main project overview (KEEP, UPDATE)
2. **CLAUDE.md** - AI context file (KEEP, UPDATE)
3. **PRE_CODING_GAP_ANALYSIS.md** - Recent comprehensive analysis (KEEP)
4. **API_GATEWAY_CONFIG_SUMMARY.md** - Recent API Gateway guide (KEEP)
5. **CICD_IMPLEMENTATION_SUMMARY.md** - Recent CI/CD guide (KEEP)

### Docs Directory (Essential)
6. **docs/API_GATEWAY_CONFIGURATION.md** - Complete API Gateway reference (KEEP)
7. **docs/CICD_SECRETS_MANAGEMENT.md** - Complete CI/CD reference (KEEP)
8. **docs/CODE_GENERATION_SYSTEM.md** - Code generation guide (KEEP)
9. **docs/AI_SCAFFOLDING_GUIDE.md** - Scaffolding guide (KEEP)
10. **docs/FEATURE_UPDATE_GUIDE.md** - Update guide (KEEP)
11. **docs/UPDATE_PARADOX_SOLVED.md** - Update paradox solution (KEEP)

### BuildingBlocks Documentation (Keep select files)
12. **docs/buildingblocks/QUICK_SWITCH_GUIDE.md** - Quick reference (KEEP)
13. **docs/buildingblocks/VENDOR_LOCK_IN_FREE_SUMMARY.md** - Summary (KEEP)

### Feature Specifications (Keep JSON only)
14. **docs/features/Identity_data.json** - (KEEP)
15. **docs/features/User_data.json** - (KEEP)
16. **docs/features/Task_data.json** - (KEEP)
17. **docs/features/Notification_data.json** - (KEEP)
18. **docs/features/AdminUser_data.json** - (KEEP)
19. **docs/features/AppUser_data.json** - (KEEP)
20. **docs/features/Product_data.json** - (KEEP)
21. **docs/features/Identity_feature_example.md** - Example (KEEP)

### Scripts (Essential)
22. **scripts/ai-scaffold.sh** - (KEEP)
23. **scripts/generate-from-spec.sh** - (KEEP)
24. **scripts/update-feature.sh** - (KEEP)
25. **scripts/deploy-with-secrets.sh** - (KEEP)
26. **scripts/deploy-aws.sh** - (KEEP)
27. **scripts/common-functions.sh** - (KEEP)
28. **scripts/generators/*.sh** - All 6 generator scripts (KEEP)
29. **run-gateway.sh** - (KEEP)

---

## Files to DELETE ❌

### Outdated/Redundant Root Documentation

1. ❌ **COMPLETE_SYSTEM_SUMMARY.md**
   - **Reason**: Replaced by PRE_CODING_GAP_ANALYSIS.md (more comprehensive and current)
   - **Lines**: 900+
   - **Status**: Outdated

2. ❌ **PROJECT_STATUS.md**
   - **Reason**: Information now in PRE_CODING_GAP_ANALYSIS.md
   - **Status**: Outdated

3. ❌ **QUICKSTART_CODE_GENERATION.md**
   - **Reason**: Information integrated into docs/CODE_GENERATION_SYSTEM.md and docs/AI_SCAFFOLDING_GUIDE.md
   - **Status**: Redundant

4. ❌ **SCAFFOLDING_SYSTEM.md**
   - **Reason**: Same as above, redundant with better docs
   - **Status**: Redundant

5. ❌ **MIGRATION_GUIDE.md**
   - **Reason**: Migration from PowerShell → Shell completed, no longer needed
   - **Status**: Obsolete

6. ❌ **DOCKER.md**
   - **Reason**: Docker info now in CI/CD docs
   - **Status**: Redundant

7. ❌ **DOCKER-FILES.md**
   - **Reason**: Redundant
   - **Status**: Redundant

8. ❌ **DOCKER-QUICKSTART.md**
   - **Reason**: Redundant
   - **Status**: Redundant

9. ❌ **DOCKER-TEST.md**
   - **Reason**: Redundant
   - **Status**: Redundant

10. ❌ **GEMINI.md**
    - **Reason**: Not using Gemini anymore (only Claude)
    - **Status**: Unused

### Redundant BuildingBlocks Documentation

11. ❌ **docs/buildingblocks/BUILDINGBLOCKS_ROADMAP.md**
    - **Reason**: BuildingBlocks are complete, roadmap no longer needed
    - **Status**: Obsolete

12. ❌ **docs/buildingblocks/CACHING_PROVIDER_SWITCHING_GUIDE.md**
    - **Reason**: Covered in QUICK_SWITCH_GUIDE.md
    - **Status**: Redundant

13. ❌ **docs/buildingblocks/CACHING_QUICK_SWITCH.md**
    - **Reason**: Same as above
    - **Status**: Redundant

14. ❌ **docs/buildingblocks/MESSAGING_PROVIDER_SWITCHING_GUIDE.md**
    - **Reason**: Covered in QUICK_SWITCH_GUIDE.md
    - **Status**: Redundant

15. ❌ **docs/buildingblocks/SERVICE_BUS_ABSTRACTION_GUIDE.md**
    - **Reason**: Covered in QUICK_SWITCH_GUIDE.md
    - **Status**: Redundant

16. ❌ **docs/buildingblocks/SWITCHING_PROVIDERS_EXAMPLES.md**
    - **Reason**: Examples in QUICK_SWITCH_GUIDE.md
    - **Status**: Redundant

17. ❌ **docs/buildingblocks/Caching_Specification.md**
    - **Reason**: Specifications were for planning, BuildingBlock is now complete
    - **Status**: Obsolete

18. ❌ **docs/buildingblocks/Common_Specification.md**
    - **Reason**: Same as above
    - **Status**: Obsolete

19. ❌ **docs/buildingblocks/EventBus_Specification.md**
    - **Reason**: Same as above
    - **Status**: Obsolete

20. ❌ **docs/buildingblocks/Messaging_Specification.md**
    - **Reason**: Same as above
    - **Status**: Obsolete

### Redundant Feature Specifications (Markdown versions)

21. ❌ **docs/features/AdminUser_complete_specification.md**
    - **Reason**: We have AdminUser_data.json (used by generators)
    - **Status**: Redundant (keep JSON only)

22. ❌ **docs/features/Notification_complete_specification.md**
    - **Reason**: We have Notification_data.json
    - **Status**: Redundant

23. ❌ **docs/features/Task_complete_specification.md**
    - **Reason**: We have Task_data.json
    - **Status**: Redundant

24. ❌ **docs/features/User_complete_specification.md**
    - **Reason**: We have User_data.json
    - **Status**: Redundant

25. ❌ **docs/features/Identity_complete_specification.md**
    - **Reason**: We have Identity_data.json
    - **Status**: Redundant

26. ❌ **docs/features/AppUser_feature.md**
    - **Reason**: We have AppUser_data.json
    - **Status**: Redundant

### Obsolete Scripts

27. ❌ **scripts/cleanup-old-files.sh**
    - **Reason**: Was used once for migration cleanup, no longer needed
    - **Status**: Obsolete

---

## Summary

### Files to Keep: 29 files
- 5 root documentation files
- 6 docs/ files
- 2 buildingblocks docs
- 8 feature specifications (7 JSON + 1 example MD)
- 8 scripts

### Files to Delete: 27 files
- 10 outdated root documentation
- 10 redundant buildingblocks docs
- 6 redundant feature specifications
- 1 obsolete script

### Space Saved
- Estimated: ~500KB of markdown documentation
- Cleaner, more maintainable codebase
- Less confusion about which docs to read

---

## Recommended File Structure After Cleanup

```
TaskFlow-Microservices/
├── README.md                                    # Project overview
├── CLAUDE.md                                    # AI context
├── PRE_CODING_GAP_ANALYSIS.md                  # Current project status
├── API_GATEWAY_CONFIG_SUMMARY.md               # API Gateway quick ref
├── CICD_IMPLEMENTATION_SUMMARY.md              # CI/CD quick ref
│
├── docs/
│   ├── API_GATEWAY_CONFIGURATION.md            # Complete API Gateway guide
│   ├── CICD_SECRETS_MANAGEMENT.md              # Complete CI/CD guide
│   ├── CODE_GENERATION_SYSTEM.md               # Code generation guide
│   ├── AI_SCAFFOLDING_GUIDE.md                 # Scaffolding guide
│   ├── FEATURE_UPDATE_GUIDE.md                 # Update guide
│   ├── UPDATE_PARADOX_SOLVED.md                # Update paradox solution
│   │
│   ├── buildingblocks/
│   │   ├── QUICK_SWITCH_GUIDE.md               # Provider switching
│   │   └── VENDOR_LOCK_IN_FREE_SUMMARY.md      # Summary
│   │
│   └── features/
│       ├── Identity_data.json                  # Feature specs (JSON only)
│       ├── User_data.json
│       ├── Task_data.json
│       ├── Notification_data.json
│       ├── AdminUser_data.json
│       ├── AppUser_data.json
│       ├── Product_data.json
│       └── Identity_feature_example.md         # Example
│
└── scripts/
    ├── ai-scaffold.sh                          # Core scripts
    ├── generate-from-spec.sh
    ├── update-feature.sh
    ├── deploy-with-secrets.sh
    ├── deploy-aws.sh
    ├── common-functions.sh
    └── generators/
        ├── generate-domain.sh                  # Generator scripts
        ├── generate-application.sh
        ├── generate-infrastructure.sh
        ├── generate-api.sh
        ├── generate-tests.sh
        └── generate-projects.sh
```

---

## Execution Plan

1. **Backup** (create git branch)
   ```bash
   git checkout -b cleanup-backup
   git checkout main
   ```

2. **Delete files** (27 files)
   ```bash
   # Execute deletion commands
   ```

3. **Update README.md** with new structure

4. **Commit cleanup**
   ```bash
   git add .
   git commit -m "chore: Clean up outdated and redundant documentation"
   git push
   ```

---

## Risk Assessment

**Risk Level**: 🟢 Low

**Mitigation**:
- All deletions are of redundant/outdated files
- No code is being deleted
- Git history preserves everything
- Can restore from backup branch if needed

**Benefits**:
- ✅ Clearer documentation structure
- ✅ No confusion about which docs to read
- ✅ Easier maintenance
- ✅ Smaller repository

---

**Status**: 📋 **READY FOR REVIEW**
**Action Required**: Approve and execute cleanup
