/*
 * Copyright (C) 2017 Spirit AI Limited. All Rights Reserved.
 * Unauthorized copying of this file, via any medium is strictly prohibited.
 * Proprietary and confidential.
 */

using UnityEngine;

namespace SpiritAI.CharacterEngine.Integrations.Unity
{
    public class TickManager : MonoBehaviour, ITickSource
    {
        private readonly TickSourceBase _base = new TickSourceBase();

        private void Update()
        {
            _base.Tick(UnityEngine.Time.deltaTime);
        }

        public void Register(ITickObject tickObject)
        {
            _base.Register(tickObject);
        }

        public void Unregister(ITickObject tickObject)
        {
            _base.Unregister(tickObject);
        }

        public void Clear()
        {
            _base.Clear();
        }
    }
}
