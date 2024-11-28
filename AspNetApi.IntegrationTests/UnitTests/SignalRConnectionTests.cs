
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace AspNetApi.Tests.UnitTests
{
    public class SignalRConnectionTests
    {
        private const string HubUrl = "http://localhost/myhub";

        [Fact]
        public async Task Client_Waits_For_OnConnected_Event_Before_Starting_Job()
        {
            // Arrange: Start a test server with SignalR hub
            using var testServer = new TestServer(new WebHostBuilder()
                .UseStartup<TestStartup>());

            var hubConnection = new HubConnectionBuilder()
                .WithUrl(HubUrl, options =>
                {
                    options.HttpMessageHandlerFactory = _ => testServer.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            bool isJobStarted = false;
            bool isConnected = false;

            // Simulate a job that should only start after OnConnected is triggered
            async Task StartJob()
            {
                if (!isConnected)
                    throw new InvalidOperationException("Connection is not ready!");

                isJobStarted = true;
            }

            // Add connection lifecycle event handlers
            hubConnection.Reconnecting += error =>
            {
                Console.WriteLine("Reconnecting...");
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += connectionId =>
            {
                Console.WriteLine($"Reconnected with ConnectionId: {connectionId}");
                return Task.CompletedTask;
            };

            hubConnection.Closed += async error =>
            {
                Console.WriteLine("Connection closed.");
                await Task.Delay(1000); // Optional delay for retries
            };

            // Track readiness using On method
            hubConnection.On<string>("Connected", async message =>
            {
                if (message == "Connected")
                {
                    isConnected = true;
                    Console.WriteLine("Client is ready.");
                    await StartJob();
                }
            });

            // Act: Start the connection
            await hubConnection.StartAsync();

            // Assert
            Assert.True(isConnected, "Connection should be established before job starts.");
            Assert.True(isJobStarted, "Job should start only after the connection is ready.");
        }
    }

    // SignalR Hub and Startup Configuration
    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TestHub>("/myhub");
            });
        }
    }

    // Test SignalR Hub with Client Notifier
    public class TestHub : Hub
    {
        public async Task NotifyClientConnected()
        {
            await Clients.Caller.SendAsync("Connected", "Connected");
        }

        public override async Task OnConnectedAsync()
        {
            // Simulate a delay before the server triggers the "Connected" event
            await Task.Delay(2000); // Simulate connection latency

            await NotifyClientConnected();

            await base.OnConnectedAsync();
        }
    }

}
