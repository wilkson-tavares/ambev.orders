# Orders API

API REST em .NET 10 para gerenciamento de pedidos com cálculo de imposto e suporte a feature flag para reforma tributária.

---

## Sumário

- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Como executar](#como-executar)
- [Como executar os testes](#como-executar-os-testes)
- [Feature Flag — reforma tributária](#feature-flag--reforma-tributária)
- [Endpoints](#endpoints)
- [Decisões de arquitetura](#decisões-de-arquitetura)

---

## Tecnologias

- .NET 10
- Entity Framework Core (in-memory)
- Serilog (console + arquivo)
- XUnit + FluentAssertions + Bogus + NSubstitute
- Swashbuckle (Swagger)

---

## Arquitetura

A solução é dividida em quatro projetos com responsabilidades bem definidas:

```
Orders.slnx
├── src/
│   ├── Ambev.Orders.API        # Controllers, DTOs, middlewares, configuração
│   ├── Ambev.Orders.Domain     # Entidades, interfaces, serviços, strategies
│   └── Ambev.Orders.Data       # Repositórios, AppDbContext, mapeamentos EF
└── tests/
    └── Ambev.Orders.Tests      # Testes unitários e de integração
```

**Regra de dependência:** `Ambev.Orders.Domain` não referencia nenhum projeto externo. `Ambev.Orders.Data` referencia apenas `Ambev.Orders.Domain`. `Ambev.Orders.API` referencia os três.

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download) ou superior

Verifique a instalação:

```bash
dotnet --version
```

---

## Como executar

**1. Clone o repositório:**

```bash
git clone <url-do-repositorio>
cd ambev.orders
```

**2. Restaure as dependências:**

```bash
dotnet restore
```

**3. Execute a API:**

```bash
dotnet run --project src/Ambev.Orders.API
```

A API estará disponível em `http://localhost:5000`.

O Swagger estará disponível em `http://localhost:5000/swagger`.

---

## Como executar os testes

**Todos os testes:**

```bash
dotnet test
```

**Com detalhamento:**

```bash
dotnet test --verbosity normal
```

**Apenas testes unitários:**

```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

**Apenas testes de integração:**

```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

---

## Feature Flag — reforma tributária

O cálculo de imposto pode ser alternado sem recompilar a aplicação.

| Flag | Cálculo | Alíquota |
|------|---------|----------|
| `false` (padrão) | Vigente | 30% do valor total dos itens |
| `true` | Reforma tributária | 20% do valor total dos itens |

**Para ativar o novo cálculo**, edite `src/Ambev.Orders.API/appsettings.json`:

```json
{
  "FeatureFlags": {
    "UsingTaxReform": true
  }
}
```

Ou via variável de ambiente (útil em containers):

```bash
FeatureFlags__UsingTaxReform=true dotnet run --project src/Ambev.Orders.API
```

> O cálculo é resolvido uma vez na inicialização via injeção de dependência. Para alternar em runtime sem reiniciar, basta implementar `IOptionsMonitor<FeatureFlagOptions>` no registro da DI — a arquitetura já está preparada para isso.

---

## Endpoints

### POST /api/orders — criar pedido

```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1,
    "clientId": 1,
    "items": [
      {
        "productId": 1001,
        "quantity": 2,
        "value": 52.70
      }
    ]
  }'
```

**Resposta 201:**
```json
{
  "id": 1,
  "status": "Created"
}
```

**Resposta 409 (pedido duplicado):**
```json
{
  "error": "An order with id '1' already exists."
}
```

---

### GET /api/orders/{id} — consultar pedido por ID

```bash
curl http://localhost:5000/api/orders/1
```

**Resposta 200:**
```json
{
  "id": 1,
  "orderId": 1,
  "clientId": 1,
  "tax": 15.81,
  "status": "Created",
  "items": [
    {
      "productId": 1001,
      "quantity": 2,
      "value": 52.70
    }
  ]
}
```

**Resposta 404:**
```json
{
  "error": "Order '1' not found."
}
```

---

### GET /api/orders?status={status} — listar por status

```bash
curl http://localhost:5000/api/orders?status=Created
```

Valores aceitos para `status`: `Created`, `Processing`, `Sent`.

**Resposta 200:**
```json
[
  {
    "id": 1,
    "orderId": 1,
    "clientId": 1,
    "tax": 15.81,
    "status": "Created",
    "items": [...]
  }
]
```

---

### PATCH /api/orders/{id}/start-processing — iniciar processamento

```bash
curl -X PATCH http://localhost:5000/api/orders/1/start-processing
```

**Resposta 200:** ordem com `status: "Processing"`.

---

### PATCH /api/orders/{id}/send — enviar para Sistema B

```bash
curl -X PATCH http://localhost:5000/api/orders/1/send
```

**Resposta 200:** ordem com `status: "Sent"`.

---

## Decisões de arquitetura

### Padrão Strategy para cálculo de imposto

O cálculo de imposto é abstraído pela interface `ITaxCalculator` (Domain), com duas implementações: `TaxAtualStrategy` e `TaxReformStrategy`. A feature flag seleciona qual implementação injetar no startup.

Essa abordagem segue o **Open/Closed Principle**: adicionar uma nova regra tributária no futuro significa criar uma nova classe, sem alterar nenhuma existente.

### Domain sem dependências externas

`Ambev.Orders.Domain` não referencia EF Core, Serilog, nem qualquer biblioteca de infraestrutura. Toda dependência externa é abstraída por interfaces definidas no próprio Domain e implementadas nas camadas superiores.

### Validação de duplicidade no Domain

A verificação de pedido duplicado ocorre no `OrderService`, antes da persistência. O repositório expõe `ExistsAsync(orderId)` e o serviço lança `DomainException` em caso de duplicidade. Isso mantém a regra de negócio no lugar correto e garante que ela seja testável de forma isolada via mock.

### CancellationToken em todas as operações async

Todos os métodos assíncronos aceitam `CancellationToken`. Com uma volumetria de 150–200 mil pedidos/dia, cancelar requisições abandonadas evita desperdício de recursos e threads presas em operações de I/O desnecessárias.

### Banco de dados in-memory

O EF Core in-memory foi escolhido para simplificar o ambiente de execução. A camada Data está completamente isolada atrás de `IOrderRepository`: trocar por PostgreSQL, SQL Server ou qualquer outro banco exige apenas registrar uma nova implementação no `Program.cs`, sem tocar em Domain ou API.

### Tratamento de erros centralizado

Um `ExceptionMiddleware` intercepta todas as `DomainException` e retorna o status HTTP adequado (400, 404 ou 409). Isso elimina try/catch nos controllers e garante um contrato de erro consistente em toda a API.
