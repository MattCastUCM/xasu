using System;
using System.Collections.Generic;
using Xasu.Exceptions;
using TinCan;
using Xasu.Util;


namespace Xasu.HighLevel
{
    public class InputTracker : AbstractSeriousGameHighLevelTracker<InputTracker>
    {
        /**********************
        *       Verbs
        * *******************/
        public enum Verb
        {
            Pressed,
            Released
        }
        public Dictionary<Enum, string> verbIds = new Dictionary<Enum, string>()
        {
            { Verb.Pressed,   "https://w3id.org/xapi/seriousgames/verbs/pressed"  },
            { Verb.Released,  "https://w3id.org/xapi/seriousgames/verbs/released" }
        };

        protected override Dictionary<Enum, string> VerbIds => verbIds;


        /**********************
        *   Input Types
        * *******************/
        public enum InputType
        {
            Screen,
            Touchscreen,
            Keyboard,
            Mouse,
            Button
        }
        private readonly Dictionary<Enum, string> typeIds = new Dictionary<Enum, string>
        {
            { InputType.Screen,      "https://w3id.org/xapi/seriousgames/activity-types/screen"      },
            { InputType.Touchscreen, "https://w3id.org/xapi/seriousgames/activity-types/touchscreen" },
            { InputType.Keyboard,    "https://w3id.org/xapi/seriousgames/activity-types/keyboard"    },
            { InputType.Mouse,       "https://w3id.org/xapi/seriousgames/activity-types/mouse"       },
            // Button does not appear in the official xAPI specification but is added as a common generic input type for game analytics.
            { InputType.Button,      "https://w3id.org/xapi/seriousgames/activity-types/button"      }
        };
        protected override Dictionary<Enum, string> TypeIds => typeIds;


        /**********************
        *   Extensions
        * *******************/
        public enum Extensions
        {
            // Empty, as no specific extensions were requested for inputs, but required by the abstract class.
        }
        private readonly Dictionary<Enum, string> extensionIds = new Dictionary<Enum, string>();
        protected override Dictionary<Enum, string> ExtensionIds => extensionIds;


        /**********************
        * Static attributes
        * *******************/
        private static Dictionary<string, DateTime> pressedTimes = new Dictionary<string, DateTime>();


        /**********************
        *   Templates
        * *******************/

        /// <summary>
        /// Player pressed an input.
        /// Type = Button by default
        /// </summary>
        /// <param name="inputId">Input identifier.</param>
        /// <param name="type">Input type.</param>
        public StatementPromise Pressed(string inputId, InputType type = InputType.Button)
        {
            bool addPressedTime = true;
            if (pressedTimes.ContainsKey(inputId))
            {
                if (XasuTracker.TrackerConfig.StrictMode)
                {
                    throw new XApiException($"The pressed statement for the specified id '{inputId}' has already been sent!");
                }
                else
                {
                    if (XasuTracker.EnableDebugLogging)
                        DebugLogger.Log($"[XASU][Warning] The pressed statement for the specified id '{inputId}' has already been sent!");
                    addPressedTime = false;
                }
            }

            if (addPressedTime)
            {
                pressedTimes.Add(inputId, DateTime.Now);
            }

            return Enqueue(new Statement
            {
                verb = GetVerb(Verb.Pressed),
                target = GetTargetActivity(inputId, type)
            });
        }

        /// <summary>
        /// Player released an input.
        /// Type = Button by default
        /// </summary>
        /// <param name="inputId">Input identifier.</param>
        /// <param name="type">Input type.</param>
        public StatementPromise Released(string inputId, InputType type = InputType.Button)
        {
            return Released(inputId, type, false, 0f);
        }

        /// <summary>
        /// Player released an input after a specific duration.
        /// Type = Button by default
        /// </summary>
        /// <param name="inputId">Input identifier.</param>
        /// <param name="durationInSeconds">Duration the input was held.</param>
        public StatementPromise Released(string inputId, float durationInSeconds)
        {
            return Released(inputId, InputType.Button, true, durationInSeconds);
        }

        /// <summary>
        /// Player released an input after a specific duration.
        /// </summary>
        /// <param name="inputId">Input identifier.</param>
        /// <param name="type">Input type.</param>
        /// <param name="durationInSeconds">Duration the input was held.</param>
        public StatementPromise Released(string inputId, InputType type, float durationInSeconds)
        {
            return Released(inputId, type, true, durationInSeconds);
        }

        /// <summary>
        /// Private helper to process the release and calculate the duration held.
        /// </summary>
        private StatementPromise Released(string inputId, InputType type, bool hasDuration, float durationInSeconds)
        {
            if (!hasDuration && !pressedTimes.ContainsKey(inputId))
            {
                if (XasuTracker.TrackerConfig.StrictMode)
                {
                    throw new XApiException($"The released statement for the specified id '{inputId}' has not been pressed!");
                }
                else
                {
                    hasDuration = true;
                    durationInSeconds = 0f;

                    if (XasuTracker.EnableDebugLogging)
                        DebugLogger.Log($"[XASU][Warning] The released statement for the specified id '{inputId}' has not been pressed and therefore the duration is going to be 0.");
                }
            }

            // Get the pressed statement time to calculate how long the input was held down
            TimeSpan duration = hasDuration ? TimeSpan.FromSeconds(durationInSeconds) : DateTime.Now - pressedTimes[inputId];
            if (pressedTimes.ContainsKey(inputId))
            {
                pressedTimes.Remove(inputId);
            }

            return Enqueue(new Statement
            {
                verb = GetVerb(Verb.Released),
                target = GetTargetActivity(inputId, type)
            })
            // Attach how long the input was held for analytics
            .WithTimeSpanDuration(duration);
        }
    }
}
