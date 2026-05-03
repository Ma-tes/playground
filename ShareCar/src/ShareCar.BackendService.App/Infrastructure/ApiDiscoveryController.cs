using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ShareCar.BackendService.App.Infrastructure;

[ApiController]
[Route("api/docs")]
public sealed class ApiDiscoveryController : ControllerBase
{
  [HttpGet]
  public IActionResult GetDocs()
  {
    var endpoints = Assembly.GetExecutingAssembly().GetTypes()
      .Where(IsApiController)
      .SelectMany(BuildEndpoints);

    return Ok(new { title = "ShareCar API", version = "1.0.0", endpoints });
  }

  private static bool IsApiController(Type t) =>
    t.IsPublic && !t.IsAbstract &&
    typeof(ControllerBase).IsAssignableFrom(t) &&
    t != typeof(ApiDiscoveryController);

  private static IEnumerable<EndpointInfo> BuildEndpoints(Type controller)
  {
    var route = ResolveRoute(controller);
    var controllerAuth = controller.GetCustomAttribute<AuthorizeAttribute>();

    return controller
      .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
      .Select(m => TryBuildEndpoint(m, route, controllerAuth))
      .Where(e => e is not null)
      .Select(e => e!);
  }

  private static EndpointInfo? TryBuildEndpoint(MethodInfo method, string controllerRoute, AuthorizeAttribute? controllerAuth)
  {
    var httpAttr = method.GetCustomAttribute<HttpMethodAttribute>();
    if (httpAttr is null) return null;

    var path = httpAttr.Template is { Length: > 0 } suffix
      ? $"/{controllerRoute}/{suffix}"
      : $"/{controllerRoute}";

    var effectiveAuth = method.GetCustomAttribute<AuthorizeAttribute>() ?? controllerAuth;

    return new EndpointInfo(
      Method: httpAttr.HttpMethods.First(),
      Path: path,
      Auth: ResolveAuthDescription(effectiveAuth),
      Body: ResolveBody(method));
  }

  private static string ResolveRoute(Type controller) =>
    (controller.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty)
      .Replace("[controller]", controller.Name.Replace("Controller", "").ToLowerInvariant());

  private static string ResolveAuthDescription(AuthorizeAttribute? auth) =>
    auth switch
    {
      null => "none",
      { Roles: { Length: > 0 } roles } => $"Bearer — role: {roles}",
      _ => "Bearer"
    };

  private static Dictionary<string, string>? ResolveBody(MethodInfo method) =>
    method.GetParameters().FirstOrDefault(p => p.GetCustomAttribute<FromBodyAttribute>() is not null)
      is { } bodyParam
      ? bodyParam.ParameterType
          .GetProperties(BindingFlags.Public | BindingFlags.Instance)
          .ToDictionary(p => p.Name, p => p.PropertyType.Name)
      : null;
}

internal sealed record EndpointInfo(string Method, string Path, string Auth, Dictionary<string, string>? Body);
