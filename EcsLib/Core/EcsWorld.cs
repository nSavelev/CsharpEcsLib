using System;
using System.Collections.Generic;
using System.Linq;

namespace EcsLib.Core {
    public class EcsWorld {
        private List<ISystem> _systems = new List<ISystem>();
        private Dictionary<Type, ISystem> _systemsMap = new Dictionary<Type, ISystem>();
        public IEnumerable<Type> CompoentTypes => _systems.Select(e => e.ComponentType);

        public EcsWorld AddSysytem<T>(BaseSystem<T> system) where T:struct{
            _systems.Add(system);
            return this;
        }
        
        public void Update() {
            for (int i = 0; i < _systems.Count; i++) {
                _systems[i].Update();
            }
        }

        // TODO: remove boxing
        public int AddComponent<T>(Entity entity, T p) where T : struct {
            if (_systemsMap.TryGetValue(typeof(T), out var system)) {
                var id = system.ReserveComponent(p);
                entity.RegisterComponent<T>(id);
                return id;
            }
            else {
                throw new Exception($"Unregistered component type {typeof(T).Name}");
            }
        }

        public void RemoveComponent<T>(Entity entity, int id) where T: struct {
            if (_systemsMap.TryGetValue(typeof(T), out var system)) {
                entity.UnregisterComponent<T>(id);
                system.ReleaseComponent(id);
            }
            else {
                throw new Exception($"Unregistered component type {typeof(T).Name}");
            }
        }
    }
}