namespace TimeId.DTOs.Geolocation;

public sealed record WorkLocationResponse(Guid Id, string Name, double Latitude, double Longitude,
    double RadiusMeters, bool CompanyWide, List<Guid> DepartmentIds, List<Guid> EmployeeIds, bool Active);
