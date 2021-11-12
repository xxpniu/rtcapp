// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: room.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace RTC.ProtoGrpc.Room {
  public static partial class RoomService
  {
    static readonly string __ServiceName = "RTC.ProtoGrpc.Room.RoomService";

    static readonly grpc::Marshaller<global::RTC.ProtoGrpc.Room.C2S_CreateRoom> __Marshaller_RTC_ProtoGrpc_Room_C2S_CreateRoom = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RTC.ProtoGrpc.Room.C2S_CreateRoom.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::RTC.ProtoGrpc.Room.S2C_CreateRoom> __Marshaller_RTC_ProtoGrpc_Room_S2C_CreateRoom = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RTC.ProtoGrpc.Room.S2C_CreateRoom.Parser.ParseFrom);

    static readonly grpc::Method<global::RTC.ProtoGrpc.Room.C2S_CreateRoom, global::RTC.ProtoGrpc.Room.S2C_CreateRoom> __Method_CreateRoom = new grpc::Method<global::RTC.ProtoGrpc.Room.C2S_CreateRoom, global::RTC.ProtoGrpc.Room.S2C_CreateRoom>(
        grpc::MethodType.Unary,
        __ServiceName,
        "CreateRoom",
        __Marshaller_RTC_ProtoGrpc_Room_C2S_CreateRoom,
        __Marshaller_RTC_ProtoGrpc_Room_S2C_CreateRoom);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::RTC.ProtoGrpc.Room.RoomReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of RoomService</summary>
    public abstract partial class RoomServiceBase
    {
      public virtual global::System.Threading.Tasks.Task<global::RTC.ProtoGrpc.Room.S2C_CreateRoom> CreateRoom(global::RTC.ProtoGrpc.Room.C2S_CreateRoom request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for RoomService</summary>
    public partial class RoomServiceClient : grpc::ClientBase<RoomServiceClient>
    {
      /// <summary>Creates a new client for RoomService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public RoomServiceClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for RoomService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public RoomServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected RoomServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected RoomServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::RTC.ProtoGrpc.Room.S2C_CreateRoom CreateRoom(global::RTC.ProtoGrpc.Room.C2S_CreateRoom request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CreateRoom(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::RTC.ProtoGrpc.Room.S2C_CreateRoom CreateRoom(global::RTC.ProtoGrpc.Room.C2S_CreateRoom request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_CreateRoom, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::RTC.ProtoGrpc.Room.S2C_CreateRoom> CreateRoomAsync(global::RTC.ProtoGrpc.Room.C2S_CreateRoom request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CreateRoomAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::RTC.ProtoGrpc.Room.S2C_CreateRoom> CreateRoomAsync(global::RTC.ProtoGrpc.Room.C2S_CreateRoom request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_CreateRoom, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override RoomServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new RoomServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(RoomServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_CreateRoom, serviceImpl.CreateRoom).Build();
    }

  }
}
#endregion