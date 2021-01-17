using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatServer.Protos;
using Grpc.Core;

namespace ChatServer
{
    public class ChatRoom
    {
        private ConcurrentDictionary<string, IServerStreamWriter<Message>> users = new ConcurrentDictionary<string, IServerStreamWriter<Message>>();

        public void Join(string name, IServerStreamWriter<Message> response) => users.TryAdd(name, response);

        public void Remove(string name) => users.TryRemove(name, out var s);

        public async Task BroadcastMessageAsync(Message message) => await BroadcastMessages(message);

        private async Task BroadcastMessages(Message message)
        {
            foreach (var user in users.Where(x => x.Key != message.User))
            {
                var item = await SendMessageToSubscriber(user, message);
                if (item != null)
                {
                    Remove(item?.Key);
                };
            }
        }

        private async Task<Nullable<KeyValuePair<string, IServerStreamWriter<Message>>>> SendMessageToSubscriber(KeyValuePair<string, IServerStreamWriter<Message>> user, Message message)
        {
            try
            {
                await user.Value.WriteAsync(message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return user;
            }
        }
    }
}
