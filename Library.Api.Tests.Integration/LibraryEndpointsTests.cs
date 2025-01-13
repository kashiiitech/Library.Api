using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.Api.Tests.Integration
{
    public class LibraryEndpointsTests : IClassFixture<WebApplicationFactory<IApiMarker>>
    {
        private readonly WebApplicationFactory<IApiMarker> _factory;

        public LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Test()
        {
            var httpClient = _factory.CreateClient();
        }
    }
}
