# GitHub Secrets Setup Guide

This guide helps you set up all required secrets for TaskFlow CI/CD pipeline.

## Quick Setup

### 1. Navigate to Repository Secrets

```
https://github.com/{your-username}/TaskFlow-Microservices/settings/secrets/actions
```

Or:
```
Repository → Settings → Secrets and variables → Actions
```

---

## Required Secrets (All Environments)

Click **"New repository secret"** and add each of the following:

### Database Secrets

| Secret Name | Example Value | Description |
|------------|---------------|-------------|
| `POSTGRES_USER` | `taskflow_admin` | PostgreSQL admin username |
| `POSTGRES_PASSWORD` | `<generate-strong-password>` | PostgreSQL admin password |

**Generate strong password**:
```bash
openssl rand -base64 32
```

### Cache Secrets

| Secret Name | Example Value | Description |
|------------|---------------|-------------|
| `REDIS_PASSWORD` | `<generate-strong-password>` | Redis password |

### Messaging Secrets

| Secret Name | Example Value | Description |
|------------|---------------|-------------|
| `RABBITMQ_USERNAME` | `taskflow` | RabbitMQ username |
| `RABBITMQ_PASSWORD` | `<generate-strong-password>` | RabbitMQ password |

---

## Cloud Provider Secrets

### AWS (if deploying to AWS)

| Secret Name | Example Value | Description |
|------------|---------------|-------------|
| `AWS_ACCESS_KEY` | `AKIA...` | AWS IAM Access Key |
| `AWS_SECRET_KEY` | `wJalr...` | AWS IAM Secret Key |
| `AWS_REGION` | `us-east-1` | AWS Region |

**How to get AWS credentials**:
1. Go to AWS Console → IAM → Users → [Your User] → Security Credentials
2. Create Access Key
3. **Copy and save immediately** (you can't view the secret key again)

### Azure (if deploying to Azure)

| Secret Name | Example Value | Description |
|------------|---------------|-------------|
| `AZURE_SERVICE_BUS_CONNECTION_STRING` | `Endpoint=sb://...` | Azure Service Bus connection string |
| `AZURE_CLIENT_ID` | `<guid>` | Service Principal Client ID |
| `AZURE_CLIENT_SECRET` | `<secret>` | Service Principal Secret |
| `AZURE_TENANT_ID` | `<guid>` | Azure Tenant ID |

**How to get Azure credentials**:
```bash
# Login
az login

# Create Service Principal
az ad sp create-for-rbac --name "TaskFlow-CICD" --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group}

# Output will show:
# - appId = AZURE_CLIENT_ID
# - password = AZURE_CLIENT_SECRET
# - tenant = AZURE_TENANT_ID
```

### GCP (if deploying to GCP)

| Secret Name | Example Value | Description |
|------------|---------------|-------------|
| `GCP_PROJECT_ID` | `taskflow-prod` | GCP Project ID |
| `GCP_SERVICE_ACCOUNT_KEY` | `{...json...}` | Service Account JSON key (entire file content) |

**How to get GCP credentials**:
```bash
# Create Service Account
gcloud iam service-accounts create taskflow-cicd \
  --display-name="TaskFlow CI/CD"

# Grant permissions
gcloud projects add-iam-policy-binding PROJECT_ID \
  --member="serviceAccount:taskflow-cicd@PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/container.admin"

# Create and download key
gcloud iam service-accounts keys create key.json \
  --iam-account=taskflow-cicd@PROJECT_ID.iam.gserviceaccount.com

# Copy entire content of key.json to GCP_SERVICE_ACCOUNT_KEY secret
cat key.json | pbcopy  # macOS
cat key.json | xclip -selection clipboard  # Linux
```

---

## Optional Monitoring Secrets

| Secret Name | Example Value | Description |
|------------|---------------|-------------|
| `SEQ_API_KEY` | `<api-key>` | Seq log aggregation API key |
| `DATADOG_API_KEY` | `<api-key>` | DataDog monitoring API key |

---

## Environment-Specific Secrets

### Setup Environments

1. Go to: `Repository → Settings → Environments`
2. Create three environments:
   - `development`
   - `staging`
   - `production`

### Production Environment

For `production` environment, add:

**Protection Rules** (recommended):
- ✅ Required reviewers: 1-2 people
- ✅ Wait timer: 5 minutes
- ✅ Deployment branches: `main` only

**Secrets** (specific to production):
```
AWS_ACCESS_KEY=<prod-access-key>
AWS_SECRET_KEY=<prod-secret-key>
SEQ_API_KEY=<prod-seq-key>
```

### Staging Environment

For `staging` environment, add:

**Protection Rules**:
- ✅ Deployment branches: `staging` only

**Secrets** (can use different credentials):
```
POSTGRES_PASSWORD=<staging-password>
RABBITMQ_PASSWORD=<staging-password>
```

---

## Verification Checklist

After adding all secrets, verify:

- [ ] All required secrets added to repository
- [ ] Cloud provider secrets added (AWS/Azure/GCP)
- [ ] Environments created (development, staging, production)
- [ ] Production environment has protection rules
- [ ] No secrets in git (check `.gitignore`)

**Test the setup**:
```bash
# Trigger workflow manually
Repository → Actions → CI/CD Pipeline → Run workflow
```

---

## Secret Rotation Schedule

Set reminders to rotate secrets:

| Secret Type | Rotation Frequency | Reminder |
|------------|-------------------|----------|
| Database passwords | Every 90 days | Create calendar reminder |
| Cloud credentials | Every 90 days | Create calendar reminder |
| Messaging passwords | Every 90 days | Create calendar reminder |
| API keys | When compromised | Immediate |

**How to rotate**:
1. Generate new password/key
2. Update in GitHub Secrets
3. Re-deploy services with new secrets
4. Verify services are running
5. (Optional) Deactivate old credentials

---

## Troubleshooting

### Q: "Secret not found" error in GitHub Actions

**A**: Check secret name spelling (case-sensitive!)
```yaml
# ❌ Wrong
${{ secrets.postgres_password }}

# ✅ Correct
${{ secrets.POSTGRES_PASSWORD }}
```

### Q: How do I view a secret value?

**A**: You can't view secrets in GitHub UI (by design). You must:
- Re-enter the value
- Or check your password manager

### Q: Can I use the same secrets for all environments?

**A**: Not recommended. Use different secrets for:
- Development (weak passwords OK)
- Staging (similar to production)
- Production (strong passwords, rotated regularly)

### Q: How do I test secrets locally?

**A**: Use environment variables:
```bash
export POSTGRES_PASSWORD="test-password"
./scripts/deploy-with-secrets.sh development
```

Or use .NET User Secrets:
```bash
dotnet user-secrets set "Messaging:Password" "test-password"
```

---

## Security Best Practices

### ✅ DO

- Use strong, unique passwords for each secret
- Rotate secrets every 90 days
- Use environment-specific secrets
- Enable branch protection on `main`
- Require approvals for production deployments
- Use least-privilege IAM policies (cloud providers)

### ❌ DON'T

- Commit secrets to git (ever!)
- Use same password for multiple services
- Share secrets via email/Slack
- Use default passwords in production
- Give production access to all team members

---

## Getting Help

**Problem with secrets?**
1. Check this guide
2. Check main documentation: [CICD_SECRETS_MANAGEMENT.md](../docs/CICD_SECRETS_MANAGEMENT.md)
3. Check workflow logs: `Actions → [Your Workflow] → View logs`
4. Create an issue: [Issues](https://github.com/{owner}/TaskFlow-Microservices/issues)

---

## Summary

**Required for CI/CD**:
1. ✅ 5 infrastructure secrets (Postgres, Redis, RabbitMQ)
2. ✅ Cloud provider secrets (AWS/Azure/GCP)
3. ✅ 3 environments (dev, staging, prod)

**Time to complete**: ~15 minutes

**Next steps**:
1. Add all secrets
2. Push code to trigger pipeline
3. Monitor deployment in Actions tab

---

**Last Updated**: 2025-11-03
**Related**: [CICD_SECRETS_MANAGEMENT.md](../docs/CICD_SECRETS_MANAGEMENT.md)
