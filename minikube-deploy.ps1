# Script para deploy no Minikube
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "deploy", "stop", "restart", "status", "dashboard", "tunnel", "logs", "cleanup")]
    [string]$Action
)

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Ensure-MinikubeRunning {
    Write-ColorOutput "Verificando status do Minikube..." "Yellow"
    $status = minikube status 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "Iniciando Minikube..." "Yellow"
        minikube start --driver=docker --memory=4096 --cpus=4
        
        Write-ColorOutput "Habilitando addons..." "Yellow"
        minikube addons enable ingress
        minikube addons enable dashboard
        minikube addons enable metrics-server
    } else {
        Write-ColorOutput "Minikube ja esta rodando!" "Green"
    }
}

function Deploy-ToMinikube {
    Write-ColorOutput "Configurando ambiente Docker para Minikube..." "Yellow"
    
    # Configurar Docker para usar o registry do Minikube
    & minikube docker-env --shell powershell | Invoke-Expression
    
    Write-ColorOutput "Fazendo build das imagens no Minikube..." "Yellow"
    docker build -t fasttechfood-identityapi:latest -f src/IdentityApi/Dockerfile .
    docker build -t fasttechfood-menuapi:latest -f src/MenuApi/Dockerfile .
    docker build -t fasttechfood-orderapi:latest -f src/OrderApi/Dockerfile .
    
    Write-ColorOutput "Aplicando recursos no Kubernetes..." "Yellow"
    
    # Aplicar PersistentVolumes e PVCs com hostPath primeiro
    kubectl apply -f deploy/postgres-identity-pv-hostpath.yaml
    kubectl apply -f deploy/postgres-menu-pv-hostpath.yaml
    kubectl apply -f deploy/postgres-order-pv-hostpath.yaml
    kubectl apply -f deploy/postgres-zabbix-pv-hostpath.yaml
    kubectl apply -f deploy/zabbix-pv-hostpath.yaml
    kubectl apply -f deploy/grafana-pv-hostpath.yaml
    
    kubectl apply -f deploy/postgres-identity-service.yaml
    kubectl apply -f deploy/postgres-menu-service.yaml
    kubectl apply -f deploy/postgres-order-service.yaml
    kubectl apply -f deploy/postgres-zabbix-service.yaml
    
    kubectl apply -f deploy/postgres-identity-deployment.yaml
    kubectl apply -f deploy/postgres-menu-deployment.yaml
    kubectl apply -f deploy/postgres-order-deployment.yaml
    kubectl apply -f deploy/postgres-zabbix-deployment.yaml
    
    Write-ColorOutput "Aguardando PostgreSQL..." "Yellow"
    kubectl wait --for=condition=available --timeout=300s deployment/postgres-identity
    kubectl wait --for=condition=available --timeout=300s deployment/postgres-menu
    kubectl wait --for=condition=available --timeout=300s deployment/postgres-order
    kubectl wait --for=condition=available --timeout=300s deployment/postgres-zabbix
    
    kubectl apply -f deploy/rabbitmq-service.yaml
    kubectl apply -f deploy/rabbitmq-deployment.yaml
    
    Write-ColorOutput "Aguardando RabbitMQ..." "Yellow"
    kubectl wait --for=condition=available --timeout=300s deployment/rabbitmq
    
    kubectl apply -f deploy/identityapi-configmap.yaml
    kubectl apply -f deploy/menuapi-configmap.yaml
    kubectl apply -f deploy/orderapi-configmap.yaml
    
    kubectl apply -f deploy/identityapi-service.yaml
    kubectl apply -f deploy/menuapi-service.yaml
    kubectl apply -f deploy/orderapi-service.yaml
    
    kubectl apply -f deploy/identityapi-deployment-minikube.yaml
    kubectl apply -f deploy/menuapi-deployment-minikube.yaml
    kubectl apply -f deploy/orderapi-deployment-minikube.yaml
    
    Write-ColorOutput "Aguardando APIs..." "Yellow"
    kubectl wait --for=condition=available --timeout=300s deployment/identityapi
    kubectl wait --for=condition=available --timeout=300s deployment/menuapi
    kubectl wait --for=condition=available --timeout=300s deployment/orderapi
    
    # Deploy Zabbix monitoring
    kubectl apply -f deploy/zabbix-server-configmap.yaml
    kubectl apply -f deploy/zabbix-agent-configmap.yaml
    kubectl apply -f deploy/zabbix-agent-rbac.yaml
    kubectl apply -f deploy/zabbix-service.yaml
    kubectl apply -f deploy/zabbix-deployment.yaml
    kubectl apply -f deploy/zabbix-web-configmap.yaml
    kubectl apply -f deploy/zabbix-web-service.yaml  
    kubectl apply -f deploy/zabbix-web-deployment.yaml
    kubectl apply -f deploy/zabbix-agent-service.yaml
    kubectl apply -f deploy/zabbix-agent-deployment.yaml
    
    Write-ColorOutput "Aguardando Zabbix..." "Yellow"
    kubectl wait --for=condition=available --timeout=300s deployment/zabbix-server
    kubectl wait --for=condition=available --timeout=300s deployment/zabbix-agent
    
    # Deploy Grafana with Zabbix integration
    kubectl apply -f deploy/grafana-datasources-configmap.yaml
    kubectl apply -f deploy/grafana-dashboards-configmap.yaml
    kubectl apply -f deploy/grafana-service.yaml
    kubectl apply -f deploy/grafana-deployment.yaml
    
    Write-ColorOutput "Deploy concluido!" "Green"
}

function Get-MinikubeUrls {
    Write-ColorOutput "URLs de acesso via Minikube:" "Cyan"
    
    $services = @("identityapi-service", "menuapi-service", "orderapi-service", "grafana", "zabbix-web", "rabbitmq")
    foreach ($service in $services) {
        try {
            $url = minikube service $service --url 2>$null
            if ($url) {
                Write-ColorOutput "• $service : $url" "White"
            }
        } catch {
            Write-ColorOutput "• $service : Nao disponivel" "Red"
        }
    }
}

switch ($Action) {
    "start" {
        Ensure-MinikubeRunning
        Write-ColorOutput "Minikube iniciado com sucesso!" "Green"
        Write-ColorOutput "Use: .\minikube-deploy.ps1 deploy para fazer o deploy das aplicacoes" "Yellow"
    }
    
    "deploy" {
        Ensure-MinikubeRunning
        Deploy-ToMinikube
        Write-ColorOutput ""
        Write-ColorOutput ""
        Write-ColorOutput "Comandos uteis:" "Yellow"
        Write-ColorOutput "• .\minikube-deploy.ps1 dashboard - Abrir dashboard" "White"
        Write-ColorOutput "• .\minikube-deploy.ps1 tunnel - Tunel para localhost" "White"
        Write-ColorOutput "• .\minikube-deploy.ps1 status - Ver status" "White"
    }
    
    "stop" {
        Write-ColorOutput "Parando Minikube..." "Red"
        minikube stop
        Write-ColorOutput "Minikube parado!" "Green"
    }
    
    "restart" {
        Write-ColorOutput "Reiniciando Minikube..." "Yellow"
        minikube stop
        minikube start --driver=docker --memory=4096 --cpus=4
        Write-ColorOutput "Minikube reiniciado!" "Green"
    }
    
    "status" {
        Write-ColorOutput "Status do Minikube:" "Cyan"
        minikube status
        Write-ColorOutput ""
        Write-ColorOutput "Status dos recursos:" "Cyan"
        kubectl get all
        Write-ColorOutput ""
        Get-MinikubeUrls
    }
    
    "dashboard" {
        Write-ColorOutput "Abrindo Dashboard do Kubernetes..." "Cyan"
        minikube dashboard
    }
    
    "tunnel" {
        Write-ColorOutput "Criando port-forwards com portas fixas..." "Yellow"
        Write-ColorOutput ""
        Write-ColorOutput "URLs de acesso (portas fixas):" "Cyan"
        Write-ColorOutput "IdentityAPI: http://127.0.0.1:5001/swagger" "White"
        Write-ColorOutput "MenuAPI: http://127.0.0.1:5002/swagger" "White"
        Write-ColorOutput "OrderAPI: http://127.0.0.1:5003/swagger" "White"
        Write-ColorOutput "Grafana: http://127.0.0.1:3000" "White"
        Write-ColorOutput "Zabbix Web: http://127.0.0.1:8080" "White"
        Write-ColorOutput "RabbitMQ Management: http://127.0.0.1:15672" "White"
        Write-ColorOutput ""
        Write-ColorOutput "Pressione Ctrl+C para parar todos os tuneis" "Yellow"
        Write-ColorOutput ""
        
        # Executar port-forwards em paralelo usando services corretos
        $jobs = @()
        $jobs += Start-Job -ScriptBlock { kubectl port-forward service/identityapi-service 5001:8080 }
        $jobs += Start-Job -ScriptBlock { kubectl port-forward service/menuapi-service 5002:8080 }
        $jobs += Start-Job -ScriptBlock { kubectl port-forward service/orderapi-service 5003:8080 }
        $jobs += Start-Job -ScriptBlock { kubectl port-forward service/grafana 3000:3000 }
        $jobs += Start-Job -ScriptBlock { kubectl port-forward service/zabbix-web 8080:8080 }
        $jobs += Start-Job -ScriptBlock { kubectl port-forward service/rabbitmq 15672:15672 }
        
        Write-ColorOutput "Port-forwards criados! URLs ativas:" "Green"
        Write-ColorOutput "• http://127.0.0.1:5001/swagger" "Green"
        Write-ColorOutput "• http://127.0.0.1:5002/swagger" "Green"
        Write-ColorOutput "• http://127.0.0.1:5003/swagger" "Green"
        Write-ColorOutput "• http://127.0.0.1:3000 (Grafana)" "Green"
        Write-ColorOutput "• http://127.0.0.1:8080 (Zabbix)" "Green"
        Write-ColorOutput "• http://127.0.0.1:15672 (RabbitMQ)" "Green"
        
        try {
            # Manter vivo até Ctrl+C
            while ($true) {
                Start-Sleep -Seconds 1
            }
        } finally {
            # Limpar jobs quando parar
            Write-ColorOutput "Parando port-forwards..." "Yellow"
            $jobs | Stop-Job
            $jobs | Remove-Job
        }
    }
    
    "logs" {
        Write-ColorOutput "Logs dos pods:" "Cyan"
        $pods = kubectl get pods -o jsonpath='{.items[*].metadata.name}'
        $podArray = $pods -split ' '
        
        foreach ($pod in $podArray) {
            if ($pod -match 'fasttech|postgres|rabbitmq|grafana') {
                Write-ColorOutput "--- Logs do $pod ---" "Yellow"
                kubectl logs $pod --tail=10
                Write-ColorOutput ""
            }
        }
    }
    
    "cleanup" {
        Write-ColorOutput "Limpando recursos..." "Red"
        kubectl delete all --all
        kubectl delete pvc --all
        kubectl delete configmap --all
        Write-ColorOutput "Recursos removidos!" "Green"
    }
}
