using MediatR;

namespace RouteOps.Application.Queries.Routes;

public record RouteOrderDto(
    Guid OrderId,
    string ClientName,
    string Address,
    double Lat,
    double Lng,
    int StopNumber
);

public record DriverRouteDto(
    Guid DriverId,
    string DriverName,
    string ColorHex,
    IReadOnlyList<RouteOrderDto> Orders
);

public record GetRoutesByDriverQuery(bool Optimize = false)
    : IRequest<IReadOnlyList<DriverRouteDto>>;
