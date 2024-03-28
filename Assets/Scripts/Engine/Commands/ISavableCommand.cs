using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Engine.Commands
{
    // Те команды которые должны сохранятся для записи игры.
    public interface ISavableCommand
    {
        public string GetTypeName { get; }

        JToken Serialize()
        {
            var jObj = JObject.FromObject(this);
            jObj[Match3GameCommand.TYPE_TOKEN_NAME] = GetTypeName;
            return jObj;
        }

        void LoadData(JToken obj);
    }
}