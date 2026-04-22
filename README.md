# Task Manager API

API RESTful para gerenciamento de tarefas, desenvolvida em .NET com arquitetura em camadas, permitindo criação, edição, exclusão e consulta de tarefas com filtros.

A API foi projetada seguindo boas práticas como SOLID, separação de responsabilidades, validação de dados e tratamento global de erros.

## Arquitetura

O projeto foi estruturado utilizando uma abordagem baseada em camadas (Clean Architecture simplificada):

- **Domain**: entidades e regras de negócio
- **Application**: serviços, DTOs e validações
- **Infrastructure**: acesso a dados (EF Core InMemory)
- **API**: controllers e configuração da aplicação

### Decisões

- Utilização de **Repository + Unit of Work** para controle de persistência
- **FluentValidation** para validação desacoplada
- **Middleware global** para tratamento de erros
- **DTOs** para isolamento entre camadas
- **InMemory Database** para simplificação do ambiente

## Tecnologias

- .NET 6+
- Entity Framework Core (InMemory)
- FluentValidation
- Swagger
- xUnit (testes)

## Como executar

1. Clone o repositório:
git clone https://github.com/vitao19/task-manager-api

2. Acesse a pasta:
cd task-manager-api

3. Execute:
dotnet run --project TaskManager.API

4. Acesse o Swagger:
https://localhost:xxxx/swagger

## Como testar

A API pode ser testada via Swagger.

Principais endpoints:

- POST /api/tasks → criar tarefa
- GET /api/tasks → listar tarefas (com filtros)
- PUT /api/tasks/{id} → atualizar
- DELETE /api/tasks/{id} → remover

Filtros disponíveis:
- status
- data de vencimento

## Regras de Validação e Contrato da API

### Campos da Tarefa

- **title**
  - obrigatório
  - máximo: 150 caracteres

- **description**
  - opcional
  - máximo: 500 caracteres

- **dueDate**
  - opcional
  - formato: ISO 8601 (ex: 2026-04-22T18:00:00Z)

- **status**
  - obrigatório
  - valores aceitos:
    - 1 = Pendente
    - 2 = Em progresso
    - 3 = Concluída

---

### Comportamento da API

- **POST**
  - valida campos obrigatórios
  - não permite dados inválidos

- **PUT**
  - realiza substituição completa do recurso
  - campos não enviados são persistidos como `null`

- **GET**
  - retorna `200 OK` com lista vazia (`[]`) quando não há dados

- **DELETE**
  - retorna `204 No Content` quando removido com sucesso

## Observações

- O banco de dados é em memória, portanto os dados são resetados ao reiniciar a aplicação.
- O projeto foi estruturado visando fácil evolução para um banco relacional.
- Foi desenvolvida opcionalmente uma interface simples para me auxiliar nos testes da API, não sendo parte do escopo do desafio.

## Observações Técnicas

- A solution utiliza o formato `.slnx`, presente em versões mais recentes do .NET. Em caso de incompatibilidade, recomenda-se utilizar um SDK atualizado.
- Foi identificado um alerta de vulnerabilidade (NU1903) relacionado ao pacote AutoMapper nas versões disponíveis. Para fins do desafio, o uso foi mantido, não impactando o funcionamento da aplicação. Em um cenário real, seria avaliada a substituição ou mitigação.

