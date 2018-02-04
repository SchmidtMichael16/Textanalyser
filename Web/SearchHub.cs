using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web
{
    public class SearchHub : Hub
    {
        public void Send(string message)
        {
            // Call the broadcastMessage method to update clients.
            //Clients.All.InvokeAsync("SendMessageToClient", word1, word2, word3, alsoSynonyms);
            Clients.Client(Context.ConnectionId).InvokeAsync("SendMessageToClient", message);
        }

        //public override Task OnConnectedAsync()
        //{
        //    this.Groups.AddAsync(this.Context.ConnectionId, "groupName");

        //    return base.OnConnectedAsync();
        //}

        //public override Task OnDisconnectedAsync(Exception exception)
        //{
        //    return base.OnDisconnectedAsync(exception);
        //}
    }
}
