using _Project.Scripts.Configs;
using _Project.Scripts.Generators;
using _Project.Scripts.NetworkLayer;
using _Project.Scripts.NetworkLayer.Client;
using _Project.Scripts.NetworkLayer.Server;
using _Project.Scripts.NetworkLayer.Transport;
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
            RegisterConfigs(builder);
            RegisterGenerators(builder);
            RegisterNetworkLayer(builder);
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(matchConfig);
            builder.RegisterInstance(shipLengthConfig);
        }
        
        private void RegisterGenerators(IContainerBuilder builder)
        {
            builder.Register<FieldGenerator>(Lifetime.Singleton);
            builder.Register<ShipPositionGenerator>(Lifetime.Singleton);
        }
        
        private void RegisterNetworkLayer(IContainerBuilder builder)
        {
            builder.Register<InProcessHub>(Lifetime.Singleton).As<IServerTransport>().AsSelf();
            
            builder.Register<IClientTransport>(
                resolver => resolver.Resolve<InProcessHub>().Client(0), Lifetime.Singleton).Keyed(0);
            builder.Register<IClientTransport>(
                resolver => resolver.Resolve<InProcessHub>().Client(1), Lifetime.Singleton).Keyed(1);
            
            builder.Register<Match>(Lifetime.Singleton);
            builder.Register<GameServer>(Lifetime.Singleton);
            
            builder.Register<ClientSession>(resolver => 
                new ClientSession(resolver.Resolve<IClientTransport>(0)), Lifetime.Singleton).Keyed(0);
            builder.Register<ClientSession>(resolver => 
                new ClientSession(resolver.Resolve<IClientTransport>(1)), Lifetime.Singleton).Keyed(1);
            
            builder.RegisterEntryPoint<NetworkEntryPoint>();
        }
    }
}
