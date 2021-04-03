using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WorkerRoleWithSBQueue1
{
    public class WorkerRole : RoleEntryPoint
    {
        // 队列的名称
        const string QueueName = "ProcessingQueue";

        // QueueClient 是线程安全的。建议你进行缓存， 
        // 而不是针对每一个请求重新创建它
        QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("正在开始处理消息");

            // 启动消息泵，并且将为每个已收到的消息调用回调，在客户端上调用关闭将停止该泵。
            Client.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        // 处理消息
                        Trace.WriteLine("正在处理 Service Bus 消息: " + receivedMessage.SequenceNumber.ToString());
                    }
                    catch
                    {
                        // 在此处处理任何处理特定异常的消息
                    }
                });

            CompletedEvent.WaitOne();
        }

        public static string getLocalServiceBusConnectionString()
        {
            var ServerFQDN = System.Net.Dns.GetHostEntry(string.Empty).HostName;
            var ServiceNamespace = "ServiceBusDefaultNamespace";
            var HttpPort = 9355;
            var TcpPort = 9354;

            var connBuilder = new ServiceBusConnectionStringBuilder();
            connBuilder.ManagementPort = HttpPort;
            connBuilder.RuntimePort = TcpPort;
            connBuilder.Endpoints.Add(new UriBuilder() { Scheme = "sb", Host = ServerFQDN, Path = ServiceNamespace }.Uri);
            connBuilder.StsEndpoints.Add(new UriBuilder() { Scheme = "https", Host = ServerFQDN, Port = HttpPort, Path = ServiceNamespace }.Uri);

            return connBuilder.ToString();
        }

        public override bool OnStart()
        {
            // 设置最大并发连接数 
            ServicePointManager.DefaultConnectionLimit = 12;

            // 如果队列不存在，则创建队列
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            // 初始化与 Service Bus 队列的连接
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            return base.OnStart();
        }

        public override void OnStop()
        {
            // 关闭与 Service Bus 队列的连接
            Client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}