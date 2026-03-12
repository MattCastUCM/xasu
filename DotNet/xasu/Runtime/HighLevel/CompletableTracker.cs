using System;
using System.Collections.Generic;
using Xasu.Exceptions;
using TinCan;

namespace Xasu.HighLevel
{
    public class CompletableTracker : AbstractSeriousGameHighLevelTracker<CompletableTracker>
    {
        /**********************
        *       Verbs
        * *******************/
        public enum Verb
        {
            Initialized,
            Progressed,
            Completed
        }
        protected readonly Dictionary<Enum, string> verbIds = new Dictionary<Enum, string>()
        {
            { Verb.Initialized,   "http://adlnet.gov/expapi/verbs/initialized"  },
            { Verb.Progressed,    "http://adlnet.gov/expapi/verbs/progressed"   },
            { Verb.Completed,     "http://adlnet.gov/expapi/verbs/completed"    }
        };
        protected override Dictionary<Enum, string> VerbIds => verbIds;


        /**********************
        *   Completable Types 
        * *******************/
        public enum CompletableType
        {
            Game,
            Session,
            Level,
            Quest,
            Stage,
            Combat,
            StoryNode,
            Race,
            Completable,

            // Dialog completables extensions
            DialogNode,
            DialogFragment
        }
        protected readonly Dictionary<Enum, string> typeIds = new Dictionary<Enum, string>
        {
            { CompletableType.Game,            "https://w3id.org/xapi/seriousgames/activity-types/serious-game"    },
            { CompletableType.Session,         "https://w3id.org/xapi/seriousgames/activity-types/session"         },
            { CompletableType.Level,           "https://w3id.org/xapi/seriousgames/activity-types/level"           },
            { CompletableType.Quest,           "https://w3id.org/xapi/seriousgames/activity-types/quest"           },
            { CompletableType.Stage,           "https://w3id.org/xapi/seriousgames/activity-types/stage"           },
            { CompletableType.Combat,          "https://w3id.org/xapi/seriousgames/activity-types/combat"          },
            { CompletableType.StoryNode,       "https://w3id.org/xapi/seriousgames/activity-types/story-node"      },
            { CompletableType.Race,            "https://w3id.org/xapi/seriousgames/activity-types/race"            },
            { CompletableType.Completable,     "https://w3id.org/xapi/seriousgames/activity-types/completable"     },
            { CompletableType.DialogNode,      "https://w3id.org/xapi/seriousgames/activity-types/dialog-node"     },
            { CompletableType.DialogFragment,  "https://w3id.org/xapi/seriousgames/activity-types/dialog-fragment" }
        };
        protected override Dictionary<Enum, string> TypeIds => typeIds;


        /**********************
        *   Extensions
        * *******************/
        public enum Extensions
        {
            Progress
        }
        protected readonly Dictionary<Enum, string> extensionIds = new Dictionary<Enum, string>
        {
            { Extensions.Progress, "https://w3id.org/xapi/seriousgames/extensions/progress"    }
        };
        protected override Dictionary<Enum, string> ExtensionIds => extensionIds;


        /**********************
        *   Attributes
        * *******************/
        protected Dictionary<string, DateTime> initializedTimes = new Dictionary<string, DateTime>();
        

        /**********************
        *   Templates
        * *******************/

        /// <summary>
        /// Player initialized a completable.
        /// Type = Completable by default
        /// </summary>
        /// <param name="completableId">Completable identifier.</param>
        /// <param name="type">Completable type.</param>
        public StatementPromise Initialized(string completableId, CompletableType type = CompletableType.Completable)
        {
            bool addInitializedTime = true;
            if (initializedTimes.ContainsKey(completableId))
            {
                if (XasuTracker.TrackerConfig.StrictMode)
                {
                    throw new XApiException("The initialized statement for the specified id has already been sent!");
                }
                else
                {
                    XasuTracker.LogWarning("The initialized statement for the specified id has already been sent!");
                    addInitializedTime = false;
                }
            }

            if (addInitializedTime)
                initializedTimes.Add(completableId, DateTime.Now);
            return Enqueue(new Statement
            {
                verb = GetVerb(Verb.Initialized),
                target = GetTargetActivity(completableId, type)
            });
        }

        /// <summary>
        /// Player progressed a completable.
        /// Type = Completable by default
        /// </summary>
        /// <param name="completableId">Completable identifier.</param>
        /// <param name="value">New value for the completable's progress.</param>
        /// <param name="type">Completable type.</param>
        public StatementPromise Progressed(string completableId, float value, CompletableType type = CompletableType.Completable)
        {
            return Enqueue(new Statement
            {
                verb = GetVerb(Verb.Progressed),
                target = GetTargetActivity(completableId, type)
            }).WithResultExtension(extensionIds[Extensions.Progress], value);
        }

        /// <summary>
        /// Player completed a completable.
        /// Type = Completable by default
        /// </summary>
        /// <param name="completableId">Completable identifier.</param>
        /// <param name="type">Completable type.</param>
        public StatementPromise Completed(string completableId, CompletableType type = CompletableType.Completable)
        {
            return Completed(completableId, type, false, 0);
        }

        /// <summary>
        /// Player completed a completable.
        /// Type = Completable by default
        /// </summary>
        /// <param name="completableId">Completable identifier.</param>
        /// <param name="durationInSeconds">Time to complete.</param>
        public StatementPromise Completed(string completableId, float durationInSeconds)
        {
            return Completed(completableId, CompletableType.Completable, true, durationInSeconds);
        }

        /// <summary>
        /// Player completed a completable.
        /// </summary>
        /// <param name="completableId">Completable identifier.</param>
        /// <param name="type">Completable type.</param>
        /// <param name="durationInSeconds">Time to complete.</param>
        public StatementPromise Completed(string completableId, CompletableType type, float durationInSeconds)
        {
            return Completed(completableId, type, true, durationInSeconds);
        }

        private StatementPromise Completed(string completableId, CompletableType type, bool hasDuration, float durationInSeconds)
        {
            if (!hasDuration && !initializedTimes.ContainsKey(completableId))
            {
                if (XasuTracker.TrackerConfig.StrictMode)
                {
                    throw new XApiException("The completed statement for the specified id has not been initialized!");
                }
                else
                {
                    hasDuration = true;
                    durationInSeconds = 0f;
                    XasuTracker.LogWarning("The completed statement for the specified id has not been initialized and therefore the duration is going to be 0.");
                }
            }

            // Get the initialized statement time to calculate the duration
            TimeSpan duration = hasDuration ? TimeSpan.FromSeconds(durationInSeconds) : DateTime.Now - initializedTimes[completableId];
            if (initializedTimes.ContainsKey(completableId))
            {
                initializedTimes.Remove(completableId);
            }

            return Enqueue(new Statement
            {
                verb = GetVerb(Verb.Completed),
                target = GetTargetActivity(completableId, type)
            }).WithCompletion(true)
            .WithTimeSpanDuration(duration);
        }
    }
}
