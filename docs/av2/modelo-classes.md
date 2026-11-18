# Modelo de classes observado

O diagrama diferencia os registros persistidos (`Operations`) do objeto de domínio transitório (`Employee`). As relações de persistência são atualmente armazenadas por identificadores e consultadas pelo `DataStore`; não são associações de navegação EF entre essas classes.

```mermaid
classDiagram
    class Staff {
        +Guid Id
        +string Name
        +string Registration
        +string Cpf
        +string Email
        +Guid DepartmentId
        +Guid? LeaderId
        +bool Active
    }
    class Team {
        +Guid Id
        +string Name
        +Guid? LeaderId
    }
    class Punch {
        +Guid Id
        +Guid EmployeeId
        +string Kind
        +DateTimeOffset At
        +string Source
    }
    class Leave {
        +Guid Id
        +Guid EmployeeId
        +DateOnly Start
        +DateOnly End
        +string Status
    }
    class Account {
        +Guid Id
        +string Email
        +string Role
        +bool Active
        +Guid? EmployeeId
    }
    class Employee {
        +Guid Id
        +string Email
        +string RegistrationNumber
        +string FullName
        +string CPF
        +UpdateContact(email, phone)
        +ChangeDepartment(department)
        +ChangePosition(position)
        +AssignLeader(leader)
        +Activate()
        +Deactivate()
    }
    class Department {
        +Guid Id
        +string Name
        +UpdateDetails(name, description)
        +CountEmployees()
    }
    class Position {
        +Guid Id
        +string Name
        +UpdateDetails(name, description)
    }
    Staff "1" --> "0..*" Punch : EmployeeId
    Staff "1" --> "0..*" Leave : EmployeeId
    Team "1" --> "0..*" Staff : DepartmentId
    Staff "0..1" --> "0..*" Staff : LeaderId
    Staff "0..1" --> "0..*" Account : EmployeeId
    Employee --> Department : referência transitória
    Employee --> Position : referência transitória
    Employee "0..1" --> "0..1" Employee : líder transitório
```

`Staff`, `Team`, `Punch`, `Leave` e `Account` são os tipos de `Operations/Models.cs` mapeados no `TimeIdDbContext`. `Employee`, `Department` e `Position` vêm de `Dominios/Employee/`; `EmployeeService` os utiliza para aplicar algumas operações antes de copiar os valores ao registro `Staff`. A identidade persistida continua sendo `Staff`.

## Limites e discrepâncias

- Multiplicidades do diagrama descrevem as relações pretendidas pelos identificadores e consultas; o banco não declara chaves estrangeiras para essas ligações.
- Um funcionário pode estar vinculado a zero ou mais contas segundo o formato e o mapeamento atuais; a aplicação trabalha normalmente com uma conta vinculada, mas não há restrição única no banco para impor esse máximo.
- O histórico `Punch` e os pedidos `Leave` não são objetos compostos dentro de `Staff`; são registros independentes associados por `EmployeeId`.
- `Leave.Status` usa texto validado pelo controller, não uma máquina de estados encapsulada na classe.
- `Employee` tem alguns métodos de comportamento, mas propriedades centrais ainda têm setters públicos e as regras de unicidade e ciclos consultam o conjunto no serviço.
- `TimeRecord`, `Vacation` e outras classes antigas em `Dominios/` não devem ser confundidas com os tipos persistidos usados pelos fluxos atuais.
- Não há contrato polimórfico de repositório em uso demonstrado por este modelo.
