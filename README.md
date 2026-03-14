# Distributed Ordering System
A scalable **distributed microservices backend** built with **.NET 8**, following **Clean Architecture principles**, designed for high performance, observability, and secure communication.  

## 📦 Tech Stack

**Backend**
- **.NET 8 (.NET Core Web API)**
- **Entity Framework**
- **SQL Server**

**API Gateway**
- **NGINX**

**Rate Limiter**
- **Concurrency Rate Limiter**

**Tests**
- **UnitTest**

**Communication**
- **gRPC** - inter-service communication
- **REST APIs** - external clients

**Authentication & Security**
- **JWT Authentication**
- **SSO (Single Sign-On)**

**Caching**
- **In-Memory Caching**
- **Redis** - distributed caching

**Observability**
- **Prometheus** - metrics collection
- **Grafana** - dashboards & visualization
- **Loki** - centralized logging

**DevOps / Infrastructure**
- **Docker**

**Deployment**
1. Clone the repository:
```bash
   git clone git@github.com:mahmudur9/distributed-ordering-system.git
   cd distributed-ordering-system
```
2. Build the images:
```bash
   docker compose build
```
3. Run the containers:
```bash
   docker compose up -d
```
4. Access the APIs:
```text
  http://localhost:8001/swagger/index.html (ProductService)
  http://localhost:8002/swagger/index.html (OrderService)
  http://localhost:8003/swagger/index.html (PaymentService)
  http://localhost:8004/swagger/index.html (UserService)
  http://localhost:8005/swagger/index.html (ObjectStoreService)
  http://localhost:3000 (Grafana)
  http://ordering-system-loki:3100 (Loki data source)
  http://ordering-system-prometheus:9090 (Prometheus data source)
```
