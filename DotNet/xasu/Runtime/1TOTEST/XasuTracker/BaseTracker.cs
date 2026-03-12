using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TinCan;
using TinCan.Json;
using Xasu.Auth;
using Xasu.Auth.Protocols;
using Xasu.Config;
using Xasu.Exceptions;
using Xasu.Processors;
using Xasu.Requests;
using Xasu.Util;

namespace Xasu
{
    public class BaseTracker : BaseTrackerTemplate<BaseTracker> { }

    public class BaseTrackerTemplate<T> : Singleton<T>, ITracker where T : class, new()
    {
        protected TrackerStatus _status;
        public virtual TrackerStatus Status
        {
            get
            {
                _status.Update();
                return _status;
            }
        }

        public virtual IAsyncLRS LRS { get; set; }
        public virtual TrackerConfig TrackerConfig { get; protected set; }
        public virtual Agent DefaultActor { get; set; }
        protected string _username = "Dummy User";
        protected string _email = "dummy@example.com";
        protected string _homePage = "https://example.com/";
        protected bool _useOfflineAccountInsteadOfEmail;
        protected Context _defaultContext;
        public virtual Context DefaultContext
        {
            get
            {
                if (_defaultContext == null)
                {
                    _defaultContext = new Context();
                }
                return new Context(new StringOfJSON(_defaultContext.ToJSON()));
            }  // your json copy 
            set { _defaultContext = new Context(new StringOfJSON(value.ToJSON())); }
        }
        protected Guid _defaultContextRegistrationId;
        public virtual Guid DefaultContextRegistrationId
        {
            get
            {
                if (_defaultContextRegistrationId == Guid.Empty)
                {
                    _defaultContextRegistrationId = Guid.NewGuid();
                    Log("Registration Id : " + _defaultContextRegistrationId);
                }
                return _defaultContextRegistrationId;
            }
            set { _defaultContextRegistrationId = value; }
        }
        public string DefaultIdPrefix { get; set; }


        // Traces processing
        protected IProcessor[] _traceProcessors;
        protected float _processingLoopTime = 1;    // In Seconds
        public virtual float ProcessingLoopTime
        {
            get { return _processingLoopTime; }
            set { _processingLoopTime = value; }
        }
        protected float _currentTime;
        protected bool _flushRequested = false;
        protected bool _finalizeRequested = false;
        protected bool _processing = false;
        protected string _processingLock = "CoolLock";


        // Error logs
        protected bool _enableDebugLogging = false;
        public virtual bool EnableDebugLogging
        {
            get { return _enableDebugLogging; }
            set { _enableDebugLogging = value; }
        }
        protected string _errorLogFilePath = "";
        protected string _errorLogDirectoryName = "";
        protected bool _canLoadConfigFromURL = true;
        public virtual bool CanLoadConfigFromURL
        {
            get { return _canLoadConfigFromURL; }
            set { _canLoadConfigFromURL = value; }
        }

        public BaseTrackerTemplate()
        {
            _status = new TrackerStatus();
            string withOutSpecialCharacters = new string(ApplicationSettings.ProductName.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-').ToArray());
            DefaultIdPrefix = "https://" + withOutSpecialCharacters.Replace(' ', '_') + "/";

            _errorLogFilePath = Path.Combine(ApplicationSettings.PersistentDataPath, "tracker_errors.log");
            _errorLogFilePath = Path.GetFullPath(_errorLogFilePath);

            _errorLogDirectoryName = Path.GetDirectoryName(_errorLogFilePath);
            CreateLogFile();
        }

        protected virtual void CreateLogFile()
        {
            if (!string.IsNullOrEmpty(_errorLogDirectoryName) && !Directory.Exists(_errorLogDirectoryName))
            {
                Directory.CreateDirectory(_errorLogDirectoryName);
                using (FileStream fs = File.Create(_errorLogFilePath)) { }
            }
        }


        #region Initialization

        protected virtual async Task<TrackerConfig> TryLoadConfig()
        {
            TrackerConfig trackerConfig = null;

            try
            {
                trackerConfig = (TrackerConfig)Factories.factories[Factories.Id.TRACKER_CONFIG]();
            }
            catch
            {
                trackerConfig = new TrackerConfig();
            }

            try
            {
                await trackerConfig.LoadConfig();
            }
            catch
            {
                trackerConfig.Offline = true;
                DebugLogger.LogWarning("[TRACKER CONFIG] Tracker config file not found. Default config will be used instead");
                return trackerConfig;
            }

            return trackerConfig;
        }

        public virtual async Task InitOffline(string user, string mail)
        {
            TrackerConfig trackerConfig = await TryLoadConfig();
            if (trackerConfig.Offline)
            {
                _username = user;
                _email = mail;
                _useOfflineAccountInsteadOfEmail = false;
            }
            else
            {
                LogWarning("[TRACKER] Don't use InitOffline() when using only Online or/and Backup. Use Init() instead.");
            }
            await Init(trackerConfig);
        }

        public virtual async Task InitOfflineWithAccount(string user, string homePg)
        {
            TrackerConfig trackerConfig = await TryLoadConfig();
            if (trackerConfig.Offline)
            {
                _username = user;
                _homePage = homePg;
                _useOfflineAccountInsteadOfEmail = true;
            }
            else
            {
                LogWarning("[TRACKER] Don't use InitOfflineWithAccount() when using only Online or/and Backup. Use Init() instead.");
            }
            await Init(trackerConfig);
        }

        public virtual async Task Init(TrackerConfig trackerConfig = null, IHttpRequestHandler requestHandler = null, IAuthProtocol onlineAuthorization = null, IAuthProtocol backupAuthorization = null)
        {
            try
            {
                Log("[TRACKER] Initializing...");
                TrackerConfig = trackerConfig == null ? await TryLoadConfig() : trackerConfig;

                if (requestHandler == null)
                {
                    requestHandler = new HttpRequestHandler();
                }

                var processors = new List<IProcessor>();

                // Working Modes and Backup
                IAuthProtocol onlineAuthProtocol = null, backupAuthProtocol = null;
                IProcessor onlineProcessor = null, localProcessor = null, backupProcessor = null;

                // TODO: Implement a ProcessorFactory that performs generic initialization
                if (TrackerConfig.Offline)
                {
                    Log("[TRACKER] Initializing local processor...");
                    localProcessor = new LocalProcessor(TrackerConfig.FileName, TrackerConfig.TraceFormat);

                    await localProcessor.Init();
                    processors.Add(localProcessor);
                }

                if (TrackerConfig.Online)
                {
                    if (!TrackerConfig.AuthParameters.ContainsKey("lrs_endpoint"))
                    {
                        TrackerConfig.AuthParameters["lrs_endpoint"] = TrackerConfig.LRSEndpoint;
                    }
                    if (!TrackerConfig.AuthParameters.ContainsKey("homepage"))
                    {
                        TrackerConfig.AuthParameters["homepage"] = TrackerConfig.HomePage;
                    }
                    onlineAuthProtocol = onlineAuthorization ?? await AuthFactory.InitAuth(TrackerConfig.AuthProtocol, TrackerConfig.AuthParameters, requestHandler, null); // TODO: Auth Policies
                    if (onlineAuthProtocol?.State == AuthState.Errored)
                    {
                        LogError("[TRACKER] Failed to initialize auth for LRS: " + onlineAuthProtocol.ErrorMessage);
                        return;
                    }

                    if (onlineAuthProtocol is Cmi5Protocol)
                    {
                        Log("[TRACKER] Initializing cmi5 online processor...");
                        onlineProcessor = new Cmi5Processor(TrackerConfig.BatchSize, onlineAuthProtocol, requestHandler, false);
                    }
                    else
                    {
                        Log("[TRACKER] Initializing online processor...");
                        onlineProcessor = new OnlineProcessor(TrackerConfig.LRSEndpoint, TCAPIVersion.V103,
                            TrackerConfig.BatchSize, onlineAuthProtocol, requestHandler, TrackerConfig.Fallback);
                    }

                    await onlineProcessor.Init();
                    processors.Add(onlineProcessor);
                }

                if (TrackerConfig.Backup)
                {
                    if (backupAuthorization != null)
                    {
                        backupAuthProtocol = backupAuthorization;
                    }
                    else if (!string.IsNullOrEmpty(TrackerConfig.BackupAuthProtocol))
                    {
                        backupAuthProtocol = TrackerConfig.BackupAuthProtocol == "same"
                            ? onlineAuthProtocol
                            : await AuthFactory.InitAuth(TrackerConfig.AuthProtocol, TrackerConfig.AuthParameters, requestHandler, null);
                    }

                    if (backupAuthProtocol != null && backupAuthProtocol.State == AuthState.Errored)
                    {
                        LogError("[TRACKER] Failed to initialize auth for backup: " + backupAuthProtocol.ErrorMessage);
                        return;
                    }

                    Log("[TRACKER] Initializing backup processor...");
                    backupProcessor = new BackupProcessor(TrackerConfig.BackupFileName, TrackerConfig.BackupTraceFormat,
                        TrackerConfig.BackupEndpoint, TrackerConfig.BackupRequestConfig, requestHandler, backupAuthProtocol, null); // TODO: Backup policy

                    await backupProcessor.Init();
                    processors.Add(backupProcessor);
                }

                if (onlineAuthProtocol != null)
                {
                    // Actor is obtained from authorization (e.g. OAuth contains username, CMI-5 obtains agent)
                    DefaultActor = onlineAuthProtocol.Agent;
                }
                else
                {
                    if (_useOfflineAccountInsteadOfEmail)
                    {
                        DefaultActor = new Agent
                        {
                            account = new AgentAccount
                            {
                                homePage = _homePage,
                                name = _username
                            }
                        };
                    }
                    else
                    {
                        DefaultActor = new Agent
                        {
                            name = _username,
                            mbox = _email
                        };
                    }
                }
                _traceProcessors = processors.ToArray();

                Status.Monitor(onlineProcessor, localProcessor, backupProcessor, onlineAuthProtocol, backupAuthProtocol);

                if (_traceProcessors.Length == 0)
                {
                    LogWarning("[TRACKER] The tracker has been initialized with no output streams! " +
                        "Please active either online, offline and/or backup in the configuration!");
                }

                // Start the processing
                if (processors.Count > 0)
                {
                    Log("[TRACKER] Started!");
                    ProcessingLoop().WrapErrors();
                }
            }
            catch (Exception ex)
            {
                Status.InitException = ex;
                LogError("[TRACKER] Init exception!", ex);
                throw;
            }

        }

        #endregion


        #region Tracker lifetime

        public virtual Task<Statement> Enqueue(Statement statement)
        {
            if (Status.State == TrackerState.Uninitialized)
            {
                throw new InvalidOperationException("The tracker is not initialized! Initialize it using Init()");
            }

            if (Status.State == TrackerState.Finalized)
            {
                LogWarning("The tracker has been finalized. Traces enqueued won't be send!");
            }

            if (Status.State == TrackerState.Errored)
            {
                LogWarning("The tracker is in an errored state. Traces won't be send! (Check the tracker status for more information)");
            }

            if (statement == null)
            {
                throw new ArgumentNullException("Statement must be different than null!");
            }

            statement.SetPoolExtensions();
            AddDefaultsToTrace(statement);

            TaskScheduler scheduler = SynchronizationContext.Current != null
                ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Default;

            // When all processors are done we notify the listener
            return Task.WhenAll(_traceProcessors.Select(p => p.Enqueue(statement)))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Log(t.Exception.GetType().ToString());
                        LogError($"[TRACKER ({Thread.CurrentThread.ManagedThreadId})] Couldn't send statement with id \"{statement.id}\".", t.Exception);
                        throw t.Exception;
                    }

                    Log($"[TRACKER ({Thread.CurrentThread.ManagedThreadId})] All processors done with statement {statement.id}");

                    // All tasks return the same statement
                    return t.Result[0];
                }, scheduler);
        }

        public virtual async Task Flush()
        {
            if (Status.State == TrackerState.Finalized || Status.State == TrackerState.Uninitialized)
            {
                // Ignoring....
                return;
            }

            if (Status.State == TrackerState.Errored)
            {
                throw new InvalidOperationException("Flushing the tracker is not allowed in error state.");
            }

            _flushRequested = true;

            while (_flushRequested)
            {
                await Task.Yield();
                if (Status.LoopException != null)
                {
                    throw new TrackerException("An exception ocurred during trace submission!", Status.LoopException);
                }
                else if (Status.State == TrackerState.Errored)
                {
                    throw new TrackerException("The tracker entered in error state! (Check the tracker status for more information)");
                }
            }
        }

        public virtual async Task ResetState()
        {
            foreach (IProcessor p in _traceProcessors)
            {
                await p.Reset();
            }

            Status.InitException = null;
            Status.LoopException = null;
            Status.FinalizeException = null;
        }

        public virtual async Task Finalize(IProgress<float> progress = null)
        {
            if (Status.State == TrackerState.Uninitialized)
            {
                throw new InvalidOperationException("The tracker is not initialized!");
            }

            if (Status.State == TrackerState.Errored)
            {
                throw new InvalidOperationException("The tracker cannot be finalized in 'Errored' state (check the tracker status for more information)");
            }

            if (Status.State == TrackerState.Finalized)
            {
                throw new InvalidOperationException("The tracker is already finalized (check the tracker status for more information)");
            }

            _finalizeRequested = true;

            try
            {
                await LockProcessing();
                Progress<float> localProgress = new Progress<float>();
                float processorsDone = 0;
                float totalProcessors = (float)_traceProcessors.Length;
                localProgress.ProgressChanged += (_, p) =>
                {
                    progress?.Report((processorsDone + p) / totalProcessors);
                };

                foreach (IProcessor p in _traceProcessors)//.Where(tp => tp.State == ProcessorState.Working || tp.State == ProcessorState.Fallback))
                {
                    if (p.State == ProcessorState.Working || p.State == ProcessorState.Fallback)
                    {
                        await p.Finalize(localProgress);
                        processorsDone++;
                    }
                }

                progress?.Report(1f);
                UnlockProcessing();
            }
            catch (Exception ex)
            {
                Status.FinalizeException = ex;
                LogError("[TRACKER] Finalize failed!", ex);
                UnlockProcessing();
                throw;
            }
        }

        #endregion


        #region Traces Processing

        protected virtual async Task ProcessingLoop()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                double lastTime = stopwatch.Elapsed.TotalSeconds;
                double deltaTime = 0;

                while (true)
                {
                    await Task.Yield();
                    deltaTime = stopwatch.Elapsed.TotalSeconds - lastTime;
                    lastTime = stopwatch.Elapsed.TotalSeconds;

                    _currentTime += (float)deltaTime;
                    bool isFlushRequested = _flushRequested;
                    if (HasToSendTraces())
                    {
                        await LockProcessing();
                        _currentTime = 0;
                        foreach (IProcessor p in _traceProcessors)
                        {
                            if (p.State != ProcessorState.Working && p.State != ProcessorState.Fallback)
                            {
                                continue;
                            }

                            await p.Process(isFlushRequested);
                        }

                        // If it was a flush, we turn off the flag
                        if (isFlushRequested)
                        {
                            _flushRequested = false;
                        }
                        UnlockProcessing();
                    }

                }
            }
            catch (Exception ex)
            {
                Status.LoopException = ex;
                LogError("[TRACKER] Main loop exception!", ex);
                UnlockProcessing();
            }
        }

        protected virtual void AddDefaultsToTrace(Statement statement)
        {
            // If we do not have an ID we create one so all processors store the trace with the same id
            if (statement.id == null || !statement.id.HasValue)
            {
                statement.id = Guid.NewGuid();
            }

            // Set the actor in case no one is provided
            if (statement.actor == null)
            {
                statement.actor = DefaultActor;
            }

            // Set the timestamp
            if (statement.timestamp == null || !statement.timestamp.HasValue)
            {
                statement.timestamp = DateTime.UtcNow;
            }
        }

        protected virtual async Task LockProcessing()
        {
            lock (_processingLock)
            {
                if (!_processing)
                {
                    _processing = true;
                    return;
                }
            }

            while (_processing)
            {
                await Task.Yield();

                lock (_processingLock)
                {
                    if (!_processing)
                    {
                        _processing = true;
                        return;
                    }
                }
            }
        }

        protected virtual void UnlockProcessing()
        {
            lock (_processingLock)
            {
                _processing = false;
            }
        }

        protected virtual bool HasToSendTraces()
        {
            return (_currentTime > _processingLoopTime || _flushRequested) && !_finalizeRequested;
        }

        #endregion


        #region Logs

        public virtual void LogError(string error, Exception ex = null)
        {
            if (_enableDebugLogging)
            {
                string text = "[Xasu] " + error;
                if (ex != null)
                {
                    DebugLogger.LogException(new TrackerException(text, ex));
                }
                else
                {
                    DebugLogger.LogError(text);
                }
            }

            // Output internal file log
            try
            {
                CreateLogFile();
                string[] appendLines = ex != null ? new string[] { error, ex.ToString() } : new string[] { error };
                File.AppendAllLines(_errorLogFilePath, appendLines);
            }
            catch { }
        }

        public virtual void LogWarning(string warning)
        {
            if (_enableDebugLogging)
            {
                DebugLogger.LogWarning("[Xasu] " + warning);
            }
        }

        public virtual void Log(string message)
        {
            if (_enableDebugLogging)
            {
                DebugLogger.Log("[Xasu] " + message);
            }
        }

        #endregion

    }
}
