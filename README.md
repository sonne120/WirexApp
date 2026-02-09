# WirexApp

Financial application with modular monolith architecture, CQRS, Kafka, and gRPC.

## Architecture

- **API Gateway** (Port 5000) - Entry point (REST → gRPC)
- **Write Service** (Port 5001, gRPC 5011) - Commands
- **Read Service** (Port 5002, gRPC 5012) - Queries
- **Kafka** (Port 9092) - CDC Events
- **Kafka UI** (Port 8080) - Monitoring

```
Client (HTTP/REST) → Gateway → gRPC → Write/Read Services
                                  ↓
                                Kafka (CDC)
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

## Documentation

- [Deployment Guide](DEPLOYMENT_GUIDE.md)
- [CDC Implementation](CDC_IMPLEMENTATION.md)

## Stop

```bash
docker-compose down
```
