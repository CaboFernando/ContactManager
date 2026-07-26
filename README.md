# ContactManager API

API REST para gerenciamento de contatos desenvolvida como avaliação técnica do processo seletivo **Medgrupo**.

Construída com **.NET 8**, seguindo os princípios de **Domain-Driven Design (DDD)**, **Clean Architecture**, **Clean Code** e **SOLID**.

---

##  Estrutura do Projeto

```
ContactManager/
├── ContactManager.API/              # Apresentação: Controllers, Middlewares, configuração
├── ContactManager.Application/      # Casos de uso: Services, DTOs, Interfaces
├── ContactManager.Domain/           # Núcleo: Entidades, Enums, Interfaces de domínio
├── ContactManager.Infrastructure/   # Persistência: EF Core, Repositórios, Migrations
├── ContactManager.Tests/            # Testes unitários: xUnit + Moq
├── .gitignore
├── .gitattributes
└── ContactManager.slnx
```

**Fluxo de dependências entre camadas:**

```
API → Application → Domain
Infrastructure  → Domain
```

> A camada de **Domain** não referencia nenhuma outra — é o núcleo isolado da aplicação.

---

## Requisitos atendidos

| Requisito | Implementação |
|-----------|--------------|
| Nome do contato | `Contato.Nome` — obrigatório, máximo 150 caracteres |
| Data de nascimento | `Contato.DataNascimento` — não pode ser maior ou igual à data de hoje |
| Sexo | Enum `Sexo`: `Masculino`, `Feminino`, `Outro` |
| **Idade calculada em runtime** | Propriedade `Idade` computada no C#, ignorada pelo EF Core — não persiste no banco |
| Contato maior de idade | Validação mínimo 18 anos no construtor e no `Atualizar` da entidade |
| Idade não pode ser 0 | Validação explícita no domínio |
| Listar apenas ativos | `ObterTodosAtivosAsync` filtra `Ativo = true` |
| Visualizar apenas ativos | `ObterAtivoByIdAsync` inclui filtro de status |
| Criar contato | `POST /api/v1/contatos` |
| Editar contato | `PUT /api/v1/contatos/{id}` |
| Ativar contato | `PATCH /api/v1/contatos/{id}/ativar` |
| Desativar contato | `PATCH /api/v1/contatos/{id}/desativar` |
| Excluir contato | `DELETE /api/v1/contatos/{id}` — remoção permanente |
| Banco de dados | SQL Server via EF Core com Migrations |
| Testes unitários | Cobertura de domínio e aplicação com xUnit + Moq |
| API REST documentada | Swagger (Swashbuckle) |
| Regras de negócio separadas da apresentação | Camada Application isolada do Controller |

---

## Como executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server instalado localmente

### 1. Clone o repositório

```bash
git clone https://github.com/CaboFernando/ContactManager.git
cd ContactManager
```

### 2. Configure a connection string

Edite `ContactManager.API/appsettings.json` com o nome da sua instância SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR\\SUA_INSTANCIA;Database=ContactManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Para encontrar o nome correto do servidor, abra o **SQL Server Management Studio (SSMS)** e copie o valor exato do campo **Server name** na tela de conexão.
>
> Exemplos comuns:
> - `Server=localhost\SQLEXPRESS`
> - `Server=DESKTOP-ABC\SQLEXPRESS2008R2`
> - `Server=(localdb)\MSSQLLocalDB`
>
> No JSON, a barra invertida deve ser escapada: `\\`

### 3. Aplique as Migrations

Via **Package Manager Console** no Visual Studio (recomendado):

```
Update-Database -Project ContactManager.Infrastructure -StartupProject ContactManager.API
```

Via **terminal**:

```bash
dotnet ef database update --project ContactManager.Infrastructure --startup-project ContactManager.API
```

> O banco `ContactManagerDB` será criado automaticamente caso não exista.

### 4. Execute a API

```bash
cd ContactManager.API
dotnet run
```

Acesse a documentação Swagger em: **http://localhost:5000/swagger**

### 5. Execute os testes

```bash
dotnet test
```

---

## Endpoints

| Método | Rota | Descrição | Status |
|--------|------|-----------|--------|
| `GET` | `/api/v1/contatos` | Lista todos os contatos **ativos** | `200` |
| `GET` | `/api/v1/contatos/{id}` | Detalhe de um contato **ativo** | `200` / `404` |
| `POST` | `/api/v1/contatos` | Cria um novo contato | `201` / `400` |
| `PUT` | `/api/v1/contatos/{id}` | Atualiza um contato ativo | `200` / `400` / `404` |
| `PATCH` | `/api/v1/contatos/{id}/ativar` | Ativa um contato inativo | `200` / `400` / `404` |
| `PATCH` | `/api/v1/contatos/{id}/desativar` | Desativa um contato ativo | `200` / `404` |
| `DELETE` | `/api/v1/contatos/{id}` | Remove permanentemente um contato | `200` / `404` |

### Exemplo — Criar contato

**Request:**
```http
POST /api/v1/contatos
Content-Type: application/json

{
  "nome": "João Silva",
  "dataNascimento": "1995-03-15",
  "sexo": 1
}
```

**Response `201 Created`:**
```json
{
  "success": true,
  "message": "Contato criado com sucesso.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João Silva",
    "dataNascimento": "1995-03-15T00:00:00",
    "sexo": "Masculino",
    "idade": 29,
    "ativo": true,
    "criadoEm": "2024-07-26T00:00:00Z",
    "atualizadoEm": null
  }
}
```

**Response `400 Bad Request` — menor de idade:**
```json
{
  "success": false,
  "message": "O contato deve ter pelo menos 18 anos.",
  "data": null
}
```

### Enum Sexo

| Valor | Descrição |
|-------|-----------|
| `1` | Masculino |
| `2` | Feminino |
| `3` | Outro |

---

## Testes unitários

Os testes cobrem as duas camadas internas da arquitetura, sem dependência de banco de dados ou HTTP.

### `ContatoEntityTests` — Domínio

- Criação com dados válidos de adulto
- Nome nulo, vazio ou acima de 150 caracteres → exceção
- Menor de 18 anos → exceção
- Data de nascimento igual a hoje (idade 0) → exceção
- Data de nascimento no futuro → exceção
- Exatamente 18 anos → deve criar com sucesso
- Idade calculada dinamicamente em runtime
- Atualização com dados válidos e inválidos
- Ativar e desativar

### `ContatoServiceTests` — Application

- Listagem retorna apenas contatos ativos
- Busca por ID de contato ativo e inativo
- Criação bem-sucedida, duplicata, menor de idade, data inválida
- Atualização com sucesso e contato não encontrado
- Desativar contato ativo
- Ativar contato inativo e tentativa de ativar já ativo
- Excluir com sucesso e contato não encontrado

---

## Migrations

Para adicionar uma nova migration após alterar as entidades:

```bash
# Via terminal
dotnet ef migrations add NomeDaMigration --project ContactManager.Infrastructure --startup-project ContactManager.API

# Via Package Manager Console
Add-Migration NomeDaMigration -Project ContactManager.Infrastructure -StartupProject ContactManager.API
```

---

## Tecnologias

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| .NET | 8.0 | Framework principal |
| ASP.NET Core Web API | 8.0 | Camada HTTP / REST |
| Entity Framework Core | 8.0 | ORM |
| SQL Server | — | Banco de dados |
| Swashbuckle (Swagger) | 6.5 | Documentação da API |
| xUnit | 2.6 | Framework de testes |
| Moq | 4.20 | Mock de dependências |

---

## Autor

**Carlos Fernando dos Santos**
GitHub: [@CaboFernando](https://github.com/CaboFernando)
