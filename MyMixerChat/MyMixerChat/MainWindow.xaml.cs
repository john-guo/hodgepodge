using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Concurrent;
using BulletScreen;
using System.Globalization;

namespace MyMixerChat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MixerChat mixer;
        private DispatcherTimer timer;
        private DispatcherTimer refresh_timer;
        private ConcurrentDictionary<int, string> calltable;
        private BulletManager bm;

        public MainWindow()
        {
            InitializeComponent();

            bm = new BulletManager(new Rectangle() { Width = Width, Height = Height });
            mixer = new MixerChat();
            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(3),
            };
            timer.Tick += Timer_Tick;

            refresh_timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(1000.0/60),
            };
            refresh_timer.Tick += Refresh_timer_Tick;
            calltable = new ConcurrentDictionary<int, string>();
        }

        private void Refresh_timer_Tick(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            calltable.TryAdd(await mixer.ping(), "ping");
        }

        private async Task shotMessage(dynamic message)
        {
            StringBuilder sb = new StringBuilder();
            foreach (dynamic msg in message)
            {
                if (msg.type == "text")
                {
                    sb.Append((string)msg.text);
                    sb.Append(" ");
                }
            }
            await Dispatcher.InvokeAsync(() =>
            {
                bm.Shot(sb.ToString(), Colors.White);
            });
        }

        private async void Run()
        {
            while (true)
            {
                var message = await mixer.GetMessage();
                switch (message.Type)
                {
                    case MixerChatMessageType.@event:
                        switch (message.@event)
                        {
                            case "WelcomeEvent":
                                calltable.TryAdd(await mixer.optOutEvents(), "optOutEvents");
                                break;
                            case "ChatMessage":
                               await shotMessage(message.data.message.message);
                                break;
                            case "Error":
                                break;
                            default:
                                break;
                        }
                        break;
                    case MixerChatMessageType.reply:
                        if (calltable.TryRemove(message.id, out string mname))
                        {
                            switch (mname)
                            {
                                case "optOutEvents":
                                    calltable.TryAdd(await mixer.auth(63441337), "auth");
                                    break;
                                case "auth":
                                    calltable.TryAdd(await mixer.history(), "history");
                                    break;
                                case "history":
                                    Dispatcher.Invoke(timer.Start);
                                    Dispatcher.Invoke(refresh_timer.Start);
                                    break;
                                case "ping":
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
                await Task.Yield();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bm.UpdateLayout();
            await mixer.Open();
            await Task.Run(() =>
            {
                Run();
            }).ConfigureAwait(false);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            bm.Update(drawingContext);
        }

        protected override int VisualChildrenCount => 0;
    }
}
