# Pedidos API

API REST em .NET 7+ para gerenciamento de pedidos com cálculo de imposto e suporte a feature flag para reforma tributária.

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

- .NET 7+
- Entity Framework Core (in-memory)
- Serilog (console + arquivo)
- XUnit + FluentAssertions + Bogus + NSubstitute
- Testcontainers (testes de integração)
- Swashbuckle (Swagger)

---

## Arquitetura

A solução é dividida em quatro projetos com responsabilidades bem definidas:

```
Pedidos.sln
├── src/
│   ├── Pedidos.API        # Controllers, DTOs, middlewares, configuração
│   ├── Pedidos.Domain     # Entidades, interfaces, serviços, strategies
│   └── Pedidos.Data       # Repositórios, AppDbContext, mapeamentos EF
└── tests/
    └── Pedidos.Tests      # Testes unitários e de integração
```

**Regra de dependência:** `Pedidos.Domain` não referencia nenhum projeto externo. `Pedidos.Data` referencia apenas `Pedidos.Domain`. `Pedidos.API` referencia os três.

---

## Pré-requisitos

- [.NET 7 SDK](https://dotnet.microsoft.com/download) ou superior
- Docker (apenas para testes de integração com Testcontainers)

Verifique a instalação:

```bash
dotnet --version
```

---

## Como executar

**1. Clone o repositório:**

```bash
git clone <url-do-repositorio>
cd pedidos
```

**2. Restaure as dependências:**

```bash
dotnet restore
```

**3. Execute a API:**

```bash
dotnet run --project src/Pedidos.API
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

> Os testes de integração requerem Docker em execução para subir o Testcontainers.

---

## Feature Flag — reforma tributária

O cálculo de imposto pode ser alternado sem recompilar a aplicação.

| Flag | Cálculo | Alíquota |
|------|---------|----------|
| `false` (padrão) | Vigente | 30% do valor total dos itens |
| `true` | Reforma tributária | 20% do valor total dos itens |

**Para ativar o novo cálculo**, edite `src/Pedidos.API/appsettings.json`:

```json
{
  "FeatureFlags": {
    "UsarReformaTributaria": true
  }
}
```

Ou via variável de ambiente (útil em containers):

```bash
FeatureFlags__UsarReformaTributaria=true dotnet run --project src/Pedidos.API
```

> O cálculo é resolvido uma vez na inicialização via injeção de dependência. Para alternar em runtime sem reiniciar, basta implementar `IOptionsMonitor<FeatureFlagOptions>` no registro da DI — a arquitetura já está preparada para isso.

---

## Endpoints

### POST /api/pedidos — criar pedido

```bash
curl -X POST http://localhost:5000/api/pedidos \
  -H "Content-Type: application/json" \
  -d '{
    "pedidoId": 1,
    "clienteId": 1,
    "itens": [
      {
        "produtoId": 1001,
        "quantidade": 2,
        "valor": 52.70
      }
    ]
  }'
```

**Resposta 201:**
```json
{
  "id": 1,
  "status": "Criado"
}
```

**Resposta 409 (pedido duplicado):**
```json
{
  "erro": "Já existe um pedido com o id '1'."
}
```

---

### GET /api/pedidos/{id} — consultar pedido por ID

```bash
curl http://localhost:5000/api/pedidos/1
```

**Resposta 200:**
```json
{
  "id": 1,
  "pedidoId": 1,
  "clienteId": 1,
  "imposto": 15.81,
  "status": "Criado",
  "itens": [
    {
      "produtoId": 1001,
      "quantidade": 2,
      "valor": 52.70
    }
  ]
}
```

**Resposta 404:**
```json
{
  "erro": "Pedido '1' não encontrado."
}
```

---

### GET /api/pedidos?status={status} — listar por status

```bash
curl http://localhost:5000/api/pedidos?status=Criado
```

Valores aceitos para `status`: `Criado`, `Processando`, `Enviado`.

**Resposta 200:**
```json
[
  {
    "id": 1,
    "pedidoId": 1,
    "clienteId": 1,
    "imposto": 15.81,
    "status": "Criado",
    "itens": [...]
  }
]
```

---

## Decisões de arquitetura

### Padrão Strategy para cálculo de imposto

O cálculo de imposto é abstraído pela interface `IImpostoCalculator` (Domain), com duas implementações: `ImpostoAtualStrategy` e `ImpostoReformaStrategy`. A feature flag seleciona qual implementação injetar no startup.

Essa abordagem segue o **Open/Closed Principle**: adicionar uma nova regra tributária no futuro significa criar uma nova classe, sem alterar nenhuma existente.

### Domain sem dependências externas

`Pedidos.Domain` não referencia EF Core, Serilog, nem qualquer biblioteca de infraestrutura. Toda dependência externa é abstraída por interfaces definidas no próprio Domain e implementadas nas camadas superiores.

### Validação de duplicidade no Domain

A verificação de pedido duplicado ocorre no `PedidoService`, antes da persistência. O repositório expõe `ExisteAsync(pedidoId)` e o serviço lança `DomainException` em caso de duplicidade. Isso mantém a regra de negócio no lugar correto e garante que ela seja testável de forma isolada via mock.

### CancellationToken em todas as operações async

Todos os métodos assíncronos aceitam `CancellationToken`. Com uma volumetria de 150–200 mil pedidos/dia, cancelar requisições abandonadas evita desperdício de recursos e threads presas em operações de I/O desnecessárias.

### Banco de dados in-memory

O EF Core in-memory foi escolhido para simplificar o ambiente de execução do teste. A camada Data está completamente isolada atrás de `IPedidoRepository`: trocar por PostgreSQL, SQL Server ou qualquer outro banco exige apenas registrar uma nova implementação no `Program.cs`, sem tocar em Domain ou API.

### Tratamento de erros centralizado

Um `ExceptionMiddleware` intercepta todas as `DomainException` e retorna o status HTTP adequado (400 ou 409). Isso elimina try/catch nos controllers e garante um contrato de erro consistente em toda a API.
