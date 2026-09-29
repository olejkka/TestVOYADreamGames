using _Project.Scripts.Configs;
using _Project.Scripts.Generators;
using _Project.Scripts.NetworkLayer;
using _Project.Scripts.NetworkLayer.Client;
using _Project.Scripts.NetworkLayer.Server;
using _Project.Scripts.NetworkLayer.Transport;
using _Project.Scripts.UI;
using _Project.Scripts.UI.Cell;
using _Project.Scripts.UI.PlayersWindows;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.LifetimeScopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private MatchConfig matchConfig;
        [SerializeField] private ShipLengthConfig shipLengthConfig;
        [SerializeField] private CellColorConfig cellColorConfig;
        [SerializeField] private PlayerWindow playerWindow0;
        [SerializeField] private PlayerWindow playerWindow1;
        [SerializeField] private CellInstantiator cellInstantiator;

        
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterConfigs(builder);
            RegisterWindows(builder);
            RegisterGenerators(builder);
            RegisterNetworkLayer(builder);
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(matchConfig);
            builder.RegisterInstance(shipLengthConfig);
            builder.RegisterInstance(cellColorConfig);
        }
        
        private void RegisterWindows(IContainerBuilder builder)
        {
            builder.RegisterComponent(cellInstantiator);
            builder.RegisterInstance(playerWindow0).Keyed(0);
            builder.RegisterInstance(playerWindow1).Keyed(1);
            builder.RegisterComponentInHierarchy<WindowsBinder>();
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
                new ClientSession(resolver.Resolve<IClientTransport>(0), 0), Lifetime.Singleton).Keyed(0);
            builder.Register<ClientSession>(resolver =>
                new ClientSession(resolver.Resolve<IClientTransport>(1), 1), Lifetime.Singleton).Keyed(1);
            
            builder.RegisterEntryPoint<NetworkEntryPoint>();
        }
    }
}
