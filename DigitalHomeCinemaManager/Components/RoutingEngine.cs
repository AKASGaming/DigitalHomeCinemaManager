/*
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
 * more details.
 *
 */

namespace DigitalHomeCinemaManager.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows.Threading;
    using System.Xml.Serialization;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controllers.Routing;

    public class RoutingEngine : IDisposable
    {

        #region Members

        private static string PATH = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\rules.xml";
        private const string INTERNAL_THREAD_NAME = "ROUTING_PROCESSOR";
    
        private ObservableCollection<MatchAction> rules = new ObservableCollection<MatchAction>();
        private ConcurrentQueue<RoutingItem> queue = new ConcurrentQueue<RoutingItem>();
        private Dictionary<string, IRoutingDestination> routes;
        private AutoResetEvent notifier = new AutoResetEvent(false);
        private Dispatcher dispatcher;
        private Thread workerThread;
        private bool disposed = false;
        private bool running;

        #endregion

        #region Contructor

        internal RoutingEngine(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        #endregion

        #region Methods

        public void Start()
        {
            Debug.Assert(this.notifier != null);

            this.running = true;
            this.workerThread = new Thread(this.ProcessQueue) {
                Name = INTERNAL_THREAD_NAME,
            };
            this.workerThread.Start();
        }

        public void Stop()
        {
            this.running = false;
            if (this.workerThread == null) { return; }

            try {
                this.workerThread.Abort();
                this.workerThread.Join();
            } catch { } 
        }

        public void BindControllers(IEnumerable<IController> controllers)
        {
            this.routes = new Dictionary<string, IRoutingDestination>();
            this.Sources = new Dictionary<string, Type>();
            this.Destinations = new Dictionary<string, IDictionary<string, Type>>();

            Debug.Assert(controllers != null);

            foreach (var controller in controllers) {
                if (controller is IRoutingSource source) {
                    source.RouteData += QueueData;
                    this.Sources.Add(source.Name, source.MatchType);
                } else if (controller is IRoutingDestination destination) {
                    this.routes.Add(destination.Name, destination);
                    this.Destinations.Add(destination.Name, destination.Actions);
                }
            }
        }

        public void LoadRules()
        {
            if (!File.Exists(PATH)) { return; }

            XmlSerializer serializer = new XmlSerializer(typeof(List<MatchAction>));
            FileStream stream = new FileStream(PATH, FileMode.Open);
            List<MatchAction> ruleList;

            try {
                ruleList = (List<MatchAction>)serializer.Deserialize(stream);
            } catch {
                OnRuleProcessed("Failed to parse rules!");
                return;
            }

            foreach (var item in ruleList) {
                // convert Match Type
                if (!string.IsNullOrEmpty(item.MatchType)) {
                    Type t = item.GetMatchType();
                    if (t != null) {
                        if (t.IsEnum) {
                            string match = item.Match.ToString();
                            try {
                                item.Match = Enum.Parse(t, match);
                            } catch { }
                        } else {
                            try {
                                Convert.ChangeType(item.Match, t);
                            } catch { }
                        }
                    }
                }
                // convert Args Type
                if (!string.IsNullOrEmpty(item.ArgsType)) {
                    Type t = Type.GetType(item.ArgsType);
                    if (t != null) {
                        if (t.IsEnum) {
                            string args = item.Args.ToString();
                            try {
                                item.Args = Enum.Parse(t, args);
                            } catch { }
                        } else {
                            try {
                                Convert.ChangeType(item.Args, t);
                            } catch { }
                        }
                    }
                }

                this.rules.Add(item);
            } // foreach

        }

        public void SaveRules()
        {
            List<MatchAction> ruleList = new List<MatchAction>(this.Rules);

            XmlSerializer serializer = new XmlSerializer(typeof(List<MatchAction>));
            TextWriter writer = new StreamWriter(PATH);
            serializer.Serialize(writer, ruleList);
            writer.Close();
        }

        public void QueueData(object sender, RoutingItem e)
        {
            Debug.Assert(e != null);

            this.queue.Enqueue(e);
            this.notifier.Set();
        }

        private void ProcessQueue()
        {
            while (this.running) {
                this.notifier.WaitOne();
                if (this.queue.TryDequeue(out RoutingItem item)) {
                    ProcessItem(item);
                }
            }
        }

        private void ProcessItem(RoutingItem item)
        {
            foreach (var rule in this.rules) {
                if (!rule.Enabled) { continue; }

                if (rule.MatchSource.Equals(item.Source.Name, StringComparison.OrdinalIgnoreCase) &&
                    rule.Match.Equals(item.Data)) { 

                    if (!this.routes.ContainsKey(rule.ActionDestination)) { continue; } // invalid destination

                    try {
                        string result = this.routes[rule.ActionDestination].RouteAction(rule.Action, rule.Args);
                        OnRuleProcessed(result);
                    } catch {
                        OnRuleProcessed("Rule processing failed!");
                    } 
                }
            } // foreach
        }

        protected void OnRuleProcessed(string message)
        {
            if ((this.dispatcher != null) && !this.dispatcher.CheckAccess()) {
                this.dispatcher.BeginInvoke((Action)(() => {
                    RuleProcessed?.Invoke(this, message);
                }));
            } else {
                RuleProcessed?.Invoke(this, message);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    if (this.notifier != null) {
                        this.notifier.Dispose();
                    }
                }

                this.notifier = null;
                this.workerThread = null;

                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Events

        public event EventHandler<string> RuleProcessed;

        #endregion

        #region Properties

        public Dictionary<string, Type> Sources { get; private set; }

        public Dictionary<string, IDictionary<string, Type>> Destinations { get; private set; }

        public ObservableCollection<MatchAction> Rules
        {
            get { return this.rules; }
        }

        #endregion

    }

}
