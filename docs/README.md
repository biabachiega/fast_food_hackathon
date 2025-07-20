# FastTech Foods - Documentação Completa do MVP

## Índice

1. [Arquitetura MVP](./ARQUITETURA_MVP.md) - Diagrama principal e visão geral
2. [Bounded Contexts](./BOUNDED_CONTEXTS.md) - Detalhamento dos domínios DDD
3. [Diagramas Técnicos](./DIAGRAMAS_TECNICOS.md) - Sequência, componentes e deployment
4. [Decisões Arquiteturais](./DECISOES_ARQUITETURAIS.md) - ADRs e justificativas detalhadas

## Resumo Executivo

O **FastTech Foods** é uma solução MVP de sistema de fast food desenvolvida seguindo as melhores práticas de arquitetura de software moderna. A solução implementa uma arquitetura de microservices baseada em Domain-Driven Design (DDD), executando em containers Docker orquestrados por Kubernetes.

## Principais Características

### ✅ Arquitetura
- **Microservices**: 3 serviços independentes (Identity, Menu, Order)
- **DDD**: Bounded contexts bem definidos
- **Kubernetes**: Orquestração completa com Minikube
- **PostgreSQL**: Banco dedicado por contexto
- **RabbitMQ**: Comunicação assíncrona

### ✅ Monitoramento Real
- **Zabbix**: Coleta de métricas reais via HTTP endpoints
- **Grafana**: Dashboards visuais com dados autênticos
- **JSONPath**: Preprocessing para extração de métricas específicas
- **Health Checks**: Endpoints de saúde em todos os serviços

### ✅ Qualidade de Código
- **Clean Architecture**: Separação clara de responsabilidades
- **SOLID Principles**: Princípios aplicados consistentemente
- **Value Objects**: Domínio rico e type-safe
- **Aggregate Roots**: Consistência transacional

### ✅ DevOps e Infraestrutura
- **Infrastructure as Code**: Manifests Kubernetes declarativos
- **Container-First**: Aplicação completamente containerizada
- **Service Discovery**: Comunicação via DNS interno do Kubernetes
- **Persistent Storage**: Volumes persistentes para dados

## Compliance com Requisitos

### Requisitos Funcionais (6/6) ✅
1. **Pedido de Lanches**: ✅ OrderAPI com fluxo completo
2. **Acompanhamento de Pedidos**: ✅ Status tracking em tempo real
3. **Gerenciamento de Pedidos**: ✅ Aceitar/Recusar pela cozinha
4. **Gerenciamento de Clientes**: ✅ IdentityAPI com CRUD completo
5. **Gerenciamento de Produtos**: ✅ MenuAPI com categorias
6. **Acompanhamento via Admin**: ✅ Endpoints para monitoramento

### Requisitos Técnicos (5/5) ✅
1. **APIs REST**: ✅ ASP.NET Core com OpenAPI/Swagger
2. **Banco de Dados**: ✅ PostgreSQL com EF Core
3. **Documentação**: ✅ Swagger UI interativo
4. **Docker**: ✅ Containers para todos os componentes
5. **Kubernetes**: ✅ Deployment completo no Minikube

## Arquitetura de Alto Nível

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   IdentityAPI   │    │    MenuAPI      │    │   OrderAPI      │
│   Port: 5001    │    │   Port: 5002    │    │   Port: 5003    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │   Kubernetes    │
                    │   Minikube      │
                    └─────────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  PostgreSQL     │    │   RabbitMQ      │    │   Zabbix +      │
│  (3 instances)  │    │   Messaging     │    │   Grafana       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Fluxo de Pedidos

```
Cliente → OrderAPI → MenuAPI (validação) → PostgreSQL (salvar)
                ↓
        Cliente confirma pedido
                ↓
        RabbitMQ (fila de pedidos)
                ↓
        Cozinha processa pedido
                ↓
        Status atualizado (Aceito/Recusado/Finalizado)
```

## Acesso aos Serviços

### APIs Principais
- **IdentityAPI**: http://localhost:5001/swagger
- **MenuAPI**: http://localhost:5002/swagger  
- **OrderAPI**: http://localhost:5003/swagger

### Monitoramento
- **Grafana**: http://localhost:3000 (admin/grafana123)
- **Zabbix**: http://localhost:8080 (Admin/zabbix)
- **RabbitMQ**: http://localhost:15672 (fasttech/fasttech123)

## Comandos de Deploy

```powershell
# Iniciar Minikube
minikube start

# Deploy completo
kubectl apply -f deploy/

# Port-forwards
kubectl port-forward svc/identityapi-service 5001:8080
kubectl port-forward svc/menuapi-service 5002:8080  
kubectl port-forward svc/orderapi-service 5003:8080
kubectl port-forward svc/grafana-service 3000:3000
kubectl port-forward svc/zabbix-web-service 8080:8080
kubectl port-forward svc/rabbitmq-service 15672:15672
```

## Estrutura de Pastas

```
fast_food_hackathon/
├── docs/                           # Esta documentação
├── src/
│   ├── IdentityApi/               # Microservice de identidade
│   ├── MenuApi/                   # Microservice de cardápio
│   └── OrderApi/                  # Microservice de pedidos
├── deploy/                        # Manifests Kubernetes
├── docker/                        # Dockerfiles e configs
└── minikube-deploy.ps1           # Script de automação
```

## Próximos Passos para Produção

### Melhorias de Segurança
- [ ] JWT Authentication
- [ ] API Gateway (Ambassador/Istio)
- [ ] Network Policies
- [ ] Secrets Management

### Escalabilidade
- [ ] Horizontal Pod Autoscaler
- [ ] Cluster Autoscaler  
- [ ] Database sharding
- [ ] Redis cache layer

### Observabilidade Avançada
- [ ] Distributed tracing (Jaeger)
- [ ] Prometheus metrics
- [ ] ELK Stack para logs
- [ ] Service mesh (Istio)

### CI/CD
- [ ] GitHub Actions
- [ ] Helm charts
- [ ] Blue/Green deployment
- [ ] Automated testing pipeline

## Contato e Suporte

Para questões sobre a arquitetura ou implementação, consulte:
- **Documentação técnica**: Arquivos nesta pasta `docs/`
- **Código fonte**: Pasta `src/` com comentários detalhados
- **Configuração**: Manifests na pasta `deploy/`

---

**FastTech Foods MVP** - Sistema de Fast Food com Arquitetura de Microservices
*Desenvolvido seguindo práticas de Clean Architecture, DDD e DevOps*
