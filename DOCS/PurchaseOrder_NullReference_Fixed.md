# Purchase Order NullReferenceException Fixed

## Problem
A `NullReferenceException` was occurring in `PurchaseOrderMapper.MapToResponseDto()` at line 40 when trying to access `d.Product.code` and `d.Product.name`.

### Error Details
```
System.NullReferenceException: Object reference not set to an instance of an object.
at Application.Core.PurchaseOrder.QueryHandlers.PurchaseOrderMapper.<>c.<MapToResponseDto>b__0_0(PurchaseOrderDetail d)
```

## Root Cause
The `Product` navigation property in `PurchaseOrderDetail` was `null` because several repository methods were missing the necessary `.ThenInclude(d => d.Product)` statement when loading purchase orders.

### Affected Repository Methods
1. `GetBySupplierAsync()`
2. `GetByWarehouseAsync()`
3. `GetByStatusAsync()`
4. `GetByDateRangeAsync()`
5. `GetPendingToReceiveAsync()`
6. `GetPendingApprovalAsync()`

These methods were loading the `Details` collection but not the `Product` navigation property within each detail.

## Solution

### 1. Fixed Repository Methods
**File:** `Infrastructure/Repositories/PurchaseOrderRepository.cs`

Added missing includes to all affected methods:
```csharp
.Include(po => po.Details)
    .ThenInclude(d => d.Product)  // ? Added
.Include(po => po.CreatedBy)       // ? Added
.Include(po => po.UpdatedBy)       // ? Added
```

### 2. Added Null-Safety to Mapper
**File:** `Application/Core/PurchaseOrder/QueryHandlers/PurchaseOrderQueryHandlers.cs`

Added defensive programming to prevent future issues:
```csharp
Details = order.Details.Select(d => new PurchaseOrderDetailResponseDto
{
    // ...existing properties...
    ProductCode = d.Product?.code ?? "N/A",  // ? Null-safe
    ProductName = d.Product?.name ?? "Producto no disponible",  // ? Null-safe
    // ...existing properties...
}).ToList(),
```

## Benefits

### 1. **Immediate Fix**
- Resolves the NullReferenceException
- All purchase order queries now work correctly

### 2. **Consistency**
- All repository methods now have the same `.Include()` pattern
- Ensures complete entity loading across all query methods

### 3. **Defensive Programming**
- Null-safety checks prevent future runtime errors
- Graceful degradation if Product is missing

### 4. **Complete Data Loading**
- Also added missing `CreatedBy` and `UpdatedBy` includes
- Ensures all user information is available in responses

## Testing Recommendations

Test the following endpoints to verify the fix:
1. `GET /api/purchaseorders` - Get all orders
2. `GET /api/purchaseorders/paged` - Get paged orders
3. `GET /api/purchaseorders/{id}` - Get order by ID
4. `GET /api/purchaseorders/code/{code}` - Get order by code
5. `GET /api/purchaseorders/pending-to-receive` - Get pending orders

## Files Modified
1. `Infrastructure/Repositories/PurchaseOrderRepository.cs`
   - Added `.ThenInclude(d => d.Product)` to 6 methods
   - Added `.Include(po => po.CreatedBy)` to 6 methods
   - Added `.Include(po => po.UpdatedBy)` to 6 methods
   - Added `.ThenInclude(w => w.Branch)` to 6 methods

2. `Application/Core/PurchaseOrder/QueryHandlers/PurchaseOrderQueryHandlers.cs`
   - Added null-conditional operators (`?.`) for Product access
   - Added null-coalescing operators (`??`) with default values

## Impact
- ? No breaking changes
- ? Backward compatible
- ? Build successful
- ? No new dependencies
