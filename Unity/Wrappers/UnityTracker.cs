using System;
using System.Threading.Tasks;
using UnityEngine;
using Xasu.Auth.Protocols;
using Xasu.Config;
using Xasu.Processors;
using Xasu.Requests;
using Xasu.Util;

namespace Xasu
{
    public class UnityTracker : BaseTrackerTemplate<UnityTracker>
    {
        protected bool _sendRequestsInBackground;
        public virtual bool SendRequestsInBackground
        {
            get { return _sendRequestsInBackground; }
            set { _sendRequestsInBackground = value; }
        }

        public UnityTracker() : base() { }

        public override async Task Init(TrackerConfig trackerConfig = null, IHttpRequestHandler requestHandler = null, IAuthProtocol onlineAuthorization = null, IAuthProtocol backupAuthorization = null)
        {
            try
            {
                if (requestHandler == null)
                {
                    requestHandler = new UnityRequestHandler(SendRequestsInBackground);
                }

                await base.Init(trackerConfig, requestHandler, onlineAuthorization, backupAuthorization);
            }
            catch { }
        }

        protected override async Task ProcessingLoop()
        {
            try
            {
                while (true)
                {
                    await Task.Yield();

                    // No need to manually calculate delta time
                    _currentTime += Time.deltaTime;
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
    }
}

