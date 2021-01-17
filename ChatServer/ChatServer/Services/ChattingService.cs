using System;
using System.Threading.Tasks;
using ChatServer.Protos;
using Grpc.Core;

namespace ChatServer.Services
{
    public class ChattingService : ChatService.ChatServiceBase
    {
        private readonly ChatRoom _chatroomService;

        public ChattingService(ChatRoom chatRoomService)
        {
            _chatroomService = chatRoomService;
        }

        public override async Task Join(IAsyncStreamReader<Message> requestStream, IServerStreamWriter<Message> responseStream, ServerCallContext context)
        {
            if (!await requestStream.MoveNext()) return;

            do
            {
                _chatroomService.Join(requestStream.Current.User, responseStream);
                await _chatroomService.BroadcastMessageAsync(requestStream.Current);
            } while (await requestStream.MoveNext());

            _chatroomService.Remove(context.Peer);

        }
    }
}
