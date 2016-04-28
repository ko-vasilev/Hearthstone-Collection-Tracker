using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.LogReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal
{
    public class AchievementLogManager
    {
        private LogReader logReader;
        private Thread thread;
        private bool running = false;
        public AchievementLogManager(LogReaderInfo info)
        {
            logReader = new LogReader(info);
        }

        public void Run(AchievementHandler handler)
        {
            if (!running)
            {
                running = true;
                logReader.Start(DateTime.MinValue);
                thread = new Thread(
                    x =>
                    {
                        try
                        {
                            while (running)
                            {
                                var newLines = logReader.Collect();
                                handler.Handle(newLines);
                                Thread.Sleep(Config.Instance.UpdateDelay);
                            }
                        }
                        catch (ThreadAbortException ex)
                        {
                            Logger.WriteLine("Hearthstone Collection Manager: logreader thread aborted");
                        }
                    }
                );
                thread.Start();
            }
        }

        public void Stop()
        {
            running = false;
            logReader.Stop();
        }
    }
}
