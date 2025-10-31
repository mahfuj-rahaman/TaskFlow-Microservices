# BuildingBlocks Implementation Roadmap

**Last Updated**: 2025-11-01
**Status**: Ready for Implementation

---

## 🎯 Overview

This document outlines the implementation order for all TaskFlow BuildingBlocks. These must be completed **before** generating microservices with the scaffolding scripts.

## 📊 Current Status

### ✅ Completed (Partial)
- **TaskFlow.BuildingBlocks.Common** (30% complete)
  - ✅ Domain (Entity, AggregateRoot, ValueObject, IDomainEvent)
  - ✅ Results (Result, Error)
  - ✅ Exceptions (DomainException)

### ❌ To Implement
1. **TaskFlow.BuildingBlocks.Common** (Complete remaining 70%)
2. **TaskFlow.BuildingBlocks.EventBus** (0% complete)
3. **TaskFlow.BuildingBlocks.Messaging** (0% complete)
4. **TaskFlow.BuildingBlocks.Caching** (0% complete)

---

## 📋 Implementation Order

### Phase 1: Common BuildingBlocks (Priority: CRITICAL)

**Estimated Time**: 2-3 hours

#### Components to Implement:
1. **Pagination**
   - `PagedList<T>`
   - `PaginationExtensions`

2. **Repository Interfaces**
   - `IRepository<TEntity, TId>`

3. **Specification Pattern**
   - `ISpecification<T>`
   - `BaseSpecification<T>`
   - `SpecificationEvaluator`

4. **Audit Properties**
   - `IAuditableEntity`
   - `ISoftDeletable`

5. **Guard Clauses**
   - `Guard` static class

6. **Extension Methods**
   - `StringExtensions`
   - `EnumerableExtensions`

7. **Clock Abstraction**
   - `ISystemClock`
   - `SystemClock`

#### Files to Create:
```
TaskFlow.BuildingBlocks.Common/
├── Pagination/
│   ├── PagedList.cs
│   └── PaginationExtensions.cs
├── Repositories/
│   └── IRepository.cs
├── Specifications/
│   ├── ISpecification.cs
│   ├── BaseSpecification.cs
│   └── SpecificationEvaluator.cs
├── Auditing/
│   ├── IAuditableEntity.cs
│   └── ISoftDeletable.cs
├── Guards/
│   └── Guard.cs
├── Extensions/
│   ├── StringExtensions.cs
│   └── EnumerableExtensions.cs
└── Time/
    ├── ISystemClock.cs
    └── SystemClock.cs
```

#### Acceptance Criteria:
- [ ] All classes compile without errors
- [ ] Unit tests for Guard clauses
- [ ] Unit tests for Extensions
- [ ] Specification pattern tested with EF Core
- [ ] Documentation examples work

---

### Phase 2: EventBus (Priority: HIGH)

**Estimated Time**: 3-4 hours
**Dependencies**: Common, MediatR, MassTransit

#### Components to Implement:
1. **Core Interfaces**
   - `IEventBus`
   - `IIntegrationEventMapper`
   - `IIntegrationEvent`

2. **Implementation**
   - `EventBus` (main class)
   - `IntegrationEventMapper`
   - `IntegrationEventBase`

3. **Configuration**
   - `EventBusExtensions` (DI)

#### Files to Create:
```
TaskFlow.BuildingBlocks.EventBus/
├── Abstractions/
│   ├── IEventBus.cs
│   ├── IIntegrationEvent.cs
│   └── IIntegrationEventMapper.cs
├── EventBus.cs
├── IntegrationEventBase.cs
├── IntegrationEventMapper.cs
└── Extensions/
    └── EventBusExtensions.cs
```

#### Acceptance Criteria:
- [ ] Can publish domain events
- [ ] In-process handlers execute (MediatR)
- [ ] Integration events published to RabbitMQ
- [ ] Event mapping works correctly
- [ ] Integration tests pass

---

### Phase 3: Messaging (Priority: HIGH)

**Estimated Time**: 3-4 hours
**Dependencies**: MassTransit, RabbitMQ

#### Components to Implement:
1. **Configuration**
   - `MessagingOptions`
   - `RetryOptions`
   - `OutboxOptions`

2. **Extensions**
   - `MassTransitExtensions`

3. **Base Classes**
   - `IMessage`
   - `MessageBase`
   - `BaseConsumer<TMessage>`

#### Files to Create:
```
TaskFlow.BuildingBlocks.Messaging/
├── Configuration/
│   ├── MessagingOptions.cs
│   ├── RetryOptions.cs
│   └── OutboxOptions.cs
├── Abstractions/
│   ├── IMessage.cs
│   └── MessageBase.cs
├── Consumers/
│   └── BaseConsumer.cs
└── Extensions/
    └── MassTransitExtensions.cs
```

#### Acceptance Criteria:
- [ ] MassTransit configured correctly
- [ ] Can publish messages to RabbitMQ
- [ ] Consumers receive messages
- [ ] Retry policy works
- [ ] Dead letter queue handling
- [ ] Integration tests with TestHarness

---

### Phase 4: Caching (Priority: MEDIUM)

**Estimated Time**: 2-3 hours
**Dependencies**: Redis, StackExchange.Redis

#### Components to Implement:
1. **Core Interface**
   - `ICacheService`

2. **Implementations**
   - `RedisCacheService`
   - `MemoryCacheService`
   - `HybridCacheService`

3. **MediatR Behavior**
   - `CachingBehavior<TRequest, TResponse>`
   - `ICacheableQuery`

4. **Configuration**
   - `CachingOptions`
   - `CachingExtensions`

#### Files to Create:
```
TaskFlow.BuildingBlocks.Caching/
├── Abstractions/
│   ├── ICacheService.cs
│   └── ICacheableQuery.cs
├── Services/
│   ├── RedisCacheService.cs
│   ├── MemoryCacheService.cs
│   └── HybridCacheService.cs
├── Behaviors/
│   └── CachingBehavior.cs
├── Configuration/
│   └── CachingOptions.cs
└── Extensions/
    └── CachingExtensions.cs
```

#### Acceptance Criteria:
- [ ] Redis connection works
- [ ] Can get/set cache values
- [ ] Hybrid cache L1/L2 works
- [ ] MediatR caching behavior works
- [ ] Cache invalidation works
- [ ] Integration tests with Redis (Testcontainers)

---

## 🔄 After BuildingBlocks Complete

### 1. Update Scaffolding Scripts

The code generation scripts need to be updated to use BuildingBlocks:

#### Changes to `generate-domain.sh`:
- Generate entities that use `AggregateRoot<TId>`
- Include domain event raising
- Use `Guard` clauses for validation

#### Changes to `generate-application.sh`:
- Use `IRepository<TEntity, TId>` interface
- Generate queries that implement `ICacheableQuery`
- Use `PagedList<T>` for list queries
- Use `Result<T>` pattern correctly

#### Changes to `generate-infrastructure.sh`:
- Generate repositories that implement `IRepository`
- Use `SpecificationEvaluator` for complex queries
- Include audit properties in DbContext SaveChanges

#### New: `generate-events.sh`:
- Generate integration events from domain events
- Generate event consumers
- Register with EventBus

### 2. Test the Scripts

Create a test service to verify:
```bash
./scripts/generate-from-spec.sh TestProduct Catalog

# Should generate:
# - Entity with AggregateRoot
# - Repository with IRepository
# - Queries with caching
# - Events with EventBus integration
# - All compiles with 0 errors
```

---

## 📈 Progress Tracking

### Week 1: Common BuildingBlocks
- [ ] Day 1-2: Pagination, Repository, Specifications
- [ ] Day 3: Audit, Guards, Extensions
- [ ] Day 4: Testing and documentation

### Week 2: EventBus + Messaging
- [ ] Day 1-2: EventBus implementation
- [ ] Day 3-4: Messaging implementation
- [ ] Day 5: Integration testing

### Week 3: Caching + Script Updates
- [ ] Day 1-2: Caching implementation
- [ ] Day 3-4: Update scaffolding scripts
- [ ] Day 5: End-to-end testing

---

## 🎯 Definition of Done

A BuildingBlock is considered **done** when:

1. ✅ All code compiles without errors/warnings
2. ✅ Unit tests written and passing (>80% coverage)
3. ✅ Integration tests passing
4. ✅ XML documentation complete
5. ✅ Usage examples in specification
6. ✅ NuGet packages configured correctly
7. ✅ Can be used by scaffolding scripts

---

## 📚 Specification Documents

- ✅ [EventBus_Specification.md](./EventBus_Specification.md)
- ✅ [Messaging_Specification.md](./Messaging_Specification.md)
- ✅ [Caching_Specification.md](./Caching_Specification.md)
- ✅ [Common_Specification.md](./Common_Specification.md)

---

## 🚀 Next Steps

1. **Start with Phase 1**: Common BuildingBlocks
2. **Follow implementation order**: Don't skip phases
3. **Test thoroughly**: Each phase must be tested before moving on
4. **Update scripts last**: Only after all BuildingBlocks are complete

---

**Ready to begin?** Start with Phase 1: Common BuildingBlocks
**First file to create**: `TaskFlow.BuildingBlocks.Common/Pagination/PagedList.cs`
