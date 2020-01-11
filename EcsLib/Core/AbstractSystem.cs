﻿using System;
 using System.Collections.Generic;

 namespace EcsLib.Core
{
    public abstract class AbstractSystem
    {
        protected EcsWorld _world;

        internal void SetWorld(EcsWorld world)
        {
            _world = world;
        }

        internal abstract Type ComponentType { get; }
        internal abstract void Update();
        internal abstract int ReserveComponent(object component);
        internal abstract void ReleaseComponent(int id);
    }
}