using System;
using System.Collections.Generic;
using TinCan;

namespace Xasu.HighLevel
{
    public class GameObjectTracker : AbstractSeriousGameHighLevelTracker<GameObjectTracker>
    {
        /**********************
        *       Verbs
        * *******************/
        public enum Verb
        {
            Interacted,
            Used
        }
        protected readonly Dictionary<Enum, string> verbIds = new Dictionary<Enum, string>
        {
            { Verb.Interacted,  "http://adlnet.gov/expapi/verbs/interacted"     },
            { Verb.Used,        "https://w3id.org/xapi/seriousgames/verbs/used" }
        };
        protected override Dictionary<Enum, string> VerbIds => verbIds;


        /**********************
        *   GameObject Types 
        * *******************/
        public enum TrackedGameObject
        {
            Enemy,
            Npc,
            Item,
            GameObject
        }
        protected readonly Dictionary<Enum, string> typeIds = new Dictionary<Enum, string>
        {
            { TrackedGameObject.Enemy,      "https://w3id.org/xapi/seriousgames/activity-types/enemy" },
            { TrackedGameObject.Npc,        "https://w3id.org/xapi/seriousgames/activity-types/non-player-character"},
            { TrackedGameObject.Item,       "https://w3id.org/xapi/seriousgames/activity-types/item"},
            { TrackedGameObject.GameObject, "https://w3id.org/xapi/seriousgames/activity-types/game-object"}
        };
        protected override Dictionary<Enum, string> TypeIds => typeIds;


        /**********************
        *   Extensions
        * *******************/
        protected override Dictionary<Enum, string> ExtensionIds => null;


        /**********************
        *     Templates
        * *******************/

        /// <summary>
        /// Player interacted with a game object.
        /// Type = GameObject by default
        /// </summary>
        /// <param name="gameobjectId">Identifier.</param>
        /// <param name="type">TrackedGameObject type.</param>
        public StatementPromise Interacted(string gameobjectId, TrackedGameObject type = TrackedGameObject.GameObject)
        {
            return Enqueue(new Statement
            {
                verb = GetVerb(Verb.Interacted),
                target = GetTargetActivity(gameobjectId, type)
            });
        }

        /// <summary>
        /// Player interacted with a game object.
        /// Type = GameObject by default
        /// </summary>
        /// <param name="gameobjectId">TrackedGameObject identifier.</param>
        /// <param name="type">TrackedGameObject type.</param>
        public StatementPromise Used(string gameobjectId, TrackedGameObject type = TrackedGameObject.GameObject)
        {
            return Enqueue(new Statement
            {
                verb = GetVerb(Verb.Used),
                target = GetTargetActivity(gameobjectId, type)
            });
        }
    }
}
