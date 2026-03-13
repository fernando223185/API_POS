# ?? **GUÍA COMPLETA: Despliegue en AWS EC2**

## ? **Problema Resuelto**

**Error Original:**
```
Unable to configure HTTPS endpoint. No server certificate was specified
```

**Solución Aplicada:**
- ? HTTPS deshabilitado en Linux (solo se usa en Windows)
- ? Kestrel configurado para HTTP en puerto 7254
- ? CORS permisivo para demo
- ? Swagger habilitado en producción

---

## ?? **Archivos Generados**

1. **`API_ERP_POS_AWS_20260306_163655.zip`** (7.14 MB)
   - Versión compilada para AWS/Linux
   - Sin dependencia de certificado HTTPS
   - Lista para desplegar

2. **`Scripts/erpapi.service`**
   - Servicio systemd para ejecutar automáticamente
   - Se reinicia automáticamente si falla

---

## ?? **PASO 1: Configurar Security Group**

En la consola de AWS EC2:

```
1. Ir a EC2 ? Security Groups
2. Seleccionar el Security Group de tu instancia
3. Inbound Rules ? Edit inbound rules
4. Add Rule:
   - Type: Custom TCP
   - Port: 7254
   - Source: 0.0.0.0/0 (o tu IP específica)
5. Save rules
```

---

## ?? **PASO 2: Subir Archivos a EC2**

### **Opción A: Usar SCP (Recomendado)**

Desde Windows PowerShell:

```powershell
# Navegar a la carpeta donde está el ZIP
cd C:\Users\PCX\source\repos\API_POS\Web.Api

# Subir archivo
scp -i "C:\path\to\your-key.pem" API_ERP_POS_AWS_20260306_163655.zip ubuntu@<ec2-public-ip>:~/
```

### **Opción B: Usar WinSCP (GUI)**

```
1. Descargar WinSCP: https://winscp.net
2. File Protocol: SFTP
3. Host name: <ec2-public-ip>
4. User name: ubuntu
5. Advanced ? SSH ? Authentication ? Private key: tu-key.pem
6. Login
7. Arrastrar el ZIP a la carpeta /home/ubuntu
```

---

## ??? **PASO 3: Conectar por SSH**

```bash
ssh -i "C:\path\to\your-key.pem" ubuntu@<ec2-public-ip>
```

---

## ?? **PASO 4: Preparar la API (EN EC2)**

```bash
# Crear carpeta para la API
mkdir -p ~/api
cd ~/api

# Mover el ZIP aquí
mv ~/API_ERP_POS_AWS_20260306_163655.zip .

# Descomprimir
sudo apt-get update
sudo apt-get install unzip -y
unzip API_ERP_POS_AWS_20260306_163655.zip -d publish

# Verificar archivos
ls -la publish/
```

---

## ?? **PASO 5: Instalar .NET 8 Runtime (si no está instalado)**

```bash
# Agregar repositorio de Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Actualizar repositorios
sudo apt-get update

# Instalar .NET 8 Runtime
sudo apt-get install -y aspnetcore-runtime-8.0

# Verificar instalación
dotnet --version
```

---

## ?? **PASO 6: Ejecutar la API**

### **Opción A: Ejecución Manual (Para Probar)**

```bash
cd ~/api/publish
dotnet Web.Api.dll
```

**Deberías ver:**
```
?? ERP POS API Server started successfully
?? Environment: Production
?? Operating System: Linux

?? API Access URLs:
   - http://localhost:7254 (Local access)
   - http://0.0.0.0:7254 (All interfaces)
   - http://<server-ip>:7254 (Network access - Linux/AWS)

?? Swagger UI:
   - http://localhost:7254/swagger
   - http://<ec2-public-ip>:7254/swagger

?? Test credentials: ADMIN001 / admin123

??  AWS/Linux Notes:
   - HTTPS is disabled (no certificate)
   - Make sure port 7254 is open in Security Group
   - Access via: http://<ec2-public-ip>:7254
```

**Probar desde tu navegador:**
```
http://<ec2-public-ip>:7254/swagger
```

**Detener la API:** `Ctrl + C`

---

### **Opción B: Ejecución en Segundo Plano (Con nohup)**

```bash
cd ~/api/publish
nohup dotnet Web.Api.dll > api.log 2>&1 &

# Ver el proceso
ps aux | grep dotnet

# Ver logs en tiempo real
tail -f api.log

# Detener el proceso
pkill -f Web.Api.dll
```

---

### **Opción C: Servicio Systemd (RECOMENDADO - Producción)**

#### **1. Crear archivo de servicio:**

```bash
sudo nano /etc/systemd/system/erpapi.service
```

#### **2. Pegar el siguiente contenido:**

```ini
[Unit]
Description=ERP POS API - .NET 8
After=network.target

[Service]
Type=notify
User=ubuntu
Group=ubuntu
WorkingDirectory=/home/ubuntu/api/publish
ExecStart=/usr/bin/dotnet /home/ubuntu/api/publish/Web.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=erpapi
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

**Guardar:** `Ctrl + O`, `Enter`, `Ctrl + X`

#### **3. Configurar el servicio:**

```bash
# Recargar systemd
sudo systemctl daemon-reload

# Habilitar servicio (inicia automáticamente al arrancar)
sudo systemctl enable erpapi

# Iniciar servicio
sudo systemctl start erpapi

# Ver estado
sudo systemctl status erpapi
```

#### **4. Comandos útiles:**

```bash
# Ver logs en tiempo real
sudo journalctl -u erpapi -f

# Ver últimos 100 logs
sudo journalctl -u erpapi -n 100

# Reiniciar servicio
sudo systemctl restart erpapi

# Detener servicio
sudo systemctl stop erpapi

# Ver estado
sudo systemctl status erpapi
```

---

## ?? **PASO 7: Verificar que Funciona**

### **Desde el navegador:**

```
http://<ec2-public-ip>:7254/swagger
```

### **Probar Login:**

```bash
# En tu máquina local o en EC2
curl -X POST http://<ec2-public-ip>:7254/api/Login/login \
  -H "Content-Type: application/json" \
  -d '{
    "code": "ADMIN001",
    "password": "admin123"
  }'
```

**Respuesta esperada:**
```json
{
  "message": "Login successful",
  "error": 0,
  "token": "eyJhbGci...",
  "tokenType": "Bearer",
  "expiresAt": "2026-03-07T00:00:00Z",
  "user": {
    "id": 1,
    "code": "ADMIN001",
    "name": "Administrador",
    "email": "admin@sistema.com",
    "active": true,
    "roleId": 1,
    "roleName": "Administrador"
  }
}
```

---

## ?? **PASO 8: Actualizar la API (Futuras Versiones)**

```bash
# 1. Detener servicio
sudo systemctl stop erpapi

# 2. Hacer backup
cd ~/api
cp -r publish publish.backup.$(date +%Y%m%d_%H%M%S)

# 3. Subir nuevo ZIP desde Windows
scp -i tu-key.pem API_ERP_POS_AWS_NUEVO.zip ubuntu@<ec2-ip>:~/api/

# 4. Descomprimir (EN EC2)
cd ~/api
rm -rf publish
unzip API_ERP_POS_AWS_NUEVO.zip -d publish

# 5. Reiniciar servicio
sudo systemctl start erpapi

# 6. Verificar
sudo systemctl status erpapi
sudo journalctl -u erpapi -f
```

---

## ??? **CONFIGURACIÓN DE SEGURIDAD**

### **Conexión a Base de Datos:**

Si tu base de datos está en otra máquina, actualiza el `appsettings.json`:

```bash
cd ~/api/publish
nano appsettings.json
```

Cambiar:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_IP_BD,1433;Database=ERP;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"
  }
}
```

---

## ?? **SOLUCIÓN DE PROBLEMAS**

### **Problema: No puedo acceder a la API**

```bash
# 1. Verificar que el servicio está corriendo
sudo systemctl status erpapi

# 2. Ver logs
sudo journalctl -u erpapi -n 50

# 3. Verificar puerto
sudo netstat -tlnp | grep 7254

# 4. Verificar firewall del EC2
# AWS Console ? EC2 ? Security Groups ? Inbound Rules
```

### **Problema: Error al conectar a la base de datos**

```bash
# Ver logs del servicio
sudo journalctl -u erpapi -f

# Verificar connection string
cat ~/api/publish/appsettings.json
```

### **Problema: API se detiene sola**

```bash
# Ver logs
sudo journalctl -u erpapi -n 100

# El servicio se reinicia automáticamente si falla
# Verificar después de 10 segundos
sudo systemctl status erpapi
```

---

## ?? **MONITOREO**

```bash
# Ver uso de CPU/Memoria
htop

# Ver logs en tiempo real
sudo journalctl -u erpapi -f

# Ver estado del servicio
sudo systemctl status erpapi
```

---

## ? **RESUMEN**

| Aspecto | Estado |
|---------|--------|
| **HTTPS** | ? Deshabilitado (no necesario) |
| **HTTP** | ? Puerto 7254 |
| **Swagger** | ? Habilitado en producción |
| **CORS** | ? Permitido para todos |
| **Servicio** | ? Systemd configurado |
| **Auto-inicio** | ? Se inicia al arrancar |
| **Auto-restart** | ? Si falla se reinicia |

---

## ?? **URLs FINALES**

```
API Base:
http://<ec2-public-ip>:7254

Swagger:
http://<ec2-public-ip>:7254/swagger

Login:
http://<ec2-public-ip>:7254/api/Login/login

Módulos:
http://<ec2-public-ip>:7254/api/Modules
http://<ec2-public-ip>:7254/api/system/modules

Clientes:
http://<ec2-public-ip>:7254/api/Customer

Productos:
http://<ec2-public-ip>:7254/api/Products
```

---

**? API LISTA PARA AWS EC2** ??

**Costo:** Solo pagas el EC2 (t2.micro en Free Tier = $0)  
**Disponibilidad:** 24/7  
**Escalabilidad:** Puedes aumentar el tamaño del EC2 cuando sea necesario
