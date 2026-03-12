using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TinCan;
using Xasu.Util;

namespace Xasu.HighLevel
{
    public abstract class AbstractHighLevelTracker<T> : Singleton<T> where T : class, new()
    {
        protected abstract Dictionary<Enum, string> VerbIds { get; }
        protected abstract Dictionary<Enum, string> TypeIds { get; }
        protected abstract Dictionary<Enum, string> ExtensionIds { get; }

        public static Dictionary<string, string> ContextActivityIds = new Dictionary<string, string>()
        {
            { "SeriousGames", "https://w3id.org/xapi/seriousgames" },
            { "Scorm", "https://w3id.org/xapi/scorm/v/2" },
        };

        protected Verb GetVerb(Enum verb)
        {
            string verbDisplay = verb.ToString().ToLower();
            return new Verb
            {
                id = new Uri(VerbIds[verb]),
                display = new LanguageMap(new Dictionary<string, string>
                {
                    { "en", verbDisplay }
                })
            };
        }

        protected StatementTarget GetTargetActivity(string id, Enum type, string name = null, string description = null)
        {
            if (!Uri.IsWellFormedUriString(id, UriKind.Absolute))
            {
                id = XasuTracker.DefaultIdPrefix + id;
            }

            return new Activity
            {
                id = id,
                definition = new ActivityDefinition
                {
                    name = !string.IsNullOrEmpty(name) ? new LanguageMap(new Dictionary<string, string>
                    {
                        { "en-US", name}
                    }) : null,
                    description = !string.IsNullOrEmpty(description) ? new LanguageMap(new Dictionary<string, string>
                    {
                        { "en-US", description}
                    }) : null,
                    type = new Uri(TypeIds[type])
                }
            };
        }

        protected TinCan.Extensions GetExtensions(Dictionary<Enum, object> extensions)
        {
            JObject jobject = new JObject();
            foreach (var ex in extensions)
            {
                jobject.Add(ExtensionIds[ex.Key], JToken.FromObject(ex.Value));
            }

            return new TinCan.Extensions(jobject);
        }

        protected Result SetResultExtensions(Result result, Dictionary<Enum, object> extensions)
        {
            result.extensions = GetExtensions(extensions);
            return result;
        }

        protected Context SetContextExtensions(Context context, Dictionary<Enum, object> extensions)
        {
            context.extensions = GetExtensions(extensions);
            return context;
        }

        protected virtual StatementPromise Enqueue(Statement statement)
        {
            return new StatementPromise(statement, XasuTracker.Enqueue(statement));
        }
    }
}
