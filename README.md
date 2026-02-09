# WirexApp

Financial application with modular monolith architecture, CQRS, Kafka, and gRPC.

## Architecture

```mermaid
graph TB
    subgraph "External Clients"
        Client[HTTP/REST Client]
    end

    subgraph "API Gateway Layer - Port 5000"
        Gateway[Ocelot API Gateway<br/>REST API Endpoints]
    end

    subgraph "Write Side - CQRS Command"
        WriteService[Write Service<br/>Port 5001 HTTP<br/>Port 5011 gRPC]
        WriteRepo[(Write Repository<br/>Event Store)]
        OutboxDB[(Outbox Pattern<br/>Transactional)]
        OutboxProcessor[Outbox Processor<br/>Background Service]
    end

    subgraph "Read Side - CQRS Query"
        ReadService[Read Service<br/>Port 5002 HTTP<br/>Port 5012 gRPC]
        ReadRepo[(Read Repository<br/>Optimized Views)]
        Cache[Memory Cache<br/>Fast Queries]
    end

    subgraph "Event-Driven Infrastructure"
        Kafka[Apache Kafka<br/>Port 9092]
        KafkaUI[Kafka UI<br/>Port 8080]
        CDC[CDC Events<br/>Topic: cdc.payment]
        CDCConsumer[CDC Consumer<br/>Background Service]
    end

    subgraph "Infrastructure Services"
        Zookeeper[Zookeeper<br/>Kafka Coordination]
    end

    Client -->|HTTP/REST| Gateway
    Gateway -->|gRPC| WriteService
    Gateway -->|gRPC| ReadService

    WriteService -->|Save Events| WriteRepo
    WriteService -->|Store CDC| OutboxDB
    OutboxProcessor -->|Poll Every 5s| OutboxDB
    OutboxProcessor -->|Publish| CDC

    CDC --> Kafka
    Kafka --> CDCConsumer
    CDCConsumer -->|Update| ReadRepo

    ReadService --> ReadRepo
    ReadService --> Cache

    Kafka --- KafkaUI
    Kafka --- Zookeeper

    style Client fill:#e1f5ff
    style Gateway fill:#fff4e1
    style WriteService fill:#ffe1e1
    style ReadService fill:#e1ffe1
    style Kafka fill:#f0e1ff
    style OutboxProcessor fill:#ffe1f0
    style CDCConsumer fill:#e1fff0
```

### Architecture Overview

- **API Gateway** (Port 5000) - Ocelot gateway exposing REST API, communicates via gRPC
- **Write Service** (Port 5001, gRPC 5011) - Handles commands, Event Sourcing, Outbox Pattern
- **Read Service** (Port 5002, gRPC 5012) - Handles queries, optimized for reads, caching
- **Kafka** (Port 9092) - Event-driven CDC synchronization between Write and Read
- **Kafka UI** (Port 8080) - Monitoring and management interface

### Data Flow

1. **Command Flow (Write)**:
   - Client → Gateway (REST) → Write Service (gRPC)
   - Write Service → Event Store + Outbox DB
   - Outbox Processor → Kafka (CDC Events)

2. **Query Flow (Read)**:
   - Client → Gateway (REST) → Read Service (gRPC)
   - Read Service → Cache/Read Repository

3. **Synchronization Flow**:
   - Kafka CDC Events → CDC Consumer
   - CDC Consumer → Read Repository (automatic updates)

### Key Patterns

- **CQRS**: Separate Write and Read models with physical separation
- **Event Sourcing**: All changes stored as events in Event Store
- **Outbox Pattern**: Reliable event delivery with transactional consistency
- **CDC**: Change Data Capture for automatic read model synchronization
- **API Gateway**: Single entry point with gRPC backend communication

## Project Structure

```mermaid
graph LR
    subgraph "WirexApp Solution"
        subgraph "Domain Layer"
            Domain[WirexApp.Domain<br/>- AggregateRoot<br/>- Payment Entity<br/>- UserAccount<br/>- BonusAccount<br/>- Domain Events]
        end

        subgraph "Application Layer"
            App[WirexApp.Application<br/>- CQRS Commands<br/>- CQRS Queries<br/>- Command Handlers<br/>- Query Handlers<br/>- FluentValidation]
        end

        subgraph "Infrastructure Layer"
            Infra[WirexApp.Infrastructure<br/>- Event Store<br/>- Repositories<br/>- Kafka Integration<br/>- CDC Publisher<br/>- Outbox Pattern<br/>- CDC Consumers]
        end

        subgraph "Service Layer"
            GW[WirexApp.Gateway<br/>- REST Controllers<br/>- gRPC Clients<br/>- Ocelot Config<br/>- Gateway Service]
            WS[WirexApp.WriteService<br/>- gRPC Server<br/>- Command API<br/>- Outbox Processor]
            RS[WirexApp.ReadService<br/>- gRPC Server<br/>- Query API<br/>- CDC Consumer<br/>- Caching]
        end

        subgraph "Contracts"
            Proto[Protobuf Contracts<br/>- payment.proto<br/>- gRPC Services<br/>- Message Types]
        end
    end

    App --> Domain
    Infra --> App
    Infra --> Domain
    GW --> App
    GW --> Proto
    WS --> App
    WS --> Infra
    WS --> Proto
    RS --> App
    RS --> Infra
    RS --> Proto

    style Domain fill:#e1f5ff
    style App fill:#fff4e1
    style Infra fill:#ffe1e1
    style GW fill:#e1ffe1
    style WS fill:#f0e1ff
    style RS fill:#ffe1f0
    style Proto fill:#e1fff0
```

### Technology Stack Diagram

```mermaid
graph TB
    subgraph "Frontend/Client"
        HTTP[HTTP/REST Client]
    end

    subgraph "API Layer"
        Ocelot[Ocelot API Gateway<br/>Routing & Aggregation]
    end

    subgraph "Communication Protocol"
        gRPC[gRPC with Protobuf<br/>High Performance RPC]
    end

    subgraph "Business Logic"
        MediatR[MediatR<br/>CQRS Mediator]
        FluentVal[FluentValidation<br/>Input Validation]
        AutoMapper[AutoMapper<br/>Object Mapping]
    end

    subgraph "Data Layer"
        EventStore[Event Store<br/>Event Sourcing]
        ReadDB[Read Database<br/>Denormalized Views]
        Cache[In-Memory Cache<br/>Performance]
    end

    subgraph "Messaging"
        Kafka2[Apache Kafka<br/>Event Streaming]
        CDC2[CDC Pattern<br/>Data Sync]
        Outbox2[Outbox Pattern<br/>Reliable Delivery]
    end

    subgraph "Observability"
        Serilog[Serilog<br/>Structured Logging]
        Health[Health Checks<br/>Service Monitoring]
        Swagger[Swagger UI<br/>API Documentation]
    end

    subgraph "Infrastructure"
        Docker[Docker<br/>Containerization]
        DockerCompose[Docker Compose<br/>Orchestration]
        GHA[GitHub Actions<br/>CI/CD Pipeline]
    end

    HTTP --> Ocelot
    Ocelot --> gRPC
    gRPC --> MediatR
    MediatR --> FluentVal
    MediatR --> EventStore
    MediatR --> ReadDB
    EventStore --> Outbox2
    Outbox2 --> Kafka2
    Kafka2 --> CDC2
    CDC2 --> ReadDB
    ReadDB --> Cache

    style HTTP fill:#e1f5ff
    style Ocelot fill:#fff4e1
    style gRPC fill:#ffe1e1
    style Kafka2 fill:#f0e1ff
    style Docker fill:#e1ffe1
```

## Quick Start

```bash
# Start all services
docker-compose up --build

# Run in background
docker-compose up -d --build
```

## Verify

```bash
# Health checks
curl http://localhost:5000/health  # Gateway
curl http://localhost:5001/health  # Write Service
curl http://localhost:5002/health  # Read Service

# Access
# Gateway: http://localhost:5000 (Use this!)
# Write:   http://localhost:5001
# Read:    http://localhost:5002
```

## Sequence Diagrams

### Create Payment Flow

```mermaid
sequenceDiagram
    participant Client
    participant Gateway
    participant WriteService
    participant EventStore
    participant OutboxDB
    participant OutboxProcessor
    participant Kafka
    participant CDCConsumer
    participant ReadService

    Client->>Gateway: POST /api/payments/user/{id}
    Gateway->>WriteService: CreatePayment (gRPC)
    WriteService->>WriteService: Validate Command
    WriteService->>EventStore: Save Domain Events
    WriteService->>OutboxDB: Store CDC Event (Transactional)
    WriteService-->>Gateway: Success Response
    Gateway-->>Client: 202 Accepted

    Note over OutboxProcessor: Background Service (Every 5s)
    OutboxProcessor->>OutboxDB: Poll Pending Messages
    OutboxProcessor->>Kafka: Publish CDC Event
    OutboxProcessor->>OutboxDB: Mark as Published

    Kafka->>CDCConsumer: CDC Event Received
    CDCConsumer->>ReadService: Update Read Model
    ReadService->>ReadService: Clear Cache
    CDCConsumer-->>Kafka: Acknowledge

    Note over Client,ReadService: Read Model is now synchronized
```

### Query Payment Flow

```mermaid
sequenceDiagram
    participant Client
    participant Gateway
    participant ReadService
    participant Cache
    participant ReadDB

    Client->>Gateway: GET /api/payments/{id}
    Gateway->>ReadService: GetPayment (gRPC)
    ReadService->>Cache: Check Cache

    alt Cache Hit
        Cache-->>ReadService: Return Cached Data
        ReadService-->>Gateway: Payment Data
    else Cache Miss
        ReadService->>ReadDB: Query Database
        ReadDB-->>ReadService: Payment Data
        ReadService->>Cache: Store in Cache
        ReadService-->>Gateway: Payment Data
    end

    Gateway-->>Client: 200 OK + Payment Data
```

## Deployment Architecture

```mermaid
graph TB
    subgraph "Docker Host / Kubernetes Cluster"
        subgraph "Network: wirexapp-network"
            subgraph "Gateway Container"
                GW[Gateway Service<br/>:5000]
            end

            subgraph "Write Container"
                WR[Write Service<br/>:5001 (HTTP)<br/>:5011 (gRPC)]
            end

            subgraph "Read Container"
                RD[Read Service<br/>:5002 (HTTP)<br/>:5012 (gRPC)]
            end

            subgraph "Kafka Infrastructure"
                ZK[Zookeeper<br/>:2181]
                KF[Kafka Broker<br/>:9092]
                UI[Kafka UI<br/>:8080]
            end
        end

        subgraph "Volumes"
            V1[kafka-data]
            V2[zookeeper-data]
            V3[event-store-data]
        end
    end

    subgraph "External"
        LB[Load Balancer]
        Users[Users/Clients]
        GHCR[GitHub Container Registry<br/>ghcr.io]
    end

    subgraph "CI/CD"
        GHA[GitHub Actions]
        Security[Security Scanning]
    end

    Users --> LB
    LB --> GW
    GW -.gRPC.-> WR
    GW -.gRPC.-> RD
    WR --> KF
    KF --> RD
    KF --> ZK
    KF --> V1
    ZK --> V2
    WR --> V3

    GHA -->|Build & Push| GHCR
    GHA -->|Deploy| GW
    GHA -->|Deploy| WR
    GHA -->|Deploy| RD
    Security -->|Scan| GHA

    style GW fill:#fff4e1
    style WR fill:#ffe1e1
    style RD fill:#e1ffe1
    style KF fill:#f0e1ff
    style LB fill:#e1f5ff
    style GHCR fill:#ffe1f0
```

## Usage

**All requests go through the Gateway (Port 5000)**

### Create Payment
```bash
curl -X POST http://localhost:5000/api/payments/user/{userId} \
  -H "Content-Type: application/json" \
  -d '{"sourceCurrency": 1, "targetCurrency": 2, "sourceValue": 100.00}'
```

### Get Payment
```bash
curl http://localhost:5000/api/payments/{paymentId}
```

### Get All Payments
```bash
curl http://localhost:5000/api/payments
```

### Get Statistics
```bash
curl http://localhost:5000/api/payments/stats
```

## Stack

- .NET 8
- Ocelot API Gateway
- gRPC (Gateway ↔ Services)
- Kafka (CDC)
- CQRS + Event Sourcing
- Outbox Pattern
- Docker


## Stop

```bash
docker-compose down
```
