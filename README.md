# FastTechFood - Sistema de Pedidos para Lanchonete

Sistema completo de pedidos para lanchonete desenvolvido em .NET 8 com arquitetura de microsserviços, deployado no Kubernetes.

## 🏗️ Arquitetura

O sistema é composto por 3 microsserviços principais:

- **IdentityAPI**: Gerenciamento de autenticação e autorização
- **MenuAPI**: Gerenciamento do cardápio e produtos
- **OrderAPI**: Gerenciamento de pedidos e integração com sistemas de pagamento

### Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **PostgreSQL**: Banco de dados para cada microsserviço
- **RabbitMQ**: Message broker para comunicação assíncrona
- **Docker**: Containerização
- **Kubernetes**: Orquestração de containers
- **Grafana**: Monitoramento e dashboards
- **Zabbix**: Monitoramento de infraestrutura

## 🚀 Como Executar

### Desenvolvimento (recomendado)
1. **Visual Studio**: 
   - Abrir a solution `FastFood.sln`
   - Executar com Docker Compose (F5)
   - Todos os serviços sobem automaticamente

### Testes com Minikube
Para testar com Kubernetes local:

⚠️ **IMPORTANTE**: 
1. Pare o Visual Studio 
2. Limpe os containers Docker: `docker-compose down -v`
3. Execute o script:

```powershell
.\minikube-deploy.ps1 -Action deploy
```

**Para acessar as APIs no Minikube (Windows + Docker Desktop):**

**🎯 MÉTODO RECOMENDADO - Portas fixas:**
```powershell
# Cria todos os port-forwards com portas fixas de uma vez
.\minikube-deploy.ps1 -Action tunnel
```

**URLs fixas (sempre iguais):**
- **IdentityAPI**: `http://localhost:5001/swagger`
- **MenuAPI**: `http://localhost:5002/swagger`
- **OrderAPI**: `http://localhost:5003/swagger`
- **Grafana**: `http://localhost:3000`

✅ **Vantagens**: Portas nunca mudam, URLs sempre iguais, um comando só!

---

**Método alternativo - URLs dinâmicas:**

```powershell
# Obter URL do IdentityAPI
minikube service identityapi-service --url

# Obter URL do MenuAPI  
minikube service menuapi-service --url

# Obter URL do OrderAPI
minikube service orderapi-service --url

# Obter URL do Grafana
minikube service grafana --url
```

⚠️ **IMPORTANTE**: 
- Os comandos acima abrem túneis temporários. Mantenha o terminal aberto enquanto usar as APIs.
- **As portas mudam a cada execução** - sempre execute os comandos novamente para obter as URLs atuais.

**Exemplo de URLs geradas:**
- IdentityAPI: `http://127.0.0.1:XXXXX/swagger`
- MenuAPI: `http://127.0.0.1:XXXXX/swagger`
- OrderAPI: `http://127.0.0.1:XXXXX/swagger`
- Grafana: `http://127.0.0.1:XXXXX`

(As portas XXXXX são geradas dinamicamente pelo Minikube e **mudam a cada execução**)

**Comandos úteis:**
- `.\minikube-deploy.ps1 -Action tunnel` - **Portas fixas** (5001, 5002, 5003, 3000)
- `.\minikube-deploy.ps1 -Action status` - Ver status
- `.\minikube-deploy.ps1 -Action dashboard` - Dashboard Kubernetes
- `.\minikube-deploy.ps1 -Action cleanup` - Limpar recursos

### Acessando as Aplicações (Minikube)

**🎯 Método Recomendado - Portas Fixas:**
```powershell
.\minikube-deploy.ps1 -Action tunnel
```
URLs sempre iguais: `localhost:5001/swagger`, `localhost:5002/swagger`, `localhost:5003/swagger`, `localhost:3000`

**Método Alternativo 1 - URLs dinâmicas:**
```powershell
minikube service identityapi-service --url
minikube service menuapi-service --url  
minikube service orderapi-service --url
minikube service grafana --url
```

**Método Alternativo 2 - Port-forward manual (portas fixas):**
```bash
# IdentityAPI - sempre será localhost:5001
kubectl port-forward svc/identityapi-service 5001:8080

# MenuAPI - sempre será localhost:5002  
kubectl port-forward svc/menuapi-service 5002:8080

# OrderAPI - sempre será localhost:5003
kubectl port-forward svc/orderapi-service 5003:8080

# Grafana - sempre será localhost:3000
kubectl port-forward svc/grafana 3000:3000
```

Depois acesse: `http://localhost:5001/swagger`, `http://localhost:5002/swagger`, etc.

💡 **Dica**: Use o Método 2 se quiser URLs fixas que não mudam entre reinicializações!

## 📊 Monitoramento

### Grafana
- **URL**: http://localhost:3000
- **Usuário**: admin
- **Senha**: grafana123

### Zabbix
- **URL**: http://localhost:8080
- **Usuário**: Admin
- **Senha**: zabbix

## 🗄️ Banco de Dados

Cada microsserviço possui seu próprio banco PostgreSQL:

- **postgres-identity**: Dados de usuários e autenticação
- **postgres-menu**: Cardápio e produtos
- **postgres-order**: Pedidos e transações

### Credenciais do PostgreSQL
- **Usuário**: postgres
- **Senha**: postgres123

## 🔄 Message Queue

O RabbitMQ é usado para comunicação assíncrona entre os microsserviços:

- **Host**: rabbitmq-service
- **Porta**: 5672
- **Usuário**: guest
- **Senha**: guest

## 🛠️ Scripts Disponíveis

### `minikube-deploy.ps1`
Deploy completo da aplicação no Minikube para testes locais de Kubernetes.

## 📁 Estrutura do Projeto

```
fast_food_hackathon/
├── src/
│   ├── IdentityApi/          # Microsserviço de autenticação
│   ├── MenuApi/              # Microsserviço do cardápio
│   └── OrderApi/             # Microsserviço de pedidos
├── deploy/                   # Manifestos Kubernetes
│   ├── *-deployment.yaml    # Deployments
│   ├── *-service.yaml       # Services
│   ├── *-configmap.yaml     # ConfigMaps
│   └── *-pvc.yaml          # Persistent Volume Claims
├── docker-compose.yml       # Para desenvolvimento local
├── docker-compose.override.yml # Configurações específicas do Visual Studio
├── minikube-deploy.ps1      # Script para deploy no Minikube
└── FastFood.sln            # Solution do Visual Studio
```

## 🔧 Desenvolvimento Local

Para desenvolvimento local com Visual Studio:

1. Abrir `FastFood.sln`
2. Executar com Docker Compose (F5)

Para desenvolvimento sem Visual Studio:
```bash
docker-compose up -d
```

## 🚨 Troubleshooting

### Pods não inicializam
```bash
kubectl describe pod <pod-name>
kubectl logs <pod-name>
```

### Verificar recursos
```bash
kubectl get all
kubectl get pvc
kubectl get configmap
```

### Limpar tudo e recomeçar
```bash
# Para Minikube
kubectl delete --all pods,services,deployments,configmaps,pvc --namespace=default
```

## 📋 Checklist de Deploy

### Visual Studio (Desenvolvimento)
- [ ] Docker Desktop rodando
- [ ] Visual Studio 2022 instalado
- [ ] Abrir FastFood.sln e executar (F5)

### Minikube (Testes Kubernetes)
- [ ] Minikube rodando
- [ ] Executar `.\minikube-deploy.ps1`
- [ ] Todos os pods em status Running
- [ ] Serviços acessíveis via port-forward

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.