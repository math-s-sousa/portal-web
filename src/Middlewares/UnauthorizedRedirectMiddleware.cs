public class UnauthorizedRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public UnauthorizedRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Verifique se a resposta é 401 e redirecione
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            context.Response.Redirect("/Login"); // Altere para a URL que você deseja redirecionar
        }
    }
}
