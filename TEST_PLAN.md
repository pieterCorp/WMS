# WMS Backend Test Plan

## Scope
Testing core warehouse management endpoints:
- **GET /orders/{id}** - Retrieve order with pick instructions
- **POST /pick** - Confirm pick and update inventory

## Test Strategy

### Unit Tests
- **OrderService**: Business logic for order retrieval and validation
- **PickService**: Pick confirmation and inventory reduction
- **InventoryService**: Inventory queries and updates
- **ValidationService**: Barcode, rack, product validation

### Integration Tests (API)
- **OrderController.GetOrderById**: Response format, non-existent orders (404)
- **PickController.ConfirmPick**: Successful pick, inventory updates, validation errors
- **Inventory State**: Verify database state changes after pick operations

### Test Database
- In-memory SQLite for fast, isolated tests
- Seeded with test data (products, locations, inventory, orders)

## Test Cases

### GET /orders/{id}

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| T1 | Valid order ID | 200, order + items with locations | HIGH |
| T2 | Non-existent order | 404 | HIGH |
| T3 | Order with multiple items | All items returned | HIGH |
| T4 | Order with no items | Empty items array | MED |

### POST /pick

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| T5 | Valid pick (all fields correct) | 200, inventory reduced by 1 | HIGH |
| T6 | Invalid order ID | 400/404 | HIGH |
| T7 | Invalid product barcode | 400 | HIGH |
| T8 | Invalid rack | 400 | HIGH |
| T9 | Insufficient inventory | 400 | HIGH |
| T10 | Multiple picks same item | Inventory decrements correctly | HIGH |
| T11 | Complete order picking | All items picked updates order status | MED |

## Blockers (Current State)

**Missing implementations:**
- [ ] Models: Product, Location, Inventory, Order, OrderItem, ScanEvent
- [ ] DbContext and EF Core setup
- [ ] OrderController (GET /orders/{id})
- [ ] PickController (POST /pick)
- [ ] OrderService, PickService, InventoryService
- [ ] Database migrations

## Test Execution
- Framework: xUnit
- Integration Testing: Microsoft.AspNetCore.Mvc.Testing
- Database: InMemory SQLite (net10.0 compatible)
- Run: `dotnet test tests/Backend.Tests`
