# WirexApp

Financial application with modular monolith architecture, CQRS, and Kafka.

## Architecture

- **Write Service** (Port 5001) - Commands
- **Read Service** (Port 5002-5003) - Queries
- **Kafka** (Port 9092) - CDC Events
- **Kafka UI** (Port 8080) - Monitoring

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
curl http://localhost:5001/health  # Write Service
curl http://localhost:5002/health  # Read Service

# Swagger UI
# Write: http://localhost:5001
# Read:  http://localhost:5002
```

## Usage

### Create Payment (Write)
```bash
curl -X POST http://localhost:5001/api/v1/payments/user/{userId} \
  -H "Content-Type: application/json" \
  -d '{"sourceCurrency": 1, "targetCurrency": 2, "sourceValue": 100.00}'
```

### Get Payment (Read)
```bash
curl http://localhost:5002/api/v1/payments/{paymentId}
```

### Get Statistics
```bash
curl http://localhost:5002/api/v1/payments/stats
```

## Stack

- .NET 8
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
