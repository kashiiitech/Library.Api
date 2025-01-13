using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Library.Api
{
    public static class ResultExtensions
    {
        public static IResult Html(this IResultExtensions extensions, string html)
        {
            return new HtmlResult(html);
        }

        private class HtmlResult : IResult
        {
            private readonly string _html;

            public HtmlResult(string html)
            {
                _html = html;
            }

            public async Task ExecuteAsync(HttpContext httpContext)
            {
                httpContext.Response.ContentType = MediaTypeNames.Text.Html;
                httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
                await httpContext.Response.WriteAsync(_html);
            }
        }
    }
}
