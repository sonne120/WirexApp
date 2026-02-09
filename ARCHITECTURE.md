# WirexApp Architecture Documentation

## Table of Contents
- [System Overview](#system-overview)
- [Architecture Patterns](#architecture-patterns)
- [Component Diagram](#component-diagram)
- [Data Flow](#data-flow)
- [Deployment](#deployment)
- [CI/CD Pipeline](#cicd-pipeline)

## System Overview

WirexApp is a **multi-currency payment processing platform** that handles cross-currency transactions with real-time exchange rates. Built with a modular monolith architecture, implementing CQRS, Event Sourcing, and CDC patterns for scalability and reliability.

**Core Features:**
- Multi-currency payment processing
- Real-time currency conversion with exchange rates
- User account management with bonus system
- Payment status tracking and notifications
- Event-driven architecture for data synchronization

### High-Level Architecture

```mermaid
C4Context
    title System Context Diagram - WirexApp

    Person(user, "User", "Financial application user")
    System(wirexapp, "WirexApp", "Financial payment processing system")
    System_Ext(kafka, "Apache Kafka", "Event streaming platform")

    Rel(user, wirexapp, "Uses", "HTTPS/REST")
    Rel(wirexapp, kafka, "Publishes/Subscribes", "CDC Events")
```

## Architecture Patterns

### 1. CQRS (Command Query Responsibility Segregation)

**Write Side (Commands)**:
- Handles state-changing operations
- Uses Event Sourcing for persistence
- Stores events in Event Store
- Publishes CDC events via Outbox Pattern

**Read Side (Queries)**:
- Handles read-only operations
- Uses denormalized views for performance
- Implements caching strategies
- Updated automatically via CDC consumers

```mermaid
graph LR
    subgraph "CQRS Pattern"
        Command[Commands<br/>Create, Update, Delete] --> WriteModel[Write Model<br/>Event Sourcing]
        Query[Queries<br/>Get, List, Stats] --> ReadModel[Read Model<br/>Denormalized Views]
        WriteModel -->|CDC Events| EventBus[Event Bus<br/>Kafka]
        EventBus -->|Update| ReadModel
    end

    style Command fill:#ffe1e1
    style Query fill:#e1ffe1
    style EventBus fill:#f0e1ff
```

### 2. Event Sourcing

All state changes are stored as a sequence of events.

```mermaid
graph TB
    subgraph "Event Sourcing Flow"
        Command[Command] --> Aggregate[Aggregate]
        Aggregate -->|Generate| Events[Domain Events]
        Events --> EventStore[(Event Store)]
        EventStore --> Replay[Event Replay]
        Replay --> CurrentState[Current State]
    end

    style Events fill:#fff4e1
    style EventStore fill:#e1f5ff
```

**Benefits**:
- Complete audit trail
- Time travel debugging
- Event replay for state reconstruction
- Immutable event history

### 3. Outbox Pattern

Ensures reliable event delivery with transactional consistency.

```mermaid
sequenceDiagram
    participant Command
    participant Aggregate
    participant DB as Database
    participant Outbox as Outbox Table
    participant Processor as Outbox Processor
    participant Kafka

    Command->>Aggregate: Execute Command
    Aggregate->>Aggregate: Generate Events

    Note over DB,Outbox: Single Transaction
    Aggregate->>DB: Save to Event Store
    Aggregate->>Outbox: Insert CDC Event

    Note over Processor: Background Service (5s interval)
    Processor->>Outbox: Poll Pending Messages
    Processor->>Kafka: Publish Event
    Processor->>Outbox: Mark as Published
```

**Key Features**:
- Atomic write to database and outbox
- At-least-once delivery guarantee
- Retry mechanism with exponential backoff
- No message loss during Kafka downtime

### 4. Change Data Capture (CDC)

Automatically synchronizes Write and Read models.

```mermaid
graph LR
    subgraph "CDC Pattern"
        Write[Write Service] -->|Save| EventStore[(Event Store)]
        Write -->|CDC Event| Outbox[(Outbox)]
        Outbox -->|Publish| Kafka[Kafka Topic<br/>cdc.payment]
        Kafka -->|Subscribe| Consumer[CDC Consumer]
        Consumer -->|Update| ReadDB[(Read Database)]
    end

    style Kafka fill:#f0e1ff
    style Consumer fill:#e1ffe1
```

**CDC Event Structure**:
```json
{
  "entityType": "Payment",
  "operation": "Create | Update | Delete",
  "data": { /* entity data */ },
  "timestamp": "2026-02-09T18:30:00Z",
  "version": 1
}
```

## Component Diagram

```mermaid
graph TB
    subgraph "Client Layer"
        Mobile[Mobile App]
        Web[Web App]
        API_Client[API Client]
    end

    subgraph "Gateway Layer"
        Gateway[API Gateway<br/>Ocelot]
        GatewayController[REST Controllers]
        GatewayService[Gateway Service]
        gRPCClient[gRPC Clients]
    end

    subgraph "Write Service"
        WriteController[Write gRPC Service]
        WriteMediator[MediatR]
        CommandHandler[Command Handlers]
        WriteRepo[Write Repository]
        EventStoreDB[(Event Store)]
        OutboxRepo[Outbox Repository]
        OutboxDB[(Outbox DB)]
        OutboxProc[Outbox Processor]
    end

    subgraph "Read Service"
        ReadController[Read gRPC Service]
        ReadMediator[MediatR]
        QueryHandler[Query Handlers]
        ReadRepo[Read Service]
        CacheLayer[Memory Cache]
        ReadDB[(Read Database)]
        CDCConsumer[CDC Consumer]
    end

    subgraph "Infrastructure"
        Kafka[Apache Kafka]
        Zookeeper[Zookeeper]
        KafkaUI[Kafka UI]
    end

    Mobile --> Gateway
    Web --> Gateway
    API_Client --> Gateway
    Gateway --> GatewayController
    GatewayController --> GatewayService
    GatewayService --> gRPCClient

    gRPCClient -->|gRPC| WriteController
    WriteController --> WriteMediator
    WriteMediator --> CommandHandler
    CommandHandler --> WriteRepo
    WriteRepo --> EventStoreDB
    WriteRepo --> OutboxRepo
    OutboxRepo --> OutboxDB
    OutboxProc --> OutboxDB
    OutboxProc --> Kafka

    gRPCClient -->|gRPC| ReadController
    ReadController --> ReadMediator
    ReadMediator --> QueryHandler
    QueryHandler --> ReadRepo
    ReadRepo --> CacheLayer
    ReadRepo --> ReadDB
    Kafka --> CDCConsumer
    CDCConsumer --> ReadDB

    Kafka --- Zookeeper
    Kafka --- KafkaUI

    style Gateway fill:#fff4e1
    style WriteController fill:#ffe1e1
    style ReadController fill:#e1ffe1
    style Kafka fill:#f0e1ff
```

## Data Flow

### Write Flow (Command Processing)

```mermaid
flowchart TD
    Start([Client Request]) --> Gateway[API Gateway]
    Gateway --> ValidateReq{Validate Request}
    ValidateReq -->|Invalid| Error1[Return 400 Bad Request]
    ValidateReq -->|Valid| gRPC[Send gRPC Request]

    gRPC --> WriteService[Write Service]
    WriteService --> Mediator[MediatR]
    Mediator --> Validator{FluentValidation}
    Validator -->|Invalid| Error2[Return Validation Error]
    Validator -->|Valid| Handler[Command Handler]

    Handler --> Aggregate[Load/Create Aggregate]
    Aggregate --> Business{Business Logic}
    Business -->|Fail| Error3[Domain Exception]
    Business -->|Success| Events[Generate Domain Events]

    Events --> Transaction{Begin Transaction}
    Transaction --> SaveEvents[Save to Event Store]
    SaveEvents --> SaveOutbox[Save to Outbox]
    Transaction --> Commit{Commit Transaction}

    Commit -->|Fail| Rollback[Rollback]
    Rollback --> Error4[Return 500 Error]
    Commit -->|Success| Response[Return 202 Accepted]

    Response --> Background[Background Processing]
    Background --> OutboxPoll[Outbox Processor Polls]
    OutboxPoll --> PublishKafka[Publish to Kafka]
    PublishKafka --> MarkPublished[Mark as Published]

    MarkPublished --> End([Complete])
    Error1 --> End
    Error2 --> End
    Error3 --> End
    Error4 --> End

    style Start fill:#e1f5ff
    style Gateway fill:#fff4e1
    style WriteService fill:#ffe1e1
    style PublishKafka fill:#f0e1ff
    style End fill:#e1ffe1
```

### Read Flow (Query Processing)

```mermaid
flowchart TD
    Start([Client Request]) --> Gateway[API Gateway]
    Gateway --> gRPC[Send gRPC Request]
    gRPC --> ReadService[Read Service]

    ReadService --> Mediator[MediatR]
    Mediator --> Handler[Query Handler]
    Handler --> CheckCache{Check Cache}

    CheckCache -->|Hit| CacheData[Return from Cache]
    CheckCache -->|Miss| QueryDB[Query Database]

    QueryDB --> Found{Data Found?}
    Found -->|No| NotFound[Return 404]
    Found -->|Yes| StoreCache[Store in Cache]

    StoreCache --> ReturnData[Return Data]
    CacheData --> ReturnData
    ReturnData --> Response[200 OK + Data]
    Response --> End([Complete])
    NotFound --> End

    style Start fill:#e1f5ff
    style Gateway fill:#fff4e1
    style ReadService fill:#e1ffe1
    style CheckCache fill:#ffe1f0
    style End fill:#e1ffe1
```

### CDC Synchronization Flow

```mermaid
flowchart TD
    Start([Outbox Processor]) --> Poll[Poll Outbox Table]
    Poll --> HasMessages{Has Pending Messages?}

    HasMessages -->|No| Wait[Wait 5 seconds]
    Wait --> Poll

    HasMessages -->|Yes| GetMessage[Get Message Batch]
    GetMessage --> Transform[Transform to CDC Event]
    Transform --> Publish[Publish to Kafka]

    Publish --> Success{Publish Success?}
    Success -->|No| Retry{Retry Count < 3?}
    Retry -->|Yes| Backoff[Exponential Backoff]
    Backoff --> Publish
    Retry -->|No| MarkFailed[Mark as Failed]

    Success -->|Yes| MarkPublished[Mark as Published]
    MarkPublished --> NextMessage{More Messages?}
    MarkFailed --> NextMessage

    NextMessage -->|Yes| GetMessage
    NextMessage -->|No| Wait

    Publish -.-> KafkaTopic[Kafka Topic: cdc.payment]
    KafkaTopic -.-> Consumer[CDC Consumer]
    Consumer --> ProcessEvent[Process CDC Event]

    ProcessEvent --> OperationType{Operation Type}
    OperationType -->|Create| InsertRead[Insert to Read DB]
    OperationType -->|Update| UpdateRead[Update Read DB]
    OperationType -->|Delete| DeleteRead[Delete from Read DB]

    InsertRead --> ClearCache[Clear Cache]
    UpdateRead --> ClearCache
    DeleteRead --> ClearCache

    ClearCache --> Acknowledge[Acknowledge Kafka]
    Acknowledge --> Complete([Synchronization Complete])

    style KafkaTopic fill:#f0e1ff
    style Consumer fill:#e1ffe1
    style Complete fill:#e1f5ff
```

## Deployment

### Docker Compose Deployment

```mermaid
graph TB
    subgraph "Docker Host"
        subgraph "wirexapp-network"
            Gateway[Gateway Container<br/>wirexapp-gateway:latest<br/>Ports: 5000]
            Write[Write Service Container<br/>wirexapp-write-service:latest<br/>Ports: 5001, 5011]
            Read[Read Service Container<br/>wirexapp-read-service:latest<br/>Ports: 5002, 5012]

            Zookeeper[Zookeeper Container<br/>confluentinc/cp-zookeeper<br/>Port: 2181]
            Kafka[Kafka Container<br/>confluentinc/cp-kafka<br/>Port: 9092]
            KafkaUI[Kafka UI Container<br/>provectuslabs/kafka-ui<br/>Port: 8080]
        end

        subgraph "Volumes"
            V1[kafka-data]
            V2[zookeeper-data]
            V3[logs]
        end
    end

    Gateway -.-> Write
    Gateway -.-> Read
    Write --> Kafka
    Kafka --> Read
    Kafka --> Zookeeper
    Kafka --> V1
    Zookeeper --> V2
    Gateway --> V3
    Write --> V3
    Read --> V3
    KafkaUI --> Kafka

    style Gateway fill:#fff4e1
    style Write fill:#ffe1e1
    style Read fill:#e1ffe1
    style Kafka fill:#f0e1ff
```

### Health Checks

```mermaid
graph LR
    subgraph "Health Check System"
        HC[Health Check Endpoint]
        DB[Database Check]
        Kafka[Kafka Check]
        Memory[Memory Check]

        HC --> DB
        HC --> Kafka
        HC --> Memory

        DB -->|Healthy| Green1[✅ Green]
        DB -->|Unhealthy| Red1[❌ Red]

        Kafka -->|Healthy| Green2[✅ Green]
        Kafka -->|Unhealthy| Red2[❌ Red]

        Memory -->|Healthy| Green3[✅ Green]
        Memory -->|Unhealthy| Red3[❌ Red]
    end

    style Green1 fill:#e1ffe1
    style Green2 fill:#e1ffe1
    style Green3 fill:#e1ffe1
    style Red1 fill:#ffe1e1
    style Red2 fill:#ffe1e1
    style Red3 fill:#ffe1e1
```

## CI/CD Pipeline

```mermaid
graph TB
    subgraph "Source Control"
        GitHub[GitHub Repository]
        PR[Pull Request]
        Push[Push to Main]
    end

    subgraph "GitHub Actions - CI"
        Trigger{Trigger Event}
        Checkout[Checkout Code]

        subgraph "Build & Test"
            Restore[Restore Dependencies]
            Build[Build .NET Projects]
            Test[Run Tests]
            Validate[Code Analysis]
        end

        subgraph "Security Scanning"
            DepScan[Dependency Scan]
            SecretScan[Secret Detection]
            ImageScan[Docker Image Scan]
            LicenseScan[License Compliance]
        end
    end

    subgraph "GitHub Actions - CD"
        subgraph "Docker Build"
            BuildGW[Build Gateway Image]
            BuildWS[Build Write Service]
            BuildRS[Build Read Service]
        end

        Push_Registry[Push to GHCR]
        Tag[Tag Images]
    end

    subgraph "Deployment"
        Deploy{Deployment Type}
        Auto[Auto Deploy to Prod]
        Manual[Manual Deploy]

        SSH[SSH to Server]
        Pull[Pull Images]
        Compose[Docker Compose Up]
        HealthCheck[Health Checks]
        Rollback{Success?}
    end

    subgraph "Container Registry"
        GHCR[GitHub Container Registry<br/>ghcr.io]
    end

    GitHub --> Trigger
    PR --> Trigger
    Push --> Trigger

    Trigger --> Checkout
    Checkout --> Restore
    Restore --> Build
    Build --> Test
    Test --> Validate

    Validate --> DepScan
    DepScan --> SecretScan
    SecretScan --> ImageScan
    ImageScan --> LicenseScan

    LicenseScan -->|Success| BuildGW
    BuildGW --> BuildWS
    BuildWS --> BuildRS
    BuildRS --> Tag
    Tag --> Push_Registry
    Push_Registry --> GHCR

    GHCR --> Deploy
    Deploy --> Auto
    Deploy --> Manual

    Auto --> SSH
    Manual --> SSH
    SSH --> Pull
    Pull --> Compose
    Compose --> HealthCheck
    HealthCheck --> Rollback

    Rollback -->|Success| Complete([✅ Deployed])
    Rollback -->|Failure| RollbackAction[Rollback Previous Version]
    RollbackAction --> Failed([❌ Failed])

    style GitHub fill:#e1f5ff
    style BuildGW fill:#fff4e1
    style BuildWS fill:#ffe1e1
    style BuildRS fill:#e1ffe1
    style GHCR fill:#f0e1ff
    style Complete fill:#e1ffe1
    style Failed fill:#ffe1e1
```

### CI/CD Workflow Files

1. **ci-cd.yml** - Main CI/CD pipeline
   - Triggers on push to main/master/develop
   - Builds and tests all services
   - Creates Docker images
   - Pushes to GitHub Container Registry
   - Auto-deploys to production

2. **docker-compose-deploy.yml** - Manual deployment
   - Manual trigger with environment selection
   - Deploys using docker-compose
   - Comprehensive health checks
   - Automatic rollback on failure

3. **security-scan.yml** - Security scanning
   - Dependency vulnerability scanning
   - Docker image security analysis
   - Secret detection
   - License compliance
   - Weekly scheduled scans

## Performance Considerations

### Caching Strategy

```mermaid
graph LR
    Request[Request] --> Cache{Cache Check}
    Cache -->|Hit| Return1[Return Cached Data<br/>~1ms]
    Cache -->|Miss| DB[Query Database<br/>~50ms]
    DB --> Store[Store in Cache]
    Store --> Return2[Return Data]

    style Return1 fill:#e1ffe1
    style DB fill:#ffe1e1
```

**Cache Configuration**:
- In-memory caching (MemoryCache)
- TTL: 5 minutes for read models
- Invalidation on CDC events
- Cache-aside pattern

### Scalability

**Horizontal Scaling**:
- Read Service: Multiple instances behind load balancer
- Write Service: Single instance (event sourcing consistency)
- Kafka: Multiple brokers for partitioning

**Vertical Scaling**:
- Increase container resources
- Optimize database queries
- Tune Kafka partitions

## Monitoring & Observability

```mermaid
graph TB
    subgraph "Application"
        Services[Services]
        Logs[Serilog Structured Logs]
        Health[Health Endpoints]
    end

    subgraph "Monitoring Stack"
        Logs --> Elasticsearch[Elasticsearch]
        Elasticsearch --> Kibana[Kibana Dashboard]

        Health --> Prometheus[Prometheus]
        Prometheus --> Grafana[Grafana Dashboard]

        Services --> Metrics[Custom Metrics]
        Metrics --> Prometheus
    end

    subgraph "Alerting"
        Grafana --> Alerts[Alert Manager]
        Alerts --> Slack[Slack Notifications]
        Alerts --> Email[Email Notifications]
    end

    style Services fill:#e1f5ff
    style Grafana fill:#f0e1ff
    style Alerts fill:#ffe1e1
```

## Security

### Authentication & Authorization

```mermaid
graph LR
    Client[Client] --> Gateway[API Gateway]
    Gateway --> Auth{Validate JWT}
    Auth -->|Invalid| Reject[401 Unauthorized]
    Auth -->|Valid| CheckRole{Check Role}
    CheckRole -->|Forbidden| Deny[403 Forbidden]
    CheckRole -->|Allowed| Service[Forward to Service]

    style Auth fill:#fff4e1
    style Reject fill:#ffe1e1
    style Deny fill:#ffe1e1
    style Service fill:#e1ffe1
```

### Security Measures

- JWT-based authentication
- Role-based authorization (RBAC)
- API rate limiting
- Input validation with FluentValidation
- SQL injection prevention (parameterized queries)
- XSS protection
- CORS configuration
- HTTPS/TLS encryption
- Secret management (environment variables)
- Docker security scanning
- Dependency vulnerability scanning

## References

- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [CDC Pattern](https://en.wikipedia.org/wiki/Change_data_capture)
- [gRPC Documentation](https://grpc.io/docs/)
- [Ocelot API Gateway](https://ocelot.readthedocs.io/)
