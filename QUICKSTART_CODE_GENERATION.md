# ⚡ Quick Start: AI Code Generation

## 🎯 Generate a Complete Feature in 2 Steps

### Step 1: Create Specification (1 minute)

```bash
./scripts/ai-scaffold.sh Product Catalog
```

Answer a few questions:
- **Purpose**: "Manage product catalog"
- **Properties**:
  - `Name:string:true`
  - `Price:decimal:true`
  - `StockQuantity:int:true`
- **Business Rules**:
  - "Price must be positive"
  - "Stock cannot be negative"
- **Operations**:
  - `UpdateStock`

### Step 2: Generate Code (30 seconds)

```bash
./scripts/generate-from-spec.sh Product Catalog
```

**Done! You now have:**
- ✅ ProductEntity with business logic
- ✅ 3 Commands (Create, Update, Delete) with handlers & validators
- ✅ 2 Queries (GetById, GetAll) with handlers
- ✅ Repository & EF configuration
- ✅ REST API controller with 5 endpoints
- ✅ Unit & integration tests
- ✅ **26+ files total!**

---

## 📁 What Got Generated

```
src/Services/Catalog/
├── Domain/
│   ├── ProductEntity.cs              ← Business logic
│   ├── ProductCreatedEvent.cs        ← Domain events
│   └── ProductDomainException.cs
├── Application/
│   ├── ProductDto.cs
│   ├── IProductRepository.cs
│   ├── CreateProductCommand.cs       ← CQRS commands
│   ├── UpdateProductCommand.cs
│   ├── GetProductByIdQuery.cs        ← CQRS queries
│   └── ... (16 files total)
├── Infrastructure/
│   ├── ProductRepository.cs          ← Data access
│   └── ProductConfiguration.cs       ← EF mapping
└── API/
    └── ProductsController.cs         ← REST endpoints

tests/
├── ProductEntityTests.cs             ← Unit tests
└── ProductsControllerTests.cs        ← Integration tests
```

---

## 🔧 Setup (One-time)

### 1. Update DbContext

```csharp
// In CatalogDbContext.cs
public DbSet<ProductEntity> Products => Set<ProductEntity>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new ProductConfiguration());
}
```

### 2. Register Services

```csharp
// In DependencyInjection.cs
services.AddScoped<IProductRepository, ProductRepository>();

// In Program.cs
ProductMappingConfig.Configure();
```

### 3. Create Migration

```bash
cd src/Services/Catalog/TaskFlow.Catalog.Infrastructure
dotnet ef migrations add AddProductEntity --startup-project ../TaskFlow.Catalog.API
dotnet ef database update --startup-project ../TaskFlow.Catalog.API
```

---

## 🧪 Test It

```bash
# Run tests
dotnet test

# Start API
cd src/Services/Catalog/TaskFlow.Catalog.API
dotnet run
```

```bash
# Create product
curl -X POST http://localhost:5000/api/v1/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","price":999.99,"stockQuantity":50}'

# Get all products
curl http://localhost:5000/api/v1/products

# Get by ID
curl http://localhost:5000/api/v1/products/{id}
```

---

## 🎨 API Endpoints Generated

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/products` | Get all products |
| GET | `/api/v1/products/{id}` | Get product by ID |
| POST | `/api/v1/products` | Create new product |
| PUT | `/api/v1/products/{id}` | Update product |
| DELETE | `/api/v1/products/{id}` | Delete product |

---

## ⏱️ Time Comparison

| Approach | Time | Files | Quality |
|----------|------|-------|---------|
| **Manual** | ~5 hours | 26 | Varies |
| **AI Generated** | ~2 minutes | 26 | Consistent |
| **Savings** | 99%+ | Same | Better |

---

## 🚀 Generate More Features

```bash
# Order management
./scripts/ai-scaffold.sh Order Order
./scripts/generate-from-spec.sh Order Order

# Customer management
./scripts/ai-scaffold.sh Customer User
./scripts/generate-from-spec.sh Customer User

# Payment processing
./scripts/ai-scaffold.sh Payment Payment
./scripts/generate-from-spec.sh Payment Payment
```

Each feature takes **~2 minutes** to generate completely!

---

## 📚 Documentation

- **Full Guide**: `docs/CODE_GENERATION_SYSTEM.md`
- **AI Scaffolding**: `docs/AI_SCAFFOLDING_GUIDE.md`
- **System Overview**: `SCAFFOLDING_SYSTEM.md`
- **Examples**: `docs/features/Identity_feature_example.md`

---

## 💡 Pro Tips

1. **Review Generated Code**: Always review before committing
2. **Customize Templates**: Edit scripts in `scripts/generators/` to match your needs
3. **Add Business Logic**: Generated code is a starting point - add your custom logic
4. **Run Tests**: Generated tests should pass immediately
5. **Version Control**: Commit generated code with meaningful messages

---

## ✨ Features Included

Every generated feature includes:

- ✅ **Clean Architecture** (4 layers)
- ✅ **Domain-Driven Design** (Aggregates, Events, Rules)
- ✅ **CQRS** (Commands & Queries separated)
- ✅ **Repository Pattern** (Data access abstraction)
- ✅ **Result Pattern** (Functional error handling)
- ✅ **FluentValidation** (Input validation)
- ✅ **Mapster** (Object mapping)
- ✅ **MediatR** (Request/response pipeline)
- ✅ **Unit Tests** (Domain logic)
- ✅ **Integration Tests** (API endpoints)
- ✅ **XML Documentation** (All public members)
- ✅ **Async/Await** (Non-blocking operations)
- ✅ **Cancellation Tokens** (Graceful cancellation)

---

## 🎉 Ready to Go!

Start generating features now:

```bash
./scripts/ai-scaffold.sh YourFeature YourService
./scripts/generate-from-spec.sh YourFeature YourService
```

**That's it! You have a complete, production-ready feature!** 🚀

---

**Questions?** Check `docs/CODE_GENERATION_SYSTEM.md` for detailed information.
