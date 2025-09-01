# CreateRekords - Dockerized .NET Application

This application is containerized using Docker and provides three main execution modes for managing Rekord records.

## Prerequisites

- Docker installed on your system
- Docker Compose (usually comes with Docker Desktop)

## Quick Start

### 1. Build the Docker Image

```bash
# Build using Docker
docker build -t create-rekords:latest .

# Or build using Docker Compose
docker compose build
```

### 2. Run the Application

#### Check Records Mode
```bash
# Check records from page 1 to 10 with default page size (100)
docker run --rm create-rekords:latest check 1 10

# Check records with custom page size
docker run --rm create-rekords:latest check 1 10 50
```

#### Create Records in Loop Mode
```bash
# Create records in an infinite loop
docker run --rm create-rekords:latest create-loop 120 30
```

#### Create Records Once Mode
```bash
# Create exactly 25 records
docker run --rm create-rekords:latest create-once 25
```

### 3. Using Docker Compose

```bash
# Build and run with specific command
docker compose run --rm create-rekords check 1 10

# Run in loop mode
docker compose run --rm create-rekords create-loop 120 30

# Run once with count
docker compose run --rm create-rekords create-once 50
```

## Configuration

The application reads configuration from `appsettings.json`. You can:
1. **Mount the file as a volume** (recommended for development)
2. **Use environment variables** to override specific settings

### Mounting Configuration

```bash
docker run --rm \
  -v $(pwd)/appsettings.local.json:/app/appsettings.json:ro \
  create-rekords:latest check 1 10
```

### Environment Variables

You can override configuration values using environment variables:

```bash
docker run --rm \
  -e "AwsSettings__Region=us-east-1" \
  -e "AwsSettings__ClientId=your-client-id" \
  -e "AwsSettings__Username=your-username" \
  -e "AwsSettings__Password=your-password" \
  create-rekords:latest check 1 10
```

## Available Modes

### Check Mode
- **Usage**: `check <pageFrom> <pageTo> [pageSize]`
- **Description**: Checks the status of existing records
- **Parameters**:
  - `pageFrom`: Starting page number
  - `pageTo`: Ending page number
  - `pageSize`: Number of records per page (optional, default: 100)

### Create Loop Mode
- **Usage**: `create-loop [baseSleep] [noiseRange]`
- **Description**: Creates records in an infinite loop with delays
- **Parameters**:
  - `baseSleep`: Base sleep time in milliseconds between records (optional, default: 120)
  - `noiseRange`: Random noise range in milliseconds to add to base sleep (optional, default: 50)

### Create Once Mode
- **Usage**: `create-once <count>`
- **Description**: Creates a specific number of records
- **Parameters**:
- `count`: Number of records to create