/*Dominio TimeRecord*/
public class TimeRecord 
{ 
    public Guid Id { get; private set; } 

    public Guid EmployeeId { get; private set; } 

    public Employee Employee { get; private set; } = null!;

    public DateTime DateTime { get; private set; } 

    public TimeRecordType Type { get; private set; }

    public bool IsManual { get; private set; }

    private TimeRecord() { }

    // Passe o histórico completo disponível; não filtre apenas pelo dia, pois um turno pode atravessar a meia-noite.
    public static TimeRecord Register(Employee employee, DateTime dateTime, TimeRecordType type,
        IEnumerable<TimeRecord> existingRecords)
    {
        ArgumentNullException.ThrowIfNull(employee);
        if (employee.Id == Guid.Empty || dateTime == default || !Enum.IsDefined(type))
        {
            throw new ArgumentException("Informe colaborador, data e tipo válidos.");
        }

        ArgumentNullException.ThrowIfNull(existingRecords);
        var records = existingRecords.Where(r => r.EmployeeId == employee.Id).ToList();
        if (records.Any(r => r.DateTime >= dateTime))
        {
            throw new InvalidOperationException("A nova marcação precisa ser posterior às marcações existentes.");
        }

        var record = new TimeRecord
        {
            Id = Guid.NewGuid(),
            Employee = employee,
            EmployeeId = employee.Id,
            DateTime = dateTime,
            Type = type,
            IsManual = false
        };

        records.Add(record);
        ValidateSequence(records);
        return record;
    }

    public TimeRecordEdit Correct(DateTime newDateTime, Guid userId, string reason,
        IEnumerable<TimeRecord> existingRecords)
    {
        ArgumentNullException.ThrowIfNull(existingRecords);
        var records = existingRecords.Where(r => r.EmployeeId == EmployeeId).ToList();
        if (!records.Any(r => ReferenceEquals(r, this)))
        {
            throw new ArgumentException("O histórico deve incluir o ponto que será corrigido.", nameof(existingRecords));
        }

        // Valide uma cópia antes de alterar o registro original.
        var corrected = new TimeRecord
        {
            Id = Id, EmployeeId = EmployeeId, Employee = Employee,
            DateTime = newDateTime, Type = Type, IsManual = true
        };
        ValidateSequence(records.Select(r => ReferenceEquals(r, this) ? corrected : r));
        var edit = TimeRecordEdit.Create(this, DateTime, newDateTime, userId, reason);
        DateTime = newDateTime;
        IsManual = true;
        return edit;
    }

    private static void ValidateSequence(IEnumerable<TimeRecord> records)
    {
        TimeRecord? previous = null;
        foreach (var record in records.OrderBy(r => r.DateTime))
        {
            if (previous != null && record.DateTime <= previous.DateTime)
            {
                throw new InvalidOperationException("As marcações precisam ter horários distintos.");
            }

            bool valid = previous?.Type switch
            {
                null => record.Type == TimeRecordType.Entry,
                TimeRecordType.Entry => record.Type is TimeRecordType.BreakStart or TimeRecordType.Exit,
                TimeRecordType.BreakStart => record.Type == TimeRecordType.BreakEnd,
                TimeRecordType.BreakEnd => record.Type is TimeRecordType.BreakStart or TimeRecordType.Exit,
                TimeRecordType.Exit => record.Type == TimeRecordType.Entry,
                _ => false
            };

            if (!valid)
            {
                throw new InvalidOperationException("Sequência de ponto inválida: confira entrada, intervalos e saída.");
            }

            previous = record;
        }
    }
}

public enum TimeRecordType 
{ 
    Entry, BreakStart, BreakEnd, Exit 
}
