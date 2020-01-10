using System;
using System.Collections.Generic;

namespace EcsLib.Core {
    public abstract class ISystem {
        internal abstract Type ComponentType { get; }
        internal abstract void Update();
        internal abstract int ReserveComponent(object component);
        internal abstract void ReleaseComponent(int id);
    }

    public abstract class BaseSystem<T> : ISystem where T:struct {
        public IEnumerable<T> Components => _components;
        internal override Type ComponentType => typeof(T);
        protected readonly T[] _components;
        protected readonly HashSet<int> _ocupied = new HashSet<int>();
        protected readonly bool _isOneTick;

        public BaseSystem(int maxSize) {
            _components = new T[maxSize];
        }

        public void UpdateComponent(int id, T component) {
            _components[id] = component;
        }

        public bool TryGetComponent(int id, ref T component) {
            if (!_ocupied.Contains(id)) {
                return false;
            }
            component = _components[id];
            return true;
        }

        internal override int ReserveComponent(object component) {
            if (component is T) {
                return ReserveComponent((T) component);
            }
            else {
                throw new Exception($"Component Should be {typeof(T).Name}, but recieved {component.GetType().Name}");
            }
        }

        public int ReserveComponent(T component) {
            var id = -1;
            for (int i = 0; i < _components.Length; i++) {
                if (!_ocupied.Contains(i)) {
                    id = i;
                    break;
                }
            }
            if (id == -1) {
                throw new Exception("All components are occupied, try create system with more available components");
            }
            _ocupied.Add(id);
            _components[id] = component;
            return id;
        }

        internal override void ReleaseComponent(int id) {
            if (!_ocupied.Contains(id)) {
                throw new ArgumentException($"Component with {id} already released!");
            }
            _ocupied.Remove(id);
            _components[id] = default;
        }

        internal override void Update() {
            // TODO: escape allocations
            foreach (var id in _ocupied) {
                Iterate(id, _components[id]);
            }
            if (_isOneTick) {
                _ocupied.Clear();
            }
        }

        public abstract void Iterate(int id, T c);
    }
}