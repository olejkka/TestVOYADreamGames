using _Project.Scripts.Configs;

namespace _Project.Scripts.Generators
{
    public class FieldGenerator
    {
        private readonly MatchConfig _matchConfig;
        

        public FieldGenerator(MatchConfig matchConfig)
        {
            _matchConfig = matchConfig;
        }

        public Field Create()
        {
            return new Field(_matchConfig.width, _matchConfig.height);
        }
    }
}
