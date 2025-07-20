# 🚀 Guia de Deploy no Minikube - FastTechFood

## 📋 Pré-requisitos
- Minikube instalado e funcionando
- kubectl configurado
- Docker Desktop rodando

## 🎯 Como Usar o Minikube

### 1. 🔧 Comandos Básicos do Script

```powershell
# Verificar status do Minikube e recursos
.\minikube-deploy.ps1 status

# Iniciar o Minikube (se não estiver rodando)
.\minikube-deploy.ps1 start

# Fazer deploy completo das aplicações
.\minikube-deploy.ps1 deploy

# Abrir dashboard do Kubernetes
.\minikube-deploy.ps1 dashboard

# Criar túnel para acesso via localhost
.\minikube-deploy.ps1 tunnel

# Ver logs dos pods
.\minikube-deploy.ps1 logs

# Parar o Minikube
.\minikube-deploy.ps1 stop

# Limpar todos os recursos
.\minikube-deploy.ps1 cleanup
```

### 2. 🌐 Acessando as Aplicações

O Minikube usa NodePort para expor os serviços. Para acessar:

**IP do Minikube:** `192.168.49.2`

**Portas dos Serviços:**
- **IdentityAPI:** `http://192.168.49.2:32123/swagger`
- **MenuAPI:** `http://192.168.49.2:31908/swagger`
- **OrderAPI:** `http://192.168.49.2:30295/swagger`
- **Grafana:** `http://192.168.49.2:31069`
- **Zabbix:** `http://192.168.49.2:32559`

### 3. 🚇 Usando o Túnel do Minikube

Para acessar via localhost, use o túnel:

```powershell
# Em uma janela separada do PowerShell
.\minikube-deploy.ps1 tunnel
```

Com o túnel ativo, você pode acessar via:
- `http://localhost:5001/swagger` (IdentityAPI)
- `http://localhost:5002/swagger` (MenuAPI) 
- `http://localhost:5003/swagger` (OrderAPI)

### 4. 📊 Dashboard do Kubernetes

```powershell
.\minikube-deploy.ps1 dashboard
```

Isso abrirá o dashboard web do Kubernetes onde você pode:
- Ver status de todos os recursos
- Visualizar logs
- Monitorar métricas
- Gerenciar deployments

### 5. 🔍 Comandos de Debug

```powershell
# Ver todos os pods
kubectl get pods

# Ver detalhes de um pod específico
kubectl describe pod <pod-name>

# Ver logs de um pod
kubectl logs <pod-name>

# Executar comando dentro de um pod
kubectl exec -it <pod-name> -- /bin/bash

# Ver serviços e suas portas
kubectl get services
```

### 6. 🛠️ Diferenças do Minikube vs Kubernetes Regular

**No Minikube:**
- As imagens são construídas localmente usando `docker build`
- Usa `imagePullPolicy: Never` para não baixar do registry
- Acesso via NodePort com IP do Minikube
- Ideal para desenvolvimento local

**No Kubernetes Regular:**
- Usa imagens do Docker Hub
- `imagePullPolicy: Always` para sempre buscar imagens atualizadas
- Acesso via LoadBalancer ou Ingress
- Ideal para produção

### 7. 🏗️ Processo de Deploy no Minikube

Quando você executa `.\minikube-deploy.ps1 deploy`, o script:

1. **Configura o ambiente Docker** para usar o registry do Minikube
2. **Constrói as imagens** localmente no Minikube
3. **Aplica os recursos** na ordem correta:
   - PVCs do PostgreSQL
   - Serviços do PostgreSQL  
   - Deployments do PostgreSQL
   - RabbitMQ
   - ConfigMaps das APIs
   - Serviços das APIs
   - Deployments das APIs (versão Minikube)
   - Grafana

### 8. ⚡ Comandos Rápidos

```powershell
# Deploy completo
.\minikube-deploy.ps1 deploy

# Verificar se tudo está funcionando
.\minikube-deploy.ps1 status

# Abrir dashboard
.\minikube-deploy.ps1 dashboard

# Acessar IdentityAPI
start "http://192.168.49.2:32123/swagger"
```

### 9. 🚨 Troubleshooting

**Se as APIs não funcionarem:**
```powershell
# Ver logs das APIs
kubectl logs -l app=identityapi
kubectl logs -l app=menuapi  
kubectl logs -l app=orderapi

# Verificar se os bancos estão rodando
kubectl get pods | findstr postgres

# Reiniciar um deployment
kubectl rollout restart deployment/identityapi
```

**Se o Minikube não iniciar:**
```powershell
# Reiniciar o Minikube
.\minikube-deploy.ps1 restart

# Ou reiniciar manualmente
minikube delete
minikube start --driver=docker --memory=4096 --cpus=4
```

## 🎉 Pronto!

Agora você tem um ambiente Kubernetes completo rodando localmente com o Minikube! 

Use o comando `.\minikube-deploy.ps1 deploy` para começar! 🚀
