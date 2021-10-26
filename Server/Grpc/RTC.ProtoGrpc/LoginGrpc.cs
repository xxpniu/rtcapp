// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: login.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace RTC.ProtoGrpc.LoginServer {
  public static partial class LoginServerService
  {
    static readonly string __ServiceName = "RTC.ProtoGrpc.LoginServer.LoginServerService";

    static readonly grpc::Marshaller<global::RTC.ProtoGrpc.LoginServer.C2L_Login> __Marshaller_RTC_ProtoGrpc_LoginServer_C2L_Login = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RTC.ProtoGrpc.LoginServer.C2L_Login.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::RTC.ProtoGrpc.LoginServer.L2C_Login> __Marshaller_RTC_ProtoGrpc_LoginServer_L2C_Login = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RTC.ProtoGrpc.LoginServer.L2C_Login.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount> __Marshaller_RTC_ProtoGrpc_LoginServer_C2L_RegisterAccount = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount> __Marshaller_RTC_ProtoGrpc_LoginServer_L2C_RegisterAccount = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount.Parser.ParseFrom);

    static readonly grpc::Method<global::RTC.ProtoGrpc.LoginServer.C2L_Login, global::RTC.ProtoGrpc.LoginServer.L2C_Login> __Method_Login = new grpc::Method<global::RTC.ProtoGrpc.LoginServer.C2L_Login, global::RTC.ProtoGrpc.LoginServer.L2C_Login>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Login",
        __Marshaller_RTC_ProtoGrpc_LoginServer_C2L_Login,
        __Marshaller_RTC_ProtoGrpc_LoginServer_L2C_Login);

    static readonly grpc::Method<global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount, global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount> __Method_Register = new grpc::Method<global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount, global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Register",
        __Marshaller_RTC_ProtoGrpc_LoginServer_C2L_RegisterAccount,
        __Marshaller_RTC_ProtoGrpc_LoginServer_L2C_RegisterAccount);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::RTC.ProtoGrpc.LoginServer.LoginReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of LoginServerService</summary>
    public abstract partial class LoginServerServiceBase
    {
      public virtual global::System.Threading.Tasks.Task<global::RTC.ProtoGrpc.LoginServer.L2C_Login> Login(global::RTC.ProtoGrpc.LoginServer.C2L_Login request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount> Register(global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for LoginServerService</summary>
    public partial class LoginServerServiceClient : grpc::ClientBase<LoginServerServiceClient>
    {
      /// <summary>Creates a new client for LoginServerService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public LoginServerServiceClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for LoginServerService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public LoginServerServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected LoginServerServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected LoginServerServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::RTC.ProtoGrpc.LoginServer.L2C_Login Login(global::RTC.ProtoGrpc.LoginServer.C2L_Login request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Login(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::RTC.ProtoGrpc.LoginServer.L2C_Login Login(global::RTC.ProtoGrpc.LoginServer.C2L_Login request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Login, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::RTC.ProtoGrpc.LoginServer.L2C_Login> LoginAsync(global::RTC.ProtoGrpc.LoginServer.C2L_Login request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return LoginAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::RTC.ProtoGrpc.LoginServer.L2C_Login> LoginAsync(global::RTC.ProtoGrpc.LoginServer.C2L_Login request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Login, null, options, request);
      }
      public virtual global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount Register(global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Register(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount Register(global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Register, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount> RegisterAsync(global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return RegisterAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::RTC.ProtoGrpc.LoginServer.L2C_RegisterAccount> RegisterAsync(global::RTC.ProtoGrpc.LoginServer.C2L_RegisterAccount request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Register, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override LoginServerServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new LoginServerServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(LoginServerServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Login, serviceImpl.Login)
          .AddMethod(__Method_Register, serviceImpl.Register).Build();
    }

  }
}
#endregion