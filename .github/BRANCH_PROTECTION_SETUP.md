# Branch Protection Setup Guide

## Branching Strategy

TaskFlow uses a **three-tier branching strategy** for maximum control and safety:

```
dev (open) → main (protected) → prod (protected)
    ↓            ↓                  ↓
  CI only    CI + Staging      CI + Production
```

### Branch Purposes

| Branch | Purpose | Protection | CI/CD Behavior |
|--------|---------|------------|----------------|
| **dev** | Development, open for contributors | ❌ None | ✅ Build + Test only |
| **main** | Staging/Pre-production | ✅ Protected | ✅ Build + Test + Deploy to Staging |
| **prod** | Production | ✅ Protected | ✅ Build + Test + Deploy to Production |

---

## Quick Setup

### Step 1: Create Branches

```bash
# Ensure you're on main
git checkout main
git pull

# Create prod branch from main
git checkout -b prod
git push -u origin prod

# Create dev branch from main
git checkout -b dev
git push -u origin dev

# Go back to main
git checkout main
```

### Step 2: Set Default Branch

1. Go to: `https://github.com/{owner}/{repo}/settings`
2. Under **Default branch**, click **Switch to another branch**
3. Select **dev**
4. Click **Update**

**Why dev as default?**
- Contributors fork and create PRs against dev by default
- Prevents accidental PRs to main or prod
- Lower risk for new contributors

### Step 3: Protect main Branch

1. Go to: `Settings → Branches → Add branch protection rule`
2. Branch name pattern: `main`
3. Enable:
   - [x] **Require a pull request before merging**
     - Required approvals: **1**
     - [x] Dismiss stale pull request approvals when new commits are pushed
     - [x] Require review from Code Owners (optional)
   - [x] **Require status checks to pass before merging**
     - [x] Require branches to be up to date before merging
     - Add status checks: `build-and-test`, `security-scan`
   - [x] **Require conversation resolution before merging**
   - [x] **Require linear history** (optional, cleaner git history)
   - [x] **Do not allow bypassing the above settings**
   - [x] **Restrict who can push to matching branches**
     - Add: Repository admins only
4. Click **Create**

### Step 4: Protect prod Branch

1. Go to: `Settings → Branches → Add branch protection rule`
2. Branch name pattern: `prod`
3. Enable:
   - [x] **Require a pull request before merging**
     - Required approvals: **2** (more strict for production!)
     - [x] Dismiss stale pull request approvals when new commits are pushed
     - [x] Require review from Code Owners
   - [x] **Require status checks to pass before merging**
     - [x] Require branches to be up to date before merging
     - Add status checks: `build-and-test`, `security-scan`, `build-docker-images`
   - [x] **Require conversation resolution before merging**
   - [x] **Require linear history**
   - [x] **Do not allow bypassing the above settings**
   - [x] **Restrict who can push to matching branches**
     - Add: Repository admins only
   - [x] **Require deployments to succeed before merging** (optional)
     - Add environment: `staging` (ensures staging deploy succeeds first)
4. Click **Create**

---

## Workflow

### For Contributors (dev → main)

```bash
# 1. Fork the repository
# 2. Clone your fork
git clone https://github.com/{your-username}/TaskFlow-Microservices.git
cd TaskFlow-Microservices

# 3. Create feature branch from dev
git checkout dev
git pull upstream dev
git checkout -b feature/my-feature

# 4. Make changes, commit
git add .
git commit -m "feat: add new feature"

# 5. Push to your fork
git push origin feature/my-feature

# 6. Create PR to upstream dev branch
# GitHub will auto-suggest dev as target (since it's default branch)
```

**CI/CD**: ✅ Build + Test runs automatically

**Merge**: No review required for dev (but recommended)

### For Maintainers (main → prod)

```bash
# 1. Merge approved PRs from dev to main via GitHub UI
# This triggers: Build + Test + Deploy to Staging

# 2. Test staging environment
curl https://api-staging.taskflow.com/health

# 3. Create release PR: main → prod
git checkout main
git pull
gh pr create --base prod --head main \
  --title "Release v1.2.3 to Production" \
  --body "Release notes..."

# 4. Get 2 approvals (strict for production)

# 5. Merge PR
# This triggers: Build + Test + Deploy to Production (AWS)

# 6. Verify production
curl https://api.taskflow.com/health
```

---

## CI/CD Behavior by Branch

### dev Branch

**Triggers**:
- Push to dev
- PR to dev

**Jobs**:
- ✅ Build and Test
- ✅ Security Scan
- ✅ Build Docker Images (but not deployed)
- ❌ Deploy (skipped)

**Purpose**: Fast feedback for contributors

### main Branch

**Triggers**:
- Push to main (via merged PR from dev)
- PR to main

**Jobs**:
- ✅ Build and Test
- ✅ Security Scan
- ✅ Build Docker Images
- ✅ Deploy to **Staging** (Docker Compose)

**Environment**: `staging`
**URL**: `https://api-staging.taskflow.com`

### prod Branch

**Triggers**:
- Push to prod (via merged PR from main)
- PR to prod

**Jobs**:
- ✅ Build and Test
- ✅ Security Scan
- ✅ Build Docker Images
- ✅ Deploy to **Production** (AWS ECS/EKS)

**Environment**: `production`
**URL**: `https://api.taskflow.com`
**Cloud**: AWS (default)

---

## GitHub Environments

You need to create 3 GitHub Environments with appropriate secrets:

### 1. development Environment

**Settings → Environments → New environment: `development`**

**Protection Rules**: None (fast iteration)

**Secrets**: Minimal (local dev secrets)

### 2. staging Environment

**Settings → Environments → New environment: `staging`**

**Protection Rules**:
- [ ] Required reviewers: None
- [x] Wait timer: 0 minutes
- [x] Deployment branches: `main` only

**Secrets**:
```
POSTGRES_USER=staging_user
POSTGRES_PASSWORD=<staging-password>
RABBITMQ_USERNAME=staging_rabbit
RABBITMQ_PASSWORD=<staging-password>
REDIS_PASSWORD=<staging-password>
```

### 3. production Environment

**Settings → Environments → New environment: `production`**

**Protection Rules**:
- [x] Required reviewers: **2 people** (critical!)
- [x] Wait timer: **10 minutes** (time to cancel if needed)
- [x] Deployment branches: `prod` only

**Secrets**:
```
POSTGRES_USER=prod_user
POSTGRES_PASSWORD=<strong-prod-password>
RABBITMQ_USERNAME=prod_rabbit
RABBITMQ_PASSWORD=<strong-prod-password>
REDIS_PASSWORD=<strong-prod-password>
AWS_ACCESS_KEY=AKIA...
AWS_SECRET_KEY=wJalr...
AWS_REGION=us-east-1
SEQ_API_KEY=<prod-seq-key>
```

---

## Security Best Practices

### ✅ DO

1. **Always use PRs** for main and prod
   ```bash
   # Good
   git push origin feature/my-feature
   # Create PR via GitHub UI

   # Bad (blocked by protection)
   git push origin main
   ```

2. **Require approvals** before merging
   - main: 1 approval
   - prod: 2 approvals

3. **Test in staging** before production
   ```bash
   # Merge dev → main (deploys to staging)
   # Test staging thoroughly
   # Then merge main → prod (deploys to production)
   ```

4. **Use semantic versioning** for prod releases
   ```
   v1.0.0 - Initial release
   v1.1.0 - New features
   v1.1.1 - Bug fixes
   ```

5. **Tag production releases**
   ```bash
   git checkout prod
   git pull
   git tag -a v1.2.3 -m "Release v1.2.3"
   git push origin v1.2.3
   ```

### ❌ DON'T

1. **Don't push directly to main or prod**
   - Protection rules prevent this
   - Always use PRs

2. **Don't bypass required checks**
   - All tests must pass
   - Security scans must pass

3. **Don't merge without reviews**
   - main: needs 1 review
   - prod: needs 2 reviews

4. **Don't skip staging**
   - Always deploy to staging first
   - Test thoroughly before prod

5. **Don't use same secrets** across environments
   - dev: weak passwords OK
   - staging: similar to prod
   - prod: strong, unique passwords

---

## Troubleshooting

### Problem: Can't push to main

**Error**: `remote: error: GH006: Protected branch update failed`

**Solution**: Create a PR instead
```bash
git checkout -b my-feature
git push origin my-feature
# Create PR via GitHub UI
```

### Problem: PR checks failing

**Solution**: Fix the issues locally
```bash
# Run tests locally
dotnet test

# Fix issues
# Push fixes
git push origin my-feature
# Checks will re-run automatically
```

### Problem: Need to bypass protection (emergency)

**Solution**: Temporarily disable protection (admins only)
1. Go to: `Settings → Branches → Edit protection rule`
2. Uncheck **Do not allow bypassing**
3. Make emergency fix
4. **Re-enable immediately**

**Better approach**: Use hotfix branch
```bash
git checkout prod
git checkout -b hotfix/critical-bug
# Fix bug
git push origin hotfix/critical-bug
# Create PR with "urgent" label
# Get fast-track approvals
```

---

## Verification

After setting up, verify protection:

### Test main Protection

```bash
# This should FAIL
git checkout main
echo "test" >> README.md
git commit -am "test: should fail"
git push origin main
# Expected: remote: error: GH006: Protected branch update failed

# This should SUCCEED
git checkout -b test-protection
git push origin test-protection
# Create PR via GitHub → Should require 1 approval
```

### Test prod Protection

```bash
# This should FAIL
git checkout prod
echo "test" >> README.md
git commit -am "test: should fail"
git push origin prod
# Expected: remote: error: GH006: Protected branch update failed

# This should SUCCEED (after staging deploy succeeds)
# Create PR: main → prod
# Should require 2 approvals
# Should require staging deployment to succeed first
```

---

## Summary

| Branch | Open/Protected | Approvals | Deploys To | Purpose |
|--------|----------------|-----------|------------|---------|
| **dev** | Open | 0 (recommended: 1) | None (CI only) | Development |
| **main** | Protected | 1 | Staging | Pre-production |
| **prod** | Protected | 2 | Production (AWS) | Production |

**Workflow**:
```
Contributors → dev (via PR)
Maintainers  → main (via PR, 1 approval, deploys to staging)
Maintainers  → prod (via PR, 2 approvals, deploys to production)
```

**Protection Benefits**:
- ✅ Prevents accidental pushes to main/prod
- ✅ Requires code review
- ✅ Enforces CI checks
- ✅ Separates staging and production
- ✅ Provides rollback safety

---

## Quick Reference

```bash
# Contributor workflow
git checkout dev
git checkout -b feature/my-feature
# make changes
git push origin feature/my-feature
# Create PR to dev

# Maintainer workflow (dev → main → prod)
# Merge PR to main (auto-deploys to staging)
# Test staging
# Create PR from main to prod (requires 2 approvals)
# Merge to prod (auto-deploys to production)
```

---

**Last Updated**: 2025-11-03
**Related**: [CI/CD Pipeline](../workflows/ci-cd-pipeline.yml), [Setup Secrets](SETUP_SECRETS.md)
