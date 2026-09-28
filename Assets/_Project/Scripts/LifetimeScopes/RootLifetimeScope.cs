using _Project.Scripts.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.LifetimeScopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private MatchConfig matchConfig;
        [SerializeField] private ShipLengthConfig shipLengthConfig;

        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(matchConfig);
            builder.RegisterInstance(shipLengthConfig);
        }
    }
}
