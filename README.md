# 🛒 E-COMMERCE MICROSERVICES ECOSYSTEM

A scalable, production-ready e-commerce platform built on a distributed **.NET Microservices Architecture**. This repository demonstrates enterprise-level patterns, including cloud-native identity management, centralized API routing, asynchronous event-driven communication, and distributed transactional consistency via Saga Orchestration.

---

## 🏗️ SYSTEM ARCHITECTURE & TECH STACK

The ecosystem utilizes a decoupled microservices design where internal services maintain complete data isolation and communicate via an API Gateway or asynchronous event streaming.

```text
                         [ React Frontend / Client ]
                                  │
                    Authenticate  │ (OAuth2 / OIDC)
                                  ▼
                       [ Azure Entra ID Tenant ]
                                  │
                     Token Issued │ (JWT Bearer Token)
                                  ▼
                      [ YARP Reverse Proxy Gateway ]
                                  │
         ┌────────────────────────┼────────────────────────┬────────────────────────┐
         │ Forward Token          │ (HTTP + JWT)           │ (HTTP + JWT)           │ (HTTP + JWT)
         ▼ (HTTP + JWT)           ▼                        ▼                        ▼
┌───────────────────┐    ┌───────────────────┐    ┌───────────────────┐    ┌───────────────────┐
│  Catalog Service  │    │  Product Service  │    │  Inventory Serv.  │    │   Order Service   │
│   (Catalog API)   │    │   (Product API)   │    │  (Inventory API)  │    │ (Order API + Saga)│
└─────────┬─────────┘    └─────────┬─────────┘    └─────────┬─────────┘    └─────────┬─────────┘
          │                        │                        │                        │
          └────────────────────────┴───────────┬────────────┴────────────────────────┘
                                               │
                                               ▼
                                      [ Apache Kafka ]
                                        (Event Bus)

```

### CORE ARCHITECTURE COMPONENTS

*   **IDENTITY & SECURITY**: Integrated with Azure Entra ID to handle authentication and authorization.
*   **TOKEN VALIDATION**: The YARP gateway validates incoming JSON Web Tokens (JWT) to secure downstream microservices.
*   **API GATEWAY**: Built with YARP (Yet Another Reverse Proxy) to act as the single entry point, managing centralized route mapping, JWT token forwarding, and cross-origin (CORS) policies.
*   **DISTRIBUTED TRANSACTIONS**: Implements the Saga Pattern (Orchestration-based) via Apache Kafka to coordinate multi-service checkout workflows (Order placement ➔ Inventory reservation) without distributed database deadlocks.
*   **APPLICATION PATTERN**: Implements CQRS (Command Query Responsibility Segregation) to maximize throughput by isolating data read pathways from domain mutation states.
*   **IN-PROCESS MESSAGING**: Powered by MediatR to keep endpoint controllers lean and bind incoming HTTP models smoothly into isolated domain command/query handlers.
*   **DISTRIBUTED EVENT BUS**: Uses Apache Kafka to handle decoupled cross-service messaging and transmit Saga command/compensation events asynchronously.

---

## 📦 MICROSERVICE MODULES

### 1. 📂 APIGATEWAY (YARP + ENTRA ID VALIDATION)
*   Acts as the central edge routing engine for client traffic.
*   Uses `Microsoft.Identity.Web` to secure routes, process authentication protocols, and validate tokens.

### 2. 💳 ORDER SERVICE (SAGA ORCHESTRATOR)
*   Coordinates customer checkout behaviors and acts as the Saga Orchestrator for checkout workflows.
*   Manages the state machine for order fulfillment (Created, StockReserved, Completed, or Cancelled/Compensated).

### 3. 📊 INVENTORY SERVICE
*   Tracks physical stock warehouse data by keeping lightweight ties to relational `ProductID` values.
*   Listens for Saga commands (`ReserveInventoryCommand`) from Kafka and emits success (`InventoryReservedEvent`) or compensation failure (`InventoryRejectedEvent`) streams back to the orchestrator.

### 4. 🛡️ PRODUCT SERVICE
*   Exposes secure endpoints (`PUT`/`PATCH`) requiring verified Entra ID scopes to update core profile descriptors or pricing matrices.
*   Decoupled from live transactional states to preserve domain boundaries.

### 5. 🏷️ CATALOG SERVICE
*   Manages overarching structural business classifications and product categorization hierarchies.
*   Demonstrates fast pagination patterns for administration dashboards.

---

## 🔄 CHECKOUT SAGA ORCHESTRATION FLOW

Because each microservice owns its database, standard ACID transactions are impossible. We implement an **Orchestrated Saga** using Kafka topics to guarantee eventual consistency.

```text
[Order Service (Orchestrator)]          [Kafka Event Bus]          [Inventory Service]
             │                                  │                           │
             │── 1. Create Order (Pending) ───> │                           │
             │── 2. Emit 'ReserveInvCmd' ────>  │                           │
             │                                  │── 3. Forward Command ────>│
             │                                  │                           │── 4. Dedicate Stock
             │                                  │<── 5. Emit 'InvResEvt' ───│
             │<─ 6. Consume 'InvReserved' ────  │                           │
             │                                  │                           │
   ┌─────────┴─────────┐                        │                           │
   │ SUCCESS WORKFLOW  │                        │                           │
   │ Complete Order    │                        │                           │
   └───────────────────┘                        │                           │
             │                                  │                           │
   ┌─────────┴─────────┐                        │                           │
   │ FAILURE WORKFLOW  │                        │                           │
   │ (Compensation)    │                        │                           │
   │ Cancel Order      │                        │                           │
   └───────────────────┘                        │                           │
```

### SAGA LIFECYCLE STAGES

*   **INITIATION**: The user submits an order. The Order Service saves the order with a `Pending` state and publishes a `ReserveStockCommand` to Kafka.
*   **EXECUTION**: The Inventory Service consumes the command, checks the warehouse, deducts the items, and publishes a `InventoryReservedEvent`.
*   **SUCCESS STEP**: The Order Service consumes the success event and changes the order status to `Completed`.
*   **COMPENSATION STEP (ROLLBACK)**: If the Inventory Service finds out an item is out of stock, it publishes a `InventoryRejectedEvent`. The Order Service catches this failure event, triggers its internal compensation handler, cancels the order transaction safely, and releases any pre-allocated assets.

---

## 🛠️ LOCAL DEVELOPMENT & QUICKSTART

### PREREQUISITES
*   [.NET 8.0 SDK](https://microsoft.com) or higher
*   [Apache Kafka Cluster](https://apache.org) (Local installation or Docker image)
*   An active **Azure Entra ID Tenant** with an Application Registration configured.

### 1. CLONE THE REPOSITORY
```bash
git clone https://github.com
cd ECommerceMicroservices
```

### 2. CONFIGURE LOCAL CONFIGURATION SAFETY
To protect security credentials from leaking into Git history, every runtime project relies on a decoupled config topology. Create an `appsettings.json` file inside your application entry points using this placeholder template [On Sat, June 20, 2026 @ 10:41 AM]:

```json
{
  "AzureAd": {
    "Instance": "https://microsoftonline.com",
    "Domain": "yourtenant.onmicrosoft.com",
    "TenantId": "YOUR_TENANT_ID_PLACEHOLDER",
    "ClientId": "YOUR_CLIENT_ID_PLACEHOLDER",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-oidc"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  },
  "AllowedHosts": "*"
}
```
## 🔬 ARCHITECTURE DESIGN PATTERNS

### CORE DESIGN CRITERIA

*   **CQRS ISOLATION WITH MEDIATR HANDLERS**: All incoming data mutations are explicitly separated from queries using MediatR pipelines to isolate responsibilities cleanly.

```csharp
// Sample Controller Action Pattern utilizing clean isolation
[Authorize(Policy = "RequireAdminScope")]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
{
    if (id != command.Id) return BadRequest();
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

*   **DISTRIBUTED DATA CONSISTENCY BOUNDARIES**: Formulates decoupled domain boundaries across relational database engines.
    *   **DESIGN RULE**: The Order and Inventory service domain schemas hold only primitive structural ID elements (e.g., `ProductID`). They never directly link to or duplicate object properties from the Catalog boundaries.
    *   **SYNCHRONIZATION FLOW**: Cross-boundary sync relies entirely on Kafka messaging topics. State machine changes flow through the Saga architecture asynchronously, ensuring high performance without cross-database constraints.

***
