using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private static ConcurrentDictionary<string, int> groupUserCount = new ConcurrentDictionary<string, int>();
    private static ConcurrentDictionary<string, HashSet<string>> groupConnections = new ConcurrentDictionary<string, HashSet<string>>();

    public async Task JoinGroup(int idAutor, int idReceptor)
    {
        string groupName = GetGroupName(idAutor, idReceptor);
        string connectionId = Context.ConnectionId;

        // Asegurarse de que el grupo exista
        if (!groupUserCount.ContainsKey(groupName))
        {
            groupUserCount[groupName] = 0;
            groupConnections[groupName] = new HashSet<string>();
        }

        // Verificar si la conexión ya está en el grupo
        if (!groupConnections[groupName].Contains(connectionId))
        {
            groupConnections[groupName].Add(connectionId);

            // Incrementar el conteo solo si no hay más de 2 usuarios en el grupo
            if (groupUserCount[groupName] < 2)
            {
                groupUserCount[groupName]++;
                await Groups.AddToGroupAsync(connectionId, groupName);
                await Clients.Group(groupName).SendAsync("UpdateGroupCount", groupUserCount[groupName]);
            }
        }
    }

    public async Task LeaveGroup(int idAutor, int idReceptor)
    {
        string groupName = GetGroupName(idAutor, idReceptor);
        string connectionId = Context.ConnectionId;

        if (groupConnections.ContainsKey(groupName) && groupConnections[groupName].Contains(connectionId))
        {
            groupConnections[groupName].Remove(connectionId);
            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            if (groupUserCount.ContainsKey(groupName))
            {
                groupUserCount[groupName]--;

                if (groupUserCount[groupName] <= 0)
                {
                    groupUserCount.TryRemove(groupName, out _);
                    groupConnections.TryRemove(groupName, out _);
                }
                else
                {
                    await Clients.Group(groupName).SendAsync("UpdateGroupCount", groupUserCount[groupName]);
                }
            }
        }
    }

    public async Task SendMessage(int idAutor, int idReceptor, string message, bool visto)
    {
        string groupName = GetGroupName(idAutor, idReceptor);
        await Clients.Group(groupName).SendAsync("ReceiveMessage", new { idAutor, contenidoMensaje = message, visto });
    }

    private string GetGroupName(int idAutor, int idReceptor)
    {
        return $"{Math.Min(idAutor, idReceptor)}-{Math.Max(idAutor, idReceptor)}";
    }
}
