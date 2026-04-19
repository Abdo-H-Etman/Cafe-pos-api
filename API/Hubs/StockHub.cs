using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public class StockHub : Hub
{
    // You can add methods here if the client needs to send messages to the server,
    // but for inventory updates, the server will mostly push to clients.
}
